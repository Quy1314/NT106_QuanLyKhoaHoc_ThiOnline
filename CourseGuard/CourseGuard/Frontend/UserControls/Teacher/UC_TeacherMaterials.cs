using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherMaterials : TeacherGridPageBase
    {
        private readonly ComboBox _courseFilter = new();

        public UC_TeacherMaterials(int teacherId) : base(teacherId, "Tài liệu", "Đăng và quản lý tài liệu cho khóa học thuộc quyền.", "Danh sách tài liệu")
        {
            _courseFilter.Width = 220;
            _courseFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            AppColors.ApplyTheme(_courseFilter);
            _courseFilter.SelectedIndexChanged += async (_, _) => await LoadDataAsync();
            AddHeaderAction(_courseFilter);
            LoadCourseFilter();
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "Id", "CourseId", "Khóa học", "Tên file", "Đường dẫn", "Ngày đăng" },
            Controller.GetMaterials(TeacherId, SelectedCourseId),
            m => new object?[] { m.Id, m.CourseId, m.CourseName, m.FileName, m.FilePath, m.UploadedAt.ToString("dd/MM/yyyy HH:mm") }));

        protected override async Task AddAsync()
        {
            var courses = Controller.GetCourses(TeacherId).Where(c => c.Status != WorkflowConstants.CourseStatus.Closed).ToList();
            using var dialog = new TeacherMaterialDialog(courses);
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                Controller.CreateMaterial(TeacherId, new TeacherMaterialModel { CourseId = dialog.CourseId, FileName = dialog.ItemTitle, FilePath = dialog.Details });
                LoadCourseFilter();
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            using var dialog = new TeacherSimpleItemDialog("Sửa tài liệu", Controller.GetCourses(TeacherId), CurrentString("Tên file"), CurrentString("Đường dẫn"), "ACTIVE");
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                Controller.UpdateMaterial(TeacherId, new TeacherMaterialModel { Id = id, CourseId = dialog.CourseId, FileName = dialog.ItemTitle, FilePath = dialog.Details });
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
