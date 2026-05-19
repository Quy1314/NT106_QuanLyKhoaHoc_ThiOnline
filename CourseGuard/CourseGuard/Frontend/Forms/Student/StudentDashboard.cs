/*
 * StudentDashboard.cs
 * 
 * Layer: Presentation (Forms)
 * Layout: Sidebar(Left) → RightPanel(Fill) → Topbar(Top) + mainboard(Fill)
 * 
 * UI/UX Rules applied:
 *   Rule 01 (Layout Skeleton) — Sidebar fixed left, Header top, Content fills rest
 *   Rule 09 — No pure black/white
 *   Rule 40 — Single accent color in navigation
 *   Rule 46 — Short navigation labels (1-2 words max)
 *   Rule 05 — Visual hierarchy
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CourseGuard.Frontend.UserControls.Student;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public partial class StudentDashboard : Form
    {
        private Dictionary<string, Func<UserControl>> _nav = new();
        private CourseGuard.Backend.Models.UserModel currentUser;
        private SidebarPanel _sidebar;
        private TopbarPanel _topbar;
        private Panel _rightPanel;   // container: topbar + mainboard
        private string _currentPageName = "Tổng quan";

        public StudentDashboard(CourseGuard.Backend.Models.UserModel user)
        {
            currentUser = user;
            AppColors.IsDarkMode = false;
            InitializeComponent();
            SearchFocusManager.Install(this);

            // ── 1. Build layout skeleton (Skill 01) ──────────────────
            // Sidebar docks Left on Form
            _sidebar = new SidebarPanel { Dock = DockStyle.Left };
            _sidebar.SetNavItems(
                new[] { "Tổng quan", "Tìm khóa học", "Khóa học của tôi", "Bài kiểm tra", "Kết quả", "Tài liệu", "Lịch học", "Tin nhắn", "Thông báo", "Hồ sơ" },
                new[] { "home", "search", "folder-check", "clipboard-check", "chart", "document", "calendar", "message", "bell", "user" }
            );
            _sidebar.NavItemClicked += Sidebar_NavItemClicked;

            // Right container holds Topbar(Top) + mainboard(Fill)
            _rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 0) };
            _topbar = new TopbarPanel
            {
                Dock = DockStyle.Top,
                PageTitle = "Tổng quan",
                Subtitle = string.Empty,
                UserName = user.Username ?? "student",
                UseStudentTopbar = true,
                QuickSearchPlaceholder = "Tìm khóa học, bài kiểm tra, tài liệu...",
                OpenExamCount = 2,
                IsOnline = true,
                NotificationCount = 3
            };
            _topbar.ThemeToggled += (_, _) => ReloadCurrentPage();
            _topbar.LogoutRequested += (_, _) => LogoutCurrentUser();

            // mainboard (from Designer) fills remaining space under topbar
            mainboard.Dock = DockStyle.Fill;

            // Assemble: mainboard Fill first, then topbar Top docks above
            _rightPanel.Controls.Add(mainboard);
            _rightPanel.Controls.Add(_topbar);

            // Sidebar left, right panel fills rest
            this.Controls.Add(_rightPanel);
            this.Controls.Add(_sidebar);

            // ── 2. Apply theme (Rule 09: no pure white/black) ────────
            BackColor = AppColors.BgBase;
            _rightPanel.BackColor = AppColors.BgBase;
            mainboard.BackColor = AppColors.BgBase;

            // ── 3. Navigation logic ──────────────────────────────────
            InitializeNavigation();

            if (_nav.TryGetValue("Tổng quan", out var defaultFactory))
            {
                _currentPageName = "Tổng quan";
                LoadUI(defaultFactory());
            }
        }

        private void InitializeNavigation()
        {
            _nav = new Dictionary<string, Func<UserControl>>
            {
                { "Tổng quan",    () => new UC_StudentDashboard() },
                { "Tìm khóa học", () => new UC_CourseList() },
                { "Khóa học của tôi", () => new UC_MyCourses() },
                { "Bài kiểm tra", () => new UC_TakeExam() },
                { "Kết quả",      () => new UC_Result() },
                { "Tài liệu",     () => new UC_Documents() },
                { "Lịch học",     () => new UC_Schedule() },
                { "Tin nhắn",     () => new UC_Chat() },
                { "Thông báo",    () => new UC_Notification() },
                { "Hồ sơ",        () => new UC_Profile() }
            };
        }

        private void Sidebar_NavItemClicked(object? sender, string pageName)
        {
            if (pageName == "Logout")
            {
                LogoutCurrentUser();
                return;
            }

            if (_nav.ContainsKey(pageName))
            {
                _currentPageName = pageName;
                _topbar.PageTitle = pageName;
                LoadUI(_nav[pageName]());
            }
        }

        private void ReloadCurrentPage()
        {
            BackColor = AppColors.BgBase;
            _rightPanel.BackColor = AppColors.BgBase;
            mainboard.BackColor = AppColors.BgBase;
            _topbar.BackColor = AppColors.BgCard;
            _topbar.PageTitle = _currentPageName;

            if (_nav.TryGetValue(_currentPageName, out var factory))
                LoadUI(factory());

            _sidebar.Invalidate();
            _topbar.Invalidate();
        }

        private void LogoutCurrentUser()
        {
            var authService = new CourseGuard.Backend.Controllers.AuthController(
                new CourseGuard.Backend.Data.CourseGuardDbContext(""));
            string ipAddress = GetLocalIpAddress();
            authService.Logout(currentUser?.Id, currentUser?.Username ?? string.Empty, ipAddress);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void LoadUI(UserControl uc)
        {
            // Dispose old UCs to prevent memory leak
            foreach (Control ctrl in mainboard.Controls)
            {
                ctrl.Dispose();
            }
            mainboard.Controls.Clear();

            uc.Dock = DockStyle.Fill;
            uc.BackColor = AppColors.BgBase;
            mainboard.Controls.Add(uc);

            AppColors.ApplyTheme(uc);
        }

        private static string GetLocalIpAddress()
        {
            try
            {
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                        continue;
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
