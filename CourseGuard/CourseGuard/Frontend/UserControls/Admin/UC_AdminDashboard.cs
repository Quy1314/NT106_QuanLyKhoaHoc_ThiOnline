using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public partial class UC_AdminDashboard : UserControl
    {
        public event Action<string>? QuickActionRequested;

        private readonly UserController _userController;
        private DataGridView? _requestsGrid;
        private ListBox? _activitiesList;
        private Label? _totalUsersValueLabel;
        private Label? _activeCoursesValueLabel;
        private Label? _pendingRequestsValueLabel;
        private Label? _todayLoginsValueLabel;

        public UC_AdminDashboard()
        {
            InitializeComponent();
            _userController = new UserController(new CourseGuardDbContext(""));
            BuildDashboardFromTemplate();
            _ = LoadDashboardDataAsync();
        }

        private void BuildDashboardFromTemplate()
        {
            Controls.Clear();
            BackColor = Color.FromArgb(248, 250, 252);

            var root = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BackColor,
                Padding = new Padding(20)
            };

            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.Transparent
            };

            var title = new Label
            {
                Text = "Admin Dashboard",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Location = new Point(4, 0)
            };

            var subtitle = new Label
            {
                Text = "Monitor and manage your enterprise course system",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(6, 36)
            };

            titlePanel.Controls.Add(title);
            titlePanel.Controls.Add(subtitle);

            var kpiPanel = CreateKpiPanel();
            kpiPanel.Dock = DockStyle.Top;
            kpiPanel.Height = 140;

            var contentPanel = CreateContentPanel();
            contentPanel.Dock = DockStyle.Fill;

            root.Controls.Add(contentPanel);
            root.Controls.Add(kpiPanel);
            root.Controls.Add(titlePanel);
            Controls.Add(root);
        }

        private Control CreateKpiPanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            panel.ColumnStyles.Clear();
            for (int i = 0; i < 4; i++)
            {
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }

            panel.Controls.Add(CreateKpiCard("Total Users", "2,450", "+12.5%", Color.FromArgb(37, 99, 235)), 0, 0);
            panel.Controls.Add(CreateKpiCard("Active Courses", "87", "+8.2%", Color.FromArgb(34, 211, 238)), 1, 0);
            panel.Controls.Add(CreateKpiCard("Pending Requests", "24", "-3.1%", Color.FromArgb(245, 158, 11)), 2, 0);
            panel.Controls.Add(CreateKpiCard("Today Logins", "1,234", "+18.7%", Color.FromArgb(34, 197, 94)), 3, 0);
            return panel;
        }

        private Control CreateKpiCard(string label, string value, string delta, Color accent)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                BackColor = Color.White
            };
            card.Paint += (_, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240));
                e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            };

            var title = new Label { Text = label, AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI", 10F), Location = new Point(16, 16) };
            var number = new Label { Text = value, AutoSize = true, ForeColor = Color.FromArgb(15, 23, 42), Font = new Font("Segoe UI", 22F, FontStyle.Bold), Location = new Point(14, 42) };
            var chip = new Label { Text = delta, AutoSize = true, ForeColor = accent, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(16, 96) };

            if (label == "Total Users") _totalUsersValueLabel = number;
            else if (label == "Active Courses") _activeCoursesValueLabel = number;
            else if (label == "Pending Requests") _pendingRequestsValueLabel = number;
            else if (label == "Today Logins") _todayLoginsValueLabel = number;

            card.Controls.Add(title);
            card.Controls.Add(number);
            card.Controls.Add(chip);
            return card;
        }

        private Control CreateContentPanel()
        {
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            container.ColumnStyles.Clear();
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            container.Controls.Add(CreateRequestsTable(), 0, 0);
            container.Controls.Add(CreateActivitiesTable(), 1, 0);
            container.Controls.Add(CreateQuickActions(), 2, 0);
            return container;
        }

        private Control CreateRequestsTable()
        {
            var panel = CreateCardPanel("Recent User Requests");
            _requestsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                EnableHeadersVisualStyles = false
            };
            _requestsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _requestsGrid.ColumnHeadersHeight = 30;
            _requestsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _requestsGrid.RowTemplate.Height = 30;
            _requestsGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            _requestsGrid.Columns.Add("Username", "User");
            _requestsGrid.Columns.Add("Type", "Type");
            _requestsGrid.Columns.Add("Status", "Status");
            _requestsGrid.Columns.Add("Info", "Info");
            _requestsGrid.Rows.Add("Loading...", "Registration", "PENDING", "Please wait");
            AddCardContent(panel, _requestsGrid);

            _ = LoadPendingRequestsAsync();
            return panel;
        }

        private async Task LoadPendingRequestsAsync()
        {
            try
            {
                var users = await Task.Run(() => _userController.GetPendingRequests());
                var pendingOnly = users
                    .Where(u => string.Equals(u.Status, "PENDING", StringComparison.OrdinalIgnoreCase))
                    .Take(100)
                    .ToList();

                if (IsDisposed || Disposing || _requestsGrid == null) return;

                _requestsGrid.Rows.Clear();
                foreach (UserModel user in pendingOnly)
                {
                    _requestsGrid.Rows.Add(
                        user.Username,
                        "Registration",
                        user.Status,
                        string.IsNullOrWhiteSpace(user.Email) ? "-" : user.Email
                    );
                }

                if (pendingOnly.Count == 0)
                {
                    _requestsGrid.Rows.Add("-", "Registration", "PENDING", "No pending users");
                }
            }
            catch (Exception ex)
            {
                if (IsDisposed || Disposing || _requestsGrid == null) return;
                _requestsGrid.Rows.Clear();
                _requestsGrid.Rows.Add("Error", "Registration", "PENDING", ex.Message);
            }
        }

        private Control CreateActivitiesTable()
        {
            var panel = CreateCardPanel("Recent Activities");
            var list = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9F),
                IntegralHeight = false,
                ItemHeight = 22
            };
            list.Items.Add("Loading activities...");
            _activitiesList = list;
            AddCardContent(panel, list);
            return panel;
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var metricsTask = Task.Run(() => _userController.GetAdminDashboardMetrics());
                var activitiesTask = Task.Run(() => _userController.GetRecentAuthActivities(20));
                await Task.WhenAll(metricsTask, activitiesTask);

                if (IsDisposed || Disposing) return;

                var metrics = await metricsTask;
                _totalUsersValueLabel?.SetTextSafely(metrics.TotalUsers.ToString("N0"));
                _activeCoursesValueLabel?.SetTextSafely(metrics.ActiveCourses.ToString("N0"));
                _pendingRequestsValueLabel?.SetTextSafely(metrics.PendingRequests.ToString("N0"));
                _todayLoginsValueLabel?.SetTextSafely(metrics.TodayLogins.ToString("N0"));

                var activities = await activitiesTask;
                if (_activitiesList != null)
                {
                    _activitiesList.Items.Clear();
                    foreach (var activity in activities)
                    {
                        string text =
                            $"{activity.Username} - {activity.Action} | IP: {activity.IpAddress} | {activity.CreatedAt:dd/MM/yyyy HH:mm:ss}";
                        _activitiesList.Items.Add(text);
                    }

                    if (activities.Count == 0)
                    {
                        _activitiesList.Items.Add("No activity logs found.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (_activitiesList != null)
                {
                    _activitiesList.Items.Clear();
                    _activitiesList.Items.Add("Failed to load activities: " + ex.Message);
                }
            }
        }

        private Control CreateQuickActions()
        {
            var panel = CreateCardPanel("Quick Actions");
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            actions.Controls.Add(CreateActionButton("Add User", Color.FromArgb(37, 99, 235), "USERS"));
            actions.Controls.Add(CreateActionButton("Create Course", Color.FromArgb(34, 211, 238), "COURSES"));
            actions.Controls.Add(CreateActionButton("Export Report", Color.FromArgb(139, 92, 246), "REPORTS"));
            actions.Controls.Add(CreateActionButton("View Audit", Color.FromArgb(245, 158, 11), "AUDIT"));
            AddCardContent(panel, actions);
            return panel;
        }

        private Button CreateActionButton(string text, Color color, string target)
        {
            var button = new Button
            {
                Text = text,
                Width = 230,
                Height = 42,
                BackColor = Color.White,
                ForeColor = color,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            button.FlatAppearance.BorderSize = 1;
            button.Click += (_, _) => QuickActionRequested?.Invoke(target);
            return button;
        }

        private static Panel CreateCardPanel(string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            var label = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            var contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 0)
            };

            panel.Paint += (_, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240));
                e.Graphics.DrawRectangle(p, 0, 0, panel.Width - 1, panel.Height - 1);
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

    internal static class LabelExtensions
    {
        public static void SetTextSafely(this Label label, string text)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.Text = text));
            }
            else
            {
                label.Text = text;
            }
        }
    }
}
