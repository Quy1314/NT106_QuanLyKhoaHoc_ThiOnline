using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_StudentDashboard : UserControl
    {
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly NotificationRepository _notificationRepository = new();

        private TableLayoutPanel _rootGrid = null!;
        private TableLayoutPanel _statsGrid = null!;
        private TableLayoutPanel _contentGrid = null!;
        private RoundedPanel _noticePanel = null!;
        private RoundedPanel _activityPanel = null!;
        private RoundedPanel _noticeBody = null!;
        private Label _noticeEmptyLabel = null!;
        private FlowLayoutPanel _activityList = null!;
        private StatCard _courseCard = null!;
        private StatCard _examCard = null!;
        private StatCard _notificationCard = null!;
        private StatCard _averageScoreCard = null!;

        public UC_StudentDashboard()
        {
            InitializeComponent();
            BuildOverviewLayout();
            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.StudentOverviewDashboard);
            try
            {
                DashboardData data = await System.Threading.Tasks.Task.Run(LoadDashboardData);
                ApplyDashboardData(data);
            }
            catch (Exception ex)
            {
                ApplyDashboardData(DashboardData.Error($"Không thể tải dữ liệu: {ex.Message}"));
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private DashboardData LoadDashboardData()
        {
            int userId = UserSessionContext.CurrentUserId ?? 0;
            if (userId <= 0)
                return DashboardData.Error("Không xác định được tài khoản học sinh.");

            int courseCount = ActivityDisplayHelper.SafeMetricCount(() => _dbContext.CountActiveEnrollments(userId));
            int openOrUpcomingExamCount = ActivityDisplayHelper.SafeMetricCount(() => _dbContext.CountAvailableExamsForStudent(userId));
            int completedExamCount = ActivityDisplayHelper.SafeMetricCount(() => _dbContext.CountCompletedExamsForStudent(userId));
            double? averageScore = ActivityDisplayHelper.SafeAverageScore(() => _dbContext.GetStudentExamAverageScore(userId));

            List<NotificationModel> notifications = ActivityDisplayHelper.SafeList(() => _notificationRepository
                .LoadByUserId(userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(6)
                .ToList());
            int unreadNotificationCount = notifications.Count(n => !n.IsRead);

            List<RecentUserActivityModel> logs = ActivityDisplayHelper.SafeList(() => _dbContext.GetRecentUserActivitiesByUser(userId, 8));
            List<EnrollmentModel> enrollments = ActivityDisplayHelper.SafeList(() => _dbContext.GetEnrollmentsByStudent(userId));

            List<ActivityRow> activities = logs
                .Select(ToActivityRow)
                .Concat(DeriveActivities(enrollments, notifications))
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .ToList();

            return new DashboardData
            {
                CourseCount = courseCount,
                ExamCount = openOrUpcomingExamCount > 0 ? openOrUpcomingExamCount : completedExamCount,
                HasOpenOrUpcomingExams = openOrUpcomingExamCount > 0,
                NotificationCount = unreadNotificationCount,
                AverageScore = averageScore,
                Notifications = notifications,
                Activities = activities
            };
        }

        private void BuildOverviewLayout()
        {
            SuspendLayout();

            Controls.Clear();
            BackColor = AppColors.BgBase;
            Padding = Padding.Empty;

            _rootGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgBase,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(24)
            };
            _rootGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _rootGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 112f));
            _rootGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 168f));
            _rootGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblTitle.AutoSize = false;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Margin = Padding.Empty;
            lblTitle.Text = "Tổng quan cá nhân";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            lblTitle.ForeColor = AppColors.TextPrimary;

            _statsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = AppColors.BgBase,
                Margin = new Padding(0, 8, 0, 16)
            };
            for (int i = 0; i < 4; i++)
                _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            _statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _courseCard = AddStatCard(0, "Khóa học", "0", "course", "Đang tham gia");
            _examCard = AddStatCard(1, "Bài thi", "0", "exam", "Đang/sắp mở");
            _notificationCard = AddStatCard(2, "Thông báo", "0", "notice", "Chưa đọc");
            _averageScoreCard = AddStatCard(3, "Điểm TB", "N/A", "score", "Bài đã chấm");

            _contentGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppColors.BgBase,
                Margin = Padding.Empty
            };
            _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            _contentGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _noticePanel = CreatePanelCard();
            _noticePanel.Margin = new Padding(0, 0, 12, 0);
            BuildNoticePanel();

            _activityPanel = CreatePanelCard();
            _activityPanel.Margin = new Padding(12, 0, 0, 0);
            BuildActivityPanel();

            _contentGrid.Controls.Add(_noticePanel, 0, 0);
            _contentGrid.Controls.Add(_activityPanel, 1, 0);

            _rootGrid.Controls.Add(StudentTabChrome.CreateHeader(
                "Tổng quan cá nhân",
                "Theo dõi khóa học đang tham gia, bài kiểm tra, thông báo và hoạt động gần đây."), 0, 0);
            _rootGrid.Controls.Add(_statsGrid, 0, 1);
            _rootGrid.Controls.Add(_contentGrid, 0, 2);
            Controls.Add(_rootGrid);

            Resize -= UC_StudentDashboard_Resize;
            Resize += UC_StudentDashboard_Resize;
            ResumeLayout(true);
        }

        private StatCard AddStatCard(int column, string title, string value, string icon, string caption)
        {
            var card = new StatCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 8, 0),
                Title = title,
                Value = value,
                IconChar = icon,
                TrendPercent = caption,
                ShowStatusArrow = false,
                StatusTone = StatCardStatusTone.Neutral,
                Caption = string.Empty,
                MiniChartValues = null
            };

            _statsGrid.Controls.Add(card, column, 0);
            return card;
        }

        private void BuildNoticePanel()
        {
            _noticePanel.Padding = new Padding(18);

            var panelGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            panelGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblRecentNotices.AutoSize = false;
            lblRecentNotices.Dock = DockStyle.Fill;
            lblRecentNotices.Margin = Padding.Empty;
            lblRecentNotices.Text = "Thông báo gần đây";
            lblRecentNotices.TextAlign = ContentAlignment.MiddleLeft;
            lblRecentNotices.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblRecentNotices.ForeColor = AppColors.TextPrimary;

            dgvRecentNotices.Dock = DockStyle.Fill;
            dgvRecentNotices.Margin = Padding.Empty;
            dgvRecentNotices.ReadOnly = true;
            dgvRecentNotices.AllowUserToAddRows = false;
            dgvRecentNotices.AllowUserToDeleteRows = false;
            dgvRecentNotices.AllowUserToResizeRows = false;
            dgvRecentNotices.RowHeadersVisible = false;
            dgvRecentNotices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecentNotices.MultiSelect = false;
            StudentTabChrome.StyleGrid(dgvRecentNotices);

            _noticeBody = StudentTabChrome.CreateTableBody(dgvRecentNotices, out _noticeEmptyLabel);
            _noticeEmptyLabel.Text = "Chưa có thông báo gần đây";

            panelGrid.Controls.Add(lblRecentNotices, 0, 0);
            panelGrid.Controls.Add(_noticeBody, 0, 1);
            _noticePanel.Controls.Add(panelGrid);
        }

        private void BuildActivityPanel()
        {
            _activityPanel.Padding = new Padding(18);

            var panelGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            panelGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var title = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Hoạt động gần đây",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            _activityList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            _activityList.Resize += (_, _) => ResizeActivityItems();

            panelGrid.Controls.Add(title, 0, 0);
            panelGrid.Controls.Add(_activityList, 0, 1);
            _activityPanel.Controls.Add(panelGrid);
        }

        private void ApplyDashboardData(DashboardData data)
        {
            _courseCard.Value = data.CourseCount.ToString(CultureInfo.InvariantCulture);
            ApplyCardState(_courseCard, "Đang tham gia", data.CourseCount > 0 ? StatCardStatusTone.Positive : StatCardStatusTone.Neutral);

            _examCard.Value = data.ExamCount.ToString(CultureInfo.InvariantCulture);
            string examStatus = data.ExamCount <= 0
                ? "Không có bài mở"
                : data.HasOpenOrUpcomingExams ? "Sắp tới" : "Đã làm";
            ApplyCardState(_examCard, examStatus, data.HasOpenOrUpcomingExams ? StatCardStatusTone.Warning : data.ExamCount > 0 ? StatCardStatusTone.Positive : StatCardStatusTone.Neutral);

            _notificationCard.Value = data.NotificationCount.ToString(CultureInfo.InvariantCulture);
            ApplyCardState(_notificationCard,
                data.NotificationCount > 0 ? $"Có {data.NotificationCount} thông báo mới" : "Không có thông báo mới",
                data.NotificationCount > 0 ? StatCardStatusTone.Warning : StatCardStatusTone.Neutral);

            _averageScoreCard.Value = data.AverageScore.HasValue
                ? data.AverageScore.Value.ToString("0.0", CultureInfo.InvariantCulture)
                : "N/A";
            ApplyCardState(_averageScoreCard,
                data.AverageScore.HasValue ? "Bài đã chấm" : "Chưa có điểm",
                data.AverageScore.HasValue ? StatCardStatusTone.Positive : StatCardStatusTone.Neutral);

            BindNotifications(data);
            BindActivities(data);
            ApplyAcademicStyle();
        }

        private static void ApplyCardState(StatCard card, string statusText, StatCardStatusTone tone)
        {
            card.TrendPercent = statusText;
            card.ShowStatusArrow = false;
            card.StatusTone = tone;
            card.Caption = string.Empty;
            card.MiniChartValues = null;
        }

        private void BindNotifications(DashboardData data)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Tiêu đề", typeof(string));
            dt.Columns.Add("Nguồn", typeof(string));

            if (!string.IsNullOrWhiteSpace(data.ErrorMessage))
            {
                ShowNoticeEmptyState(data.ErrorMessage, AppColors.Warning);
                return;
            }
            else if (data.Notifications.Count == 0)
            {
                ShowNoticeEmptyState("Chưa có thông báo gần đây", AppColors.TextMuted);
                return;
            }
            else
            {
                foreach (NotificationModel notification in data.Notifications)
                {
                    dt.Rows.Add(
                        FormatDateTime(notification.CreatedAt),
                        notification.Title,
                        InferNotificationSource(notification));
                }
            }

            dgvRecentNotices.DataSource = dt;
            dgvRecentNotices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            StudentTabChrome.SetTableState(_noticeBody, dgvRecentNotices, _noticeEmptyLabel, showTable: true, string.Empty);
            dgvRecentNotices.BringToFront();
            dgvRecentNotices.ClearSelection();
            dgvRecentNotices.CurrentCell = null;
        }

        private void ShowNoticeEmptyState(string message, Color color)
        {
            dgvRecentNotices.DataSource = null;
            StudentTabChrome.SetTableState(_noticeBody, dgvRecentNotices, _noticeEmptyLabel, showTable: false, message);
            _noticeEmptyLabel.ForeColor = color;
            _noticeEmptyLabel.BringToFront();
        }

        private void BindActivities(DashboardData data)
        {
            _activityList.SuspendLayout();
            _activityList.Controls.Clear();

            if (!string.IsNullOrWhiteSpace(data.ErrorMessage))
            {
                AddActivityItem("Không thể tải hoạt động gần đây", "", AppColors.Warning);
            }
            else if (data.Activities.Count == 0)
            {
                AddActivityItem("Chưa có hoạt động gần đây", "", AppColors.TextMuted);
            }
            else
            {
                foreach (ActivityRow activity in data.Activities)
                    AddActivityItem(activity.Title, FormatDateTime(activity.CreatedAt), activity.Accent);
            }

            _activityList.ResumeLayout(true);
            ResizeActivityItems();
        }

        private void AddActivityItem(string title, string time, Color accent)
        {
            int itemWidth = Math.Max(180, _activityList.ClientSize.Width - 18);
            var item = new Panel
            {
                Width = itemWidth,
                Height = string.IsNullOrWhiteSpace(time) ? 44 : 58,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.Transparent
            };
            item.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using SolidBrush dot = new SolidBrush(accent);
                e.Graphics.FillEllipse(dot, 2, 11, 10, 10);
            };

            var lbl = new Label
            {
                Text = title,
                Location = new Point(22, 4),
                Size = new Size(Math.Max(120, itemWidth - 34), 24),
                Font = AppFonts.Button,
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };
            var meta = new Label
            {
                Text = time,
                Location = new Point(22, 30),
                Size = new Size(Math.Max(120, itemWidth - 34), 20),
                Font = AppFonts.Caption,
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };

            item.Controls.Add(lbl);
            if (!string.IsNullOrWhiteSpace(time))
                item.Controls.Add(meta);
            _activityList.Controls.Add(item);
        }

        private void ResizeActivityItems()
        {
            if (_activityList == null)
                return;

            int itemWidth = Math.Max(180, _activityList.ClientSize.Width - 18);
            foreach (Control item in _activityList.Controls)
            {
                item.Width = itemWidth;
                foreach (Control child in item.Controls)
                    child.Width = Math.Max(120, itemWidth - 34);
            }
        }

        private static RoundedPanel CreatePanelCard()
        {
            return new RoundedPanel
            {
                Dock = DockStyle.Fill,
                FillColor = AppColors.BgCard,
                BorderColor = AppColors.Border,
                CornerRadius = 16
            };
        }

        private void ApplyAcademicStyle()
        {
            BackColor = AppColors.BgBase;
            if (_rootGrid != null)
                _rootGrid.BackColor = AppColors.BgBase;
            if (_statsGrid != null)
                _statsGrid.BackColor = AppColors.BgBase;
            if (_contentGrid != null)
                _contentGrid.BackColor = AppColors.BgBase;

            foreach (RoundedPanel panel in new[] { _noticePanel, _activityPanel })
            {
                panel.FillColor = AppColors.BgCard;
                panel.BorderColor = AppColors.Border;
                panel.Invalidate();
            }

            AppColors.ApplyTheme(this);
            lblTitle.ForeColor = AppColors.TextPrimary;
            lblRecentNotices.ForeColor = AppColors.TextPrimary;
            if (_noticeEmptyLabel != null && dgvRecentNotices.Visible)
                _noticeEmptyLabel.ForeColor = AppColors.TextMuted;
            if (_noticeBody != null)
            {
                _noticeBody.FillColor = dgvRecentNotices.Visible
                    ? AppColors.BgCard
                    : (AppColors.IsDarkMode ? AppColors.BgCardHover : ColorTranslator.FromHtml("#F8FAFC"));
                _noticeBody.BorderColor = dgvRecentNotices.Visible ? Color.Transparent : AppColors.Border;
                _noticeBody.Invalidate();
            }
            dgvRecentNotices.ClearSelection();
        }

        private void UC_StudentDashboard_Resize(object? sender, EventArgs e)
        {
            bool compact = Width < 980;
            _statsGrid.ColumnCount = compact ? 2 : 4;
            _contentGrid.ColumnCount = compact ? 1 : 2;
            _rootGrid.RowStyles[1].Height = compact ? 336f : 168f;

            RebuildColumnStyles(_statsGrid, compact ? 2 : 4);
            RebuildContentGrid(compact);
            ResizeActivityItems();
        }

        private void RebuildContentGrid(bool compact)
        {
            _contentGrid.SuspendLayout();
            _contentGrid.Controls.Clear();
            _contentGrid.ColumnStyles.Clear();
            _contentGrid.RowStyles.Clear();

            if (compact)
            {
                _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                _contentGrid.RowCount = 2;
                _contentGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 58f));
                _contentGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));
                _noticePanel.Margin = new Padding(0, 0, 0, 12);
                _activityPanel.Margin = new Padding(0, 12, 0, 0);
                _contentGrid.Controls.Add(_noticePanel, 0, 0);
                _contentGrid.Controls.Add(_activityPanel, 0, 1);
            }
            else
            {
                _contentGrid.RowCount = 1;
                _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
                _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
                _contentGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                _noticePanel.Margin = new Padding(0, 0, 12, 0);
                _activityPanel.Margin = new Padding(12, 0, 0, 0);
                _contentGrid.Controls.Add(_noticePanel, 0, 0);
                _contentGrid.Controls.Add(_activityPanel, 1, 0);
            }

            _contentGrid.ResumeLayout(true);
        }

        private static void RebuildColumnStyles(TableLayoutPanel grid, int columns)
        {
            grid.SuspendLayout();
            grid.ColumnStyles.Clear();
            for (int i = 0; i < columns; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));

            for (int i = 0; i < grid.Controls.Count; i++)
            {
                Control control = grid.Controls[i];
                int column = i % columns;
                int row = i / columns;
                grid.SetColumn(control, column);
                grid.SetRow(control, row);
            }

            grid.RowCount = columns == 2 ? 2 : 1;
            grid.RowStyles.Clear();
            for (int i = 0; i < grid.RowCount; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / grid.RowCount));
            grid.ResumeLayout(true);
        }

        private static List<ActivityRow> DeriveActivities(
            List<EnrollmentModel> enrollments,
            List<NotificationModel> notifications)
        {
            var rows = new List<ActivityRow>();

            rows.AddRange(enrollments
                .Where(e => e.JoinedAt != DateTime.MinValue)
                .Select(e => new ActivityRow
                {
                    CreatedAt = e.JoinedAt,
                    Title = BuildEnrollmentActivityTitle(e),
                    Accent = IsActiveEnrollment(e.Status) ? AppColors.Success : AppColors.Warning
                }));

            rows.AddRange(notifications.Select(n => new ActivityRow
            {
                CreatedAt = n.CreatedAt,
                Title = $"Nhận thông báo: {n.Title}",
                Accent = AppColors.AccentBlue
            }));

            return rows
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }

        private static ActivityRow ToActivityRow(RecentUserActivityModel activity)
        {
            return new ActivityRow
            {
                CreatedAt = activity.CreatedAt,
                Title = ActivityDisplayHelper.TranslateActivity(activity),
                Accent = ActivityDisplayHelper.GetActivityAccent(activity.Action)
            };
        }

        private static string BuildEnrollmentActivityTitle(EnrollmentModel enrollment)
        {
            string courseName = string.IsNullOrWhiteSpace(enrollment.CourseName)
                ? "khóa học"
                : enrollment.CourseName.Trim();

            return IsActiveEnrollment(enrollment.Status)
                ? $"Được duyệt vào khóa học {courseName}"
                : $"Gửi yêu cầu tham gia khóa học {courseName}";
        }

        private static bool IsActiveEnrollment(string? status)
        {
            return string.Equals(status, "ACTIVE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "APPROVED", StringComparison.OrdinalIgnoreCase);
        }

        private static string InferNotificationSource(NotificationModel notification)
        {
            string text = $"{notification.Title} {notification.Content}".ToLowerInvariant();
            if (text.Contains("bài") || text.Contains("thi") || text.Contains("kiểm tra"))
                return "Bài kiểm tra";
            if (text.Contains("khóa") || text.Contains("lớp") || text.Contains("course"))
                return "Khóa học";
            if (text.Contains("tài liệu"))
                return "Tài liệu";
            return "Hệ thống";
        }

        private static string FormatDateTime(DateTime value)
        {
            return SystemTimeFormatter.FormatVietnamTime(value);
        }

        private sealed class DashboardData
        {
            public int CourseCount { get; set; }
            public int ExamCount { get; set; }
            public bool HasOpenOrUpcomingExams { get; set; }
            public int NotificationCount { get; set; }
            public double? AverageScore { get; set; }
            public List<NotificationModel> Notifications { get; set; } = new();
            public List<ActivityRow> Activities { get; set; } = new();
            public string ErrorMessage { get; set; } = string.Empty;

            public static DashboardData Error(string message)
            {
                return new DashboardData { ErrorMessage = message };
            }
        }

        private sealed class ActivityRow
        {
            public DateTime CreatedAt { get; set; }
            public string Title { get; set; } = string.Empty;
            public Color Accent { get; set; }
        }
    }
}
