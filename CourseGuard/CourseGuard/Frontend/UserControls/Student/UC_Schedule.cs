// UC_Schedule.cs
// Trang "Lịch học" – hiển thị lịch học dựa trên các khóa học sinh viên đã đăng ký (ACTIVE).

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
    public partial class UC_Schedule : UserControl
    {
        private readonly CourseController _controller;
        private List<EnrollmentModel> _enrollments = new();

        public UC_Schedule()
        {
            InitializeComponent();
            cboTimeFilter.SelectedIndex = 0;

            _controller = new CourseController(new CourseGuardDbContext(""));

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnJoinOnline, 10);

            // Events
            cboTimeFilter.SelectedIndexChanged += (s, e) => ApplyTimeFilter();

            btnJoinOnline.Click += (s, e) =>
            {
                CourseGuard.Frontend.Forms.Student.OnlineClassForm frm = new CourseGuard.Frontend.Forms.Student.OnlineClassForm();
                frm.Show();
            };

            // Style
            StyleDataGridView();

            // Load dữ liệu thực
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                if (studentId == 0) return;

                _enrollments = _controller.GetMyEnrollments(studentId)
                    .Where(e => e.Status == "ACTIVE")
                    .ToList();

                ApplyTimeFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch học: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTimeFilter()
        {
            var filtered = _enrollments.AsEnumerable();
            DateTime now = DateTime.Now;

            switch (cboTimeFilter.SelectedIndex)
            {
                case 0: // Tuần này
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek + 1); // Monday
                    var endOfWeek = startOfWeek.AddDays(6);
                    filtered = filtered.Where(e =>
                        e.CourseStartDate <= endOfWeek && e.CourseEndDate >= startOfWeek);
                    break;
                case 1: // Tháng này
                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                    filtered = filtered.Where(e =>
                        e.CourseStartDate <= endOfMonth && e.CourseEndDate >= startOfMonth);
                    break;
                    // 2 = Tất cả → không lọc
            }

            BindToGrid(filtered.ToList());
        }

        private void BindToGrid(List<EnrollmentModel> enrollments)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Môn học", typeof(string));
            dt.Columns.Add("Giảng viên", typeof(string));
            dt.Columns.Add("Ngày bắt đầu", typeof(string));
            dt.Columns.Add("Ngày kết thúc", typeof(string));
            dt.Columns.Add("Trạng thái", typeof(string));

            foreach (var e in enrollments)
            {
                string status = "Đang diễn ra";
                if (e.CourseEndDate != DateTime.MinValue && e.CourseEndDate < DateTime.Now)
                    status = "Đã kết thúc";
                else if (e.CourseStartDate != DateTime.MinValue && e.CourseStartDate > DateTime.Now)
                    status = "Sắp diễn ra";

                dt.Rows.Add(
                    e.CourseName,
                    e.TeacherName,
                    e.CourseStartDate != DateTime.MinValue ? e.CourseStartDate.ToString("dd/MM/yyyy") : "N/A",
                    e.CourseEndDate != DateTime.MinValue ? e.CourseEndDate.ToString("dd/MM/yyyy") : "N/A",
                    status
                );
            }

            dgvSchedule.DataSource = dt;
            dgvSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Tô màu theo trạng thái
            foreach (DataGridViewRow row in dgvSchedule.Rows)
            {
                if (row.IsNewRow) continue;
                string status = row.Cells["Trạng thái"].Value?.ToString() ?? "";
                if (status.Contains("Đang"))
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(5, 150, 105);
                else if (status.Contains("kết thúc"))
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (status.Contains("Sắp"))
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(37, 99, 235);
            }
        }

        private void StyleDataGridView()
        {
            dgvSchedule.EnableHeadersVisualStyles = false;
            dgvSchedule.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 138);
            dgvSchedule.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSchedule.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvSchedule.ColumnHeadersHeight = 40;

            dgvSchedule.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgvSchedule.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvSchedule.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 58, 138);

            dgvSchedule.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvSchedule.GridColor = Color.FromArgb(226, 232, 240);
        }
    }
}