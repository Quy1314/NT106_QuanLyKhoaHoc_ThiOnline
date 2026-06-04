using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public sealed class UC_StudentAssignments : StudentGridPageBase, IStudentSearchTarget
    {
        private const string AllCoursesText = "Tất cả khóa học";

        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly List<StudentAssignmentRow> _assignments = new();
        private readonly Button _detailButton = new() { Text = "Chi tiết / Nộp bài" };

        private string _globalSearchKeyword = string.Empty;

        public UC_StudentAssignments()
            : base(
                "Bài tập",
                "Xem và nộp các bài tập từ khóa học của bạn.",
                "Danh sách bài tập",
                "Không có bài tập nào.",
                hintText: "Chỉ hiển thị các bài tập từ những khóa học bạn đang tham gia.",
                showCourseFilter: true,
                showSearchButton: true)
        {
            Name = "UC_StudentAssignments";
            Size = new Size(960, 560);

            _detailButton.Enabled = false;
            StudentTabChrome.StylePrimaryButton(_detailButton);
            AddHeaderAction(_detailButton);

            _detailButton.Click += (_, _) => OpenSelectedAssignment();
            Grid.CellDoubleClick += (_, _) => OpenSelectedAssignment();

            LoadDataAsync().FireAndForgetSafe(this);
        }

        protected override string LoadErrorMessagePrefix => "Lỗi tải bài tập";

        protected override async Task<DataTable> CreateTableAsync()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (studentId == 0)
            {
                MetaTheme.ShowModernDialog(
                    "Không xác định được tài khoản. Vui lòng đăng nhập lại.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _assignments.Clear();
                ReloadCourseFilter();
                return CreateAssignmentsTable(new List<StudentAssignmentRow>());
            }

            var rows = await _dbContext.GetStudentAssignmentsAsync(studentId);
            _assignments.Clear();
            _assignments.AddRange(rows);

            ReloadCourseFilter();
            return CreateAssignmentsTable(GetFilteredAssignments().ToList());
        }

        protected override string GetEmptyMessage()
        {
            return string.IsNullOrWhiteSpace(_globalSearchKeyword)
                ? "Không có bài tập nào."
                : "Không tìm thấy bài tập phù hợp.";
        }

        protected override void OnSearchRequested() => ApplyFilter();

        protected override void OnCourseFilterChanged() => ApplyFilter();

        protected override void OnTableBound(DataTable table, bool hasRows)
        {
            _detailButton.Enabled = hasRows;
        }

        public void ApplyGlobalSearch(string keyword)
        {
            _globalSearchKeyword = keyword ?? string.Empty;
            if (_assignments.Count > 0)
                ApplyFilter();
        }

        private void ReloadCourseFilter()
        {
            if (CourseFilter == null)
                return;

            string? selected = CourseFilter.SelectedItem?.ToString();
            var courses = _assignments
                .Select(d => d.CourseName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            CourseFilter.Items.Clear();
            CourseFilter.Items.Add(AllCoursesText);
            foreach (string course in courses)
                CourseFilter.Items.Add(course);

            int selectedIndex = selected != null ? CourseFilter.Items.IndexOf(selected) : -1;
            CourseFilter.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
        }

        private void ApplyFilter()
        {
            SetGridTable(CreateAssignmentsTable(GetFilteredAssignments().ToList()));
        }

        private IEnumerable<StudentAssignmentRow> GetFilteredAssignments()
        {
            string keyword = _globalSearchKeyword.Trim().ToLowerInvariant();
            string courseFilter = CourseFilter?.SelectedItem?.ToString() ?? AllCoursesText;

            IEnumerable<StudentAssignmentRow> filtered = _assignments;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filtered = filtered.Where(d =>
                    d.Title.ToLowerInvariant().Contains(keyword) ||
                    d.CourseName.ToLowerInvariant().Contains(keyword));
            }

            if (courseFilter != AllCoursesText)
                filtered = filtered.Where(d => d.CourseName == courseFilter);

            return filtered;
        }

        private static DataTable CreateAssignmentsTable(List<StudentAssignmentRow> rows)
        {
            DataTable table = new();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Khóa học", typeof(string));
            table.Columns.Add("Tiêu đề", typeof(string));
            table.Columns.Add("Hạn nộp", typeof(string));
            table.Columns.Add("Trạng thái", typeof(string));
            table.Columns.Add("Đã nộp?", typeof(string));
            table.Columns.Add("Điểm", typeof(string));

            foreach (var row in rows)
            {
                string statusText = row.Status == "OPEN" ? "Đã mở" : "Đã đóng";
                string submittedText = row.IsSubmitted ? "Đã nộp" : "Chưa nộp";
                string scoreText = row.Score.HasValue ? row.Score.Value.ToString("0.##") : "Chưa chấm";

                table.Rows.Add(
                    row.AssignmentId,
                    row.CourseName,
                    row.Title,
                    row.DueDate.ToString("dd/MM/yyyy HH:mm"),
                    statusText,
                    submittedText,
                    scoreText);
            }

            return table;
        }

        private void OpenSelectedAssignment()
        {
            int assignmentId = CurrentInt("ID");
            if (assignmentId <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một bài tập.", "Thông báo");
                return;
            }

            var assignment = _assignments.FirstOrDefault(a => a.AssignmentId == assignmentId);
            if (assignment == null)
            {
                MetaTheme.ShowModernDialog("Không tìm thấy thông tin bài tập.", "Lỗi");
                return;
            }

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            using var dialog = new CourseGuard.Frontend.Forms.Student.StudentAssignmentSubmitDialog(_dbContext, assignment, studentId);

            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                LoadDataAsync().FireAndForgetSafe(this);

                if (FindForm() is CourseGuard.Frontend.Forms.Student.StudentDashboard dashboard)
                    dashboard.RefreshNotificationSummary();
            }
        }
    }
}
