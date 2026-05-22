/*
 * TeacherDashboard.cs
 * 
 * Layer: Presentation (Forms)
 * Layout: Sidebar(Left) → RightPanel(Fill) → Topbar(Top) + pnlMainboard(Fill)
 *
 * UI/UX Rules applied:
 *   Rule 01 (Layout Skeleton) — Sidebar fixed left, Header top, Content fills rest
 *   Rule 09 — No pure black/white
 *   Rule 40 — Single accent color in navigation
 *   Rule 46 — Short navigation labels (1-2 words max)
 *   Rule 05 — Visual hierarchy
 *   Rule 06 — Red for destructive actions (logout)
 *   Rule 15 — Actionable button text
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class TeacherDashboard : Form
    {
        private Dictionary<string, Func<UserControl>> _nav = new();
        private UserControl? activeUserControl;
        private int _currentTeacherId = 0;
        private string _currentTeacherUsername = string.Empty;
        private SidebarPanel _sidebar;
        private TopbarPanel _topbar;
        private Panel _rightPanel;   // container: topbar + pnlMainboard

        public TeacherDashboard()
        {
            InitializeComponent();
            SearchFocusManager.Install(this);

            // ── 1. Build layout skeleton (Skill 01) ──────────────────
            // Sidebar docks Left on Form
            _sidebar = new SidebarPanel { Dock = DockStyle.Left };
            _sidebar.SetNavItems(
                // Rule 46: Short labels (1-2 words max)
                new[] { "Tổng quan", "Khóa học", "Kiểm tra", "Giám sát", "Thông báo" },
                new[] { "🏠", "📚", "✍", "👁", "🔔" }
            );
            _sidebar.NavItemClicked += Sidebar_NavItemClicked;

            // Right container holds Topbar(Top) + pnlMainboard(Fill)
            _rightPanel = new Panel { Dock = DockStyle.Fill, Padding = Padding.Empty };
            _topbar = new TopbarPanel
            {
                Dock = DockStyle.Top,
                PageTitle = "Tổng quan",
                Subtitle = "Giáo viên"
            };

            // Re-parent btnMail onto TopbarPanel
            btnMail.Parent = _topbar;
            btnMail.Location = new Point(_topbar.Width - 60, 15);
            btnMail.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // pnlMainboard (from Designer) fills remaining space under topbar
            pnlMainboard.Dock = DockStyle.Fill;

            // Assemble: pnlMainboard Fill first, then topbar Top docks above
            _rightPanel.Controls.Add(pnlMainboard);
            _rightPanel.Controls.Add(_topbar);

            // Sidebar on the left, right panel fills the rest
            this.Controls.Add(_rightPanel);
            this.Controls.Add(_sidebar);

            // Re-parent submenu panels as children of _sidebar
            pnlSubMenuCourseDocs.Parent = _sidebar;
            pnlSubMenuTesting.Parent = _sidebar;
            pnlSubMenuMonitoring.Parent = _sidebar;

            // ── 2. Apply theme (Rule 09: no pure white/black) ────────
            BackColor = AppColors.BgBase;
            _rightPanel.BackColor = AppColors.BgBase;
            pnlMainboard.BackColor = AppColors.BgBase;
            ForeColor = AppColors.TextPrimary;

            // Submenu panels
            pnlSubMenuCourseDocs.BackColor = AppColors.BgSidebar;
            pnlSubMenuTesting.BackColor = AppColors.BgSidebar;
            pnlSubMenuMonitoring.BackColor = AppColors.BgSidebar;

            // Style submenu child buttons
            StyleSubmenuButtons(pnlSubMenuCourseDocs);
            StyleSubmenuButtons(pnlSubMenuTesting);
            StyleSubmenuButtons(pnlSubMenuMonitoring);

            lblEmailHeader.ForeColor = AppColors.AccentBlue;
            btnMail.ForeColor = AppColors.TextSecondary;

            // ── 3. Navigation logic ──────────────────────────────────
            HideAllSubMenus();
            InitializeEmailDropdown();
            InitializeNavigation();

            pnlEmailDropdown.BringToFront();
        }

        public TeacherDashboard(UserModel user) : this()
        {
            _currentTeacherId = user?.Id ?? 0;
            _currentTeacherUsername = user?.Username ?? string.Empty;
            _topbar.PageTitle = "Tổng quan";
            _topbar.Subtitle = "Chào mừng, " + _currentTeacherUsername;
            _topbar.UserName = _currentTeacherUsername;
        }

        // ═══════════════ NAVIGATION ═══════════════

        private void InitializeNavigation()
        {
            _nav = new Dictionary<string, Func<UserControl>>
            {
                // Sub-items under "Khóa học" group
                { "Phân công lớp",        () => CreateEmptyView("Quản lý phân công lớp") },
                { "Lớp trực tuyến",       () => new UC_Chat() },
                // Sub-items under "Kiểm tra" group
                { "Ngân hàng câu hỏi",    () => CreateEmptyView("Ngân hàng câu hỏi") },
                { "Cấu hình đề thi",      () => CreateEmptyView("Cấu hình đề thi") },
                { "Quản lý kỳ thi",       () => new UC_ExamManagement() },
                // Sub-items under "Giám sát" group
                { "Giám sát Live",         () => CreateEmptyView("Giám sát Live") },
                { "Chấm tự luận",          () => CreateEmptyView("Chấm tự luận") },
                { "Quản lý điểm",          () => new UC_ScoreManagement() },
                // Direct nav items
                { "Thông báo",             () => new UC_Notification(_currentTeacherId) },
            };
        }

        private void Sidebar_NavItemClicked(object? sender, string pageName)
        {
            if (pageName == "Logout")
            {
                HandleLogout();
                return;
            }

            _topbar.PageTitle = pageName;

            // Items with submenus — toggle instead of loading a UC
            switch (pageName)
            {
                case "Khóa học":
                    ShowSubMenu(pnlSubMenuCourseDocs);
                    return;
                case "Kiểm tra":
                    ShowSubMenu(pnlSubMenuTesting);
                    return;
                case "Giám sát":
                    ShowSubMenu(pnlSubMenuMonitoring);
                    return;
            }

            // Normal nav items
            HideAllSubMenus();

            if (pageName == "Tổng quan")
            {
                // Clear mainboard for overview (no specific UC yet)
                if (activeUserControl != null)
                {
                    pnlMainboard.Controls.Remove(activeUserControl);
                    activeUserControl.Dispose();
                    activeUserControl = null;
                }
            }
            else if (pageName == "Thông báo")
            {
                LoadUserControl(new UC_Notification(_currentTeacherId));
            }
        }

        // ═══════════════ SUB-MENU BUTTON CLICKS ═══════════════

        private void btnAssignedCourses_Click(object sender, EventArgs e)
        {
            _topbar.PageTitle = "Phân công lớp";
            if (_nav.ContainsKey("Phân công lớp"))
                LoadUserControl(_nav["Phân công lớp"]());
        }

        private void btnOnlineClasses_Click(object sender, EventArgs e)
        {
            _topbar.PageTitle = "Lớp trực tuyến";
            LoadUserControl(new UC_Chat());
        }

        private void btnQuestionBank_Click(object sender, EventArgs e)
        {
            _topbar.PageTitle = "Ngân hàng câu hỏi";
            if (_nav.ContainsKey("Ngân hàng câu hỏi"))
                LoadUserControl(_nav["Ngân hàng câu hỏi"]());
        }

        private void btnExamConfig_Click(object sender, EventArgs e)
        {
            _topbar.PageTitle = "Cấu hình đề thi";
            if (_nav.ContainsKey("Cấu hình đề thi"))
                LoadUserControl(_nav["Cấu hình đề thi"]());
        }

        private void btnExamList_Click(object sender, EventArgs e)
        {
            _topbar.PageTitle = "Quản lý kỳ thi";
            LoadUserControl(new UC_ExamManagement());
        }

        private void btnLiveMonitor_Click(object sender, EventArgs e)
        {
            _topbar.PageTitle = "Giám sát Live";
            if (_nav.ContainsKey("Giám sát Live"))
                LoadUserControl(_nav["Giám sát Live"]());
        }

        private void btnEssayGrading_Click(object sender, EventArgs e)
        {
            _topbar.PageTitle = "Chấm tự luận";
            if (_nav.ContainsKey("Chấm tự luận"))
                LoadUserControl(_nav["Chấm tự luận"]());
        }

        private void btnScoreManagement_Click(object sender, EventArgs e)
        {
            _topbar.PageTitle = "Quản lý điểm";
            LoadUserControl(new UC_ScoreManagement());
        }

        // ═══════════════ UI LOADING ═══════════════

        public void LoadUserControl(UserControl uc)
        {
            if (activeUserControl != null)
            {
                pnlMainboard.Controls.Remove(activeUserControl);
                activeUserControl.Dispose();
            }

            activeUserControl = uc;
            uc.Dock = DockStyle.Fill;
            uc.BackColor = AppColors.BgBase;
            pnlMainboard.Controls.Add(uc);
            uc.BringToFront();

            AppColors.ApplyTheme(uc);
        }

        // ═══════════════ HELPERS ═══════════════

        private void HideAllSubMenus()
        {
            pnlSubMenuCourseDocs.Visible = false;
            pnlSubMenuTesting.Visible = false;
            pnlSubMenuMonitoring.Visible = false;
        }

        private void ShowSubMenu(Panel subMenu)
        {
            if (!subMenu.Visible) { HideAllSubMenus(); subMenu.Visible = true; }
            else { subMenu.Visible = false; }
        }

        private static void StyleSubmenuButtons(Panel submenu)
        {
            foreach (Control c in submenu.Controls)
            {
                if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = AppColors.TextSecondary;
                    btn.Font = AppFonts.Body;
                    btn.Cursor = Cursors.Hand;
                    btn.FlatAppearance.MouseOverBackColor = AppColors.BgCardHover;
                }
            }
        }

        private static UserControl CreateEmptyView(string title)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.BgBase };
            var label = new Label
            {
                Dock = DockStyle.Fill,
                Text = $"{title}\n(Đang phát triển)",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = AppFonts.Title,
                ForeColor = AppColors.TextMuted
            };
            panel.Controls.Add(label);
            var uc = new UserControl { Dock = DockStyle.Fill, BackColor = AppColors.BgBase };
            uc.Controls.Add(panel);
            return uc;
        }

        // ═══════════════ LOGOUT (Rule 06: red for destructive) ═══════════════

        private void HandleLogout()
        {
            // Rule 15: Actionable button text in confirmation
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống không?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var authService = new CourseGuard.Backend.Controllers.AuthController(
                    new CourseGuard.Backend.Data.CourseGuardDbContext(""));
                string ipAddress = GetLocalIpAddress();
                authService.Logout(_currentTeacherId, _currentTeacherUsername, ipAddress);
                this.Close();
            }
        }

        // ═══════════════ MAIL DROPDOWN ═══════════════

        private void InitializeEmailDropdown()
        {
            pnlEmailDropdown.BackColor = AppColors.BgCard;
            pnlEmailDropdown.Paint += (s, e) =>
            {
                if (s is not Panel pnl) return;
                // Rule 26: nested border radii — inner = outer - padding
                int radius = 16;
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(pnl.Width - radius - 1, 0, radius, radius, 270, 90);
                path.AddArc(pnl.Width - radius - 1, pnl.Height - radius - 1, radius, radius, 0, 90);
                path.AddArc(0, pnl.Height - radius - 1, radius, radius, 90, 90);
                path.CloseFigure();
                pnl.Region = new System.Drawing.Region(path);

                using (Pen pen = new Pen(AppColors.Border, 1.5f))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            };
            _ = LoadEmailsAsync();
        }

        private async System.Threading.Tasks.Task LoadEmailsAsync()
        {
            try
            {
                var gmailHelper = new CourseGuard.Backend.Services.GmailServiceHelper();
                var emails = await gmailHelper.GetLatestEmailsAsync(10);
                if (this.InvokeRequired)
                    this.Invoke(new Action(() => PopulateEmails(emails)));
                else
                    PopulateEmails(emails);
            }
            catch (Exception ex)
            {
                if (this.InvokeRequired)
                    this.Invoke(new Action(() => ShowEmailError(ex.Message)));
                else
                    ShowEmailError(ex.Message);
            }
        }

        private void PopulateEmails(System.Collections.Generic.List<CourseGuard.Backend.Services.EmailItem> emails)
        {
            flpEmails.SuspendLayout();
            flpEmails.Controls.Clear();
            foreach (var email in emails)
                flpEmails.Controls.Add(new UC_EmailCard(email.Sender, email.Subject, email.Snippet, email.Date));
            flpEmails.ResumeLayout();
        }

        // Rule 47: Constructive error message
        private void ShowEmailError(string errorMsg)
        {
            flpEmails.Controls.Clear();
            flpEmails.Controls.Add(new Label
            {
                Text = "Không thể tải email: " + errorMsg,
                AutoSize = true,
                ForeColor = AppColors.Danger,
                Padding = new Padding(10)
            });
        }

        private void btnMail_Click(object sender, EventArgs e)
        {
            pnlEmailDropdown.Visible = !pnlEmailDropdown.Visible;
            if (pnlEmailDropdown.Visible) pnlEmailDropdown.BringToFront();
        }

        private void btnMail_MouseEnter(object sender, EventArgs e) => btnMail.ForeColor = AppColors.AccentBlue;
        private void btnMail_MouseLeave(object sender, EventArgs e) => btnMail.ForeColor = AppColors.TextSecondary;

        // ═══════════════ UTILITIES ═══════════════

        private static string GetLocalIpAddress()
        {
            try
            {
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                            !System.Net.IPAddress.IsLoopback(ip.Address))
                            return ip.Address.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }
    }
}
