using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherLessons : TeacherGridPageBase
    {
        private readonly System.Windows.Forms.Button _btnDownloadFile;

        public UC_TeacherLessons(int teacherId, TeacherController controller) : base(teacherId, controller, "Bài học", "Quản lý bài học theo khóa học thuộc quyền.", "Danh sách bài học")
        {
            _btnDownloadFile = TeacherTabChrome.SecondaryButton("Tải giáo trình");
            _btnDownloadFile.Click += async (s, e) => await DownloadFileAsync();
            AddHeaderAction(_btnDownloadFile);
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "Id", "CourseId", "Khóa học", "Tiêu đề", "Trạng thái", "Ngày đăng", "Giáo trình", "Nội dung" },
            Controller.GetLessons(TeacherId),
            l => new object?[] { l.Id, l.CourseId, l.CourseName, l.Title, l.Status, l.PublishAt?.ToString("dd/MM/yyyy HH:mm") ?? "", string.IsNullOrEmpty(l.FileName) ? "Không có" : l.FileName, l.Content }));

        protected override async Task AddAsync()
        {
            using var dialog = new TeacherLessonDialog(Controller.GetCourses(TeacherId));
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                var model = new TeacherLessonModel { CourseId = dialog.CourseId, Title = dialog.ItemTitle, Content = dialog.ItemContent, PublishAt = dialog.PublishAt, Status = dialog.Status };
                if (dialog.FileContent != null)
                {
                    model.FileName = Path.GetFileName(dialog.SelectedFilePath);
                    model.FilePath = dialog.SelectedFilePath;
                    model.FileSize = dialog.FileSize;
                    model.FileContent = dialog.FileContent;
                    model.ContentType = dialog.ContentType;
                }
                Controller.CreateLesson(TeacherId, model);
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            var existing = Controller.GetLessons(TeacherId).FirstOrDefault(l => l.Id == id);
            using var dialog = new TeacherLessonDialog(Controller.GetCourses(TeacherId), existing);
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                var model = new TeacherLessonModel { Id = id, CourseId = dialog.CourseId, Title = dialog.ItemTitle, Content = dialog.ItemContent, PublishAt = dialog.PublishAt, Status = dialog.Status };
                if (dialog.FileContent != null)
                {
                    model.FileName = Path.GetFileName(dialog.SelectedFilePath);
                    model.FilePath = dialog.SelectedFilePath;
                    model.FileSize = dialog.FileSize;
                    model.FileContent = dialog.FileContent;
                    model.ContentType = dialog.ContentType;
                }
                else if (existing != null)
                {
                    model.FileName = string.IsNullOrEmpty(dialog.SelectedFilePath) ? null : existing.FileName;
                    model.FilePath = string.IsNullOrEmpty(dialog.SelectedFilePath) ? null : existing.FilePath;
                    model.ContentType = string.IsNullOrEmpty(dialog.SelectedFilePath) ? null : existing.ContentType;
                    model.FileSize = string.IsNullOrEmpty(dialog.SelectedFilePath) ? null : existing.FileSize;
                    model.HasStoredContent = !string.IsNullOrEmpty(dialog.SelectedFilePath) && existing.HasStoredContent;
                }
                Controller.UpdateLesson(TeacherId, model);
                await LoadDataAsync();
            }
        }

        protected override async Task DeleteAsync()
        {
            int id = CurrentInt("Id");
            if (id > 0) { Controller.DeleteLesson(TeacherId, id); await LoadDataAsync(); }
        }

        private async Task DownloadFileAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            var existing = Controller.GetLessons(TeacherId).FirstOrDefault(l => l.Id == id);
            if (existing == null || !existing.HasStoredContent || string.IsNullOrEmpty(existing.FileName))
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Bài học này không có file đính kèm.", "Thông báo");
                return;
            }

            using var sfd = new System.Windows.Forms.SaveFileDialog
            {
                FileName = existing.FileName,
                Filter = "All files (*.*)|*.*"
            };

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var bytes = await Controller.GetLessonFileContentAsync(TeacherId, id);
                if (bytes != null && bytes.Length > 0)
                {
                    await File.WriteAllBytesAsync(sfd.FileName, bytes);
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Tải file thành công!", "Thông báo");
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        });
                    }
                    catch { /* Ignore if no default app */ }
                }
                else
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Không thể tải nội dung file từ máy chủ.", "Lỗi");
                }
            }
        }
    }
}
