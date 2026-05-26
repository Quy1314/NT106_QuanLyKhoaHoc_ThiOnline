using System.Data;
using System.Threading.Tasks;
using CourseGuard.Frontend.Forms.Teacher;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_ExamMonitor : TeacherGridPageBase
    {
        public UC_ExamMonitor(int teacherId) : base(teacherId, "Giám sát thi", "Chỉ hiển thị kỳ thi đang diễn ra thuộc khóa học của giảng viên.", "Phiên thi đang hoạt động")
        {
            AddButton.Text = "Giám sát";
            TeacherTabChrome.FitButtonToText(AddButton);
            EditButton.Visible = false;
            DeleteButton.Visible = false;
        }

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
    }
}
