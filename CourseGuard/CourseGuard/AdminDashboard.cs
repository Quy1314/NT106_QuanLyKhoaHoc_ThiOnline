using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CourseGuard
{
    public partial class AdminDashboard : Form
    {
        private UserModel? currentUser;

        public AdminDashboard()
        {
            InitializeComponent();
        }

        public AdminDashboard(UserModel user) : this()
        {
            this.currentUser = user;
            // Additional setup with user data if needed
            this.Text = $"Admin Dashboard - {user.Username}";
        }
    }
}
