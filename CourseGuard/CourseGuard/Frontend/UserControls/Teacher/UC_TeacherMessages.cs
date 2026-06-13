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
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Shared.Chat;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherMessages : UserControl
    {
        private readonly int _teacherId;
        private readonly ChatController _chat = new(new CourseGuardDbContext(""));
        private readonly List<ChatCourseModel> _courses = new();
        private readonly System.Windows.Forms.Timer _pollTimer = new() { Interval = 3000 };
        private readonly ListBox _courseList = new();
        private readonly ChatMessageListControl _messageList = new();
        private readonly TextBox _input = new();
        private readonly Button _send = TeacherTabChrome.PrimaryButton("Gửi");
        private readonly Button _attach = TeacherTabChrome.SecondaryButton("Ảnh");
        private readonly Button _createPoll = TeacherTabChrome.SecondaryButton("Vote");
        private int _selectedCourseId;
        private bool _isLoadingMessages;
        private bool _isLoadingOlderMessages;
        private bool _hasLoadedInitialMessages;
        private bool _isSending;
        private bool _uiAlive = true;
        private int _nextTemporaryMessageId = -1;

        public UC_TeacherMessages(int teacherId)
        {
            _teacherId = teacherId;
            InitializeComponent();
            BuildLayout();
            ApplyStyles();
            AllowDrop = true;
            _messageList.AllowDrop = true;

            DragEnter += OnChatDragEnter;
            DragDrop += async (_, e) => await OnChatDragDropAsync(e);
            _messageList.DragEnter += OnChatDragEnter;
            _messageList.DragDrop += async (_, e) => await OnChatDragDropAsync(e);

            _courseList.SelectedIndexChanged += async (_, _) => await OnCourseChangedAsync();
            _messageList.TopReached += async (_, _) => await LoadOlderMessagesAsync();
            _messageList.PollVoteRequested += async (_, args) => await VotePollAsync(args);
            _messageList.PollCloseRequested += async (_, args) => await ClosePollAsync(args);
            _courseList.DrawItem += DrawCourseItem;
            _send.Click += async (_, _) => await SendTextAsync();
            _attach.Click += async (_, _) => await PickAndSendImageAsync();
            _createPoll.Click += async (_, _) => await CreatePollAsync();
            _input.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    SendTextAsync().FireAndForgetSafe(this);
                }
                else if (e.Control && e.KeyCode == Keys.O)
                {
                    e.SuppressKeyPress = true;
                    PickAndSendImageAsync().FireAndForgetSafe(this);
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
                _createPoll,
                _attach,
                _send), 0, 0);

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

            var courseCard = TeacherTabChrome.CreateDataCard("Khóa học", _courseList);
            courseCard.Margin = new Padding(0, 0, 8, 0);
            content.Controls.Add(courseCard, 0, 0);

            var chatCard = TeacherTabChrome.CreateCard();
            chatCard.Padding = new Padding(12);
            chatCard.Margin = new Padding(8, 0, 0, 0);

            var chatGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgCard,
                ColumnCount = 1,
                RowCount = 3
            };
            chatGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
            chatGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            chatGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
            chatGrid.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = AppColors.BgCard,
                Font = AppFonts.Semibold(12f),
                ForeColor = AppColors.TextPrimary,
                Text = "Nội dung trao đổi",
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            _messageList.Dock = DockStyle.Fill;
            _messageList.Margin = new Padding(0);
            chatGrid.Controls.Add(_messageList, 0, 1);

            var composer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgCard,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0, 12, 0, 0)
            };
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92f));
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92f));
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92f));

            _input.PlaceholderText = "Nhập tin nhắn...";
            _input.Dock = DockStyle.Fill;
            _input.Margin = new Padding(0, 0, 10, 0);
            _input.Multiline = true;
            _input.ScrollBars = ScrollBars.Vertical;
            _createPoll.Dock = DockStyle.Fill;
            _createPoll.Margin = new Padding(0, 0, 10, 0);
            _attach.Dock = DockStyle.Fill;
            _attach.Margin = new Padding(0, 0, 10, 0);
            _send.Dock = DockStyle.Fill;
            _send.Margin = Padding.Empty;
            composer.Controls.Add(_input, 0, 0);
            composer.Controls.Add(_createPoll, 1, 0);
            composer.Controls.Add(_attach, 2, 0);
            composer.Controls.Add(_send, 3, 0);
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
            _courseList.Font = AppFonts.Semibold(11f);
            _courseList.DrawMode = DrawMode.OwnerDrawFixed;
            _courseList.ItemHeight = 42;
            _courseList.IntegralHeight = false;
            _courseList.HorizontalScrollbar = false;
            _courseList.Margin = new Padding(0);
            _messageList.BackColor = AppColors.BgCard;
            _input.BackColor = AppColors.BgCard;
            _input.ForeColor = AppColors.TextPrimary;
            _input.Font = AppFonts.Body;
        }

        private void DrawCourseItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _courseList.Items.Count)
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
                _courseList.Items[e.Index]?.ToString() ?? string.Empty,
                _courseList.Font,
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
            _courseList.Items.Clear();
            _messageList.ClearMessages();

            if (_teacherId <= 0)
            {
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
                _hasLoadedInitialMessages = false;
                _messageList.ClearMessages();
                return;
            }

            _courseList.SelectedIndex = 0;
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
            _hasLoadedInitialMessages = false;
            _messageList.ClearMessages();
            await RefreshMessagesAsync();
        }

        private async Task RefreshMessagesAsync()
        {
            if (!_uiAlive || IsDisposed || _isLoadingMessages || _selectedCourseId <= 0)
                return;

            int courseId = _selectedCourseId;
            try
            {
                _isLoadingMessages = true;
                List<ChatMessageModel> messages;
                int newestMessageId = _messageList.NewestMessageId ?? 0;
                if (!_hasLoadedInitialMessages || newestMessageId <= 0)
                {
                    messages = await _chat.GetMessagesAsync(_teacherId, courseId, 20);
                }
                else
                {
                    messages = await _chat.GetMessagesAfterAsync(_teacherId, courseId, newestMessageId, 50);
                }

                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                    return;

                if (!_hasLoadedInitialMessages)
                {
                    _messageList.SetMessages(messages, _teacherId);
                    _hasLoadedInitialMessages = true;
                }
                else
                {
                    _messageList.AppendMessages(messages, _teacherId);
                }
            }
            finally
            {
                _isLoadingMessages = false;
            }
        }

        private async Task LoadOlderMessagesAsync()
        {
            if (!_uiAlive || IsDisposed || _isLoadingOlderMessages || _selectedCourseId <= 0)
                return;

            int? oldestMessageId = _messageList.OldestMessageId;
            if (!oldestMessageId.HasValue || oldestMessageId.Value <= 0)
                return;

            int courseId = _selectedCourseId;
            try
            {
                _isLoadingOlderMessages = true;
                List<ChatMessageModel> olderMessages = await _chat.GetMessagesBeforeAsync(_teacherId, courseId, oldestMessageId.Value, 20);
                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                    return;

                _messageList.PrependOlderMessages(olderMessages, _teacherId);
            }
            finally
            {
                _isLoadingOlderMessages = false;
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

            int courseId = _selectedCourseId;
            int temporaryMessageId = _nextTemporaryMessageId--;
            var temporaryMessage = new ChatMessageModel
            {
                Id = temporaryMessageId,
                CourseId = courseId,
                SenderId = _teacherId,
                SenderName = "Bạn",
                SenderRole = "Teacher",
                Content = content,
                MessageType = "TEXT",
                SentAt = DateTime.Now,
                DeliveryStatus = "PENDING"
            };

            _input.Clear();
            _messageList.AppendTemporaryMessage(temporaryMessage, _teacherId);

            try
            {
                ChatMessageModel? sentMessage = await _chat.SendMessageAsync(_teacherId, courseId, content);
                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                {
                    return;
                }

                if (sentMessage == null)
                {
                    _messageList.MarkMessageFailed(temporaryMessageId, _chat.LastErrorMessage);
                    return;
                }

                sentMessage.DeliveryStatus = "SENT";
                _messageList.ReplaceMessage(temporaryMessageId, sentMessage, _teacherId);
            }
            catch (Exception ex)
            {
                if (_uiAlive && !IsDisposed && !_messageList.IsDisposed)
                {
                    _messageList.MarkMessageFailed(temporaryMessageId, ex.Message);
                }
            }
        }

        private void OnChatDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                e.Effect = files.Length > 0 && ChatImageHelper.IsSupportedImage(files[0]) ? DragDropEffects.Copy : DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.None;
        }

        private async Task OnChatDragDropAsync(DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) != true)
            {
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (files.Length == 0)
            {
                return;
            }

            await SendImageAsync(files[0]);
        }

        private async Task PickAndSendImageAsync()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Chọn ảnh để gửi",
                Filter = "Ảnh JPG/PNG|*.jpg;*.jpeg;*.png",
                Multiselect = false
            };

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
                return;

            await SendImageAsync(dialog.FileName);
        }

        private async Task SendImageAsync(string imagePath)
        {
            if (_isSending)
                return;

            if (_teacherId <= 0 || _selectedCourseId <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn phòng chat hợp lệ.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ChatImageHelper.ValidateImageForUpload(imagePath);
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog(ex.Message, "Ảnh không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fileName = Path.GetFileName(imagePath);
            string caption = string.IsNullOrWhiteSpace(_input.Text) ? string.Empty : _input.Text.Trim();
            int courseId = _selectedCourseId;
            int temporaryMessageId = _nextTemporaryMessageId--;
            var temporaryMessage = new ChatMessageModel
            {
                Id = temporaryMessageId,
                CourseId = courseId,
                SenderId = _teacherId,
                SenderName = "Bạn",
                SenderRole = "Teacher",
                Content = caption,
                MessageType = "FILE",
                FileUrl = imagePath,
                FileName = fileName,
                FileSize = new FileInfo(imagePath).Length,
                MimeType = ChatImageHelper.GetMimeType(imagePath),
                SentAt = DateTime.Now,
                DeliveryStatus = "PENDING"
            };

            _input.Clear();
            _messageList.AppendTemporaryMessage(temporaryMessage, _teacherId);
            SetComposerSending(true, "Đang xử lý...");
            try
            {
                using CompressedChatImageResult compressed = await ChatImageHelper.CompressImageAsync(imagePath);
                SetComposerSending(true, "Đang tải ảnh...");
                ChatMessageModel? sentMessage = await _chat.SendImageMessageAsync(_teacherId, courseId, compressed.FilePath, compressed.OriginalFileName, caption);
                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                {
                    return;
                }

                if (sentMessage == null)
                {
                    _messageList.MarkMessageFailed(temporaryMessageId, _chat.LastErrorMessage);
                    return;
                }

                sentMessage.DeliveryStatus = "SENT";
                _messageList.ReplaceMessage(temporaryMessageId, sentMessage, _teacherId);
            }
            finally
            {
                SetComposerSending(false);
            }
        }

        private async Task CreatePollAsync()
        {
            if (_isSending)
                return;

            if (_teacherId <= 0 || _selectedCourseId <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn phòng chat hợp lệ trước khi tạo vote.", "Vote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new CreatePollDialog();
            if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            int courseId = _selectedCourseId;
            SetComposerSending(true);
            try
            {
                int messageId = await _chat.CreatePollAsync(_teacherId, courseId, dialog.PollQuestion, dialog.PollOptions);
                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                {
                    return;
                }

                if (messageId <= 0)
                {
                    MetaTheme.ShowModernDialog(_chat.LastErrorMessage, "Tạo vote thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await RefreshMessagesAsync();
            }
            catch (Exception ex)
            {
                if (_uiAlive && !IsDisposed)
                {
                    MetaTheme.ShowModernDialog($"Không thể tạo vote lúc này.\n{ex.Message}", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                SetComposerSending(false);
            }
        }

        private async Task VotePollAsync(PollVoteEventArgs args)
        {
            if (_teacherId <= 0 || _selectedCourseId <= 0)
            {
                _messageList.SetPollActionPending(args.MessageId, false);
                MetaTheme.ShowModernDialog("Vui lòng chọn phòng chat hợp lệ.", "Vote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int courseId = _selectedCourseId;
            try
            {
                bool success = await _chat.VotePollAsync(_teacherId, args.PollId, args.OptionId);
                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                {
                    return;
                }

                if (!success)
                {
                    MetaTheme.ShowModernDialog(_chat.LastErrorMessage, "Vote thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await RefreshSinglePollBubbleAsync(args.MessageId, _teacherId, courseId);
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

        private async Task ClosePollAsync(PollCloseEventArgs args)
        {
            if (_teacherId <= 0 || _selectedCourseId <= 0)
            {
                _messageList.SetPollActionPending(args.MessageId, false);
                MetaTheme.ShowModernDialog("Vui lòng chọn phòng chat hợp lệ.", "Vote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MetaTheme.ShowModernDialog(
                "Bạn chắc chắn muốn đóng vote này? Sau khi đóng, mọi người sẽ không thể vote thêm.",
                "Đóng vote",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                _messageList.SetPollActionPending(args.MessageId, false);
                return;
            }

            int courseId = _selectedCourseId;
            try
            {
                bool success = await _chat.ClosePollAsync(_teacherId, args.PollId);
                if (!_uiAlive || IsDisposed || _messageList.IsDisposed || _selectedCourseId != courseId)
                {
                    return;
                }

                if (!success)
                {
                    MetaTheme.ShowModernDialog(_chat.LastErrorMessage, "Đóng vote thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await RefreshSinglePollBubbleAsync(args.MessageId, _teacherId, courseId);
            }
            catch (Exception ex)
            {
                if (_uiAlive && !IsDisposed)
                {
                    MetaTheme.ShowModernDialog($"Không thể đóng vote lúc này.\n{ex.Message}", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            List<ChatMessageModel> latestMessages = await _chat.GetMessagesAsync(userId, courseId, 50);
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
                return;

            MetaTheme.ShowModernDialog(errorMessage ?? "Gửi tin nhắn thất bại.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void SetComposerSending(bool sending, string? statusText = null)
        {
            _isSending = sending;
            _send.Enabled = !sending;
            _attach.Enabled = !sending;
            _createPoll.Enabled = !sending;
            _input.Enabled = !sending;
            _attach.Text = sending ? (statusText ?? "Đang gửi...") : "Ảnh";
        }


    }
}
