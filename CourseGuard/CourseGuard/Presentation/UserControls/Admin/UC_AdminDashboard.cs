using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Infrastructure.Data;
using CourseGuard.Presentation.UserControls.Shared;

namespace CourseGuard.Presentation.UserControls.Admin
{
    public partial class UC_AdminDashboard : UC_Dashboard
    {
        private readonly CourseGuard.Application.Interfaces.IUserService _userService;

        public UC_AdminDashboard()
        {
            InitializeComponent();
            
            // Manual Injection
            var userRepository = new CourseGuard.Infrastructure.Data.Repositories.UserRepository();
            _userService = new CourseGuard.Application.Services.UserService(userRepository);

            LoadData(); // Load data on initialization
        }

        public override void LoadData()
        {
            try
            {
                var dashboardData = _userService.GetDashboardData();
                
                if (dataGridView1 != null)
                {
                    dataGridView1.DataSource = dashboardData;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu Dashboard: " + ex.Message);
            }
        }
    }
}
