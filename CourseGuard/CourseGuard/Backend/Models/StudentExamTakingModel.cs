namespace CourseGuard.Backend.Models
{
    public class StudentExamTakingModel
    {
        public int ExamId { get; set; }
        public int CourseId { get; set; }
        public int AttemptId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime? OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public List<StudentExamTakingQuestionModel> Questions { get; } = new();
        public Dictionary<int, string> SavedAnswers { get; } = new();
    }

    public class StudentExamTakingQuestionModel
    {
        public int QuestionId { get; set; }
        public int DisplayOrder { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty;
        public decimal Points { get; set; }
    }

    public class StudentExamAttemptStateModel
    {
        public int AttemptId { get; set; }
        public DateTime StartTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public double? Score { get; set; }
        public DateTime? SubmitTime { get; set; }
    }

    public class SubmitStudentExamResultModel
    {
        public int AttemptId { get; set; }
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public double Score { get; set; }
    }
}
