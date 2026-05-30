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
using System.Linq;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.UserControls.Student;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public partial class StudentDashboard : Form
    {
        private const string OverviewPage = "T\u1ed5ng quan";
        private const string ProfilePage = "H\u1ed3 s\u01a1";
        private const string ProfileTitle = "H\u1ed3 s\u01a1 c\u00e1 nh\u00e2n";

        private Dictionary<string, Func<UserControl>> _nav = new();
        private CourseGuard.Backend.Models.UserModel currentUser;
        private readonly CourseGuardDbContext _dbContext = new("");
        private SidebarPanel _sidebar;
        private TopbarPanel _topbar;
        private Panel _rightPanel;   // container: topbar + mainboard
        private string _currentPageName = OverviewPage;
        private string _pendingGlobalSearchKeyword = string.Empty;

        public StudentDashboard(CourseGuard.Backend.Models.UserModel user)
        {
            currentUser = user;
            AppColors.IsDarkMode = false;
            InitializeComponent();
            SearchFocusManager.Install(this);

            StudentProfileModel? profile = SafeGetProfile(user.Id);
            string displayName = GetDisplayName(user, profile);
            string avatarPath = profile?.AvatarPath ?? UserSessionContext.CurrentAvatarPath;
            UserSessionContext.UpdateProfile(displayName, avatarPath);
            List<NotificationModel> notifications = LoadCurrentNotifications();
            int notificationCount = CountUnreadNotifications(notifications);
            int openExamCount = SafeCount(() => _dbContext.CountOpenExamsForStudent(user.Id));

            // ── 1. Build layout skeleton (Skill 01) ──────────────────
            // Sidebar docks Left on Form
            _sidebar = new SidebarPanel { Dock = DockStyle.Left };
            _sidebar.SetNavItems(
                new[] { "Tổng quan", "Tìm khóa học", "Khóa học của tôi", "Bài kiểm tra", "Bài tập", "Kết quả", "Tài liệu", "Lịch học", "Tin nhắn", "Thông báo", "Hồ sơ" },
                new[] { "home", "search", "folder-check", "clipboard-check", "document-text", "chart", "document", "calendar", "message", "bell", "user" }
            );
            _sidebar.NavItemClicked += Sidebar_NavItemClicked;

            // Right container holds Topbar(Top) + mainboard(Fill)
            _rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 0) };
            _topbar = new TopbarPanel
            {
                Dock = DockStyle.Top,
                PageTitle = "Tổng quan",
                Subtitle = string.Empty,
                UserName = displayName,
                UseStudentTopbar = true,
                QuickSearchPlaceholder = "Tìm khóa học, bài kiểm tra, tài liệu...",
                OpenExamCount = openExamCount,
                IsOnline = true,
                NotificationCount = notificationCount,
                NotificationPreviewItems = BuildNotificationPreviewItems(notifications)
            };
            _topbar.ThemeToggled += (_, _) => ReloadCurrentPage();
            _topbar.LogoutRequested += (_, _) => LogoutCurrentUser();
            _topbar.NavigationRequested += Topbar_NavigationRequested;
            _topbar.QuickSearchRequested += Topbar_QuickSearchRequested;
            _topbar.SearchResultSelected += Topbar_SearchResultSelected;

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

            NavigateToPage(OverviewPage);
        }

        private StudentProfileModel? SafeGetProfile(int userId)
        {
            try
            {
                return _dbContext.GetStudentProfile(userId);
            }
            catch
            {
                return null;
            }
        }

        private static string GetDisplayName(UserModel user, StudentProfileModel? profile)
        {
            if (!string.IsNullOrWhiteSpace(profile?.FullName))
                return profile.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(user.FullName))
                return user.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(user.Username))
                return user.Username.Trim();

            return "student";
        }

        private static int SafeCount(Func<int> getter)
        {
            try
            {
                return Math.Max(0, getter());
            }
            catch
            {
                return 0;
            }
        }

        public void RefreshNotificationSummary()
        {
            List<NotificationModel> notifications = LoadCurrentNotifications();
            _topbar.NotificationCount = CountUnreadNotifications(notifications);
            _topbar.NotificationPreviewItems = BuildNotificationPreviewItems(notifications);
            _topbar.Invalidate();
        }

        private List<NotificationModel> LoadCurrentNotifications()
        {
            int userId = currentUser?.Id ?? UserSessionContext.CurrentUserId ?? 0;
            return SafeList(() => new NotificationRepository().LoadByUserId(userId));
        }

        private int CountUnreadNotifications(List<NotificationModel> notifications)
        {
            return notifications.Count > 0
                ? notifications.Count(n => !n.IsRead)
                : SafeCount(() => _dbContext.CountUnreadNotifications(currentUser?.Id ?? UserSessionContext.CurrentUserId ?? 0));
        }

        private static IEnumerable<string> BuildNotificationPreviewItems(IEnumerable<NotificationModel> notifications)
        {
            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => string.IsNullOrWhiteSpace(n.Content) ? n.Title : $"{n.Title} - {n.Content}");
        }

        private static List<T> SafeList<T>(Func<List<T>> getter)
        {
            try
            {
                return getter() ?? new List<T>();
            }
            catch
            {
                return new List<T>();
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
                { "Bài tập",      () => new UC_StudentAssignments() },
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

            NavigateToPage(pageName);
        }

        private void Topbar_NavigationRequested(object? sender, TopbarNavigationRequestedEventArgs e)
        {
            NavigateToPage(e.PageName, e.FocusSecurity);
        }

        private async void Topbar_QuickSearchRequested(object? sender, TopbarQuickSearchRequestedEventArgs e)
        {
            string keyword = e.Keyword.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
                return;

            try
            {
                int studentId = currentUser?.Id ?? UserSessionContext.CurrentUserId ?? 0;
                List<StudentSearchResultModel> results = await System.Threading.Tasks.Task.Run(
                    () => _dbContext.SearchStudentGlobal(studentId, keyword));

                _topbar.ShowQuickSearchResults(results.Select(result => new TopbarSearchResult
                {
                    Group = result.Group,
                    Title = result.Title,
                    Description = result.Description,
                    PageName = result.PageName,
                    Keyword = result.Keyword
                }));
            }
            catch
            {
                _topbar.ShowQuickSearchResults(Array.Empty<TopbarSearchResult>());
            }
        }

        private void Topbar_SearchResultSelected(object? sender, TopbarSearchResultSelectedEventArgs e)
        {
            string pageName = e.Result.PageName;
            if (string.IsNullOrWhiteSpace(pageName) || !_nav.ContainsKey(pageName))
                return;

            _pendingGlobalSearchKeyword = e.Result.Keyword;
            NavigateToPage(pageName);
        }

        private void NavigateToPage(string pageName, bool focusSecurity = false)
        {
            if (!_nav.TryGetValue(pageName, out var factory))
                return;

            _currentPageName = pageName;
            _sidebar.SetActiveByName(pageName);
            _topbar.PageTitle = GetPageTitle(pageName);
            LoadUI(factory());

            if (focusSecurity)
                FocusProfileSecuritySection();
        }

        private static string GetPageTitle(string pageName)
        {
            return pageName == ProfilePage ? ProfileTitle : pageName;
        }

        private void FocusProfileSecuritySection()
        {
            foreach (Control ctrl in mainboard.Controls)
            {
                if (ctrl is UC_Profile profile)
                {
                    profile.FocusSecuritySection();
                    return;
                }
            }
        }

        private void ReloadCurrentPage()
        {
            BackColor = AppColors.BgBase;
            _rightPanel.BackColor = AppColors.BgBase;
            mainboard.BackColor = AppColors.BgBase;
            _topbar.BackColor = AppColors.BgCard;
            _topbar.PageTitle = GetPageTitle(_currentPageName);
            _sidebar.SetActiveByName(_currentPageName);

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

            if (!string.IsNullOrWhiteSpace(_pendingGlobalSearchKeyword)
                && uc is IStudentSearchTarget searchTarget)
            {
                searchTarget.ApplyGlobalSearch(_pendingGlobalSearchKeyword);
                _pendingGlobalSearchKeyword = string.Empty;
            }
            else if (!string.IsNullOrWhiteSpace(_pendingGlobalSearchKeyword))
            {
                _pendingGlobalSearchKeyword = string.Empty;
            }
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
