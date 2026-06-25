using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Services.Monitoring;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public partial class DoExamForm : Form
    {
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly AuthController _authController;
        private readonly int _examId;
        private readonly System.Threading.CancellationTokenSource _monitoringCts = new();
        private readonly System.Windows.Forms.Timer _timer = new();
        private readonly System.Windows.Forms.Timer _antiCheatTimer = new();
        private StudentScreenStreamClient? _screenStreamClient;
        private StudentExamTakingModel? _session;
        private StudentExamTakingModel? _preloadedSession;
        private int _currentIndex;
        private bool _loadingQuestion;
        private bool _submitted;
        private bool _connectionLostViolationRecorded;
        private bool _autoSubmitTriggered;
        private bool _isShowingSubmitConfirmation;
        private Label _progressLabel = null!;
        private Label _saveStatusLabel = null!;

        public DoExamForm() : this(0)
        {
        }

        public DoExamForm(int examId)
        {
            _examId = examId;
            _authController = new AuthController(_dbContext);
            InitializeComponent();
            ApplyAcademicExamTheme();
            WireEvents();
            RoundedButtonHelper.Apply(10, btnSubmit, btnPrev, btnNext);
            
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;

            Shown += DoExamForm_Shown;
            this.FormClosing += DoExamForm_FormClosing;

            LowLevelKeyboardHook.OnCheatKeyPressed += LowLevelKeyboardHook_OnCheatKeyPressed;
            LowLevelKeyboardHook.SetHook();
            this.Deactivate += DoExamForm_Deactivate;
        }

        public DoExamForm(int examId, StudentExamTakingModel preloadedSession) : this(examId)
        {
            _preloadedSession = preloadedSession ?? throw new ArgumentNullException(nameof(preloadedSession));
        }

        private void LowLevelKeyboardHook_OnCheatKeyPressed(object? sender, EventArgs e)
        {
            _screenStreamClient?.SendWarning();
            HandleViolationAsync("KEY_PRESS", "RECORDED_AUTOMATICALLY").FireAndForgetSafe(this);
        }

        private void DoExamForm_Deactivate(object? sender, EventArgs e)
        {
            if (_submitted || _autoSubmitTriggered || _isShowingSubmitConfirmation)
                return;

            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            _screenStreamClient?.SendWarning();
            HandleViolationAsync("SCREEN_SWITCH", "RECORDED_AUTOMATICALLY").FireAndForgetSafe(this);
        }

        private void DoExamForm_Shown(object? sender, EventArgs e)
        {
            try
            {
                LoadExamSession();
            }
            catch (Exception ex)
            {
                ShowErrorAndClose("Không thể tải bài kiểm tra: " + ex.Message);
            }
        }

        private void WireEvents()
        {
            btnSubmit.Click += (_, _) => SubmitExam(confirm: true);
            btnPrev.Click += (_, _) => MoveQuestion(-1);
            btnNext.Click += (_, _) => MoveQuestion(1);
            chkMark.CheckedChanged += (_, _) => 
            {
                if (_session != null && !_loadingQuestion)
                {
                    _session.Questions[_currentIndex].IsMarkedForReview = chkMark.Checked;
                    UpdateQuestionButtons();
                    UpdateProgressSummary();
                }
            };
            rbA.CheckedChanged += (_, _) => SaveSelectedAnswer("A");
            rbB.CheckedChanged += (_, _) => SaveSelectedAnswer("B");
            rbC.CheckedChanged += (_, _) => SaveSelectedAnswer("C");
            rbD.CheckedChanged += (_, _) => SaveSelectedAnswer("D");
            _timer.Interval = 1000;
            _timer.Tick += (_, _) => UpdateTimerText();
            _antiCheatTimer.Interval = 1000;
            _antiCheatTimer.Tick += AntiCheatTimer_Tick;
        }

        private void LoadExamSession()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (_examId <= 0 || studentId <= 0)
            {
                ShowErrorAndClose("Không xác định được bài kiểm tra hoặc tài khoản học sinh.");
                return;
            }

            // --- Startup Anti-Cheat Checks ---
            if (AntiCheatEngine.HasMultipleScreens())
            {
                ShowErrorAndClose("Hệ thống phát hiện bạn đang sử dụng nhiều màn hình. Vui lòng ngắt kết nối màn hình phụ trước khi làm bài.");
                return;
            }

            if (AntiCheatEngine.IsRunningInVirtualMachine())
            {
                ShowErrorAndClose("Hệ thống phát hiện ứng dụng đang chạy trên máy ảo. Không cho phép làm bài thi trên máy ảo.");
                return;
            }

            var startupBlacklistedApps = AntiCheatEngine.GetRunningBlacklistedApps(out bool _);
            if (startupBlacklistedApps.Count > 0)
            {
                ShowErrorAndClose($"Hệ thống phát hiện ứng dụng bị cấm đang hoạt động: {string.Join(", ", startupBlacklistedApps)}. Vui lòng tắt các ứng dụng này trước khi bắt đầu thi.");
                return;
            }
            // ---------------------------------

            StudentExamTakingModel? session = _preloadedSession;
            _preloadedSession = null;
            if (session == null)
                session = _dbContext.StartOrResumeStudentExam(studentId, _examId);

            _session = session;
            if (_session == null || _session.Questions.Count == 0)
            {
                ShowErrorAndClose("Bài kiểm tra chưa thể làm ở thời điểm hiện tại.");
                return;
            }

            lblExamName.Text = _session.ExamTitle;
            Text = $"Làm bài - {_session.ExamTitle}";
            BuildQuestionButtons();
            ShowQuestion(0);
            UpdateTimerText();
            _timer.Start();
            _antiCheatTimer.Start();
            StartScreenMonitoring();
            UpdateProgressSummary();
        }

        private void ShowErrorAndClose(string message)
        {
            MetaTheme.ShowModernDialog(this, message, "Thông báo");
            Close();
        }

        private void StartScreenMonitoring()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (_session == null || _examId <= 0 || studentId <= 0)
                return;

            CourseGuard.Backend.Config.AppEnvironment.LoadDotEnvIfExists();
            string relayUrl = CourseGuard.Backend.Config.AppEnvironment.GetOptional("RELAY_WS_URL") ?? "ws://localhost:8080/relay";

            _screenStreamClient = new StudentScreenStreamClient(_examId, studentId, _session.AttemptId, relayUrl);
            StudentScreenStreamClient client = _screenStreamClient;
            client.ConnectionLostThresholdReached += HandleMonitoringConnectionLost;
            System.Threading.Tasks.Task.Run(() => client.StartAsync(_monitoringCts.Token)).FireAndForgetSafe(this);
        }

        private void HandleMonitoringConnectionLost(object? sender, ScreenMonitorConnectionLostEventArgs e)
        {
            if (_submitted || _connectionLostViolationRecorded)
                return;

            _connectionLostViolationRecorded = true;
            RecordConnectionLostViolationAsync(e).FireAndForgetSafe(this);
            ShowConnectionLostWarning(e.DisconnectedFor);
        }

        private async System.Threading.Tasks.Task RecordConnectionLostViolationAsync(ScreenMonitorConnectionLostEventArgs e)
        {
            try
            {
                await HandleViolationAsync(
                    ScreenMonitorConnectionLossTracker.ViolationType,
                    "RECORDED_AUTOMATICALLY",
                    e.StudentId,
                    e.AttemptId);

                string username = UserSessionContext.CurrentUsername ?? "không xác định";
                await _authController.LogUserActivityAsync(
                    e.StudentId,
                    "EXAM_MONITOR_CONNECTION_LOST",
                    $"Người dùng {username} mất kết nối giám sát màn hình trong bài thi ID={e.ExamId}, attempt={e.AttemptId}, thời lượng={e.DisconnectedFor.TotalSeconds:0}s.",
                    string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot record connection lost violation: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task HandleViolationAsync(string type, string? actionTaken = null, int? studentIdOverride = null, int? attemptIdOverride = null)
        {
            if (_submitted)
                return;

            int studentId = studentIdOverride ?? UserSessionContext.CurrentUserId ?? 0;
            int attemptId = attemptIdOverride ?? _session?.AttemptId ?? 0;
            if (studentId <= 0 || attemptId <= 0)
                return;

            var violationRepository = new ViolationRepository();
            await violationRepository.InsertViolationAsync(new ViolationModel
            {
                UserId = studentId,
                ExamAttemptId = attemptId,
                Type = type,
                Severity = ViolationSeverityMap.Get(type),
                ActionTaken = actionTaken
            });

            await EvaluateViolationThresholdAsync(violationRepository, attemptId);
        }

        private async System.Threading.Tasks.Task EvaluateViolationThresholdAsync(ViolationRepository violationRepository, int attemptId)
        {
            int maxViolations = _session?.MaxViolations ?? 0;
            if (maxViolations <= 0 || _submitted || _autoSubmitTriggered)
                return;

            int violationCount = await violationRepository.CountViolationsByAttemptIdAsync(attemptId);
            if (violationCount < maxViolations)
                return;

            _autoSubmitTriggered = true;

            void SubmitForThreshold()
            {
                if (IsDisposed || _submitted)
                    return;

                MetaTheme.ShowModernDialog(
                    this,
                    $"Ban da dat nguong {maxViolations} vi pham. He thong se tu dong nop bai.",
                    "Tu dong nop bai",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                SubmitExam(confirm: false);
            }

            try
            {
                if (InvokeRequired)
                    BeginInvoke(new MethodInvoker(SubmitForThreshold));
                else
                    SubmitForThreshold();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void ShowConnectionLostWarning(TimeSpan disconnectedFor)
        {
            void ShowWarning()
            {
                if (IsDisposed)
                    return;

                string seconds = Math.Ceiling(disconnectedFor.TotalSeconds).ToString("0");
                MetaTheme.ShowModernDialog(
                    this,
                    $"Giám sát màn hình bị mất kết nối quá {seconds} giây. Hệ thống đã ghi nhận CONNECTION_LOST, vui lòng kiểm tra mạng và tiếp tục làm bài trong màn hình thi.",
                    "Cảnh báo giám sát",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            try
            {
                if (InvokeRequired)
                    BeginInvoke(new MethodInvoker(ShowWarning));
                else
                    ShowWarning();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void ApplyAcademicExamTheme()
        {
            btnSubmit.Text = "Nộp bài";
            btnPrev.Text = "< Trước";
            btnNext.Text = "Tiếp >";
            chkMark.Text = "Đánh dấu xem lại";
            lblSidebarTitle.Text = "Danh sách câu hỏi";
            BackColor = AcademicTheme.AppBackground;
            pnlHeader.BackColor = AcademicTheme.PrimaryStrong;
            pnlMain.BackColor = AcademicTheme.SurfaceLow;
            pnlRightSidebar.BackColor = AcademicTheme.Surface;
            lblExamName.ForeColor = Color.White;
            lblSidebarTitle.ForeColor = AcademicTheme.TextPrimary;
            lblQuestionText.ForeColor = AcademicTheme.TextPrimary;
            btnSubmit.BackColor = Color.FromArgb(34, 197, 94);
            btnSubmit.ForeColor = Color.White;
            btnNext.BackColor = AcademicTheme.Primary;
            btnNext.ForeColor = Color.White;
            btnPrev.BackColor = AcademicTheme.Surface;
            btnPrev.ForeColor = AcademicTheme.TextSecondary;
            btnPrev.FlatAppearance.BorderColor = AcademicTheme.BorderSoft;
            btnPrev.FlatAppearance.BorderSize = 1;
            chkMark.ForeColor = AcademicTheme.TextSecondary;
            foreach (RadioButton option in Options())
                option.ForeColor = AcademicTheme.TextPrimary;
            EnsureProgressHeader();
        }

        private void EnsureProgressHeader()
        {
            if (_progressLabel != null && !_progressLabel.IsDisposed)
                return;

            const int statusRowGap = 4;
            const int statusRowHeight = 28;
            const int bottomPadding = 8;
            int statusRowTop = Math.Max(lblExamName.Bottom, lblTimer.Bottom) + statusRowGap;
            pnlHeader.Height = Math.Max(pnlHeader.Height, statusRowTop + statusRowHeight + bottomPadding);

            _progressLabel = new Label
            {
                AutoSize = false,
                Width = 260,
                Height = statusRowHeight,
                Top = statusRowTop,
                Left = lblExamName.Left,
                Font = AppFonts.Caption,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            _saveStatusLabel = new Label
            {
                AutoSize = false,
                Width = 320,
                Height = statusRowHeight,
                Top = statusRowTop,
                Left = _progressLabel.Right + 12,
                Font = AppFonts.Caption,
                ForeColor = Color.FromArgb(219, 234, 254),
                BackColor = Color.Transparent
            };

            pnlHeader.Controls.Add(_progressLabel);
            pnlHeader.Controls.Add(_saveStatusLabel);
            _progressLabel.BringToFront();
            _saveStatusLabel.BringToFront();
        }

        private void BuildQuestionButtons()
        {
            flpQuestions.Controls.Clear();
            if (_session == null)
                return;

            for (int i = 0; i < _session.Questions.Count; i++)
            {
                Button btn = new Button
                {
                    Text = _session.Questions[i].DisplayOrder.ToString(),
                    Tag = i,
                    Width = 40,
                    Height = 40,
                    Margin = new Padding(5),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (_, _) => ShowQuestion((int)btn.Tag);
                RoundedButtonHelper.Apply(btn, 10);
                flpQuestions.Controls.Add(btn);
            }
        }

        private void ShowQuestion(int index)
        {
            if (_session == null || index < 0 || index >= _session.Questions.Count)
                return;

            _currentIndex = index;
            StudentExamTakingQuestionModel question = _session.Questions[index];
            _loadingQuestion = true;
            lblQuestionText.Text = $"Câu {question.DisplayOrder}: {question.QuestionText}";
            rbA.Text = $"A. {question.OptionA}";
            rbB.Text = $"B. {question.OptionB}";
            rbC.Text = $"C. {question.OptionC}";
            rbD.Text = $"D. {question.OptionD}";
            foreach (RadioButton option in Options())
                option.Checked = false;
            switch (question.SelectedOption)
            {
                case "A": rbA.Checked = true; break;
                case "B": rbB.Checked = true; break;
                case "C": rbC.Checked = true; break;
                case "D": rbD.Checked = true; break;
            }
            chkMark.Checked = question.IsMarkedForReview;
            _loadingQuestion = false;

            btnPrev.Enabled = index > 0;
            btnNext.Enabled = index < _session.Questions.Count - 1;
            UpdateQuestionButtons();
            UpdateProgressSummary();
        }

        private void MoveQuestion(int delta)
        {
            ShowQuestion(_currentIndex + delta);
        }

        private void SaveSelectedAnswer(string option)
        {
            if (_loadingQuestion || _session == null)
                return;

            RadioButton selected = option switch
            {
                "A" => rbA,
                "B" => rbB,
                "C" => rbC,
                "D" => rbD,
                _ => rbA
            };
            if (!selected.Checked)
                return;

            StudentExamTakingQuestionModel question = _session.Questions[_currentIndex];
            bool saved;
            try
            {
                saved = _dbContext.SaveStudentExamAnswer(UserSessionContext.CurrentUserId ?? 0, _session.AttemptId, question.Id, option);
            }
            catch (Exception)
            {
                UpdateSaveStatus(false);
                ShowQuestion(_currentIndex);
                return;
            }

            if (saved)
            {
                question.SelectedOption = option;
                UpdateQuestionButtons();
                UpdateProgressSummary();
                UpdateSaveStatus(true);
                return;
            }

            UpdateSaveStatus(false);
            ShowQuestion(_currentIndex);
        }

        private void UpdateProgressSummary()
        {
            if (_session == null || _progressLabel == null)
                return;

            int answered = _session.Questions.Count(q => !string.IsNullOrWhiteSpace(q.SelectedOption));
            int marked = _session.Questions.Count(q => q.IsMarkedForReview);
            _progressLabel.Text = ExamProgressPresenter.BuildProgressText(answered, _session.Questions.Count, marked);
        }

        private void UpdateSaveStatus(bool success)
        {
            if (_saveStatusLabel == null)
                return;

            _saveStatusLabel.Text = ExamProgressPresenter.BuildSaveStatus(success, DateTime.Now);
            _saveStatusLabel.ForeColor = success ? Color.FromArgb(219, 234, 254) : Color.FromArgb(254, 202, 202);
        }

        private void SubmitExam(bool confirm)
        {
            if (_session == null || _submitted)
                return;

            if (confirm)
            {
                int unansweredCount = _session.Questions.Count(q => string.IsNullOrWhiteSpace(q.SelectedOption));
                int markedCount = _session.Questions.Count(q => q.IsMarkedForReview);
                string message = ExamProgressPresenter.BuildSubmitConfirmMessage(unansweredCount, markedCount);

                _isShowingSubmitConfirmation = true;
                DialogResult res = MetaTheme.ShowModernDialog(this, message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                _isShowingSubmitConfirmation = false;
                if (res != DialogResult.Yes)
                    return;
            }

            btnSubmit.Enabled = false;
            string previousSubmitText = btnSubmit.Text;
            btnSubmit.Text = "Đang nộp...";

            try
            {
                StudentExamSubmitResultModel result = _dbContext.SubmitStudentExamAttempt(UserSessionContext.CurrentUserId ?? 0, _session.AttemptId);
                if (!result.Success)
                {
                    MetaTheme.ShowModernDialog(this, result.Message, "Thông báo");
                    return;
                }

                _submitted = true;
                int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
                string username = UserSessionContext.CurrentUsername ?? "không xác định";
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        _authController.LogUserActivity(userId, "EXAM_SUBMIT", $"Người dùng {username} đã nộp bài thi ID={_examId}, điểm={result.Score:0.##}.", string.Empty);
                    }
                    catch { }
                }).FireAndForgetSafe(this);
                MetaTheme.ShowModernDialog(this, $"Đã nộp bài. Điểm của bạn: {result.Score:0.##}/10", "Hoàn tất");
                Close();
            }
            finally
            {
                if (!_submitted && !IsDisposed)
                {
                    btnSubmit.Enabled = true;
                    btnSubmit.Text = previousSubmitText;
                }
            }
        }

        private void UpdateTimerText()
        {
            if (_session == null || _session.DurationMinutes <= 0)
            {
                lblTimer.Text = "--:--";
                return;
            }

            DateTime end = _session.StartTime.AddMinutes(_session.DurationMinutes);
            TimeSpan remaining = end - DateTime.Now;
            if (remaining <= TimeSpan.Zero)
            {
                _timer.Stop();
                lblTimer.Text = "00:00";
                MetaTheme.ShowModernDialog(this, "Đã hết thời gian làm bài. Hệ thống sẽ tự động nộp bài.", "Hết giờ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SubmitExam(confirm: false);
                return;
            }
            lblTimer.Text = $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00}";
        }

        private void UpdateQuestionButtons()
        {
            if (_session == null)
                return;

            for (int i = 0; i < flpQuestions.Controls.Count; i++)
            {
                if (flpQuestions.Controls[i] is not Button button)
                    continue;

                var question = _session.Questions[i];
                bool answered = !string.IsNullOrWhiteSpace(question.SelectedOption);
                
                if (i == _currentIndex)
                {
                    button.BackColor = AcademicTheme.Primary;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = AcademicTheme.Primary;
                }
                else if (question.IsMarkedForReview)
                {
                    button.BackColor = Color.Orange;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = Color.DarkOrange;
                }
                else if (answered)
                {
                    button.BackColor = Color.FromArgb(220, 252, 231);
                    button.ForeColor = Color.FromArgb(15, 23, 42);
                    button.FlatAppearance.BorderColor = AcademicTheme.BorderSoft;
                }
                else
                {
                    button.BackColor = Color.White;
                    button.ForeColor = Color.FromArgb(15, 23, 42);
                    button.FlatAppearance.BorderColor = AcademicTheme.BorderSoft;
                }
            }
        }

        private IEnumerable<RadioButton> Options()
        {
            yield return rbA;
            yield return rbB;
            yield return rbC;
            yield return rbD;
        }

        private void DoExamForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            LowLevelKeyboardHook.Unhook();
            LowLevelKeyboardHook.OnCheatKeyPressed -= LowLevelKeyboardHook_OnCheatKeyPressed;
            
            _timer.Stop();
            _antiCheatTimer.Stop();
            _monitoringCts.Cancel();
            if (_screenStreamClient != null)
                _screenStreamClient.ConnectionLostThresholdReached -= HandleMonitoringConnectionLost;
            _screenStreamClient?.Dispose();
            int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
            string username = UserSessionContext.CurrentUsername ?? "không xác định";
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    _authController.LogUserActivity(userId, "EXAM_EXIT", $"Người dùng {username} đã thoát màn hình làm bài thi ID={_examId}.", string.Empty);
                }
                catch { }
            }).FireAndForgetSafe(this);
        }

        private bool _isAntiCheatDialogShowing;

        private void AntiCheatTimer_Tick(object? sender, EventArgs e)
        {
            if (_submitted || _autoSubmitTriggered || _isAntiCheatDialogShowing)
                return;

            // 1. Overwrite/Clear Clipboard
            AntiCheatEngine.ClearClipboard();

            // 2. Check for multiple screens
            if (AntiCheatEngine.HasMultipleScreens())
            {
                _isAntiCheatDialogShowing = true;
                try
                {
                    _screenStreamClient?.SendWarning();
                    HandleViolationAsync("MULTI_MONITOR", "Hệ thống phát hiện kết nối thêm màn hình phụ trong khi thi.").FireAndForgetSafe(this);
                    MetaTheme.ShowModernDialog(
                        this,
                        "Hệ thống phát hiện bạn đang sử dụng nhiều màn hình. Vui lòng ngắt kết nối màn hình phụ ngay lập tức để tránh bị nộp bài tự động.",
                        "Cảnh báo vi phạm",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                finally
                {
                    _isAntiCheatDialogShowing = false;
                }
                return;
            }

            // 3. Check for blacklisted apps
            var detected = AntiCheatEngine.GetRunningBlacklistedApps(out bool hasRemoteControl);
            if (detected.Count > 0)
            {
                _isAntiCheatDialogShowing = true;
                try
                {
                    _screenStreamClient?.SendWarning();
                    string appList = string.Join(", ", detected);
                    HandleViolationAsync("BLACKLISTED_APP", $"Phát hiện ứng dụng cấm đang chạy: {appList}").FireAndForgetSafe(this);

                    if (hasRemoteControl)
                    {
                        _autoSubmitTriggered = true;
                        MetaTheme.ShowModernDialog(
                            this,
                            $"Hệ thống phát hiện ứng dụng điều khiển từ xa đang chạy ({appList}). Bài thi của bạn sẽ tự động được nộp lập tức để bảo mật đề thi.",
                            "Tự động nộp bài",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        SubmitExam(confirm: false);
                    }
                    else
                    {
                        MetaTheme.ShowModernDialog(
                            this,
                            $"Hệ thống phát hiện ứng dụng cấm đang chạy: {appList}. Vui lòng đóng ứng dụng này ngay lập tức.",
                            "Cảnh báo vi phạm",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                finally
                {
                    _isAntiCheatDialogShowing = false;
                }
            }
        }
    }
}
