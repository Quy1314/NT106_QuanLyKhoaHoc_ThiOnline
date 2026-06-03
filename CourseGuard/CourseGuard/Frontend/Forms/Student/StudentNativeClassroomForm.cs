using AForge.Video;
using AForge.Video.DirectShow;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Services.Classroom;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public sealed class StudentNativeClassroomForm : Form
    {
        private readonly int _sessionId;
        private readonly string _sessionName;
        private readonly TcpClassroomClient _client = new();
        private Label _statusLabel = null!;
        private ListBox _eventsList = null!;
        private PictureBox _teacherVideo = null!;
        private PictureBox _studentPreview = null!;
        private Label _videoPlaceholder = null!;
        private Button _btnCamera = null!;
        private Button _btnMic = null!;
        private Button _btnShareScreen = null!;
        private Panel _teacherCameraPip = null!;
        private PictureBox _teacherCameraPipPreview = null!;
        private ListBox _chatList = null!;
        private TextBox _chatInput = null!;
        private Button _btnSendChat = null!;

        private FilterInfoCollection? _videoDevices;
        private VideoCaptureDevice? _camera;
        private bool _isCameraOn;
        private bool _isMicOn = true;
        private DateTime _lastFrameSentAt = DateTime.MinValue;
        private DateTime _lastScreenFrameSentAt = DateTime.MinValue;
        private int _isSendingFrame;
        private int _isSendingScreenFrame;
        private bool _isScreenSharing;
        private Rectangle _screenShareBounds;
        private string _screenShareTitle = "Màn hình";
        private System.Windows.Forms.Timer? _screenShareTimer;

        public StudentNativeClassroomForm(int sessionId, string sessionName)
        {
            _sessionId = sessionId;
            _sessionName = sessionName;
            Text = "CourseGuard Classroom - Học sinh";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(940, 600);
            Size = new Size(1080, 680);
            BackColor = AppColors.BgBase;
            FormClosing += StudentNativeClassroomForm_FormClosing;

            BuildLayout();
            WireClientEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                await _client.ConnectAsync("127.0.0.1", ClassroomProtocol.DefaultPort);
                await _client.JoinRoomAsync(
                    _sessionId,
                    UserSessionContext.CurrentUserId ?? 0,
                    UserSessionContext.CurrentUsername ?? "Student",
                    "STUDENT");
                _statusLabel.Text = $"Đã vào lớp native: {_sessionName}. Đang chờ video từ giáo viên...";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Không kết nối được classroom socket.";
                MetaTheme.ShowModernDialog("Không kết nối được lớp học native: " + ex.Message, "Thông báo");
            }
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(18),
                BackColor = AppColors.BgBase
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            Controls.Add(root);

            var stage = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 14, 14)
            };
            root.Controls.Add(stage, 0, 0);

            _teacherVideo = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 10, 18),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            stage.Controls.Add(_teacherVideo);

            _videoPlaceholder = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Đang chờ video từ giáo viên...\nKhi giáo viên bật camera, hình ảnh sẽ hiện tại đây.",
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                BackColor = Color.Transparent
            };
            stage.Controls.Add(_videoPlaceholder);
            _videoPlaceholder.BringToFront();

            _teacherCameraPip = new Panel
            {
                Width = 220,
                Height = 128,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Left = Math.Max(18, stage.Width - 242),
                Top = Math.Max(18, stage.Height - 150),
                BackColor = Color.FromArgb(230, 8, 10, 18),
                Padding = new Padding(4),
                Visible = false
            };
            stage.Controls.Add(_teacherCameraPip);
            _teacherCameraPip.BringToFront();
            stage.Resize += (_, _) =>
            {
                _teacherCameraPip.Left = Math.Max(18, stage.Width - _teacherCameraPip.Width - 22);
                _teacherCameraPip.Top = Math.Max(18, stage.Height - _teacherCameraPip.Height - 22);
            };

            _teacherCameraPipPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            _teacherCameraPip.Controls.Add(_teacherCameraPipPreview);

            var side = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgCard,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 0, 14)
            };
            root.Controls.Add(side, 1, 0);

            var sideLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 6,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
            side.Controls.Add(sideLayout);

            sideLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Camera của bạn",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            }, 0, 0);

            _studentPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 10, 18),
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0, 0, 0, 8)
            };
            sideLayout.Controls.Add(_studentPreview, 0, 1);

            sideLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Hoạt động lớp học",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            }, 0, 2);

            _eventsList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgInput,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9)
            };
            sideLayout.Controls.Add(_eventsList, 0, 3);

            sideLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Chat lớp học",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            }, 0, 4);

            var chatPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 2,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(10)
            };
            chatPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            chatPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            chatPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            chatPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74));
            sideLayout.Controls.Add(chatPanel, 0, 5);

            _chatList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(226, 232, 240),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            chatPanel.SetColumnSpan(_chatList, 2);
            chatPanel.Controls.Add(_chatList, 0, 0);

            _chatInput = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                PlaceholderText = "Nhập tin nhắn..."
            };
            _chatInput.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    await SendChatMessageAsync();
                }
            };
            chatPanel.Controls.Add(_chatInput, 0, 1);

            _btnSendChat = new Button
            {
                Dock = DockStyle.Fill,
                Text = "Gửi",
                BackColor = AppColors.AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnSendChat.FlatAppearance.BorderSize = 0;
            _btnSendChat.Click += async (_, _) => await SendChatMessageAsync();
            chatPanel.Controls.Add(_btnSendChat, 1, 1);

            var controlBar = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgCard,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 14, 14)
            };
            root.Controls.Add(controlBar, 0, 1);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            controlBar.Controls.Add(buttons);

            _btnCamera = CreateActionButton("Bật Camera", AppColors.AccentBlue);
            _btnMic = CreateActionButton("Tắt Mic", AppColors.Warning);
            _btnShareScreen = CreateActionButton("Share màn hình", Color.FromArgb(139, 92, 246));
            _btnCamera.Click += async (_, _) => await ToggleCameraAsync();
            _btnMic.Click += async (_, _) => await ToggleMicAsync();
            _btnShareScreen.Click += async (_, _) => await ToggleScreenShareAsync();
            buttons.Controls.Add(_btnCamera);
            buttons.Controls.Add(_btnMic);
            buttons.Controls.Add(_btnShareScreen);

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Đang kết nối classroom socket...",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Padding = new Padding(14),
                BackColor = AppColors.BgCard
            };
            root.SetColumnSpan(_statusLabel, 2);
            root.Controls.Add(_statusLabel, 0, 2);
        }

        private static Button CreateActionButton(string text, Color color)
        {
            var button = new Button
            {
                Text = text,
                Width = 150,
                Height = 46,
                Margin = new Padding(0, 0, 12, 0),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            RoundedButtonHelper.Apply(button, 12);
            return button;
        }

        private void WireClientEvents()
        {
            _client.StatusChanged += (_, e) => SafeAddEvent(e.Status);
            _client.SignalReceived += (_, e) => HandleSignal(e.Signal);
        }

        private void HandleSignal(ClassroomSignalModel signal)
        {
            if (signal.Type == ClassroomMessageType.VideoFrame && signal.SenderRole == "TEACHER")
            {
                RenderTeacherVideoFrame(signal);
                return;
            }

            if (signal.Type == ClassroomMessageType.ScreenShareFrame && signal.SenderRole == "TEACHER")
            {
                RenderTeacherScreenShareFrame(signal);
                return;
            }

            if (signal.Type == ClassroomMessageType.Chat)
            {
                RenderChatMessage(signal);
                return;
            }

            SafeAddEvent($"{signal.Type} từ {signal.SenderName}");
            if (signal.Type == ClassroomMessageType.CamOff && signal.SenderRole == "TEACHER")
            {
                SafeShowTeacherPlaceholder("Camera giáo viên đã tắt.");
            }
            else if (signal.Type == ClassroomMessageType.CamOn && signal.SenderRole == "TEACHER")
            {
                SafeSetStatus("Giáo viên đã bật camera. Đang nhận video...");
            }
            else if (signal.Type == ClassroomMessageType.ScreenShareOn && signal.SenderRole == "TEACHER")
            {
                SafeSetStatus("Giáo viên đang chia sẻ màn hình...");
            }
            else if (signal.Type == ClassroomMessageType.ScreenShareOff && signal.SenderRole == "TEACHER")
            {
                SafeShowTeacherPlaceholder("Giáo viên đã dừng chia sẻ màn hình.");
            }
        }

        private async Task SendChatMessageAsync()
        {
            string message = _chatInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;
            if (!_client.IsConnected)
            {
                SafeSetStatus("Chưa kết nối classroom socket nên chưa gửi được chat.");
                return;
            }

            _chatInput.Clear();
            string senderName = UserSessionContext.CurrentUsername ?? "Student";

            await _client.SendAsync(new ClassroomSignalModel
            {
                Type = ClassroomMessageType.Chat,
                SessionId = _sessionId,
                SenderId = UserSessionContext.CurrentUserId ?? 0,
                SenderName = senderName,
                SenderRole = "STUDENT",
                Payload =
                {
                    ["message"] = message,
                    ["sentAt"] = DateTime.Now.ToString("HH:mm")
                }
            });
        }

        private void RenderChatMessage(ClassroomSignalModel signal)
        {
            if (!signal.Payload.TryGetValue("message", out string? message) || string.IsNullOrWhiteSpace(message)) return;
            AppendChatMessage(signal.SenderName, message, signal.SenderRole == "TEACHER");
        }

        private void AppendChatMessage(string senderName, string message, bool isTeacher)
        {
            if (IsDisposed || !IsHandleCreated) return;
            string label = isTeacher ? "GV" : "HS";
            BeginInvoke(() =>
            {
                _chatList.Items.Add($"[{DateTime.Now:HH:mm}] {label} {senderName}: {message}");
                _chatList.TopIndex = Math.Max(0, _chatList.Items.Count - 1);
            });
        }

        private async Task ToggleCameraAsync()
        {
            if (_isCameraOn)
            {
                StopCamera();
                _btnCamera.Text = "Bật Camera";
                _btnCamera.BackColor = AppColors.AccentBlue;
                await SendStateAsync(ClassroomMessageType.CamOff);
                return;
            }

            try
            {
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (_videoDevices.Count == 0)
                {
                    MetaTheme.ShowModernDialog("Không tìm thấy webcam trên máy học sinh.", "Camera");
                    return;
                }

                _camera = new VideoCaptureDevice(_videoDevices[0].MonikerString);
                if (_camera.VideoCapabilities.Length > 0)
                {
                    _camera.VideoResolution = _camera.VideoCapabilities
                        .OrderByDescending(c => c.FrameSize.Width * c.FrameSize.Height)
                        .First();
                }

                _camera.NewFrame += Camera_NewFrame;
                _camera.Start();
                _isCameraOn = true;
                _btnCamera.Text = "Tắt Camera";
                _btnCamera.BackColor = AppColors.Danger;
                _statusLabel.Text = "Camera học sinh đang bật - giáo viên có thể nhìn thấy bạn.";
                await SendStateAsync(ClassroomMessageType.CamOn);
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể bật camera: " + ex.Message, "Camera");
            }
        }

        private async Task ToggleMicAsync()
        {
            _isMicOn = !_isMicOn;
            _btnMic.Text = _isMicOn ? "Tắt Mic" : "Bật Mic";
            _btnMic.BackColor = _isMicOn ? AppColors.Warning : AppColors.AccentBlue;
            await SendStateAsync(_isMicOn ? ClassroomMessageType.MicOn : ClassroomMessageType.MicOff);
        }

        private async Task SendStateAsync(string type)
        {
            if (!_client.IsConnected) return;
            await _client.SendAsync(new ClassroomSignalModel
            {
                Type = type,
                SessionId = _sessionId,
                SenderId = UserSessionContext.CurrentUserId ?? 0,
                SenderName = UserSessionContext.CurrentUsername ?? "Student",
                SenderRole = "STUDENT"
            });
        }

        private async Task ToggleScreenShareAsync()
        {
            if (_isScreenSharing)
            {
                StopScreenShare();
                await SendStateAsync(ClassroomMessageType.ScreenShareOff);
                return;
            }

            using var picker = new ScreenSharePickerDialog();
            if (picker.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _screenShareBounds = picker.SelectedBounds;
            _screenShareTitle = picker.SelectedTitle;
            _isScreenSharing = true;
            _btnShareScreen.Text = "Dừng trình bày";
            _btnShareScreen.BackColor = AppColors.Danger;
            _statusLabel.Text = $"Bạn đang trình bày: {_screenShareTitle}";
            SafeAddEvent($"Đang trình bày {_screenShareTitle} cho giáo viên.");
            await SendStateAsync(ClassroomMessageType.ScreenShareOn);

            _screenShareTimer = new System.Windows.Forms.Timer { Interval = 240 };
            _screenShareTimer.Tick += (_, _) => CaptureAndSendScreenFrameAsync().FireAndForgetSafe(this);
            _screenShareTimer.Start();
        }

        private async Task CaptureAndSendScreenFrameAsync()
        {
            Bitmap? frame = null;
            try
            {
                if (!_isScreenSharing || !_client.IsConnected) return;
                if ((DateTime.UtcNow - _lastScreenFrameSentAt).TotalMilliseconds < 260) return;
                if (Interlocked.Exchange(ref _isSendingScreenFrame, 1) == 1) return;

                _lastScreenFrameSentAt = DateTime.UtcNow;
                frame = CaptureScreenBounds(_screenShareBounds);
                using Bitmap resized = ResizeFrame(frame, 1280, 720);
                string base64Frame = EncodeJpegBase64(resized, 44L);

                await _client.SendAsync(new ClassroomSignalModel
                {
                    Type = ClassroomMessageType.ScreenShareFrame,
                    SessionId = _sessionId,
                    SenderId = UserSessionContext.CurrentUserId ?? 0,
                    SenderName = UserSessionContext.CurrentUsername ?? "Student",
                    SenderRole = "STUDENT",
                    Payload =
                    {
                        ["imageBase64"] = base64Frame,
                        ["width"] = resized.Width.ToString(),
                        ["height"] = resized.Height.ToString(),
                        ["sourceTitle"] = _screenShareTitle
                    }
                });
            }
            catch
            {
                // Drop screen-share frames silently to keep classroom responsive.
            }
            finally
            {
                frame?.Dispose();
                Interlocked.Exchange(ref _isSendingScreenFrame, 0);
            }
        }

        private static Bitmap CaptureScreenBounds(Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                bounds = Screen.PrimaryScreen?.Bounds ?? Screen.AllScreens[0].Bounds;
            }

            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            return bitmap;
        }

        private void Camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            if (_studentPreview.IsDisposed || !_studentPreview.IsHandleCreated)
            {
                frame.Dispose();
                return;
            }

            Bitmap frameForNetwork = (Bitmap)frame.Clone();
            _studentPreview.BeginInvoke(() =>
            {
                Image? old = _studentPreview.Image;
                _studentPreview.Image = frame;
                old?.Dispose();
            });

            SendStudentVideoFrameAsync(frameForNetwork).FireAndForgetSafe(this);
        }

        private async Task SendStudentVideoFrameAsync(Bitmap frame)
        {
            try
            {
                if (!_isCameraOn || !_client.IsConnected)
                {
                    return;
                }

                if ((DateTime.UtcNow - _lastFrameSentAt).TotalMilliseconds < 220)
                {
                    return;
                }

                if (Interlocked.Exchange(ref _isSendingFrame, 1) == 1)
                {
                    return;
                }

                _lastFrameSentAt = DateTime.UtcNow;
                using Bitmap resized = ResizeFrame(frame, 360, 240);
                string base64Frame = EncodeJpegBase64(resized, 40L);

                await _client.SendAsync(new ClassroomSignalModel
                {
                    Type = ClassroomMessageType.VideoFrame,
                    SessionId = _sessionId,
                    SenderId = UserSessionContext.CurrentUserId ?? 0,
                    SenderName = UserSessionContext.CurrentUsername ?? "Student",
                    SenderRole = "STUDENT",
                    Payload =
                    {
                        ["imageBase64"] = base64Frame,
                        ["width"] = resized.Width.ToString(),
                        ["height"] = resized.Height.ToString()
                    }
                });
            }
            catch
            {
                // Drop frame silently to keep UI/network stable.
            }
            finally
            {
                frame.Dispose();
                Interlocked.Exchange(ref _isSendingFrame, 0);
            }
        }

        private void RenderTeacherVideoFrame(ClassroomSignalModel signal)
        {
            try
            {
                if (!signal.Payload.TryGetValue("imageBase64", out string? base64) || string.IsNullOrWhiteSpace(base64)) return;
                byte[] bytes = Convert.FromBase64String(base64);
                using var stream = new MemoryStream(bytes);
                Image frame = Image.FromStream(stream);

                if (IsDisposed || !_teacherVideo.IsHandleCreated)
                {
                    frame.Dispose();
                    return;
                }

                BeginInvoke(() =>
                {
                    PictureBox target = _teacherCameraPip.Visible ? _teacherCameraPipPreview : _teacherVideo;
                    Image? old = target.Image;
                    target.Image = frame;
                    old?.Dispose();
                    _videoPlaceholder.Visible = false;
                    if (_teacherCameraPip.Visible)
                    {
                        _teacherCameraPip.BringToFront();
                        _statusLabel.Text = $"Đang xem giáo viên trình bày, camera thu nhỏ - {DateTime.Now:HH:mm:ss}";
                    }
                    else
                    {
                        _statusLabel.Text = $"Đang xem video giáo viên - {DateTime.Now:HH:mm:ss}";
                    }
                });
            }
            catch
            {
                // Drop corrupt frames silently.
            }
        }

        private void RenderTeacherScreenShareFrame(ClassroomSignalModel signal)
        {
            try
            {
                if (!signal.Payload.TryGetValue("imageBase64", out string? base64) || string.IsNullOrWhiteSpace(base64)) return;
                byte[] bytes = Convert.FromBase64String(base64);
                using var stream = new MemoryStream(bytes);
                Image frame = Image.FromStream(stream);

                if (IsDisposed || !_teacherVideo.IsHandleCreated)
                {
                    frame.Dispose();
                    return;
                }

                BeginInvoke(() =>
                {
                    Image? old = _teacherVideo.Image;
                    _teacherVideo.Image = frame;
                    old?.Dispose();
                    _videoPlaceholder.Visible = false;
                    _teacherCameraPip.Visible = true;
                    _teacherCameraPip.BringToFront();
                    string sourceTitle = signal.Payload.TryGetValue("sourceTitle", out string? title) ? title : "màn hình";
                    _statusLabel.Text = $"Đang xem giáo viên trình bày: {sourceTitle} - {DateTime.Now:HH:mm:ss}";
                });
            }
            catch
            {
                // Drop corrupt screen-share frames silently.
            }
        }

        private static Bitmap ResizeFrame(Bitmap source, int maxWidth, int maxHeight)
        {
            double ratio = Math.Min((double)maxWidth / source.Width, (double)maxHeight / source.Height);
            int width = Math.Max(1, (int)(source.Width * ratio));
            int height = Math.Max(1, (int)(source.Height * ratio));
            var resized = new Bitmap(width, height);
            using Graphics graphics = Graphics.FromImage(resized);
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            graphics.DrawImage(source, 0, 0, width, height);
            return resized;
        }

        private static string EncodeJpegBase64(Bitmap bitmap, long quality)
        {
            using var stream = new MemoryStream();
            System.Drawing.Imaging.ImageCodecInfo encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                .First(codec => codec.MimeType == "image/jpeg");
            using var encoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
            encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            bitmap.Save(stream, encoder, encoderParameters);
            return Convert.ToBase64String(stream.ToArray());
        }

        private void SafeShowTeacherPlaceholder(string text)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(() =>
            {
                _videoPlaceholder.Text = text;
                _videoPlaceholder.Visible = true;
                _videoPlaceholder.BringToFront();
                Image? old = _teacherVideo.Image;
                _teacherVideo.Image = null;
                old?.Dispose();
                _teacherCameraPip.Visible = false;
                Image? oldPip = _teacherCameraPipPreview.Image;
                _teacherCameraPipPreview.Image = null;
                oldPip?.Dispose();
                _statusLabel.Text = text;
            });
        }

        private void SafeSetStatus(string text)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(() => _statusLabel.Text = text);
        }

        private void SafeAddEvent(string text)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(() => _eventsList.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} - {text}"));
        }

        private void StopCamera()
        {
            if (_camera != null)
            {
                _camera.NewFrame -= Camera_NewFrame;
                if (_camera.IsRunning)
                {
                    _camera.SignalToStop();
                    _camera.WaitForStop();
                }
                _camera = null;
            }

            _isCameraOn = false;
            Image? old = _studentPreview.Image;
            _studentPreview.Image = null;
            old?.Dispose();
        }

        private void StopScreenShare()
        {
            _screenShareTimer?.Stop();
            _screenShareTimer?.Dispose();
            _screenShareTimer = null;
            _isScreenSharing = false;
            _btnShareScreen.Text = "Share màn hình";
            _btnShareScreen.BackColor = Color.FromArgb(139, 92, 246);
            _statusLabel.Text = $"Đã dừng trình bày {_screenShareTitle}.";
            Interlocked.Exchange(ref _isSendingScreenFrame, 0);
        }

        private async void StudentNativeClassroomForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopScreenShare();
            StopCamera();
            if (_client.IsConnected)
            {
                await _client.LeaveRoomAsync(
                    _sessionId,
                    UserSessionContext.CurrentUserId ?? 0,
                    UserSessionContext.CurrentUsername ?? "Student",
                    "STUDENT");
            }

            Image? oldTeacher = _teacherVideo.Image;
            _teacherVideo.Image = null;
            oldTeacher?.Dispose();
            await _client.DisconnectAsync();
        }
        private sealed class ScreenSharePickerDialog : Form
        {
            public Rectangle SelectedBounds { get; private set; }
            public string SelectedTitle { get; private set; } = "Màn hình";

            public ScreenSharePickerDialog()
            {
                Text = "Trình bày ngay";
                StartPosition = FormStartPosition.CenterParent;
                Size = new Size(620, 430);
                MinimumSize = new Size(620, 430);
                BackColor = AppColors.BgBase;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;

                var root = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 3,
                    Padding = new Padding(22),
                    BackColor = AppColors.BgBase
                };
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
                Controls.Add(root);

                root.Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "Chọn nội dung muốn trình bày\nBạn chọn màn hình trước, sau đó lớp học mới nhận được màn hình.",
                    ForeColor = AppColors.TextPrimary,
                    Font = new Font("Segoe UI", 13, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, 0);

                var cards = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    WrapContents = true,
                    BackColor = AppColors.BgCard,
                    Padding = new Padding(12)
                };
                root.Controls.Add(cards, 0, 1);

                Screen[] screens = Screen.AllScreens;
                for (int i = 0; i < screens.Length; i++)
                {
                    Screen screen = screens[i];
                    string title = screens.Length == 1 ? "Toàn bộ màn hình" : $"Màn hình {i + 1}";
                    cards.Controls.Add(CreateScreenCard(title, screen.Bounds, screen.Primary));
                }

                var actions = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    BackColor = Color.Transparent
                };
                root.Controls.Add(actions, 0, 2);

                var cancel = new Button
                {
                    Text = "Hủy",
                    Width = 110,
                    Height = 42,
                    BackColor = AppColors.BgInput,
                    ForeColor = AppColors.TextPrimary,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    DialogResult = DialogResult.Cancel
                };
                cancel.FlatAppearance.BorderSize = 0;
                RoundedButtonHelper.Apply(cancel, 10);
                actions.Controls.Add(cancel);

                SelectedBounds = Screen.PrimaryScreen?.Bounds ?? Screen.AllScreens[0].Bounds;
                SelectedTitle = "Toàn bộ màn hình";
            }

            private Control CreateScreenCard(string title, Rectangle bounds, bool primary)
            {
                var card = new Panel
                {
                    Width = 260,
                    Height = 150,
                    Margin = new Padding(0, 0, 12, 12),
                    BackColor = Color.FromArgb(20, 25, 38),
                    Cursor = Cursors.Hand,
                    Tag = bounds
                };

                var preview = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 92,
                    BackColor = Color.FromArgb(8, 10, 18)
                };
                card.Controls.Add(preview);

                var icon = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "▣",
                    ForeColor = Color.FromArgb(139, 92, 246),
                    Font = new Font("Segoe UI", 34, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                preview.Controls.Add(icon);

                var name = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = primary ? $"{title} · Chính" : title,
                    ForeColor = AppColors.TextPrimary,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(name);

                void Choose(object? _, EventArgs __)
                {
                    SelectedBounds = bounds;
                    SelectedTitle = title;
                    DialogResult = DialogResult.OK;
                    Close();
                }

                card.Click += Choose;
                preview.Click += Choose;
                icon.Click += Choose;
                name.Click += Choose;
                return card;
            }
        }
    }
}
