using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Result : UserControl
    {
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly ComboBox _courseFilter = new();
        private readonly TextBox _searchBox = new();
        private readonly Button _hideResult = new();
        private bool _loadingCourseFilter;

        public UC_Result()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyAcademicStyle();
            WireEvents();
            _ = LoadDataAsync();
        }

        private void ApplyAcademicStyle()
        {
            BackColor = AppColors.BgBase;
            StudentTabChrome.StylePrimaryButton(btnReview);
            StudentTabChrome.StyleSecondaryButton(_hideResult);
            StudentTabChrome.StyleInput(_courseFilter);
            StudentTabChrome.StyleSearchInput(_searchBox);
            StudentTabChrome.StyleGrid(dgvResults);
            RoundedButtonHelper.Apply(btnReview, 10);
            RoundedButtonHelper.Apply(_hideResult, 10);
        }

        private void BuildCardLayout()
        {
            btnReview.Text = "Xem lại bài";
            _hideResult.Text = "Ẩn khỏi Kết quả";
            _courseFilter.Width = 220;
            _searchBox.Width = 240;
            _searchBox.PlaceholderText = "Tìm theo tên bài thi";

            var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            actions.Controls.Add(_courseFilter);
            actions.Controls.Add(_searchBox);
            actions.Controls.Add(btnReview);
            actions.Controls.Add(_hideResult);

            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Kết quả học tập",
                "Xem điểm, trạng thái chấm và mở lại bài làm khi được phép.",
                actions), 0, 0);
            root.Controls.Add(StudentTabChrome.CreateDataCard("Bảng điểm bài kiểm tra", dgvResults), 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvResults);
        }

        private void WireEvents()
        {
            btnReview.Click += (_, _) => ReviewSelectedResult();
            _hideResult.Click += async (_, _) => await HideSelectedResultAsync();
            dgvResults.SelectionChanged += (_, _) => UpdateActionButtons();
            _courseFilter.SelectedIndexChanged += async (_, _) =>
            {
                if (!_loadingCourseFilter)
                    await LoadDataAsync();
            };
            _searchBox.TextChanged += async (_, _) => await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.ResultTable);
            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                if (studentId <= 0)
                {
                    BindResultTable(CreateMessageTable("Không xác định được tài khoản học sinh."));
                    return;
                }

                await EnsureCourseFilterLoadedAsync(studentId);
                int selectedCourseId = _courseFilter.SelectedItem is StudentResultCourseFilterModel selected ? selected.CourseId : 0;
                string keyword = _searchBox.Text.Trim();
                DataTable table = await System.Threading.Tasks.Task.Run(() => LoadResultTable(studentId, selectedCourseId, keyword));
                BindResultTable(table);
            }
            catch (Exception ex)
            {
                BindResultTable(CreateMessageTable($"Không thể tải kết quả: {ex.Message}"));
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private async System.Threading.Tasks.Task EnsureCourseFilterLoadedAsync(int studentId)
        {
            if (_courseFilter.Items.Count > 0)
                return;

            var courses = await System.Threading.Tasks.Task.Run(() => _dbContext.GetActiveResultCourseFiltersForStudent(studentId));
            _loadingCourseFilter = true;
            try
            {
                _courseFilter.DisplayMember = nameof(StudentResultCourseFilterModel.CourseName);
                _courseFilter.ValueMember = nameof(StudentResultCourseFilterModel.CourseId);
                _courseFilter.Items.Add(new StudentResultCourseFilterModel { CourseId = 0, CourseName = "Tất cả khóa học" });
                foreach (var course in courses)
                    _courseFilter.Items.Add(course);
                _courseFilter.SelectedIndex = 0;
            }
            finally
            {
                _loadingCourseFilter = false;
            }
        }

        private DataTable LoadResultTable(int studentId, int selectedCourseId, string keyword)
        {
            var results = _dbContext.GetStudentResultItems(studentId, selectedCourseId > 0 ? selectedCourseId : null, keyword);
            DataTable dt = CreateResultTableSchema();

            if (results.Count == 0)
            {
                string message = !string.IsNullOrWhiteSpace(keyword)
                    ? "Không tìm thấy bài thi phù hợp."
                    : selectedCourseId > 0
                        ? "Khóa học này chưa có kết quả bài kiểm tra."
                        : "Bạn chưa có kết quả bài kiểm tra nào.";
                dt.Rows.Add(0, 0, 0, "", message, "", "", "", "");
                return dt;
            }

            foreach (StudentResultListItemModel item in results)
            {
                dt.Rows.Add(
                    item.AttemptId,
                    item.ExamId,
                    item.CourseId,
                    item.ExamStatus,
                    item.ExamTitle,
                    item.CourseName,
                    item.CorrectAnswersText,
                    item.Score.ToString("0.0", CultureInfo.InvariantCulture),
                    item.StatusText);
            }

            return dt;
        }

        private void BindResultTable(DataTable table)
        {
            dgvResults.DataSource = table;
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (string columnName in new[] { "AttemptId", "ExamId", "CourseId", "ExamStatus" })
            {
                if (dgvResults.Columns[columnName] != null)
                    dgvResults.Columns[columnName]!.Visible = false;
            }
            dgvResults.ClearSelection();
            dgvResults.CurrentCell = null;
            UpdateActionButtons();
        }

        private static DataTable CreateResultTableSchema()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AttemptId", typeof(int));
            dt.Columns.Add("ExamId", typeof(int));
            dt.Columns.Add("CourseId", typeof(int));
            dt.Columns.Add("ExamStatus", typeof(string));
            dt.Columns.Add("Kỳ thi", typeof(string));
            dt.Columns.Add("Khóa học", typeof(string));
            dt.Columns.Add("Số câu đúng", typeof(string));
            dt.Columns.Add("Điểm", typeof(string));
            dt.Columns.Add("Xếp loại", typeof(string));
            return dt;
        }

        private static DataTable CreateMessageTable(string message)
        {
            DataTable dt = CreateResultTableSchema();
            dt.Rows.Add(0, 0, 0, "", message, "", "", "", "");
            return dt;
        }

        private void UpdateActionButtons()
        {
            int attemptId = CurrentInt("AttemptId");
            string examStatus = CurrentString("ExamStatus");
            bool hasResult = attemptId > 0;
            btnReview.Enabled = hasResult && string.Equals(examStatus, WorkflowConstants.ExamStatus.Closed, StringComparison.OrdinalIgnoreCase);
            _hideResult.Enabled = hasResult;
        }

        private void ReviewSelectedResult()
        {
            int attemptId = CurrentInt("AttemptId");
            string examStatus = CurrentString("ExamStatus");
            if (attemptId <= 0 || !string.Equals(examStatus, WorkflowConstants.ExamStatus.Closed, StringComparison.OrdinalIgnoreCase))
            {
                MetaTheme.ShowModernDialog("Chỉ có thể xem lại bài kiểm tra đã đóng.", "Thông báo");
                return;
            }

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            var review = _dbContext.GetStudentExamReview(studentId, attemptId);
            if (review == null)
            {
                MetaTheme.ShowModernDialog("Không tìm thấy dữ liệu xem lại cho bài kiểm tra này.", "Thông báo");
                return;
            }

            using var form = new CourseGuard.Frontend.Forms.Student.StudentExamReviewForm(review);
            form.ShowDialog(FindForm());
        }

        private async System.Threading.Tasks.Task HideSelectedResultAsync()
        {
            int attemptId = CurrentInt("AttemptId");
            if (attemptId <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một kết quả hợp lệ.", "Thông báo");
                return;
            }

            if (MetaTheme.ShowModernDialog("Ẩn kết quả này khỏi trang Kết quả của bạn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (_dbContext.HideStudentResult(studentId, attemptId))
                await LoadDataAsync();
        }

        private int CurrentInt(string columnName)
        {
            if (dgvResults.CurrentRow == null || dgvResults.CurrentRow.IsNewRow)
                return 0;
            object? value = dgvResults.CurrentRow.Cells[columnName].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private string CurrentString(string columnName)
        {
            if (dgvResults.CurrentRow == null || dgvResults.CurrentRow.IsNewRow)
                return string.Empty;
            return dgvResults.CurrentRow.Cells[columnName].Value?.ToString() ?? string.Empty;
        }
    }
}
