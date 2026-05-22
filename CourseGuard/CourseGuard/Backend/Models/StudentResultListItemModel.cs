namespace CourseGuard.Backend.Models
{
    public class StudentResultListItemModel
    {
        public string ExamTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CorrectAnswersText { get; set; } = "N/A";
        public double Score { get; set; }
        public string StatusText { get; set; } = string.Empty;
    }
}
