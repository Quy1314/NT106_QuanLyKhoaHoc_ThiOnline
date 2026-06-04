using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class TeacherDashboard : Form
    {
        private const string OverviewPage = "Tổng quan";
        private const string ProfilePage = "Hồ sơ";
        private const string ProfileTitle = "Hồ sơ cá nhân";

        private readonly UserModel _currentUser;
        private readonly int _teacherId;
        private readonly string _teacherName;
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly TeacherController _teacherController;
        private Dictionary<string, Func<UserControl>> _routes = new();
        private SidebarPanel _sidebar = null!;
        private TopbarPanel _topbar = null!;
        private Panel _rightPanel = null!;
        private Panel _content = null!;
        private string _currentPageName = OverviewPage;

        public TeacherDashboard() : this(new UserModel { Id = 0, Username = "teacher", FullName = "Teacher" })
        {
        }

        public TeacherDashboard(UserModel user)
        {
            _currentUser = user ?? new UserModel { Id = 0, Username = "teacher", FullName = "Teacher" };
            _teacherId = _currentUser.Id;
            _teacherName = GetDisplayName(_currentUser);
            _teacherController = new TeacherController(_dbContext);

            AppColors.IsDarkMode = false;
            InitializeComponent();
            SearchFocusManager.Install(this);
            BuildStudentStyleShell();
            InitializeNavigation();
            NavigateToPage(OverviewPage);
        }

        private void BuildStudentStyleShell()
        {
            _sidebar = new SidebarPanel { Dock = DockStyle.Left };
            _sidebar.SetNavItems(
                new[]
                {
                    "Tổng quan",
                    "Khóa học",
                    "Bài học",
                    "Bài tập",
                    "Bài kiểm tra",
                    "Giám sát thi",
                    "Kết quả",
                    "Sinh viên",
                    "Tài liệu",
                    "Lịch dạy",
                    "Tin nhắn",
                    "Thông báo",
                    "Hồ sơ"
                },
                new[]
                {
                    "home",
                    "folder-check",
                    "book",
                    "document",
                    "clipboard-check",
                    "exam",
                    "chart",
                    "user",
                    "document",
                    "calendar",
                    "message",
                    "bell",
                    "user"
                });
            _sidebar.NavItemClicked += Sidebar_NavItemClicked;

            _rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 0) };
            _topbar = new TopbarPanel
            {
                Dock = DockStyle.Top,
                PageTitle = OverviewPage,
                Subtitle = string.Empty,
                UserName = _teacherName,
                UseStudentTopbar = true,
                QuickSearchPlaceholder = "Tìm khóa học, bài kiểm tra, học viên, tài liệu...",
                OpenExamCount = ActivityDisplayHelper.SafeMetricCount(() => _teacherController.GetActiveExamSessions(_teacherId).Count),
                IsOnline = true,
                NotificationCount = ActivityDisplayHelper.SafeMetricCount(() => _dbContext.CountUnreadNotifications(_teacherId)),
                NotificationPreviewItems = BuildNotificationPreviewItems()
            };
            _topbar.ThemeToggled += (_, _) => ReloadCurrentPage();
            _topbar.LogoutRequested += (_, _) => LogoutCurrentUser();
            _topbar.NavigationRequested += Topbar_NavigationRequested;
            _topbar.QuickSearchRequested += Topbar_QuickSearchRequested;
            _topbar.SearchResultSelected += Topbar_SearchResultSelected;

            _content = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.BgBase };
            _rightPanel.Controls.Add(_content);
            _rightPanel.Controls.Add(_topbar);
            Controls.Add(_rightPanel);
            Controls.Add(_sidebar);

            BackColor = AppColors.BgBase;
            _rightPanel.BackColor = AppColors.BgBase;
            _content.BackColor = AppColors.BgBase;
        }

        private void InitializeNavigation()
        {
            _routes = new Dictionary<string, Func<UserControl>>
            {
                { "Tổng quan", () => new UC_TeacherOverview(_teacherId) },
                { "Khóa học", () => new UC_TeacherCourses(_teacherId) },
                { "Bài học", () => new UC_TeacherLessons(_teacherId) },
                { "Bài tập", () => new UC_TeacherAssignments(_teacherId) },
                { "Bài kiểm tra", () => new UC_TeacherExams(_teacherId) },
                { "Giám sát thi", () => new UC_ExamMonitor(_teacherId) },
                { "Kết quả", () => new UC_TeacherResults(_teacherId) },
                { "Sinh viên", () => new UC_TeacherStudents(_teacherId) },
                { "Tài liệu", () => new UC_TeacherMaterials(_teacherId) },
                { "Lịch dạy", () => new UC_TeacherSchedule(_teacherId) },
                { "Tin nhắn", () => new UC_TeacherMessages(_teacherId) },
                { "Thông báo", () => new UC_TeacherNotifications(_teacherId) },
                { "Hồ sơ", CreateTeacherProfilePage }
            };
        }

        private UserControl CreateTeacherProfilePage()
        {
            var profile = new UC_TeacherProfile(_teacherId);
            profile.ProfileChanged += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.FullName))
                    _topbar.UserName = e.FullName.Trim();
                _topbar.RefreshUserFromSession();
                _topbar.Invalidate();
            };
            return profile;
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
            NavigateToPage(e.PageName);
        }

        private void Topbar_QuickSearchRequested(object? sender, TopbarQuickSearchRequestedEventArgs e)
        {
            string keyword = e.Keyword.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
                return;

            string normalized = keyword.ToLowerInvariant();
            IEnumerable<TopbarSearchResult> results = _routes.Keys
                .Where(page => page.ToLowerInvariant().Contains(normalized))
                .Select(page => new TopbarSearchResult
                {
                    Group = "Chức năng giảng viên",
                    Title = page,
                    Description = BuildSearchDescription(page),
                    PageName = page,
                    Keyword = keyword
                });

            _topbar.ShowQuickSearchResults(results);
        }

        private void Topbar_SearchResultSelected(object? sender, TopbarSearchResultSelectedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Result.PageName))
                NavigateToPage(e.Result.PageName);
        }

        private void NavigateToPage(string pageName)
        {
            if (!_routes.TryGetValue(pageName, out Func<UserControl>? factory))
                return;

            _currentPageName = pageName;
            _sidebar.SetActiveByName(pageName);
            _topbar.PageTitle = pageName == ProfilePage ? ProfileTitle : pageName;
            LoadUI(factory());
        }

        private void LoadUI(UserControl uc)
        {
            foreach (Control control in _content.Controls)
                control.Dispose();

            _content.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            uc.BackColor = AppColors.BgBase;
            _content.Controls.Add(uc);
            AppColors.ApplyTheme(uc);
        }

        private void ReloadCurrentPage()
        {
            BackColor = AppColors.BgBase;
            _rightPanel.BackColor = AppColors.BgBase;
            _content.BackColor = AppColors.BgBase;
            _topbar.BackColor = AppColors.BgCard;
            _topbar.OpenExamCount = ActivityDisplayHelper.SafeMetricCount(() => _teacherController.GetActiveExamSessions(_teacherId).Count);
            _topbar.NotificationCount = ActivityDisplayHelper.SafeMetricCount(() => _dbContext.CountUnreadNotifications(_teacherId));
            _topbar.NotificationPreviewItems = BuildNotificationPreviewItems();
            _topbar.PageTitle = _currentPageName == ProfilePage ? ProfileTitle : _currentPageName;
            _sidebar.SetActiveByName(_currentPageName);

            if (_routes.TryGetValue(_currentPageName, out Func<UserControl>? factory))
                LoadUI(factory());

            _sidebar.Invalidate();
            _topbar.Invalidate();
        }

        private void LogoutCurrentUser()
        {
            if (!LogoutConfirmation.Confirm())
                return;

            var authService = new AuthController(_dbContext);
            authService.Logout(_teacherId, _currentUser?.Username ?? _teacherName, GetLocalIpAddress());
            DialogResult = DialogResult.OK;
            Close();
        }

        private IEnumerable<string> BuildNotificationPreviewItems()
        {
            return ActivityDisplayHelper.SafeList(() => new NotificationRepository().LoadByUserId(_teacherId))
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n =>
                {
                    string text = string.IsNullOrWhiteSpace(n.Content) ? n.Title : $"{n.Title} - {n.Content}";
                    string time = SystemTimeFormatter.FormatVietnamTime(n.CreatedAt);
                    return string.IsNullOrWhiteSpace(time) ? text : $"{time} - {text}";
                });
        }

        private static string BuildSearchDescription(string pageName)
        {
            return pageName switch
            {
                "Khóa học" => "Quản lý các khóa học giảng viên phụ trách",
                "Bài học" => "Tạo và cập nhật nội dung bài học",
                "Bài tập" => "Quản lý bài tập và hạn nộp",
                "Bài kiểm tra" => "Tạo và cấu hình kỳ thi",
                "Giám sát thi" => "Theo dõi các phiên thi đang diễn ra",
                "Kết quả" => "Xem và cập nhật điểm",
                "Sinh viên" => "Duyệt ghi danh và xem học viên",
                "Tài liệu" => "Quản lý tài liệu khóa học",
                "Lịch dạy" => "Quản lý lịch dạy và buổi học",
                "Tin nhắn" => "Trao đổi theo khóa học",
                "Thông báo" => "Xem thông báo cá nhân",
                "Hồ sơ" => "Thông tin tài khoản giảng viên",
                _ => "Mở chức năng giảng viên"
            };
        }

        private static string GetDisplayName(UserModel user)
        {
            if (!string.IsNullOrWhiteSpace(user.FullName))
                return user.FullName.Trim();
            if (!string.IsNullOrWhiteSpace(user.Username))
                return user.Username.Trim();
            return "teacher";
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
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
                            return ip.Address.ToString();
                    }
                }
            }
            catch
            {
            }

            return "127.0.0.1";
        }
    }
}
