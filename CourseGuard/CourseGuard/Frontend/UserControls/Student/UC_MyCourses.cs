// UC_MyCourses.cs
// Trang "Khóa học của tôi" – hiển thị danh sách khóa học sinh viên đã đăng ký,
// cho phép xem chi tiết và hủy/rút khỏi khóa học.

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
    public partial class UC_MyCourses : UserControl
    {
        private readonly CourseController _controller;
        private List<EnrollmentModel> _allEnrollments = new();

        public UC_MyCourses()
        {
            InitializeComponent();

            _controller = new CourseController(new CourseGuardDbContext(""));

            // Bo góc buttons
            RoundedButtonHelper.Apply(10, btnRefresh, btnViewDetail, btnDrop);

            // Events
            cboStatusFilter.SelectedIndex = 0;
            cboStatusFilter.SelectedIndexChanged += (s, e) => ApplyFilter();
            btnRefresh.Click += (s, e) => LoadEnrollments();
            btnDrop.Click += BtnDrop_Click;
            btnViewDetail.Click += BtnViewDetail_Click;
            dgvMyCourses.SelectionChanged += DgvMyCourses_SelectionChanged;

            // Styling DataGridView
            StyleDataGridView();

            // Load dữ liệu thực
            LoadEnrollments();
        }

        // ── Load dữ liệu từ DB ──────────────────────────────────────────
        private void LoadEnrollments()
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

                _allEnrollments = _controller.GetMyEnrollments(studentId);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Lọc theo trạng thái ──────────────────────────────────────────
        private void ApplyFilter()
        {
            var filtered = _allEnrollments.AsEnumerable();

            switch (cboStatusFilter.SelectedIndex)
            {
                case 1: // ACTIVE
                    filtered = filtered.Where(e => e.Status == "ACTIVE");
                    break;
                case 2: // PENDING
                    filtered = filtered.Where(e => e.Status == "PENDING");
                    break;
                case 3: // DROPPED
                    filtered = filtered.Where(e => e.Status == "DROPPED");
                    break;
                    // 0 = Tất cả → không lọc
            }

            BindToGrid(filtered.ToList());
        }

        // ── Bind dữ liệu vào DataGridView ───────────────────────────────
        private void BindToGrid(List<EnrollmentModel> enrollments)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("CourseId", typeof(int));
            dt.Columns.Add("Tên khóa học", typeof(string));
            dt.Columns.Add("Giảng viên", typeof(string));
            dt.Columns.Add("Trạng thái", typeof(string));
            dt.Columns.Add("Ngày tham gia", typeof(string));
            dt.Columns.Add("Thời gian", typeof(string));

            foreach (var e in enrollments)
            {
                string statusVi = e.Status switch
                {
                    "ACTIVE" => "✅ Đang học",
                    "PENDING" => "⏳ Chờ duyệt",
                    "DROPPED" => "❌ Đã rút",
                    "COMPLETED" => "🎓 Hoàn thành",
                    _ => e.Status
                };

                string dates = "";
                if (e.CourseStartDate != DateTime.MinValue && e.CourseEndDate != DateTime.MinValue)
                {
                    dates = $"{e.CourseStartDate:dd/MM/yyyy} → {e.CourseEndDate:dd/MM/yyyy}";
                }

                dt.Rows.Add(
                    e.Id,
                    e.CourseId,
                    e.CourseName,
                    e.TeacherName,
                    statusVi,
                    e.JoinedAt != DateTime.MinValue ? e.JoinedAt.ToString("dd/MM/yyyy HH:mm") : "",
                    dates
                );
            }

            dgvMyCourses.DataSource = dt;
            dgvMyCourses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Ẩn cột ID/CourseId nội bộ
            if (dgvMyCourses.Columns.Contains("ID"))
                dgvMyCourses.Columns["ID"].Visible = false;
            if (dgvMyCourses.Columns.Contains("CourseId"))
                dgvMyCourses.Columns["CourseId"].Visible = false;

            // Reset detail panel
            ClearDetailPanel();

            // Tô màu theo trạng thái
            ColorizeRows();
        }

        // ── Tô màu hàng theo trạng thái ─────────────────────────────────
        private void ColorizeRows()
        {
            foreach (DataGridViewRow row in dgvMyCourses.Rows)
            {
                if (row.IsNewRow) continue;
                string status = row.Cells["Trạng thái"].Value?.ToString() ?? "";

                if (status.Contains("Đang học"))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(5, 150, 105);
                }
                else if (status.Contains("Chờ duyệt"))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(217, 119, 6);
                }
                else if (status.Contains("Đã rút"))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                }
                else if (status.Contains("Hoàn thành"))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(37, 99, 235);
                }
            }
        }

        // ── Khi chọn dòng → hiện chi tiết ───────────────────────────────
        private void DgvMyCourses_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvMyCourses.CurrentRow == null || dgvMyCourses.CurrentRow.IsNewRow)
            {
                ClearDetailPanel();
                return;
            }

            int courseId = Convert.ToInt32(dgvMyCourses.CurrentRow.Cells["CourseId"].Value);
            var enrollment = _allEnrollments.FirstOrDefault(en => en.CourseId == courseId);

            if (enrollment == null)
            {
                ClearDetailPanel();
                return;
            }

            lblCourseName.Text = enrollment.CourseName;
            lblTeacher.Text = $"👤 Giảng viên: {enrollment.TeacherName}";

            if (enrollment.CourseStartDate != DateTime.MinValue)
                lblDates.Text = $"📅 {enrollment.CourseStartDate:dd/MM/yyyy} → {enrollment.CourseEndDate:dd/MM/yyyy}";
            else
                lblDates.Text = "📅 Chưa xác định";

            lblStatus.Text = enrollment.Status switch
            {
                "ACTIVE" => "● Đang học",
                "PENDING" => "● Chờ duyệt",
                "DROPPED" => "● Đã rút",
                "COMPLETED" => "● Hoàn thành",
                _ => enrollment.Status
            };
            lblStatus.ForeColor = enrollment.Status switch
            {
                "ACTIVE" => Color.FromArgb(16, 185, 129),
                "PENDING" => Color.FromArgb(245, 158, 11),
                "DROPPED" => Color.FromArgb(239, 68, 68),
                _ => Color.FromArgb(100, 116, 139)
            };

            lblDescription.Text = string.IsNullOrWhiteSpace(enrollment.CourseDescription)
                ? "(Không có mô tả)"
                : enrollment.CourseDescription;
        }

        private void ClearDetailPanel()
        {
            lblCourseName.Text = "Chọn khóa học để xem chi tiết";
            lblTeacher.Text = "";
            lblDates.Text = "";
            lblStatus.Text = "";
            lblDescription.Text = "";
        }

        // ── Xem chi tiết ─────────────────────────────────────────────────
        private void BtnViewDetail_Click(object? sender, EventArgs e)
        {
            if (dgvMyCourses.CurrentRow == null || dgvMyCourses.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Vui lòng chọn một khóa học.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int courseId = Convert.ToInt32(dgvMyCourses.CurrentRow.Cells["CourseId"].Value);
            var enrollment = _allEnrollments.FirstOrDefault(en => en.CourseId == courseId);
            if (enrollment == null) return;

            string info = $"📚 {enrollment.CourseName}\n\n" +
                          $"👤 Giảng viên: {enrollment.TeacherName}\n" +
                          $"📅 Thời gian: {(enrollment.CourseStartDate != DateTime.MinValue ? enrollment.CourseStartDate.ToString("dd/MM/yyyy") : "N/A")} → " +
                          $"{(enrollment.CourseEndDate != DateTime.MinValue ? enrollment.CourseEndDate.ToString("dd/MM/yyyy") : "N/A")}\n" +
                          $"📋 Trạng thái ghi danh: {enrollment.Status}\n" +
                          $"📝 Trạng thái khóa học: {enrollment.CourseStatus}\n\n" +
                          $"📖 Mô tả:\n{(string.IsNullOrWhiteSpace(enrollment.CourseDescription) ? "(Không có)" : enrollment.CourseDescription)}";

            MessageBox.Show(info, "Chi tiết khóa học", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Hủy / Rút khỏi khóa học ─────────────────────────────────────
        private void BtnDrop_Click(object? sender, EventArgs e)
        {
            if (dgvMyCourses.CurrentRow == null || dgvMyCourses.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Vui lòng chọn một khóa học để hủy/rút.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int courseId = Convert.ToInt32(dgvMyCourses.CurrentRow.Cells["CourseId"].Value);
            string courseName = dgvMyCourses.CurrentRow.Cells["Tên khóa học"].Value?.ToString() ?? "";
            string statusText = dgvMyCourses.CurrentRow.Cells["Trạng thái"].Value?.ToString() ?? "";

            // Không cho rút nếu đã rút hoặc hoàn thành
            if (statusText.Contains("Đã rút") || statusText.Contains("Hoàn thành"))
            {
                MessageBox.Show("Không thể thực hiện thao tác này trên khóa học đã rút/hoàn thành.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string confirmMsg = statusText.Contains("Chờ duyệt")
                ? $"Bạn có muốn HỦY yêu cầu tham gia khóa học \"{courseName}\"?"
                : $"Bạn có muốn RÚT khỏi khóa học \"{courseName}\"?\n\nLưu ý: Thao tác này không thể hoàn tác.";

            var result = MessageBox.Show(confirmMsg, "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            string message = _controller.DropCourse(courseId, studentId);
            MessageBox.Show(message, "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Reload
            LoadEnrollments();
        }

        // ── Style DataGridView ───────────────────────────────────────────
        private void StyleDataGridView()
        {
            dgvMyCourses.EnableHeadersVisualStyles = false;
            dgvMyCourses.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 138);
            dgvMyCourses.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvMyCourses.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvMyCourses.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvMyCourses.ColumnHeadersHeight = 40;

            dgvMyCourses.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgvMyCourses.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvMyCourses.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 58, 138);
            dgvMyCourses.DefaultCellStyle.Padding = new Padding(5, 3, 5, 3);

            dgvMyCourses.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvMyCourses.GridColor = Color.FromArgb(226, 232, 240);
            dgvMyCourses.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }
    }
}
