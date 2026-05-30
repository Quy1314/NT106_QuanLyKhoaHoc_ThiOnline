using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services;
using CourseGuard.Frontend.Forms.Teacher;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherAssignments : TeacherGridPageBase
    {
        private readonly System.Windows.Forms.Button _btnViewSubmissions;

        public UC_TeacherAssignments(int teacherId) : base(teacherId, "Bài tập", "Quản lý bài tập và hạn nộp theo khóa học.", "Danh sách bài tập") 
        { 
            _btnViewSubmissions = TeacherTabChrome.SecondaryButton("Xem bài nộp");
            _btnViewSubmissions.Click += (s, e) => {
                using var dialog = new TeacherSubmissionsDialog(TeacherId, Controller, Controller.GetCourses(TeacherId));
                dialog.ShowDialog(FindForm());
            };
            AddHeaderAction(_btnViewSubmissions);
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "Id", "CourseId", "Khóa học", "Tiêu đề", "Hạn nộp", "Trạng thái", "File đính kèm", "Mô tả" },
            Controller.GetAssignments(TeacherId),
            a => new object?[] { a.Id, a.CourseId, a.CourseName, a.Title, a.DueAt?.ToString("dd/MM/yyyy HH:mm") ?? "", a.Status, string.IsNullOrEmpty(a.FileName) ? "Không có" : a.FileName, a.Description }));
        protected override async Task AddAsync()
        {
            using var dialog = new TeacherAssignmentDialog(Controller.GetCourses(TeacherId));
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                var model = new TeacherAssignmentModel { CourseId = dialog.CourseId, Title = dialog.ItemTitle, Description = dialog.Details, DueAt = dialog.SelectedDate, Status = dialog.Status };
                if (!string.IsNullOrEmpty(dialog.SelectedFilePath) && File.Exists(dialog.SelectedFilePath))
                {
                    var info = new FileInfo(dialog.SelectedFilePath);
                    model.FileName = info.Name;
                    model.FilePath = dialog.SelectedFilePath;
                    model.FileSize = info.Length;
                    model.FileContent = File.ReadAllBytes(dialog.SelectedFilePath);
                    model.ContentType = MaterialFilePolicy.ResolveMimeType(info.Name);
                }
                Controller.CreateAssignment(TeacherId, model);
                await LoadDataAsync();
            }
        }
        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            var existing = Controller.GetAssignments(TeacherId).FirstOrDefault(a => a.Id == id);
            using var dialog = new TeacherAssignmentDialog(Controller.GetCourses(TeacherId), existing);
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                var model = new TeacherAssignmentModel { Id = id, CourseId = dialog.CourseId, Title = dialog.ItemTitle, Description = dialog.Details, DueAt = dialog.SelectedDate, Status = dialog.Status };
                if (!string.IsNullOrEmpty(dialog.SelectedFilePath) && File.Exists(dialog.SelectedFilePath))
                {
                    var info = new FileInfo(dialog.SelectedFilePath);
                    model.FileName = info.Name;
                    model.FilePath = dialog.SelectedFilePath;
                    model.FileSize = info.Length;
                    model.FileContent = File.ReadAllBytes(dialog.SelectedFilePath);
                    model.ContentType = MaterialFilePolicy.ResolveMimeType(info.Name);
                }
                else if (existing != null)
                {
                    // Giữ lại file cũ nếu không chọn file mới và file cũ chưa bị xóa
                    model.FileName = existing.FileName;
                    model.FilePath = existing.FilePath;
                    model.ContentType = existing.ContentType;
                    model.FileSize = existing.FileSize;
                }
                Controller.UpdateAssignment(TeacherId, model);
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
