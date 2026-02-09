using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CourseGuard.Models;
using CourseGuard.UserControls.Admin;

namespace CourseGuard.Forms.Admin
{
    public partial class AdminDashboard : Form
    {
        private Dictionary<Button, UserControl> _navigationMap;
        private UserModel? currentUser;

        public AdminDashboard()
        {
            InitializeComponent();
            InitializeNavigation();
            // Default load (Dashboard)
            if (_navigationMap.ContainsKey(btn_Dashboard))
            {
                LoadForm(_navigationMap[btn_Dashboard]);
            }
        }

        public AdminDashboard(UserModel user) : this()
        {
            this.currentUser = user;
            CustomizeUI();
            this.Text = $"Admin Dashboard - {user.Username}";
        }

        private void InitializeNavigation()
        {
            // Initialize mapping: Button -> UserControl
            _navigationMap = new Dictionary<Button, UserControl>
            {
                { btn_Dashboard, new UC_AdminDashboard() },
                { btn_users, new UC_UsersManage() },
                { btn_Courses, new UC_CoursesManage() },
                { btn_Reports, new UC_AdminReports() }
            };
        }

        private void CustomizeUI()
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Default Active Button logic
            SetActiveButton(btn_Dashboard);
        }

        private void Sidebar_Btn_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && _navigationMap.ContainsKey(btn))
            {
                SetActiveButton(btn);
                LoadForm(_navigationMap[btn]);
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
    }
}
