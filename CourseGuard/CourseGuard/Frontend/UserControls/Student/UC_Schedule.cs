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
using CourseGuard.Backend.Services.Realtime;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Schedule : UserControl
    {
        private readonly AuthController _authController;
        private readonly CourseController _controller;
        private readonly TcpClassroomClient _tcpClient = new();
        private readonly System.Windows.Forms.Timer _filterDebounceTimer;
        private readonly Dictionary<int, (List<StudentScheduleItemModel> Sessions, Dictionary<DateTime, List<StudentScheduleItemModel>> SessionsByDate, DateTime CalendarStart, DateTime CalendarEnd, string EmptyMessage)> _filterCache = new();
        private readonly List<RoundedPanel> _calendarCellPool = new();

        private List<StudentScheduleItemModel> _sessions = new();
        private RoundedPanel _scheduleBody = null!;
        private Label _emptyStateLabel = null!;
        private TableLayoutPanel _calendarView = null!;
        private Button _btnQuickNote = null!;
        private Button _btnToggleView = null!;
        private RoundedPanel _selectedSessionSummary = null!;
        private Label _selectedSessionTitle = null!;
        private Label _selectedSessionDetail = null!;
        private Label _selectedSessionAction = null!;
        private bool _isCalendarView = true;
        private int _loadVersion;
        private int _filterRenderVersion;
        private int? _selectedCalendarSessionId;

        public UC_Schedule()
        {
            CourseGuardDbContext dbContext = new(string.Empty);
            _authController = new AuthController(dbContext);
            _controller = new CourseController(dbContext);

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

            RoundedButtonHelper.Apply(btnJoinOnline, 10);
            cboTimeFilter.SelectedIndexChanged += (_, _) => ScheduleFilterRefresh();
            btnJoinOnline.Click += (_, _) => JoinSelectedSession();
            dgvSchedule.SelectionChanged += (_, _) => HandleGridSelectionChanged();
            MetaTheme.StyleGrid(dgvSchedule);

            LoadSchedule().FireAndForgetSafe(this);

            _tcpClient.ClassStatusChanged += TcpClient_ClassStatusChanged;
            _tcpClient.StartAsync().FireAndForgetSafe(this);

            Disposed += (_, _) =>
            {
                _filterDebounceTimer.Dispose();
                _tcpClient.Dispose();
            };
        }

        private void TcpClient_ClassStatusChanged(object? sender, ClassStatusEventArgs e)
        {
            if (!IsHandleCreated)
                return;

            Invoke((MethodInvoker)delegate
            {
                if (_sessions.Any(s => s.SessionId == e.SessionId))
                    LoadSchedule().FireAndForgetSafe(this);
            });
        }

        private async System.Threading.Tasks.Task LoadSchedule()
        {
            int loadVersion = ++_loadVersion;
            this.ShowSkeleton(SkeletonType.CalendarView);
            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                if (studentId == 0)
                {
                    if (IsDisposed || loadVersion != _loadVersion)
                        return;

                    ClearScheduleStateAfterLoadFailure("Không xác định được tài khoản học sinh.");
                    return;
                }

                List<StudentScheduleItemModel> sessions = (await System.Threading.Tasks.Task.Run(() => _controller.GetStudentOnlineSessions(studentId)))
                    .OrderBy(s => s.StartTime ?? DateTime.MaxValue)
                    .ThenBy(s => s.CourseName)
                    .ToList();
                if (IsDisposed || loadVersion != _loadVersion)
                    return;

                _sessions = sessions;
                _filterCache.Clear();
                await ApplyTimeFilterAsync();
            }
            catch (Exception ex)
            {
                if (loadVersion != _loadVersion)
                    return;

                ClearScheduleStateAfterLoadFailure($"Lỗi tải lịch học: {ex.Message}");
                MetaTheme.ShowModernDialog("Lỗi tải lịch học: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (loadVersion == _loadVersion)
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

            var scheduleCard = StudentTabChrome.CreateDataCard("Lịch khóa học", _scheduleBody);
            scheduleCard.Margin = new Padding(0, 0, 0, 12);
            root.Controls.Add(scheduleCard, 0, 1);

            root.RowCount = 3;
            root.RowStyles.Clear();
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _selectedSessionSummary = StudentTabChrome.CreateCard();
            _selectedSessionSummary.Padding = new Padding(18, 14, 18, 14);
            _selectedSessionSummary.AutoSize = true;
            _selectedSessionSummary.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var summaryLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            summaryLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _selectedSessionTitle = new Label
            {
                AutoSize = true,
                Font = AppFonts.Semibold(11f),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };
            _selectedSessionDetail = new Label
            {
                AutoSize = true,
                Font = AppFonts.Body,
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 6, 0, 0)
            };
            _selectedSessionAction = new Label
            {
                AutoSize = true,
                Font = MetaTheme.Fonts.BodySmBold(),
                ForeColor = AppColors.AccentBlue,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 8, 0, 0)
            };

            summaryLayout.Controls.Add(_selectedSessionTitle, 0, 0);
            summaryLayout.Controls.Add(_selectedSessionDetail, 0, 1);
            summaryLayout.Controls.Add(_selectedSessionAction, 0, 2);
            _selectedSessionSummary.Controls.Add(summaryLayout);
            root.Controls.Add(_selectedSessionSummary, 0, 2);

            ResetSelectedSessionSummary();
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

            var prepared = await System.Threading.Tasks.Task.Run(() => PrepareFilterData(_sessions.ToList(), filterIndex));
            if (IsDisposed || renderVersion != _filterRenderVersion)
                return;

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
                    DateTime startOfWeek = StartOfWeek(now);
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
                    calendarStart = StartOfWeek(now);
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
            EnsureSelectedSessionStillVisible(sessions.Select(s => s.SessionId));

            if (_isCalendarView)
                RenderCalendar(sessions, sessionsByDate, emptyMessage, calendarStart, calendarEnd);
            else
                BindToGrid(sessions, emptyMessage);

            UpdateSelectedSessionSummary(GetSelectedSession());
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
                dt.Columns.Add("Hành động", typeof(string));
                dt.Columns.Add("Link", typeof(string));

                foreach (var session in sessions)
                {
                    ScheduleUxPresentation view = ScheduleUxPresenter.PresentStudent(session, DateTime.Now);
                    dt.Rows.Add(
                        session.SessionId,
                        session.CourseName,
                        string.IsNullOrWhiteSpace(session.Title) ? session.CourseName : session.Title,
                        session.TeacherName,
                        session.StartTime?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
                        session.EndTime?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
                        view.StatusText,
                        view.PrimaryActionText,
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
                dgvSchedule.ClearSelection();
                dgvSchedule.CurrentCell = null;
            }
            finally
            {
                dgvSchedule.ResumeLayout();
                _scheduleBody.ResumeLayout(true);
            }

            UpdateSelectedSessionSummary(GetSelectedSession());
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
            _emptyStateLabel.Visible = !hasRows;
            _scheduleBody.Padding = Padding.Empty;
            _scheduleBody.FillColor = AppColors.BgCard;
            _scheduleBody.BorderColor = Color.Transparent;
            _scheduleBody.BackColor = AppColors.BgCard;

            _scheduleBody.SuspendLayout();
            _calendarView.SuspendLayout();
            try
            {
                _calendarView.Controls.Clear();
                _calendarView.RowStyles.Clear();

                DateTime today = DateTime.Now.Date;
                DateTime start = rangeStart.Date;
                DateTime end = rangeEnd.Date;
                if (end < start)
                    end = start;

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
                foreach (RoundedPanel cell in _calendarCellPool)
                    cell.Visible = false;

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
            UpdateSelectedSessionSummary(GetSelectedSession());
        }

        private void EnsureCalendarCellPool(int requiredCount)
        {
            while (_calendarCellPool.Count < requiredCount)
            {
                RoundedPanel cell = CreateCalendarDayCell();
                EnableDoubleBuffering(cell);
                _calendarCellPool.Add(cell);
            }
        }

        private static RoundedPanel CreateCalendarDayCell()
        {
            var panel = new RoundedPanel
            {
                CornerRadius = 10,
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Padding = new Padding(6)
            };

            panel.Controls.Add(new Label
            {
                Name = "lblDate",
                AutoSize = false,
                Height = 22,
                Left = 6,
                Top = 6,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            });

            for (int i = 0; i < 3; i++)
            {
                panel.Controls.Add(new Label
                {
                    Name = $"lblSession{i}",
                    AutoSize = false,
                    Height = 28,
                    Left = 6,
                    Top = 30 + i * 30,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    AutoEllipsis = true,
                    Cursor = Cursors.Hand
                });
            }

            panel.Controls.Add(new Label
            {
                Name = "lblMore",
                AutoSize = false,
                Height = 22,
                Left = 6,
                Top = 112,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true
            });

            panel.Resize += (_, _) =>
            {
                foreach (Control child in panel.Controls)
                    child.Width = Math.Max(20, panel.ClientSize.Width - 12);
            };

            return panel;
        }

        private void ConfigureCalendarDayCell(RoundedPanel panel, DateTime currentDate, List<StudentScheduleItemModel> daySessions, DateTime today)
        {
            bool hasSession = daySessions.Count > 0;
            Color scheduledDayColor = Color.FromArgb(34, 197, 94);
            Color scheduledSessionColor = Color.FromArgb(220, 252, 231);
            Color dayBackColor = hasSession
                ? scheduledDayColor
                : currentDate.Date == today ? AppColors.Border : AppColors.BgCard;
            Color dateTextColor = hasSession ? Color.White : AppColors.TextPrimary;
            Color sessionTextColor = hasSession ? scheduledSessionColor : AppColors.TextSecondary;

            panel.BackColor = dayBackColor;
            panel.FillColor = dayBackColor;
            panel.BorderColor = hasSession ? Color.FromArgb(22, 163, 74) : AppColors.Border;

            var lblDate = (Label)panel.Controls["lblDate"]!;
            lblDate.Text = $"{GetVietnameseDayOfWeek(currentDate.DayOfWeek)} - {currentDate:dd/MM}";
            lblDate.Font = MetaTheme.Fonts.BodySmBold();
            lblDate.ForeColor = dateTextColor;
            lblDate.BackColor = dayBackColor;

            var orderedSessions = daySessions.OrderBy(s => s.StartTime).Take(3).ToList();
            for (int i = 0; i < 3; i++)
            {
                var lblSession = (Label)panel.Controls[$"lblSession{i}"]!;
                lblSession.Visible = i < orderedSessions.Count;
                lblSession.Font = MetaTheme.Fonts.Caption();
                lblSession.ForeColor = sessionTextColor;
                lblSession.BackColor = dayBackColor;
                lblSession.Width = Math.Max(20, panel.ClientSize.Width - 12);

                if (i < orderedSessions.Count)
                {
                    StudentScheduleItemModel session = orderedSessions[i];
                    string title = string.IsNullOrWhiteSpace(session.Title) ? session.CourseName : session.Title;
                    lblSession.Text = $"{session.StartTime:HH:mm} - {title}";
                    lblSession.Tag = session.SessionId;
                    lblSession.Cursor = Cursors.Hand;
                    lblSession.Click -= CalendarSessionLabel_Click;
                    lblSession.Click += CalendarSessionLabel_Click;
                }
                else
                {
                    lblSession.Text = string.Empty;
                    lblSession.Tag = null;
                    lblSession.Cursor = Cursors.Default;
                    lblSession.Click -= CalendarSessionLabel_Click;
                }
            }

            var lblMore = (Label)panel.Controls["lblMore"]!;
            lblMore.Visible = daySessions.Count > 3;
            lblMore.Text = $"+{daySessions.Count - 3} tiết học khác";
            lblMore.Font = MetaTheme.Fonts.Caption();
            lblMore.ForeColor = sessionTextColor;
            lblMore.BackColor = dayBackColor;
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
            StudentScheduleItemModel? session = GetSelectedSession();
            if (session == null)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một buổi học.", "Thông báo");
                return;
            }

            ScheduleUxPresentation view = ScheduleUxPresenter.PresentStudent(session, DateTime.Now);
            if (!view.CanJoin)
            {
                MetaTheme.ShowModernDialog($"{view.StatusText}: {view.DetailText}", "Thông báo");
                UpdateSelectedSessionSummary(session);
                return;
            }

            string sessionName = string.IsNullOrWhiteSpace(session.Title) ? session.CourseName : session.Title;
            int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
            string username = UserSessionContext.CurrentUsername ?? "không xác định";
            _authController.LogUserActivity(userId, "ONLINE_SESSION_JOIN", $"Người dùng {username} tham gia lớp học online: {sessionName}", string.Empty);

            if (session.SessionId <= 0)
            {
                MetaTheme.ShowModernDialog("Không xác định được buổi học cần tham gia.", "Thông báo");
                return;
            }

            using var meetingForm = new CourseGuard.Frontend.Forms.Student.StudentNativeClassroomForm(session.SessionId, sessionName);
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
                dialog.ShowDialog(FindForm());
            }
        }

        private void HandleGridSelectionChanged()
        {
            if (dgvSchedule.Focused && dgvSchedule.CurrentRow != null && !dgvSchedule.CurrentRow.IsNewRow)
                _selectedCalendarSessionId = null;

            UpdateSelectedSessionSummary(GetSelectedSession());
        }

        private StudentScheduleItemModel? GetSelectedSession()
        {
            if (_selectedCalendarSessionId.HasValue)
                return _sessions.FirstOrDefault(s => s.SessionId == _selectedCalendarSessionId.Value);

            if (dgvSchedule.CurrentRow == null || dgvSchedule.CurrentRow.IsNewRow)
                return null;

            object? idValue = dgvSchedule.CurrentRow.Cells["SessionId"]?.Value;
            if (idValue == null || !int.TryParse(idValue.ToString(), out int sessionId))
                return null;

            return _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        }

        private void UpdateSelectedSessionSummary(StudentScheduleItemModel? session)
        {
            if (session == null)
            {
                ResetSelectedSessionSummary();
                return;
            }

            ScheduleUxPresentation view = ScheduleUxPresenter.PresentStudent(session, DateTime.Now);
            string title = string.IsNullOrWhiteSpace(session.Title) ? session.CourseName : session.Title;
            _selectedSessionTitle.Text = $"{title} - {view.StatusText}";
            _selectedSessionDetail.Text = view.DetailText;
            _selectedSessionAction.Text = $"Hành động: {view.PrimaryActionText}";
            btnJoinOnline.Text = view.PrimaryActionText;
            btnJoinOnline.Enabled = view.CanJoin;
            _btnQuickNote.Enabled = !_isCalendarView && dgvSchedule.CurrentRow != null && !dgvSchedule.CurrentRow.IsNewRow;
        }

        private void ResetSelectedSessionSummary()
        {
            _selectedSessionTitle.Text = "Chưa chọn buổi học";
            _selectedSessionDetail.Text = "Chọn một buổi học trong bảng hoặc lịch để xem trạng thái và tham gia đúng lúc.";
            _selectedSessionAction.Text = "Hành động: Tham gia online";
            btnJoinOnline.Text = "Tham gia online";
            btnJoinOnline.Enabled = false;
            _btnQuickNote.Enabled = false;
        }

        private void CalendarSessionLabel_Click(object? sender, EventArgs e)
        {
            if (sender is Label label && label.Tag is int sessionId)
                SelectSessionFromCalendar(sessionId);
        }

        private void SelectSessionFromCalendar(int sessionId)
        {
            _selectedCalendarSessionId = sessionId;
            dgvSchedule.ClearSelection();
            dgvSchedule.CurrentCell = null;
            UpdateSelectedSessionSummary(GetSelectedSession());
        }

        private void EnsureSelectedSessionStillVisible(IEnumerable<int> visibleSessionIds)
        {
            HashSet<int> visible = visibleSessionIds.ToHashSet();
            if (_selectedCalendarSessionId.HasValue && !visible.Contains(_selectedCalendarSessionId.Value))
                _selectedCalendarSessionId = null;
        }

        private void ClearScheduleStateAfterLoadFailure(string emptyMessage)
        {
            _sessions.Clear();
            _filterCache.Clear();
            _selectedCalendarSessionId = null;
            _filterRenderVersion++;

            dgvSchedule.DataSource = null;
            dgvSchedule.ClearSelection();
            dgvSchedule.CurrentCell = null;
            dgvSchedule.Rows.Clear();
            _calendarView.Controls.Clear();
            dgvSchedule.Visible = false;
            _calendarView.Visible = false;
            _emptyStateLabel.Text = emptyMessage;
            _emptyStateLabel.Visible = true;

            ResetSelectedSessionSummary();
        }

        private static DateTime StartOfWeek(DateTime value)
        {
            int offset = ((int)value.DayOfWeek + 6) % 7;
            return value.Date.AddDays(-offset);
        }

        private static void EnableDoubleBuffering(Control control)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(control, true, null);
        }
    }
}
