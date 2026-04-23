/*
 * AdminDashboard.cs
 * 
 * Layer: Presentation (Forms)
 * Vai trò: Form chính quản lý (Container), chứa menu và các UserControl con (Quản lý User, Khóa học, Báo cáo).
 * Phụ thuộc: Các UserControl con.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.UserControls.Admin;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Admin
{
    public partial class AdminDashboard : Form
    {
        private Dictionary<Button, Func<UserControl>> _nav;
        private UserModel? currentUser;

        public AdminDashboard()
        {
            InitializeComponent();
            ApplyTheme();
            SetupButtonEvents();
            InitializeNavigation();
            // Bo góc tất cả buttons
            RoundedButtonHelper.Apply(12, btnDashboard, btnUsers, btnCourses,
                btnReports, btnDeviceMonitoring, btnAuditLogs, btnSettings, btnLogout);

            // Logout handler
            btnLogout.Click += (s, e) =>
            {
                var authService = new CourseGuard.Backend.Controllers.AuthController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
                authService.Logout(currentUser?.Id, currentUser?.Username ?? string.Empty);
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            SetActiveButton(btnDashboard);
            LoadUI(_nav[btnDashboard]());
        }

        public AdminDashboard(UserModel user) : this()
        {
            this.currentUser = user;
            this.Text = $"Admin Dashboard - {user.Username}";
        }

        private void ApplyTheme()
        {
            BackColor = AcademicTheme.AppBackground;
            sidebar.BackColor = AcademicTheme.Surface;
            mainboard.BackColor = AcademicTheme.AppBackground;

            Button[] menuButtons = { btnDashboard, btnUsers, btnCourses, btnReports, btnDeviceMonitoring, btnAuditLogs, btnSettings };
            foreach (var btn in menuButtons)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;
                btn.ForeColor = AcademicTheme.TextSecondary;
                btn.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(16, 0, 0, 0);
            }

            btnLogout.BackColor = Color.FromArgb(220, 38, 38);
            btnLogout.ForeColor = Color.White;
            btnLogout.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        private void SetupButtonEvents()
        {
            // Gán sự kiện click và hover cho tất cả sidebar buttons (trừ Logout)
            Button[] sidebarButtons = { btnDashboard, btnUsers, btnCourses, btnReports, btnDeviceMonitoring, btnAuditLogs, btnSettings };
            Color activeColor = AcademicTheme.Primary;

            foreach (var btn in sidebarButtons)
            {
                btn.Click += Sidebar_Click;

                btn.MouseEnter += (s, e) =>
                {
                    if (btn.BackColor != activeColor)
                        btn.BackColor = Color.FromArgb(222, 224, 255);
                };

                btn.MouseLeave += (s, e) =>
                {
                    if (btn.BackColor != activeColor)
                        btn.BackColor = Color.Transparent;
                };
            }
        }

        private void InitializeNavigation()
        {
            var adminOverview = new Func<UserControl>(() =>
            {
                var uc = new UC_AdminDashboard();
                uc.QuickActionRequested += AdminOverview_QuickActionRequested;
                return uc;
            });

            _nav = new Dictionary<Button, Func<UserControl>>
            {
                { btnDashboard, adminOverview },
                { btnUsers, () => new UC_UsersManage() },
                { btnCourses, () => new UC_CoursesManage() },
                { btnReports, () => new UC_AdminReports() },
                { btnDeviceMonitoring, () => CreateEmptyView("Device Monitoring") },
                { btnAuditLogs, () => CreateEmptyView("Audit Logs") },
                { btnSettings, () => CreateEmptyView("Settings") }
            };
        }

        private void AdminOverview_QuickActionRequested(string target)
        {
            Button? targetBtn = null;
            switch (target)
            {
                case "USERS": targetBtn = btnUsers; break;
                case "COURSES": targetBtn = btnCourses; break;
                case "REPORTS": targetBtn = btnReports; break;
                case "AUDIT": targetBtn = btnAuditLogs; break;
            }
            if (targetBtn != null && _nav.ContainsKey(targetBtn))
            {
                SetActiveButton(targetBtn);
                LoadUI(_nav[targetBtn]());
            }
        }

        private void Sidebar_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && _nav.ContainsKey(btn))
            {
                SetActiveButton(btn);
                LoadUI(_nav[btn]());
            }
        }

        private void SetActiveButton(Button activeBtn)
        {
            foreach (var key in _nav.Keys)
            {
                if (key == activeBtn)
                {
                    key.BackColor = AcademicTheme.Primary;
                    key.ForeColor = Color.White;
                    key.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
                else
                {
                    key.BackColor = Color.Transparent;
                    key.ForeColor = AcademicTheme.TextSecondary;
                    key.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
            }
        }

        private void LoadUI(UserControl uc)
        {
            // Dispose UC cũ để tránh memory leak
            foreach (Control ctrl in mainboard.Controls)
            {
                ctrl.Dispose();
            }
            mainboard.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            mainboard.Controls.Add(uc);
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
