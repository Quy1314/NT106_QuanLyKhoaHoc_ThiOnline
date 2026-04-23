//UC_CourseList.cs

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_CourseList : UserControl
    {
        public UC_CourseList()
        {
            InitializeComponent();
            ApplyAcademicStyle();
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

        private void ApplyAcademicStyle()
        {
            BackColor = AcademicTheme.AppBackground;
            btnSearch.BackColor = AcademicTheme.Primary;
            btnSearch.ForeColor = Color.White;
            btnJoin.BackColor = AcademicTheme.Primary;
            btnJoin.ForeColor = Color.White;
            btnViewDetails.BackColor = AcademicTheme.Surface;
            btnViewDetails.ForeColor = AcademicTheme.TextSecondary;
            btnViewDetails.FlatAppearance.BorderColor = AcademicTheme.BorderSoft;
            btnViewDetails.FlatAppearance.BorderSize = 1;
            AcademicTheme.StyleGrid(dgvCourses);
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