using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        private List<StudentScheduleItemModel> _sessions = new();

        public UC_Schedule()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyMetaStyle();
            cboTimeFilter.SelectedIndex = 0;

            _controller = new CourseController(new CourseGuardDbContext(""));
            RoundedButtonHelper.Apply(btnJoinOnline, 10);
            cboTimeFilter.SelectedIndexChanged += (_, _) => ApplyTimeFilter();
            btnJoinOnline.Click += (_, _) => JoinSelectedSession();
            MetaTheme.StyleGrid(dgvSchedule);
            _ = LoadSchedule();
        }

        private async System.Threading.Tasks.Task LoadSchedule()
        {
            this.ShowSkeleton(SkeletonType.CalendarView);
            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                if (studentId == 0) return;

                _sessions = await System.Threading.Tasks.Task.Run(() => _controller.GetStudentOnlineSessions(studentId));
                ApplyTimeFilter();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Lỗi tải lịch học: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            IEnumerable<StudentScheduleItemModel> filtered = _sessions;
            DateTime now = DateTime.Now;

            switch (cboTimeFilter.SelectedIndex)
            {
                case 0:
                    DateTime startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + 1);
                    DateTime endOfWeek = startOfWeek.AddDays(7);
                    filtered = filtered.Where(s => s.StartTime.HasValue && s.StartTime.Value >= startOfWeek && s.StartTime.Value < endOfWeek);
                    break;
                case 1:
                    DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
                    DateTime endOfMonth = startOfMonth.AddMonths(1);
                    filtered = filtered.Where(s => s.StartTime.HasValue && s.StartTime.Value >= startOfMonth && s.StartTime.Value < endOfMonth);
                    break;
            }

            BindToGrid(filtered.ToList());
        }

        private void BindToGrid(List<StudentScheduleItemModel> sessions)
        {
            DataTable dt = new();
            dt.Columns.Add("SessionId", typeof(int));
            dt.Columns.Add("Môn học", typeof(string));
            dt.Columns.Add("Buổi học", typeof(string));
            dt.Columns.Add("Giảng viên", typeof(string));
            dt.Columns.Add("Bắt đầu", typeof(string));
            dt.Columns.Add("Kết thúc", typeof(string));
            dt.Columns.Add("Trạng thái", typeof(string));
            dt.Columns.Add("Link", typeof(string));

            foreach (var session in sessions)
            {
                string status = BuildStatus(session);
                dt.Rows.Add(
                    session.SessionId,
                    session.CourseName,
                    string.IsNullOrWhiteSpace(session.Title) ? session.CourseName : session.Title,
                    session.TeacherName,
                    session.StartTime?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    session.EndTime?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    status,
                    session.MeetingLink);
            }

            dgvSchedule.DataSource = dt;
            dgvSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            if (dgvSchedule.Columns["SessionId"] != null)
                dgvSchedule.Columns["SessionId"]!.Visible = false;
            if (dgvSchedule.Columns["Link"] != null)
                dgvSchedule.Columns["Link"]!.Visible = false;
            dgvSchedule.ClearSelection();
            dgvSchedule.CurrentCell = null;
        }

        private void JoinSelectedSession()
        {
            if (dgvSchedule.CurrentRow == null || dgvSchedule.CurrentRow.IsNewRow)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một buổi học.", "Thông báo");
                return;
            }

            string link = dgvSchedule.CurrentRow.Cells["Link"].Value?.ToString() ?? string.Empty;
            string sessionName = dgvSchedule.CurrentRow.Cells["Buổi học"].Value?.ToString() ?? "buổi học online";
            int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
            string username = UserSessionContext.CurrentUsername ?? "không xác định";
            _authController.LogUserActivity(userId, "ONLINE_SESSION_JOIN", $"Người dùng {username} tham gia lớp học online: {sessionName}", string.Empty);

            if (!string.IsNullOrWhiteSpace(link))
            {
                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
                return;
            }

            CourseGuard.Frontend.Forms.Student.OnlineClassForm frm = new();
            frm.Show();
        }

        private static string BuildStatus(StudentScheduleItemModel session)
        {
            DateTime now = DateTime.Now;
            if (session.EndTime.HasValue && session.EndTime.Value < now)
                return "Đã kết thúc";
            if (session.StartTime.HasValue && session.StartTime.Value <= now && (!session.EndTime.HasValue || session.EndTime.Value >= now))
                return "Đang diễn ra";
            return "Sắp diễn ra";
        }
    }
}
