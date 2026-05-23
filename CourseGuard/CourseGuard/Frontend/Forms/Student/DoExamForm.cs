using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Services.Monitoring;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public partial class DoExamForm : Form
    {
        private readonly AuthController _authController = new(new CourseGuardDbContext(""));
        private readonly int _examId;
        private readonly System.Threading.CancellationTokenSource _monitoringCts = new();
        private StudentScreenStreamClient? _screenStreamClient;

        public DoExamForm() : this(0)
        {
        }

        public DoExamForm(int examId)
        {
            _examId = examId;
            InitializeComponent();
            ApplyAcademicExamTheme();
            LoadDummyQuestionsBox();
            btnSubmit.Click += (s, e) => {
                var res = MetaTheme.ShowModernDialog("Bạn có chắc chắn muốn nộp bài không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
                    string username = UserSessionContext.CurrentUsername ?? "không xác định";
                    _authController.LogUserActivity(userId, "EXAM_SUBMIT", $"Người dùng {username} đã nộp bài thi.", string.Empty);
                    this.Close();
                }
            };
            this.FormClosing += DoExamForm_FormClosing;
            StartScreenMonitoring();

            // Bo góc buttons
            RoundedButtonHelper.Apply(10, btnSubmit, btnPrev, btnNext);
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

        private void LoadDummyQuestionsBox()
        {
            for (int i = 1; i <= 50; i++)
            {
                Button btn = new Button
                {
                    Text = i.ToString(),
                    Width = 40,
                    Height = 40,
                    Margin = new Padding(5),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White
                };
                btn.FlatAppearance.BorderSize = 0;
                if (i <= 5) btn.BackColor = Color.FromArgb(220, 252, 231);
                if (i == 6) btn.BackColor = Color.FromArgb(254, 243, 199);
                RoundedButtonHelper.Apply(btn, 10);
                flpQuestions.Controls.Add(btn);
            }
        }

        private void DoExamForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _monitoringCts.Cancel();
            _screenStreamClient?.Dispose();
            int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
            string username = UserSessionContext.CurrentUsername ?? "không xác định";
            _authController.LogUserActivity(userId, "EXAM_EXIT", $"Người dùng {username} đã thoát màn hình làm bài thi.", string.Empty);
        }
    }
}
