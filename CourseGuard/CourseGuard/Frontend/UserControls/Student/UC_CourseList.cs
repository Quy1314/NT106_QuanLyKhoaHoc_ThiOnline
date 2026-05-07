//UC_CourseList.cs

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_CourseList : UserControl
    {
        private readonly AuthController _authController = new(new CourseGuardDbContext(""));
        private readonly CourseController _courseController = new(new CourseGuardDbContext(""));
        private readonly BindingSource _coursesBinding = new();

        public UC_CourseList()
        {
            InitializeComponent();
            ApplyAcademicStyle();
            LoadCoursesFromDb();
            
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

            btnJoin.Click += btnJoin_Click;
            btnSearch.Click += (_, _) => ApplySearchFilter();
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
        
        private void LoadCoursesFromDb()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            List<CourseModel> courses = _courseController.GetAllCourses();
            HashSet<int> enrolledCourseIds = _courseController.GetStudentEnrolledCourseIds(studentId);

            DataTable dt = new DataTable();
            dt.Columns.Add("CourseId", typeof(int));
            dt.Columns.Add("Mã khóa", typeof(string));
            dt.Columns.Add("Tên khóa học", typeof(string));
            dt.Columns.Add("Giảng viên", typeof(string));
            dt.Columns.Add("Trạng thái", typeof(string));

            foreach (CourseModel course in courses)
            {
                string status = enrolledCourseIds.Contains(course.Id) ? "Đã tham gia" : "Chưa tham gia";
                dt.Rows.Add(
                    course.Id,
                    $"COURSE_{course.Id:D3}",
                    course.Name,
                    string.IsNullOrWhiteSpace(course.TeacherName) ? "Không xác định" : course.TeacherName,
                    status);
            }

            _coursesBinding.DataSource = dt;
            dgvCourses.DataSource = _coursesBinding;
            if (dgvCourses.Columns.Contains("CourseId"))
            {
                dgvCourses.Columns["CourseId"].Visible = false;
            }
            dgvCourses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnJoin_Click(object? sender, EventArgs e)
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (studentId <= 0 || dgvCourses.CurrentRow == null)
            {
                MessageBox.Show("Không tìm thấy học viên hoặc khóa học được chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object? rawCourseId = dgvCourses.CurrentRow.Cells["CourseId"].Value;
            if (rawCourseId == null || rawCourseId == DBNull.Value)
            {
                MessageBox.Show("Không thể xác định khóa học để tham gia.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int courseId = Convert.ToInt32(rawCourseId);
            bool joined = _courseController.StudentJoinCourse(courseId, studentId);
            if (joined)
            {
                MessageBox.Show("Tham gia khóa học thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCoursesFromDb();
                return;
            }

            MessageBox.Show("Không thể tham gia khóa học (có thể bạn đã tham gia trước đó).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ApplySearchFilter()
        {
            if (_coursesBinding.DataSource is not DataTable dt)
            {
                return;
            }

            string keyword = txtSearch.Text.Trim().Replace("'", "''");
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _coursesBinding.RemoveFilter();
                return;
            }

            _coursesBinding.Filter = $"[Tên khóa học] LIKE '%{keyword}%' OR [Giảng viên] LIKE '%{keyword}%'";
        }
    }
}