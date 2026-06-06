using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Chat : UserControl
    {
        private readonly ChatController _chatController;
        private readonly System.Windows.Forms.Timer _pollTimer;
        private readonly List<ChatCourseModel> _courses = new();
        private int _selectedCourseId;
        private bool _isLoadingMessages;
        private bool _hasPendingLocalSend;
        private bool _isSending;
        private bool _uiAlive = true;

        public UC_Chat()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyCardStyle();
            _chatController = new ChatController(new CourseGuardDbContext(""));
            _pollTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _pollTimer.Tick += async (_, _) => await RefreshMessagesAsync();
            Disposed += (_, _) =>
            {
                _uiAlive = false;
                _pollTimer.Stop();
                _pollTimer.Dispose();
            };

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnSend, 10);
            lstContacts.SelectedIndexChanged += async (_, _) => await OnCourseChangedAsync();

            btnSend.Click += async (_, _) => await SendTextAsync();

            var sendMenu = new ContextMenuStrip();
            var attachItem = new ToolStripMenuItem("Gửi file...");
            attachItem.Click += async (_, _) => await SendFileAsync();
            sendMenu.Items.Add(attachItem);
            StudentDropdownStyler.Apply(sendMenu);
            btnSend.ContextMenuStrip = sendMenu;

            txtInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnSend.PerformClick();
                }
                else if (e.Control && e.KeyCode == Keys.O)
                {
                    e.SuppressKeyPress = true;
                    SendFileAsync().FireAndForgetSafe(this);
                }
            };

            Load += async (_, _) =>
            {
                await LoadCoursesAsync();
                _pollTimer.Start();
            };
        }

        private void BuildCardLayout()
        {
            btnSend.Text = "Gửi";
            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Tin nhắn",
                "Trao đổi với lớp học và giáo viên trong các khóa đã tham gia.",
                btnSend), 0, 0);

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

            var contactsCard = StudentTabChrome.CreateDataCard("Khóa học", lstContacts);
            contactsCard.Margin = new Padding(0, 0, 12, 0);

            var messageShell = StudentTabChrome.CreateCard();
            messageShell.Padding = new Padding(18);
            messageShell.Margin = new Padding(12, 0, 0, 0);
            var messageGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 3
            };
            messageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
            messageGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            messageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            messageGrid.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = MetaTheme.Fonts.HeadingSm(),
                ForeColor = AppColors.TextPrimary,
                Text = "Nội dung trao đổi",
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            }, 0, 0);
            txtMessages.Dock = DockStyle.Fill;
            txtMessages.Margin = Padding.Empty;
            messageGrid.Controls.Add(txtMessages, 0, 1);

            var composer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 12, 0, 0)
            };
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108f));
            txtInput.Dock = DockStyle.Fill;
            txtInput.Margin = new Padding(0, 0, 10, 0);
            btnSend.Dock = DockStyle.Fill;
            btnSend.Margin = Padding.Empty;
            composer.Controls.Add(txtInput, 0, 0);
            composer.Controls.Add(btnSend, 1, 0);
            messageGrid.Controls.Add(composer, 0, 2);
            messageShell.Controls.Add(messageGrid);

            content.Controls.Add(contactsCard, 0, 0);
            content.Controls.Add(messageShell, 1, 0);
            root.Controls.Add(content, 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this);
        }

        private void ApplyCardStyle()
        {
            BackColor = AppColors.BgBase;
            StudentTabChrome.StylePrimaryButton(btnSend);
            lstContacts.BorderStyle = BorderStyle.None;
            lstContacts.BackColor = AppColors.BgCard;
            lstContacts.ForeColor = AppColors.TextPrimary;
            txtMessages.BorderStyle = BorderStyle.None;
            txtMessages.BackColor = AppColors.BgCard;
            txtMessages.ForeColor = AppColors.TextPrimary;
            txtInput.BackColor = AppColors.BgCard;
            txtInput.ForeColor = AppColors.TextPrimary;
        }

        private async Task LoadCoursesAsync()
        {
            this.ShowSkeleton(SkeletonType.ChatLayout);
            try
            {
                // Xóa dữ liệu mock trên designer để tránh hiển thị sai trước khi query DB xong.
                lstContacts.Items.Clear();
                txtMessages.Text = "Đang tải danh sách chat...";

                int userId = UserSessionContext.CurrentUserId ?? 0;
                if (userId <= 0)
                {
                    txtMessages.Text = "Bạn cần đăng nhập để sử dụng chat.";
                    return;
                }

                List<ChatCourseModel> courses = await _chatController.GetMyCoursesAsync(userId);
                _courses.Clear();
                _courses.AddRange(courses);

                lstContacts.Items.Clear();
                foreach (ChatCourseModel course in _courses)
                {
                    string roleTag = course.IsTeacherCourse ? "GV" : "HV";
                    string display = $"[{roleTag}] {course.CourseName}";
                    if (!string.IsNullOrWhiteSpace(course.ClassCode))
                    {
                        display += $" - {course.ClassCode}";
                    }

                    lstContacts.Items.Add(display);
                }

                if (lstContacts.Items.Count > 0)
                {
                    lstContacts.ClearSelected();
                    _selectedCourseId = 0;
                    txtMessages.Text = "Chọn khóa học để xem tin nhắn.";
                }
                else
                {
                    txtMessages.Text = "Bạn chưa có khóa học nào để chat.";
                }
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private async Task OnCourseChangedAsync()
        {
            if (!_uiAlive || IsDisposed)
            {
                return;
            }

            int idx = lstContacts.SelectedIndex;
            if (idx < 0 || idx >= _courses.Count)
            {
                _selectedCourseId = 0;
                return;
            }

            _selectedCourseId = _courses[idx].CourseId;
            await RefreshMessagesAsync();
        }

        private async Task RefreshMessagesAsync()
        {
            if (!_uiAlive || IsDisposed)
            {
                return;
            }

            if (_isLoadingMessages || _selectedCourseId <= 0 || _hasPendingLocalSend)
            {
                return;
            }

            try
            {
                _isLoadingMessages = true;
                int userId = UserSessionContext.CurrentUserId ?? 0;
                int courseId = _selectedCourseId;
                List<ChatMessageModel> messages = await _chatController.GetMessagesAsync(userId, courseId, 200);

                // Có thể control đã bị đóng/đổi course trong lúc chờ DB xong.
                if (!_uiAlive || IsDisposed || txtMessages.IsDisposed || _selectedCourseId != courseId)
                {
                    return;
                }

                txtMessages.Clear();
                foreach (ChatMessageModel msg in messages.OrderBy(m => m.SentAt))
                {
                    string when = msg.SentAt.ToString("HH:mm");
                    if (string.Equals(msg.MessageType, "FILE", StringComparison.OrdinalIgnoreCase))
                    {
                        string sizeText = msg.FileSize <= 0 ? "-" : $"{Math.Round(msg.FileSize / 1024.0, 1)} KB";
                        txtMessages.AppendText($"[{when}] {msg.SenderName}: [FILE] {msg.FileName} ({sizeText}){Environment.NewLine}");
                        if (!string.IsNullOrWhiteSpace(msg.Content))
                        {
                            txtMessages.AppendText($"  Ghi chú: {msg.Content}{Environment.NewLine}");
                        }
                        txtMessages.AppendText($"  Lưu tại: {msg.FileUrl}{Environment.NewLine}");
                    }
                    else
                    {
                        txtMessages.AppendText($"[{when}] {msg.SenderName}: {msg.Content}{Environment.NewLine}");
                    }
                }

                if (messages.Count == 0)
                {
                    txtMessages.Text = "Chưa có tin nhắn nào trong phòng chat này.";
                }
                MarkDisplayedMessagesReadAsync(courseId).FireAndForgetSafe(this);
            }
            catch (ObjectDisposedException)
            {
                // Control đã bị dispose khi async đang chạy -> bỏ qua để tránh crash.
                return;
            }
            finally
            {
                _isLoadingMessages = false;
            }
        }

        private Task MarkDisplayedMessagesReadAsync(int courseId)
        {
            int userId = UserSessionContext.CurrentUserId ?? 0;
            if (userId <= 0 || courseId <= 0)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                try
                {
                    _chatController.MarkCourseRead(userId, courseId);
                }
                catch
                {
                }
            });
        }

        private async Task SendTextAsync()
        {
            if (_isSending)
                return;

            string content = txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(content))
                return;

            int userId = UserSessionContext.CurrentUserId ?? 0;
            if (userId <= 0 || _selectedCourseId <= 0)
            {
                MetaTheme.ShowModernDialog("Không tìm thấy phòng chat hợp lệ.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetComposerSending(true);
            AppendLocalSendStatus("TEXT", "PENDING", content);
            try
            {
                bool sent = await _chatController.SendMessageAsync(userId, _selectedCourseId, content);
                if (!sent)
                {
                    AppendLocalSendStatus("TEXT", "FAILED", content, _chatController.LastErrorMessage);
                    MetaTheme.ShowModernDialog(_chatController.LastErrorMessage, "Gửi tin nhắn thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtInput.Clear();
                AppendLocalSendStatus("TEXT", "SENT", content);
                await RefreshMessagesAsync();
            }
            finally
            {
                SetComposerSending(false);
            }
        }

        private async Task SendFileAsync()
        {
            if (_isSending)
                return;

            int userId = UserSessionContext.CurrentUserId ?? 0;
            if (userId <= 0 || _selectedCourseId <= 0)
            {
                MetaTheme.ShowModernDialog("Không tìm thấy phòng chat hợp lệ.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = "Chọn file để gửi",
                Filter = "Supported files|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.png;*.jpg;*.jpeg;*.txt;*.zip;*.rar|All files|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
            {
                return;
            }

            string fileName = Path.GetFileName(dialog.FileName);
            string caption = $"Gửi file: {fileName}";
            SetComposerSending(true);
            AppendLocalSendStatus("FILE", "PENDING", fileName);
            try
            {
                bool sent = await _chatController.SendFileMessageAsync(userId, _selectedCourseId, dialog.FileName, caption);
                if (!sent)
                {
                    AppendLocalSendStatus("FILE", "FAILED", fileName, _chatController.LastErrorMessage);
                    MetaTheme.ShowModernDialog(_chatController.LastErrorMessage, "Gửi file thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppendLocalSendStatus("FILE", "SENT", fileName);
                await RefreshMessagesAsync();
            }
            finally
            {
                SetComposerSending(false);
            }
        }

        private void AppendLocalSendStatus(string messageType, string status, string preview, string? errorMessage = null)
        {
            if (!_uiAlive || IsDisposed || txtMessages.IsDisposed)
                return;

            _hasPendingLocalSend = string.Equals(status, "PENDING", StringComparison.OrdinalIgnoreCase);
            if (ShouldClearBeforeLocalStatus(txtMessages.Text))
                txtMessages.Clear();

            txtMessages.AppendText(ChatSendStatusLineFormatter.Render("Bạn", preview, messageType, status, errorMessage) + Environment.NewLine);
            txtMessages.SelectionStart = txtMessages.TextLength;
            txtMessages.ScrollToCaret();
        }

        private void SetComposerSending(bool sending)
        {
            _isSending = sending;
            btnSend.Enabled = !sending;
            txtInput.Enabled = !sending;
        }

        private static bool ShouldClearBeforeLocalStatus(string currentText)
        {
            if (string.IsNullOrWhiteSpace(currentText))
                return false;

            string trimmed = currentText.TrimStart();
            return trimmed.StartsWith("Đang tải", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("Chọn khóa", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("Chưa có", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("Bạn cần", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("Bạn chưa", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("Không tìm", StringComparison.OrdinalIgnoreCase);
        }
    }
}
