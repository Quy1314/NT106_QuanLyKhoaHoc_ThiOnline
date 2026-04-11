/*
 * UC_Notification.cs
 *
 * Layer: Presentation (UserControls / Teacher)
 * Vai trò: Hiển thị và quản lý thông báo hệ thống — Master/Detail layout
 * 
 * Chức năng:
 *   - Tải dữ liệu thông báo từ Supabase (NotificationRepository)
 *   - SplitContainer chia màn hình: Danh sách (Master) - 40%, Chi tiết (Detail) - 60%
 *   - Master list card có trạng thái unread dot và hover color.
 *   - Chi tiết thông báo xem trong pnlDetail chứa RichTextBox.
 *
 * Phụ thuộc:
 *   - CourseGuard.Backend.Data.NotificationRepository  : kết nối Supabase
 *   - CourseGuard.Backend.Models.NotificationModel     : mô hình dữ liệu
 *   - CourseGuard.Frontend.Theme.ColorPalette          : bảng màu giao diện
 *   - CourseGuard.Frontend.Theme.RoundedButtonHelper   : bo góc nút chức năng
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_Notification : UserControl
    {
        // ── Biến toàn cục ────────────────────────────────────────────────────────

        /// <summary>Repository tương tác với CSDL Supabase.</summary>
        private readonly NotificationRepository _repo = new NotificationRepository();

        /// <summary>Danh sách nguồn chứa toàn bộ thông báo (chưa áp bộ lọc).</summary>
        private List<NotificationModel> _allNotifications = new List<NotificationModel>();

        /// <summary>Cờ trạng thái: true = đang lọc chỉ hiển thị thông báo chưa đọc.</summary>
        private bool _isFilteringUnread = false;

        /// <summary>ID người dùng hiện tại.</summary>
        private int _currentUserId = 1; 

        /// <summary>Lưu tham chiếu tới ID thông báo đang được chọn xem để highlight thẻ và thao tác</summary>
        private int _selectedNotificationId = -1;

        // ── Constructor ──────────────────────────────────────────────────────────

        public UC_Notification()
        {
            InitializeComponent();

            // Khởi tạo danh sách
            _allNotifications = new List<NotificationModel>();

            // Đăng ký sự kiện
            BindEvents();

            // Áp dụng giao diện theo ColorPalette và Theme Project
            FormatUI();

            // Cấu hình ban đầu cho form chi tiết
            ShowDetailEmptyState();

            // Tải dữ liệu từ Supabase ngay khi control xuất hiện
            this.Load += async (s, e) =>
            {
                await LoadDataFromSupabaseAsync();
            };
        }

        // ── Đăng ký sự kiện ─────────────────────────────────────────────

        private void BindEvents()
        {
            txtSearch.TextChanged += TxtSearch_TextChanged;
            btnFilterUnread.Click += BtnFilterUnread_Click;

            // Xử lý nút chức năng chi tiết
            btnMarkAsReadDetail.Click += async (s, e) => await BtnMarkAsReadDetail_ClickAsync();
            btnDeleteDetail.Click += async (s, e) => await BtnDeleteDetail_ClickAsync();
        }

        // ── Định dạng giao diện ──────────────────────────────────────────

        private void FormatUI()
        {
            this.BackColor = ColorPalette.LightMode.Base;

            // ── Định dạng Header Panel ──────────────────────────────────
            pnlHeader.BackColor = ColorPalette.LightMode.Secondary;

            lblTitle.ForeColor = ColorPalette.LightMode.Accent;
            lblTitle.Font = new Font("Segoe UI", lblTitle.Font.Size, FontStyle.Bold);
            lblTitle.Text = lblTitle.Text.ToUpper();

            lblSubtitle.ForeColor = ColorPalette.LightMode.TextSecondary;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // TextBox tìm kiếm
            txtSearch.BackColor = ColorPalette.LightMode.Secondary;
            txtSearch.ForeColor = ColorPalette.LightMode.TextPrimary;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 9.5F);
            txtSearch.Text = "🔍 Tìm theo tiêu đề hoặc nội dung...";

            // Sự kiện placeholder giả cho txtSearch
            txtSearch.Enter += (s, e) => { if (txtSearch.Text.Contains("🔍")) txtSearch.Text = ""; };
            txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) txtSearch.Text = "🔍 Tìm theo tiêu đề hoặc nội dung..."; };

            // Nút lọc
            StyleSecondaryButton(btnFilterUnread);
            btnFilterUnread.Text = "⚑ Chưa đọc";

            // ── SplitContainer UI ───────────────────────────────────
            splitContainer.BackColor = ColorPalette.LightMode.Border; // Màu viền ngăn cách
            splitContainer.Panel1.BackColor = ColorPalette.LightMode.Base;
            splitContainer.Panel2.BackColor = ColorPalette.LightMode.Base;

            flpNotifications.BackColor = ColorPalette.LightMode.Base;

            // ── Detail Panel UI ─────────────────────────────────────
            pnlDetail.BackColor = ColorPalette.LightMode.Secondary;
            lblEmptyDetail.ForeColor = ColorPalette.LightMode.TextSecondary;
            lblEmptyDetail.BackColor = ColorPalette.LightMode.Base;

            lblDetailTitle.ForeColor = ColorPalette.LightMode.TextPrimary;
            lblDetailSender.ForeColor = ColorPalette.LightMode.Accent;
            lblDetailDate.ForeColor = ColorPalette.LightMode.TextSecondary;

            rtbDetailBody.BackColor = ColorPalette.LightMode.Secondary;
            rtbDetailBody.ForeColor = ColorPalette.LightMode.TextPrimary;

            // Bo góc cho hai nút chức năng trong pnlDetail
            btnMarkAsReadDetail.BackColor = ColorPalette.LightMode.Accent;
            btnMarkAsReadDetail.ForeColor = Color.White;
            RoundedButtonHelper.Apply(btnMarkAsReadDetail, 15);

            btnDeleteDetail.BackColor = ColorPalette.Status.ErrorLight;
            btnDeleteDetail.ForeColor = Color.White;
            RoundedButtonHelper.Apply(btnDeleteDetail, 15);

            // ── Status Bar ───────────────────────────────────────────
            pnlStatus.BackColor = ColorPalette.LightMode.Secondary;
            lblStatus.ForeColor = ColorPalette.LightMode.TextSecondary;
        }

        private void StyleSecondaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = ColorPalette.LightMode.Secondary;
            btn.ForeColor = ColorPalette.LightMode.TextPrimary;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = ColorPalette.LightMode.Border;
            btn.FlatAppearance.MouseOverBackColor = ColorPalette.LightMode.Base;
            btn.Cursor = Cursors.Hand;
        }

        // ── Tìm kiếm thời gian thực & Áp dụng lọc ──────────────────────────────

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text.Contains("🔍")) return; // Skip giả định placeholder
            ApplyFilters();
        }

        private void BtnFilterUnread_Click(object sender, EventArgs e)
        {
            _isFilteringUnread = !_isFilteringUnread;

            if (_isFilteringUnread)
            {
                btnFilterUnread.Text = "✕ Bỏ lọc";
                btnFilterUnread.BackColor = ColorPalette.Status.InfoLight;
                btnFilterUnread.ForeColor = Color.White;
            }
            else
            {
                btnFilterUnread.Text = "⚑ Chưa đọc";
                btnFilterUnread.BackColor = ColorPalette.LightMode.Secondary;
                btnFilterUnread.ForeColor = ColorPalette.LightMode.TextPrimary;
            }

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IEnumerable<NotificationModel> result = _allNotifications;

            string keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(keyword) && !keyword.Contains("🔍"))
            {
                result = result.Where(n =>
                    n.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.Content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (_isFilteringUnread)
            {
                result = result.Where(n => !n.IsRead);
            }

            RefreshGrid(result.ToList());
        }

        // ── Xử lý Master List Layout ────────────────────────────────────

        private void RefreshGrid(List<NotificationModel> notifications)
        {
            flpNotifications.SuspendLayout();
            flpNotifications.Controls.Clear();

            if (notifications.Count == 0)
            {
                Label lblEmpty = new Label
                {
                    Text = "Không có thông báo nào",
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = ColorPalette.LightMode.TextSecondary,
                    Font = new Font("Segoe UI", 10F, FontStyle.Italic)
                };
                flpNotifications.Controls.Add(lblEmpty);
            }
            else
            {
                foreach (var notification in notifications)
                {
                    Panel cardPanel = CreateNotificationCard(notification);
                    flpNotifications.Controls.Add(cardPanel);
                }
            }

            flpNotifications.ResumeLayout();
        }

        /// <summary>
        /// Tạo Master Card cho mỗi Notification, không chứa Button mà hỗ trợ click chọn.
        /// Sử dụng Unread dot thay cho text.
        /// </summary>
        private Panel CreateNotificationCard(NotificationModel notification)
        {
            int cardWidth = flpNotifications.Width - 25;
            bool isSelected = notification.Id == _selectedNotificationId;

            // Nền thẻ. Sáng hơn một chút nếu đang chọn.
            Panel card = new Panel
            {
                Width = cardWidth,
                Height = 85,
                BorderStyle = BorderStyle.None,
                BackColor = isSelected ? ColorPalette.LightMode.Border : ColorPalette.LightMode.Secondary, 
                Margin = new Padding(0, 0, 0, 8),
                Cursor = Cursors.Hand,
                Tag = notification
            };

            // Vẽ Unread Dot sử dụng event Paint của Panel "card"
            card.Paint += (sender, e) =>
            {
                if (!notification.IsRead)
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (SolidBrush dotBrush = new SolidBrush(ColorPalette.LightMode.Accent))
                    {
                        e.Graphics.FillEllipse(dotBrush, 12, 16, 10, 10);
                    }
                }
            };

            // Tiêu đề (In đậm nếu chưa đọc)
            Label lblTitle = new Label
            {
                Text = notification.Title,
                AutoSize = false,
                Width = card.Width - 40,
                Height = 25,
                Left = 32, // Chừa trống phần Dot
                Top = 10,
                Font = notification.IsRead ? new Font("Segoe UI", 10.5F) : new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ColorPalette.LightMode.TextPrimary
            };
            card.Controls.Add(lblTitle);

            // Nội dung thu gọn
            string shortContent = notification.Content.Replace("\r", " ").Replace("\n", " ");
            Label lblContent = new Label
            {
                Text = shortContent,
                AutoSize = false,
                AutoEllipsis = true,
                Width = card.Width - 40,
                Height = 20,
                Left = 32,
                Top = 38,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ColorPalette.LightMode.TextSecondary
            };
            card.Controls.Add(lblContent);

            // Thời gian tạo 
            Label lblTime = new Label
            {
                Text = FormatDateTime(notification.CreatedAt),
                AutoSize = true,
                Left = 32,
                Top = 60,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = ColorPalette.LightMode.TextSecondary
            };
            card.Controls.Add(lblTime);

            // Khai báo sự kiện click và hover
            Action onClickCard = () => SelectNotification(notification);

            // Attach event click and hover to card and ALL children controls
            AttachCardEvents(card, card, lblTitle, lblContent, lblTime);
            
            card.Click += (s, e) => onClickCard();
            lblTitle.Click += (s, e) => onClickCard();
            lblContent.Click += (s, e) => onClickCard();
            lblTime.Click += (s, e) => onClickCard();

            return card;
        }

        private void AttachCardEvents(Panel card, params Control[] elements)
        {
            foreach (var ctrl in elements)
            {
                ctrl.MouseEnter += (s, e) =>
                {
                    NotificationModel notif = (NotificationModel)card.Tag;
                    if (notif.Id != _selectedNotificationId)
                        card.BackColor = ColorPalette.LightMode.Base; // Hover color
                };
                ctrl.MouseLeave += (s, e) =>
                {
                    // Lấy vị trí chuột hiện tại để check bound của thẻ (do thẻ chứa các con)
                    Point clientPoint = card.PointToClient(Cursor.Position);
                    if (!card.ClientRectangle.Contains(clientPoint))
                    {
                        NotificationModel notif = (NotificationModel)card.Tag;
                        card.BackColor = notif.Id == _selectedNotificationId 
                            ? ColorPalette.LightMode.Border 
                            : ColorPalette.LightMode.Secondary;
                    }
                };
            }
        }

        // ── Xử lý Layout Chi Tiết (Detail) ──────────────────────────────

        private void ShowDetailEmptyState()
        {
            lblEmptyDetail.Visible = true;
            lblEmptyDetail.BringToFront();
            pnlDetail.Visible = false;
        }

        /// <summary>
        /// Được gọi khi click vào 1 item trên Master List. Load dữ liệu lên khung phải.
        /// </summary>
        private void SelectNotification(NotificationModel notification)
        {
            _selectedNotificationId = notification.Id;
            
            // Xoá màn trống và hiển thị pnlDetail
            lblEmptyDetail.Visible = false;
            pnlDetail.Visible = true;
            pnlDetail.BringToFront();

            // Đổ dữ liệu
            lblDetailTitle.Text = notification.Title;
            lblDetailSender.Text = "Hệ thống Admin"; // Logic mẫu Sender
            lblDetailDate.Text = "Nhận lúc: " + notification.CreatedAt.ToString("HH:mm - dd/MM/yyyy");
            rtbDetailBody.Text = notification.Content;

            // Xử lý hiển thị các nút thao tác
            btnMarkAsReadDetail.Visible = !notification.IsRead;

            // Load lại lưới (để tô sáng thẻ vừa chọn)
            ApplyFilters(); 
        }

        // ── Khối Actions Khung Chi Tiết ─────────────────────────────────

        private async System.Threading.Tasks.Task BtnMarkAsReadDetail_ClickAsync()
        {
            NotificationModel notification = _allNotifications.FirstOrDefault(n => n.Id == _selectedNotificationId);
            if (notification == null) return;

            try
            {
                bool success = await System.Threading.Tasks.Task.Run(() => _repo.MarkAsRead(notification.Id));
                if (success)
                {
                    notification.IsRead = true;
                    btnMarkAsReadDetail.Visible = false; // Ẩn nút đi
                    ApplyFilters(); // Load lại trái (xóa unread dot)
                    SetStatus("✅  Đã đánh dấu thông báo là đã đọc.", ColorPalette.Status.SuccessLight);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"❌  Lỗi: {ex.Message}", ColorPalette.Status.ErrorLight);
            }
        }

        private async System.Threading.Tasks.Task BtnDeleteDetail_ClickAsync()
        {
            NotificationModel notification = _allNotifications.FirstOrDefault(n => n.Id == _selectedNotificationId);
            if (notification == null) return;

            var confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa thông báo này?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                bool success = await System.Threading.Tasks.Task.Run(() => _repo.Delete(notification.Id));
                if (success)
                {
                    _allNotifications.Remove(notification);
                    _selectedNotificationId = -1; // Xóa trạng thái đang focus
                    ShowDetailEmptyState();       // Trở lại trạng thái 빈
                    ApplyFilters(); 
                    SetStatus("✅  Thông báo đã được xóa.", ColorPalette.Status.SuccessLight);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"❌  Lỗi xóa thông báo: {ex.Message}", ColorPalette.Status.ErrorLight);
            }
        }

        // ── Tải dữ liệu từ Supabase ──────────────────────────────────────

        private async System.Threading.Tasks.Task LoadDataFromSupabaseAsync()
        {
            SetStatus("⏳  Đang kết nối Supabase và tải dữ liệu...", ColorPalette.LightMode.TextSecondary);
            SetControlsEnabled(false);

            try
            {
                _allNotifications = await System.Threading.Tasks.Task.Run(() => _repo.LoadByUserId(_currentUserId));

                ApplyFilters(); // Đổ dữ liệu lên Grid

                SetStatus($"✅  Đã tải {_allNotifications.Count} thông báo.", ColorPalette.Status.SuccessLight);
            }
            catch (Exception ex)
            {
                SetStatus($"❌  Lỗi kết nối DB: {ex.Message}", ColorPalette.Status.ErrorLight);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        // ── Helper Methods ────────────────────────────────────────────────

        private string FormatDateTime(DateTime createdAt)
        {
            TimeSpan diff = DateTime.Now - createdAt;

            if (diff.TotalHours < 1)
                return $"Vừa xong";
            else if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h trước";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} ngày trước";
            else
                return createdAt.ToString("dd/MM/yyyy");
        }

        private void SetStatus(string message, Color color)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => SetStatus(message, color)));
                return;
            }
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (btnFilterUnread.InvokeRequired)
            {
                btnFilterUnread.Invoke(new Action(() => SetControlsEnabled(enabled)));
                return;
            }
            btnFilterUnread.Enabled = enabled;
            txtSearch.Enabled = enabled;
        }
    }
}
