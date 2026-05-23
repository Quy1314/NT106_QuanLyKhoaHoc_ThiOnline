using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherResults : TeacherGridPageBase
    {
        public UC_TeacherResults(int teacherId) : base(teacherId, "Kết quả", "Xem và cập nhật điểm cho bài thi thuộc khóa học của mình.", "Bảng điểm") 
        {
            AddButton.Visible = false;
            EditButton.Text = "Cập nhật điểm";
            TeacherTabChrome.FitButtonToText(EditButton);
            DeleteButton.Visible = false;
        }

        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "AttemptId", "ExamId", "CourseId", "StudentId", "Khóa học", "Kỳ thi", "Sinh viên", "Điểm", "Trạng thái", "Nộp lúc" },
            Controller.GetResults(TeacherId),
            r => new object?[] { r.AttemptId, r.ExamId, r.CourseId, r.StudentId, r.CourseName, r.ExamTitle, r.StudentName, r.Score, r.Status, r.SubmitTime?.ToString("dd/MM/yyyy HH:mm") ?? "" }));

        protected override async Task EditAsync()
        {
            int attemptId = CurrentInt("AttemptId");
            if (attemptId <= 0) return;
            string value = Interaction.InputBox("Nhập điểm mới:", "Cập nhật điểm", CurrentString("Điểm"));
            if (double.TryParse(value, out double score))
            {
                Controller.UpdateScore(TeacherId, new CourseGuard.Backend.Models.TeacherScoreModel { AttemptId = attemptId, Score = score });
                await LoadDataAsync();
            }
        }
    }
}
