using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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

        private bool _isCalendarView = true;
        private bool _isOpeningClass;
        private List<TeacherScheduleItemModel> _sessions = new();

        public UC_TeacherSchedule(int teacherId)
            : this(teacherId, new TeacherController(new CourseGuardDbContext("")))
        {
        }

        public UC_TeacherSchedule(int teacherId, TeacherController controller)
        {
            _teacherId = teacherId;
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _classroomSignal = new ClassroomOpenSignalCoordinator(TcpClassroomService.Instance);
            BuildUI();

            LoadDataAsync().FireAndForgetSafe(this);
        }

        private void BuildUI()
        {
            this.BackColor = AppColors.BgBase;
            this.Dock = DockStyle.Fill;
            
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
            for (int i = 0; i < 7; i++) _calendarView.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));

            _btnAdd = TeacherTabChrome.PrimaryButton("Thêm");
            _btnEdit = TeacherTabChrome.SecondaryButton("Sửa");
            _btnDelete = TeacherTabChrome.DangerButton("Xóa");
            _btnRefresh = TeacherTabChrome.SecondaryButton("Tải lại");
            _btnToggleView = TeacherTabChrome.SecondaryButton("Dạng Bảng");
            _btnOpenClass = TeacherTabChrome.PrimaryButton("Dạy online");
            
            _btnAdd.Click += async (_, _) => await AddAsync();
            _btnEdit.Click += async (_, _) => await EditAsync();
            _btnDelete.Click += async (_, _) => await DeleteAsync();
            _btnRefresh.Click += async (_, _) => await LoadDataAsync();
            _btnToggleView.Click += ToggleView;
            _btnOpenClass.Click += async (_, _) => await OpenClassAsync();
            _grid.SelectionChanged += (_, _) => UpdateActionButtons();

            _rootLayout = TeacherTabChrome.CreateRoot(this);
            var header = TeacherTabChrome.CreateHeader("Lịch dạy", "Quản lý lịch học và mở lớp trực tuyến",
                new Control[] { _btnOpenClass, _btnToggleView, _btnAdd, _btnEdit, _btnDelete, _btnRefresh });
            _rootLayout.Controls.Add(header, 0, 0);

            _emptyStateLabel = new Label
            {
                Text = "Chưa có lịch dạy.",
                ForeColor = AppColors.TextSecondary,
                Font = new Font(AppFonts.Body.FontFamily, 12F, FontStyle.Regular),
                AutoSize = true,
                Visible = false
            };
            
            _dataCard = new RoundedPanel { CornerRadius = 10, BackColor = AppColors.BgElevated, Dock = DockStyle.Fill, Padding = new Padding(8) };
            _dataCard.Controls.Add(_emptyStateLabel);
            _dataCard.Controls.Add(_grid);
            _dataCard.Controls.Add(_calendarView);
            
            var scheduleCard = TeacherTabChrome.CreateDataCard("Lịch dạy của bạn", _dataCard);
            scheduleCard.Padding = new Padding(12);
            _rootLayout.Controls.Add(scheduleCard, 0, 1);
        }
        
        private async Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.CalendarView);
            try
            {
                _sessions = await Task.Run(() => _controller.GetSchedule(_teacherId).ToList());
                RefreshView();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Lỗi tải lịch dạy: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.HideSkeleton();
            }
        }
        
        private void RefreshView()
        {
            bool hasRows = _sessions.Count > 0;
            UpdateActionButtons();
            
            if (_isCalendarView)
            {
                _grid.Visible = false;
                _calendarView.Visible = true;
                _emptyStateLabel.Visible = !hasRows;
                RenderCalendar();
            }
            else
            {
                _calendarView.Visible = false;
                _grid.Visible = hasRows;
                _emptyStateLabel.Visible = !hasRows;
                
                var dt = new DataTable();
                dt.Columns.Add("Id", typeof(int));
                dt.Columns.Add("CourseId", typeof(int));
                dt.Columns.Add("Khóa học", typeof(string));
                dt.Columns.Add("Tiêu đề", typeof(string));
                dt.Columns.Add("Bắt đầu", typeof(string));
                dt.Columns.Add("Kết thúc", typeof(string));
                dt.Columns.Add("Link", typeof(string));
                
                foreach (var s in _sessions)
                {
                    dt.Rows.Add(s.Id, s.CourseId, s.CourseName, s.Title, 
                        s.StartTime?.ToString("dd/MM/yyyy HH:mm") ?? "", 
                        s.EndTime?.ToString("dd/MM/yyyy HH:mm") ?? "", 
                        s.MeetingLink);
                }
                
                _grid.DataSource = dt;
                if (_grid.Columns["Id"] != null) _grid.Columns["Id"]!.Visible = false;
                if (_grid.Columns["CourseId"] != null) _grid.Columns["CourseId"]!.Visible = false;
                _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ApplyScheduleGridSizing();
            }
            CenterEmptyLabel();
            UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            bool hasRows = _sessions.Count > 0;
            _btnEdit.Enabled = hasRows && !_isCalendarView;
            _btnDelete.Enabled = hasRows && !_isCalendarView;
            _btnOpenClass.Enabled = hasRows && !_isCalendarView;
        }

        private TeacherScheduleItemModel? GetSelectedSession()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow)
                return null;

            var idValue = _grid.CurrentRow.Cells["Id"]?.Value;
            if (idValue == null || !int.TryParse(idValue.ToString(), out int sessionId))
                return null;

            return _sessions.FirstOrDefault(s => s.Id == sessionId);
        }

        private void ApplyScheduleGridSizing()
        {
            if (_grid.Columns.Count == 0) return;

            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _grid.ScrollBars = ScrollBars.Both;

            if (_grid.Columns["Khóa học"] != null) _grid.Columns["Khóa học"]!.FillWeight = 150;
            if (_grid.Columns["Tiêu đề"] != null) _grid.Columns["Tiêu đề"]!.FillWeight = 180;
            if (_grid.Columns["Bắt đầu"] != null) _grid.Columns["Bắt đầu"]!.FillWeight = 110;
            if (_grid.Columns["Kết thúc"] != null) _grid.Columns["Kết thúc"]!.FillWeight = 110;
            if (_grid.Columns["Link"] != null) _grid.Columns["Link"]!.FillWeight = 220;
        }

        private void CenterEmptyLabel()
        {
            if (_emptyStateLabel.Visible)
            {
                _emptyStateLabel.Location = new Point(
                    (_dataCard.Width - _emptyStateLabel.Width) / 2,
                    (_dataCard.Height - _emptyStateLabel.Height) / 2
                );
            }
        }
        
        private void ToggleView(object? sender, EventArgs e)
        {
            _isCalendarView = !_isCalendarView;
            _btnToggleView.Text = _isCalendarView ? "Dạng Bảng" : "Dạng Lịch";
            RefreshView();
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

        private void RenderCalendar()
        {
            _calendarView.SuspendLayout();
            _calendarView.Controls.Clear();
            _calendarView.RowStyles.Clear();
            
            DateTime today = DateTime.Now.Date;
            DateTime start = today.AddDays(-(int)today.DayOfWeek);
            DateTime end = today.AddMonths(6);
            int totalDays = (end - start).Days + 1;
            int totalWeeks = Math.Max(1, (int)Math.Ceiling(totalDays / 7d));
            
            _calendarView.RowCount = totalWeeks;
            for (int i = 0; i < totalWeeks; i++)
                _calendarView.RowStyles.Add(new RowStyle(SizeType.Absolute, 128f));
            
            for (int row = 0; row < totalWeeks; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    DateTime currentDate = start.AddDays(row * 7 + col);
                    var daySessions = _sessions.Where(s => s.StartTime.HasValue && s.StartTime.Value.Date == currentDate).ToList();
                    
                    bool hasSession = daySessions.Count > 0;
                    Color scheduledDayColor = Color.FromArgb(34, 197, 94);
                    Color scheduledSessionColor = Color.FromArgb(220, 252, 231);
                    Color dayBackColor = hasSession
                        ? scheduledDayColor
                        : currentDate.Date == today ? AppColors.Border : AppColors.BgBase;
                    Color dateTextColor = hasSession ? Color.White : AppColors.TextPrimary;
                    Color sessionTextColor = hasSession ? scheduledSessionColor : AppColors.TextSecondary;
                    
                    var pnl = new RoundedPanel
                    {
                        CornerRadius = 10,
                        BackColor = dayBackColor,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2),
                        Padding = new Padding(5)
                    };
                    
                    var lblDate = new Label
                    {
                        Text = $"{GetVietnameseDayOfWeek(currentDate.DayOfWeek)} - {currentDate:dd/MM}",
                        Font = MetaTheme.Fonts.BodySmBold(),
                        ForeColor = dateTextColor,
                        BackColor = dayBackColor,
                        AutoSize = true,
                        Dock = DockStyle.Top
                    };
                    pnl.Controls.Add(lblDate);
                    
                    foreach (var s in daySessions)
                    {
                        var lblSession = new Label
                        {
                            Text = $"{s.StartTime:HH:mm} - {s.Title}",
                            Font = MetaTheme.Fonts.Caption(),
                            ForeColor = sessionTextColor,
                            BackColor = dayBackColor,
                            AutoSize = false,
                            Height = 38,
                            Dock = DockStyle.Top,
                            MaximumSize = new Size(0, 38)
                        };
                        pnl.Controls.Add(lblSession);
                        lblSession.BringToFront();
                    }
                    _calendarView.Controls.Add(pnl, col, row);
                }
            }

            _calendarView.ResumeLayout();
        }
        
        private async Task OpenClassAsync()
        {
            var selectedSession = GetSelectedSession();
            int sessionId = selectedSession?.Id ?? 0;
            if (_isCalendarView)
            {
                MetaTheme.ShowModernDialog("Vui lòng chuyển sang Dạng Bảng để chọn lịch dạy online.", "Thông báo");
                return;
            }
            if (sessionId <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một lịch dạy để dạy online.", "Thông báo");
                return;
            }
            if (!selectedSession!.StartTime.HasValue)
            {
                MetaTheme.ShowModernDialog("Lịch dạy này chưa có thời gian bắt đầu.", "Thông báo");
                return;
            }
            if (DateTime.Now < selectedSession.StartTime.Value)
            {
                MetaTheme.ShowModernDialog($"Chưa đến giờ dạy online. Lớp này bắt đầu lúc {selectedSession.StartTime:dd/MM/yyyy HH:mm}.", "Thông báo");
                return;
            }

            if (_isOpeningClass)
                return;

            _isOpeningClass = true;
            bool wasOpenClassEnabled = _btnOpenClass.Enabled;
            _btnOpenClass.Enabled = false;
            try
            {
                _classroomSignal.EnsureListeningStarted();
                await _controller.UpdateSessionStatusAsync(_teacherId, sessionId, true);
                await _classroomSignal.BroadcastClassOpenedAsync(sessionId);
                await LoadDataAsync();

                using var onlineClassForm = new TeacherNativeClassroomForm(sessionId);
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
                _btnOpenClass.Enabled = wasOpenClassEnabled;
            }
        }
        
        // --- CRUD Methods ---
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
                _controller.CreateScheduleItem(_teacherId, new TeacherScheduleItemModel { 
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
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return;
            int id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
            var session = _sessions.FirstOrDefault(s => s.Id == id);
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
                _controller.UpdateScheduleItem(_teacherId, new TeacherScheduleItemModel { 
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
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return;
            int id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
            var res = MetaTheme.ShowModernDialog("Bạn có chắc chắn muốn xóa lịch này không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                _controller.DeleteScheduleItem(_teacherId, id);
                await LoadDataAsync();
            }
        }
    }
}
