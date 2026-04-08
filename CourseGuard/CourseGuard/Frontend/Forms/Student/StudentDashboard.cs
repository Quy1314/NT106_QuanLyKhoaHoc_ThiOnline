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
        private Panel sidebar;
        private Panel mainboard;

        private Dictionary<Button, Func<UserControl>> _nav;
        private CourseGuard.Backend.Models.UserModel currentUser;

        public StudentDashboard(CourseGuard.Backend.Models.UserModel user)
        {
            currentUser = user;
            InitializeUI();
            InitializeNavigation();

            SetActiveButton(btnDashboard);
            LoadUI(_nav[btnDashboard]());
        }

        // ===== BUTTON =====
        private Button btnDashboard, btnCourses, btnExam, btnResult, btnSchedule, btnChat, btnNotify, btnProfile, btnLogout;

        private void InitializeUI()
        {
            this.Text = "Student Dashboard";
            this.WindowState = FormWindowState.Maximized;

            // ===== SIDEBAR =====
            sidebar = new Panel
            {
                Width = 200,
                Dock = DockStyle.Left,
                BackColor = ColorPalette.DarkMode.Secondary
            };

            // ===== MAINBOARD =====
            mainboard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorPalette.LightMode.Base
            };

            // ===== BUTTONS =====
            btnDashboard = CreateButton("Dashboard", 0);
            btnCourses = CreateButton("Courses", 1);
            btnExam = CreateButton("Take Exam", 2);
            btnResult = CreateButton("Result", 3);
            btnSchedule = CreateButton("Schedule", 4);
            btnChat = CreateButton("Chat", 5);
            btnNotify = CreateButton("Notification", 6);
            btnProfile = CreateButton("Profile", 7);
            
            btnLogout = new Button
            {
                Text = "Logout",
                Width = 200,
                Height = 50,
                Dock = DockStyle.Bottom,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69), // Red
                ForeColor = Color.White
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => 
            { 
                var authService = new CourseGuard.Backend.Controllers.AuthController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
                authService.Logout();
                this.DialogResult = DialogResult.OK; // Assuming Parent form handles return to Login
                this.Close(); 
            };

            // Bo góc tất cả buttons
            RoundedButtonHelper.Apply(12, btnDashboard, btnCourses, btnExam,
                btnResult, btnSchedule, btnChat, btnNotify, btnProfile, btnLogout);

            sidebar.Controls.AddRange(new Control[]
            {
                btnDashboard, btnCourses, btnExam,
                btnResult, btnSchedule, btnChat, btnNotify, btnProfile, btnLogout
            });

            this.Controls.Add(mainboard);
            this.Controls.Add(sidebar);
        }

        private Button CreateButton(string text, int index)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 200,
                Height = 50,
                Location = new Point(0, index * 50),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = ColorPalette.DarkMode.TextPrimary
            };

            btn.FlatAppearance.BorderSize = 0;
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

            return btn;
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