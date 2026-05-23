using System.Data;
using System.Threading.Tasks;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherExams : TeacherGridPageBase
    {
        public UC_TeacherExams(int teacherId) : base(teacherId, "Bài kiểm tra", "Tạo và quản lý kỳ thi. Giám sát nằm ở tab riêng.", "Danh sách bài kiểm tra") { }
        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "Id", "CourseId", "Khóa học", "Tên kỳ thi", "Mở", "Đóng", "Thời lượng", "Lượt", "Câu hỏi", "Trạng thái" },
            Controller.GetExams(TeacherId),
            e => new object?[] { e.Id, e.CourseId, e.CourseName, e.Title, e.OpenTime?.ToString("dd/MM/yyyy HH:mm") ?? "", e.CloseTime?.ToString("dd/MM/yyyy HH:mm") ?? "", e.DurationMinutes, e.MaxAttempts, e.QuestionCount, e.StatusText }));
        protected override async Task AddAsync()
        {
            using var dialog = new TeacherExamDialog(Controller.GetCourses(TeacherId));
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.CreateExam(TeacherId, new TeacherExamModel { CourseId = dialog.CourseId, Title = dialog.ItemTitle, OpenTime = dialog.SelectedDate, CloseTime = dialog.SelectedDate.AddHours(1), DurationMinutes = 60, MaxAttempts = 1 });
                await LoadDataAsync();
            }
        }
        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            using var dialog = new TeacherSimpleItemDialog("Sửa bài kiểm tra", Controller.GetCourses(TeacherId), CurrentString("Tên kỳ thi"), string.Empty, "ACTIVE");
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.UpdateExam(TeacherId, new TeacherExamModel { Id = id, CourseId = dialog.CourseId, Title = dialog.ItemTitle, OpenTime = dialog.SelectedDate, CloseTime = dialog.SelectedDate.AddHours(1), DurationMinutes = 60, MaxAttempts = 1 });
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
