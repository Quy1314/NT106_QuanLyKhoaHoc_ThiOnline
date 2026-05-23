using System.Data;
using System.Threading.Tasks;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherCourses : TeacherGridPageBase
    {
        private readonly System.Windows.Forms.Button _submitButton = TeacherTabChrome.PrimaryButton("Gửi duyệt");

        public UC_TeacherCourses(int teacherId) : base(teacherId, "Khóa học của tôi", "Tạo và quản lý các khóa học thuộc quyền giảng viên.", "Danh sách khóa học")
        {
            _submitButton.Click += async (_, _) => await SubmitForApprovalAsync();
            AddHeaderAction(_submitButton);
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() =>
            TeacherTabChrome.ToTable(new[] { "Id", "Tên khóa học", "Trạng thái", "Học viên", "Mô tả", "Lý do từ chối" },
                Controller.GetCourses(TeacherId),
                c => new object?[] { c.Id, c.Name, c.Status, c.StudentCount, c.Description, c.RejectionReason }));

        protected override async Task AddAsync()
        {
            using var dialog = new TeacherCourseDialog();
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.CreateCourse(TeacherId, dialog.Course);
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            int id = CurrentInt("Id");
            if (id <= 0) return;
            var course = new TeacherCourseModel
            {
                Id = id,
                Name = CurrentString("Tên khóa học"),
                Status = CurrentString("Trạng thái"),
                Description = CurrentString("Mô tả"),
                RejectionReason = CurrentString("Lý do từ chối")
            };
            using var dialog = new TeacherCourseDialog(course);
            if (dialog.ShowDialog(FindForm()) == System.Windows.Forms.DialogResult.OK)
            {
                Controller.UpdateCourse(TeacherId, dialog.Course);
                await LoadDataAsync();
            }
        }

        protected override async Task DeleteAsync()
        {
            int id = CurrentInt("Id");
            if (id > 0 && System.Windows.Forms.MessageBox.Show("Xóa khóa học đã chọn?", "Xác nhận", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                Controller.DeleteCourse(TeacherId, id);
                await LoadDataAsync();
            }
        }

        private async Task SubmitForApprovalAsync()
        {
            int id = CurrentInt("Id");
            string status = CurrentString("Trạng thái");
            if (id <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một khóa học để gửi duyệt.", "Thông báo");
                return;
            }

            if (status != WorkflowConstants.CourseStatus.Draft && status != WorkflowConstants.CourseStatus.Rejected)
            {
                MetaTheme.ShowModernDialog("Chỉ khóa học nháp hoặc bị từ chối mới có thể gửi duyệt.", "Thông báo");
                return;
            }

            bool ok = await Task.Run(() => Controller.SubmitCourseForApproval(TeacherId, id));
            MetaTheme.ShowModernDialog(ok ? "Đã gửi khóa học cho Admin duyệt." : "Không thể gửi duyệt khóa học này.", "Thông báo");
            await LoadDataAsync();
        }
    }
}
