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
        private Dictionary<Button, Func<UserControl>> _nav;
        private CourseGuard.Backend.Models.UserModel currentUser;

        public StudentDashboard(CourseGuard.Backend.Models.UserModel user)
        {
            currentUser = user;
            InitializeComponent();
            ApplyAdminLikeTheme();
            SetupButtonEvents();
            InitializeNavigation();

            // Bo góc tất cả buttons
            RoundedButtonHelper.Apply(12, btnDashboard, btnCourses, btnExam,
                btnResult, btnSchedule, btnChat, btnNotify, btnProfile, btnLogout);

            // Logout handler
            btnLogout.Click += (s, e) => 
            { 
                var authService = new CourseGuard.Backend.Controllers.AuthController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
                authService.Logout(currentUser?.Id, currentUser?.Username ?? string.Empty);
                this.DialogResult = DialogResult.OK; // Assuming Parent form handles return to Login
                this.Close(); 
            };

            SetActiveButton(btnDashboard);
            LoadUI(_nav[btnDashboard]());
        }

        private void ApplyAdminLikeTheme()
        {
            BackColor = Color.FromArgb(248, 250, 252);
            sidebar.BackColor = Color.White;
            mainboard.BackColor = Color.FromArgb(248, 250, 252);

            Button[] menuButtons = { btnDashboard, btnCourses, btnExam, btnResult, btnSchedule, btnChat, btnNotify, btnProfile };
            foreach (var btn in menuButtons)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.FromArgb(100, 116, 139);
                btn.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(16, 0, 0, 0);
            }

            btnLogout.BackColor = Color.FromArgb(239, 68, 68);
            btnLogout.ForeColor = Color.White;
            btnLogout.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        private void SetupButtonEvents()
        {
            // Gán sự kiện click và hover cho tất cả sidebar buttons (trừ Logout)
            Button[] sidebarButtons = { btnDashboard, btnCourses, btnExam, btnResult, btnSchedule, btnChat, btnNotify, btnProfile };
            Color activeColor = Color.FromArgb(37, 99, 235);

            foreach (var btn in sidebarButtons)
            {
                btn.Click += Sidebar_Click;

                btn.MouseEnter += (s, e) =>
                {
                    if (btn.BackColor != activeColor)
                        btn.BackColor = Color.FromArgb(235, 240, 252);
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
            _nav = new Dictionary<Button, Func<UserControl>>
            {
                { btnDashboard, () => new UC_StudentDashboard() },
                { btnCourses, () => new UC_CourseList() },
                { btnExam, () => new UC_TakeExam() },
                { btnResult, () => new UC_Result() },
                { btnSchedule, () => new UC_Schedule() },
                { btnChat, () => new UC_Chat() },
                { btnNotify, () => new UC_Notification() },
                { btnProfile, () => new UC_Profile() }
            };
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
                    key.BackColor = Color.FromArgb(37, 99, 235);
                    key.ForeColor = Color.White;
                    key.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
                else
                {
                    key.BackColor = Color.Transparent;
                    key.ForeColor = Color.FromArgb(100, 116, 139);
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
    }
}