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
            SetupButtonEvents();
            InitializeNavigation();

            // Bo góc tất cả buttons
            RoundedButtonHelper.Apply(12, btnDashboard, btnCourses, btnExam,
                btnResult, btnSchedule, btnChat, btnNotify, btnProfile, btnLogout);

            // Logout handler
            btnLogout.Click += (s, e) => 
            { 
                var authService = new CourseGuard.Backend.Controllers.AuthController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
                authService.Logout();
                this.DialogResult = DialogResult.OK; // Assuming Parent form handles return to Login
                this.Close(); 
            };

            SetActiveButton(btnDashboard);
            LoadUI(_nav[btnDashboard]());
        }

        private void SetupButtonEvents()
        {
            // Gán sự kiện click và hover cho tất cả sidebar buttons (trừ Logout)
            Button[] sidebarButtons = { btnDashboard, btnCourses, btnExam, btnResult, btnSchedule, btnChat, btnNotify, btnProfile };

            foreach (var btn in sidebarButtons)
            {
                btn.Click += Sidebar_Click;

                btn.MouseEnter += (s, e) =>
                {
                    if (btn.BackColor != ColorPalette.DarkMode.Active)
                        btn.BackColor = ColorPalette.DarkMode.Hover;
                };

                btn.MouseLeave += (s, e) =>
                {
                    if (btn.BackColor != ColorPalette.DarkMode.Active)
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
                    key.BackColor = ColorPalette.DarkMode.Active;
                    key.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
                else
                {
                    key.BackColor = Color.Transparent;
                    key.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
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