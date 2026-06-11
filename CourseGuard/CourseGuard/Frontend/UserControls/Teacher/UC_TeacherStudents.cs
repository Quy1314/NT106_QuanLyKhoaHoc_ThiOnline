using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherStudents : TeacherGridPageBase, ITeacherQuickSearchTarget
    {
        private readonly TextBox _searchBox = new();
        private List<TeacherStudentModel> _allStudents = new();
        private string _quickSearchKeyword = string.Empty;

        public UC_TeacherStudents(int teacherId, TeacherController controller) : base(teacherId, controller, "Sinh viên", "Duyệt ghi danh và xem danh sách học viên thuộc khóa học của mình.", "Yêu cầu và học viên")
        {
            AddButton.Text = "Duyệt";
            EditButton.Text = "Từ chối";
            TeacherTabChrome.StyleDangerButton(EditButton);
            TeacherTabChrome.FitButtonToText(AddButton);
            TeacherTabChrome.FitButtonToText(EditButton);
            DeleteButton.Visible = false;

            ConfigureSearchBox();

            var attendanceButton = TeacherTabChrome.SecondaryButton("Điểm danh");
            attendanceButton.Click += (_, _) =>
            {
                using var dialog = new TeacherAttendanceDialog(TeacherId, Controller);
                dialog.ShowDialog(FindForm());
            };
            AddHeaderAction(attendanceButton);
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() =>
        {
            _allStudents = Controller.GetPendingEnrollments(TeacherId)
                .Concat(Controller.GetEnrolledStudents(TeacherId))
                .ToList();

            return BuildStudentTable(ApplyLocalFilter(_allStudents));
        });

        protected override async Task AddAsync()
        {
            if (CurrentString("Trạng thái").ToUpperInvariant() == "PENDING")
            {
                Controller.ApproveEnrollment(TeacherId, CurrentInt("CourseId"), CurrentInt("StudentId"));
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            if (CurrentString("Trạng thái").ToUpperInvariant() == "PENDING"
                && CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Từ chối yêu cầu ghi danh?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Controller.RejectEnrollment(TeacherId, CurrentInt("CourseId"), CurrentInt("StudentId"));
                await LoadDataAsync();
            }
        }

        public async Task ApplyQuickSearchAsync(TeacherQuickSearchRequest request)
        {
            if (!string.Equals(request.Kind, TeacherQuickSearchKinds.Student, StringComparison.OrdinalIgnoreCase))
                return;

            _quickSearchKeyword = request.Keyword ?? string.Empty;
            _searchBox.Text = _quickSearchKeyword;
            await LoadDataAsync();
            SelectStudentRow(request.Id, request.ParentId);
        }

        private void ConfigureSearchBox()
        {
            _searchBox.Name = "txtStudentSearch";
            _searchBox.Width = 240;
            _searchBox.PlaceholderText = "Tìm học viên, email, khóa học...";
            AppColors.ApplyTheme(_searchBox);
            _searchBox.TextChanged += async (_, _) =>
            {
                _quickSearchKeyword = _searchBox.Text.Trim();
                if (IsHandleCreated && !Disposing && !IsDisposed)
                    await LoadDataAsync();
            };
            AddHeaderAction(_searchBox);
        }

        private IEnumerable<TeacherStudentModel> ApplyLocalFilter(IEnumerable<TeacherStudentModel> students)
        {
            string keyword = _quickSearchKeyword.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
                return students;

            return students.Where(s => ContainsKeyword(s.StudentName, keyword)
                || ContainsKeyword(s.Email, keyword)
                || ContainsKeyword(s.CourseName, keyword)
                || ContainsKeyword(s.Status, keyword));
        }

        private static DataTable BuildStudentTable(IEnumerable<TeacherStudentModel> rows) =>
            TeacherTabChrome.ToTable(new[] { "EnrollmentId", "CourseId", "StudentId", "Khóa học", "Sinh viên", "Email", "Trạng thái", "Ngày tham gia" },
                rows,
                s => new object?[] { s.EnrollmentId, s.CourseId, s.StudentId, s.CourseName, s.StudentName, s.Email, s.Status, s.JoinedAt.ToString("dd/MM/yyyy HH:mm") });

        private void SelectStudentRow(int studentId, int? courseId)
        {
            if (studentId <= 0 || !Grid.Columns.Contains("StudentId"))
                return;

            Grid.ClearSelection();
            foreach (DataGridViewRow row in Grid.Rows)
            {
                if (row.IsNewRow || row.Cells["StudentId"].Value == null)
                    continue;

                int currentStudentId = Convert.ToInt32(row.Cells["StudentId"].Value);
                int currentCourseId = Grid.Columns.Contains("CourseId") && row.Cells["CourseId"].Value != null
                    ? Convert.ToInt32(row.Cells["CourseId"].Value)
                    : 0;

                if (currentStudentId != studentId || (courseId.HasValue && courseId.Value > 0 && currentCourseId != courseId.Value))
                    continue;

                row.Selected = true;
                Grid.CurrentCell = GetFirstVisibleCell(row);
                Grid.FirstDisplayedScrollingRowIndex = Math.Max(0, row.Index);
                break;
            }
        }

        private DataGridViewCell? GetFirstVisibleCell(DataGridViewRow row)
        {
            return row.Cells.Cast<DataGridViewCell>().FirstOrDefault(cell => cell.Visible);
        }

        private static bool ContainsKeyword(string? value, string keyword)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
