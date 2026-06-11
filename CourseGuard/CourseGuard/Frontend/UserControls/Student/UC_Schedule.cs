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
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;
using CourseGuard.Backend.Services.Realtime;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Schedule : UserControl
    {
        private readonly AuthController _authController;
        private readonly CourseController _controller;
        private List<StudentScheduleItemModel> _sessions = new();
        private RoundedPanel _scheduleBody = null!;
        private Label _emptyStateLabel = null!;
        private TableLayoutPanel _calendarView = null!;
        private readonly TcpClassroomClient _tcpClient = new();
        private Button _btnQuickNote = null!;
        private Button _btnToggleView = null!;
        private bool _isCalendarView = true;
        private readonly System.Windows.Forms.Timer _filterDebounceTimer;
        private readonly Dictionary<int, (List<StudentScheduleItemModel> Sessions, Dictionary<DateTime, List<StudentScheduleItemModel>> SessionsByDate, DateTime CalendarStart, DateTime CalendarEnd, string EmptyMessage)> _filterCache = new();
        private readonly List<RoundedPanel> _calendarCellPool = new();
        private int _filterRenderVersion;

        public UC_Schedule()
        {
            CourseGuardDbContext dbContext = new("");
            _authController = new AuthController(dbContext);

            InitializeComponent();
            BuildCardLayout();
            ApplyMetaStyle();
            cboTimeFilter.SelectedIndex = 2;

            _filterDebounceTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _filterDebounceTimer.Tick += (_, _) =>
            {
                _filterDebounceTimer.Stop();
                ApplyTimeFilterAsync().FireAndForgetSafe(this);
            };

            _controller = new CourseController(dbContext);
            RoundedButtonHelper.Apply(btnJoinOnline, 10);
            cboTimeFilter.SelectedIndexChanged += (_, _) => ScheduleFilterRefresh();
            btnJoinOnline.Click += (_, _) => JoinSelectedSession();
            MetaTheme.StyleGrid(dgvSchedule);
            LoadSchedule().FireAndForgetSafe(this);
            
            _tcpClient.ClassStatusChanged += TcpClient_ClassStatusChanged;
            _tcpClient.StartAsync().FireAndForgetSafe(this);
            
            this.Disposed += (s, e) =>
            {
                _filterDebounceTimer.Dispose();
                _tcpClient.Dispose();
            };
        }

        private void TcpClient_ClassStatusChanged(object? sender, ClassStatusEventArgs e)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    var session = _sessions.FirstOrDefault(s => s.SessionId == e.SessionId);
                    if (session != null)
                    {
                        // Refresh if the opened status changed
                        LoadSchedule().FireAndForgetSafe(this);
                    }
                });
            }
        }

        private async System.Threading.Tasks.Task LoadSchedule()
        {
            this.ShowSkeleton(SkeletonType.CalendarView);
            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                if (studentId == 0)
                {
                    BindToGrid(new List<StudentScheduleItemModel>(), "Không xác định được tài khoản học sinh.");
                    return;
                }

                _sessions = (await System.Threading.Tasks.Task.Run(() => _controller.GetStudentOnlineSessions(studentId)))
                    .OrderBy(s => s.StartTime ?? DateTime.MaxValue)
                    .ThenBy(s => s.CourseName)
                    .ToList();
                _filterCache.Clear();
                ApplyTimeFilterAsync().FireAndForgetSafe(this);
            }
            catch (Exception ex)
            {
                BindToGrid(new List<StudentScheduleItemModel>(), $"Lỗi tải lịch học: {ex.Message}");
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

            _btnQuickNote = new Button { Text = "Ghi chú nhanh", Width = 140 };
            StudentTabChrome.StyleSecondaryButton(_btnQuickNote);
            RoundedButtonHelper.Apply(_btnQuickNote, 10);
            _btnQuickNote.Click += (_, _) => OpenQuickNote();

            _btnToggleView = new Button { Text = "Dạng bảng", Width = 120 };
            StudentTabChrome.StyleSecondaryButton(_btnToggleView);
            RoundedButtonHelper.Apply(_btnToggleView, 10);
            _btnToggleView.Click += ToggleView;

            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Lịch học",
                "Xem lịch học theo tuần, tháng và tham gia buổi học online.",
                cboTimeFilter, _btnToggleView, _btnQuickNote, btnJoinOnline), 0, 0);

            _calendarView = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 1,
                Visible = false,
                BackColor = AppColors.BgCard,
                AutoScroll = true,
                Margin = Padding.Empty
            };
            for (int i = 0; i < 7; i++)
                _calendarView.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));

            _scheduleBody = StudentTabChrome.CreateTableBody(dgvSchedule, out _emptyStateLabel);
            _scheduleBody.Controls.Add(_calendarView);
            EnableDoubleBuffering(this);
            EnableDoubleBuffering(_scheduleBody);
            EnableDoubleBuffering(_calendarView);
            EnableDoubleBuffering(dgvSchedule);
            root.Controls.Add(StudentTabChrome.CreateDataCard("Lịch khóa học", _scheduleBody), 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvSchedule);
        }

        private void ScheduleFilterRefresh()
        {
            _filterDebounceTimer.Stop();
            _filterDebounceTimer.Start();
        }

        private async System.Threading.Tasks.Task ApplyTimeFilterAsync()
        {
            int renderVersion = ++_filterRenderVersion;
            int filterIndex = cboTimeFilter.SelectedIndex;
            if (_filterCache.TryGetValue(filterIndex, out var cached))
            {
                RenderCurrentView(cached.Sessions, cached.SessionsByDate, cached.EmptyMessage, cached.CalendarStart, cached.CalendarEnd);
                return;
            }

            var source = _sessions.ToList();
            var prepared = await System.Threading.Tasks.Task.Run(() => PrepareFilterData(source, filterIndex));
            if (IsDisposed || renderVersion != _filterRenderVersion) return;

            _filterCache[filterIndex] = prepared;
            RenderCurrentView(prepared.Sessions, prepared.SessionsByDate, prepared.EmptyMessage, prepared.CalendarStart, prepared.CalendarEnd);
        }

        private static (List<StudentScheduleItemModel> Sessions, Dictionary<DateTime, List<StudentScheduleItemModel>> SessionsByDate, DateTime CalendarStart, DateTime CalendarEnd, string EmptyMessage) PrepareFilterData(List<StudentScheduleItemModel> sessions, int filterIndex)
        {
            IEnumerable<StudentScheduleItemModel> filtered = sessions;
            DateTime now = DateTime.Now;
            string emptyMessage = "Chưa có lịch học phù hợp.";

            DateTime calendarStart;
            DateTime calendarEnd;

            switch (filterIndex)
            {
                case 0:
                    DateTime startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + 1);
                    DateTime endOfWeek = startOfWeek.AddDays(7);
                    filtered = filtered.Where(s => s.StartTime.HasValue && s.StartTime.Value >= startOfWeek && s.StartTime.Value < endOfWeek);
                    calendarStart = startOfWeek;
                    calendarEnd = endOfWeek.AddDays(-1);
                    emptyMessage = "Chưa có lịch học trong tuần này.";
                    break;
                case 1:
                    DateTime startOfMonth = new(now.Year, now.Month, 1);
                    DateTime endOfMonth = startOfMonth.AddMonths(1);
                    filtered = filtered.Where(s => s.StartTime.HasValue && s.StartTime.Value >= startOfMonth && s.StartTime.Value < endOfMonth);
                    calendarStart = startOfMonth;
                    calendarEnd = endOfMonth.AddDays(-1);
                    emptyMessage = "Chưa có lịch học trong tháng này.";
                    break;
                default:
                    calendarStart = now.Date.AddDays(-(int)now.DayOfWeek);
                    calendarEnd = now.Date.AddMonths(6);
                    break;
            }

            var filteredList = filtered
                .OrderBy(s => s.StartTime ?? DateTime.MaxValue)
                .ThenBy(s => s.CourseName)
                .ToList();
            var sessionsByDate = filteredList
                .Where(s => s.StartTime.HasValue)
                .GroupBy(s => s.StartTime!.Value.Date)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StartTime).ToList());

            return (filteredList, sessionsByDate, calendarStart, calendarEnd, emptyMessage);
        }

        private void RenderCurrentView(List<StudentScheduleItemModel> sessions, Dictionary<DateTime, List<StudentScheduleItemModel>> sessionsByDate, string emptyMessage, DateTime calendarStart, DateTime calendarEnd)
        {
            if (_isCalendarView)
            {
                RenderCalendar(sessions, sessionsByDate, emptyMessage, calendarStart, calendarEnd);
            }
            else
            {
                BindToGrid(sessions, emptyMessage);
            }
        }

        private void BindToGrid(List<StudentScheduleItemModel> sessions, string emptyMessage = "Chưa có lịch học phù hợp.")
        {
            _scheduleBody.SuspendLayout();
            dgvSchedule.SuspendLayout();
            try
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
                    dt.Rows.Add(
                        session.SessionId,
                        session.CourseName,
                        string.IsNullOrWhiteSpace(session.Title) ? session.CourseName : session.Title,
                        session.TeacherName,
                        session.StartTime?.ToString("dd/MM/yyyy HH:mm") ?? "",
                        session.EndTime?.ToString("dd/MM/yyyy HH:mm") ?? "",
                        BuildStatus(session),
                        session.MeetingLink);
                }

                dgvSchedule.DataSource = dt;
                dgvSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                if (dgvSchedule.Columns["SessionId"] != null)
                    dgvSchedule.Columns["SessionId"]!.Visible = false;
                if (dgvSchedule.Columns["Link"] != null)
                    dgvSchedule.Columns["Link"]!.Visible = false;

                bool hasRows = dt.Rows.Count > 0;
                dgvSchedule.Visible = hasRows;
                _calendarView.Visible = false;
                StudentTabChrome.SetTableState(_scheduleBody, dgvSchedule, _emptyStateLabel, hasRows, emptyMessage);
                btnJoinOnline.Enabled = hasRows;
                _btnQuickNote.Enabled = hasRows;
                dgvSchedule.ClearSelection();
                dgvSchedule.CurrentCell = null;
            }
            finally
            {
                dgvSchedule.ResumeLayout();
                _scheduleBody.ResumeLayout(true);
            }
        }

        private void ToggleView(object? sender, EventArgs e)
        {
            _isCalendarView = !_isCalendarView;
            _btnToggleView.Text = _isCalendarView ? "Dạng bảng" : "Dạng lịch";
            ApplyTimeFilterAsync().FireAndForgetSafe(this);
        }

        private void RenderCalendar(List<StudentScheduleItemModel> sessions, Dictionary<DateTime, List<StudentScheduleItemModel>> sessionsByDate, string emptyMessage, DateTime rangeStart, DateTime rangeEnd)
        {
            bool hasRows = sessions.Count > 0;
            dgvSchedule.Visible = false;
            _calendarView.Visible = true;
            _emptyStateLabel.Text = emptyMessage;
            _emptyStateLabel.Visible = false;
            _scheduleBody.Padding = Padding.Empty;
            _scheduleBody.FillColor = AppColors.BgCard;
            _scheduleBody.BorderColor = Color.Transparent;
            _scheduleBody.BackColor = AppColors.BgCard;
            btnJoinOnline.Enabled = false;
            _btnQuickNote.Enabled = false;

            _scheduleBody.SuspendLayout();
            _calendarView.SuspendLayout();
            try
            {
                _calendarView.Controls.Clear();
                _calendarView.RowStyles.Clear();

                DateTime today = DateTime.Now.Date;
                DateTime start = rangeStart.Date;
                DateTime end = rangeEnd.Date;
                if (end < start) end = start;
                int totalDays = (end - start).Days + 1;
                int totalWeeks = Math.Max(1, (int)Math.Ceiling(totalDays / 7d));

                _calendarView.RowCount = totalWeeks;
                bool useScrollableRows = totalWeeks > 6;
                _calendarView.AutoScroll = useScrollableRows;
                for (int i = 0; i < totalWeeks; i++)
                {
                    _calendarView.RowStyles.Add(useScrollableRows
                        ? new RowStyle(SizeType.Absolute, 128f)
                        : new RowStyle(SizeType.Percent, 100f / totalWeeks));
                }

                EnsureCalendarCellPool(totalWeeks * 7);

                for (int i = 0; i < _calendarCellPool.Count; i++)
                    _calendarCellPool[i].Visible = false;

                for (int row = 0; row < totalWeeks; row++)
                {
                    for (int col = 0; col < 7; col++)
                    {
                        int cellIndex = row * 7 + col;
                        DateTime currentDate = start.AddDays(cellIndex);
                        sessionsByDate.TryGetValue(currentDate, out var daySessions);
                        RoundedPanel cell = _calendarCellPool[cellIndex];
                        ConfigureCalendarDayCell(cell, currentDate, daySessions ?? new List<StudentScheduleItemModel>(), today);
                        if (!_calendarView.Controls.Contains(cell))
                            _calendarView.Controls.Add(cell, col, row);
                        else
                            _calendarView.SetCellPosition(cell, new TableLayoutPanelCellPosition(col, row));
                        cell.Visible = true;
                    }
                }
            }
            finally
            {
                _calendarView.ResumeLayout();
                _scheduleBody.ResumeLayout(true);
            }
            _calendarView.BringToFront();
        }

        private void EnsureCalendarCellPool(int requiredCount)
        {
            while (_calendarCellPool.Count < requiredCount)
            {
                var cell = CreateCalendarDayCell();
                EnableDoubleBuffering(cell);
                _calendarCellPool.Add(cell);
            }
        }

        private static RoundedPanel CreateCalendarDayCell()
        {
            var pnl = new RoundedPanel
            {
                CornerRadius = 10,
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Padding = new Padding(6)
            };

            pnl.Controls.Add(new Label { Name = "lblDate", AutoSize = false, Height = 22, Left = 6, Top = 6, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });
            for (int i = 0; i < 3; i++)
            {
                pnl.Controls.Add(new Label { Name = $"lblSession{i}", AutoSize = false, Height = 28, Left = 6, Top = 30 + i * 30, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, AutoEllipsis = true });
            }
            pnl.Controls.Add(new Label { Name = "lblMore", AutoSize = false, Height = 22, Left = 6, Top = 112, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, AutoEllipsis = true });
            pnl.Resize += (_, _) =>
            {
                foreach (Control child in pnl.Controls)
                    child.Width = Math.Max(20, pnl.ClientSize.Width - 12);
            };
            return pnl;
        }

        private static void ConfigureCalendarDayCell(RoundedPanel pnl, DateTime currentDate, List<StudentScheduleItemModel> daySessions, DateTime today)
        {
            bool hasSession = daySessions.Count > 0;
            Color scheduledDayColor = Color.FromArgb(34, 197, 94);
            Color scheduledSessionColor = Color.FromArgb(220, 252, 231);
            Color dayBackColor = hasSession
                ? scheduledDayColor
                : currentDate.Date == today ? AppColors.Border : AppColors.BgCard;
            Color dateTextColor = hasSession ? Color.White : AppColors.TextPrimary;
            Color sessionTextColor = hasSession ? scheduledSessionColor : AppColors.TextSecondary;

            pnl.BackColor = dayBackColor;
            pnl.FillColor = dayBackColor;
            pnl.BorderColor = hasSession ? Color.FromArgb(22, 163, 74) : AppColors.Border;

            var lblDate = (Label)pnl.Controls["lblDate"]!;
            lblDate.Text = $"{GetVietnameseDayOfWeek(currentDate.DayOfWeek)} - {currentDate:dd/MM}";
            lblDate.Font = MetaTheme.Fonts.BodySmBold();
            lblDate.ForeColor = dateTextColor;
            lblDate.BackColor = dayBackColor;
            lblDate.Width = Math.Max(20, pnl.ClientSize.Width - 12);

            var orderedSessions = daySessions.OrderBy(s => s.StartTime).Take(3).ToList();
            for (int i = 0; i < 3; i++)
            {
                var lblSession = (Label)pnl.Controls[$"lblSession{i}"]!;
                lblSession.Visible = i < orderedSessions.Count;
                lblSession.Font = MetaTheme.Fonts.Caption();
                lblSession.ForeColor = sessionTextColor;
                lblSession.BackColor = dayBackColor;
                lblSession.Width = Math.Max(20, pnl.ClientSize.Width - 12);
                if (i < orderedSessions.Count)
                {
                    var s = orderedSessions[i];
                    string title = string.IsNullOrWhiteSpace(s.Title) ? s.CourseName : s.Title;
                    lblSession.Text = $"{s.StartTime:HH:mm} - {title}";
                }
            }

            var lblMore = (Label)pnl.Controls["lblMore"]!;
            lblMore.Visible = daySessions.Count > 3;
            lblMore.Text = $"+{daySessions.Count - 3} tiết học khác";
            lblMore.Font = MetaTheme.Fonts.Caption();
            lblMore.ForeColor = sessionTextColor;
            lblMore.BackColor = dayBackColor;
            lblMore.Width = Math.Max(20, pnl.ClientSize.Width - 12);
        }

        private static string GetVietnameseDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                _ => "CN"
            };
        }

        private void JoinSelectedSession()
        {
            if (!dgvSchedule.Visible || dgvSchedule.CurrentRow == null || dgvSchedule.CurrentRow.IsNewRow)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một buổi học.", "Thông báo");
                return;
            }

            string link = dgvSchedule.CurrentRow.Cells["Link"].Value?.ToString() ?? string.Empty;
            string sessionName = dgvSchedule.CurrentRow.Cells["Buổi học"].Value?.ToString() ?? "buổi học online";
            int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
            string username = UserSessionContext.CurrentUsername ?? "không xác định";
            _authController.LogUserActivity(userId, "ONLINE_SESSION_JOIN", $"Người dùng {username} tham gia lớp học online: {sessionName}", string.Empty);

            int sessionId = 0;
            if (dgvSchedule.CurrentRow.Cells["SessionId"].Value is int sid) sessionId = sid;

            if (sessionId <= 0)
            {
                MetaTheme.ShowModernDialog("Không xác định được buổi học cần tham gia.", "Thông báo");
                return;
            }

            using var meetingForm = new CourseGuard.Frontend.Forms.Student.StudentNativeClassroomForm(sessionId, sessionName);
            meetingForm.ShowDialog(FindForm());
            _ = LoadSchedule();
        }

        private void OpenQuickNote()
        {
            if (!dgvSchedule.Visible || dgvSchedule.CurrentRow == null || dgvSchedule.CurrentRow.IsNewRow)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một buổi học để ghi chú.", "Thông báo");
                return;
            }
            if (dgvSchedule.CurrentRow.Cells["SessionId"].Value is int sessionId)
            {
                var dialog = new CourseGuard.Frontend.Forms.Student.QuickNoteDialog(UserSessionContext.CurrentUserId ?? 0, sessionId);
                dialog.ShowDialog(this.FindForm());
            }
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

        private static void EnableDoubleBuffering(Control control)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(control, true, null);
        }
    }
}
