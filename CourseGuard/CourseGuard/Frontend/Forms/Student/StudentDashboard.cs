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
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Services;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.UserControls.Student;
using CourseGuard.Frontend.Theme;
using CourseGuard.Backend.Services.Realtime;

namespace CourseGuard.Frontend.Forms.Student
{
    public partial class StudentDashboard : Form
    {
        private const string OverviewPage = "T\u1ed5ng quan";
        private const string ProfilePage = "H\u1ed3 s\u01a1";
        private const string ChatPage = "Tin nh\u1eafn";
        private const string NotificationPage = "Th\u00f4ng b\u00e1o";
        private const string ProfileTitle = "H\u1ed3 s\u01a1 c\u00e1 nh\u00e2n";

        private Dictionary<string, Func<UserControl>> _nav = new();
        private CourseGuard.Backend.Models.UserModel currentUser;
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly CourseController _courseController;
        private readonly ChatController _chatController;
        private SidebarPanel _sidebar;
        private TopbarPanel _topbar;
        private Panel _rightPanel;   // container: topbar + mainboard
        private DeadlineReminderService? _reminderService;
        private string _currentPageName = OverviewPage;
        private string _pendingGlobalSearchKeyword = string.Empty;
        private int _chatUnreadLoadVersion;
        private bool _isForceChangePasswordActive;
        private Panel? _forceChangePasswordBanner;

        public StudentDashboard(CourseGuard.Backend.Models.UserModel user, bool focusSecurityOnStart = false)
        {
            currentUser = user;
            AppColors.IsDarkMode = false;
            InitializeComponent();
            _chatController = new ChatController(_dbContext);
            _courseController = new CourseController(_dbContext);
            SearchFocusManager.Install(this);

            StudentProfileModel? profile = SafeGetProfile(user.Id);
            string displayName = GetDisplayName(user, profile);
            string avatarPath = profile?.AvatarPath ?? UserSessionContext.CurrentAvatarPath;
            UserSessionContext.UpdateProfile(displayName, avatarPath);
            List<NotificationModel> notifications = LoadCurrentNotifications();
            int notificationCount = CountUnreadNotifications(notifications);
            int openExamCount = ActivityDisplayHelper.SafeMetricCount(() => _dbContext.CountOpenExamsForStudent(user.Id));

            // ── 1. Build layout skeleton (Skill 01) ──────────────────
            // Sidebar docks Left on Form
            _sidebar = new SidebarPanel { Dock = DockStyle.Left };
            _sidebar.SetNavItems(new[]
            {
                new SidebarNavItem("Tổng quan", string.Empty, isHeading: true),
                new SidebarNavItem("Tổng quan", "home"),
                new SidebarNavItem("Học tập", string.Empty, isHeading: true),
                new SidebarNavItem("Tìm khóa học", "search"),
                new SidebarNavItem("Khóa học của tôi", "folder-check"),
                new SidebarNavItem("Tài liệu", "document"),
                new SidebarNavItem("Lịch học", "calendar"),
                new SidebarNavItem("Kiểm tra", string.Empty, isHeading: true),
                new SidebarNavItem("Bài kiểm tra", "clipboard-check"),
                new SidebarNavItem("Bài tập", "document"),
                new SidebarNavItem("Kết quả", "chart"),
                new SidebarNavItem("Cộng đồng", string.Empty, isHeading: true),
                new SidebarNavItem(ChatPage, "message"),
                new SidebarNavItem(NotificationPage, "bell"),
                new SidebarNavItem("Tài khoản", string.Empty, isHeading: true),
                new SidebarNavItem("Hồ sơ", "user")
            });
            _sidebar.NavItemClicked += Sidebar_NavItemClicked;
            LoadChatUnreadCountAsync().FireAndForgetSafe(this);
            SupabaseRealtimeChatService.Instance.OnChatChanged += OnChatRealtimeChanged;
            SupabaseRealtimeChatService.Instance.Start();

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

            // Warning banner for temporary password change
            _forceChangePasswordBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(40, 251, 64, 75),
                Padding = new Padding(12, 0, 12, 0),
                Visible = false
            };
            var lblWarning = new Label
            {
                Text = "⚠️ Mật khẩu của bạn là mật khẩu tạm thời. Bạn bắt buộc phải đổi mật khẩu tại trang Hồ sơ để mở khóa toàn bộ chức năng hệ thống.",
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = AppFonts.Semibold(9.5f),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                UseCompatibleTextRendering = false
            };
            _forceChangePasswordBanner.Controls.Add(lblWarning);

