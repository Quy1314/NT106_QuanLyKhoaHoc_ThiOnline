using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CourseGuard.Presentation.UserControls.Student
{
    public partial class UC_StudentDashboard : UserControl
    {
        public UC_StudentDashboard()
        {
            InitializeComponent();
            LoadDummyData();
        }

        private void LoadDummyData()
        {
            // Dummy table for DataGridView
            DataTable dt = new DataTable();
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Tiêu đề", typeof(string));
            dt.Columns.Add("Khóa học", typeof(string));

            dt.Rows.Add("02/04/2026", "Bài tập mới: OOP Cơ bản", "Lập trình C#");
            dt.Rows.Add("01/04/2026", "Nhắc nhở: Lịch thi giữa kỳ", "Mạng máy tính");
            dt.Rows.Add("28/03/2026", "Cập nhật tài liệu mới", "Lập trình C#");

            dgvRecentNotices.DataSource = dt;
            dgvRecentNotices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}