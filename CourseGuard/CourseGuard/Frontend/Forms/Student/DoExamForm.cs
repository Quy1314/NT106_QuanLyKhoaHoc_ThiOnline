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
        private StudentScreenStreamClient? _screenStreamClient;
        private StudentExamTakingModel? _session;
        private StudentExamTakingModel? _preloadedSession;
        private int _currentIndex;
        private bool _loadingQuestion;
        private bool _submitted;

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
        }

        public DoExamForm(int examId, StudentExamTakingModel preloadedSession) : this(examId)
        {
            _preloadedSession = preloadedSession ?? throw new ArgumentNullException(nameof(preloadedSession));
        }

        private void LowLevelKeyboardHook_OnCheatKeyPressed(object? sender, EventArgs e)
        {
            _screenStreamClient?.SendWarning();
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
                }
            };
            rbA.CheckedChanged += (_, _) => SaveSelectedAnswer("A");
            rbB.CheckedChanged += (_, _) => SaveSelectedAnswer("B");
            rbC.CheckedChanged += (_, _) => SaveSelectedAnswer("C");
            rbD.CheckedChanged += (_, _) => SaveSelectedAnswer("D");
            _timer.Interval = 1000;
            _timer.Tick += (_, _) => UpdateTimerText();
        }

        private void LoadExamSession()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (_examId <= 0 || studentId <= 0)
            {
                ShowErrorAndClose("Không xác định được bài kiểm tra hoặc tài khoản học sinh.");
                return;
            }

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
            StartScreenMonitoring();
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

            _screenStreamClient = new StudentScreenStreamClient(_examId, studentId, _session.AttemptId);
            StudentScreenStreamClient client = _screenStreamClient;
            System.Threading.Tasks.Task.Run(() => client.StartAsync(_monitoringCts.Token)).FireAndForgetSafe(this);
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
            if (_dbContext.SaveStudentExamAnswer(UserSessionContext.CurrentUserId ?? 0, _session.AttemptId, question.Id, option))
            {
                question.SelectedOption = option;
                UpdateQuestionButtons();
            }
        }

        private void SubmitExam(bool confirm)
        {
            if (_session == null || _submitted)
                return;

            if (confirm)
            {
                int unansweredCount = _session.Questions.Count(q => string.IsNullOrWhiteSpace(q.SelectedOption));
                string message = unansweredCount > 0 
                    ? $"Bạn còn {unansweredCount} câu chưa trả lời. Bạn có chắc chắn muốn nộp bài không?" 
                    : "Bạn có chắc chắn muốn nộp bài không?";

                DialogResult res = MetaTheme.ShowModernDialog(this, message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res != DialogResult.Yes)
                    return;
            }

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
            _monitoringCts.Cancel();
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
    }
}
