using CourseGuard.Backend.Models;

namespace CourseGuard.Backend.Services
{
    public static class ExamScoringService
    {
        public static decimal CalculateScore(IEnumerable<TeacherExamQuestionModel> questions, IReadOnlyDictionary<int, string> selectedOptions)
        {
            decimal total = 0m;
            foreach (TeacherExamQuestionModel question in questions)
            {
                if (!selectedOptions.TryGetValue(question.Id, out string? selected))
                    continue;

                if (string.Equals(Normalize(selected), Normalize(question.CorrectOption), StringComparison.OrdinalIgnoreCase))
                    total += question.Points;
            }

            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    }
}
