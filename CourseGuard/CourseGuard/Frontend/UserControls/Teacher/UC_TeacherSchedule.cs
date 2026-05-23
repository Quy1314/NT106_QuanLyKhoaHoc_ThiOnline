using System.Data;
using System.Threading.Tasks;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherSchedule : TeacherGridPageBase
    {
        public UC_TeacherSchedule(int teacherId) : base(teacherId, "Lịch dạy", "Quản lý các buổi học trực tuyến theo khóa học.", "Lịch dạy") { }
        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "Id", "CourseId", "Khóa học", "Tiêu đề", "Bắt đầu", "Kết thúc", "Link" },
            Controller.GetSchedule(TeacherId),
            s => new object?[] { s.Id, s.CourseId, s.CourseName, s.Title, s.StartTime?.ToString("dd/MM/yyyy HH:mm") ?? "", s.EndTime?.ToString("dd/MM/yyyy HH:mm") ?? "", s.MeetingLink }));
        protected override async Task AddAsync()
        {
            using var dialog = new TeacherSimpleItemDialog("Lịch dạy", Controller.GetCourses(TeacherId), status: "ACTIVE");
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.CreateScheduleItem(TeacherId, new TeacherScheduleItemModel { CourseId = dialog.CourseId, Title = dialog.ItemTitle, StartTime = dialog.SelectedDate, EndTime = dialog.SelectedDate.AddHours(2), MeetingLink = dialog.Details });
                await LoadDataAsync();
            }
        }
        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            using var dialog = new TeacherSimpleItemDialog("Sửa lịch dạy", Controller.GetCourses(TeacherId), CurrentString("Tiêu đề"), CurrentString("Link"), "ACTIVE");
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.UpdateScheduleItem(TeacherId, new TeacherScheduleItemModel { Id = id, CourseId = dialog.CourseId, Title = dialog.ItemTitle, StartTime = dialog.SelectedDate, EndTime = dialog.SelectedDate.AddHours(2), MeetingLink = dialog.Details });
                await LoadDataAsync();
            }
        }
        protected override async Task DeleteAsync()
        {
            int id = CurrentInt("Id");
            if (id > 0) { Controller.DeleteScheduleItem(TeacherId, id); await LoadDataAsync(); }
        }
    }
}
