using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services.Monitoring;
using CourseGuard.Backend.Services.Storage;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class MonitorPopupForm : Form
    {
        private readonly int _examId;
        private readonly int _studentId;
        private readonly int _attemptId;
        private readonly CancellationTokenSource _cts = new();
        private readonly PictureBox _picture = new();
        private readonly Label _status = new();
        
        // Cột bên phải cho Vi phạm
        private readonly DataGridView _dgvViolations = new();
        private readonly Button _btnRecordViolation = new();
        
        private readonly ViolationRepository _violationRepo = new();
        private readonly SupabaseStorageService _storageService = new();
        private ClientWebSocket? _ws;

        public MonitorPopupForm(int teacherId, int examId, int studentId, int attemptId)
        {
            _examId = examId;
            _studentId = studentId;
            _attemptId = attemptId;
            InitializeComponent();
            Text = $"Giám sát thi - HS {studentId}";
            Width = 1200;
            Height = 760;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = MetaTheme.Colors.FormBg;

            SetupLayout();

            FormClosing += (_, _) =>
            {
                _cts.Cancel();
                _ws?.Dispose();
                _picture.Image?.Dispose();
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartWebSocketReceiver().FireAndForgetSafe(this);
            LoadViolationsAsync().FireAndForgetSafe(this);
        }

        private async Task StartWebSocketReceiver()
        {
            CourseGuard.Backend.Config.AppEnvironment.LoadDotEnvIfExists();
            string relayUrl = CourseGuard.Backend.Config.AppEnvironment.GetOptional("RELAY_WS_URL") ?? "ws://localhost:8080/relay";

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    _ws?.Dispose();
                    _ws = new ClientWebSocket();

                    string connectionUrl = $"{relayUrl}?role=teacher&examId={_examId}";
                    SetStatus("Đang kết nối tới Cloud Relay...");
                    await _ws.ConnectAsync(new Uri(connectionUrl), _cts.Token);
                    SetStatus("Đã kết nối tới Cloud Relay. Đang chờ học sinh...");

                    var buffer = new byte[1024 * 1024 * 5]; // 5MB buffer
                    while (_ws.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                    {
                        var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                            break;

                        if (result.MessageType == WebSocketMessageType.Binary && result.Count > 0)
                        {
                            ProcessReceivedPacket(buffer, result.Count);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_cts.Token.IsCancellationRequested) break;
                    SetStatus($"Lỗi kết nối: {ex.Message}. Đang kết nối lại...");
                    await Task.Delay(2000, _cts.Token);
                }
            }
        }

        private void ProcessReceivedPacket(byte[] packet, int count)
        {
            if (count < ScreenStreamProtocol.HeaderLength) return;

            var header = new byte[ScreenStreamProtocol.HeaderLength];
            Buffer.BlockCopy(packet, 0, header, 0, ScreenStreamProtocol.HeaderLength);

            if (!ScreenStreamProtocol.TryReadHeader(header, out byte frameType, out int frameExamId, out int frameStudentId, out int frameAttemptId, out _, out int jpegLength))
                return;

            if (count - ScreenStreamProtocol.HeaderLength < jpegLength) return;

            var jpegBytes = new byte[jpegLength];
            Buffer.BlockCopy(packet, ScreenStreamProtocol.HeaderLength, jpegBytes, 0, jpegLength);

            var args = new ScreenFrameReceivedEventArgs(frameType, frameExamId, frameStudentId, frameAttemptId, jpegBytes);
            
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Service_FrameReceived(this, args)));
            }
            else
            {
                Service_FrameReceived(this, args);
            }
        }

        private void SetupLayout()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 800,
                FixedPanel = FixedPanel.Panel2,
                BackColor = MetaTheme.Colors.Border
            };

            // PANEL 1: Hình ảnh giám sát
            var pnlLeft = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black };
            _status.Dock = DockStyle.Top;
            _status.Height = 38;
            _status.Text = "Đang kết nối...";
            _status.ForeColor = Color.White;
            _status.TextAlign = ContentAlignment.MiddleLeft;
            _status.Padding = new Padding(12, 0, 0, 0);
            
            _picture.Dock = DockStyle.Fill;
            _picture.BackColor = Color.Black;
            _picture.SizeMode = PictureBoxSizeMode.Zoom;
            
            pnlLeft.Controls.Add(_picture);
            pnlLeft.Controls.Add(_status);

            // PANEL 2: Danh sách vi phạm
            var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = MetaTheme.Colors.FormBg, Padding = new Padding(16) };
            
            var lblTitle = new Label
            {
                Text = "Danh sách vi phạm",
                Font = MetaTheme.Fonts.HeadingMd(),
                ForeColor = MetaTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _dgvViolations.Dock = DockStyle.Fill;
            MetaTheme.StyleGrid(_dgvViolations);
            _dgvViolations.Columns.Add("CreatedAt", "Thời gian");
            _dgvViolations.Columns.Add("Type", "Loại vi phạm");

            _dgvViolations.Columns.Add("Severity", "Muc do");

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(0, 16, 0, 0) };
            _btnRecordViolation.Text = "Ghi nhận vi phạm";
            _btnRecordViolation.Dock = DockStyle.Fill;
            MetaTheme.StyleLogoutButton(_btnRecordViolation);
            _btnRecordViolation.Click += BtnRecordViolation_Click;
            pnlBottom.Controls.Add(_btnRecordViolation);

            pnlRight.Controls.Add(_dgvViolations);
            pnlRight.Controls.Add(lblTitle);
            pnlRight.Controls.Add(pnlBottom);

            split.Panel1.Controls.Add(pnlLeft);
            split.Panel2.Controls.Add(pnlRight);
            Controls.Add(split);
        }

        private async void BtnRecordViolation_Click(object? sender, EventArgs e)
        {
            if (_picture.Image == null)
            {
                MetaTheme.ShowModernDialog("Chưa có hình ảnh từ máy học sinh để ghi nhận.", "Thông báo");
                return;
            }

            _btnRecordViolation.Enabled = false;
            _btnRecordViolation.Text = "Đang xử lý...";

            try
            {
                // Chụp lại khung hình hiện tại
                using var ms = new MemoryStream();
                using var bmp = new Bitmap(_picture.Image);
                bmp.Save(ms, ImageFormat.Jpeg);
                byte[] imageBytes = ms.ToArray();

                // 1. Upload ảnh
                string fileName = $"violation_exam{_examId}_student{_studentId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.jpg";
                string imageUrl = await _storageService.UploadViolationImageAsync(imageBytes, fileName);

                // 2. Lưu database
                var violation = new ViolationModel
                {
                    UserId = _studentId,
                    ExamAttemptId = _attemptId > 0 ? _attemptId : null,
                    Type = "Mất Focus / Chuyển Tab (Ghi nhận thủ công)",
                    Severity = ViolationSeverityMap.Get("SCREEN_SWITCH"),
                    ActionTaken = "RECORDED_MANUALLY",
                    ImageUrl = imageUrl
                };
                await _violationRepo.InsertViolationAsync(violation);

                // 3. Reload list
                await LoadViolationsAsync();
                MetaTheme.ShowModernDialog("Đã ghi nhận vi phạm thành công!", "Thành công");
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog($"Lỗi khi ghi nhận vi phạm: {ex.Message}", "Lỗi");
            }
            finally
            {
                _btnRecordViolation.Enabled = true;
                _btnRecordViolation.Text = "Ghi nhận vi phạm";
            }
        }

        private async Task LoadViolationsAsync()
        {
            if (_attemptId <= 0) return;

            try
            {
                var violations = await _violationRepo.GetViolationsByAttemptIdAsync(_attemptId);
                if (InvokeRequired)
                {
                    BeginInvoke(new MethodInvoker(() => BindViolations(violations)));
                }
                else
                {
                    BindViolations(violations);
                }
            }
            catch (Exception ex)
            {
                // Bỏ qua lỗi load ban đầu để không làm phiền giám thị
                Console.WriteLine(ex.Message);
            }
        }

        private void BindViolations(System.Collections.Generic.List<ViolationModel> violations)
        {
            _dgvViolations.Rows.Clear();
            foreach (var v in violations)
            {
                _dgvViolations.Rows.Add(v.CreatedAt.ToString("HH:mm:ss"), v.Type, v.Severity);
            }
        }

        private void Service_FrameReceived(object? sender, ScreenFrameReceivedEventArgs e)
        {
            if (e.StudentId != _studentId) return;

            try
            {
                using var ms = new MemoryStream(e.JpegBytes);
                using var source = Image.FromStream(ms);
                var frame = new Bitmap(source);
                if (IsDisposed) return;
                
                if (InvokeRequired)
                {
                    BeginInvoke(new MethodInvoker(() => { if (!IsDisposed) SetFrame(frame, e.FrameType); }));
                }
                else
                {
                    SetFrame(frame, e.FrameType);
                }
            }
            catch
            {
                // Bỏ qua lỗi parse hình ảnh bị hỏng nếu có
            }
        }

        private void SetFrame(Image frame, byte frameType)
        {
            Image? old = _picture.Image;
            _picture.Image = frame;
            old?.Dispose();
            
            _picture.Refresh(); // Ép vẽ lại ngay lập tức để tránh dính ảnh cũ
            
            if (frameType == 1) // Cảnh báo gian lận
            {
                _status.Text = "🚨 PHÁT HIỆN GIAN LẬN: SINH VIÊN CỐ TÌNH CHUYỂN TAB 🚨";
                _status.ForeColor = Color.Red;
                _status.BackColor = Color.Yellow;
            }
            else
            {
                _status.Text = "Đang nhận hình liên tục (Real-time)";
                _status.ForeColor = MetaTheme.Colors.Success;
                _status.BackColor = Color.Transparent;
            }
        }

        private void SetStatus(string status)
        {
            if (InvokeRequired)
                BeginInvoke(new MethodInvoker(() => _status.Text = status));
            else
            {
                _status.Text = status;
                _status.ForeColor = Color.White;
            }
        }
    }
}
