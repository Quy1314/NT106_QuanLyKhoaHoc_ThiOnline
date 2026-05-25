using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services;

Run("exam scoring sums only correct selected options", () =>
{
    var questions = new[]
    {
        new TeacherExamQuestionModel { Id = 10, CorrectOption = "A", Points = 2.5m },
        new TeacherExamQuestionModel { Id = 20, CorrectOption = "B", Points = 2.5m },
        new TeacherExamQuestionModel { Id = 30, CorrectOption = "D", Points = 5m }
    };
    var selected = new Dictionary<int, string>
    {
        [10] = "a",
        [20] = "C",
        [30] = "D"
    };

    decimal score = ExamScoringService.CalculateScore(questions, selected);

    AssertEqual(7.5m, score);
});

Run("material file policy accepts common documents and rejects unsafe files", () =>
{
    MaterialFileValidation valid = MaterialFilePolicy.Validate("lesson.docx", 1024);
    MaterialFileValidation badExtension = MaterialFilePolicy.Validate("tool.exe", 1024);
    MaterialFileValidation tooLarge = MaterialFilePolicy.Validate("deck.pdf", 21L * 1024 * 1024);

    AssertTrue(valid.IsValid, valid.ErrorMessage);
    AssertFalse(badExtension.IsValid, "exe must be rejected");
    AssertFalse(tooLarge.IsValid, "files above 20MB must be rejected");
});

Run("student exam availability separates visibility from start eligibility", () =>
{
    var future = new StudentExamListItemModel
    {
        Title = "Future active exam",
        OpenTime = DateTime.Now.AddHours(1),
        MaxAttempts = 1,
        AttemptCount = 0,
        QuestionCount = 5
    };

    var noQuestions = new StudentExamListItemModel
    {
        Title = "Active shell",
        MaxAttempts = 1,
        AttemptCount = 0,
        QuestionCount = 0
    };

    var resumable = new StudentExamListItemModel
    {
        Title = "Resume exam",
        MaxAttempts = 1,
        AttemptCount = 1,
        InProgressAttemptCount = 1,
        QuestionCount = 3
    };

    AssertFalse(StudentExamAvailabilityService.CanStart(future), "future active exam must be visible but not startable");
    AssertEqual("Chưa đến giờ", StudentExamAvailabilityService.GetStatusText(future));
    AssertFalse(StudentExamAvailabilityService.CanStart(noQuestions), "active shell without questions must be visible but not startable");
    AssertEqual("Chưa có câu hỏi", StudentExamAvailabilityService.GetStatusText(noQuestions));
    AssertTrue(StudentExamAvailabilityService.CanStart(resumable), "in-progress attempt must be resumable even when max attempts is reached");
    AssertEqual("Đang làm dở", StudentExamAvailabilityService.GetStatusText(resumable));
});

Console.WriteLine("Feature tests passed.");

static void Run(string name, Action test)
{
    try
    {
        test();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"FAIL {name}: {ex.Message}");
        Environment.ExitCode = 1;
    }
}

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, got {actual}");
}

static void AssertTrue(bool value, string message)
{
    if (!value)
        throw new InvalidOperationException(message);
}

static void AssertFalse(bool value, string message)
{
    if (value)
        throw new InvalidOperationException(message);
}
