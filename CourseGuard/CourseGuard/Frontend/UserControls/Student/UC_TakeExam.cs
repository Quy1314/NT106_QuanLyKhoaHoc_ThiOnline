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
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Services;
using CourseGuard.Frontend.Forms.Student;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_TakeExam : UserControl, IStudentSearchTarget
    {
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly AuthController _authController;
        private DataTable? _examTable;
        private List<StudentExamListItemModel> _exams = new();
        private string _globalSearchKeyword = string.Empty;
        private RoundedPanel _examBody = null!;
        private Label _emptyStateLabel = null!;

        public UC_TakeExam()
        {
            _authController = new AuthController(_dbContext);
            InitializeComponent();
            BuildCardLayout();
            ApplyAcademicStyle();
            LoadDataAsync().FireAndForgetSafe(this);

            RoundedButtonHelper.Apply(btnStartExam, 10);
        }

        private void ApplyAcademicStyle()
        {
            BackColor = AppColors.BgBase;
            StudentTabChrome.StylePrimaryButton(btnStartExam);
            StudentTabChrome.StyleGrid(dgvExams);
        }

        private void BuildCardLayout()
        {
            btnStartExam.Text = "Bắt đầu làm bài";
            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Bài kiểm tra",
                "Theo dõi bài kiểm tra active trong các khóa học đang học và trạng thái làm bài.",
                btnStartExam), 0, 0);
            _examBody = StudentTabChrome.CreateTableBody(dgvExams, out _emptyStateLabel);
            root.Controls.Add(StudentTabChrome.CreateDataCard("Danh sách bài kiểm tra", _examBody), 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvExams);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.ExamListWithToolbar);
            try
            {
                _examTable = await System.Threading.Tasks.Task.Run(LoadExamTable);
                BindExamTable(FilterExamTable(_examTable));
            }
            catch (Exception ex)
            {
                _examTable = CreateExamTableSchema();
                BindExamTable(_examTable, $"Không thể tải bài kiểm tra: {ex.Message}");
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private DataTable LoadExamTable()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (studentId <= 0)
                return CreateExamTableSchema();

            _exams = _dbContext.GetAvailableExamsForStudent(studentId);
            DataTable dt = CreateExamTableSchema();

            foreach (StudentExamListItemModel exam in _exams)
            {
                dt.Rows.Add(
                    exam.Id,
                    exam.CanStart,
                    exam.Title,
                    exam.CourseName,
                    BuildExamTimeText(exam),
                    exam.QuestionCount > 0 ? exam.QuestionCount.ToString(CultureInfo.InvariantCulture) : "N/A",
                    exam.StatusText);
            }

            return dt;
        }

        private void BindExamTable(DataTable table, string emptyMessage = "Chưa có bài kiểm tra active trong các khóa học đang học.")
        {
            if (table.Rows.Count == 0)
            {
                dgvExams.DataSource = null;
                StudentTabChrome.SetTableState(_examBody, dgvExams, _emptyStateLabel, showTable: false, emptyMessage);
                btnStartExam.Enabled = false;
                return;
            }

            dgvExams.DataSource = table;
            dgvExams.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            HideInternalColumn("ExamId");
            HideInternalColumn("CanStart");
            StudentTabChrome.SetTableState(_examBody, dgvExams, _emptyStateLabel, showTable: true, string.Empty);
            btnStartExam.Enabled = true;
            dgvExams.ClearSelection();
            dgvExams.CurrentCell = null;
        }

        public void ApplyGlobalSearch(string keyword)
        {
            _globalSearchKeyword = keyword ?? string.Empty;
            if (_examTable == null)
                return;

            DataTable filtered = FilterExamTable(_examTable);
            string emptyMessage = string.IsNullOrWhiteSpace(_globalSearchKeyword)
                ? "Chưa có bài kiểm tra active trong các khóa học đang học."
                : "Không tìm thấy bài kiểm tra phù hợp";
            BindExamTable(filtered, emptyMessage);
        }

        private DataTable FilterExamTable(DataTable source)
        {
            string keyword = _globalSearchKeyword.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(keyword))
                return source;

            DataTable filtered = source.Clone();
            foreach (DataRow row in source.Rows)
            {
                string text = $"{row["Kỳ thi"]} {row["Khóa học"]} {row["Tình trạng"]}".ToLowerInvariant();
                if (text.Contains(keyword))
                    filtered.ImportRow(row);
            }

            return filtered;
        }

        private void HideInternalColumn(string columnName)
        {
            if (dgvExams.Columns[columnName] != null)
                dgvExams.Columns[columnName]!.Visible = false;
        }

        private static DataTable CreateExamTableSchema()
        {
            DataTable dt = new();
            dt.Columns.Add("ExamId", typeof(int));
            dt.Columns.Add("CanStart", typeof(bool));
            dt.Columns.Add("Kỳ thi", typeof(string));
            dt.Columns.Add("Khóa học", typeof(string));
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Số câu", typeof(string));
            dt.Columns.Add("Tình trạng", typeof(string));
            return dt;
        }

        private static string BuildExamTimeText(StudentExamListItemModel exam)
        {
            string duration = exam.DurationMinutes > 0
                ? $"{exam.DurationMinutes} phút"
                : "Không giới hạn";

            if (exam.OpenTime.HasValue && exam.CloseTime.HasValue)
                return $"{exam.OpenTime.Value:dd/MM HH:mm} - {exam.CloseTime.Value:dd/MM HH:mm} ({duration})";
            if (exam.OpenTime.HasValue)
                return $"Mở từ {exam.OpenTime.Value:dd/MM HH:mm} ({duration})";
            if (exam.CloseTime.HasValue)
                return $"Đến {exam.CloseTime.Value:dd/MM HH:mm} ({duration})";

            return duration;
        }

        private async void btnStartExam_Click(object sender, EventArgs e)
        {
            StudentExamLaunchContext? launchContext = CreateExamLaunchContext(dgvExams.CurrentRow);
            if (!dgvExams.Visible || launchContext == null)
            {
                MetaTheme.ShowModernDialog(this.FindForm(), "Vui lòng chọn một bài kiểm tra.", "Thông báo");
                return;
            }

            if (launchContext.ExamId <= 0 || !launchContext.CanStart)
            {
                StudentExamListItemModel? exam = _exams.FirstOrDefault(item => item.Id == launchContext.ExamId);
                string message = exam == null
                    ? "Bài kiểm tra này chưa thể làm ở thời điểm hiện tại."
                    : StudentExamAvailabilityService.GetStartBlockedMessage(exam);
                MetaTheme.ShowModernDialog(this.FindForm(), message, "Thông báo");
                return;
            }

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (studentId <= 0)
            {
                MetaTheme.ShowModernDialog(this.FindForm(), "Không xác định được tài khoản học sinh.", "Thông báo");
                return;
            }

            bool wasStartButtonEnabled = btnStartExam.Enabled;
            bool wasGridEnabled = dgvExams.Enabled;
            string previousStartButtonText = btnStartExam.Text;
            btnStartExam.Enabled = false;
            btnStartExam.Text = "Đang tải...";
            dgvExams.Enabled = false;
            this.ShowSkeleton(SkeletonType.ExamListWithToolbar);
            bool preloadSkeletonVisible = true;

            try
            {
                StudentExamTakingModel? session = await PreloadExamSessionAsync(studentId, launchContext.ExamId);
                if (session == null || session.Questions.Count == 0)
                {
                    MetaTheme.ShowModernDialog(this.FindForm(), "Bài kiểm tra chưa thể làm ở thời điểm hiện tại.", "Thông báo");
                    return;
                }

                int? userId = studentId > 0 ? studentId : null;
                string username = UserSessionContext.CurrentUsername ?? "unknown";
                _authController.LogUserActivity(userId, "EXAM_JOIN", $"User {username} joined exam: {launchContext.ExamName}", string.Empty);

                this.HideSkeleton();
                preloadSkeletonVisible = false;
                using var form = new DoExamForm(launchContext.ExamId, session);
                form.ShowDialog(this.FindForm());
                LoadDataAsync().FireAndForgetSafe(this);
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog(this.FindForm(), "Không thể tải bài kiểm tra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (preloadSkeletonVisible)
                    this.HideSkeleton();
                btnStartExam.Text = previousStartButtonText;
                btnStartExam.Enabled = wasStartButtonEnabled;
                dgvExams.Enabled = wasGridEnabled;
            }
        }

        private Task<StudentExamTakingModel?> PreloadExamSessionAsync(int studentId, int examId)
        {
            return Task.Run(() => _dbContext.StartOrResumeStudentExam(studentId, examId));
        }

        private static StudentExamLaunchContext? CreateExamLaunchContext(DataGridViewRow? row)
        {
            if (row == null || row.IsNewRow)
                return null;

            return new StudentExamLaunchContext(
                Convert.ToInt32(row.Cells["ExamId"].Value),
                Convert.ToBoolean(row.Cells["CanStart"].Value),
                row.Cells["Kỳ thi"].Value?.ToString() ?? "unknown-exam");
        }

        private sealed class StudentExamLaunchContext
        {
            public StudentExamLaunchContext(int examId, bool canStart, string examName)
            {
                ExamId = examId;
                CanStart = canStart;
                ExamName = examName;
            }

            public int ExamId { get; }
            public bool CanStart { get; }
            public string ExamName { get; }
        }
    }
}
