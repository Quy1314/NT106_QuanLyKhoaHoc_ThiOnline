using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Frontend.UserControls.Shared;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public partial class UC_AdminDashboard : UC_Dashboard
    {
        private readonly CourseGuard.Backend.Controllers.UserController _userService;

        public UC_AdminDashboard()
        {
            InitializeComponent();
            
            _userService = new CourseGuard.Backend.Controllers.UserController(new CourseGuardDbContext(""));

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
