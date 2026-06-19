using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services.Realtime;
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherSchedule : UserControl
    {
        private readonly int _teacherId;
        private readonly TeacherController _controller;
        private readonly ClassroomOpenSignalCoordinator _classroomSignal;
        private readonly System.Windows.Forms.Timer _filterDebounceTimer;
        private readonly Dictionary<int, (List<TeacherScheduleItemModel> Sessions, Dictionary<DateTime, List<TeacherScheduleItemModel>> SessionsByDate, DateTime CalendarStart, DateTime CalendarEnd, string EmptyMessage)> _filterCache = new();

        private TableLayoutPanel _rootLayout = null!;
        private RoundedPanel _dataCard = null!;
        private DataGridView _grid = null!;
        private TableLayoutPanel _calendarView = null!;
        private Label _emptyStateLabel = null!;
        private Button _btnToggleView = null!;
        private Button _btnOpenClass = null!;
        private Button _btnAdd = null!;
        private Button _btnEdit = null!;
        private Button _btnDelete = null!;
        private Button _btnRefresh = null!;
        private ComboBox _cboTimeFilter = null!;
        private RoundedPanel _selectedSessionSummary = null!;
        private Label _selectedSessionTitle = null!;
        private Label _selectedSessionDetail = null!;
        private Label _selectedSessionAction = null!;

        private bool _isCalendarView = true;
        private bool _isOpeningClass;
        private int _filterRenderVersion;
        private int? _selectedCalendarSessionId;
        private List<TeacherScheduleItemModel> _sessions = new();
        private List<TeacherScheduleItemModel> _filteredSessions = new();

        public UC_TeacherSchedule(int teacherId)
            : this(teacherId, new TeacherController(new CourseGuardDbContext(string.Empty)))
        {
        }

        public UC_TeacherSchedule(int teacherId, TeacherController controller)
        {
            _teacherId = teacherId;
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _classroomSignal = new ClassroomOpenSignalCoordinator(TcpClassroomService.Instance);
            _filterDebounceTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _filterDebounceTimer.Tick += (_, _) =>
            {
                _filterDebounceTimer.Stop();
                ApplyTimeFilterAsync().FireAndForgetSafe(this);
            };

            BuildUI();
            LoadDataAsync().FireAndForgetSafe(this);
            Disposed += (_, _) => _filterDebounceTimer.Dispose();
        }

        private void BuildUI()
        {
            BackColor = AppColors.BgBase;
            Dock = DockStyle.Fill;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            };
            TeacherTabChrome.StyleGrid(_grid);
            _grid.RowTemplate.Height = 52;
            _grid.ColumnHeadersHeight = 48;

            _calendarView = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 1,
                Visible = false,
                BackColor = AppColors.BgBase,
                AutoScroll = true
            };
            for (int i = 0; i < 7; i++)
                _calendarView.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));

            _btnAdd = TeacherTabChrome.PrimaryButton("Thêm");
            _btnEdit = TeacherTabChrome.SecondaryButton("Sửa");
            _btnDelete = TeacherTabChrome.DangerButton("Xóa");
            _btnRefresh = TeacherTabChrome.SecondaryButton("Tải lại");
            _btnToggleView = TeacherTabChrome.SecondaryButton("Dạng Bảng");
            _btnOpenClass = TeacherTabChrome.PrimaryButton("Dạy online");
            _cboTimeFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 130
            };
            _cboTimeFilter.Items.AddRange(new object[] { "Hôm nay", "Tuần này", "Tháng này", "Tất cả" });
            _cboTimeFilter.SelectedIndex = 3;
            _cboTimeFilter.Font = AppFonts.Body;
            _cboTimeFilter.BackColor = AppColors.BgCard;
            _cboTimeFilter.ForeColor = AppColors.TextPrimary;

            _btnAdd.Click += async (_, _) => await AddAsync();
            _btnEdit.Click += async (_, _) => await EditAsync();
            _btnDelete.Click += async (_, _) => await DeleteAsync();
            _btnRefresh.Click += async (_, _) => await LoadDataAsync();
            _btnToggleView.Click += ToggleView;
            _btnOpenClass.Click += async (_, _) => await OpenClassAsync();
            _cboTimeFilter.SelectedIndexChanged += (_, _) => ScheduleFilterRefresh();
            _grid.SelectionChanged += (_, _) => HandleGridSelectionChanged();

            _rootLayout = TeacherTabChrome.CreateRoot(this);
            _rootLayout.Controls.Add(TeacherTabChrome.CreateHeader(
                "Lịch dạy",
                "Quản lý lịch học và mở lớp trực tuyến",
                new Control[] { _cboTimeFilter, _btnOpenClass, _btnToggleView, _btnAdd, _btnEdit, _btnDelete, _btnRefresh }), 0, 0);

            _dataCard = new RoundedPanel
            {
                CornerRadius = 10,
                BackColor = AppColors.BgElevated,
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            _emptyStateLabel = new Label
            {
                Text = "Chưa có lịch dạy.",
                ForeColor = AppColors.TextSecondary,
                Font = new Font(AppFonts.Body.FontFamily, 12F, FontStyle.Regular),
                AutoSize = true,
                Visible = false
            };
            _dataCard.Controls.Add(_emptyStateLabel);
            _dataCard.Controls.Add(_grid);
            _dataCard.Controls.Add(_calendarView);

            var scheduleCard = TeacherTabChrome.CreateDataCard("Lịch dạy của bạn", _dataCard);
            scheduleCard.Padding = new Padding(12);
            scheduleCard.Margin = new Padding(0, 0, 0, 12);
            EnableDoubleBuffering(this);
            EnableDoubleBuffering(_dataCard);
            EnableDoubleBuffering(_calendarView);
            EnableDoubleBuffering(_grid);
            _rootLayout.Controls.Add(scheduleCard, 0, 1);

            _rootLayout.RowCount = 3;
            _rootLayout.RowStyles.Clear();
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _selectedSessionSummary = TeacherTabChrome.CreateCard();
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
            _rootLayout.Controls.Add(_selectedSessionSummary, 0, 2);

            ResetSelectedSessionSummary();
        }

        private async Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.CalendarView);
            try
            {
                _sessions = (await Task.Run(() => _controller.GetSchedule(_teacherId).ToList()))
                    .OrderBy(s => s.StartTime ?? DateTime.MaxValue)
                    .ThenBy(s => s.CourseName)
                    .ThenBy(s => s.Title)
                    .ToList();
                _filterCache.Clear();
                await ApplyTimeFilterAsync();
            }
            catch (Exception ex)
            {
                ClearScheduleStateAfterLoadFailure($"Lỗi tải lịch dạy: {ex.Message}");
                MetaTheme.ShowModernDialog("Lỗi tải lịch dạy: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void ScheduleFilterRefresh()
        {
            _filterDebounceTimer.Stop();
            _filterDebounceTimer.Start();
        }

        private async Task ApplyTimeFilterAsync()
        {
            int renderVersion = ++_filterRenderVersion;
            int filterIndex = _cboTimeFilter.SelectedIndex;
            if (_filterCache.TryGetValue(filterIndex, out var cached))
            {
                _filteredSessions = cached.Sessions;
                RefreshView(cached.Sessions, cached.SessionsByDate, cached.EmptyMessage, cached.CalendarStart, cached.CalendarEnd);
                return;
            }

            var prepared = await Task.Run(() => PrepareFilterData(_sessions.ToList(), filterIndex));
            if (IsDisposed || renderVersion != _filterRenderVersion)
                return;

            _filterCache[filterIndex] = prepared;
            _filteredSessions = prepared.Sessions;
            RefreshView(prepared.Sessions, prepared.SessionsByDate, prepared.EmptyMessage, prepared.CalendarStart, prepared.CalendarEnd);
        }

        private static (List<TeacherScheduleItemModel> Sessions, Dictionary<DateTime, List<TeacherScheduleItemModel>> SessionsByDate, DateTime CalendarStart, DateTime CalendarEnd, string EmptyMessage) PrepareFilterData(List<TeacherScheduleItemModel> sessions, int filterIndex)
        {
            IEnumerable<TeacherScheduleItemModel> filtered = sessions;
            DateTime now = DateTime.Now;
            string emptyMessage = "Chưa có lịch dạy phù hợp.";
            DateTime calendarStart;
            DateTime calendarEnd;

            switch (filterIndex)
            {
                case 0:
                    calendarStart = now.Date;
                    calendarEnd = now.Date;
                    filtered = filtered.Where(s => s.StartTime.HasValue && s.StartTime.Value.Date == calendarStart);
                    emptyMessage = "Hôm nay chưa có lịch dạy.";
                    break;
                case 1:
                    DateTime startOfWeek = StartOfWeek(now);
                    DateTime endOfWeek = startOfWeek.AddDays(7);
                    filtered = filtered.Where(s => s.StartTime.HasValue && s.StartTime.Value >= startOfWeek && s.StartTime.Value < endOfWeek);
                    calendarStart = startOfWeek;
                    calendarEnd = endOfWeek.AddDays(-1);
                    emptyMessage = "Chưa có lịch dạy trong tuần này.";
                    break;
                case 2:
                    DateTime startOfMonth = new(now.Year, now.Month, 1);
                    DateTime endOfMonth = startOfMonth.AddMonths(1);
                    filtered = filtered.Where(s => s.StartTime.HasValue && s.StartTime.Value >= startOfMonth && s.StartTime.Value < endOfMonth);
                    calendarStart = startOfMonth;
                    calendarEnd = endOfMonth.AddDays(-1);
                    emptyMessage = "Chưa có lịch dạy trong tháng này.";
                    break;
                default:
                    calendarStart = StartOfWeek(now);
                    calendarEnd = now.Date.AddMonths(6);
                    filtered = filtered.Where(s => !s.StartTime.HasValue || (s.StartTime.Value.Date >= calendarStart && s.StartTime.Value.Date <= calendarEnd));
                    break;
            }

            var filteredList = filtered
                .OrderBy(s => s.StartTime ?? DateTime.MaxValue)
                .ThenBy(s => s.CourseName)
                .ThenBy(s => s.Title)
                .ToList();
            var sessionsByDate = filteredList
                .Where(s => s.StartTime.HasValue)
                .GroupBy(s => s.StartTime!.Value.Date)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StartTime).ThenBy(s => s.Title).ToList());

            return (filteredList, sessionsByDate, calendarStart, calendarEnd, emptyMessage);
        }

        private void RefreshView(List<TeacherScheduleItemModel> sessions, Dictionary<DateTime, List<TeacherScheduleItemModel>> sessionsByDate, string emptyMessage, DateTime calendarStart, DateTime calendarEnd)
        {
            EnsureSelectedSessionStillVisible(sessions.Select(s => s.Id));
            _emptyStateLabel.Text = emptyMessage;

            _dataCard.SuspendLayout();
            try
            {
                if (_isCalendarView)
                {
                    _grid.Visible = false;
                    _calendarView.Visible = true;
                    _emptyStateLabel.Visible = !sessions.Any();
                    RenderCalendar(sessions, sessionsByDate, calendarStart, calendarEnd);
                }
                else
                {
                    _calendarView.Visible = false;
                    _grid.Visible = sessions.Any();
                    _emptyStateLabel.Visible = !sessions.Any();
                    BindGrid(sessions);
                }
            }
            finally
            {
                _dataCard.ResumeLayout(true);
            }

            CenterEmptyLabel();
            UpdateSelectedSessionSummary(GetSelectedSession());
            UpdateActionButtons();
        }

        private void BindGrid(List<TeacherScheduleItemModel> sessions)
        {
            _grid.SuspendLayout();
            try
            {
                DataTable dt = new();
                dt.Columns.Add("Id", typeof(int));
                dt.Columns.Add("CourseId", typeof(int));
                dt.Columns.Add("Khóa học", typeof(string));
                dt.Columns.Add("Tiêu đề", typeof(string));
                dt.Columns.Add("Bắt đầu", typeof(string));
                dt.Columns.Add("Kết thúc", typeof(string));
                dt.Columns.Add("Trạng thái", typeof(string));
                dt.Columns.Add("Hành động", typeof(string));
                dt.Columns.Add("Link", typeof(string));

                foreach (TeacherScheduleItemModel session in sessions)
                {
                    ScheduleUxPresentation view = ScheduleUxPresenter.PresentTeacher(session, DateTime.Now);
                    dt.Rows.Add(
                        session.Id,
                        session.CourseId,
                        session.CourseName,
                        session.Title,
                        session.StartTime?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
                        session.EndTime?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
                        view.StatusText,
                        view.PrimaryActionText,
                        session.MeetingLink);
                }

                _grid.DataSource = dt;
                if (_grid.Columns["Id"] != null)
                    _grid.Columns["Id"]!.Visible = false;
                if (_grid.Columns["CourseId"] != null)
                    _grid.Columns["CourseId"]!.Visible = false;
                if (_grid.Columns["Link"] != null)
                    _grid.Columns["Link"]!.Visible = false;
                _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ApplyScheduleGridSizing();
                _grid.ClearSelection();
                _grid.CurrentCell = null;
            }
            finally
            {
                _grid.ResumeLayout();
            }
        }

        private void UpdateActionButtons()
        {
            bool hasGridSelection = !_isCalendarView
                && _grid.CurrentRow != null
                && !_grid.CurrentRow.IsNewRow;
            _btnEdit.Enabled = hasGridSelection;
            _btnDelete.Enabled = hasGridSelection;

            TeacherScheduleItemModel? session = GetSelectedSession();
            if (session == null)
            {
                _btnOpenClass.Text = "Dạy online";
                _btnOpenClass.Enabled = false;
                return;
            }

            ScheduleUxPresentation view = ScheduleUxPresenter.PresentTeacher(session, DateTime.Now);
            _btnOpenClass.Text = view.PrimaryActionText;
            _btnOpenClass.Enabled = view.CanOpenClass && !_isOpeningClass;
        }

        private TeacherScheduleItemModel? GetSelectedSession()
        {
            if (_selectedCalendarSessionId.HasValue)
            {
                return _filteredSessions.FirstOrDefault(s => s.Id == _selectedCalendarSessionId.Value)
                    ?? _sessions.FirstOrDefault(s => s.Id == _selectedCalendarSessionId.Value);
            }

            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow)
                return null;

            object? idValue = _grid.CurrentRow.Cells["Id"]?.Value;
            if (idValue == null || !int.TryParse(idValue.ToString(), out int sessionId))
                return null;

            return _filteredSessions.FirstOrDefault(s => s.Id == sessionId)
                ?? _sessions.FirstOrDefault(s => s.Id == sessionId);
        }

        private void ApplyScheduleGridSizing()
        {
            if (_grid.Columns.Count == 0)
                return;

            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _grid.ScrollBars = ScrollBars.Both;

            if (_grid.Columns["Khóa học"] != null) _grid.Columns["Khóa học"]!.FillWeight = 150;
            if (_grid.Columns["Tiêu đề"] != null) _grid.Columns["Tiêu đề"]!.FillWeight = 180;
            if (_grid.Columns["Bắt đầu"] != null) _grid.Columns["Bắt đầu"]!.FillWeight = 110;
            if (_grid.Columns["Kết thúc"] != null) _grid.Columns["Kết thúc"]!.FillWeight = 110;
            if (_grid.Columns["Trạng thái"] != null) _grid.Columns["Trạng thái"]!.FillWeight = 95;
            if (_grid.Columns["Hành động"] != null) _grid.Columns["Hành động"]!.FillWeight = 115;
        }

        private void CenterEmptyLabel()
        {
            if (_emptyStateLabel.Visible)
            {
                _emptyStateLabel.Location = new Point(
                    (_dataCard.Width - _emptyStateLabel.Width) / 2,
                    (_dataCard.Height - _emptyStateLabel.Height) / 2);
            }
        }

        private void ToggleView(object? sender, EventArgs e)
        {
            _isCalendarView = !_isCalendarView;
            _btnToggleView.Text = _isCalendarView ? "Dạng Bảng" : "Dạng Lịch";
            ApplyTimeFilterAsync().FireAndForgetSafe(this);
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

        private void RenderCalendar(List<TeacherScheduleItemModel> sessions, Dictionary<DateTime, List<TeacherScheduleItemModel>> sessionsByDate, DateTime rangeStart, DateTime rangeEnd)
        {
            _dataCard.SuspendLayout();
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

                for (int row = 0; row < totalWeeks; row++)
                {
                    for (int col = 0; col < 7; col++)
                    {
                        DateTime currentDate = start.AddDays(row * 7 + col);
                        sessionsByDate.TryGetValue(currentDate, out var daySessions);
                        _calendarView.Controls.Add(CreateCalendarDayCell(currentDate, daySessions ?? new List<TeacherScheduleItemModel>(), today), col, row);
                    }
                }
            }
            finally
            {
                _calendarView.ResumeLayout();
                _dataCard.ResumeLayout(true);
            }
        }

        private Control CreateCalendarDayCell(DateTime currentDate, List<TeacherScheduleItemModel> daySessions, DateTime today)
        {
            bool hasSession = daySessions.Count > 0;
            Color scheduledDayColor = Color.FromArgb(34, 197, 94);
            Color scheduledSessionColor = Color.FromArgb(220, 252, 231);
            Color dayBackColor = hasSession
                ? scheduledDayColor
                : currentDate.Date == today ? AppColors.Border : AppColors.BgBase;
            Color dateTextColor = hasSession ? Color.White : AppColors.TextPrimary;
            Color sessionTextColor = hasSession ? scheduledSessionColor : AppColors.TextSecondary;

            var panel = new RoundedPanel
            {
                CornerRadius = 10,
                BackColor = dayBackColor,
                FillColor = dayBackColor,
                BorderColor = hasSession ? Color.FromArgb(22, 163, 74) : AppColors.Border,
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Padding = new Padding(5)
            };
            EnableDoubleBuffering(panel);

            var lblDate = new Label
            {
                Text = $"{GetVietnameseDayOfWeek(currentDate.DayOfWeek)} - {currentDate:dd/MM}",
                Font = MetaTheme.Fonts.BodySmBold(),
                ForeColor = dateTextColor,
                BackColor = dayBackColor,
                AutoSize = true,
                Dock = DockStyle.Top
            };
            panel.Controls.Add(lblDate);

            foreach (TeacherScheduleItemModel session in daySessions.OrderBy(s => s.StartTime).Take(3))
            {
                string title = string.IsNullOrWhiteSpace(session.Title) ? session.CourseName : session.Title;
                var lblSession = new Label
                {
                    Text = $"{session.StartTime:HH:mm} - {title}",
                    Font = MetaTheme.Fonts.Caption(),
                    ForeColor = sessionTextColor,
                    BackColor = dayBackColor,
                    AutoSize = false,
                    Height = 34,
                    Dock = DockStyle.Top,
                    MaximumSize = new Size(0, 34),
                    Cursor = Cursors.Hand,
                    Tag = session.Id
                };
                lblSession.Click += CalendarSessionLabel_Click;
                panel.Controls.Add(lblSession);
                lblSession.BringToFront();
            }

            if (daySessions.Count > 3)
            {
                var lblMore = new Label
                {
                    Text = $"+{daySessions.Count - 3} lịch dạy khác",
                    Font = MetaTheme.Fonts.Caption(),
                    ForeColor = sessionTextColor,
                    BackColor = dayBackColor,
                    AutoSize = false,
                    Height = 22,
                    Dock = DockStyle.Top
                };
                panel.Controls.Add(lblMore);
                lblMore.BringToFront();
            }

            return panel;
        }

        private async Task OpenClassAsync()
        {
            TeacherScheduleItemModel? selectedSession = GetSelectedSession();
            if (selectedSession == null || selectedSession.Id <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một lịch dạy để dạy online.", "Thông báo");
                return;
            }

            ScheduleUxPresentation view = ScheduleUxPresenter.PresentTeacher(selectedSession, DateTime.Now);
            if (!view.CanOpenClass)
            {
                MetaTheme.ShowModernDialog($"{view.StatusText}: {view.DetailText}", "Thông báo");
                UpdateSelectedSessionSummary(selectedSession);
                UpdateActionButtons();
                return;
            }

            if (_isOpeningClass)
                return;

            _isOpeningClass = true;
            UpdateActionButtons();
            try
            {
                _classroomSignal.EnsureListeningStarted();
                bool classroomOpened = false;
                using var onlineClassForm = new TeacherNativeClassroomForm(
                    selectedSession.Id,
                    async () =>
                    {
                        bool openedStatusUpdated = await _controller.UpdateSessionStatusAsync(_teacherId, selectedSession.Id, true);
                        if (!openedStatusUpdated)
                        {
                            throw new InvalidOperationException("Không thể cập nhật trạng thái buổi học sang đang mở.");
                        }

                        try
                        {
                            await _classroomSignal.BroadcastClassOpenedAsync(selectedSession.Id);
                            classroomOpened = true;
                            await LoadDataAsync();
                        }
                        catch
                        {
                            await _controller.UpdateSessionStatusAsync(_teacherId, selectedSession.Id, false);
                            throw;
                        }
                    },
                    async () =>
                    {
                        if (!classroomOpened)
                        {
                            return;
                        }

                        bool updated = await _controller.UpdateSessionStatusAsync(_teacherId, selectedSession.Id, false);
                        if (!updated)
                        {
                            throw new InvalidOperationException("Không thể cập nhật trạng thái buổi học sang đã đóng.");
                        }

                        await _classroomSignal.BroadcastClassClosedAsync(selectedSession.Id);
                        await LoadDataAsync();
                    });
                onlineClassForm.ShowDialog(FindForm());
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Lỗi khi vào dạy online: " + ex.Message, "Lỗi");
            }
            finally
            {
                _isOpeningClass = false;
                UpdateActionButtons();
            }
        }

        private async Task AddAsync()
        {
            DateTime defaultStart = DateTime.Now.Date.AddHours(Math.Max(DateTime.Now.Hour, 7));
            using var dialog = new TeacherSimpleItemDialog(
                "Thêm lịch dạy",
                _controller.GetCourses(_teacherId),
                status: "ACTIVE",
                enableTimeRange: true,
                selectedStartTime: defaultStart,
                selectedEndTime: defaultStart.AddHours(2));
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                _controller.CreateScheduleItem(_teacherId, new TeacherScheduleItemModel
                {
                    CourseId = dialog.CourseId,
                    Title = dialog.ItemTitle,
                    StartTime = dialog.SelectedStartTime,
                    EndTime = dialog.SelectedEndTime,
                    MeetingLink = dialog.Details
                });
                await LoadDataAsync();
            }
        }

        private async Task EditAsync()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow)
                return;

            int id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
            TeacherScheduleItemModel? session = _sessions.FirstOrDefault(s => s.Id == id);
            if (session == null)
            {
                MetaTheme.ShowModernDialog("Không tìm thấy lịch dạy cần sửa.", "Thông báo");
                return;
            }

            DateTime selectedStart = session.StartTime ?? DateTime.Now;
            DateTime selectedEnd = session.EndTime ?? selectedStart.AddHours(2);

            using var dialog = new TeacherSimpleItemDialog(
                "Sửa lịch dạy",
                _controller.GetCourses(_teacherId),
                session.Title,
                session.MeetingLink,
                "ACTIVE",
                session.CourseId,
                enableTimeRange: true,
                selectedStartTime: selectedStart,
                selectedEndTime: selectedEnd);
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                _controller.UpdateScheduleItem(_teacherId, new TeacherScheduleItemModel
                {
                    Id = id,
                    CourseId = dialog.CourseId,
                    Title = dialog.ItemTitle,
                    StartTime = dialog.SelectedStartTime,
                    EndTime = dialog.SelectedEndTime,
                    MeetingLink = dialog.Details
                });
                await LoadDataAsync();
            }
        }

        private async Task DeleteAsync()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow)
                return;

            int id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
            DialogResult result = MetaTheme.ShowModernDialog("Bạn có chắc chắn muốn xóa lịch này không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _controller.DeleteScheduleItem(_teacherId, id);
                await LoadDataAsync();
            }
        }

        private static DateTime? ParseGridDate(string value)
        {
            return DateTime.TryParseExact(value, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed)
                ? parsed
                : null;
        }

        private void HandleGridSelectionChanged()
        {
            if (_grid.Focused && _grid.CurrentRow != null && !_grid.CurrentRow.IsNewRow)
                _selectedCalendarSessionId = null;

            UpdateSelectedSessionSummary(GetSelectedSession());
            UpdateActionButtons();
        }

        private void UpdateSelectedSessionSummary(TeacherScheduleItemModel? session)
        {
            if (session == null)
            {
                ResetSelectedSessionSummary();
                return;
            }

            ScheduleUxPresentation view = ScheduleUxPresenter.PresentTeacher(session, DateTime.Now);
            string title = string.IsNullOrWhiteSpace(session.Title) ? session.CourseName : session.Title;
            _selectedSessionTitle.Text = $"{title} - {view.StatusText}";
            _selectedSessionDetail.Text = view.DetailText;
            _selectedSessionAction.Text = $"Hành động: {view.PrimaryActionText}";
        }

        private void ResetSelectedSessionSummary()
        {
            _selectedSessionTitle.Text = "Chưa chọn lịch dạy";
            _selectedSessionDetail.Text = "Chọn một buổi dạy trong bảng hoặc lịch để xem trạng thái và mở lớp đúng lúc.";
            _selectedSessionAction.Text = "Hành động: Dạy online";
        }

        private void CalendarSessionLabel_Click(object? sender, EventArgs e)
        {
            if (sender is Label label && label.Tag is int sessionId)
                SelectSessionFromCalendar(sessionId);
        }

        private void SelectSessionFromCalendar(int sessionId)
        {
            _selectedCalendarSessionId = sessionId;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            UpdateSelectedSessionSummary(GetSelectedSession());
            UpdateActionButtons();
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
            _filteredSessions.Clear();
            _filterCache.Clear();
            _selectedCalendarSessionId = null;

            _grid.DataSource = null;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            _calendarView.Controls.Clear();
            _grid.Visible = false;
            _calendarView.Visible = false;
            _emptyStateLabel.Text = emptyMessage;
            _emptyStateLabel.Visible = true;

            ResetSelectedSessionSummary();
            _btnOpenClass.Text = "Dạy online";
            _btnOpenClass.Enabled = false;
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
            CenterEmptyLabel();
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
