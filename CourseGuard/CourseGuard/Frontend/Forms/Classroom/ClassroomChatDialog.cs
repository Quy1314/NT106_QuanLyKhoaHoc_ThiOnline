using CourseGuard.Frontend.Theme;
using System.Runtime.InteropServices;

namespace CourseGuard.Frontend.Forms.Classroom
{
    public sealed class ClassroomChatDialog : Form
    {
        private const int WmNcLButtonDown = 0xA1;
        private const int HtCaption = 0x2;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private static readonly Color ChatSurface = ColorTranslator.FromHtml("#1E1E2E");
        private static readonly Color ChatSurfaceDeep = ColorTranslator.FromHtml("#16161A");
        private static readonly Color ChatSurfaceHover = ColorTranslator.FromHtml("#252535");
        private static readonly Color ChatTextPrimary = ColorTranslator.FromHtml("#F1F5F9");
        private static readonly Color ChatTextMuted = ColorTranslator.FromHtml("#94A3B8");

        private readonly ListBox _chatList;
        private readonly TextBox _chatInput;
        private readonly Button _btnSend;
        private readonly Button _btnClose;

        public event Func<string, Task>? SendRequested;

        public ClassroomChatDialog(string title)
        {
            Text = string.Empty;
            StartPosition = FormStartPosition.Manual;
            MinimumSize = new Size(420, 520);
            Size = new Size(520, 660);
            BackColor = ChatSurface;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            Padding = Padding.Empty;

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = ChatSurface,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            Controls.Add(shell);

            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ChatSurface,
                Padding = new Padding(18, 0, 10, 0),
                Margin = Padding.Empty,
                Cursor = Cursors.SizeAll
            };
            header.MouseDown += Header_MouseDown;
            shell.Controls.Add(header, 0, 0);

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = ChatSurface,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
            header.Controls.Add(headerLayout);

            var headerTitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Chat lớp học",
                ForeColor = ChatTextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = MetaTheme.Fonts.SubtitleLg(),
                BackColor = ChatSurface,
                Cursor = Cursors.SizeAll
            };
            headerTitle.MouseDown += Header_MouseDown;
            headerLayout.Controls.Add(headerTitle, 0, 0);

            _btnClose = new Button
            {
                Dock = DockStyle.Fill,
                Text = "✕",
                BackColor = ChatSurface,
                ForeColor = ChatTextMuted,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.FlatAppearance.MouseOverBackColor = AppColors.Danger;
            _btnClose.FlatAppearance.MouseDownBackColor = AppColors.Danger;
            _btnClose.Click += (_, _) => Hide();
            headerLayout.Controls.Add(_btnClose, 1, 0);

            _chatList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = ChatSurface,
                ForeColor = ChatTextPrimary,
                BorderStyle = BorderStyle.None,
                Font = MetaTheme.Fonts.BodyMdBold(),
                IntegralHeight = false,
                HorizontalScrollbar = true,
                Margin = Padding.Empty
            };
            shell.Controls.Add(_chatList, 0, 1);

            var composer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = ChatSurface,
                Padding = new Padding(14, 10, 14, 10),
                Margin = Padding.Empty
            };
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            shell.Controls.Add(composer, 0, 2);

            _chatInput = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.None,
                Font = MetaTheme.Fonts.BodyMdBold(),
                PlaceholderText = "Nhập tin nhắn...",
                Margin = new Padding(0, 7, 10, 0)
            };
            _chatInput.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    await SendCurrentMessageAsync();
                }
            };
            composer.Controls.Add(_chatInput, 0, 0);

            _btnSend = new Button
            {
                Dock = DockStyle.Fill,
                Text = "Gửi",
                BackColor = AppColors.AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = MetaTheme.Fonts.BodySmBold(),
                Cursor = Cursors.Hand,
                Margin = Padding.Empty
            };
            _btnSend.FlatAppearance.BorderSize = 0;
            RoundedButtonHelper.Apply(_btnSend, 10);
            _btnSend.Click += async (_, _) => await SendCurrentMessageAsync();
            composer.Controls.Add(_btnSend, 1, 0);

            FormClosing += (_, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    Hide();
                }
            };
        }

        public bool IsOpen => Visible && !IsDisposed;

        public void AppendMessage(string senderName, string message, bool isTeacher)
        {
            if (IsDisposed) return;

            string label = isTeacher ? "GV" : "HS";
            _chatList.Items.Add($"[{DateTime.Now:HH:mm}] {label} {senderName}: {message}");
            _chatList.TopIndex = Math.Max(0, _chatList.Items.Count - 1);
        }

        public void FocusComposer()
        {
            if (IsDisposed) return;
            _chatInput.Focus();
        }

        private void Header_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WmNcLButtonDown, HtCaption, 0);
        }

        private async Task SendCurrentMessageAsync()
        {
            string message = _chatInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;

            _chatInput.Clear();
            if (SendRequested != null)
            {
                await SendRequested.Invoke(message);
            }
        }
    }
}
