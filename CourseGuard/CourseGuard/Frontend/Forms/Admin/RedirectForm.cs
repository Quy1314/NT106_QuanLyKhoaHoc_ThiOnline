using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Login;
using CourseGuard.Frontend.Forms.Student;
using CourseGuard.Frontend.Forms.Teacher;

namespace CourseGuard.Frontend.Forms.Admin
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

            bool isRunning = true;
            while (isRunning)
            {
                using (LoginPage login = new LoginPage())
                {
                    if (login.ShowDialog() == DialogResult.OK)
                    {
                        UserModel? user = login.CurrentUser;
                        if (user == null)
                        {
                            isRunning = false;
                            break;
                        }

                        Form? dashboard = null;

                        switch (user.Role)
                        {
                            case "ADMIN":
                                dashboard = new AdminDashboard(user);
                                break;
                            case "TEACHER":
                                dashboard = new TeacherDashboard(user);
                                break;
                            case "STUDENT":
                                dashboard = new StudentDashboard(user);
                                break;
                            default:
                                MessageBox.Show($"Quyền không xác định: {user.Role}");
                                break;
                        }

                        if (dashboard != null)
                        {
                            if (dashboard.ShowDialog() == DialogResult.OK)
                            {
                                // Người dùng bấm đăng xuất (Logout) trả về DialogResult.OK
                                // Tiếp tục vòng lặp để hiện lại Form Đăng nhập
                                continue;
                            }
                            else
                            {
                                // Người dùng tắt bằng nút X hoặc cách khác
                                isRunning = false;
                            }
                        }
                        else
                        {
                            isRunning = false;
                        }
                    }
                    else
                    {
                        // Người dùng tắt Form đăng nhập chữ không đăng nhập
                        isRunning = false;
                    }
                }
            }

            System.Windows.Forms.Application.Exit();
        }
    }
}
