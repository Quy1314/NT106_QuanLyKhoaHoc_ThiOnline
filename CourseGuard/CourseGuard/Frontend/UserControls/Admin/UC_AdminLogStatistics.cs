using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public class UC_AdminLogStatistics : UserControl
    {
        private readonly DashboardController _dashboardController;
        private Label? _totalAccountsValue;
        private Label? _activeAccountsValue;
        private Label? _pendingAccountsValue;
        private Label? _teacherAccountsValue;
        private Label? _studentAccountsValue;
        private DataGridView? _loginStatsGrid;
        private DataGridView? _courseStatsGrid;

        public UC_AdminLogStatistics()
        {
            _dashboardController = new DashboardController(new CourseGuardDbContext(string.Empty));
            BuildLayout();
            _ = LoadStatisticsAsync();
        }

        private void BuildLayout()
        {
            Controls.Clear();
            BackColor = AcademicTheme.AppBackground;

            var root = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AcademicTheme.AppBackground,
                Padding = new Padding(18),
                AutoScroll = true
            };

            var title = new Label
            {
                Dock = DockStyle.Top,
                Height = 44,
                Text = "Thống kê nhật ký Admin",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = AcademicTheme.TextPrimary
            };

            var summarySection = CreateAccountSummarySection();
            summarySection.Dock = DockStyle.Top;
            summarySection.Height = 165;

            var loginSection = CreateLoginFrequencySection();
            loginSection.Dock = DockStyle.Top;
            loginSection.Height = 280;

            var courseSection = CreateCourseListSection();
            courseSection.Dock = DockStyle.Fill;

            root.Controls.Add(courseSection);
            root.Controls.Add(loginSection);
            root.Controls.Add(summarySection);
            root.Controls.Add(title);
            Controls.Add(root);
        }

        private Control CreateAccountSummarySection()
        {
            var panel = CreateCardPanel("Phần 1 - Tổng quan tài khoản");

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1
            };
            for (int i = 0; i < 5; i++)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            }

            layout.Controls.Add(CreateMetricCard("Tổng", out _totalAccountsValue), 0, 0);
            layout.Controls.Add(CreateMetricCard("Hoạt động", out _activeAccountsValue), 1, 0);
            layout.Controls.Add(CreateMetricCard("Chờ duyệt", out _pendingAccountsValue), 2, 0);
            layout.Controls.Add(CreateMetricCard("Giảng viên", out _teacherAccountsValue), 3, 0);
            layout.Controls.Add(CreateMetricCard("Học viên", out _studentAccountsValue), 4, 0);

            AddCardContent(panel, layout);
            return panel;
        }

        private Control CreateLoginFrequencySection()
        {
            var panel = CreateCardPanel("Phần 2 - Tần suất đăng nhập (14 ngày gần nhất)");
            _loginStatsGrid = CreateGrid();
            _loginStatsGrid.Columns.Add("LoginDate", "Ngày");
            _loginStatsGrid.Columns.Add("LoginCount", "Số lượt đăng nhập");
            AddCardContent(panel, _loginStatsGrid);
            return panel;
        }

        private Control CreateCourseListSection()
        {
            var panel = CreateCardPanel("Phần 3 - Thống kê danh sách khóa học");
            _courseStatsGrid = CreateGrid();
            _courseStatsGrid.Columns.Add("CourseId", "Mã khóa học");
            _courseStatsGrid.Columns.Add("CourseName", "Tên khóa học");
            _courseStatsGrid.Columns.Add("TeacherName", "Giảng viên");
            _courseStatsGrid.Columns.Add("Status", "Trạng thái");
            _courseStatsGrid.Columns.Add("EnrollmentCount", "Số lượt ghi danh");
            AddCardContent(panel, _courseStatsGrid);
            return panel;
        }

        private static Control CreateMetricCard(string title, out Label valueLabel)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                Padding = new Padding(12),
                BackColor = AcademicTheme.Surface
            };
            AcademicTheme.StyleCard(card);

            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AcademicTheme.TextSecondary
            };

            valueLabel = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                ForeColor = AcademicTheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLabel);
            return card;
        }

        private static DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = AcademicTheme.Surface,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                EnableHeadersVisualStyles = false
            };

            AcademicTheme.StyleGrid(grid);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 30;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 30;

            return grid;
        }

        private async Task LoadStatisticsAsync()
        {
            this.ShowSkeleton(SkeletonType.DashboardOverview);
            try
            {
                var summaryTask = Task.Run(() => _dashboardController.GetAccountSummaryStatistics());
                var frequencyTask = Task.Run(() => _dashboardController.GetLoginFrequencyStatistics(14));
                var courseTask = Task.Run(() => _dashboardController.GetCourseListStatistics(100));

                await Task.WhenAll(summaryTask, frequencyTask, courseTask);
                if (IsDisposed || Disposing)
                {
                    return;
                }

                BindAccountSummary(await summaryTask);
                BindLoginFrequency(await frequencyTask);
                BindCourseList(await courseTask);
            }
            catch (Exception ex)
            {
                if (_loginStatsGrid != null)
                {
                    _loginStatsGrid.Rows.Clear();
                    _loginStatsGrid.Rows.Add("Lỗi", ex.Message);
                }

                if (_courseStatsGrid != null)
                {
                    _courseStatsGrid.Rows.Clear();
                    _courseStatsGrid.Rows.Add("Lỗi", ex.Message, "-", "-", "-");
                }
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void BindAccountSummary(AccountSummaryModel summary)
        {
            _totalAccountsValue?.SetTextSafely(summary.TotalAccounts.ToString("N0"));
            _activeAccountsValue?.SetTextSafely(summary.ActiveAccounts.ToString("N0"));
            _pendingAccountsValue?.SetTextSafely(summary.PendingAccounts.ToString("N0"));
            _teacherAccountsValue?.SetTextSafely(summary.TeacherAccounts.ToString("N0"));
            _studentAccountsValue?.SetTextSafely(summary.StudentAccounts.ToString("N0"));
        }

        private void BindLoginFrequency(System.Collections.Generic.List<LoginFrequencyModel> frequencyRows)
        {
            if (_loginStatsGrid == null)
            {
                return;
            }

            _loginStatsGrid.Rows.Clear();
            foreach (var row in frequencyRows)
            {
                _loginStatsGrid.Rows.Add(row.LoginDate.ToString("dd/MM/yyyy"), row.LoginCount);
            }

            if (frequencyRows.Count == 0)
            {
                _loginStatsGrid.Rows.Add("-", 0);
            }
        }

        private void BindCourseList(System.Collections.Generic.List<CourseListItemModel> courseRows)
        {
            if (_courseStatsGrid == null)
            {
                return;
            }

            _courseStatsGrid.Rows.Clear();
            foreach (var row in courseRows)
            {
                _courseStatsGrid.Rows.Add(
                    row.CourseId,
                    row.CourseName,
                    row.TeacherName,
                    row.Status,
                    row.EnrollmentCount);
            }

            if (courseRows.Count == 0)
            {
                _courseStatsGrid.Rows.Add("-", "Không có khóa học", "-", "-", 0);
            }
        }

        private static Panel CreateCardPanel(string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                BackColor = AcademicTheme.Surface,
                Padding = new Padding(12)
            };
            AcademicTheme.StyleCard(panel);

            var label = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AcademicTheme.TextPrimary
            };

            var contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AcademicTheme.Surface,
                Padding = new Padding(0, 4, 0, 0)
            };

            panel.Controls.Add(contentHost);
            panel.Controls.Add(label);
            panel.Tag = contentHost;
            return panel;
        }

        private static void AddCardContent(Panel cardPanel, Control content)
        {
            if (cardPanel.Tag is Panel contentHost)
            {
                contentHost.Controls.Add(content);
                return;
            }

            cardPanel.Controls.Add(content);
        }
    }
}
