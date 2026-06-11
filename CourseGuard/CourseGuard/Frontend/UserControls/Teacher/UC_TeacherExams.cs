using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherExams : TeacherGridPageBase
    {
        private const string ColumnExamId = "Id";
        private const string ColumnCourseId = "CourseId";
        private const string ColumnCourse = "Khóa học";
        private const string ColumnTitle = "Tên kỳ thi";
        private const string ColumnOpen = "Mở";
        private const string ColumnClose = "Đóng";
        private const string ColumnDuration = "Thời lượng";
        private const string ColumnAttempts = "Lượt";
        private const string ColumnViolations = "Vi phạm";
        private const string ColumnQuestions = "Câu hỏi";
        private const string ColumnStatus = "Trạng thái";

        public UC_TeacherExams(int teacherId, TeacherController controller) : base(teacherId, controller, "Bài kiểm tra", "Tạo và quản lý kỳ thi. Giám sát nằm ở tab riêng.", "Danh sách bài kiểm tra")
        {
            var questionsButton = TeacherTabChrome.SecondaryButton("Soạn câu hỏi");
            questionsButton.Click += async (_, _) => await EditQuestionsAsync();
            AddHeaderAction(questionsButton);
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { ColumnExamId, ColumnCourseId, ColumnCourse, ColumnTitle, ColumnOpen, ColumnClose, ColumnDuration, ColumnAttempts, ColumnViolations, ColumnQuestions, ColumnStatus },
            Controller.GetExams(TeacherId),
            e => new object?[] { e.Id, e.CourseId, e.CourseName, e.Title, e.OpenTime?.ToString("dd/MM/yyyy HH:mm") ?? "", e.CloseTime?.ToString("dd/MM/yyyy HH:mm") ?? "", e.DurationMinutes, e.MaxAttempts, e.MaxViolations, e.QuestionCount, e.StatusText }));

        private async Task EditQuestionsAsync()
        {
            int id = CurrentInt(ColumnExamId);
            if (id <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một bài kiểm tra.", "Thông báo");
                return;
            }

            int courseId = CurrentInt(ColumnCourseId);
            using var dialog = new TeacherExamQuestionsDialog(TeacherId, id, CurrentString(ColumnTitle), courseId);
            dialog.ShowDialog(FindForm());
            await LoadDataAsync();
        }

        protected override async Task AddAsync()
        {
            using var dialog = new TeacherExamDialog(Controller.GetCourses(TeacherId));
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                if (string.Equals(dialog.Status, WorkflowConstants.ExamStatus.Active, StringComparison.OrdinalIgnoreCase))
                {
                    MetaTheme.ShowModernDialog("Bài kiểm tra cần được tạo ở trạng thái nháp, thêm câu hỏi, rồi mới kích hoạt.", "Chưa thể kích hoạt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Controller.CreateExam(TeacherId, new TeacherExamModel
                {
                    CourseId = dialog.CourseId,
                    Title = dialog.ItemTitle,
                    OpenTime = dialog.OpenTime,
                    CloseTime = dialog.CloseTime,
                    DurationMinutes = dialog.DurationMinutes,
                    MaxAttempts = dialog.MaxAttempts,
                    MaxViolations = dialog.MaxViolations,
                    Status = dialog.Status
                });
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            int id = CurrentInt(ColumnExamId);
            if (id <= 0)
                return;

            using var dialog = new TeacherExamDialog(Controller.GetCourses(TeacherId), new TeacherExamModel
            {
                Id = id,
                CourseId = CurrentInt(ColumnCourseId),
                Title = CurrentString(ColumnTitle),
                OpenTime = ParseGridDate(CurrentString(ColumnOpen)),
                CloseTime = ParseGridDate(CurrentString(ColumnClose)),
                DurationMinutes = CurrentInt(ColumnDuration),
                MaxAttempts = CurrentInt(ColumnAttempts),
                MaxViolations = CurrentInt(ColumnViolations),
                Status = CurrentString(ColumnStatus)
            });
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                if (string.Equals(dialog.Status, WorkflowConstants.ExamStatus.Active, StringComparison.OrdinalIgnoreCase) && !Controller.CanActivateExam(TeacherId, id))
                {
                    MetaTheme.ShowModernDialog("Bài kiểm tra cần có ít nhất 1 câu hỏi trước khi kích hoạt.", "Chưa thể kích hoạt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Controller.UpdateExam(TeacherId, new TeacherExamModel
                {
                    Id = id,
                    CourseId = dialog.CourseId,
                    Title = dialog.ItemTitle,
                    OpenTime = dialog.OpenTime,
                    CloseTime = dialog.CloseTime,
                    DurationMinutes = dialog.DurationMinutes,
                    MaxAttempts = dialog.MaxAttempts,
                    MaxViolations = dialog.MaxViolations,
                    Status = dialog.Status
                });
                await LoadDataAsync();
            }
        }

        protected override async Task DeleteAsync()
        {
            int id = CurrentInt(ColumnExamId);
            if (id > 0)
            {
                Controller.DeleteExam(TeacherId, id);
                await LoadDataAsync();
            }
        }

        private static DateTime? ParseGridDate(string value)
        {
            return DateTime.TryParseExact(value, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed)
                ? parsed
                : null;
        }
    }
}
