using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Application.Services;

namespace CourseGuard.Presentation.UserControls.Teacher
{
    public partial class UC_TeacherNotifications : UserControl
    {
        private NotificationApiService _apiService;
        private List<NotificationModel> _data;
        private FlowLayoutPanel bodyPanel;
        private Panel headerPanel;
        private string currentFilter = "All";

        public UC_TeacherNotifications()
        {
            InitializeComponent();
            _apiService = new NotificationApiService();
            InitializeLayout();
            
            // Hàm tải dữ liệu bất đồng bộ sẽ được gọi khi UserControl chuẩn bị hiển thị (Load)
            this.Load += async (s, e) => { await LoadDataAsync(); };
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ColorTranslator.FromHtml("#F3F4F6");

            // --- HEADER PANEL ---
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = ColorTranslator.FromHtml("#FFFFFF"),
            };

            Button btnAll = new Button
            {
                Text = "Tất cả",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 30),
                Location = new Point(20, 10),
                BackColor = ColorTranslator.FromHtml("#E5E7EB"),
                Cursor = Cursors.Hand
            };
            btnAll.FlatAppearance.BorderSize = 0;
            btnAll.Click += (s, e) => { currentFilter = "All"; RenderNotifications(); };

            Button btnUnread = new Button
            {
                Text = "Chưa đọc",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 30),
                Location = new Point(110, 10),
                BackColor = ColorTranslator.FromHtml("#FFFFFF"),
                Cursor = Cursors.Hand
            };
            btnUnread.FlatAppearance.BorderSize = 1;
            btnUnread.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#D1D5DB");
            btnUnread.Click += (s, e) => { currentFilter = "Unread"; RenderNotifications(); };


            LinkLabel lnkMarkAllRead = new LinkLabel
            {
                Text = "Đánh dấu tất cả đã đọc",
                AutoSize = true,
                Location = new Point(this.Width - 180, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                LinkColor = ColorTranslator.FromHtml("#3B82F6"),
                LinkBehavior = LinkBehavior.HoverUnderline,
                Cursor = Cursors.Hand
            };
            lnkMarkAllRead.Click += (s, e) =>
            {
                if (_data != null)
                {
                    foreach (var item in _data) item.IsRead = true;
                    RenderNotifications();
                }
            };

            headerPanel.Controls.Add(btnAll);
            headerPanel.Controls.Add(btnUnread);
            headerPanel.Controls.Add(lnkMarkAllRead);

            // Chống tràn màn hình mỏ neo
            headerPanel.Resize += (s, e) => 
            { 
                lnkMarkAllRead.Left = headerPanel.Width - lnkMarkAllRead.Width - 20; 
            };

            // --- BODY PANEL ---
            bodyPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20)
            };

            bodyPanel.Resize += (s, e) => 
            {
                foreach (Control c in bodyPanel.Controls) { c.Width = bodyPanel.ClientSize.Width - 40; }
            };

            this.Controls.Add(bodyPanel);
            this.Controls.Add(headerPanel);
        }

        // ============================================
        // HÀM GỌI API KIỂU BẤT ĐỒNG BỘ
        // ============================================
        private async Task LoadDataAsync()
        {
            bodyPanel.Controls.Clear();

            // === 1. BƯỚC LOADING ===
            Label lblLoading = new Label
            {
                Text = "Đang kết nối đến máy chủ API... Vui lòng đợi",
                Font = new Font("Segoe UI", 12F, FontStyle.Italic),
                ForeColor = ColorTranslator.FromHtml("#6B7280"),
                AutoSize = true
            };
            
            Panel centerPanel = new Panel { Width = bodyPanel.ClientSize.Width - 40, Height = 200 };
            lblLoading.Left = (centerPanel.Width - 300) / 2;
            lblLoading.Top = 80;
            lblLoading.Anchor = AnchorStyles.None;
            centerPanel.Controls.Add(lblLoading);
            
            centerPanel.Resize += (s, e) => { lblLoading.Left = (centerPanel.Width - lblLoading.Width) / 2; };
            bodyPanel.Controls.Add(centerPanel);

            try
            {
                // Hàm API chạy lấy kết quả thực
                _data = await _apiService.GetNotificationsAsync(2);
                
                // === 2. BƯỚC SUCCESS ===
                RenderNotifications();
            }
            catch (Exception ex)
            {
                // === 3. BƯỚC ERROR ===
                bodyPanel.Controls.Clear();

                Panel errorContainer = new Panel
                {
                    Width = bodyPanel.ClientSize.Width > 40 ? bodyPanel.ClientSize.Width - 40 : 600,
                    Height = 150,
                    BackColor = ColorTranslator.FromHtml("#FEF2F2"),
                    Margin = new Padding(0, 20, 0, 0)
                };

                Panel borderError = new Panel
                {
                    Dock = DockStyle.Left,
                    Width = 4,
                    BackColor = ColorTranslator.FromHtml("#EF4444")
                };
                errorContainer.Controls.Add(borderError);

                Label lblErrorTitle = new Label
                {
                    Text = "Đã xảy ra lỗi kết nối!",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = ColorTranslator.FromHtml("#EF4444"),
                    AutoSize = true,
                    Location = new Point(20, 20)
                };

                Label lblErrorDetails = new Label
                {
                    Text = ex.Message,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = ColorTranslator.FromHtml("#B91C1C"),
                    AutoSize = true,
                    Location = new Point(20, 50)
                };

                Button btnRetry = new Button
                {
                    Text = "Thử lại",
                    BackColor = ColorTranslator.FromHtml("#EF4444"),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(100, 35),
                    Location = new Point(20, 90),
                    Cursor = Cursors.Hand
                };
                btnRetry.FlatAppearance.BorderSize = 0;
                btnRetry.Click += async (s, e) => { await LoadDataAsync(); }; // Gọi tải lại dữ liệu

                errorContainer.Controls.Add(lblErrorTitle);
                errorContainer.Controls.Add(lblErrorDetails);
                errorContainer.Controls.Add(btnRetry);

                bodyPanel.Controls.Add(errorContainer);
            }
        }

        private void RenderNotifications()
        {
            if (_data == null) return;

            bodyPanel.Controls.Clear();

            var filteredData = _data;
            if (currentFilter == "Unread")
            {
                filteredData = _data.Where(x => !x.IsRead).ToList();
            }

            foreach (var item in filteredData)
            {
                Panel notificationPanel = CreateNotificationPanel(item);
                bodyPanel.Controls.Add(notificationPanel);
            }
        }

        private Panel CreateNotificationPanel(NotificationModel item)
        {
            Panel pnl = new Panel
            {
                Width = bodyPanel.ClientSize.Width > 40 ? bodyPanel.ClientSize.Width - 40 : 600,
                Height = string.IsNullOrEmpty(item.ActionText) ? 80 : 120,
                Margin = new Padding(0, 0, 0, 15),
                Cursor = Cursors.Hand
            };

            // Tô màu nền theo thẻ chưa đọc hoặc đã đọc 
            if (!item.IsRead)
            {
                pnl.BackColor = item.Type == NotificationType.Alert ? ColorTranslator.FromHtml("#FEF2F2") : ColorTranslator.FromHtml("#EFF6FF");
                
                Panel borderLeft = new Panel
                {
                    Dock = DockStyle.Left,
                    Width = 4,
                    BackColor = item.Type == NotificationType.Alert ? ColorTranslator.FromHtml("#EF4444") : ColorTranslator.FromHtml("#3B82F6")
                };
                pnl.Controls.Add(borderLeft);

                Label dot = new Label
                {
                    Size = new Size(10, 10),
                    Location = new Point(pnl.Width - 25, 15),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = ColorTranslator.FromHtml("#3B82F6"),
                    Margin = new Padding(0)
                };
                System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.AddEllipse(0, 0, dot.Width, dot.Height);
                dot.Region = new Region(gp);
                pnl.Controls.Add(dot);

                pnl.Resize += (s, e) => { dot.Left = pnl.Width - 25; };
            }
            else
            {
                pnl.BackColor = ColorTranslator.FromHtml("#FFFFFF");
                
                Color defaultColor = pnl.BackColor;
                Color hoverColor = ColorTranslator.FromHtml("#F9FAFB");

                pnl.MouseEnter += (s, e) => { pnl.BackColor = hoverColor; };
                pnl.MouseLeave += (s, e) => { pnl.BackColor = defaultColor; };
                
                pnl.ControlAdded += (s, e) => 
                {
                    e.Control.MouseEnter += (s1, e1) => { pnl.BackColor = hoverColor; };
                    e.Control.MouseLeave += (s1, e1) => { pnl.BackColor = defaultColor; };
                };
            }

            // Tiêu đề
            Label lblTitle = new Label
            {
                Text = item.Title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = ColorTranslator.FromHtml("#111827")
            };

            // Nội dung
            Label lblContent = new Label
            {
                Text = item.Content,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Location = new Point(20, 40),
                AutoSize = true,
                MaximumSize = new Size(pnl.Width - 150, 0),
                ForeColor = ColorTranslator.FromHtml("#4B5563")
            };

            // Thời gian
            Label lblTime = new Label
            {
                Text = item.Time,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                Location = new Point(pnl.Width - 100, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                ForeColor = ColorTranslator.FromHtml("#9CA3AF")
            };
            pnl.Resize += (s, e) => 
            { 
                lblTime.Left = pnl.Width - lblTime.Width - 20; 
                lblContent.MaximumSize = new Size(pnl.Width - 150, 0);
            };

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblContent);
            pnl.Controls.Add(lblTime);

            // Nút nhấn tuỳ chọn
            if (!string.IsNullOrEmpty(item.ActionText))
            {
                Button btnAction = new Button
                {
                    Text = item.ActionText,
                    Location = new Point(20, 75),
                    AutoSize = true,
                    MinimumSize = new Size(100, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ColorTranslator.FromHtml("#F3F4F6"),
                    ForeColor = ColorTranslator.FromHtml("#374151"),
                    Cursor = Cursors.Hand
                };
                btnAction.FlatAppearance.BorderSize = 1;
                btnAction.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#D1D5DB");
                
                btnAction.Click += (s, e) => { MessageBox.Show("Thực hiện hành động: " + item.ActionText); };
                pnl.Controls.Add(btnAction);
            }

            // Gắn sự kiện để sửa hover khi bị ghi đè
            if (item.IsRead)
            {
                Color defaultColor = ColorTranslator.FromHtml("#FFFFFF");
                Color hoverColor = ColorTranslator.FromHtml("#F9FAFB");
                foreach (Control c in pnl.Controls)
                {
                    c.MouseEnter += (s, e) => { pnl.BackColor = hoverColor; };
                    c.MouseLeave += (s, e) => { pnl.BackColor = defaultColor; };
                }
            }

            EventHandler clickHandler = (s, e) =>
            {
                if (!item.IsRead)
                {
                    item.IsRead = true;
                    RenderNotifications();
                }
            };

            pnl.Click += clickHandler;
            lblTitle.Click += clickHandler;
            lblContent.Click += clickHandler;
            lblTime.Click += clickHandler;

            return pnl;
        }
    }
}
