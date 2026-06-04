using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services;
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherMaterials : TeacherGridPageBase
    {
        private readonly ComboBox _courseFilter = new();

        public UC_TeacherMaterials(int teacherId, TeacherController controller) : base(teacherId, controller, "Tài liệu", "Đăng và quản lý tài liệu cho khóa học thuộc quyền.", "Danh sách tài liệu")
        {
            _courseFilter.Width = 220;
            _courseFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            AppColors.ApplyTheme(_courseFilter);
            _courseFilter.SelectedIndexChanged += async (_, _) => await LoadDataAsync();
            AddHeaderAction(_courseFilter);
            LoadCourseFilter();
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "Id", "CourseId", "Khóa học", "Tên file", "Kích thước", "Nguồn", "Ngày đăng", "Đường dẫn" },
            Controller.GetMaterials(TeacherId, SelectedCourseId),
            m => new object?[] { m.Id, m.CourseId, m.CourseName, m.FileName, FormatSize(m.FileSize), m.HasStoredContent ? "Database" : "Đường dẫn", m.UploadedAt.ToString("dd/MM/yyyy HH:mm"), m.FilePath }));

        protected override async Task AddAsync()
        {
            var courses = Controller.GetCourses(TeacherId).Where(c => c.Status != WorkflowConstants.CourseStatus.Closed).ToList();
            using var dialog = new TeacherMaterialUploadDialog(courses, SelectedCourseId ?? 0);
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                Controller.CreateMaterial(TeacherId, BuildMaterialModel(dialog.CourseId, dialog.SelectedFilePath));
                LoadCourseFilter();
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            using var dialog = new TeacherMaterialUploadDialog(Controller.GetCourses(TeacherId), CurrentInt("CourseId"));
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                TeacherMaterialModel model = BuildMaterialModel(dialog.CourseId, dialog.SelectedFilePath);
                model.Id = id;
                Controller.UpdateMaterial(TeacherId, model);
                LoadCourseFilter();
                await LoadDataAsync();
            }
        }

        protected override async Task DeleteAsync()
        {
            int id = CurrentInt("Id");
            if (id > 0)
            {
                Controller.DeleteMaterial(TeacherId, id);
                await LoadDataAsync();
            }
        }

        private int? SelectedCourseId => _courseFilter.SelectedItem is CourseFilterItem item && item.CourseId > 0
            ? item.CourseId
            : null;

        private void LoadCourseFilter()
        {
            object? selected = _courseFilter.SelectedItem;
            int selectedCourseId = selected is CourseFilterItem item ? item.CourseId : 0;
            _courseFilter.Items.Clear();
            _courseFilter.Items.Add(new CourseFilterItem(0, "Toàn bộ khóa học"));
            foreach (TeacherCourseModel course in Controller.GetCourses(TeacherId))
                _courseFilter.Items.Add(new CourseFilterItem(course.Id, course.Name));

            int index = 0;
            for (int i = 0; i < _courseFilter.Items.Count; i++)
            {
                if (_courseFilter.Items[i] is CourseFilterItem candidate && candidate.CourseId == selectedCourseId)
                {
                    index = i;
                    break;
                }
            }
            _courseFilter.SelectedIndex = index;
        }

        private static TeacherMaterialModel BuildMaterialModel(int courseId, string filePath)
        {
            var info = new FileInfo(filePath);
            return new TeacherMaterialModel
            {
                CourseId = courseId,
                FileName = info.Name,
                FilePath = filePath,
                ContentType = MaterialFilePolicy.ResolveMimeType(info.Name),
                FileSize = info.Length,
                FileContent = File.ReadAllBytes(filePath),
                HasStoredContent = true
            };
        }

        private static string FormatSize(long bytes)
        {
            if (bytes <= 0)
                return "";
            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024d:0.#} KB";
            return $"{bytes / 1024d / 1024d:0.#} MB";
        }

        private sealed class CourseFilterItem
        {
            public CourseFilterItem(int courseId, string name)
            {
                CourseId = courseId;
                Name = name;
            }

            public int CourseId { get; }
            public string Name { get; }
            public override string ToString() => Name;
        }
    }
}
