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
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public partial class DoExamForm : Form
    {
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly AuthController _authController = new(new CourseGuardDbContext(""));
        private readonly int _examId;
        private readonly System.Threading.CancellationTokenSource _monitoringCts = new();
        private readonly Dictionary<int, string> _answers = new();
        private readonly HashSet<int> _markedQuestions = new();
        private readonly List<Button> _questionButtons = new();
        private readonly System.Windows.Forms.Timer _timer = new();
        private StudentScreenStreamClient? _screenStreamClient;
        private StudentExamTakingModel? _exam;
        private int _currentQuestionIndex;
        private bool _loadingQuestion;
        private bool _submitted;
        private bool _suppressExitLog;

        public DoExamForm() : this(0)
        {
        }

        public DoExamForm(int examId)
        {
            _examId = examId;
            InitializeComponent();
            ApplyAcademicExamTheme();
            WireEvents();
            LoadExam();
            StartScreenMonitoring();

            RoundedButtonHelper.Apply(10, btnSubmit, btnPrev, btnNext);
        }

        private void WireEvents()
        {
            btnSubmit.Click += (_, _) => SubmitWithConfirmation();
            btnPrev.Click += (_, _) => MoveQuestion(-1);
            btnNext.Click += (_, _) => MoveQuestion(1);
            chkMark.CheckedChanged += (_, _) => ToggleMark();
            rbA.CheckedChanged += (_, _) => SaveSelectedAnswer("A", rbA.Checked);
            rbB.CheckedChanged += (_, _) => SaveSelectedAnswer("B", rbB.Checked);
            rbC.CheckedChanged += (_, _) => SaveSelectedAnswer("C", rbC.Checked);
            rbD.CheckedChanged += (_, _) => SaveSelectedAnswer("D", rbD.Checked);
            _timer.Interval = 1000;
            _timer.Tick += (_, _) => UpdateTimer();
            FormClosing += DoExamForm_FormClosing;
        }

        private void LoadExam()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (studentId <= 0 || _examId <= 0)
            {
                MetaTheme.ShowModernDialog("Không xác định được bài kiểm tra hoặc tài khoản học sinh.", "Không thể làm bài", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CloseAfterShown();
                return;
            }

            try
            {
                _exam = _dbContext.GetStudentExamForTaking(studentId, _examId);
                if (_exam == null)
                {
                    MetaTheme.ShowModernDialog("Bài kiểm tra này chưa thể làm ở thời điểm hiện tại.", "Không thể làm bài", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CloseAfterShown();
                    return;
                }

                if (_exam.Questions.Count == 0)
                {
                    MetaTheme.ShowModernDialog("Bài kiểm tra chưa có câu hỏi.", "Không thể làm bài", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CloseAfterShown();
                    return;
                }

                foreach (KeyValuePair<int, string> savedAnswer in _exam.SavedAnswers)
                    _answers[savedAnswer.Key] = savedAnswer.Value;

                lblExamName.Text = string.IsNullOrWhiteSpace(_exam.CourseName)
                    ? _exam.ExamTitle
                    : $"{_exam.ExamTitle} - {_exam.CourseName}";
                BuildQuestionButtons();
                ShowQuestion(0);
                _timer.Start();
                UpdateTimer();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể mở bài kiểm tra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CloseAfterShown();
            }
        }

        private void CloseAfterShown()
        {
            _suppressExitLog = true;
            if (IsHandleCreated)
                BeginInvoke(new Action(Close));
            else
                Shown += (_, _) => Close();
        }

        private void BuildQuestionButtons()
        {
            flpQuestions.Controls.Clear();
            _questionButtons.Clear();

            for (int i = 0; i < _exam!.Questions.Count; i++)
            {
                int questionIndex = i;
                Button btn = new Button
                {
                    Text = _exam.Questions[i].DisplayOrder.ToString(),
                    Width = 40,
                    Height = 40,
                    Margin = new Padding(5),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (_, _) => ShowQuestion(questionIndex);
                RoundedButtonHelper.Apply(btn, 10);
                _questionButtons.Add(btn);
                flpQuestions.Controls.Add(btn);
            }
        }

        private void ShowQuestion(int index)
        {
            if (_exam == null || index < 0 || index >= _exam.Questions.Count)
                return;

            _loadingQuestion = true;
            _currentQuestionIndex = index;
            StudentExamTakingQuestionModel question = _exam.Questions[index];
            lblQuestionText.Text = $"Câu {question.DisplayOrder}: {question.QuestionText}";
            rbA.Text = "A. " + question.OptionA;
            rbB.Text = "B. " + question.OptionB;
            rbC.Text = "C. " + question.OptionC;
            rbD.Text = "D. " + question.OptionD;

            rbA.Checked = false;
            rbB.Checked = false;
            rbC.Checked = false;
            rbD.Checked = false;

            if (_answers.TryGetValue(question.QuestionId, out string? answer))
            {
                rbA.Checked = string.Equals(answer, "A", StringComparison.OrdinalIgnoreCase);
                rbB.Checked = string.Equals(answer, "B", StringComparison.OrdinalIgnoreCase);
                rbC.Checked = string.Equals(answer, "C", StringComparison.OrdinalIgnoreCase);
                rbD.Checked = string.Equals(answer, "D", StringComparison.OrdinalIgnoreCase);
            }

            chkMark.Checked = _markedQuestions.Contains(question.QuestionId);
            btnPrev.Enabled = index > 0;
            btnNext.Enabled = index < _exam.Questions.Count - 1;
            _loadingQuestion = false;
            RefreshQuestionButtons();
        }

        private void MoveQuestion(int delta)
        {
            ShowQuestion(_currentQuestionIndex + delta);
        }

        private void ToggleMark()
        {
            if (_loadingQuestion || _exam == null)
                return;

            int questionId = _exam.Questions[_currentQuestionIndex].QuestionId;
            if (chkMark.Checked)
                _markedQuestions.Add(questionId);
            else
                _markedQuestions.Remove(questionId);

            RefreshQuestionButtons();
        }

        private void SaveSelectedAnswer(string option, bool isChecked)
        {
            if (_loadingQuestion || !isChecked || _exam == null)
                return;

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            StudentExamTakingQuestionModel question = _exam.Questions[_currentQuestionIndex];
            _answers[question.QuestionId] = option;
            RefreshQuestionButtons();

            try
            {
                _dbContext.SaveStudentAnswer(studentId, _exam.AttemptId, question.QuestionId, option);
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể lưu đáp án: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshQuestionButtons()
        {
            if (_exam == null)
                return;

            for (int i = 0; i < _questionButtons.Count; i++)
            {
                int questionId = _exam.Questions[i].QuestionId;
                Button button = _questionButtons[i];
                button.BackColor = i == _currentQuestionIndex
                    ? AcademicTheme.Primary
                    : _answers.ContainsKey(questionId)
                        ? Color.FromArgb(220, 252, 231)
                        : Color.White;
                button.ForeColor = i == _currentQuestionIndex ? Color.White : AcademicTheme.TextPrimary;
                button.FlatAppearance.BorderSize = _markedQuestions.Contains(questionId) ? 2 : 0;
                button.FlatAppearance.BorderColor = Color.FromArgb(245, 158, 11);
            }
        }

        private void UpdateTimer()
        {
            if (_exam == null)
                return;

            TimeSpan? remaining = GetRemainingTime();
            if (!remaining.HasValue)
            {
                lblTimer.Text = "--:--";
                return;
            }

            if (remaining.Value <= TimeSpan.Zero)
            {
                lblTimer.Text = "00:00";
                _timer.Stop();
                SubmitExam(autoSubmit: true);
                return;
            }

            lblTimer.Text = remaining.Value.TotalHours >= 1
                ? $"{(int)remaining.Value.TotalHours:00}:{remaining.Value.Minutes:00}:{remaining.Value.Seconds:00}"
                : $"{remaining.Value.Minutes:00}:{remaining.Value.Seconds:00}";
        }

        private TimeSpan? GetRemainingTime()
        {
            if (_exam == null)
                return null;

            DateTime now = DateTime.Now;
            DateTime? dueTime = null;
            if (_exam.DurationMinutes > 0)
                dueTime = _exam.StartTime.AddMinutes(_exam.DurationMinutes);
            if (_exam.CloseTime.HasValue)
                dueTime = dueTime.HasValue && dueTime.Value < _exam.CloseTime.Value ? dueTime : _exam.CloseTime.Value;

            return dueTime.HasValue ? dueTime.Value - now : null;
        }

        private void SubmitWithConfirmation()
        {
            if (_exam == null)
                return;

            int unanswered = _exam.Questions.Count(question => !_answers.ContainsKey(question.QuestionId));
            string message = unanswered > 0
                ? $"Bạn còn {unanswered} câu chưa trả lời. Bạn có chắc chắn muốn nộp bài không?"
                : "Bạn có chắc chắn muốn nộp bài không?";

            DialogResult result = MetaTheme.ShowModernDialog(message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                SubmitExam(autoSubmit: false);
        }

        private void SubmitExam(bool autoSubmit)
        {
            if (_submitted || _exam == null)
                return;

            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                SubmitStudentExamResultModel result = _dbContext.SubmitStudentExam(studentId, _exam.AttemptId);
                _submitted = true;
                _timer.Stop();

                int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
                string username = UserSessionContext.CurrentUsername ?? "không xác định";
                _authController.LogUserActivity(userId, "EXAM_SUBMIT", $"Người dùng {username} đã nộp bài thi.", string.Empty);

                string message = autoSubmit
                    ? $"Hết giờ. Hệ thống đã nộp bài.\nĐiểm: {result.Score:0.##} ({result.CorrectCount}/{result.TotalQuestions} câu đúng)."
                    : $"Đã nộp bài.\nĐiểm: {result.Score:0.##} ({result.CorrectCount}/{result.TotalQuestions} câu đúng).";
                MetaTheme.ShowModernDialog(message, "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể nộp bài: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartScreenMonitoring()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (_examId <= 0 || studentId <= 0)
                return;

            _screenStreamClient = new StudentScreenStreamClient(_examId, studentId);
            _ = System.Threading.Tasks.Task.Run(() => _screenStreamClient.StartAsync(_monitoringCts.Token));
        }

        private void ApplyAcademicExamTheme()
        {
            BackColor = AcademicTheme.AppBackground;
            pnlHeader.BackColor = AcademicTheme.PrimaryStrong;
            pnlMain.BackColor = AcademicTheme.SurfaceLow;
            pnlRightSidebar.BackColor = AcademicTheme.Surface;
            lblExamName.ForeColor = Color.White;
            lblSidebarTitle.ForeColor = AcademicTheme.TextPrimary;
            lblQuestionText.ForeColor = AcademicTheme.TextPrimary;
            btnSubmit.BackColor = Color.FromArgb(34, 197, 94);
            btnNext.BackColor = AcademicTheme.Primary;
            btnNext.ForeColor = Color.White;
            btnPrev.BackColor = AcademicTheme.Surface;
            btnPrev.ForeColor = AcademicTheme.TextSecondary;
            btnPrev.FlatAppearance.BorderColor = AcademicTheme.BorderSoft;
            btnPrev.FlatAppearance.BorderSize = 1;
            chkMark.ForeColor = AcademicTheme.TextSecondary;
        }

        private void DoExamForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _timer.Stop();
            _monitoringCts.Cancel();
            _screenStreamClient?.Dispose();
            if (_submitted || _suppressExitLog)
                return;

            int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
            string username = UserSessionContext.CurrentUsername ?? "không xác định";
            _authController.LogUserActivity(userId, "EXAM_EXIT", $"Người dùng {username} đã thoát màn hình làm bài thi.", string.Empty);
        }
    }
}
