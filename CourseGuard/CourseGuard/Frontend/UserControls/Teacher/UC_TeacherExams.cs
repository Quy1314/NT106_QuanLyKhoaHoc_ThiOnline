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
            var questionsButton = TeacherTabChrome.PrimaryButton("Soạn câu hỏi");
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
                Controller.CreateExam(TeacherId, new TeacherExamModel { 
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
            if (id <= 0) return;
            
            var currentExam = new TeacherExamModel
            {
                Id = id,
                CourseId = CurrentInt("CourseId"),
                Title = CurrentString("Tên kỳ thi"),
                Status = CurrentString("Trạng thái")
            };
            
            string openTimeStr = CurrentString("Mở");
            if (!string.IsNullOrEmpty(openTimeStr) && System.DateTime.TryParseExact(openTimeStr, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out var openTime))
                currentExam.OpenTime = openTime;
                
            string closeTimeStr = CurrentString("Đóng");
            if (!string.IsNullOrEmpty(closeTimeStr) && System.DateTime.TryParseExact(closeTimeStr, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out var closeTime))
                currentExam.CloseTime = closeTime;
                
            currentExam.DurationMinutes = CurrentInt("Thời lượng");
            currentExam.MaxAttempts = CurrentInt("Lượt");

            using var dialog = new TeacherExamDialog(Controller.GetCourses(TeacherId), currentExam);
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                if (dialog.Status == WorkflowConstants.ExamStatus.Active && !Controller.CanActivateExam(TeacherId, id))
                {
                    MetaTheme.ShowModernDialog("Bài kiểm tra cần có ít nhất 1 câu hỏi trước khi kích hoạt.", "Chưa thể kích hoạt", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return;
                }

                Controller.UpdateExam(TeacherId, new TeacherExamModel { 
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
            if (id > 0) { Controller.DeleteExam(TeacherId, id); await LoadDataAsync(); }
        }
    }
}
