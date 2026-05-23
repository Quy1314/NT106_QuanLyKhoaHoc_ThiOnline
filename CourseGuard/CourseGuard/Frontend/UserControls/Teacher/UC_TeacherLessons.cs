using System.Data;
using System.Threading.Tasks;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherLessons : TeacherGridPageBase
    {
        public UC_TeacherLessons(int teacherId) : base(teacherId, "Bài học", "Quản lý bài học theo khóa học thuộc quyền.", "Danh sách bài học") { }
        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "Id", "CourseId", "Khóa học", "Tiêu đề", "Trạng thái", "Ngày đăng", "Nội dung" },
            Controller.GetLessons(TeacherId),
            l => new object?[] { l.Id, l.CourseId, l.CourseName, l.Title, l.Status, l.PublishAt?.ToString("dd/MM/yyyy HH:mm") ?? "", l.Content }));
        protected override async Task AddAsync()
        {
            using var dialog = new TeacherLessonDialog(Controller.GetCourses(TeacherId));
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.CreateLesson(TeacherId, new TeacherLessonModel { CourseId = dialog.CourseId, Title = dialog.ItemTitle, Content = dialog.Details, PublishAt = dialog.SelectedDate, Status = dialog.Status });
                await LoadDataAsync();
            }
        }
        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            using var dialog = new TeacherSimpleItemDialog("Sửa bài học", Controller.GetCourses(TeacherId), CurrentString("Tiêu đề"), CurrentString("Nội dung"), CurrentString("Trạng thái"));
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.UpdateLesson(TeacherId, new TeacherLessonModel { Id = id, CourseId = dialog.CourseId, Title = dialog.ItemTitle, Content = dialog.Details, PublishAt = dialog.SelectedDate, Status = dialog.Status });
                await LoadDataAsync();
            }
        }
        protected override async Task DeleteAsync()
        {
            int id = CurrentInt("Id");
            if (id > 0) { Controller.DeleteLesson(TeacherId, id); await LoadDataAsync(); }
        }
    }
}
