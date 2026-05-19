// UC_Schedule.cs
// Trang "Lịch học" – hiển thị lịch học dựa trên các khóa học sinh viên đã đăng ký (ACTIVE).

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
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
        private readonly AuthController _authController = new(new CourseGuardDbContext(""));
        private readonly CourseController _controller;
        private List<EnrollmentModel> _enrollments = new();

        public UC_Schedule()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyMetaStyle();
            cboTimeFilter.SelectedIndex = 0;

            _controller = new CourseController(new CourseGuardDbContext(""));

            // Pill-shaped button per DESIGN.md
            RoundedButtonHelper.Apply(btnJoinOnline, 10);

            // Events
            cboTimeFilter.SelectedIndexChanged += (s, e) => ApplyTimeFilter();

            btnJoinOnline.Click += (s, e) =>
            {
                if (dgvSchedule.CurrentRow == null || dgvSchedule.CurrentRow.IsNewRow)
                {
                    MetaTheme.ShowModernDialog("Vui lòng chọn một buổi học.", "Thông báo");
                    return;
                }

                int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
                string username = UserSessionContext.CurrentUsername ?? "không xác định";
                string sessionName = dgvSchedule.CurrentRow.Cells.Count > 1
                    ? dgvSchedule.CurrentRow.Cells[1].Value?.ToString() ?? "buổi học online"
                    : "buổi học online";
                _authController.LogUserActivity(userId, "ONLINE_SESSION_JOIN", $"Người dùng {username} tham gia lớp học online: {sessionName}", string.Empty);
                CourseGuard.Frontend.Forms.Student.OnlineClassForm frm = new CourseGuard.Frontend.Forms.Student.OnlineClassForm();
                frm.Show();
            };

            // Style — use MetaTheme.StyleGrid
            MetaTheme.StyleGrid(dgvSchedule);

            // Load dữ liệu thực
            _ = LoadSchedule();
        }

        private async System.Threading.Tasks.Task LoadSchedule()
        {
            this.ShowSkeleton(SkeletonType.CalendarView);
            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                if (studentId == 0) return;

                _enrollments = await System.Threading.Tasks.Task.Run(() => 
                    _controller.GetMyEnrollments(studentId)
                        .Where(e => e.Status == "ACTIVE")
                        .ToList()
                );

                ApplyTimeFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch học: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void ApplyMetaStyle()
        {
            BackColor = AppColors.BgBase;
            StudentTabChrome.StylePrimaryButton(btnJoinOnline);
            StudentTabChrome.StyleGrid(dgvSchedule);
            lblTitle.ForeColor = AppColors.TextPrimary;
            StudentTabChrome.StyleInput(cboTimeFilter);
        }

        private void BuildCardLayout()
        {
            btnJoinOnline.Text = "Tham gia online";
            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Lịch học",
                "Xem lịch học theo tuần, tháng và tham gia buổi học online.",
                cboTimeFilter, btnJoinOnline), 0, 0);
            root.Controls.Add(StudentTabChrome.CreateDataCard("Lịch khóa học", dgvSchedule), 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvSchedule);
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
                    row.DefaultCellStyle.ForeColor = MetaTheme.Colors.Success;
                else if (status.Contains("kết thúc"))
                    row.DefaultCellStyle.ForeColor = MetaTheme.Colors.Critical;
                else if (status.Contains("Sắp"))
                    row.DefaultCellStyle.ForeColor = MetaTheme.Colors.Primary;
            }

            dgvSchedule.ClearSelection();
            dgvSchedule.CurrentCell = null;
        }


    }
}
