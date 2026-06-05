using System.Data;
using System.Threading.Tasks;
using CourseGuard.Backend.Controllers;
using CourseGuard.Frontend.Forms.Teacher;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_ExamMonitor : TeacherGridPageBase
    {
        public UC_ExamMonitor(int teacherId, TeacherController controller) : base(teacherId, controller, "Giám sát thi", "Chỉ hiển thị kỳ thi đang diễn ra thuộc khóa học của giảng viên.", "Phiên thi đang hoạt động")
        {
            AddButton.Text = "Giám sát cá nhân";
            TeacherTabChrome.FitButtonToText(AddButton);
            EditButton.Visible = true;
            EditButton.Text = "Giám sát tất cả";
            TeacherTabChrome.FitButtonToText(EditButton);
            DeleteButton.Visible = false;
        }

        protected override bool RequiresSelectionForEdit => false;

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "AttemptId", "ExamId", "CourseId", "StudentId", "Khóa học", "Kỳ thi", "Sinh viên", "Bắt đầu", "Trạng thái" },
            Controller.GetActiveExamSessions(TeacherId),
            s => new object?[] { s.AttemptId, s.ExamId, s.CourseId, s.StudentId, s.CourseName, s.ExamTitle, s.StudentName, s.StartTime.ToString("dd/MM/yyyy HH:mm"), s.Status }));

        protected override Task AddAsync()
        {
            int examId = CurrentInt("ExamId");
            int studentId = CurrentInt("StudentId");
            int attemptId = CurrentInt("AttemptId");
            if (examId > 0 && studentId > 0)
            {
                var popup = new MonitorPopupForm(TeacherId, examId, studentId, attemptId);
                popup.Show(FindForm());
            }
            else
            {
                Theme.MetaTheme.ShowModernDialog("Vui lòng chọn một sinh viên đang trong phiên thi để giám sát.", "Thông báo");
            }
            return Task.CompletedTask;
        }

        protected override Task EditAsync()
        {
            var form = new MonitorAllForm();
            form.Show(FindForm());
            return Task.CompletedTask;
        }
    }
}