            // mainboard (from Designer) fills remaining space under topbar
            mainboard.Dock = DockStyle.Fill;

            // Assemble: mainboard Fill first, then banner/topbar Top docks above
            _rightPanel.Controls.Add(mainboard);
            _rightPanel.Controls.Add(_forceChangePasswordBanner);
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

            _reminderService = new DeadlineReminderService(currentUser.Id, _dbContext);
            _reminderService.NotificationCreated += DeadlineReminderService_NotificationCreated;
            _reminderService.Start();

            if (focusSecurityOnStart)
            {
                _isForceChangePasswordActive = true;
                if (_forceChangePasswordBanner != null)
                {
                    _forceChangePasswordBanner.Visible = true;
                }
                this.Load += (s, e) =>
                {
                    var searchBox = _topbar.Controls.OfType<TextBox>().FirstOrDefault();
                    if (searchBox != null)
                    {
                        searchBox.Enabled = false;
                    }
                };
                NavigateToPage(ProfilePage, focusSecurity: true);
            }
            else
            {
                NavigateToPage(OverviewPage);
            }
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

        public void RefreshNotificationSummary()
        {
            List<NotificationModel> notifications = LoadCurrentNotifications();
            _topbar.NotificationCount = CountUnreadNotifications(notifications);
            _topbar.NotificationPreviewItems = BuildNotificationPreviewItems(notifications);
            _topbar.Invalidate();
        }

        private void DeadlineReminderService_NotificationCreated(object? sender, EventArgs e)
        {
            RefreshNotificationSummarySafe();
        }

        private void RefreshNotificationSummarySafe()
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                if (!IsHandleCreated)
                {
                    return;
                }

                try
                {
                    BeginInvoke(new MethodInvoker(RefreshNotificationSummarySafe));
                }
                catch (InvalidOperationException)
                {
                }

                return;
            }

