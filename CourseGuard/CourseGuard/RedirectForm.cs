using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CourseGuard
{
    public partial class RedirectForm : Form
    {
        public RedirectForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.Opacity = 0; // Completely invisible
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 1. Show Login
            using (LoginPage login = new LoginPage())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    // 2. Get User
                    UserModel? user = login.CurrentUser;
                    if (user == null) return;

                    // 3. Route to Dashboard
                    Form dashboard = null;

                    switch (user.Role)
                    {
                        case "ADMIN":
                            dashboard = new AdminDashboard(user);
                            break;
                        case "TEACHER":
                            // dashboard = new TeacherDashboard(user);
                            MessageBox.Show("Teacher Dashboard chưa được cài đặt.");
                            break;
                        case "STUDENT":
                            // dashboard = new StudentDashboard(user);
                            MessageBox.Show("Student Dashboard chưa được cài đặt.");
                            break;
                        default:
                            MessageBox.Show($"Quyền không xác định: {user.Role}");
                            break;
                    }

                    // 4. Show Dashboard if valid
                    if (dashboard != null)
                    {
                        dashboard.ShowDialog(); // Blocks until Dashboard closes
                    }
                }
            }

            // 5. Exit App
            Application.Exit();
        }
    }
}
