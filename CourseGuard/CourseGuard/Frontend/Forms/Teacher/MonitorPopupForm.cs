using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Services.Monitoring;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class MonitorPopupForm : Form
    {
        private readonly int _examId;
        private readonly int _studentId;
        private readonly int _attemptId;
        private readonly CancellationTokenSource _cts = new();
        private readonly TcpScreenMonitorService _service = new();
        private readonly PictureBox _picture = new();
        private readonly Label _status = new();

        public MonitorPopupForm(int teacherId, int examId, int studentId, int attemptId)
        {
            _examId = examId;
            _studentId = studentId;
            _attemptId = attemptId;
            InitializeComponent();
            Text = $"Giám sát thi - HS {studentId}";
            Width = 1100;
            Height = 760;
            StartPosition = FormStartPosition.CenterParent;

            _status.Dock = DockStyle.Top;
            _status.Height = 38;
            _status.Text = "Đang kết nối";
            _status.TextAlign = ContentAlignment.MiddleLeft;
            _status.Padding = new Padding(12, 0, 0, 0);

            _picture.Dock = DockStyle.Fill;
            _picture.BackColor = Color.Black;
            _picture.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(_picture);
            Controls.Add(_status);

            _service.FrameReceived += Service_FrameReceived;
            _service.StatusChanged += (_, e) => SetStatus(e.Status);
            FormClosing += (_, _) =>
            {
                _cts.Cancel();
                _service.Dispose();
                _picture.Image?.Dispose();
            };
            _ = Task.Run(() => _service.StartAsync(_examId, _studentId, _attemptId, _cts.Token));
        }

        private void Service_FrameReceived(object? sender, ScreenFrameReceivedEventArgs e)
        {
            using var ms = new MemoryStream(e.JpegBytes);
            using var source = Image.FromStream(ms);
            var frame = new Bitmap(source);
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => SetFrame(frame)));
            }
            else
            {
                SetFrame(frame);
            }
        }

        private void SetFrame(Image frame)
        {
            Image? old = _picture.Image;
            _picture.Image = frame;
            old?.Dispose();
            _status.Text = "Đang nhận hình";
        }

        private void SetStatus(string status)
        {
            if (InvokeRequired)
                BeginInvoke(new MethodInvoker(() => _status.Text = status));
            else
                _status.Text = status;
        }
    }
}
