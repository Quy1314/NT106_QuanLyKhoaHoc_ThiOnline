using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.UserControls.Student;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.Forms.Student
{
    public partial class StudentDashboard : Form
    {
        private Panel sidebar;
        private Panel mainboard;

        private Dictionary<Button, Func<UserControl>> _nav;

        public StudentDashboard()
        {
            InitializeUI();
            InitializeNavigation();

            LoadUI(_nav[btnDashboard]());
        }

        // ===== BUTTON =====
        private Button btnDashboard, btnCourses, btnExam, btnResult, btnSchedule, btnChat, btnNotify;

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
            btnDashboard.BackColor = ColorPalette.DarkMode.Active;

            sidebar.Controls.AddRange(new Control[]
            {
                btnDashboard, btnCourses, btnExam,
                btnResult, btnSchedule, btnChat, btnNotify
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
                btn.BackColor = ColorPalette.DarkMode.Hover;
            };

            btn.MouseLeave += (s, e) =>
            {
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
                { btnNotify, () => new UC_Notification() }
            };
        }

        private void Sidebar_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && _nav.ContainsKey(btn))
            {
                LoadUI(_nav[btn]());
            }
        }

        private void LoadUI(UserControl uc)
        {
            mainboard.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            mainboard.Controls.Add(uc);
        }
    }
}