            RefreshNotificationSummary();
            RefreshVisibleNotificationPage();
        }

        private void RefreshVisibleNotificationPage()
        {
            if (_currentPageName != NotificationPage)
            {
                return;
            }

            foreach (Control ctrl in mainboard.Controls)
            {
                if (ctrl is UC_Notification notifications)
                {
                    notifications.RefreshAsync().FireAndForgetSafe(notifications);
                    return;
                }
            }
        }

        private List<NotificationModel> LoadCurrentNotifications()
        {
            int userId = currentUser?.Id ?? UserSessionContext.CurrentUserId ?? 0;
            return ActivityDisplayHelper.SafeList(() => new NotificationRepository().LoadByUserId(userId));
        }

        private int CountUnreadNotifications(List<NotificationModel> notifications)
        {
            return notifications.Count > 0
                ? notifications.Count(n => !n.IsRead)
                : ActivityDisplayHelper.SafeMetricCount(() => _dbContext.CountUnreadNotifications(currentUser?.Id ?? UserSessionContext.CurrentUserId ?? 0));
        }

        private static IEnumerable<string> BuildNotificationPreviewItems(IEnumerable<NotificationModel> notifications)
        {
            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n =>
                {
                    string text = string.IsNullOrWhiteSpace(n.Content) ? n.Title : $"{n.Title} - {n.Content}";
                    string time = SystemTimeFormatter.FormatVietnamTime(n.CreatedAt);
                    return string.IsNullOrWhiteSpace(time) ? text : $"{time} - {text}";
                });
        }

        private void InitializeNavigation()
        {
            _nav = new Dictionary<string, Func<UserControl>>
            {
                { "Tổng quan",    CreateStudentOverviewPage },
                { "Tìm khóa học", () => new UC_CourseList(currentUser.Id, _courseController) },
                { "Khóa học của tôi", () => new UC_MyCourses(currentUser.Id, _courseController) },
                { "Bài kiểm tra", () => new UC_TakeExam() },
                { "Bài tập",      () => new UC_StudentAssignments() },
                { "Kết quả",      () => new UC_Result() },
                { "Tài liệu",     () => new UC_StudentLessons() },
                { "Lịch học",     () => new UC_Schedule() },
                { ChatPage,     () => new UC_Chat(currentUser.Id, _chatController) },
                { NotificationPage,    () => new UC_Notification() },
                { "Hồ sơ",        () => new UC_Profile() }
            };
        }

        private UserControl CreateStudentOverviewPage()
        {
            var overview = new UC_StudentDashboard();
            overview.ActionNavigationRequested += (_, pageName) => NavigateToPage(pageName);
            return overview;
        }

        private void Sidebar_NavItemClicked(object? sender, string pageName)
        {
            if (_isForceChangePasswordActive)
            {
                if (pageName == "Logout")
                {
                    LogoutCurrentUser();
                }
                else if (pageName != ProfilePage)
                {
                    MetaTheme.ShowModernDialog("Bạn bắt buộc phải đổi mật khẩu tại trang Hồ sơ để mở khóa toàn bộ chức năng hệ thống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            if (pageName == "Logout")
            {
                LogoutCurrentUser();
                return;
            }

            NavigateToPage(pageName);
        }

        private void Topbar_NavigationRequested(object? sender, TopbarNavigationRequestedEventArgs e)
        {
            if (_isForceChangePasswordActive)
            {
                if (e.PageName != ProfilePage)
                {
                    MetaTheme.ShowModernDialog("Bạn bắt buộc phải đổi mật khẩu tại trang Hồ sơ để mở khóa toàn bộ chức năng hệ thống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            NavigateToPage(e.PageName, e.FocusSecurity);
        }

        private async void Topbar_QuickSearchRequested(object? sender, TopbarQuickSearchRequestedEventArgs e)
        {
            if (_isForceChangePasswordActive) return;
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
            if (_isForceChangePasswordActive) return;
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

            bool isChatPage = pageName == ChatPage;
            if (isChatPage)
            {
                _chatUnreadLoadVersion++;
                _sidebar.ChatUnreadCount = 0;
            }

            LoadUI(factory());

            if (isChatPage)
            {
                MarkAllChatReadAsync().FireAndForgetSafe(this);
            }

            if (focusSecurity)
                FocusProfileSecuritySection();
        }

        private void OnChatRealtimeChanged(object? sender, ChatChangedEventArgs e)
        {
            if (_currentPageName != ChatPage && e.SenderId != (currentUser?.Id ?? 0))
            {
                LoadChatUnreadCountAsync().FireAndForgetSafe(this);
            }
        }

        private async Task LoadChatUnreadCountAsync()
        {
            int loadVersion = _chatUnreadLoadVersion;
            int userId = currentUser?.Id ?? UserSessionContext.CurrentUserId ?? 0;
            int unreadCount = await Task.Run(
                () => ActivityDisplayHelper.SafeMetricCount(() => _chatController.GetUnreadCount(userId)));
            SetChatUnreadCountSafe(unreadCount, loadVersion);
        }

        private async Task MarkAllChatReadAsync()
        {
            int userId = currentUser?.Id ?? UserSessionContext.CurrentUserId ?? 0;
            await Task.Run(() =>
            {
                try
                {
                    _chatController.MarkAllRead(userId);
                }
                catch
                {
                }
            });
        }

        private void SetChatUnreadCountSafe(int count, int loadVersion)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                if (!IsHandleCreated)
                {
                    return;
                }

                try
                {
                    BeginInvoke(new MethodInvoker(() => SetChatUnreadCountSafe(count, loadVersion)));
                }
                catch (InvalidOperationException)
                {
                }

                return;
            }

            if (loadVersion != _chatUnreadLoadVersion || _currentPageName == ChatPage)
            {
                return;
            }

            _sidebar.ChatUnreadCount = count;
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
            if (!LogoutConfirmation.Confirm())
                return;

            var authService = new CourseGuard.Backend.Controllers.AuthController(
                _dbContext);
            string ipAddress = GetLocalIpAddress();
            Task.Run(() => authService.Logout(currentUser?.Id, currentUser?.Username ?? string.Empty, ipAddress));
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

            if (uc is UC_Profile profile)
            {
                profile.PasswordChangedSuccessfully += (s, e) => UnlockDashboard();
            }

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

        private void UnlockDashboard()
        {
            _isForceChangePasswordActive = false;
            if (_forceChangePasswordBanner != null)
            {
                _forceChangePasswordBanner.Visible = false;
            }
            var searchBox = _topbar.Controls.OfType<TextBox>().FirstOrDefault();
            if (searchBox != null)
            {
                searchBox.Enabled = true;
            }
            // Force redraw/layout of right container
            _rightPanel.PerformLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isForceChangePasswordActive && DialogResult != DialogResult.OK && e.CloseReason == CloseReason.UserClosing)
            {
                MetaTheme.ShowModernDialog("Bạn phải đổi mật khẩu hoặc Đăng xuất để thoát hệ thống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }
            _reminderService?.Dispose();
            SupabaseRealtimeChatService.Instance.OnChatChanged -= OnChatRealtimeChanged;
            SupabaseRealtimeChatService.Instance.Stop();
            base.OnFormClosing(e);
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
