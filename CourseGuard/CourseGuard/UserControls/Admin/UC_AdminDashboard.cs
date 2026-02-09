using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Data;
using CourseGuard.UserControls.shareUC;

namespace CourseGuard.UserControls.Admin
{
    public partial class UC_AdminDashboard : UC_Dashboard
    {
        public UC_AdminDashboard()
        {
            InitializeComponent();
            LoadData(); // Load data on initialization
        }

        public override void LoadData()
        {
            try
            {
                // Query detailed user info and their last active device session
                string query = @"
                    SELECT 
                        u.ID, 
                        u.USERNAME, 
                        u.FULL_NAME, 
                        u.EMAIL, 
                        r.NAME AS ROLE, 
                        u.STATUS,
                        (SELECT TOP 1 d.LAST_ACTIVE 
                         FROM DEVICES d 
                         WHERE d.USER_ID = u.ID 
                         ORDER BY d.LAST_ACTIVE DESC) AS LAST_LOGIN,
                         (SELECT TOP 1 d.IP_ADDRESS 
                         FROM DEVICES d 
                         WHERE d.USER_ID = u.ID 
                         ORDER BY d.LAST_ACTIVE DESC) AS LAST_IP
                    FROM USERS u
                    JOIN ROLES r ON u.ROLE_ID = r.ID
                    ORDER BY LAST_LOGIN DESC";

                DataTable dt = DatabaseAction.ExecuteQuery(query);
                
                if (dataGridView1 != null)
                {
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu Dashboard: " + ex.Message);
            }
        }
    }
}
