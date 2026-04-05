//UC_CourseList.cs

using System;
using System.Data;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Student
{
    public partial class UC_CourseList : UserControl
    {
        public UC_CourseList()
        {
            InitializeComponent();
            LoadDummyData();
            
            // Bo góc buttons
            RoundedButtonHelper.Apply(10, btnSearch, btnJoin, btnViewDetails);

            txtSearch.KeyDown += (s, e) => 
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnSearch.PerformClick();
                }
            };
        }
        
        private void LoadDummyData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Mã khóa", typeof(string));
            dt.Columns.Add("Tên khóa học", typeof(string));
            dt.Columns.Add("Giảng viên", typeof(string));
            dt.Columns.Add("Trạng thái", typeof(string));

            dt.Rows.Add("COURSE_01", "Lập trình C#", "Nguyễn Văn A", "Đã tham gia");
            dt.Rows.Add("COURSE_02", "Mạng máy tính", "Trần Thị B", "Đang chờ duyệt");
            dt.Rows.Add("COURSE_03", "Hệ cơ sở dữ liệu", "Lê Văn C", "Chưa tham gia");

            dgvCourses.DataSource = dt;
            dgvCourses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}