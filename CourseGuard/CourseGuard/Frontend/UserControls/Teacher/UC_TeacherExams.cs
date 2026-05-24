using System.Data;
using System.Threading.Tasks;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherExams : TeacherGridPageBase
    {
        public UC_TeacherExams(int teacherId) : base(teacherId, "Bài kiểm tra", "Tạo và quản lý kỳ thi. Giám sát nằm ở tab riêng.", "Danh sách bài kiểm tra")
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

            using var dialog = new TeacherExamQuestionsDialog(TeacherId, id, CurrentString("Tên kỳ thi"));
            dialog.ShowDialog(FindForm());
            await LoadDataAsync();
        }

        protected override async Task AddAsync()
        {
            using var dialog = new TeacherExamDialog(Controller.GetCourses(TeacherId));
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.CreateExam(TeacherId, new TeacherExamModel { CourseId = dialog.CourseId, Title = dialog.ItemTitle, OpenTime = dialog.SelectedDate, CloseTime = dialog.SelectedDate.AddHours(1), DurationMinutes = 60, MaxAttempts = 1, Status = WorkflowConstants.ExamStatus.Draft });
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            int currentCourseId = CurrentInt("CourseId");
            using var dialog = new TeacherSimpleItemDialog("Sửa bài kiểm tra", Controller.GetCourses(TeacherId), CurrentString("Tên kỳ thi"), string.Empty, CurrentString("Trạng thái"));
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                if (dialog.Status == WorkflowConstants.ExamStatus.Active && !Controller.CanActivateExam(TeacherId, id))
                {
                    MetaTheme.ShowModernDialog("Bài kiểm tra cần có ít nhất 1 câu hỏi trước khi kích hoạt.", "Chưa thể kích hoạt", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return;
                }

                Controller.UpdateExam(TeacherId, new TeacherExamModel { Id = id, CourseId = currentCourseId, Title = dialog.ItemTitle, OpenTime = dialog.SelectedDate, CloseTime = dialog.SelectedDate.AddHours(1), DurationMinutes = 60, MaxAttempts = 1, Status = dialog.Status });
                await LoadDataAsync();
            }
        }

        protected override async Task DeleteAsync()
        {
            int id = CurrentInt("Id");
            if (id > 0) { Controller.DeleteExam(TeacherId, id); await LoadDataAsync(); }
        }
    }
}
