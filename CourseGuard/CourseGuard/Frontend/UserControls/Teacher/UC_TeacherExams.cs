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
        public UC_TeacherExams(int teacherId, TeacherController controller) : base(teacherId, controller, "Bài kiểm tra", "Tạo và quản lý kỳ thi. Giám sát nằm ở tab riêng.", "Danh sách bài kiểm tra")
        {
            var questionsButton = TeacherTabChrome.SecondaryButton("Soạn câu hỏi");
            questionsButton.Click += async (_, _) => await EditQuestionsAsync();
            AddHeaderAction(questionsButton);
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "Id", "CourseId", "Khóa học", "Tên kỳ thi", "Mở", "Đóng", "Thời lượng", "Lượt", "Câu hỏi", "Trạng thái" },
            Controller.GetExams(TeacherId),
            e => new object?[] { e.Id, e.CourseId, e.CourseName, e.Title, e.OpenTime?.ToString("dd/MM/yyyy HH:mm") ?? "", e.CloseTime?.ToString("dd/MM/yyyy HH:mm") ?? "", e.DurationMinutes, e.MaxAttempts, e.QuestionCount, e.StatusText }));

        private async Task EditQuestionsAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một bài kiểm tra.", "Thông báo");
                return;
            }

            int courseId = CurrentInt("CourseId");
            using var dialog = new TeacherExamQuestionsDialog(TeacherId, id, CurrentString("Tên kỳ thi"), courseId);
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
                    Status = dialog.Status
                });
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0)
                return;

            using var dialog = new TeacherExamDialog(Controller.GetCourses(TeacherId), new TeacherExamModel
            {
                Id = id,
                CourseId = CurrentInt("CourseId"),
                Title = CurrentString("Tên kỳ thi"),
                OpenTime = ParseGridDate(CurrentString("Mở")),
                CloseTime = ParseGridDate(CurrentString("Đóng")),
                DurationMinutes = CurrentInt("Thời lượng"),
                MaxAttempts = CurrentInt("Lượt"),
                Status = CurrentString("Trạng thái")
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
                    Status = dialog.Status
                });
                await LoadDataAsync();
            }
        }

        protected override async Task DeleteAsync()
        {
            int id = CurrentInt("Id");
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
