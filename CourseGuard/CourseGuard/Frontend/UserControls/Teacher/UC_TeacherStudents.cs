using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherStudents : TeacherGridPageBase
    {
        public UC_TeacherStudents(int teacherId, TeacherController controller) : base(teacherId, controller, "Sinh viên", "Duyệt ghi danh và xem danh sách học viên thuộc khóa học của mình.", "Yêu cầu và học viên")
        {
            AddButton.Text = "Duyệt";
            EditButton.Text = "Từ chối";
            TeacherTabChrome.StyleDangerButton(EditButton);
            TeacherTabChrome.FitButtonToText(AddButton);
            TeacherTabChrome.FitButtonToText(EditButton);
            DeleteButton.Visible = false;
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() =>
        {
            var rows = Controller.GetPendingEnrollments(TeacherId)
                .Concat(Controller.GetEnrolledStudents(TeacherId))
                .ToList();
            return TeacherTabChrome.ToTable(new[] { "EnrollmentId", "CourseId", "StudentId", "Khóa học", "Sinh viên", "Email", "Trạng thái", "Ngày tham gia" },
                rows,
                s => new object?[] { s.EnrollmentId, s.CourseId, s.StudentId, s.CourseName, s.StudentName, s.Email, s.Status, s.JoinedAt.ToString("dd/MM/yyyy HH:mm") });
        });

        protected override async Task AddAsync()
        {
            if (CurrentString("Trạng thái").ToUpperInvariant() == "PENDING")
            {
                Controller.ApproveEnrollment(TeacherId, CurrentInt("CourseId"), CurrentInt("StudentId"));
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            if (CurrentString("Trạng thái").ToUpperInvariant() == "PENDING"
                && CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Từ chối yêu cầu ghi danh?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Controller.RejectEnrollment(TeacherId, CurrentInt("CourseId"), CurrentInt("StudentId"));
                await LoadDataAsync();
            }
        }
    }
}
