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
        private bool _uiAlive = true;

        public UC_Chat()
        {
            InitializeComponent();
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

            btnSend.Click += async (s, e) =>
            {
                string content = txtInput.Text.Trim();
                if (string.IsNullOrWhiteSpace(content))
                {
                    return;
                }

                int userId = UserSessionContext.CurrentUserId ?? 0;
                if (userId <= 0 || _selectedCourseId <= 0)
                {
                    MessageBox.Show("Không tìm thấy phòng chat hợp lệ.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool sent = await _chatController.SendMessageAsync(userId, _selectedCourseId, content);
                if (!sent)
                {
                    MessageBox.Show(_chatController.LastErrorMessage, "Gửi tin nhắn thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtInput.Clear();
                await RefreshMessagesAsync();
            };

            var sendMenu = new ContextMenuStrip();
            var attachItem = new ToolStripMenuItem("Gửi file...");
            attachItem.Click += async (_, _) => await SendFileAsync();
            sendMenu.Items.Add(attachItem);
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
                    _ = SendFileAsync();
                }
            };

            Load += async (_, _) =>
            {
                await LoadCoursesAsync();
                _pollTimer.Start();
            };
        }

        private async Task LoadCoursesAsync()
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
                lstContacts.SelectedIndex = 0;
            }
            else
            {
                txtMessages.Text = "Bạn chưa có khóa học nào để chat.";
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

            if (_isLoadingMessages || _selectedCourseId <= 0)
            {
                return;
            }

            try
            {
                _isLoadingMessages = true;
                int userId = UserSessionContext.CurrentUserId ?? 0;
                List<ChatMessageModel> messages = await _chatController.GetMessagesAsync(userId, _selectedCourseId, 200);

                // Có thể control đã bị đóng/đổi course trong lúc chờ DB xong.
                if (!_uiAlive || IsDisposed || txtMessages.IsDisposed)
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

        private async Task SendFileAsync()
        {
            int userId = UserSessionContext.CurrentUserId ?? 0;
            if (userId <= 0 || _selectedCourseId <= 0)
            {
                MessageBox.Show("Không tìm thấy phòng chat hợp lệ.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            string caption = $"Gửi file: {Path.GetFileName(dialog.FileName)}";
            bool sent = await _chatController.SendFileMessageAsync(userId, _selectedCourseId, dialog.FileName, caption);
            if (!sent)
            {
                MessageBox.Show(_chatController.LastErrorMessage, "Gửi file thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RefreshMessagesAsync();
        }
    }
}