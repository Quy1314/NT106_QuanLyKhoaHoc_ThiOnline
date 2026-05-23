using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherOverview : UserControl
    {
        private readonly int _teacherId;
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly TeacherController _controller = new(new CourseGuardDbContext(""));
        private readonly NotificationRepository _notificationRepository = new();

        private TableLayoutPanel _rootGrid = null!;
        private TableLayoutPanel _statsGrid = null!;
        private TableLayoutPanel _contentGrid = null!;
        private RoundedPanel _noticePanel = null!;
        private RoundedPanel _activityPanel = null!;
        private RoundedPanel _noticeBody = null!;
        private DataGridView _notificationGrid = null!;
        private Label _noticeEmptyLabel = null!;
        private FlowLayoutPanel _activityList = null!;
        private StatCard _courseCard = null!;
        private StatCard _pendingCard = null!;
        private StatCard _studentCard = null!;
        private StatCard _examCard = null!;

        public UC_TeacherOverview(int teacherId)
        {
            _teacherId = teacherId;
            BuildLayout();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.StudentOverviewDashboard);
            try
            {
                OverviewData data = await Task.Run(LoadOverviewData);
                ApplyOverviewData(data);
            }
            catch (Exception ex)
            {
                ApplyOverviewData(OverviewData.Error("Không thể tải dữ liệu: " + ex.Message));
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private OverviewData LoadOverviewData()
        {
            TeacherDashboardSummaryModel summary = _controller.GetDashboardSummary(_teacherId);
            List<TeacherTeachingTaskModel> tasks = SafeList(() => _controller.GetTeachingTasks(_teacherId));
            List<RecentUserActivityModel> auditLogs = SafeList(() => _dbContext.GetRecentUserActivitiesByUser(_teacherId, 8));

            List<ActivityRow> activities = auditLogs
                .Select(ToActivityRow)
                .Concat(summary.RecentActivities.Select((text, index) => new ActivityRow
                {
                    Title = text,
                    TimeText = index == 0 ? "Mới nhất" : string.Empty,
                    Accent = AppColors.AccentBlue
                }))
                .Take(6)
                .ToList();

            return new OverviewData
            {
                Summary = summary,
                Tasks = tasks,
                Activities = activities
            };
        }

        private void BuildLayout()
        {
            SuspendLayout();
            Controls.Clear();
            BackColor = AppColors.BgBase;

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

            _courseCard = AddStatCard(0, "Khóa học", "0", "course", "Đang quản lý");
            _pendingCard = AddStatCard(1, "Chờ duyệt", "0", "notice", "Yêu cầu ghi danh");
            _studentCard = AddStatCard(2, "Học viên", "0", "user", "Đang học");
            _examCard = AddStatCard(3, "Kỳ thi", "0", "exam", "Đang mở");

            _contentGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppColors.BgBase
            };
            _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            _contentGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _noticePanel = CreatePanelCard();
            _noticePanel.Margin = new Padding(0, 0, 12, 0);
            BuildNotificationPanel();

            _activityPanel = CreatePanelCard();
            _activityPanel.Margin = new Padding(12, 0, 0, 0);
            BuildActivityPanel();

            _contentGrid.Controls.Add(_noticePanel, 0, 0);
            _contentGrid.Controls.Add(_activityPanel, 1, 0);

            _rootGrid.Controls.Add(TeacherTabChrome.CreateHeader(
                "Tổng quan giảng viên",
                "Theo dõi lớp đang quản lý, yêu cầu ghi danh, kỳ thi và thông báo mới."), 0, 0);
            _rootGrid.Controls.Add(_statsGrid, 0, 1);
            _rootGrid.Controls.Add(_contentGrid, 0, 2);
            Controls.Add(_rootGrid);

            Resize -= UC_TeacherOverview_Resize;
            Resize += UC_TeacherOverview_Resize;
            TeacherTabChrome.EnableNaturalFocusClear(this, _notificationGrid);
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

        private void BuildNotificationPanel()
        {
            _noticePanel.Padding = new Padding(18);
            var panelGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            panelGrid.Controls.Add(CreateCardTitle("Lịch dạy & việc cần xử lý"), 0, 0);

            _notificationGrid = new DataGridView { Dock = DockStyle.Fill, Margin = Padding.Empty };
            TeacherTabChrome.StyleGrid(_notificationGrid);

            _noticeBody = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FillColor = AppColors.IsDarkMode ? AppColors.BgCardHover : ColorTranslator.FromHtml("#F8FAFC"),
                BorderColor = AppColors.Border,
                CornerRadius = 12,
                Padding = new Padding(18)
            };
            _noticeEmptyLabel = new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ForeColor = AppColors.TextMuted,
                Font = AppFonts.Body,
                Text = "Không có lịch dạy hoặc việc cần xử lý gần đây.",
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                UseCompatibleTextRendering = false
            };

            _noticeBody.Controls.Add(_notificationGrid);
            _noticeBody.Controls.Add(_noticeEmptyLabel);
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
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            panelGrid.Controls.Add(CreateCardTitle("Hoạt động gần đây"), 0, 0);

            _activityList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = Padding.Empty
            };
            _activityList.Resize += (_, _) => ResizeActivityItems();
            panelGrid.Controls.Add(_activityList, 0, 1);
            _activityPanel.Controls.Add(panelGrid);
        }

        private void ApplyOverviewData(OverviewData data)
        {
            ApplyStatCard(_courseCard, data.Summary.TotalCourses, "Đang quản lý", data.Summary.TotalCourses > 0 ? StatCardStatusTone.Positive : StatCardStatusTone.Neutral);
            ApplyStatCard(_pendingCard, data.Summary.PendingEnrollments, "Yêu cầu ghi danh", data.Summary.PendingEnrollments > 0 ? StatCardStatusTone.Warning : StatCardStatusTone.Neutral);
            ApplyStatCard(_studentCard, data.Summary.TotalStudents, "Đang học", data.Summary.TotalStudents > 0 ? StatCardStatusTone.Positive : StatCardStatusTone.Neutral);
            ApplyStatCard(_examCard, data.Summary.ActiveExams, "Đang mở", data.Summary.ActiveExams > 0 ? StatCardStatusTone.Warning : StatCardStatusTone.Neutral);
            BindTeachingTasks(data);
            BindActivities(data);
            ApplyTheme();
        }

        private static void ApplyStatCard(StatCard card, int value, string status, StatCardStatusTone tone)
        {
            card.Value = value.ToString(CultureInfo.InvariantCulture);
            card.TrendPercent = status;
            card.StatusTone = tone;
            card.ShowStatusArrow = false;
            card.Caption = string.Empty;
            card.MiniChartValues = null;
        }

        private void BindTeachingTasks(OverviewData data)
        {
            if (!string.IsNullOrWhiteSpace(data.ErrorMessage))
            {
                ShowNotificationEmptyState(data.ErrorMessage, AppColors.Warning);
                return;
            }

            if (data.Tasks.Count == 0)
            {
                ShowNotificationEmptyState("Không có lịch dạy hoặc việc cần xử lý gần đây.", AppColors.TextMuted);
                return;
            }

            var table = new DataTable();
            table.Columns.Add("Thời gian", typeof(string));
            table.Columns.Add("Việc cần xử lý", typeof(string));
            table.Columns.Add("Nhóm", typeof(string));
            table.Columns.Add("Trạng thái", typeof(string));
            foreach (TeacherTeachingTaskModel task in data.Tasks)
                table.Rows.Add(task.DueAt.HasValue ? FormatDateTime(task.DueAt.Value) : string.Empty, task.Title, task.Category, task.StatusText);

            _notificationGrid.DataSource = table;
            _notificationGrid.Visible = true;
            _noticeEmptyLabel.Visible = false;
            _noticeBody.Padding = Padding.Empty;
            _noticeBody.FillColor = AppColors.BgCard;
            _noticeBody.BorderColor = Color.Transparent;
            _notificationGrid.ClearSelection();
            _notificationGrid.CurrentCell = null;
        }

        private void ShowNotificationEmptyState(string message, Color color)
        {
            _notificationGrid.DataSource = null;
            _notificationGrid.Visible = false;
            _noticeBody.Padding = new Padding(18);
            _noticeBody.FillColor = AppColors.IsDarkMode ? AppColors.BgCardHover : ColorTranslator.FromHtml("#F8FAFC");
            _noticeBody.BorderColor = AppColors.Border;
            _noticeEmptyLabel.Text = message;
            _noticeEmptyLabel.ForeColor = color;
            _noticeEmptyLabel.Visible = true;
            _noticeEmptyLabel.BringToFront();
            _noticeBody.Invalidate();
        }

        private void BindActivities(OverviewData data)
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
                    AddActivityItem(activity.Title, activity.TimeText, activity.Accent);
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
            item.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using SolidBrush dot = new SolidBrush(accent);
                e.Graphics.FillEllipse(dot, 2, 11, 10, 10);
            };

            item.Controls.Add(new Label
            {
                Text = title,
                Location = new Point(22, 4),
                Size = new Size(Math.Max(120, itemWidth - 34), 24),
                Font = AppFonts.Button,
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                UseCompatibleTextRendering = false
            });
            if (!string.IsNullOrWhiteSpace(time))
            {
                item.Controls.Add(new Label
                {
                    Text = time,
                    Location = new Point(22, 30),
                    Size = new Size(Math.Max(120, itemWidth - 34), 20),
                    Font = AppFonts.Caption,
                    ForeColor = AppColors.TextSecondary,
                    BackColor = Color.Transparent,
                    AutoEllipsis = true,
                    UseCompatibleTextRendering = false
                });
            }
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

        private void ApplyTheme()
        {
            BackColor = AppColors.BgBase;
            _rootGrid.BackColor = AppColors.BgBase;
            _statsGrid.BackColor = AppColors.BgBase;
            _contentGrid.BackColor = AppColors.BgBase;
            foreach (RoundedPanel panel in new[] { _noticePanel, _activityPanel })
            {
                panel.FillColor = AppColors.BgCard;
                panel.BorderColor = AppColors.Border;
                panel.Invalidate();
            }
            TeacherTabChrome.StyleGrid(_notificationGrid);
            AppColors.ApplyTheme(this);
            _notificationGrid.ClearSelection();
        }

        private void UC_TeacherOverview_Resize(object? sender, EventArgs e)
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
                grid.SetColumn(control, i % columns);
                grid.SetRow(control, i / columns);
            }

            grid.RowCount = columns == 2 ? 2 : 1;
            grid.RowStyles.Clear();
            for (int i = 0; i < grid.RowCount; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / grid.RowCount));
            grid.ResumeLayout(true);
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

        private static Label CreateCardTitle(string text)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = AppFonts.Semibold(12f),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                UseCompatibleTextRendering = false
            };
        }

        private static ActivityRow ToActivityRow(RecentUserActivityModel activity)
        {
            return new ActivityRow
            {
                Title = TranslateActivity(activity),
                TimeText = FormatDateTime(activity.CreatedAt),
                Accent = GetActivityAccent(activity.Action)
            };
        }

        private static string TranslateActivity(RecentUserActivityModel activity)
        {
            string title = (activity.Action ?? string.Empty).ToUpperInvariant() switch
            {
                "LOGIN" => "Đăng nhập hệ thống",
                "LOGOUT" => "Đăng xuất hệ thống",
                "CHANGE_PASSWORD" => "Đổi mật khẩu",
                "CHAT_USE" => "Trao đổi với học viên",
                _ => "Cập nhật hoạt động giảng dạy"
            };
            string details = CleanDetails(activity.Details);
            return string.IsNullOrWhiteSpace(details) ? title : $"{title} - {details}";
        }

        private static string CleanDetails(string? details)
        {
            if (string.IsNullOrWhiteSpace(details))
                return string.Empty;
            string value = details.Trim();
            return value.Length > 72 ? value[..72] + "..." : value;
        }

        private static Color GetActivityAccent(string? action)
        {
            return (action ?? string.Empty).ToUpperInvariant() switch
            {
                "LOGIN" or "CHANGE_PASSWORD" => AppColors.Success,
                "CHAT_USE" => AppColors.Warning,
                "LOGOUT" => AppColors.TextMuted,
                _ => AppColors.AccentBlue
            };
        }

        private static string InferNotificationSource(NotificationModel notification)
        {
            string text = $"{notification.Title} {notification.Content}".ToLowerInvariant();
            if (text.Contains("thi") || text.Contains("kiểm tra") || text.Contains("exam"))
                return "Kỳ thi";
            if (text.Contains("ghi danh") || text.Contains("học viên") || text.Contains("student"))
                return "Học viên";
            if (text.Contains("khóa") || text.Contains("lớp") || text.Contains("course"))
                return "Khóa học";
            if (text.Contains("tài liệu") || text.Contains("material"))
                return "Tài liệu";
            return "Hệ thống";
        }

        private static string FormatDateTime(DateTime value)
        {
            return value == DateTime.MinValue ? string.Empty : value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
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

        private sealed class OverviewData
        {
            public TeacherDashboardSummaryModel Summary { get; set; } = new();
            public List<TeacherTeachingTaskModel> Tasks { get; set; } = new();
            public List<ActivityRow> Activities { get; set; } = new();
            public string ErrorMessage { get; set; } = string.Empty;

            public static OverviewData Error(string message) => new() { ErrorMessage = message };
        }

        private sealed class ActivityRow
        {
            public string Title { get; set; } = string.Empty;
            public string TimeText { get; set; } = string.Empty;
            public Color Accent { get; set; }
        }
    }
}
