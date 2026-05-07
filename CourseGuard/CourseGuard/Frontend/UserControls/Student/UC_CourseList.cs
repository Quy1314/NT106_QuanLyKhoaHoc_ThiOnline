// UC_CourseList.cs
// Trang "Duyệt khóa học mới" – hiển thị các khóa học ACTIVE mà sinh viên chưa đăng ký,
// cho phép tìm kiếm, xem chi tiết và gửi yêu cầu đăng ký (PENDING).

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
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
        private readonly CourseController _controller;
        private List<CourseModel> _allCourses = new();

        public UC_CourseList()
        {
            InitializeComponent();
            ApplyAcademicStyle();
            LoadCoursesFromDb();
            

            _controller = new CourseController(new CourseGuardDbContext(""));

            // Bo góc buttons
            RoundedButtonHelper.Apply(10, btnSearch, btnJoin, btnViewDetails, btnRefresh);

            // Events
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
            btnSearch.Click += (s, e) => ApplySearch();
            btnRefresh.Click += (s, e) => LoadAvailableCourses();
            btnJoin.Click += BtnJoin_Click;
            btnViewDetails.Click += BtnViewDetails_Click;
            dgvCourses.SelectionChanged += DgvCourses_SelectionChanged;

            // Style
            StyleDataGridView();

            // Load dữ liệu thực
            LoadAvailableCourses();
        }

        // ── Load khóa học khả dụng ───────────────────────────────────────
        private void LoadAvailableCourses()
        {
            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                if (studentId == 0)
                {
                    MessageBox.Show("Không xác định được tài khoản. Vui lòng đăng nhập lại.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _allCourses = _controller.GetAvailableCourses(studentId);
                txtSearch.Clear();
                BindToGrid(_allCourses);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Tìm kiếm theo tên/giảng viên ────────────────────────────────
        private void ApplySearch()
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                BindToGrid(_allCourses);
                return;
            }

            var filtered = _allCourses.Where(c =>
                c.Name.ToLower().Contains(keyword) ||
                c.TeacherName.ToLower().Contains(keyword) ||
                c.Description.ToLower().Contains(keyword)
            ).ToList();

            BindToGrid(filtered);
        }

        // ── Bind dữ liệu vào DataGridView ───────────────────────────────
        private void BindToGrid(List<CourseModel> courses)
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            List<CourseModel> courses = _courseController.GetAllCourses();
            HashSet<int> enrolledCourseIds = _courseController.GetStudentEnrolledCourseIds(studentId);

            DataTable dt = new DataTable();
            dt.Columns.Add("CourseId", typeof(int));
            dt.Columns.Add("Mã khóa", typeof(string));
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Tên khóa học", typeof(string));
            dt.Columns.Add("Giảng viên", typeof(string));
            dt.Columns.Add("Trạng thái", typeof(string));
            dt.Columns.Add("Bắt đầu", typeof(string));
            dt.Columns.Add("Kết thúc", typeof(string));

            foreach (CourseModel course in courses)
            {
                string status = enrolledCourseIds.Contains(course.Id) ? "Đã tham gia" : "Chưa tham gia";
                dt.Rows.Add(
                    course.Id,
                    $"COURSE_{course.Id:D3}",
                    course.Name,
                    string.IsNullOrWhiteSpace(course.TeacherName) ? "Không xác định" : course.TeacherName,
                    status);
            foreach (var c in courses)
            {
                dt.Rows.Add(
                    c.Id,
                    c.Name,
                    c.TeacherName,
                    c.Status,
                    c.StartDate != DateTime.MinValue ? c.StartDate.ToString("dd/MM/yyyy") : "N/A",
                    c.EndDate != DateTime.MinValue ? c.EndDate.ToString("dd/MM/yyyy") : "N/A"
                );
            }

            _coursesBinding.DataSource = dt;
            dgvCourses.DataSource = _coursesBinding;
            if (dgvCourses.Columns.Contains("CourseId"))
            {
                dgvCourses.Columns["CourseId"].Visible = false;
            }
            dgvCourses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Ẩn cột ID nội bộ
            if (dgvCourses.Columns.Contains("ID"))
                dgvCourses.Columns["ID"].Visible = false;

            ClearDetailPanel();
        }

        // ── Khi chọn dòng → hiện chi tiết ───────────────────────────────
        private void DgvCourses_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvCourses.CurrentRow == null || dgvCourses.CurrentRow.IsNewRow)
            {
                ClearDetailPanel();
                return;
            }

            int courseId = Convert.ToInt32(dgvCourses.CurrentRow.Cells["ID"].Value);
            var course = _allCourses.FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                ClearDetailPanel();
                return;
            }

            lblDetailName.Text = course.Name;
            lblDetailTeacher.Text = $"👤 Giảng viên: {course.TeacherName}";

            if (course.StartDate != DateTime.MinValue)
                lblDetailDates.Text = $"📅 {course.StartDate:dd/MM/yyyy} → {course.EndDate:dd/MM/yyyy}";
            else
                lblDetailDates.Text = "📅 Chưa xác định";

            // Đếm số sinh viên đã đăng ký
            try
            {
                int count = _controller.GetEnrolledCount(courseId);
                lblDetailStudents.Text = $"👥 Số sinh viên đã đăng ký: {count}";
            }
            catch
            {
                lblDetailStudents.Text = "👥 Không thể tải";
            }

            lblDetailDesc.Text = string.IsNullOrWhiteSpace(course.Description)
                ? "(Không có mô tả)"
                : course.Description;
        }

        private void ClearDetailPanel()
        {
            lblDetailName.Text = "Chọn khóa học để xem chi tiết";
            lblDetailTeacher.Text = "";
            lblDetailDates.Text = "";
            lblDetailStudents.Text = "";
            lblDetailDesc.Text = "";
        }

        // ── Xem chi tiết ─────────────────────────────────────────────────
        private void BtnViewDetails_Click(object? sender, EventArgs e)
        {
            if (dgvCourses.CurrentRow == null || dgvCourses.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Vui lòng chọn một khóa học.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int courseId = Convert.ToInt32(dgvCourses.CurrentRow.Cells["ID"].Value);
            var course = _allCourses.FirstOrDefault(c => c.Id == courseId);
            if (course == null) return;

            int enrolledCount = 0;
            try { enrolledCount = _controller.GetEnrolledCount(courseId); } catch { }

            string info = $"📚 {course.Name}\n\n" +
                          $"👤 Giảng viên: {course.TeacherName}\n" +
                          $"📅 Bắt đầu: {(course.StartDate != DateTime.MinValue ? course.StartDate.ToString("dd/MM/yyyy") : "N/A")}\n" +
                          $"📅 Kết thúc: {(course.EndDate != DateTime.MinValue ? course.EndDate.ToString("dd/MM/yyyy") : "N/A")}\n" +
                          $"👥 Số SV đã đăng ký: {enrolledCount}\n" +
                          $"📋 Trạng thái: {course.Status}\n\n" +
                          $"📖 Mô tả:\n{(string.IsNullOrWhiteSpace(course.Description) ? "(Không có)" : course.Description)}";

            MessageBox.Show(info, "Chi tiết khóa học", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Đăng ký tham gia khóa học ────────────────────────────────────
        private void BtnJoin_Click(object? sender, EventArgs e)
        {
            if (dgvCourses.CurrentRow == null || dgvCourses.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Vui lòng chọn một khóa học để đăng ký.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int courseId = Convert.ToInt32(dgvCourses.CurrentRow.Cells["ID"].Value);
            string courseName = dgvCourses.CurrentRow.Cells["Tên khóa học"].Value?.ToString() ?? "";

            var confirm = MessageBox.Show(
                $"Bạn muốn đăng ký tham gia khóa học \"{courseName}\"?\n\n" +
                $"Yêu cầu sẽ được gửi đến Admin/Giảng viên để duyệt.",
                "Xác nhận đăng ký",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            string result = _controller.RequestEnrollment(courseId, studentId);

            MessageBox.Show(result, "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Reload để loại bỏ khóa học đã đăng ký khỏi danh sách
            LoadAvailableCourses();
        }

        // ── Style DataGridView ───────────────────────────────────────────
        private void StyleDataGridView()
        {
            dgvCourses.EnableHeadersVisualStyles = false;
            dgvCourses.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 138);
            dgvCourses.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCourses.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvCourses.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCourses.ColumnHeadersHeight = 40;

            dgvCourses.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgvCourses.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvCourses.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 58, 138);
            dgvCourses.DefaultCellStyle.Padding = new Padding(5, 3, 5, 3);

            dgvCourses.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvCourses.GridColor = Color.FromArgb(226, 232, 240);
            dgvCourses.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
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