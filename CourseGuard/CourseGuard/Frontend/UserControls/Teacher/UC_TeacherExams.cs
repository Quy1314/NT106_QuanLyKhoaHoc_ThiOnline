using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherExams : TeacherGridPageBase
    {
        private const string ColumnExamId = "Id";
        private const string ColumnCourseId = "CourseId";
        private const string ColumnCourse = "Khoa hoc";
        private const string ColumnTitle = "Ten ky thi";
        private const string ColumnOpen = "Mo";
        private const string ColumnClose = "Dong";
        private const string ColumnDuration = "Thoi luong";
        private const string ColumnAttempts = "Luot";
        private const string ColumnViolations = "Vi pham";
        private const string ColumnQuestions = "Cau hoi";
        private const string ColumnStatus = "Trang thai";

        public UC_TeacherExams(int teacherId, TeacherController controller) : base(teacherId, controller, "Bai kiem tra", "Tao va quan ly ky thi. Giam sat nam o tab rieng.", "Danh sach bai kiem tra")
        {
            var questionsButton = TeacherTabChrome.SecondaryButton("Soan cau hoi");
            questionsButton.Click += async (_, _) => await EditQuestionsAsync();
            AddHeaderAction(questionsButton);
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { ColumnExamId, ColumnCourseId, ColumnCourse, ColumnTitle, ColumnOpen, ColumnClose, ColumnDuration, ColumnAttempts, ColumnViolations, ColumnQuestions, ColumnStatus },
            Controller.GetExams(TeacherId),
            e => new object?[] { e.Id, e.CourseId, e.CourseName, e.Title, e.OpenTime?.ToString("dd/MM/yyyy HH:mm") ?? "", e.CloseTime?.ToString("dd/MM/yyyy HH:mm") ?? "", e.DurationMinutes, e.MaxAttempts, e.MaxViolations, e.QuestionCount, e.StatusText }));

        private async Task EditQuestionsAsync()
        {
            int id = CurrentInt(ColumnExamId);
            if (id <= 0)
            {
                MetaTheme.ShowModernDialog("Vui long chon mot bai kiem tra.", "Thong bao");
                return;
            }

            int courseId = CurrentInt(ColumnCourseId);
            using var dialog = new TeacherExamQuestionsDialog(TeacherId, id, CurrentString(ColumnTitle), courseId);
            dialog.ShowDialog(FindForm());
            await LoadDataAsync();
        }

        protected override async Task AddAsync()
        {
            using var dialog = new TeacherExamDialog(Controller.GetCourses(TeacherId));
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                if (string.Equals(dialog.Status, WorkflowConstants.ExamStatus.Active, StringComparison.OrdinalIgnoreCase))
                {
                    MetaTheme.ShowModernDialog("Bai kiem tra can duoc tao o trang thai nhap, them cau hoi, roi moi kich hoat.", "Chua the kich hoat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Controller.CreateExam(TeacherId, new TeacherExamModel
                {
                    CourseId = dialog.CourseId,
                    Title = dialog.ItemTitle,
                    OpenTime = dialog.OpenTime,
                    CloseTime = dialog.CloseTime,
                    DurationMinutes = dialog.DurationMinutes,
                    MaxAttempts = dialog.MaxAttempts,
                    MaxViolations = dialog.MaxViolations,
                    Status = dialog.Status
                });
                await LoadDataAsync();
            }
        }

        protected override async Task EditAsync()
        {
            int id = CurrentInt(ColumnExamId);
            if (id <= 0)
                return;

            using var dialog = new TeacherExamDialog(Controller.GetCourses(TeacherId), new TeacherExamModel
            {
                Id = id,
                CourseId = CurrentInt(ColumnCourseId),
                Title = CurrentString(ColumnTitle),
                OpenTime = ParseGridDate(CurrentString(ColumnOpen)),
                CloseTime = ParseGridDate(CurrentString(ColumnClose)),
                DurationMinutes = CurrentInt(ColumnDuration),
                MaxAttempts = CurrentInt(ColumnAttempts),
                MaxViolations = CurrentInt(ColumnViolations),
                Status = CurrentString(ColumnStatus)
            });
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                if (string.Equals(dialog.Status, WorkflowConstants.ExamStatus.Active, StringComparison.OrdinalIgnoreCase) && !Controller.CanActivateExam(TeacherId, id))
                {
                    MetaTheme.ShowModernDialog("Bai kiem tra can co it nhat 1 cau hoi truoc khi kich hoat.", "Chua the kich hoat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Controller.UpdateExam(TeacherId, new TeacherExamModel
                {
                    Id = id,
                    CourseId = dialog.CourseId,
                    Title = dialog.ItemTitle,
                    OpenTime = dialog.OpenTime,
                    CloseTime = dialog.CloseTime,
                    DurationMinutes = dialog.DurationMinutes,
                    MaxAttempts = dialog.MaxAttempts,
                    MaxViolations = dialog.MaxViolations,
                    Status = dialog.Status
                });
                await LoadDataAsync();
            }
        }

        protected override async Task DeleteAsync()
        {
            int id = CurrentInt(ColumnExamId);
            if (id > 0)
            {
                Controller.DeleteExam(TeacherId, id);
                await LoadDataAsync();
            }
        }

        private static DateTime? ParseGridDate(string value)
        {
            return DateTime.TryParseExact(value, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed)
                ? parsed
                : null;
        }
    }
}
