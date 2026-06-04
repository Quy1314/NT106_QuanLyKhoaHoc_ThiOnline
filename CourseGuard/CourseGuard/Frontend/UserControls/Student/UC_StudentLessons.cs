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
    public sealed class UC_StudentLessons : StudentGridPageBase, IStudentSearchTarget
    {
        private const string AllCoursesText = "Tất cả khóa học";

        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly List<TeacherLessonModel> _lessons = new();
        private readonly Button _detailButton = new() { Text = "Xem chi tiết" };

        public UC_StudentLessons()
            : base(
                "Bài học & Tài liệu",
                "Xem nội dung bài học và tải tài liệu từ khóa học.",
                "Danh sách bài học",
                "Chưa có bài học nào từ giáo viên.",
                hintText: "Chỉ hiển thị bài học thuộc các khóa học bạn đã được duyệt tham gia.",
                showSearch: true,
                searchPlaceholder: "Tìm theo tên bài học hoặc khóa học...",
                showCourseFilter: true,
                showSearchButton: true)
        {
            Name = "UC_StudentLessons";
            Size = new Size(960, 560);

            _detailButton.Enabled = false;
            StudentTabChrome.StylePrimaryButton(_detailButton);
            AddHeaderAction(_detailButton);

            _detailButton.Click += (_, _) => OpenSelectedLesson();
            Grid.CellDoubleClick += (_, _) => OpenSelectedLesson();

            LoadDataAsync().FireAndForgetSafe(this);
        }

        protected override string LoadErrorMessagePrefix => "Lỗi tải bài học";

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
                _lessons.Clear();
                ReloadCourseFilter();
                return CreateLessonsTable(new List<TeacherLessonModel>());
            }

            var rows = await _dbContext.GetStudentLessonsAsync(studentId);
            _lessons.Clear();
            _lessons.AddRange(rows);

            ReloadCourseFilter();
            return CreateLessonsTable(GetFilteredLessons().ToList());
        }

        protected override string GetEmptyMessage()
        {
            return string.IsNullOrWhiteSpace(SearchBox?.Text)
                ? "Chưa có bài học nào từ giáo viên."
                : "Không tìm thấy bài học phù hợp.";
        }

        protected override void OnSearchRequested() => ApplyFilter();

        protected override void OnCourseFilterChanged() => ApplyFilter();

        protected override void OnTableBound(DataTable table, bool hasRows)
        {
            _detailButton.Enabled = hasRows;
        }

        public void ApplyGlobalSearch(string keyword)
        {
            if (SearchBox != null)
                SearchBox.Text = keyword ?? string.Empty;

            if (_lessons.Count > 0)
                ApplyFilter();
        }

        private void ReloadCourseFilter()
        {
            if (CourseFilter == null)
                return;

            string? selected = CourseFilter.SelectedItem?.ToString();
            var courses = _lessons
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
            SetGridTable(CreateLessonsTable(GetFilteredLessons().ToList()));
        }

        private IEnumerable<TeacherLessonModel> GetFilteredLessons()
        {
            string keyword = SearchBox?.Text.Trim().ToLowerInvariant() ?? string.Empty;
            string courseFilter = CourseFilter?.SelectedItem?.ToString() ?? AllCoursesText;

            IEnumerable<TeacherLessonModel> filtered = _lessons;

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

        private static DataTable CreateLessonsTable(List<TeacherLessonModel> rows)
        {
            DataTable table = new();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Khóa học", typeof(string));
            table.Columns.Add("Tên bài học", typeof(string));
            table.Columns.Add("Ngày đăng", typeof(string));
            table.Columns.Add("Tài liệu đính kèm", typeof(string));
            table.Columns.Add("Kích thước", typeof(string));

            foreach (var row in rows)
            {
                table.Rows.Add(
                    row.Id,
                    row.CourseName,
                    row.Title,
                    row.PublishAt.HasValue ? row.PublishAt.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    row.HasStoredContent ? row.FileName : "Không có",
                    FormatSize(row.FileSize ?? 0));
            }

            return table;
        }

        private void OpenSelectedLesson()
        {
            int lessonId = CurrentInt("ID");
            if (lessonId <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một bài học.", "Thông báo");
                return;
            }

            var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId);
            if (lesson == null)
                return;

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            using var dialog = new CourseGuard.Frontend.Forms.Student.StudentLessonDetailDialog(_dbContext, lesson, studentId);
            dialog.ShowDialog(FindForm());
        }

        private static string FormatSize(long bytes)
        {
            if (bytes <= 0)
                return "";
            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024d:0.#} KB";
            return $"{bytes / 1024d / 1024d:0.#} MB";
        }
    }
}
