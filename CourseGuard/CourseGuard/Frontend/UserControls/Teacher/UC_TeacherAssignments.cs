using System.Data;
using System.Threading.Tasks;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherAssignments : TeacherGridPageBase
    {
        public UC_TeacherAssignments(int teacherId) : base(teacherId, "Bài tập", "Quản lý bài tập và hạn nộp theo khóa học.", "Danh sách bài tập") { }
        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "Id", "CourseId", "Khóa học", "Tiêu đề", "Hạn nộp", "Trạng thái", "Mô tả" },
            Controller.GetAssignments(TeacherId),
            a => new object?[] { a.Id, a.CourseId, a.CourseName, a.Title, a.DueAt?.ToString("dd/MM/yyyy HH:mm") ?? "", a.Status, a.Description }));
        protected override async Task AddAsync()
        {
            using var dialog = new TeacherAssignmentDialog(Controller.GetCourses(TeacherId));
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.CreateAssignment(TeacherId, new TeacherAssignmentModel { CourseId = dialog.CourseId, Title = dialog.ItemTitle, Description = dialog.Details, DueAt = dialog.SelectedDate, Status = dialog.Status });
                await LoadDataAsync();
            }
        }
        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            using var dialog = new TeacherSimpleItemDialog("Sửa bài tập", Controller.GetCourses(TeacherId), CurrentString("Tiêu đề"), CurrentString("Mô tả"), CurrentString("Trạng thái"));
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.UpdateAssignment(TeacherId, new TeacherAssignmentModel { Id = id, CourseId = dialog.CourseId, Title = dialog.ItemTitle, Description = dialog.Details, DueAt = dialog.SelectedDate, Status = dialog.Status });
                await LoadDataAsync();
            }
        }
        protected override async Task DeleteAsync()
        {
            int id = CurrentInt("Id");
            if (id > 0) { Controller.DeleteAssignment(TeacherId, id); await LoadDataAsync(); }
        }
    }
}
