using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
using CourseGuard.Frontend.UserControls.Shared.Chat;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Chat : UserControl
    {
        private readonly ChatController _chatController;
        private readonly System.Windows.Forms.Timer _pollTimer;
        private readonly List<ChatCourseModel> _courses = new();
        private readonly ChatMessageListControl _messageList = new();
        private int _selectedCourseId;
        private bool _isLoadingMessages;
        private bool _isLoadingOlderMessages;
        private bool _hasLoadedInitialMessages;
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

            RoundedButtonHelper.Apply(btnSend, 10);
            lstContacts.SelectedIndexChanged += async (_, _) => await OnCourseChangedAsync();
            lstContacts.DrawItem += DrawCourseItem;
            _messageList.TopReached += async (_, _) => await LoadOlderMessagesAsync();
            _messageList.PollVoteRequested += async (_, args) => await VotePollAsync(args);
            _messageList.PollCloseRequested += (_, args) =>
            {
                _messageList.SetPollActionPending(args.MessageId, false);
                MetaTheme.ShowModernDialog("Chỉ giảng viên mới được đóng vote.", "Vote", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

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
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24f));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 76f));
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
            _messageList.Dock = DockStyle.Fill;
            _messageList.Margin = Padding.Empty;
            messageGrid.Controls.Add(_messageList, 0, 1);

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
            lstContacts.Font = AppFonts.Semibold(11f);
            lstContacts.DrawMode = DrawMode.OwnerDrawFixed;
            lstContacts.ItemHeight = 42;
            lstContacts.IntegralHeight = false;
            lstContacts.HorizontalScrollbar = false;
            lstContacts.Margin = new Padding(0);
            _messageList.BackColor = AppColors.BgCard;
            txtMessages.Visible = false;
            txtInput.BackColor = AppColors.BgCard;
            txtInput.ForeColor = AppColors.TextPrimary;
        }

        private void DrawCourseItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstContacts.Items.Count)
                return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Rectangle bounds = e.Bounds;
            Color backColor = selected ? MetaTheme.Colors.Primary : AppColors.BgCard;
            Color textColor = selected ? Color.White : AppColors.TextPrimary;

            using var canvas = new SolidBrush(AppColors.BgCard);
            e.Graphics.FillRectangle(canvas, bounds);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle itemBounds = new(bounds.Left + 8, bounds.Top + 3, bounds.Width - 16, bounds.Height - 6);
            using GraphicsPath itemPath = CreateRoundedRectanglePath(itemBounds, 10);
            using var background = new SolidBrush(backColor);
            e.Graphics.FillPath(background, itemPath);

            if (!selected)
            {
                using var border = new Pen(MetaTheme.Colors.BorderSoft);
                e.Graphics.DrawPath(border, itemPath);
            }

            Rectangle textBounds = new(itemBounds.Left + 12, itemBounds.Top + 1, itemBounds.Width - 24, itemBounds.Height - 2);
            TextRenderer.DrawText(
                e.Graphics,
                lstContacts.Items[e.Index]?.ToString() ?? string.Empty,
                lstContacts.Font,
                textBounds,
                textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            e.DrawFocusRectangle();
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private async Task LoadCoursesAsync()
        {
            this.ShowSkeleton(SkeletonType.ChatLayout);
            try
            {
                // Xóa dữ liệu mock trên designer để tránh hiển thị sai trước khi query DB xong.
                lstContacts.Items.Clear();
                _messageList.ClearMessages();

                int userId = UserSessionContext.CurrentUserId ?? 0;
                if (userId <= 0)
                {
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
                    lstContacts.SelectedIndex = 0;
                }
                else
                {
                    _selectedCourseId = 0;
                    _hasLoadedInitialMessages = false;
                    _messageList.ClearMessages();
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
            _hasLoadedInitialMessages = false;
            _messageList.ClearMessages();
            await RefreshMessagesAsync();
        }

        private async Task RefreshMessagesAsync()
        {
            if (!_uiAlive || IsDisposed)
            {
                return;
            }

            if (_isLoadingMessages || _selectedCourseId <= 0)
            {
                return;
            }

            int userId = UserSessionContext.CurrentUserId ?? 0;
            int courseId = _selectedCourseId;
            if (userId <= 0)
            {
                return;
            }

            try
            {
                _isLoadingMessages = true;
                List<ChatMessageModel> messages;
                int newestMessageId = _messageList.NewestMessageId ?? 0;
                if (!_hasLoadedInitialMessages || newestMessageId <= 0)
                {
                    messages = await _chatController.GetMessagesAsync(userId, courseId, 20);
                }
                else
                {
                    messages = await _chatController.GetMessagesAfterAsync(userId, courseId, newestMessageId, 50);
                }

                // Có thể control đã bị đóng/đổi course trong lúc chờ DB xong.
                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                {
                    return;
                }

                if (!_hasLoadedInitialMessages)
                {
                    _messageList.SetMessages(messages, userId);
                    _hasLoadedInitialMessages = true;
                }
                else
                {
                    _messageList.AppendMessages(messages, userId);
                }

                if (messages.Count == 0)
                {
                    return;
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

        private async Task LoadOlderMessagesAsync()
        {
            if (!_uiAlive || IsDisposed || _isLoadingOlderMessages || _selectedCourseId <= 0)
            {
                return;
            }

            int? oldestMessageId = _messageList.OldestMessageId;
            if (!oldestMessageId.HasValue || oldestMessageId.Value <= 0)
            {
                return;
            }

            int userId = UserSessionContext.CurrentUserId ?? 0;
            int courseId = _selectedCourseId;
            if (userId <= 0)
            {
                return;
            }

            try
            {
                _isLoadingOlderMessages = true;
                List<ChatMessageModel> olderMessages = await _chatController.GetMessagesBeforeAsync(userId, courseId, oldestMessageId.Value, 20);
                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                {
                    return;
                }

                _messageList.PrependOlderMessages(olderMessages, userId);
            }
            finally
            {
                _isLoadingOlderMessages = false;
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

                await RefreshMessagesAsync();
            }
            finally
            {
                SetComposerSending(false);
            }
        }

        private async Task VotePollAsync(PollVoteEventArgs args)
        {
            int userId = UserSessionContext.CurrentUserId ?? 0;
            int courseId = _selectedCourseId;
            if (userId <= 0 || courseId <= 0)
            {
                _messageList.SetPollActionPending(args.MessageId, false);
                MetaTheme.ShowModernDialog("Không tìm thấy phòng chat hợp lệ.", "Vote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool success = await _chatController.VotePollAsync(userId, args.PollId, args.OptionId);
                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                {
                    return;
                }

                if (!success)
                {
                    MetaTheme.ShowModernDialog(_chatController.LastErrorMessage, "Vote thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await RefreshSinglePollBubbleAsync(args.MessageId, userId, courseId);
            }
            catch (Exception ex)
            {
                if (_uiAlive && !IsDisposed)
                {
                    MetaTheme.ShowModernDialog($"Không thể gửi vote lúc này.\n{ex.Message}", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (_uiAlive && !IsDisposed && !_messageList.IsDisposed)
                {
                    _messageList.SetPollActionPending(args.MessageId, false);
                }
            }
        }

        private async Task RefreshSinglePollBubbleAsync(int messageId, int userId, int courseId)
        {
            List<ChatMessageModel> latestMessages = await _chatController.GetMessagesAsync(userId, courseId, 50);
            if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
            {
                return;
            }

            ChatMessageModel? updatedMessage = latestMessages.FirstOrDefault(message => message.Id == messageId);
            if (updatedMessage?.Poll != null)
            {
                _messageList.RefreshPollBubble(updatedMessage);
            }
        }

        private void AppendLocalSendStatus(string messageType, string status, string preview, string? errorMessage = null)
        {
            if (!_uiAlive || IsDisposed || !string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            MetaTheme.ShowModernDialog(errorMessage ?? "Gửi tin nhắn thất bại.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void SetComposerSending(bool sending)
        {
            _isSending = sending;
            btnSend.Enabled = !sending;
            txtInput.Enabled = !sending;
        }


    }
}
