namespace CourseGuard.Backend.Services
{
    public readonly record struct StudentExamScoringQuestion(int QuestionId, string CorrectOption, decimal Points);

    public sealed record StudentExamScoreResult(int CorrectCount, int TotalQuestions, double Score);

    public static class StudentExamScoringService
    {
        public static StudentExamScoreResult Calculate(
            IEnumerable<StudentExamScoringQuestion> questions,
            IReadOnlyDictionary<int, string> answers)
        {
            int correctCount = 0;
            decimal score = 0;
            int totalQuestions = 0;

            foreach (StudentExamScoringQuestion question in questions)
            {
                totalQuestions++;
                if (!answers.TryGetValue(question.QuestionId, out string? answer))
                    continue;

                if (string.Equals(
                        NormalizeOption(answer),
                        NormalizeOption(question.CorrectOption),
                        StringComparison.OrdinalIgnoreCase))
                {
                    correctCount++;
                    score += question.Points;
                }
            }

            return new StudentExamScoreResult(correctCount, totalQuestions, Math.Round((double)score, 2));
        }

        public static bool IsCorrect(StudentExamScoringQuestion question, string? answer)
        {
            return !string.IsNullOrWhiteSpace(answer)
                && string.Equals(
                    NormalizeOption(answer),
                    NormalizeOption(question.CorrectOption),
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeOption(string? option)
        {
            return (option ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
