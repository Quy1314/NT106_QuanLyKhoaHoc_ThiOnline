/*
 * AdminDashboard.cs
 * 
 * Layer: Presentation (Forms)
 * Vai trò: Form chính quản lý (Container), chứa menu và các UserControl con (Quản lý User, Khóa học, Báo cáo).
 * Phụ thuộc: Các UserControl con.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.UserControls.Admin;

namespace CourseGuard.Frontend.Forms.Admin
{
    public partial class AdminDashboard : Form
    {
        private Dictionary<Button, UserControl> _navigationMap = new();
        private UserModel? currentUser;
        private Button? _btnDeviceMonitoring;
        private Button? _btnAuditLogs;
        private Button? _btnSettings;
        private TextBox? _txtSearch;
        private Label? _lblAdminIdentity;

        public AdminDashboard()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.Sizable;
            BuildAdminTemplateStyle();
            InitializeNavigation();
            // Default load (Dashboard)
            if (_navigationMap.TryGetValue(btn_Dashboard, out UserControl? defaultView))
            {
                LoadForm(defaultView);
            }
            btn_logout.Click += btn_logout_Click;
        }

        private void btn_logout_Click(object? sender, EventArgs e)
        {
            var authService = new CourseGuard.Backend.Controllers.AuthController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
            authService.Logout(currentUser?.Id, currentUser?.Username ?? string.Empty);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public AdminDashboard(UserModel user) : this()
        {
            this.currentUser = user;
            CustomizeUI();
            this.Text = $"Admin Dashboard - {user.Username}";
        }

        private void InitializeNavigation()
        {
            var adminOverview = new UC_AdminDashboard();
            adminOverview.QuickActionRequested += AdminOverview_QuickActionRequested;

            // Initialize mapping: Button -> UserControl
            _navigationMap = new Dictionary<Button, UserControl>
            {
                { btn_Dashboard, adminOverview },
                { btn_users, new UC_UsersManage() },
                { btn_Courses, new UC_CoursesManage() },
                { btn_Reports, new UC_AdminReports() }
            };

            if (_btnDeviceMonitoring != null) _navigationMap[_btnDeviceMonitoring] = CreateEmptyView("Device Monitoring");
            if (_btnAuditLogs != null) _navigationMap[_btnAuditLogs] = CreateEmptyView("Audit Logs");
            if (_btnSettings != null) _navigationMap[_btnSettings] = CreateEmptyView("Settings");
        }

        private void AdminOverview_QuickActionRequested(string target)
        {
            switch (target)
            {
                case "USERS":
                    NavigateToButton(btn_users);
                    break;
                case "COURSES":
                    NavigateToButton(btn_Courses);
                    break;
                case "REPORTS":
                    NavigateToButton(btn_Reports);
                    break;
                case "AUDIT":
                    if (_btnAuditLogs != null) NavigateToButton(_btnAuditLogs);
                    break;
            }
        }

        private void NavigateToButton(Button btn)
        {
            if (!_navigationMap.ContainsKey(btn)) return;
            SetActiveButton(btn);
            LoadForm(_navigationMap[btn]);
        }

        private void CustomizeUI()
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Default Active Button logic
            SetActiveButton(btn_Dashboard);
        }

        private void Sidebar_Btn_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && _navigationMap.ContainsKey(btn))
            {
                NavigateToButton(btn);
            }
        }

        private void SetActiveButton(Button activeBtn)
        {
            // Iterate through dictionary keys (Buttons)
            foreach (var btn in _navigationMap.Keys)
            {
                if (btn == activeBtn)
                {
                    // Active Style
                    btn.BackColor = Color.FromArgb(37, 99, 235); // Blue
                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                }
                else
                {
                    // Inactive Style
                    btn.BackColor = Color.Transparent; // Transparent
                    btn.ForeColor = Color.FromArgb(156, 163, 175); // Gray
                    btn.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
                }
            }
        }

        private void Mainboard_Paint(object sender, PaintEventArgs e)
        {

        }

        private void LoadForm(UserControl uc)
        {
            Mainboard.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            Mainboard.Controls.Add(uc);
        }

        private void BuildAdminTemplateStyle()
        {
            BackColor = Color.FromArgb(248, 250, 252);
            Mainboard.BackColor = Color.FromArgb(248, 250, 252);
            Sidebar.BackColor = Color.White;
            Sidebar.Width = 260;
            Header.BackColor = Color.White;
            Header.Height = 64;

            LOGO.Text = "CourseGuard";
            LOGO.ForeColor = Color.FromArgb(15, 23, 42);
            LOGO.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            LOGO.TextAlign = ContentAlignment.MiddleLeft;
            LOGO.Padding = new Padding(18, 0, 0, 0);
            LOGO.Height = 48;

            StyleMenuButton(btn_Dashboard, "Dashboard");
            StyleMenuButton(btn_users, "User Management");
            StyleMenuButton(btn_Courses, "Course Management");
            StyleMenuButton(btn_Reports, "Reports");

            _btnDeviceMonitoring = CreateMenuButton("Device Monitoring", 300);
            _btnAuditLogs = CreateMenuButton("Audit Logs", 348);
            _btnSettings = CreateMenuButton("Settings", 396);
            Sidebar.Controls.Add(_btnDeviceMonitoring);
            Sidebar.Controls.Add(_btnAuditLogs);
            Sidebar.Controls.Add(_btnSettings);

            btn_logout.Text = "Logout";
            btn_logout.BackColor = Color.FromArgb(239, 68, 68);
            btn_logout.ForeColor = Color.White;
            btn_logout.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn_logout.Height = 42;

            BuildHeaderContent();
        }

        private void BuildHeaderContent()
        {
            Header.Controls.Clear();

            _txtSearch = new TextBox
            {
                Location = new Point(24, 18),
                Width = 360,
                Height = 30,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Text = "Search users, courses, reports..."
            };
            _txtSearch.GotFocus += (_, _) =>
            {
                if (_txtSearch.Text == "Search users, courses, reports...") _txtSearch.Text = string.Empty;
            };
            _txtSearch.LostFocus += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text)) _txtSearch.Text = "Search users, courses, reports...";
            };

            _lblAdminIdentity = new Label
            {
                AutoSize = true,
                Text = "Admin User",
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _lblAdminIdentity.Location = new Point(Header.Width - 140, 22);
            _lblAdminIdentity.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            Header.Controls.Add(_txtSearch);
            Header.Controls.Add(_lblAdminIdentity);
        }

        private void StyleMenuButton(Button button, string text)
        {
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.Transparent;
            button.ForeColor = Color.FromArgb(100, 116, 139);
            button.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(16, 0, 0, 0);
            button.Width = 236;
            button.Height = 42;
        }

        private Button CreateMenuButton(string text, int top)
        {
            Button btn = new Button
            {
                Text = text,
                Name = $"btn_{text.Replace(" ", string.Empty)}",
                Left = 12,
                Top = top,
                Width = 236,
                Height = 42
            };
            StyleMenuButton(btn, text);
            btn.Click += Sidebar_Btn_Click;
            return btn;
        }

        private static UserControl CreateEmptyView(string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var label = new Label
            {
                Dock = DockStyle.Fill,
                Text = $"{title}\n(Chưa có logic)",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            panel.Controls.Add(label);
            return new UserControl { Dock = DockStyle.Fill, BackColor = Color.White, Controls = { panel } };
        }
    }
}
