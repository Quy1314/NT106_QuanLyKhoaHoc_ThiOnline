using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherMessages : UserControl
    {
        private readonly int _teacherId;
        private readonly ChatController _chat = new(new CourseGuardDbContext(""));
        private readonly List<ChatCourseModel> _courses = new();
        private readonly System.Windows.Forms.Timer _pollTimer = new() { Interval = 3000 };
        private readonly ListBox _courseList = new();
        private readonly TextBox _messages = new();
        private readonly TextBox _input = new();
        private readonly Button _send = TeacherTabChrome.PrimaryButton("Gửi");
        private readonly Button _attach = TeacherTabChrome.SecondaryButton("File");
        private int _selectedCourseId;
        private bool _isLoadingMessages;
        private bool _uiAlive = true;

        public UC_TeacherMessages(int teacherId)
        {
            _teacherId = teacherId;
            InitializeComponent();
            BuildLayout();
            ApplyStyles();

            _courseList.SelectedIndexChanged += async (_, _) => await OnCourseChangedAsync();
            _send.Click += async (_, _) => await SendTextAsync();
            _attach.Click += async (_, _) => await SendFileAsync();
            _input.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _ = SendTextAsync();
                }
                else if (e.Control && e.KeyCode == Keys.O)
                {
                    e.SuppressKeyPress = true;
                    _ = SendFileAsync();
                }
            };

            _pollTimer.Tick += async (_, _) => await RefreshMessagesAsync();
            Load += async (_, _) =>
            {
                await LoadCoursesAsync();
                _pollTimer.Start();
            };
            Disposed += (_, _) =>
            {
                _uiAlive = false;
                _pollTimer.Stop();
                _pollTimer.Dispose();
            };
        }

        private void BuildLayout()
        {
            var root = TeacherTabChrome.CreateRoot(this);
            root.Controls.Add(TeacherTabChrome.CreateHeader(
                "Tin nhắn",
                "Trao đổi với học viên trong các phòng chat khóa học.",
                _attach,
                _send), 0, 0);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgBase,
                ColumnCount = 2,
                RowCount = 1
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var courseCard = TeacherTabChrome.CreateDataCard("Khóa học", _courseList);
            courseCard.Margin = new Padding(0, 0, 12, 0);
            content.Controls.Add(courseCard, 0, 0);

            var chatCard = TeacherTabChrome.CreateCard();
            chatCard.Padding = new Padding(18);
            chatCard.Margin = new Padding(12, 0, 0, 0);

            var chatGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 3
            };
            chatGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            chatGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            chatGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
            chatGrid.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = AppFonts.Semibold(12f),
                ForeColor = AppColors.TextPrimary,
                Text = "Nội dung trao đổi",
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            _messages.Multiline = true;
            _messages.ReadOnly = true;
            _messages.ScrollBars = ScrollBars.Vertical;
            chatGrid.Controls.Add(_messages, 0, 1);

            var composer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 12, 0, 0)
            };
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92f));
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92f));

            _input.PlaceholderText = "Nhập tin nhắn...";
            _input.Dock = DockStyle.Fill;
            _input.Margin = new Padding(0, 0, 10, 0);
            _attach.Dock = DockStyle.Fill;
            _attach.Margin = new Padding(0, 0, 10, 0);
            _send.Dock = DockStyle.Fill;
            _send.Margin = Padding.Empty;
            composer.Controls.Add(_input, 0, 0);
            composer.Controls.Add(_attach, 1, 0);
            composer.Controls.Add(_send, 2, 0);
            chatGrid.Controls.Add(composer, 0, 2);

            chatCard.Controls.Add(chatGrid);
            content.Controls.Add(chatCard, 1, 0);
            root.Controls.Add(content, 0, 1);
            TeacherTabChrome.EnableNaturalFocusClear(this);
        }

        private void ApplyStyles()
        {
            BackColor = AppColors.BgBase;
            _courseList.BorderStyle = BorderStyle.None;
            _courseList.BackColor = AppColors.BgCard;
            _courseList.ForeColor = AppColors.TextPrimary;
            _courseList.Font = AppFonts.Body;
            _messages.BorderStyle = BorderStyle.None;
            _messages.BackColor = AppColors.BgCard;
            _messages.ForeColor = AppColors.TextPrimary;
            _messages.Font = AppFonts.Body;
            _input.BackColor = AppColors.BgCard;
            _input.ForeColor = AppColors.TextPrimary;
            _input.Font = AppFonts.Body;
        }

        private async Task LoadCoursesAsync()
        {
            _courseList.Items.Clear();
            _messages.Text = "Đang tải danh sách chat...";

            if (_teacherId <= 0)
            {
                _messages.Text = "Không tìm thấy tài khoản giáo viên.";
                return;
            }

            List<ChatCourseModel> courses = await Task.Run(() => _chat.GetMyCourses(_teacherId));
            if (!_uiAlive || IsDisposed)
                return;

            _courses.Clear();
            _courses.AddRange(courses);
            _courseList.Items.Clear();
            foreach (ChatCourseModel course in _courses)
            {
                string display = course.CourseName;
                if (!string.IsNullOrWhiteSpace(course.ClassCode))
                    display += $" - {course.ClassCode}";
                _courseList.Items.Add(display);
            }

            if (_courseList.Items.Count == 0)
            {
                _selectedCourseId = 0;
                _messages.Text = "Chưa có khóa học nào để chat.";
                return;
            }

            _courseList.ClearSelected();
            _selectedCourseId = 0;
            _messages.Text = "Chọn khóa học để xem tin nhắn.";
        }

        private async Task OnCourseChangedAsync()
        {
            int index = _courseList.SelectedIndex;
            if (index < 0 || index >= _courses.Count)
            {
                _selectedCourseId = 0;
                return;
            }

            _selectedCourseId = _courses[index].CourseId;
            await RefreshMessagesAsync();
        }

        private async Task RefreshMessagesAsync()
        {
            if (!_uiAlive || IsDisposed || _isLoadingMessages || _selectedCourseId <= 0)
                return;

            try
            {
                _isLoadingMessages = true;
                List<ChatMessageModel> messages = await Task.Run(() => _chat.GetMessages(_teacherId, _selectedCourseId, 200));
                if (!_uiAlive || IsDisposed || _messages.IsDisposed)
                    return;

                _messages.Clear();
                foreach (ChatMessageModel message in messages.OrderBy(m => m.SentAt))
                {
                    string when = message.SentAt.ToString("HH:mm");
                    if (string.Equals(message.MessageType, "FILE", StringComparison.OrdinalIgnoreCase))
                    {
                        string sizeText = message.FileSize <= 0 ? "-" : $"{Math.Round(message.FileSize / 1024.0, 1)} KB";
                        _messages.AppendText($"[{when}] {message.SenderName}: [FILE] {message.FileName} ({sizeText}){Environment.NewLine}");
                        if (!string.IsNullOrWhiteSpace(message.Content))
                            _messages.AppendText($"  Ghi chú: {message.Content}{Environment.NewLine}");
                        _messages.AppendText($"  Lưu tại: {message.FileUrl}{Environment.NewLine}");
                    }
                    else
                    {
                        _messages.AppendText($"[{when}] {message.SenderName}: {message.Content}{Environment.NewLine}");
                    }
                }

                if (messages.Count == 0)
                    _messages.Text = "Chưa có tin nhắn nào trong phòng chat này.";
            }
            finally
            {
                _isLoadingMessages = false;
            }
        }

        private async Task SendTextAsync()
        {
            string content = _input.Text.Trim();
            if (string.IsNullOrWhiteSpace(content))
                return;

            if (_teacherId <= 0 || _selectedCourseId <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn phòng chat hợp lệ.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool sent = await Task.Run(() => _chat.SendMessage(_teacherId, _selectedCourseId, content));
            if (!sent)
            {
                MetaTheme.ShowModernDialog(_chat.LastErrorMessage, "Gửi tin nhắn thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _input.Clear();
            await RefreshMessagesAsync();
        }

        private async Task SendFileAsync()
        {
            if (_teacherId <= 0 || _selectedCourseId <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn phòng chat hợp lệ.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = "Chọn file để gửi",
                Filter = "Supported files|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.png;*.jpg;*.jpeg;*.txt;*.zip;*.rar|All files|*.*"
            };

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
                return;

            string caption = $"Gửi file: {Path.GetFileName(dialog.FileName)}";
            bool sent = await Task.Run(() => _chat.SendFileMessage(_teacherId, _selectedCourseId, dialog.FileName, caption));
            if (!sent)
            {
                MetaTheme.ShowModernDialog(_chat.LastErrorMessage, "Gửi file thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RefreshMessagesAsync();
        }
    }
}
