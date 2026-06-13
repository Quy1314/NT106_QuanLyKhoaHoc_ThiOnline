using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Services;
using CourseGuard.Backend.Services.Monitoring;
using CourseGuard.Backend.Services.Realtime;
using CourseGuard.Frontend.Forms.Student;
using CourseGuard.Frontend.Forms.Teacher;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls;
using CourseGuard.Frontend.UserControls.Student;
using CourseGuard.Frontend.UserControls.Teacher;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using EmitOpCode = System.Reflection.Emit.OpCode;
using EmitOpCodes = System.Reflection.Emit.OpCodes;
using EmitOperandType = System.Reflection.Emit.OperandType;
using static TestRunner;

Run("student exam scoring sums matching answers", () =>
{
    var questions = new[]
    {
        new StudentExamScoringQuestion(1, "A", 2.5m),
        new StudentExamScoringQuestion(2, "C", 2.5m),
        new StudentExamScoringQuestion(3, "D", 2.5m),
        new StudentExamScoringQuestion(4, "B", 2.5m)
    };
    var answers = new Dictionary<int, string>
    {
        [1] = "a",
        [2] = "B",
        [3] = "D"
    };

    StudentExamScoreResult result = StudentExamScoringService.Calculate(questions, answers);

    AssertEqual(2, result.CorrectCount);
    AssertEqual(4, result.TotalQuestions);
    AssertEqual(5.0, result.Score);
});

Run("teacher exam scoring sums only correct selected options", () =>
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

Run("teacher grid pages accept shared teacher controller", () =>
{
    Type[] pageTypes =
    {
        typeof(UC_TeacherCourses),
        typeof(UC_TeacherLessons),
        typeof(UC_TeacherAssignments),
        typeof(UC_TeacherExams),
        typeof(UC_ExamMonitor),
        typeof(UC_TeacherResults),
        typeof(UC_TeacherStudents),
        typeof(UC_TeacherMaterials),
        typeof(UC_TeacherSchedule)
    };
    Type[] parameterTypes = { typeof(int), typeof(TeacherController) };

    foreach (Type pageType in pageTypes)
    {
        AssertTrue(
            pageType.GetConstructor(parameterTypes) != null,
            $"{pageType.Name} must expose public constructor (int, TeacherController)");
    }
});

Run("student exam launch preloads session before opening form", () =>
{
    Type[] preloadedConstructorTypes = { typeof(int), typeof(StudentExamTakingModel) };
    AssertTrue(
        typeof(DoExamForm).GetConstructor(preloadedConstructorTypes) != null,
        "DoExamForm must expose public constructor (int, StudentExamTakingModel)");

    Type[] scopedSessionStatusTypes = { typeof(int), typeof(int), typeof(bool), typeof(string) };
    AssertTrue(
        typeof(TeacherController).GetMethod("UpdateSessionStatusAsync", scopedSessionStatusTypes) != null,
        "TeacherController session status updates must be scoped by teacherId");
    Type[] unscopedSessionStatusTypes = { typeof(int), typeof(bool), typeof(string) };
    AssertTrue(
        typeof(TeacherController).GetMethod("UpdateSessionStatusAsync", unscopedSessionStatusTypes) == null,
        "TeacherController must not expose unscoped session status updates");

    MethodInfo? preloadMethod = typeof(UC_TakeExam).GetMethod(
        "PreloadExamSessionAsync",
        BindingFlags.Instance | BindingFlags.NonPublic);
    AssertTrue(preloadMethod != null, "UC_TakeExam must preload the exam session before opening DoExamForm");
    AssertEqual(typeof(Task<StudentExamTakingModel?>), preloadMethod!.ReturnType);
});

Run("student exam launch context snapshots selected row before preload", () =>
{
    MethodInfo? createContext = typeof(UC_TakeExam).GetMethod(
        "CreateExamLaunchContext",
        BindingFlags.Static | BindingFlags.NonPublic);
    AssertTrue(createContext != null, "UC_TakeExam must snapshot selected exam context before await");

    using DataGridView grid = new();
    grid.Columns.Add("ExamId", "ExamId");
    grid.Columns.Add("CanStart", "CanStart");
    grid.Columns.Add("Kỳ thi", "Kỳ thi");
    grid.Rows.Add(7, true, "Giữa kỳ");

    object context = createContext!.Invoke(null, new object?[] { grid.Rows[0] })
        ?? throw new InvalidOperationException("launch context must not be null for a valid row");
    grid.Rows[0].Cells["Kỳ thi"].Value = "Đã đổi";

    AssertEqual(7, GetPropertyValue<int>(context, "ExamId"));
    AssertTrue(GetPropertyValue<bool>(context, "CanStart"), "launch context should snapshot CanStart");
    AssertEqual("Giữa kỳ", GetPropertyValue<string>(context, "ExamName"));
});

Run("screen monitor connection loss tracker reports after threshold once", () =>
{
    Type trackerType = typeof(StudentScreenStreamClient).Assembly.GetType("CourseGuard.Backend.Services.Monitoring.ScreenMonitorConnectionLossTracker")
        ?? throw new InvalidOperationException("ScreenMonitorConnectionLossTracker must exist");
    FieldInfo violationType = trackerType.GetField("ViolationType", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("ScreenMonitorConnectionLossTracker must expose ViolationType");
    AssertEqual("CONNECTION_LOST", violationType.GetValue(null)?.ToString());

    DateTimeOffset now = new(2026, 6, 5, 0, 0, 0, TimeSpan.Zero);
    Func<DateTimeOffset> clock = () => now;
    object tracker = Activator.CreateInstance(trackerType, TimeSpan.FromSeconds(30), clock)
        ?? throw new InvalidOperationException("Cannot create connection loss tracker");
    MethodInfo observeDisconnected = trackerType.GetMethod("ObserveDisconnected")
        ?? throw new InvalidOperationException("Tracker must expose ObserveDisconnected");
    MethodInfo observeConnected = trackerType.GetMethod("ObserveConnected")
        ?? throw new InvalidOperationException("Tracker must expose ObserveConnected");

    observeConnected.Invoke(tracker, Array.Empty<object>());
    AssertFalse((bool)observeDisconnected.Invoke(tracker, Array.Empty<object>())!, "initial disconnect should start the timer only");
    now = now.AddSeconds(29);
    AssertFalse((bool)observeDisconnected.Invoke(tracker, Array.Empty<object>())!, "disconnect below threshold must not report");
    now = now.AddSeconds(1);
    AssertTrue((bool)observeDisconnected.Invoke(tracker, Array.Empty<object>())!, "disconnect at threshold must report");
    now = now.AddSeconds(10);
    AssertFalse((bool)observeDisconnected.Invoke(tracker, Array.Empty<object>())!, "same outage must report once");

    observeConnected.Invoke(tracker, Array.Empty<object>());
    AssertFalse((bool)observeDisconnected.Invoke(tracker, Array.Empty<object>())!, "reconnect should reset the outage window");
});

Run("screen monitor connection loss tracker ignores initial unavailable monitor", () =>
{
    Type trackerType = typeof(StudentScreenStreamClient).Assembly.GetType("CourseGuard.Backend.Services.Monitoring.ScreenMonitorConnectionLossTracker")
        ?? throw new InvalidOperationException("ScreenMonitorConnectionLossTracker must exist");
    DateTimeOffset now = new(2026, 6, 5, 0, 0, 0, TimeSpan.Zero);
    object tracker = Activator.CreateInstance(trackerType, TimeSpan.FromSeconds(30), () => now)
        ?? throw new InvalidOperationException("Cannot create connection loss tracker");
    MethodInfo observeDisconnected = trackerType.GetMethod("ObserveDisconnected")
        ?? throw new InvalidOperationException("Tracker must expose ObserveDisconnected");
    MethodInfo observeConnected = trackerType.GetMethod("ObserveConnected")
        ?? throw new InvalidOperationException("Tracker must expose ObserveConnected");

    AssertFalse((bool)observeDisconnected.Invoke(tracker, Array.Empty<object>())!, "initial unavailable monitor should not start a violation window");
    now = now.AddSeconds(60);
    AssertFalse((bool)observeDisconnected.Invoke(tracker, Array.Empty<object>())!, "initial unavailable monitor should not be reported as connection lost");

    observeConnected.Invoke(tracker, Array.Empty<object>());
    AssertFalse((bool)observeDisconnected.Invoke(tracker, Array.Empty<object>())!, "disconnect after a connection should start the violation window");
    now = now.AddSeconds(30);
    AssertTrue((bool)observeDisconnected.Invoke(tracker, Array.Empty<object>())!, "disconnect after established connection should report at threshold");
});

Run("student screen stream client exposes connection loss notification", () =>
{
    Type eventArgsType = typeof(StudentScreenStreamClient).Assembly.GetType("CourseGuard.Backend.Services.Monitoring.ScreenMonitorConnectionLostEventArgs")
        ?? throw new InvalidOperationException("ScreenMonitorConnectionLostEventArgs must exist");
    EventInfo connectionLostEvent = typeof(StudentScreenStreamClient).GetEvent("ConnectionLostThresholdReached")
        ?? throw new InvalidOperationException("StudentScreenStreamClient must expose ConnectionLostThresholdReached");

    AssertEqual(typeof(EventHandler<>).MakeGenericType(eventArgsType), connectionLostEvent.EventHandlerType);
    AssertTrue(eventArgsType.GetProperty("ExamId") != null, "connection lost args must include ExamId");
    AssertTrue(eventArgsType.GetProperty("StudentId") != null, "connection lost args must include StudentId");
    AssertTrue(eventArgsType.GetProperty("AttemptId") != null, "connection lost args must include AttemptId");
    AssertTrue(eventArgsType.GetProperty("DisconnectedFor") != null, "connection lost args must include DisconnectedFor");
});

Run("student exam form handles monitor connection lost violation", () =>
{
    MethodInfo? handler = typeof(DoExamForm).GetMethod("HandleMonitoringConnectionLost", BindingFlags.Instance | BindingFlags.NonPublic);
    AssertTrue(handler != null, "DoExamForm must handle monitor connection lost events");

    MethodInfo? recorder = typeof(DoExamForm).GetMethod("RecordConnectionLostViolationAsync", BindingFlags.Instance | BindingFlags.NonPublic);
    AssertTrue(recorder != null, "DoExamForm must record connection lost violations");
    AssertEqual(typeof(Task), recorder!.ReturnType);
});

Run("plan 03 models expose metadata and violation threshold fields", () =>
{
    AssertProperty(typeof(TeacherExamQuestionModel), "Difficulty");
    AssertProperty(typeof(TeacherExamQuestionModel), "Chapter");
    AssertProperty(typeof(TeacherExamQuestionModel), "QuestionType");
    AssertProperty(typeof(ExcelQuestionRowModel), "Difficulty");
    AssertProperty(typeof(ExcelQuestionRowModel), "Chapter");
    string excelModelSource = File.ReadAllText(Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Backend", "Models", "ExcelQuestionRowModel.cs"));
    AssertTrue(excelModelSource.Contains("Độ khó") && excelModelSource.Contains("Chương"), "Excel metadata columns must use Vietnamese template headers");
    AssertProperty(typeof(TeacherExamModel), "MaxViolations");
    AssertProperty(typeof(StudentExamTakingModel), "MaxViolations");
    AssertProperty(typeof(ViolationModel), "Severity");
    AssertProperty(typeof(ViolationModel), "ActionTaken");

    Type criteriaType = typeof(TeacherExamQuestionModel).Assembly.GetType("CourseGuard.Backend.Models.RandomQuestionCriteria")
        ?? throw new InvalidOperationException("RandomQuestionCriteria must exist");
    AssertProperty(criteriaType, "Difficulty");
    AssertProperty(criteriaType, "Chapter");
    AssertProperty(criteriaType, "Count");
});

Run("plan 03 normalizers expose expected question metadata and severity behavior", () =>
{
    Type metadataNormalizer = typeof(TeacherExamQuestionModel).Assembly.GetType("CourseGuard.Backend.Services.QuestionMetadataNormalizer")
        ?? throw new InvalidOperationException("QuestionMetadataNormalizer must exist");
    MethodInfo normalizeDifficulty = metadataNormalizer.GetMethod("NormalizeDifficulty", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("QuestionMetadataNormalizer must expose NormalizeDifficulty");
    MethodInfo normalizeQuestionType = metadataNormalizer.GetMethod("NormalizeQuestionType", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("QuestionMetadataNormalizer must expose NormalizeQuestionType");
    MethodInfo normalizeChapter = metadataNormalizer.GetMethod("NormalizeChapter", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("QuestionMetadataNormalizer must expose NormalizeChapter");

    AssertEqual("EASY", normalizeDifficulty.Invoke(null, new object?[] { "easy" })?.ToString());
    AssertEqual("MEDIUM", normalizeDifficulty.Invoke(null, new object?[] { "invalid" })?.ToString());
    AssertEqual("MULTIPLE_CHOICE", normalizeQuestionType.Invoke(null, new object?[] { "" })?.ToString());
    AssertEqual(null, normalizeChapter.Invoke(null, new object?[] { "   " }));

    Type severityMap = typeof(StudentScreenStreamClient).Assembly.GetType("CourseGuard.Backend.Services.Monitoring.ViolationSeverityMap")
        ?? throw new InvalidOperationException("ViolationSeverityMap must exist");
    MethodInfo getSeverity = severityMap.GetMethod("Get", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("ViolationSeverityMap must expose Get");

    AssertEqual("HIGH", getSeverity.Invoke(null, new object?[] { "CONNECTION_LOST" })?.ToString());
    AssertEqual("LOW", getSeverity.Invoke(null, new object?[] { "KEY_PRESS" })?.ToString());
    AssertEqual("MEDIUM", getSeverity.Invoke(null, new object?[] { "Mat Focus / Chuyen Tab" })?.ToString());
    AssertEqual("MEDIUM", getSeverity.Invoke(null, new object?[] { "UNKNOWN" })?.ToString());
});

Run("plan 03 source wiring matches question bank and violation threshold architecture", () =>
{
    string repoRoot = RepoRoot();
    string db = File.ReadAllText(Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Backend", "Data", "CourseGuardDbContext.cs"));
    string teacherRepo = File.ReadAllText(Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Backend", "Data", "TeacherRepository.cs"));
    string violationRepo = File.ReadAllText(Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Backend", "Data", "ViolationRepository.cs"));
    string teacherController = File.ReadAllText(Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Backend", "Controllers", "TeacherController.cs"));
    string doExam = File.ReadAllText(Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Frontend", "Forms", "Student", "DoExamForm.cs"));
    string questionDialog = File.ReadAllText(Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Frontend", "Forms", "Teacher", "TeacherExamQuestionsDialog.cs"));
    string randomDialog = File.ReadAllText(Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Frontend", "Forms", "Teacher", "RandomExamBuilderDialog.cs"));

    AssertTrue(db.Contains("EnsureQuestionBankMetadataSchema"), "db context must guard question bank metadata schema");
    AssertTrue(db.Contains("ALTER TABLE questions"), "metadata must be added to the questions bank table");
    AssertTrue(db.Contains("ALTER TABLE exam_questions"), "metadata must also be added to exam question snapshots");
    AssertTrue(db.Contains("max_violations"), "db context must persist exam violation threshold");
    AssertTrue(violationRepo.Contains("COUNT(*)") && violationRepo.Contains("FROM violations"), "auto-submit must use persisted violation count");
    AssertTrue(db.Contains("GetRandomQuestionsByCriteria"), "db context must expose random question selection");
    AssertTrue(db.Contains("questions WHERE course_id = @course_id"), "random question selection must use questions.course_id");
    AssertFalse(db.Contains("exam_questions\n            WHERE course_id"), "random bank selection must not query exam_questions.course_id");
    AssertTrue(db.Contains("COALESCE(MAX(display_order), 0)") && db.Contains("NOT EXISTS") && db.Contains("existing.exam_id = @exam_id") && db.Contains("existing.question_id = q.id"), "bank/random snapshots must append display order and skip duplicate bank questions");
    AssertFalse(db.Contains("row_number() over (order by id),"), "bank/random snapshots must not reset display order to 1 for every insert");
    AssertTrue(teacherRepo.Contains("max_violations"), "teacher repository must read and write max violations");
    AssertTrue(teacherController.Contains("AddRandomQuestionsToExamAsync"), "teacher controller must expose random question add flow");
    AssertTrue(randomDialog.Contains("_previewQuestions.Select(q => q.Id)") && randomDialog.Contains("AddQuestionsFromBankAsync(_teacherId, _examId, _courseId"), "random builder must add the exact previewed question ids");
    AssertTrue(doExam.Contains("HandleViolationAsync"), "student exam form must centralize violation handling");
    AssertTrue(doExam.Contains("EvaluateViolationThresholdAsync"), "student exam form must evaluate violation threshold");
    AssertTrue(doExam.Contains("SubmitExam(confirm: false)"), "threshold breach must submit without confirmation");
    AssertTrue(questionDialog.Contains("RandomExamBuilderDialog"), "question editor must open random exam builder");
});

Run("chat send status formatter labels pending sent and failed sends", () =>
{
    Type formatterType = typeof(UC_Chat).Assembly.GetType("CourseGuard.Frontend.Helpers.ChatSendStatusLineFormatter")
        ?? throw new InvalidOperationException("ChatSendStatusLineFormatter must exist");
    MethodInfo render = formatterType.GetMethod("Render", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("ChatSendStatusLineFormatter must expose Render");

    AssertEqual(
        "[Đang gửi] Bạn: Xin chào",
        render.Invoke(null, new object?[] { "Bạn", "Xin chào", "TEXT", "PENDING", null })?.ToString());
    AssertEqual(
        "[Đã gửi] Bạn: [FILE] bai.pdf",
        render.Invoke(null, new object?[] { "Bạn", "bai.pdf", "FILE", "SENT", null })?.ToString());
    AssertEqual(
        "[Lỗi] Bạn: Xin chào - Mất mạng",
        render.Invoke(null, new object?[] { "Bạn", "Xin chào", "TEXT", "FAILED", "Mất mạng" })?.ToString());
});

Run("student and teacher chat pages can append local send statuses", () =>
{
    AssertTrue(
        typeof(UC_Chat).GetMethod("AppendLocalSendStatus", BindingFlags.Instance | BindingFlags.NonPublic) != null,
        "student chat page must append local send statuses");
    AssertTrue(
        typeof(UC_TeacherMessages).GetMethod("AppendLocalSendStatus", BindingFlags.Instance | BindingFlags.NonPublic) != null,
        "teacher chat page must append local send statuses");
});

Run("student and teacher profile pages share profile page base", () =>
{
    AssertTrue(typeof(ProfilePageBase).IsAssignableFrom(typeof(UC_Profile)), "student profile page must inherit ProfilePageBase");
    AssertTrue(typeof(ProfilePageBase).IsAssignableFrom(typeof(UC_TeacherProfile)), "teacher profile page must inherit ProfilePageBase");

    string[] helperNames =
    {
        "CreateInputGroup",
        "CreateMultilineInputGroup",
        "CreateComboGroup",
        "CreateFieldWrapper",
        "WireInputFocus"
    };

    foreach (string helperName in helperNames)
    {
        AssertTrue(
            typeof(ProfilePageBase).GetMember(helperName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Length > 0,
            $"ProfilePageBase must expose {helperName}");
    }

    TestProfilePage profileBase = new();

    TextBox passwordInput = new() { Tag = "student-input" };
    Panel passwordWrapper = AssertControl<Panel>(
        profileBase.BuildInput("Password", passwordInput, password: true, inputWidth: 320),
        "password input helper should return a wrapper panel");
    RoundedPanel passwordPanel = SingleChild<RoundedPanel>(passwordWrapper);

    AssertEqual('*', passwordInput.PasswordChar);
    AssertFalse(passwordInput.Multiline, "single-line input must remain single-line");
    AssertEqual(BorderStyle.None, passwordInput.BorderStyle);
    AssertEqual(DockStyle.Fill, passwordInput.Dock);
    AssertEqual(Padding.Empty, passwordInput.Margin);
    AssertEqual<object?>(null, passwordInput.Tag);
    AssertEqual(new Size(320, 42), passwordPanel.Size);
    AssertEqual(new Size(240, 42), passwordPanel.MinimumSize);
    AssertEqual(new Padding(12, 9, 12, 9), passwordPanel.Padding);
    AssertEqual(MetaTheme.Colors.BorderSoft, passwordPanel.BorderColor);

    RaiseFocusChanged(passwordInput, focused: true);
    AssertEqual(MetaTheme.Colors.BorderFocus, passwordPanel.BorderColor);
    RaiseFocusChanged(passwordInput, focused: false);
    AssertEqual(MetaTheme.Colors.BorderSoft, passwordPanel.BorderColor);

    TextBox teacherInput = new() { Tag = "teacher-input" };
    profileBase.BuildInput("Teacher", teacherInput, clearInputTag: false);
    AssertEqual("teacher-input", teacherInput.Tag);

    TextBox studentMultiline = new() { Tag = "student-bio", Margin = new Padding(7) };
    Panel studentMultilineWrapper = AssertControl<Panel>(
        profileBase.BuildMultiline("Bio", studentMultiline),
        "student multiline helper should return a wrapper panel");
    RoundedPanel studentMultilinePanel = SingleChild<RoundedPanel>(studentMultilineWrapper);

    AssertTrue(studentMultiline.Multiline, "multiline input should enable multiline mode");
    AssertEqual(ScrollBars.Vertical, studentMultiline.ScrollBars);
    AssertEqual(DockStyle.Fill, studentMultiline.Dock);
    AssertEqual(new Padding(7), studentMultiline.Margin);
    AssertEqual<object?>(null, studentMultiline.Tag);
    AssertEqual(new Size(280, 86), studentMultilinePanel.Size);
    AssertEqual(AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top, studentMultilinePanel.Anchor);

    TextBox teacherMultiline = new() { Tag = "teacher-bio", Margin = new Padding(7) };
    profileBase.BuildMultiline("Teacher bio", teacherMultiline, clearTextBoxMargin: true, clearInputTag: false);
    AssertEqual(Padding.Empty, teacherMultiline.Margin);
    AssertEqual("teacher-bio", teacherMultiline.Tag);

    ComboBox comboBox = new() { Tag = "gender" };
    Panel comboWrapper = AssertControl<Panel>(
        profileBase.BuildCombo("Gender", comboBox),
        "combo helper should return a wrapper panel");

    AssertEqual<object?>(null, comboBox.Tag);
    AssertEqual(new Point(0, 25), comboBox.Location);
    AssertEqual(280, comboBox.Width);
    AssertTrue(comboBox.Height >= 36, "styled combo height should preserve at least the theme minimum");
    AssertEqual(240, comboBox.MinimumSize.Width);
    AssertEqual(AnchorStyles.Left | AnchorStyles.Top, comboBox.Anchor);
    AssertEqual(FlatStyle.Flat, comboBox.FlatStyle);
    AssertEqual(DrawMode.OwnerDrawFixed, comboBox.DrawMode);
    AssertEqual(ComboBoxStyle.DropDownList, comboBox.DropDownStyle);
    AssertFalse(comboBox.IntegralHeight, "styled combo should disable integral height");
    AssertEqual(34, comboBox.ItemHeight);
    AssertEqual(1, comboBox.DropDownHeight);
    AssertTrue(comboWrapper.Controls.Contains(comboBox), "combo wrapper should contain the styled combo");
});

Run("profile inline validation helper returns field level messages", () =>
{
    Type helperType = typeof(UC_Profile).Assembly.GetType("CourseGuard.Frontend.Helpers.ProfileInlineValidationHelper")
        ?? throw new InvalidOperationException("ProfileInlineValidationHelper must exist");
    MethodInfo validateFullName = helperType.GetMethod("ValidateFullName", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("ProfileInlineValidationHelper must expose ValidateFullName");
    MethodInfo validateEmail = helperType.GetMethod("ValidateEmail", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("ProfileInlineValidationHelper must expose ValidateEmail");
    MethodInfo validatePhone = helperType.GetMethod("ValidatePhone", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("ProfileInlineValidationHelper must expose ValidatePhone");
    MethodInfo validateBirthDate = helperType.GetMethod("ValidateBirthDate", BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException("ProfileInlineValidationHelper must expose ValidateBirthDate");

    AssertEqual("Họ và tên không được để trống.", validateFullName.Invoke(null, new object?[] { "   " })?.ToString());
    AssertEqual(string.Empty, validateFullName.Invoke(null, new object?[] { "Nguyễn Văn A" })?.ToString());

    AssertEqual("Email không hợp lệ.", validateEmail.Invoke(null, new object?[] { string.Empty, true })?.ToString());
    AssertEqual("Email không hợp lệ.", validateEmail.Invoke(null, new object?[] { "bad-email", true })?.ToString());
    AssertEqual(string.Empty, validateEmail.Invoke(null, new object?[] { string.Empty, false })?.ToString());
    AssertEqual(string.Empty, validateEmail.Invoke(null, new object?[] { "student@example.com", false })?.ToString());

    AssertEqual("Số điện thoại không hợp lệ.", validatePhone.Invoke(null, new object?[] { "abc123" })?.ToString());
    AssertEqual("Số điện thoại không hợp lệ.", validatePhone.Invoke(null, new object?[] { "٠٩٠١٢٣٤٥٦" })?.ToString());
    AssertEqual(string.Empty, validatePhone.Invoke(null, new object?[] { "0901 234 567" })?.ToString());

    AssertEqual("Ngày sinh cần có định dạng dd/MM/yyyy.", validateBirthDate.Invoke(null, new object?[] { "2026-06-05" })?.ToString());
    AssertEqual(string.Empty, validateBirthDate.Invoke(null, new object?[] { "05/06/2026" })?.ToString());
});

Run("student and teacher profile pages wire inline validation", () =>
{
    AssertTrue(
        typeof(UC_Profile).GetField("_validationErrors", BindingFlags.Instance | BindingFlags.NonPublic)?.FieldType == typeof(ErrorProvider),
        "student profile page must own an ErrorProvider for inline validation");
    AssertTrue(
        typeof(UC_TeacherProfile).GetField("_validationErrors", BindingFlags.Instance | BindingFlags.NonPublic)?.FieldType == typeof(ErrorProvider),
        "teacher profile page must own an ErrorProvider for inline validation");

    string[] inlineMethods =
    {
        "WireInlineValidation",
        "ValidateFullNameInline",
        "ValidateEmailInline",
        "ValidatePhoneInline",
        "ValidateBirthDateInline"
    };

    foreach (string methodName in inlineMethods)
    {
        AssertTrue(
            typeof(UC_Profile).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic) != null,
            $"student profile page must expose {methodName}");
        AssertTrue(
            typeof(UC_TeacherProfile).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic) != null,
            $"teacher profile page must expose {methodName}");
    }
});

Run("profile avatar saves use validated save path", () =>
{
    AssertProfileUsesValidatedSave(typeof(UC_Profile), "student");
    AssertProfileUsesValidatedSave(typeof(UC_TeacherProfile), "teacher");
});

Run("student profile validated save blocks invalid input behavior", () =>
{
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        try
        {
            UserSessionContext.Clear();
            using ContainerControl validationContainer = new();
            using ErrorProvider errors = new()
            {
                BlinkStyle = ErrorBlinkStyle.NeverBlink,
                ContainerControl = validationContainer
            };
            UC_Profile page = (UC_Profile)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(UC_Profile));

            TextBox fullName = new();
            TextBox email = new();
            TextBox phone = new();
            TextBox birthDate = new();
            SetStudentProfileTestFields(page, fullName, email, phone, birthDate, errors);
            MethodInfo save = typeof(UC_Profile).GetMethod("SaveProfileIfValid", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("student profile must expose SaveProfileIfValid");

            fullName.Text = "Nguyen Van A";
            email.Text = "bad-email";
            phone.Text = "0901234567";
            birthDate.Text = "05/06/2026";

            bool result = InvokeProfileSaveWithDialogClose(page, save);
            AssertFalse(result, "invalid email must block student profile save");
            AssertEqual("Email không hợp lệ.", errors.GetError(email));

            email.Text = "student@example.com";
            phone.Text = "abc123";
            result = InvokeProfileSaveWithDialogClose(page, save);
            AssertFalse(result, "invalid phone must block student profile save");
            AssertEqual("Số điện thoại không hợp lệ.", errors.GetError(phone));

            phone.Text = "0901234567";
            birthDate.Text = "2026-06-05";
            result = InvokeProfileSaveWithDialogClose(page, save);
            AssertFalse(result, "invalid birth date must block student profile save");
            AssertEqual("Ngày sinh cần có định dạng dd/MM/yyyy.", errors.GetError(birthDate));

            birthDate.Text = "05/06/2026";
            fullName.Text = " ";
            result = InvokeProfileSaveWithDialogClose(page, save);
            AssertFalse(result, "blank full name must block student profile save");
            AssertEqual("Họ và tên không được để trống.", errors.GetError(fullName));

            fullName.Text = "Nguyen Van A";
            result = (bool)save.Invoke(page, Array.Empty<object>())!;
            AssertTrue(result, "valid student profile fields should allow save when no DB user is active");
        }
        catch (Exception ex)
        {
            failure = ex;
        }
        finally
        {
            UserSessionContext.Clear();
        }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure != null)
        throw failure;
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

Run("avatar manager cover rectangle preserves aspect ratio and fills target", () =>
{
    Rectangle wideCover = AvatarManager.GetCoverRectangle(new Size(200, 100), new Rectangle(10, 20, 80, 80));
    Rectangle tallCover = AvatarManager.GetCoverRectangle(new Size(100, 200), new Rectangle(0, 0, 80, 40));

    AssertEqual(new Rectangle(-30, 20, 160, 80), wideCover);
    AssertEqual(new Rectangle(0, -60, 80, 160), tallCover);
});

Run("avatar manager initials handles blank users and multi word names", () =>
{
    AssertEqual("U", AvatarManager.GetInitials(null, "   "));
    AssertEqual("NN", AvatarManager.GetInitials("Nhat Quoc Nguyen", "ignored"));
    AssertEqual("AB", AvatarManager.GetInitials("   ", "admin beta"));
    AssertEqual("A", AvatarManager.GetInitials("a", null));
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

    var missingAttemptStorage = new StudentExamListItemModel
    {
        Title = "Storage missing",
        MaxAttempts = 1,
        AttemptCount = 0,
        QuestionCount = 3,
        AttemptStorageAvailable = false
    };

    AssertFalse(StudentExamAvailabilityService.CanStart(future), "future active exam must be visible but not startable");
    AssertEqual(StudentExamAvailabilityService.StatusNotOpenYet, StudentExamAvailabilityService.GetStatusText(future));
    AssertFalse(StudentExamAvailabilityService.CanStart(noQuestions), "active shell without questions must be visible but not startable");
    AssertEqual(StudentExamAvailabilityService.StatusNoQuestions, StudentExamAvailabilityService.GetStatusText(noQuestions));
    AssertTrue(StudentExamAvailabilityService.CanStart(resumable), "in-progress attempt must be resumable even when max attempts is reached");
    AssertEqual(StudentExamAvailabilityService.StatusInProgress, StudentExamAvailabilityService.GetStatusText(resumable));
    AssertFalse(StudentExamAvailabilityService.CanStart(missingAttemptStorage), "exam must not be startable without attempt storage");
    AssertEqual(StudentExamAvailabilityService.StatusStorageUnavailable, StudentExamAvailabilityService.GetStatusText(missingAttemptStorage));
});

Run("activity display helper translates known actions and appends cleaned details", () =>
{
    var activity = new RecentUserActivityModel
    {
        Action = "chat_use",
        Details = "  Lop NT106 - nhom 1  "
    };

    AssertEqual(
        "Trao \u0111\u1ed5i trong l\u1edbp h\u1ecdc - Lop NT106 - nhom 1",
        ActivityDisplayHelper.TranslateActivity(activity));
});

Run("activity display helper preserves student default wording", () =>
{
    AssertEqual(
        "\u0110\u0103ng nh\u1eadp h\u1ec7 th\u1ed1ng",
        ActivityDisplayHelper.TranslateActivity(new RecentUserActivityModel { Action = "LOGIN" }));
    AssertEqual(
        "\u0110\u0103ng xu\u1ea5t h\u1ec7 th\u1ed1ng",
        ActivityDisplayHelper.TranslateActivity(new RecentUserActivityModel { Action = "LOGOUT" }));
    AssertEqual(
        "\u0110\u1ed5i m\u1eadt kh\u1ea9u",
        ActivityDisplayHelper.TranslateActivity(new RecentUserActivityModel { Action = "CHANGE_PASSWORD" }));
    AssertEqual(
        "G\u1eedi y\u00eau c\u1ea7u tham gia kh\u00f3a h\u1ecdc",
        ActivityDisplayHelper.TranslateActivity(new RecentUserActivityModel { Action = "COURSE_ENROLL_REQUEST" }));
    AssertEqual(
        "B\u1eaft \u0111\u1ea7u l\u00e0m b\u00e0i ki\u1ec3m tra",
        ActivityDisplayHelper.TranslateActivity(new RecentUserActivityModel { Action = "EXAM_JOIN" }));
    AssertEqual(
        "C\u1eadp nh\u1eadt ho\u1ea1t \u0111\u1ed9ng",
        ActivityDisplayHelper.TranslateActivity(new RecentUserActivityModel { Action = "UNKNOWN_TEACHER_EVENT" }));
});

Run("activity display helper preserves teacher context wording", () =>
{
    AssertEqual(
        "\u0110\u00e3 \u0111\u0103ng nh\u1eadp v\u00e0o h\u1ec7 th\u1ed1ng",
        ActivityDisplayHelper.TranslateActivity(
            new RecentUserActivityModel { Action = "LOGIN" },
            ActivityDisplayContext.Teacher));
    AssertEqual(
        "\u0110\u00e3 \u0111\u0103ng xu\u1ea5t kh\u1ecfi h\u1ec7 th\u1ed1ng",
        ActivityDisplayHelper.TranslateActivity(
            new RecentUserActivityModel { Action = "LOGOUT" },
            ActivityDisplayContext.Teacher));
    AssertEqual(
        "\u0110\u00e3 \u0111\u1ed5i m\u1eadt kh\u1ea9u",
        ActivityDisplayHelper.TranslateActivity(
            new RecentUserActivityModel { Action = "CHANGE_PASSWORD" },
            ActivityDisplayContext.Teacher));
    AssertEqual(
        "\u0110\u00e3 trao \u0111\u1ed5i v\u1edbi h\u1ecdc vi\u00ean",
        ActivityDisplayHelper.TranslateActivity(
            new RecentUserActivityModel { Action = "CHAT_USE" },
            ActivityDisplayContext.Teacher));
    AssertEqual(
        "\u0110\u00e3 c\u1eadp nh\u1eadt ho\u1ea1t \u0111\u1ed9ng gi\u1ea3ng d\u1ea1y",
        ActivityDisplayHelper.TranslateActivity(
            new RecentUserActivityModel { Action = "UNKNOWN_TEACHER_EVENT" },
            ActivityDisplayContext.Teacher));
});

Run("activity display helper cleans blank and long details", () =>
{
    string longDetails = new string('a', 80);

    AssertEqual(string.Empty, ActivityDisplayHelper.CleanDetails("   "));
    AssertEqual(new string('a', 72) + "...", ActivityDisplayHelper.CleanDetails(longDetails));
});

Run("activity display helper safely reads metric counts", () =>
{
    AssertEqual(7, ActivityDisplayHelper.SafeMetricCount(() => 7));
    AssertEqual(0, ActivityDisplayHelper.SafeMetricCount(() => -3));
    AssertEqual(0, ActivityDisplayHelper.SafeMetricCount(() => throw new InvalidOperationException("boom")));
});

Run("activity display helper safely reads nullable averages", () =>
{
    AssertEqual(8.5, ActivityDisplayHelper.SafeAverageScore(() => 8.5));
    AssertEqual<double?>(null, ActivityDisplayHelper.SafeAverageScore(() => null));
    AssertEqual<double?>(null, ActivityDisplayHelper.SafeAverageScore(() => throw new InvalidOperationException("boom")));
});

Run("activity display helper safely reads lists", () =>
{
    List<int> values = ActivityDisplayHelper.SafeList(() => new List<int> { 1, 2 });
    List<int> nullValues = ActivityDisplayHelper.SafeList<int>(() => null!);
    List<int> failedValues = ActivityDisplayHelper.SafeList<int>(() => throw new InvalidOperationException("boom"));

    AssertEqual(2, values.Count);
    AssertEqual(0, nullValues.Count);
    AssertEqual(0, failedValues.Count);
});

Run("classroom frame helper resize preserves aspect ratio within bounds", () =>
{
    using var wide = new Bitmap(400, 200);
    using var tall = new Bitmap(100, 300);

    using Bitmap resizedWide = ClassroomFrameHelper.ResizeFrame(wide, 120, 120);
    using Bitmap resizedTall = ClassroomFrameHelper.ResizeFrame(tall, 120, 120);

    AssertEqual(new Size(120, 60), resizedWide.Size);
    AssertEqual(new Size(40, 120), resizedTall.Size);
    AssertTrue(resizedWide.Width <= 120 && resizedWide.Height <= 120, "wide resize must stay inside max bounds");
    AssertTrue(resizedTall.Width <= 120 && resizedTall.Height <= 120, "tall resize must stay inside max bounds");
});

Run("classroom frame helper jpeg encode decode round trips a small bitmap", () =>
{
    using var source = new Bitmap(8, 6);
    using (Graphics graphics = Graphics.FromImage(source))
    {
        graphics.Clear(Color.CornflowerBlue);
        graphics.FillRectangle(Brushes.IndianRed, 2, 1, 4, 3);
    }

    string encoded = ClassroomFrameHelper.EncodeJpegFrame(source, 80L);
    using Bitmap decoded = ClassroomFrameHelper.DecodeFrame(encoded);

    AssertEqual(source.Size, decoded.Size);
    AssertTrue(decoded.Width > 0 && decoded.Height > 0, "decoded frame must be a usable bitmap");
});

Run("classroom frame helper replace image transfers ownership and disposes old image", () =>
{
    using var target = new PictureBox();
    var oldImage = new Bitmap(2, 2);
    var newImage = new Bitmap(3, 3);
    target.Image = oldImage;

    bool replaced = ClassroomFrameHelper.TryReplaceImage(target, newImage);

    AssertTrue(replaced, "replacement should succeed for live picture box");
    AssertTrue(ReferenceEquals(newImage, target.Image), "picture box should own the replacement image");
    AssertImageDisposed(oldImage, "old image should be disposed after replacement");

    target.Image = null;
    newImage.Dispose();
});

Run("classroom frame helper replace image disposes dropped image", () =>
{
    var droppedImage = new Bitmap(2, 2);
    using var target = new PictureBox();
    target.Dispose();

    bool replaced = ClassroomFrameHelper.TryReplaceImage(target, droppedImage);

    AssertFalse(replaced, "replacement should fail for disposed picture box");
    AssertImageDisposed(droppedImage, "dropped image should be disposed when replacement fails");
});

Run("classroom screen share manager normalizes empty bounds to available screen area", () =>
{
    Rectangle fallback = new Rectangle(10, 20, 300, 200);

    Rectangle normalized = ClassroomScreenShareManager.NormalizeBounds(Rectangle.Empty, fallback);
    Rectangle explicitBounds = ClassroomScreenShareManager.NormalizeBounds(new Rectangle(1, 2, 3, 4), fallback);

    AssertEqual(fallback, normalized);
    AssertEqual(new Rectangle(1, 2, 3, 4), explicitBounds);
});

Run("classroom open signal coordinator starts lazily and broadcasts selected session", async () =>
{
    var signalService = new FakeClassroomSignalService();
    var coordinator = new ClassroomOpenSignalCoordinator(signalService, replayCount: 0);

    AssertEqual(0, signalService.StartCount);

    await coordinator.BroadcastClassOpenedAsync(42);
    await coordinator.BroadcastClassOpenedAsync(43);

    AssertEqual(1, signalService.StartCount);
    AssertEqual("42,43", string.Join(",", signalService.BroadcastSessionIds));
});

Run("classroom open signal coordinator replays open signal for reconnecting clients", async () =>
{
    var signalService = new FakeClassroomSignalService();
    var coordinator = new ClassroomOpenSignalCoordinator(signalService, replayCount: 2, replayDelay: TimeSpan.FromMilliseconds(20));

    await coordinator.BroadcastClassOpenedAsync(42);

    AssertEqual(1, signalService.StartCount);
    AssertEqual("42", string.Join(",", signalService.BroadcastSessionIds));
    AssertTrue(signalService.WaitForBroadcastCount(3, TimeSpan.FromSeconds(2)), "open signal replay was not observed");
    AssertEqual("42,42,42", string.Join(",", signalService.BroadcastSessionIds));
});

Run("classroom open signal coordinator retries start after failure without broadcasting", async () =>
{
    var signalService = new FakeClassroomSignalService { ThrowOnStartListening = true };
    var coordinator = new ClassroomOpenSignalCoordinator(signalService, replayCount: 0);

    bool threw = false;
    try
    {
        await coordinator.BroadcastClassOpenedAsync(42);
    }
    catch (InvalidOperationException)
    {
        threw = true;
    }

    AssertTrue(threw, "first broadcast should fail when listener start fails");
    AssertEqual(1, signalService.StartCount);
    AssertEqual(string.Empty, string.Join(",", signalService.BroadcastSessionIds));

    signalService.ThrowOnStartListening = false;
    await coordinator.BroadcastClassOpenedAsync(43);

    AssertEqual(2, signalService.StartCount);
    AssertEqual("43", string.Join(",", signalService.BroadcastSessionIds));
});

Run("classroom open signal coordinator starts once for concurrent broadcasts", async () =>
{
    using var ready = new CountdownEvent(2);
    using var releaseBroadcasts = new ManualResetEventSlim();
    using var startEntered = new ManualResetEventSlim();
    using var allowStartListening = new ManualResetEventSlim();

    var signalService = new FakeClassroomSignalService
    {
        OnStartListening = () =>
        {
            startEntered.Set();
            AssertTrue(allowStartListening.Wait(TimeSpan.FromSeconds(3)), "start listener delay was not released");
        }
    };
    var coordinator = new ClassroomOpenSignalCoordinator(signalService, replayCount: 0);

    Task first = StartConcurrentBroadcast(coordinator, 42, ready, releaseBroadcasts);
    Task second = StartConcurrentBroadcast(coordinator, 43, ready, releaseBroadcasts);

    AssertTrue(ready.Wait(TimeSpan.FromSeconds(3)), "broadcast tasks did not become ready");
    releaseBroadcasts.Set();
    AssertTrue(startEntered.Wait(TimeSpan.FromSeconds(3)), "listener start was not attempted");
    allowStartListening.Set();

    await Task.WhenAll(first, second);

    int[] broadcastIds = signalService.BroadcastSessionIds.OrderBy(id => id).ToArray();
    AssertEqual(1, signalService.StartCount);
    AssertEqual("42,43", string.Join(",", broadcastIds));
});

Run("student exam form constructor does not invoke before handle exists", () =>
{
    Environment.SetEnvironmentVariable("COURSEGUARD_DB_CONNECTION", "Host=localhost;Username=test;Password=test;Database=test");
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        try
        {
            using var form = new DoExamForm(0);
        }
        catch (Exception ex)
        {
            failure = ex;
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure != null)
        throw new InvalidOperationException(failure.Message, failure);
});

Run("fire and forget safe with null owner does not throw synchronously", () =>
{
    int reporterCallCount = 0;
    Exception? observedException = null;
    using var observed = new ManualResetEventSlim();
    Task failedTask = Task.FromException(new InvalidOperationException("boom"));
    SynchronizationContext? priorContext = SynchronizationContext.Current;

    CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = (_, _, _, _) => reporterCallCount++;
    CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = exception =>
    {
        observedException = exception;
        observed.Set();
    };

    try
    {
        SynchronizationContext.SetSynchronizationContext(null);
        failedTask.FireAndForgetSafe(null);

        AssertTrue(observed.Wait(TimeSpan.FromSeconds(3)), "null owner failed task was not observed");
        AssertTrue(observedException is InvalidOperationException, "null owner should observe original exception");
        AssertEqual(0, reporterCallCount);
    }
    finally
    {
        SynchronizationContext.SetSynchronizationContext(priorContext);
        CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = null;
        CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = null;
    }
});

Run("fire and forget safe with disposed uncreated control does not throw", () =>
{
    using var owner = new Control();
    owner.Dispose();
    int reporterCallCount = 0;
    Exception? observedException = null;
    using var observed = new ManualResetEventSlim();
    Task failedTask = Task.FromException(new InvalidOperationException("boom"));
    SynchronizationContext? priorContext = SynchronizationContext.Current;

    CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = (_, _, _, _) => reporterCallCount++;
    CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = exception =>
    {
        observedException = exception;
        observed.Set();
    };

    try
    {
        SynchronizationContext.SetSynchronizationContext(null);
        failedTask.FireAndForgetSafe(owner);

        AssertTrue(observed.Wait(TimeSpan.FromSeconds(3)), "disposed owner failed task was not observed");
        AssertTrue(observedException is InvalidOperationException, "disposed owner should observe original exception");
        AssertEqual(0, reporterCallCount);
    }
    finally
    {
        SynchronizationContext.SetSynchronizationContext(priorContext);
        CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = null;
        CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = null;
    }
});

Run("fire and forget safe observes canceled task without reporting", () =>
{
    int reporterCallCount = 0;
    Exception? observedException = new InvalidOperationException("not observed");
    using var observed = new ManualResetEventSlim();
    using var cts = new CancellationTokenSource();
    var context = new RecordingSynchronizationContext();
    SynchronizationContext? priorContext = SynchronizationContext.Current;

    cts.Cancel();
    CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = (_, _, _, _) => reporterCallCount++;
    CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = exception =>
    {
        observedException = exception;
        observed.Set();
    };

    try
    {
        SynchronizationContext.SetSynchronizationContext(context);
        Task.FromCanceled(cts.Token).FireAndForgetSafe(null);

        AssertTrue(observed.Wait(TimeSpan.FromSeconds(3)), "canceled task was not observed");
        AssertEqual<Exception?>(null, observedException);
        AssertFalse(context.WaitForPost(TimeSpan.FromMilliseconds(100)), "canceled task should not post a report");
        AssertEqual(0, reporterCallCount);
    }
    finally
    {
        SynchronizationContext.SetSynchronizationContext(priorContext);
        CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = null;
        CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = null;
    }
});

Run("fire and forget safe falls back to context when supplied owner becomes invalid", () =>
{
    using var owner = new Control();
    IntPtr handle = owner.Handle;
    int reporterCallCount = 0;
    Exception? observedException = null;
    using var observed = new ManualResetEventSlim();
    var context = new RecordingSynchronizationContext();
    var failedLater = new TaskCompletionSource();
    SynchronizationContext? priorContext = SynchronizationContext.Current;

    CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = (_, _, _, _) => reporterCallCount++;
    CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = exception =>
    {
        observedException = exception;
        observed.Set();
    };

    try
    {
        SynchronizationContext.SetSynchronizationContext(context);
        failedLater.Task.FireAndForgetSafe(owner);
        owner.Dispose();
        failedLater.SetException(new InvalidOperationException("boom"));

        AssertTrue(observed.Wait(TimeSpan.FromSeconds(3)), "invalid owner failed task was not observed");
        AssertTrue(observedException is InvalidOperationException, "invalid owner should observe original exception");
        AssertTrue(context.WaitForPost(TimeSpan.FromSeconds(3)), "invalid supplied owner should fall back to captured context");
        AssertEqual(0, reporterCallCount);

        context.ExecuteAll();

        AssertEqual(1, reporterCallCount);
    }
    finally
    {
        SynchronizationContext.SetSynchronizationContext(priorContext);
        CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = null;
        CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = null;
    }
});

Run("fire and forget safe posts to captured synchronization context", () =>
{
    Exception? capturedException = null;
    string? capturedTitle = null;
    string? capturedMessage = null;
    var context = new RecordingSynchronizationContext();
    SynchronizationContext? priorContext = SynchronizationContext.Current;
    using var observed = new ManualResetEventSlim();

    CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = (control, title, message, exception) =>
    {
        AssertEqual<Control?>(null, control);
        capturedException = exception;
        capturedTitle = title;
        capturedMessage = message;
        throw new InvalidOperationException("context reporter failure");
    };
    CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = _ => observed.Set();

    try
    {
        SynchronizationContext.SetSynchronizationContext(context);
        Task.FromException(new InvalidOperationException("boom")).FireAndForgetSafe(null);
        AssertTrue(observed.Wait(TimeSpan.FromSeconds(3)), "context failed task was not observed");
        AssertTrue(context.WaitForPost(TimeSpan.FromSeconds(3)), "context callback was not posted");

        context.ExecuteAll();

        AssertEqual("Loi nen", capturedTitle);
        AssertTrue(capturedMessage?.Contains("boom") == true, "context reporter message should include exception message");
        AssertTrue(capturedException is InvalidOperationException, "context reporter should capture exception");
    }
    finally
    {
        SynchronizationContext.SetSynchronizationContext(priorContext);
        CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = null;
        CourseGuard.Frontend.Helpers.TaskExtensions.ObservationCompleted = null;
    }
});

Run("fire and forget safe returns before reporting already failed live owner task", () =>
{
    using var ready = new ManualResetEventSlim();
    using var reported = new ManualResetEventSlim();
    Exception? threadFailure = null;

    var thread = new Thread(() =>
    {
        try
        {
            using var owner = new Control();
            IntPtr handle = owner.Handle;
            CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = (_, _, _, _) => reported.Set();

            ready.Set();
            Task.FromException(new InvalidOperationException("boom")).FireAndForgetSafe(owner, "Background task error");
            if (reported.IsSet)
                throw new InvalidOperationException("reporter ran before FireAndForgetSafe returned");

            DateTime deadline = DateTime.UtcNow.AddSeconds(3);
            while (!reported.IsSet && DateTime.UtcNow < deadline)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
        }
        catch (Exception ex)
        {
            threadFailure = ex;
        }
        finally
        {
            CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = null;
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    AssertTrue(ready.Wait(TimeSpan.FromSeconds(3)), "live control test did not start");
    thread.Join();

    if (threadFailure != null)
        throw new InvalidOperationException(threadFailure.Message, threadFailure);
    AssertTrue(reported.IsSet, "live control reporter was not scheduled");
});

Run("fire and forget safe live owner uses begin invoke and swallows reporter exceptions", () =>
{
    Exception? capturedException = null;
    string? capturedTitle = null;
    string? capturedMessage = null;
    int uiThreadId = 0;
    int reporterThreadId = 0;
    using var ready = new ManualResetEventSlim();
    using var reported = new ManualResetEventSlim();
    var failingTask = new TaskCompletionSource();
    Exception? threadFailure = null;

    var thread = new Thread(() =>
    {
        try
        {
            uiThreadId = Thread.CurrentThread.ManagedThreadId;
            using var owner = new Control();
            IntPtr handle = owner.Handle;
            CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = (control, title, message, exception) =>
            {
                AssertTrue(ReferenceEquals(owner, control), "live owner reporter should receive owner");
                reporterThreadId = Thread.CurrentThread.ManagedThreadId;
                capturedException = exception;
                capturedTitle = title;
                capturedMessage = message;
                reported.Set();
                throw new InvalidOperationException("reporter failure");
            };

            ready.Set();
            ThreadPool.QueueUserWorkItem(_ => failingTask.Task.FireAndForgetSafe(owner, "Background task error"));
            failingTask.SetException(new InvalidOperationException("boom"));

            DateTime deadline = DateTime.UtcNow.AddSeconds(3);
            while (!reported.IsSet && DateTime.UtcNow < deadline)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
        }
        catch (Exception ex)
        {
            threadFailure = ex;
        }
        finally
        {
            CourseGuard.Frontend.Helpers.TaskExtensions.ErrorReporter = null;
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    AssertTrue(ready.Wait(TimeSpan.FromSeconds(3)), "begin invoke live control test did not start");
    thread.Join();

    if (threadFailure != null)
        throw new InvalidOperationException(threadFailure.Message, threadFailure);
    AssertTrue(reported.IsSet, "begin invoke live control reporter was not scheduled");
    AssertEqual(uiThreadId, reporterThreadId);
    AssertEqual("Background task error", capturedTitle);
    AssertTrue(capturedMessage?.Contains("boom") == true, "reported message should include exception message");
    AssertTrue(capturedException is InvalidOperationException, "reported exception should be captured");
});

Run("table grid border token is opaque in both themes", () =>
{
    AppColors.IsDarkMode = true;
    AssertEqual(255, AppColors.GridBorder.A);

    AppColors.IsDarkMode = false;
    AssertEqual(255, AppColors.GridBorder.A);

    AppColors.IsDarkMode = true;
});

Run("vietnam time formatter does not offset database local timestamps", () =>
{
    var databaseLocalTime = new DateTime(2026, 5, 25, 3, 4, 0, DateTimeKind.Unspecified);
    AssertEqual("25/05/2026 03:04", FormatVietnamTime(databaseLocalTime));
});

Run("vietnam time formatter converts explicit utc timestamps", () =>
{
    var utcTime = new DateTime(2026, 5, 25, 3, 4, 0, DateTimeKind.Utc);
    AssertEqual("25/05/2026 10:04", FormatVietnamTime(utcTime));
});

Run("student and teacher data cards wrap grids in rounded bodies", RunDashboardCardTests);
Run("student and teacher page descriptions reserve descender space", RunDashboardHeaderSubtitleLayoutTests);
Run("rounded UI chrome keeps painted borders inside clipping regions", RunRoundedChromeClippingGuardTests);
Run("overview action and indicator models expose required display fields", () =>
{
    Type actionType = typeof(UC_Profile).Assembly.GetType("CourseGuard.Frontend.Helpers.OverviewActionItem")
        ?? throw new InvalidOperationException("OverviewActionItem must exist");
    Type indicatorType = typeof(UC_Profile).Assembly.GetType("CourseGuard.Frontend.Helpers.OverviewIndicatorItem")
        ?? throw new InvalidOperationException("OverviewIndicatorItem must exist");

    foreach (string propertyName in new[] { "Title", "Subtitle", "PageName", "ActionText", "Priority" })
        AssertTrue(actionType.GetProperty(propertyName) != null, $"OverviewActionItem must expose {propertyName}");

    foreach (string propertyName in new[] { "Label", "Value", "Tone" })
        AssertTrue(indicatorType.GetProperty(propertyName) != null, $"OverviewIndicatorItem must expose {propertyName}");
});
Run("student overview next actions follow agreed priority", () =>
{
    Type builderType = typeof(UC_Profile).Assembly.GetType("CourseGuard.Frontend.Helpers.OverviewActionBuilder")
        ?? throw new InvalidOperationException("OverviewActionBuilder must exist");
    MethodInfo build = builderType.GetMethod("BuildStudentActions", new[] { typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })
        ?? throw new InvalidOperationException("BuildStudentActions must exist");

    object[] actions = (object[])build.Invoke(null, new object?[] { true, true, true, true, true })!;
    string joinedTitles = string.Join("|", actions.Select(a => a.GetType().GetProperty("Title")!.GetValue(a)?.ToString()));

    AssertTrue(joinedTitles.StartsWith("Làm bài thi đang mở|Nộp bài tập sắp hết hạn|Vào lớp học hôm nay|Đọc tin nhắn mới|Xem thông báo mới"),
        "student actions must be ordered exam, deadline, class, chat, notification");
});
Run("teacher overview next actions follow agreed priority", () =>
{
    Type builderType = typeof(UC_Profile).Assembly.GetType("CourseGuard.Frontend.Helpers.OverviewActionBuilder")
        ?? throw new InvalidOperationException("OverviewActionBuilder must exist");
    MethodInfo build = builderType.GetMethod("BuildTeacherActions", new[] { typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })
        ?? throw new InvalidOperationException("BuildTeacherActions must exist");

    object[] actions = (object[])build.Invoke(null, new object?[] { true, true, true, true, true })!;
    string joinedTitles = string.Join("|", actions.Select(a => a.GetType().GetProperty("Title")!.GetValue(a)?.ToString()));

    AssertTrue(joinedTitles.StartsWith("Giám sát kỳ thi đang diễn ra|Xử lý việc cần chú ý|Mở buổi dạy sắp tới|Trả lời tin nhắn mới|Xem thông báo mới"),
        "teacher actions must be ordered active exam, required task, class, chat, notification");
});
Run("overview next actions expose Vietnamese navigation labels", () =>
{
    OverviewActionItem[] studentActions = OverviewActionBuilder.BuildStudentActions(true, true, true, true, true);
    OverviewActionItem[] teacherActions = OverviewActionBuilder.BuildTeacherActions(true, true, true, true, true);

    AssertEqual("Làm bài", studentActions[0].ActionText);
    AssertEqual("Bài kiểm tra", studentActions[0].PageName);
    AssertEqual("Lịch học", studentActions[2].PageName);
    AssertEqual("Giám sát", teacherActions[0].ActionText);
    AssertEqual("Giám sát thi", teacherActions[0].PageName);
    AssertEqual("Lịch dạy", teacherActions[2].PageName);
});
Run("overview communication actions route chat and notifications separately", () =>
{
    OverviewActionItem studentChatOnly = OverviewActionBuilder.BuildStudentActions(false, false, false, true, false).Single();
    AssertEqual("Đọc tin nhắn mới", studentChatOnly.Title);
    AssertEqual("Tin nhắn", studentChatOnly.PageName);
    AssertEqual("Mở tin nhắn", studentChatOnly.ActionText);

    OverviewActionItem studentNotificationOnly = OverviewActionBuilder.BuildStudentActions(false, false, false, false, true).Single();
    AssertEqual("Xem thông báo mới", studentNotificationOnly.Title);
    AssertEqual("Thông báo", studentNotificationOnly.PageName);
    AssertEqual("Mở thông báo", studentNotificationOnly.ActionText);

    OverviewActionItem[] studentBoth = OverviewActionBuilder.BuildStudentActions(false, false, false, true, true);
    AssertEqual("Tin nhắn", studentBoth[0].PageName);
    AssertEqual("Thông báo", studentBoth[1].PageName);

    OverviewActionItem teacherChatOnly = OverviewActionBuilder.BuildTeacherActions(false, false, false, true, false).Single();
    AssertEqual("Trả lời tin nhắn mới", teacherChatOnly.Title);
    AssertEqual("Tin nhắn", teacherChatOnly.PageName);

    OverviewActionItem teacherNotificationOnly = OverviewActionBuilder.BuildTeacherActions(false, false, false, false, true).Single();
    AssertEqual("Xem thông báo mới", teacherNotificationOnly.Title);
    AssertEqual("Thông báo", teacherNotificationOnly.PageName);
});
Run("overview action labels reserve enough single line space", () =>
{
    string root = RepoRoot();
    string studentSource = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "UserControls", "Student", "UC_StudentDashboard.cs"));
    string teacherSource = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "UserControls", "Teacher", "UC_TeacherOverview.cs"));

    AssertFalse(studentSource.Contains("Width = 96"),
        "student overview action label should not use the old fixed 96px width");
    AssertFalse(teacherSource.Contains("Width = 96"),
        "teacher overview action label should not use the old fixed 96px width");
    AssertTrue(studentSource.Contains("GetActionLabelWidth(item.ActionText)") && studentSource.Contains("TextRenderer.MeasureText"),
        "student overview action label should measure text and reserve single-line width");
    AssertTrue(teacherSource.Contains("GetActionLabelWidth(item.ActionText)") && teacherSource.Contains("TextRenderer.MeasureText"),
        "teacher overview action label should measure text and reserve single-line width");

    IEnumerable<string> actionTexts = OverviewActionBuilder.BuildStudentActions(true, true, true, true, true)
        .Concat(OverviewActionBuilder.BuildTeacherActions(true, true, true, true, true))
        .Select(action => action.ActionText);
    int widestAction = actionTexts.Max(text => TextRenderer.MeasureText(text, AppFonts.Semibold(9.5f)).Width);
    AssertTrue(widestAction + 24 <= 150,
        "overview action label max width should fit current Vietnamese action text on one line");
});
Run("overview action rows can request dashboard navigation", () =>
{
    AssertTrue(typeof(UC_StudentDashboard).GetEvent("ActionNavigationRequested") != null,
        "student overview action rows must expose a navigation event");
    AssertTrue(typeof(UC_TeacherOverview).GetEvent("ActionNavigationRequested") != null,
        "teacher overview action rows must expose a navigation event");

    string root = RepoRoot();
    string studentDashboardSource = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "Forms", "Student", "StudentDashboard.cs"));
    string teacherDashboardSource = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "Forms", "Teacher", "TeacherDashboard.cs"));

    AssertTrue(studentDashboardSource.Contains("ActionNavigationRequested += (_, pageName) => NavigateToPage(pageName)"),
        "student dashboard must route overview action clicks");
    AssertTrue(teacherDashboardSource.Contains("ActionNavigationRequested += (_, pageName) => NavigateToPage(pageName)"),
        "teacher dashboard must route overview action clicks");
});
Run("teacher overview context panels are visually independent", () =>
{
    string root = RepoRoot();
    string source = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "UserControls", "Teacher", "UC_TeacherOverview.cs"));

    AssertTrue(source.Contains("private TableLayoutPanel _contextGrid"),
        "teacher overview should use a context grid instead of one outer context card");
    AssertFalse(source.Contains("private RoundedPanel _contextPanel"),
        "teacher overview should not wrap context cards in a shared outer panel");
    AssertTrue(source.Contains("CreateCardTitle(\"Lịch dạy và việc cần xử lý\")"),
        "teacher context heading should use Vietnamese copy without mnemonic ampersand");
    AssertFalse(source.Contains("Lịch dạy &"),
        "teacher context heading should not use ampersand because WinForms treats it as a mnemonic");
});
Run("sidebar headings render larger than caption text", () =>
{
    string root = RepoRoot();
    string source = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "Theme", "SidebarPanel.cs"));
    string colorsSource = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "Theme", "AppColors.cs"));

    AssertTrue(source.Contains("AppFonts.Semibold(10f)"),
        "sidebar headings should use a larger semibold font");
    AssertTrue(source.Contains("return collapsed ? 10 : 32;"),
        "expanded sidebar headings should reserve taller heading rows");
    AssertTrue(source.Contains("SidebarHeadingText") && source.Contains("SidebarHeadingAccent") && source.Contains("SidebarHeadingBg"),
        "sidebar headings should use dedicated heading tokens instead of muted nav text");
    AssertTrue(colorsSource.Contains("SidebarHeadingText") && colorsSource.Contains("SidebarHeadingAccent") && colorsSource.Contains("SidebarHeadingBg"),
        "AppColors must expose light/dark heading tokens");
});
Run("student overview uses compact indicators and next actions instead of stat grid", () =>
{
    Type type = typeof(UC_StudentDashboard);
    AssertTrue(type.GetField("_indicatorStrip", BindingFlags.Instance | BindingFlags.NonPublic) != null,
        "student overview must expose compact indicator strip");
    AssertTrue(type.GetField("_nextActionList", BindingFlags.Instance | BindingFlags.NonPublic) != null,
        "student overview must expose next action list");
    AssertTrue(type.GetField("_statsGrid", BindingFlags.Instance | BindingFlags.NonPublic) == null,
        "student overview should not keep the old large stats grid");
});
Run("teacher overview uses compact indicators and next actions instead of stat grid", () =>
{
    Type type = typeof(UC_TeacherOverview);
    AssertTrue(type.GetField("_indicatorStrip", BindingFlags.Instance | BindingFlags.NonPublic) != null,
        "teacher overview must expose compact indicator strip");
    AssertTrue(type.GetField("_nextActionList", BindingFlags.Instance | BindingFlags.NonPublic) != null,
        "teacher overview must expose next action list");
    AssertTrue(type.GetField("_statsGrid", BindingFlags.Instance | BindingFlags.NonPublic) == null,
        "teacher overview should not keep the old large stats grid");
});
Run("profile pages retain role metrics and account security sections", () =>
{
    string root = RepoRoot();
    string studentProfileSource = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "UserControls", "Student", "UC_Profile.cs"));
    string teacherProfileSource = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "UserControls", "Teacher", "UC_TeacherProfile.cs"));

    AssertTrue(studentProfileSource.Contains("Thong tin hoc tap") || studentProfileSource.Contains("Thông tin học tập"),
        "student profile must keep learning metrics in profile");
    AssertTrue(teacherProfileSource.Contains("Thong tin giang day") || teacherProfileSource.Contains("Thông tin giảng dạy"),
        "teacher profile must keep teaching metrics in profile");
    AssertTrue(studentProfileSource.Contains("Bao mat tai khoan") || studentProfileSource.Contains("Bảo mật tài khoản"),
        "student profile must keep account security");
    AssertTrue(teacherProfileSource.Contains("Bao mat tai khoan") || teacherProfileSource.Contains("Bảo mật tài khoản"),
        "teacher profile must keep account security");
});
Run("sidebar supports visible expanded navigation groups without replacing old API", () =>
{
    Type sidebarType = typeof(SidebarPanel);
    AssertTrue(sidebarType.GetMethod("SetNavItems", new[] { typeof(string[]), typeof(string[]) }) != null,
        "SidebarPanel must preserve SetNavItems(string[], string[])");

    Type? itemType = typeof(SidebarPanel).Assembly.GetType("CourseGuard.Frontend.Theme.SidebarNavItem");
    AssertTrue(itemType != null, "SidebarNavItem must exist");
    AssertTrue(itemType!.GetProperty("Label") != null, "SidebarNavItem must expose Label");
    AssertTrue(itemType.GetProperty("Icon") != null, "SidebarNavItem must expose Icon");
    AssertTrue(itemType.GetProperty("IsHeading") != null, "SidebarNavItem must expose IsHeading");

    Type listType = typeof(IEnumerable<>).MakeGenericType(itemType);
    MethodInfo? groupedMethod = sidebarType.GetMethod("SetNavItems", new[] { listType });
    AssertTrue(groupedMethod != null, "SidebarPanel must expose SetNavItems(IEnumerable<SidebarNavItem>)");
});
Run("student and teacher dashboards define grouped sidebar navigation", () =>
{
    string root = RepoRoot();
    string studentSource = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "Forms", "Student", "StudentDashboard.cs"));
    string teacherSource = File.ReadAllText(Path.Combine(root, "CourseGuard", "CourseGuard", "Frontend", "Forms", "Teacher", "TeacherDashboard.cs"));

    AssertTrue(studentSource.Contains("new SidebarNavItem(\"Tổng quan\", string.Empty, isHeading: true)"),
        "student sidebar must include always-visible Tổng quan heading");
    AssertTrue(studentSource.Contains("new SidebarNavItem(\"Học tập\", string.Empty, isHeading: true)"),
        "student sidebar must include always-visible Học tập heading");
    AssertTrue(studentSource.Contains("new SidebarNavItem(\"Kiểm tra\", string.Empty, isHeading: true)"),
        "student sidebar must include always-visible Kiểm tra heading");
    AssertTrue(studentSource.Contains("new SidebarNavItem(\"Cộng đồng\", string.Empty, isHeading: true)"),
        "student sidebar must include always-visible Cộng đồng heading");
    AssertTrue(studentSource.Contains("new SidebarNavItem(\"Tài khoản\", string.Empty, isHeading: true)"),
        "student sidebar must include always-visible Tài khoản heading");

    AssertTrue(teacherSource.Contains("new SidebarNavItem(\"Giảng dạy\", string.Empty, isHeading: true)"),
        "teacher sidebar must include always-visible Giảng dạy heading");
    AssertTrue(teacherSource.Contains("new SidebarNavItem(\"Tổng quan\", string.Empty, isHeading: true)"),
        "teacher sidebar must include always-visible Tổng quan heading");
    AssertTrue(teacherSource.Contains("new SidebarNavItem(\"Kiểm tra\", string.Empty, isHeading: true)"),
        "teacher sidebar must include always-visible Kiểm tra heading");
    AssertTrue(teacherSource.Contains("new SidebarNavItem(\"Lớp học\", string.Empty, isHeading: true)"),
        "teacher sidebar must include always-visible Lớp học heading");
    AssertTrue(teacherSource.Contains("new SidebarNavItem(\"Thông tin\", string.Empty, isHeading: true)"),
        "teacher sidebar must include always-visible Thông tin heading");
    AssertTrue(teacherSource.Contains("new SidebarNavItem(\"Tài khoản\", string.Empty, isHeading: true)"),
        "teacher sidebar must include always-visible Tài khoản heading");

    AssertTrue(!studentSource.Contains("ToggleGroup") && !teacherSource.Contains("ToggleGroup"),
        "grouped sidebar must not introduce collapsible group state in this phase");
});
Run("teacher dashboard honors profile security focus requests", () =>
{
    Type type = typeof(TeacherDashboard);
    MethodInfo? navigate = type.GetMethod("NavigateToPage", BindingFlags.Instance | BindingFlags.NonPublic);
    AssertTrue(navigate != null, "TeacherDashboard must expose NavigateToPage");
    AssertTrue(navigate!.GetParameters().Any(p => p.Name == "focusSecurity" && p.ParameterType == typeof(bool)),
        "TeacherDashboard NavigateToPage must accept focusSecurity");
    AssertTrue(type.GetMethod("FocusProfileSecuritySection", BindingFlags.Instance | BindingFlags.NonPublic) != null,
        "TeacherDashboard must expose FocusProfileSecuritySection");
});
Run("student grid page base builds layout and empty state without db access", RunStudentGridPageBaseTests);
Run("student grid page base appends derived actions to header action strip", RunStudentGridPageHeaderActionTests);
Run("student grid page base shows error empty state on initial failure", RunStudentGridPageInitialFailureTests);
Run("student grid page base preserves initial error state across search rebind", RunStudentGridPageInitialFailureRebindTests);
Run("student grid page base preserves bound rows across failed refresh and rebind", RunStudentGridPageRefreshFailureTests);
Run("student grid concrete pages are sealed", () =>
{
    AssertTrue(typeof(UC_StudentLessons).IsSealed, "student lessons page should be sealed");
    AssertTrue(typeof(UC_StudentAssignments).IsSealed, "student assignments page should be sealed");
    AssertTrue(typeof(UC_Documents).IsSealed, "student documents page should be sealed");
});
Run("modern message dialog exposes requested buttons", RunMessageDialogButtonTests);
Run("deadline reminder service sends one action notification", RunDeadlineReminderServiceSendsOneNotification);
Run("deadline reminder service can send 1h reminder after 24h reminder", RunDeadlineReminderServiceSendsOneHourAfterTwentyFourHour);
Run("deadline reminder service retries after notification create failure", RunDeadlineReminderServiceRetriesAfterCreateFailure);
Run("deadline reminder service avoids duplicate concurrent claims", RunDeadlineReminderServiceAvoidsDuplicateConcurrentClaims);
Run("deadline reminder source wiring matches current notification architecture", RunDeadlineReminderSourceTests);
Run("native classroom records attendance in and out", RunNativeClassroomAttendanceSourceTests);
Run("teacher attendance summary is exposed through repository and controller", RunTeacherAttendanceSummarySourceTests);
Run("teacher students page opens attendance dialog", RunTeacherAttendanceUiSourceTests);
Run("attendance close cleanup and teacher schema are guarded", RunAttendanceCleanupAndSchemaSourceTests);
Run("chat unread API uses chat reads", RunChatUnreadSourceTests);
Run("student sidebar exposes chat unread badge", RunSidebarChatBadgeSourceTests);
Run("student dashboard loads and clears chat unread badge", RunStudentDashboardChatBadgeSourceTests);
Run("student chat page advances read watermark after refresh", RunStudentChatPageReadWatermarkSourceTests);

Console.WriteLine("Feature tests passed.");

static string RepoRoot()
{
    string current = AppContext.BaseDirectory;
    while (!string.IsNullOrWhiteSpace(current))
    {
        if (Directory.Exists(Path.Combine(current, "CourseGuard"))
            && Directory.Exists(Path.Combine(current, "tests")))
        {
            return current;
        }

        DirectoryInfo? parent = Directory.GetParent(current);
        if (parent == null)
            break;

        current = parent.FullName;
    }

    throw new InvalidOperationException("Cannot locate repository root.");
}

static async Task RunDeadlineReminderServiceSendsOneNotification()
{
    DateTime now = new DateTime(2026, 6, 6, 10, 0, 0, DateTimeKind.Unspecified);
    var store = new FakeDeadlineReminderStore();
    store.Deadlines.Add(new DeadlineReminderItem
    {
        SourceType = DeadlineReminderItem.SourceTypeAssignment,
        SourceId = 10,
        Title = "Final report",
        CourseName = "Network Programming",
        DueAt = now.AddHours(20)
    });
    int eventCount = 0;

    using var service = new DeadlineReminderService(
        userId: 42,
        store: store,
        clock: () => now,
        pollInterval: TimeSpan.FromMinutes(1),
        initialDelay: TimeSpan.Zero);
    service.NotificationCreated += (_, _) => eventCount++;

    await service.CheckNowAsync().ConfigureAwait(false);
    await service.CheckNowAsync().ConfigureAwait(false);

    AssertEqual(1, store.Created.Count);
    AssertEqual(1, store.Sent.Count);
    AssertEqual(1, eventCount);
    AssertEqual(WorkflowConstants.NotificationCategory.Assignment, store.Created[0].Category);
    AssertEqual(WorkflowConstants.NotificationType.ActionRequired, store.Created[0].NotificationType);
    AssertEqual(DeadlineReminderItem.SourceTypeAssignment, store.Created[0].SourceType);
    AssertEqual(10, store.Created[0].SourceId);
}

static async Task RunDeadlineReminderServiceSendsOneHourAfterTwentyFourHour()
{
    DateTime now = new DateTime(2026, 6, 6, 10, 0, 0, DateTimeKind.Unspecified);
    var store = new FakeDeadlineReminderStore();
    store.Deadlines.Add(new DeadlineReminderItem
    {
        SourceType = DeadlineReminderItem.SourceTypeExam,
        SourceId = 99,
        Title = "Midterm",
        CourseName = "Network Programming",
        DueAt = now.AddMinutes(45)
    });
    store.AlreadySent.Add(FakeDeadlineReminderStore.Key(7, DeadlineReminderItem.SourceTypeExam, 99, DeadlineReminderItem.ReminderType24H));

    using var service = new DeadlineReminderService(
        userId: 7,
        store: store,
        clock: () => now,
        pollInterval: TimeSpan.FromMinutes(1),
        initialDelay: TimeSpan.Zero);

    await service.CheckNowAsync().ConfigureAwait(false);

    AssertEqual(1, store.Created.Count);
    AssertEqual(1, store.Sent.Count);
    AssertTrue(store.Sent.Contains(FakeDeadlineReminderStore.Key(7, DeadlineReminderItem.SourceTypeExam, 99, DeadlineReminderItem.ReminderType1H)), "1h reminder must use a separate sent key");
    AssertEqual(WorkflowConstants.NotificationCategory.Exam, store.Created[0].Category);
}

static async Task RunDeadlineReminderServiceRetriesAfterCreateFailure()
{
    DateTime now = new DateTime(2026, 6, 6, 10, 0, 0, DateTimeKind.Unspecified);
    var store = new FakeDeadlineReminderStore();
    store.Deadlines.Add(new DeadlineReminderItem
    {
        SourceType = DeadlineReminderItem.SourceTypeAssignment,
        SourceId = 10,
        Title = "Final report",
        CourseName = "Network Programming",
        DueAt = now.AddHours(20)
    });
    store.FailNextCreate = true;

    using var service = new DeadlineReminderService(
        userId: 42,
        store: store,
        clock: () => now,
        pollInterval: TimeSpan.FromMinutes(1),
        initialDelay: TimeSpan.Zero);

    try
    {
        await service.CheckNowAsync().ConfigureAwait(false);
    }
    catch (InvalidOperationException)
    {
    }

    await service.CheckNowAsync().ConfigureAwait(false);

    AssertEqual(1, store.Created.Count);
    AssertEqual(1, store.Sent.Count);
}

static async Task RunDeadlineReminderServiceAvoidsDuplicateConcurrentClaims()
{
    DateTime now = new DateTime(2026, 6, 6, 10, 0, 0, DateTimeKind.Unspecified);
    var store = new FakeDeadlineReminderStore
    {
        CoordinateConcurrentClaims = true
    };
    store.Deadlines.Add(new DeadlineReminderItem
    {
        SourceType = DeadlineReminderItem.SourceTypeAssignment,
        SourceId = 10,
        Title = "Final report",
        CourseName = "Network Programming",
        DueAt = now.AddHours(20)
    });

    using var first = new DeadlineReminderService(
        userId: 42,
        store: store,
        clock: () => now,
        pollInterval: TimeSpan.FromMinutes(1),
        initialDelay: TimeSpan.Zero);
    using var second = new DeadlineReminderService(
        userId: 42,
        store: store,
        clock: () => now,
        pollInterval: TimeSpan.FromMinutes(1),
        initialDelay: TimeSpan.Zero);

    await Task.WhenAll(first.CheckNowAsync(), second.CheckNowAsync()).ConfigureAwait(false);

    AssertEqual(1, store.Created.Count);
    AssertEqual(1, store.Sent.Distinct().Count());
}

static void RunDeadlineReminderSourceTests()
{
    string repoRoot = RepoRoot();
    string servicePath = Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Backend", "Services", "DeadlineReminderService.cs");
    string dashboardPath = Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Frontend", "Forms", "Student", "StudentDashboard.cs");
    string notificationPagePath = Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Frontend", "UserControls", "Student", "UC_Notification.cs");
    string dbPath = Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Backend", "Data", "CourseGuardDbContext.cs");
    string notificationRepoPath = Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Backend", "Data", "NotificationRepository.cs");

    AssertTrue(File.Exists(servicePath), "deadline reminder service must exist");
    string service = File.ReadAllText(servicePath);
    string dashboard = File.ReadAllText(dashboardPath);
    string notificationPage = File.ReadAllText(notificationPagePath);
    string db = File.ReadAllText(dbPath);
    string notificationRepo = File.ReadAllText(notificationRepoPath);

    AssertFalse(service.Contains("CourseGuard.Frontend.Helpers"), "backend reminder service must not depend on frontend helpers");
    AssertFalse(service.Contains("InsertNotification"), "service must not use legacy notification helpers");
    AssertTrue(service.Contains("SemaphoreSlim"), "service must guard overlapping timer callbacks");
    AssertTrue(service.Contains("CreateDeadlineReminderNotification"), "service must delegate reminder notification creation to the atomic store operation");
    AssertFalse(service.Contains("TryClaimReminder"), "service must not claim reminders separately from creating notifications");
    AssertTrue(service.Contains("NotificationCreated"), "service must notify dashboard after creating a notification");
    AssertTrue(notificationRepo.Contains(": INotificationWriter"), "NotificationRepository must implement notification writer abstraction");
    AssertTrue(db.Contains("GetUpcomingDeadlines"), "db context must expose deadline query");
    AssertTrue(db.Contains("deadline_reminders_sent"), "db context must create/use deadline reminder tracking table");
    AssertTrue(db.Contains("WITH claimed"), "db context must insert the reminder marker and notification in one SQL operation");
    AssertTrue(db.Contains("BeginTransaction"), "db context must wrap deadline reminder notification creation in a transaction");
    AssertTrue(db.Contains("s.id IS NULL"), "assignment reminders must skip submitted assignments");
    AssertTrue(db.Contains("exam_questions"), "exam reminders must require questions");
    AssertTrue(dashboard.Contains("DeadlineReminderService"), "student dashboard must own the deadline reminder service");
    AssertTrue(dashboard.Contains("RefreshVisibleNotificationPage"), "dashboard must refresh the notification page explicitly");
    AssertTrue(notificationPage.Contains("RefreshAsync"), "notification user control must expose refresh for dashboard events");
}

static void RunNativeClassroomAttendanceSourceTests()
{
    string path = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Frontend", "Forms", "Student", "StudentNativeClassroomForm.cs");
    string source = File.ReadAllText(path);
    AssertTrue(source.Contains("CourseGuardDbContext"), "student native classroom must depend on db context for attendance logging");
    AssertTrue(source.Contains("_attendanceLogId"), "student native classroom must retain the attendance log id");
    AssertTrue(source.Contains("LogAttendanceInAsync"), "student native classroom must log attendance in after joining");
    AssertTrue(source.Contains("LogAttendanceOutAsync"), "student native classroom must log attendance out on close");
    int joinIndex = source.IndexOf("JoinRoomAsync", StringComparison.Ordinal);
    int attendanceInIndex = source.IndexOf("LogAttendanceInAsync", StringComparison.Ordinal);
    AssertTrue(joinIndex >= 0, "student native classroom must join the room before attendance logging");
    AssertTrue(attendanceInIndex >= 0, "student native classroom must log attendance in after joining");
    AssertTrue(joinIndex < attendanceInIndex, "attendance in should be recorded only after the classroom join succeeds");
}

static void RunTeacherAttendanceSummarySourceTests()
{
    string repoPath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Backend", "Data", "TeacherRepository.cs");
    string controllerPath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Backend", "Controllers", "TeacherController.cs");
    string modelPath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Backend", "Models", "AttendanceLogModel.cs");
    string repo = File.ReadAllText(repoPath);
    string controller = File.ReadAllText(controllerPath);
    string model = File.ReadAllText(modelPath);
    AssertTrue(model.Contains("StudentName"), "attendance model must expose student name for grid display");
    AssertTrue(repo.Contains("GetAttendanceSummary"), "teacher repository must query attendance summary");
    AssertTrue(repo.Contains("attendance_logs"), "attendance summary must read from attendance_logs");
    AssertTrue(repo.Contains("c.teacher_id = @teacher_id"), "attendance summary must be scoped to the teacher's courses");
    AssertTrue(controller.Contains("GetAttendanceSummary"), "teacher controller must expose attendance summary");
}

static void RunTeacherAttendanceUiSourceTests()
{
    string pagePath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Frontend", "UserControls", "Teacher", "UC_TeacherStudents.cs");
    string dialogPath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Frontend", "Forms", "Teacher", "TeacherAttendanceDialog.cs");
    string page = File.ReadAllText(pagePath);
    string dialog = File.ReadAllText(dialogPath);
    AssertTrue(File.Exists(dialogPath), "teacher attendance dialog must exist");
    AssertTrue(page.Contains("TeacherAttendanceDialog"), "teacher students page must open attendance dialog");
    AssertTrue(page.Contains("AddHeaderAction"), "attendance action should be added to the existing grid page header");
    AssertTrue(dialog.Contains("_isBindingSessions"), "attendance dialog must guard initial session binding from duplicate attendance loads");
    AssertTrue(dialog.Contains("if (_isBindingSessions)"), "attendance dialog session changed handler must skip initial binding");
}

static void RunAttendanceCleanupAndSchemaSourceTests()
{
    string classroomPath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Frontend", "Forms", "Student", "StudentNativeClassroomForm.cs");
    string repoPath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Backend", "Data", "TeacherRepository.cs");
    string classroom = File.ReadAllText(classroomPath);
    string repo = File.ReadAllText(repoPath);

    AssertTrue(classroom.Contains("_isClosing"), "native classroom close handler must guard re-entrant close");
    AssertTrue(classroom.Contains("_cleanupComplete"), "native classroom must track when async close cleanup is complete");
    AssertTrue(classroom.Contains("_attendanceInTask"), "native classroom must retain in-flight attendance-in task during close");
    AssertTrue(classroom.Contains("await _attendanceInTask"), "native classroom close cleanup must await in-flight attendance-in");
    AssertTrue(classroom.Contains("if (_isClosing)"), "native classroom OnShown path must guard close state");
    AssertTrue(classroom.Contains("e.Cancel = true"), "native classroom close handler must cancel the first close while async cleanup runs");
    AssertTrue(classroom.Contains("if (_cleanupComplete)"), "native classroom final programmatic close must be allowed after cleanup");
    AssertTrue(classroom.Contains("_cleanupComplete = true"), "native classroom must mark cleanup complete before final programmatic close");
    int closeHandlerIndex = classroom.IndexOf("private async void StudentNativeClassroomForm_FormClosing", StringComparison.Ordinal);
    int cleanupGuardIndex = classroom.IndexOf("if (_cleanupComplete)", closeHandlerIndex, StringComparison.Ordinal);
    int cancelIndex = classroom.IndexOf("e.Cancel = true", closeHandlerIndex, StringComparison.Ordinal);
    int inProgressGuardIndex = classroom.IndexOf("if (_isClosing)", closeHandlerIndex, StringComparison.Ordinal);
    AssertTrue(cleanupGuardIndex >= 0 && cancelIndex >= 0 && cleanupGuardIndex < cancelIndex, "native classroom must allow completed cleanup close before canceling active cleanup closes");
    AssertTrue(cancelIndex >= 0 && inProgressGuardIndex > cancelIndex, "native classroom re-entrant close must be canceled before returning while cleanup is in progress");
    AssertTrue(classroom.Contains("CloseClassroomAsync"), "native classroom close cleanup should be moved to an async helper");
    AssertTrue(classroom.Contains("await LogAttendanceOutIfPossibleAsync()"), "native classroom close cleanup must await attendance out");
    AssertTrue(repo.Contains("CREATE TABLE IF NOT EXISTS attendance_logs"), "teacher repository schema must create attendance_logs on clean databases");
    AssertTrue(repo.Contains("idx_attendance_logs_session_student"), "teacher repository schema must create session/student attendance index");
    AssertTrue(repo.Contains("idx_attendance_logs_open_session_student"), "teacher repository schema must create open attendance index");
}

static void RunChatUnreadSourceTests()
{
    string dbPath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Backend", "Data", "CourseGuardDbContext.cs");
    string controllerPath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Backend", "Controllers", "ChatController.cs");
    string db = File.ReadAllText(dbPath);
    string controller = File.ReadAllText(controllerPath);
    AssertTrue(db.Contains("GetUnreadChatCount"), "db context must expose unread chat count");
    AssertTrue(db.Contains("MarkAllChatRead"), "db context must expose mark all read");
    AssertTrue(db.Contains("MarkCourseChatRead"), "db context must expose course-scoped mark read");
    AssertTrue(db.Contains("CHAT_READS"), "unread logic must use CHAT_READS");
    AssertTrue(db.Contains("LAST_READ_MESSAGE_ID"), "unread logic must compare against last read message id");
    AssertTrue(db.Contains("m.SENDER_ID <> @user_id"), "unread count must exclude messages sent by the current user");
    AssertTrue(db.Contains("COALESCE(m.IS_DELETED, FALSE) = FALSE"), "unread count must exclude deleted messages");
    AssertTrue(db.Contains("c.TEACHER_ID = @user_id"), "unread visibility must include teacher-owned courses");
    AssertTrue(db.Contains("FROM ENROLLMENTS e"), "unread visibility must include enrolled student courses");
    AssertTrue(db.Contains("UPPER(COALESCE(e.STATUS, 'ACTIVE')) IN ('ACTIVE', 'APPROVED')"), "unread visibility must require active or approved enrollment");
    AssertTrue(db.Contains("LEFT JOIN CHAT_READS cr"), "unread count must join chat read watermarks");
    AssertTrue(db.Contains("cr.LAST_READ_MESSAGE_ID IS NULL OR m.ID > cr.LAST_READ_MESSAGE_ID"), "unread count must compare message id to last read watermark");
    AssertTrue(db.Contains("ON CONFLICT (USER_ID, COURSE_ID) DO UPDATE"), "mark read must upsert one read watermark per course");
    AssertTrue(db.Contains("WHERE m.COURSE_ID = @course_id"), "course-scoped mark read must only advance one selected course");
    AssertTrue(db.Contains("WHERE CHAT_READS.LAST_READ_MESSAGE_ID IS NULL"), "mark read must guard null existing watermarks");
    AssertTrue(db.Contains("EXCLUDED.LAST_READ_MESSAGE_ID > CHAT_READS.LAST_READ_MESSAGE_ID"), "mark read must not move the watermark backwards");
    AssertTrue(controller.Contains("GetUnreadCount"), "chat controller must expose unread count");
    AssertTrue(controller.Contains("MarkAllRead"), "chat controller must expose mark all read");
    AssertTrue(controller.Contains("MarkCourseRead"), "chat controller must expose course-scoped mark read");
}

static void RunSidebarChatBadgeSourceTests()
{
    string sidebarPath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Frontend", "Theme", "SidebarPanel.cs");
    string badgePath = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Frontend", "Theme", "CountBadgePainter.cs");
    string sidebar = File.ReadAllText(sidebarPath);
    string badge = File.ReadAllText(badgePath);
    AssertTrue(File.Exists(badgePath), "count badge painter helper must exist");
    AssertTrue(sidebar.Contains("ChatUnreadCount"), "sidebar must expose chat unread count");
    AssertTrue(sidebar.Contains("IsChatNavItem(iconKey)"), "sidebar badge must target the stable nav icon key");
    AssertTrue(sidebar.Contains("\"message\""), "sidebar badge must target the message nav icon key");
    AssertFalse(sidebar.Contains("Tin nhÃ¡ÂºÂ¯n"), "sidebar badge should not depend on mojibake label fallbacks");
    AssertTrue(sidebar.Contains("CountBadgePainter.Draw"), "sidebar must draw the badge through the helper");
    AssertTrue(sidebar.Contains("_chatUnreadCount > 99 ? 26 : badgeSize"), "sidebar must reserve pill width for capped counts");
    AssertTrue(badge.Contains("99+"), "count badge painter must cap large counts");
    AssertTrue(badge.Contains("text.Length >= 3"), "count badge painter must widen three-character labels");
    AssertTrue(badge.Contains("AddArc"), "count badge painter must draw pill badges for wider labels");
}

static void RunStudentDashboardChatBadgeSourceTests()
{
    string path = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Frontend", "Forms", "Student", "StudentDashboard.cs");
    string source = File.ReadAllText(path);
    AssertTrue(source.Contains("ChatController"), "student dashboard must use chat controller for unread count");
    AssertTrue(source.Contains("GetUnreadCount"), "student dashboard must load unread count");
    AssertTrue(source.Contains("ChatUnreadCount"), "student dashboard must update sidebar chat badge");
    AssertTrue(source.Contains("MarkAllRead"), "student dashboard must mark chat read when user opens chat");
    AssertTrue(source.Contains("LoadChatUnreadCountAsync().FireAndForgetSafe(this)"), "student dashboard must load chat unread count without blocking construction");
    AssertTrue(source.Contains("Task.Run"), "student dashboard must move chat unread database work off the UI thread");
    AssertTrue(source.Contains("SetChatUnreadCountSafe"), "student dashboard must marshal unread badge UI updates safely");
    AssertTrue(source.Contains("BeginInvoke"), "student dashboard must use BeginInvoke for cross-thread unread badge updates");
    AssertTrue(source.Contains("MarkAllChatReadAsync().FireAndForgetSafe(this)"), "student dashboard must mark chat read in the background");
    AssertTrue(source.Contains("private const string ChatPage"), "student dashboard should use one chat page key for navigation and unread guards");
    AssertTrue(source.Contains("_chatUnreadLoadVersion"), "student dashboard must version unread badge loads");
    AssertTrue(source.Contains("int loadVersion = _chatUnreadLoadVersion"), "student dashboard must capture unread badge load version before background work");
    AssertTrue(source.Contains("_chatUnreadLoadVersion++"), "student dashboard must invalidate pending unread badge loads when chat opens");
    AssertTrue(source.Contains("SetChatUnreadCountSafe(unreadCount, loadVersion)"), "student dashboard must pass unread load version into UI update");
    AssertTrue(source.Contains("loadVersion != _chatUnreadLoadVersion || _currentPageName == ChatPage"), "student dashboard must suppress stale unread badge updates while chat is open");
    int loadUiIndex = source.IndexOf("LoadUI(factory())", StringComparison.Ordinal);
    int markReadIndex = source.IndexOf("MarkAllChatReadAsync().FireAndForgetSafe(this)", StringComparison.Ordinal);
    AssertTrue(loadUiIndex >= 0 && markReadIndex > loadUiIndex, "student dashboard must not block chat navigation on mark-read database work");
}

static void RunStudentChatPageReadWatermarkSourceTests()
{
    string path = Path.Combine(RepoRoot(), "CourseGuard", "CourseGuard", "Frontend", "UserControls", "Student", "UC_Chat.cs");
    string source = File.ReadAllText(path);
    AssertTrue(source.Contains("MarkDisplayedMessagesReadAsync"), "student chat page must own a read watermark advancement helper");
    AssertTrue(source.Contains("int courseId = _selectedCourseId"), "student chat page must capture selected course before background mark-read");
    AssertTrue(source.Contains("_selectedCourseId != courseId"), "student chat page must ignore stale refreshes after selected course changes");
    AssertTrue(source.Contains("_chatController.MarkCourseRead(userId, courseId)"), "student chat page must advance only the selected course read watermark through ChatController");
    AssertFalse(source.Contains("MarkAllRead"), "student chat page must not mark every course read during selected-course refresh");
    AssertTrue(source.Contains("MarkDisplayedMessagesReadAsync(courseId).FireAndForgetSafe(this)"), "student chat page must advance read watermark in the background after refresh");
    int emptyStateIndex = source.IndexOf("if (messages.Count == 0)", StringComparison.Ordinal);
    int markReadIndex = source.IndexOf("MarkDisplayedMessagesReadAsync(courseId).FireAndForgetSafe(this)", StringComparison.Ordinal);
    AssertTrue(emptyStateIndex >= 0 && markReadIndex > emptyStateIndex, "student chat page must mark read only after successful message rendering");
}

static void RunDashboardCardTests()
{
    using var studentGrid = new DataGridView();
    using var teacherGrid = new DataGridView();

    Control studentCard = InvokeChromeDataCard(
        "CourseGuard.Frontend.UserControls.Student.StudentTabChrome",
        "Danh sach",
        studentGrid);
    Control teacherCard = InvokeChromeDataCard(
        "CourseGuard.Frontend.UserControls.Teacher.TeacherTabChrome",
        "Danh sach",
        teacherGrid);

    AssertGridIsWrappedInRoundedBody(studentCard, studentGrid, "student table data card");
    AssertGridIsWrappedInRoundedBody(teacherCard, teacherGrid, "teacher table data card");

    studentCard.Dispose();
    teacherCard.Dispose();
}

static void RunDashboardHeaderSubtitleLayoutTests()
{
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        try
        {
            using Control studentHeader = InvokeChromeHeader(
                "CourseGuard.Frontend.UserControls.Student.StudentTabChrome",
                "Tổng quan cá nhân",
                "Theo dõi khóa học đang tham gia, bài kiểm tra, thông báo và hoạt động gần đây.");
            using Control teacherHeader = InvokeChromeHeader(
                "CourseGuard.Frontend.UserControls.Teacher.TeacherTabChrome",
                "Lịch dạy",
                "Quản lý lịch học và mở lớp trực tuyến");

            AssertHeaderSubtitleHasDescenderRoom(studentHeader, "student header");
            AssertHeaderSubtitleHasDescenderRoom(teacherHeader, "teacher header");
        }
        catch (Exception ex)
        {
            failure = ex;
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure != null)
        throw new InvalidOperationException(failure.Message, failure);
}

static void RunRoundedChromeClippingGuardTests()
{
    string repoRoot = RepoRoot();
    string metaTheme = File.ReadAllText(Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Frontend", "Theme", "MetaTheme.cs"));
    string dropdownStyler = File.ReadAllText(Path.Combine(repoRoot, "CourseGuard", "CourseGuard", "Frontend", "Theme", "StudentDropdownStyler.cs"));

    AssertTrue(
        metaTheme.Contains("new Rectangle(-1, -1, Width + 2, Height + 2), CornerRadius + 1", StringComparison.Ordinal),
        "themed message dialog region should overscan rounded corners so the outer border is not clipped");
    AssertTrue(
        metaTheme.Contains("Margin = new Padding(CornerRadius, 0, CornerRadius, 0)", StringComparison.Ordinal),
        "themed message dialog tone strip should stay clear of rounded top corners");
    AssertTrue(
        dropdownStyler.Contains("new RectangleF(1f, 1f, combo.Width - 2f, combo.Height - 2f)", StringComparison.Ordinal),
        "styled combobox chrome should draw its border fully inside the window DC to preserve the bottom edge");
    AssertTrue(
        dropdownStyler.Contains("combo.Height - 2", StringComparison.Ordinal)
            && dropdownStyler.Contains("g.DrawLine(pen, 10, bottomY, combo.Width - 11, bottomY)", StringComparison.Ordinal),
        "styled combobox chrome should explicitly redraw the bottom border inside the control");
}

static void RunStudentGridPageBaseTests()
{
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        try
        {
            using var page = new TestStudentGridPage();
            TableLayoutPanel root = page.Controls.OfType<TableLayoutPanel>().Single();

            AssertEqual(0, page.CreateTableCallCount);
            AssertEqual(1, root.ColumnCount);
            AssertEqual(2, root.RowCount);
            AssertTrue(page.Controls.Contains(root), "base page should create the root layout");
            AssertTrue(GetAllControls(page).Contains(page.ExposedGrid), "root layout should contain the grid");
            AssertTrue(GetAllControls(page).Contains(page.ExposedRefreshButton), "header should contain the refresh button");
            AssertTrue(page.ExposedSearchBox != null, "base page should create requested search input");
            AssertTrue(page.ExposedCourseFilter != null, "base page should create requested course filter");
            AssertTrue(page.ExposedSearchButton != null, "base page should create requested search button");
            AssertTrue(GetAllControls(page).Contains(page.ExposedSearchBox!), "header should contain the search input");
            AssertTrue(GetAllControls(page).Contains(page.ExposedCourseFilter!), "header should contain the course filter");
            AssertTrue(GetAllControls(page).Contains(page.ExposedSearchButton!), "header should contain the search button");

            AssertTrue(page.ExposedGrid.ReadOnly, "grid should be styled readonly");
            AssertEqual(BorderStyle.None, page.ExposedGrid.BorderStyle);
            AssertEqual(DataGridViewSelectionMode.FullRowSelect, page.ExposedGrid.SelectionMode);
            AssertFalse(page.ExposedGrid.EnableHeadersVisualStyles, "grid should use theme header styles");

            page.LoadForTestAsync().GetAwaiter().GetResult();

            AssertEqual(1, page.CreateTableCallCount);
            AssertTrue(page.ExposedBindingSource.DataSource is DataTable, "load should bind a DataTable");
            AssertFalse(page.ExposedGrid.Visible, "empty table should hide the grid");
            AssertTrue(page.ExposedGridBody.Controls.Contains(page.ExposedEmptyStateLabel), "table body should contain empty label");
            AssertTrue(page.ExposedEmptyStateLabel.Visible, "empty table should show empty state label");
            AssertEqual("No rows for this test.", page.ExposedEmptyStateLabel.Text);

            page.RebindCachedRows();

            AssertTrue(page.ExposedBindingSource.DataSource is DataTable, "successful empty load should allow local rebind");
            AssertFalse(page.ExposedGrid.Visible, "empty rebind should keep the grid hidden");
            AssertEqual("No rows for this test.", page.ExposedEmptyStateLabel.Text);
        }
        catch (Exception ex)
        {
            failure = ex;
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure != null)
        throw new InvalidOperationException(failure.Message, failure);
}

static void RunStudentGridPageHeaderActionTests()
{
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        try
        {
            using var page = new TestStudentGridPage();
            TableLayoutPanel root = page.Controls.OfType<TableLayoutPanel>().Single();
            Control header = root.GetControlFromPosition(0, 0)
                ?? throw new InvalidOperationException("base page should create a header");
            TableLayoutPanel headerLayout = header.Controls.OfType<TableLayoutPanel>().Single();
            Control titleStack = headerLayout.GetControlFromPosition(0, 0)
                ?? throw new InvalidOperationException("header should contain a title stack");
            Control actionStrip = headerLayout.GetControlFromPosition(1, 0)
                ?? throw new InvalidOperationException("header should contain an action strip");

            AssertFalse(titleStack.Controls.Contains(page.ExposedExtraAction), "derived action must not be added to title stack");
            AssertTrue(actionStrip.Controls.Contains(page.ExposedExtraAction), "derived action should be added to header action strip");
        }
        catch (Exception ex)
        {
            failure = ex;
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure != null)
        throw new InvalidOperationException(failure.Message, failure);
}

static void RunStudentGridPageRefreshFailureTests()
{
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        try
        {
            using var page = new TestStudentGridPage();
            page.SetNextRows("Cached row");
            page.LoadForTestAsync().GetAwaiter().GetResult();

            DataTable boundBeforeFailure = page.ExposedBindingSource.DataSource as DataTable
                ?? throw new InvalidOperationException("successful load should bind a DataTable");
            AssertEqual(1, boundBeforeFailure.Rows.Count);
            AssertTrue(page.ExposedGrid.Visible, "successful load should show the grid");

            page.FailNextLoad();
            RunWithAutoClosingThemedDialog(() => page.LoadForTestAsync());

            AssertTrue(ReferenceEquals(boundBeforeFailure, page.ExposedBindingSource.DataSource), "failed refresh should preserve prior binding");
            AssertTrue(page.ExposedGrid.Visible, "failed refresh should not hide previously bound rows");
            AssertEqual(1, page.ExposedGrid.Rows.Count);

            page.RebindCachedRows();

            AssertTrue(page.ExposedGrid.Visible, "cache rebind should keep rows visible");
            AssertEqual(1, page.ExposedGrid.Rows.Count);
            AssertEqual("Cached row", page.ExposedGrid.Rows[0].Cells["Name"].Value?.ToString());
        }
        catch (Exception ex)
        {
            failure = ex;
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure != null)
        throw new InvalidOperationException(failure.Message, failure);
}

static void RunStudentGridPageInitialFailureRebindTests()
{
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        try
        {
            using var page = new TestStudentGridPage();
            page.FailNextLoad();
            RunWithAutoClosingThemedDialog(() => page.LoadForTestAsync());

            page.RebindCachedRows();

            AssertEqual<object?>(null, page.ExposedBindingSource.DataSource);
            AssertFalse(page.ExposedGrid.Visible, "search rebind after initial failure should keep the grid hidden");
            AssertTrue(page.ExposedEmptyStateLabel.Visible, "search rebind after initial failure should preserve error state");
            AssertEqual("Không thể tải dữ liệu.", page.ExposedEmptyStateLabel.Text);
        }
        catch (Exception ex)
        {
            failure = ex;
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure != null)
        throw new InvalidOperationException(failure.Message, failure);
}

static void RunStudentGridPageInitialFailureTests()
{
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        try
        {
            using var page = new TestStudentGridPage();
            page.FailNextLoad();
            RunWithAutoClosingThemedDialog(() => page.LoadForTestAsync());

            AssertEqual<object?>(null, page.ExposedBindingSource.DataSource);
            AssertFalse(page.ExposedGrid.Visible, "initial failure should hide the grid");
            AssertTrue(page.ExposedEmptyStateLabel.Visible, "initial failure should show the error empty state");
            AssertEqual("Không thể tải dữ liệu.", page.ExposedEmptyStateLabel.Text);
        }
        catch (Exception ex)
        {
            failure = ex;
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure != null)
        throw new InvalidOperationException(failure.Message, failure);
}

static void RunMessageDialogButtonTests()
{
    using Form yesNoCancel = CreateThemedDialog(
        "Ban co chac muon dang xuat?",
        "Dang xuat",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question);
    AssertEqual(255, yesNoCancel.BackColor.A);
    AssertDialogResults(
        yesNoCancel,
        new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel },
        "logout confirmation should expose yes/no/cancel actions");

    using Form okCancel = CreateThemedDialog(
        "Thao tac nay can xac nhan.",
        "Xac nhan",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Warning);
    AssertDialogResults(
        okCancel,
        new[] { DialogResult.OK, DialogResult.Cancel },
        "ok/cancel dialog should expose both actions");
}

static string FormatVietnamTime(DateTime value)
{
    Type formatterType = typeof(AppColors).Assembly.GetType("CourseGuard.Frontend.Theme.SystemTimeFormatter")
        ?? throw new InvalidOperationException("Cannot find SystemTimeFormatter");
    MethodInfo method = formatterType.GetMethod("FormatVietnamTime", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Cannot find FormatVietnamTime");
    return (string)(method.Invoke(null, new object[] { value })
        ?? throw new InvalidOperationException("FormatVietnamTime returned null"));
}

static Form CreateThemedDialog(string content, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
{
    Type dialogType = typeof(StudentExamScoringService).Assembly.GetType("CourseGuard.Frontend.Theme.MetaTheme+ThemedMessageDialog")
        ?? throw new InvalidOperationException("Cannot find MetaTheme.ThemedMessageDialog");
    object? dialog = TryCreateDialog(dialogType, new object?[] { content, title, buttons, icon, null, null, null })
        ?? TryCreateDialog(dialogType, new object?[] { content, title, buttons, icon });
    return (Form)(dialog
        ?? throw new InvalidOperationException("Cannot construct themed dialog"));
}

static object? TryCreateDialog(Type dialogType, object?[] args)
{
    try
    {
        return Activator.CreateInstance(dialogType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, args, null);
    }
    catch (MissingMethodException)
    {
        return null;
    }
}

static void RunWithAutoClosingThemedDialog(Func<Task> action)
{
    using var timer = new System.Windows.Forms.Timer { Interval = 20 };
    timer.Tick += (_, _) =>
    {
        Form? dialog = Application.OpenForms
            .Cast<Form>()
            .FirstOrDefault(form => form.GetType().Name.Contains("ThemedMessageDialog", StringComparison.Ordinal));
        if (dialog == null)
            return;

        dialog.DialogResult = DialogResult.OK;
        dialog.Close();
    };

    timer.Start();
    try
    {
        action().GetAwaiter().GetResult();
    }
    finally
    {
        timer.Stop();
    }
}

static void AssertDialogResults(Form dialog, DialogResult[] expected, string scenario)
{
    DialogResult[] actual = GetAllControls(dialog)
        .OfType<Button>()
        .Where(button => button.DialogResult != DialogResult.None)
        .Select(button => button.DialogResult)
        .OrderBy(result => result.ToString())
        .ToArray();
    DialogResult[] sortedExpected = expected.OrderBy(result => result.ToString()).ToArray();

    if (!actual.SequenceEqual(sortedExpected))
    {
        string actualText = string.Join(", ", actual);
        string expectedText = string.Join(", ", sortedExpected);
        throw new InvalidOperationException($"{scenario}: expected [{expectedText}], got [{actualText}]");
    }
}

static IEnumerable<Control> GetAllControls(Control root)
{
    foreach (Control child in root.Controls)
    {
        yield return child;
        foreach (Control grandChild in GetAllControls(child))
            yield return grandChild;
    }
}

static Control InvokeChromeDataCard(string typeName, string title, Control content)
{
    Type chromeType = typeof(StudentExamScoringService).Assembly.GetType(typeName)
        ?? throw new InvalidOperationException($"Cannot find {typeName}");
    MethodInfo method = chromeType.GetMethod("CreateDataCard", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException($"Cannot find CreateDataCard on {typeName}");
    return (Control)(method.Invoke(null, new object[] { title, content })
        ?? throw new InvalidOperationException("CreateDataCard returned null"));
}

static Control InvokeChromeHeader(string typeName, string title, string subtitle)
{
    Type chromeType = typeof(StudentExamScoringService).Assembly.GetType(typeName)
        ?? throw new InvalidOperationException($"Cannot find {typeName}");
    MethodInfo method = chromeType.GetMethod(
            "CreateHeader",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(string), typeof(Control[]) },
            null)
        ?? throw new InvalidOperationException($"Cannot find CreateHeader(string, string, Control[]) on {typeName}");
    return (Control)(method.Invoke(null, new object[] { title, subtitle, Array.Empty<Control>() })
        ?? throw new InvalidOperationException("CreateHeader returned null"));
}

static void AssertGridIsWrappedInRoundedBody(Control card, DataGridView grid, string scenario)
{
    TableLayoutPanel layout = card.Controls.OfType<TableLayoutPanel>().Single();
    Control body = layout.GetControlFromPosition(0, 1)
        ?? throw new InvalidOperationException($"{scenario}: missing content body");
    if (body is not RoundedPanel)
        throw new InvalidOperationException($"{scenario}: expected DataGridView content to be hosted inside RoundedPanel");
    if (!body.Controls.Contains(grid))
        throw new InvalidOperationException($"{scenario}: RoundedPanel does not contain original grid");
}

static void AssertHeaderSubtitleHasDescenderRoom(Control header, string scenario)
{
    TableLayoutPanel layout = header.Controls.OfType<TableLayoutPanel>().Single();
    FlowLayoutPanel titleStack = layout.GetControlFromPosition(0, 0) as FlowLayoutPanel
        ?? throw new InvalidOperationException($"{scenario}: missing title stack");
    Label[] labels = titleStack.Controls.OfType<Label>().ToArray();
    if (labels.Length < 2)
        throw new InvalidOperationException($"{scenario}: missing subtitle label");

    Label subtitle = labels[1];
    AssertFalse(subtitle.AutoSize, $"{scenario}: subtitle should use fixed height instead of clipping-prone AutoSize");
    AssertFalse(subtitle.UseCompatibleTextRendering, $"{scenario}: subtitle should use native TextRenderer to avoid clipping descenders");
    AssertTrue(subtitle.Height >= 28, $"{scenario}: subtitle should reserve enough height for Vietnamese descenders");
    AssertTrue(subtitle.Padding.Bottom >= 4, $"{scenario}: subtitle should keep bottom padding for glyph descenders");

    int requiredHeight = header.Padding.Top + header.Padding.Bottom + labels.Sum(label => label.Height + label.Margin.Vertical);
    AssertTrue(header.MinimumSize.Height >= requiredHeight, $"{scenario}: header card should be tall enough for title and subtitle without clipping");
    AssertTrue(header.Height >= header.MinimumSize.Height, $"{scenario}: header card should use its minimum height instead of the default panel height");
    AssertTrue(header.Height <= 104, $"{scenario}: header card should stay compact and not push dashboard content down");
}

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, got {actual}");
}

static T GetPropertyValue<T>(object instance, string propertyName)
{
    PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Cannot find property {propertyName}");
    object? value = property.GetValue(instance);
    return value is T typed
        ? typed
        : throw new InvalidOperationException($"Property {propertyName} was not a {typeof(T).Name}");
}

static void SetFieldValue(object instance, string fieldName, object? value)
{
    FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Cannot find field {fieldName}");
    field.SetValue(instance, value);
}

static void SetStudentProfileTestFields(
    UC_Profile page,
    TextBox fullName,
    TextBox email,
    TextBox phone,
    TextBox birthDate,
    ErrorProvider errors)
{
    SetFieldValue(page, "txtFullName", fullName);
    SetFieldValue(page, "txtEmail", email);
    SetFieldValue(page, "txtPhone", phone);
    SetFieldValue(page, "txtBirthDate", birthDate);
    SetFieldValue(page, "txtAddress", new TextBox());
    SetFieldValue(page, "txtMajor", new TextBox());
    SetFieldValue(page, "txtBio", new TextBox());
    ComboBox gender = new();
    gender.Items.Add("Nam");
    gender.SelectedIndex = 0;
    SetFieldValue(page, "cboGender", gender);

    SetFieldValue(page, "lblValName", new Label());
    SetFieldValue(page, "lblValStudentCode", new Label());
    SetFieldValue(page, "lblEditStudentCode", new Label());
    SetFieldValue(page, "lblValEmail", new Label());
    SetFieldValue(page, "lblValPhone", new Label());
    SetFieldValue(page, "lblValAddress", new Label());
    SetFieldValue(page, "lblValMajor", new Label());
    SetFieldValue(page, "lblValGender", new Label());
    SetFieldValue(page, "lblValBirthDate", new Label());
    SetFieldValue(page, "lblValBio", new Label());
    SetFieldValue(page, "_lblHeaderName", new Label());
    SetFieldValue(page, "_studentCode", "HS00000");
    SetFieldValue(page, "_validationErrors", errors);
}

static bool InvokeProfileSaveWithDialogClose(Control owner, MethodInfo saveMethod)
{
    bool? result = null;
    RunWithAutoClosingThemedDialog(() =>
    {
        result = (bool)saveMethod.Invoke(owner, Array.Empty<object>())!;
        return Task.CompletedTask;
    });

    return result ?? throw new InvalidOperationException("Profile save did not return a result");
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

static void AssertProperty(Type type, string name)
{
    AssertTrue(type.GetProperty(name) != null, $"{type.Name} must expose {name}");
}

static void AssertProfileUsesValidatedSave(Type pageType, string pageName)
{
    const BindingFlags privateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    MethodInfo validatedSave = pageType.GetMethod("SaveProfileIfValid", privateInstance)
        ?? throw new InvalidOperationException($"{pageName} profile page must expose SaveProfileIfValid");
    MethodInfo validateInputs = pageType.GetMethod("ValidateProfileInputs", privateInstance)
        ?? throw new InvalidOperationException($"{pageName} profile page must expose ValidateProfileInputs");
    MethodInfo saveProfile = pageType.GetMethod("SaveProfile", privateInstance)
        ?? throw new InvalidOperationException($"{pageName} profile page must expose SaveProfile");
    MethodInfo changeAvatar = pageType.GetMethod("ChangeAvatar", privateInstance)
        ?? throw new InvalidOperationException($"{pageName} profile page must expose ChangeAvatar");

    AssertTrue(
        MethodCalls(validatedSave, validateInputs),
        $"{pageName} validated save path must run profile validation first");
    AssertTrue(
        MethodCalls(validatedSave, saveProfile),
        $"{pageName} validated save path must save only after validation");
    AssertTrue(
        MethodCalls(changeAvatar, validatedSave),
        $"{pageName} avatar changes must use the validated save path");
}

static bool MethodCalls(MethodInfo caller, MethodInfo callee)
{
    byte[]? il = caller.GetMethodBody()?.GetILAsByteArray();
    if (il == null)
        return false;

    Dictionary<ushort, EmitOpCode> opCodes = GetOpCodeMap();
    int offset = 0;
    while (offset < il.Length)
    {
        ushort opCodeValue = il[offset++];
        if (opCodeValue == 0xFE)
            opCodeValue = (ushort)(0xFE00 | il[offset++]);

        if (!opCodes.TryGetValue(opCodeValue, out EmitOpCode opCode))
            return false;

        int operandStart = offset;
        if (opCode.OperandType == EmitOperandType.InlineMethod)
        {
            int token = BitConverter.ToInt32(il, operandStart);
            try
            {
                MethodBase? resolved = caller.Module.ResolveMethod(
                    token,
                    caller.DeclaringType?.GetGenericArguments(),
                    caller.GetGenericArguments());
                if (resolved != null && resolved.Module == callee.Module && resolved.MetadataToken == callee.MetadataToken)
                    return true;
            }
            catch (ArgumentException)
            {
                // Keep scanning if this runtime cannot resolve a metadata token.
            }
        }

        offset += GetOperandSize(opCode.OperandType, il, operandStart);
    }

    return false;
}

static Dictionary<ushort, EmitOpCode> GetOpCodeMap()
{
    Dictionary<ushort, EmitOpCode> opCodes = new();
    foreach (FieldInfo field in typeof(EmitOpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
    {
        if (field.GetValue(null) is EmitOpCode opCode)
            opCodes[(ushort)opCode.Value] = opCode;
    }

    return opCodes;
}

static int GetOperandSize(EmitOperandType operandType, byte[] il, int operandStart)
{
    return operandType switch
    {
        EmitOperandType.InlineNone => 0,
        EmitOperandType.ShortInlineBrTarget or EmitOperandType.ShortInlineI or EmitOperandType.ShortInlineVar => 1,
        EmitOperandType.InlineVar => 2,
        EmitOperandType.InlineI or EmitOperandType.InlineBrTarget or EmitOperandType.InlineField or EmitOperandType.InlineMethod
            or EmitOperandType.InlineSig or EmitOperandType.InlineString or EmitOperandType.InlineTok or EmitOperandType.InlineType
            or EmitOperandType.ShortInlineR => 4,
        EmitOperandType.InlineI8 or EmitOperandType.InlineR => 8,
        EmitOperandType.InlineSwitch => 4 + (4 * BitConverter.ToInt32(il, operandStart)),
        _ => 0
    };
}

static TControl AssertControl<TControl>(Control control, string message)
    where TControl : Control
{
    AssertTrue(control is TControl, message);
    return (TControl)control;
}

static TControl SingleChild<TControl>(Control parent)
    where TControl : Control
{
    TControl[] matches = parent.Controls.OfType<TControl>().ToArray();
    AssertEqual(1, matches.Length);
    return matches[0];
}

static void RaiseFocusChanged(Control control, bool focused)
{
    string methodName = focused ? "OnGotFocus" : "OnLostFocus";
    MethodInfo method = typeof(Control).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingMethodException(nameof(Control), methodName);
    method.Invoke(control, new object[] { EventArgs.Empty });
}

static void AssertImageDisposed(Image image, string message)
{
    try
    {
        _ = image.Width;
    }
    catch (ObjectDisposedException)
    {
        return;
    }
    catch (ArgumentException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}

static Task StartConcurrentBroadcast(
    ClassroomOpenSignalCoordinator coordinator,
    int sessionId,
    CountdownEvent ready,
    ManualResetEventSlim releaseBroadcasts)
{
    return Task.Factory.StartNew(async () =>
    {
        ready.Signal();
        AssertTrue(releaseBroadcasts.Wait(TimeSpan.FromSeconds(3)), "broadcast release was not signaled");
        await coordinator.BroadcastClassOpenedAsync(sessionId);
    }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
}

static class TestRunner
{
    public static void Run(string name, Action test)
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

    public static void Run(string name, Func<Task> test)
    {
        try
        {
            test().GetAwaiter().GetResult();
            Console.WriteLine($"PASS {name}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FAIL {name}: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}

sealed class RecordingSynchronizationContext : SynchronizationContext
{
    private readonly Queue<(SendOrPostCallback Callback, object? State)> _callbacks = new();
    private readonly ManualResetEventSlim _posted = new();

    public override void Post(SendOrPostCallback d, object? state)
    {
        lock (_callbacks)
        {
            _callbacks.Enqueue((d, state));
        }

        _posted.Set();
    }

    public bool WaitForPost(TimeSpan timeout)
    {
        return _posted.Wait(timeout);
    }

    public void ExecuteAll()
    {
        while (true)
        {
            SendOrPostCallback callback;
            object? state;
            lock (_callbacks)
            {
                if (_callbacks.Count == 0)
                    return;

                (callback, state) = _callbacks.Dequeue();
            }

            callback(state);
        }
    }
}

sealed class FakeDeadlineReminderStore : IDeadlineReminderStore
{
    private readonly object _claimLock = new();
    public List<DeadlineReminderItem> Deadlines { get; } = new();
    public HashSet<string> AlreadySent { get; } = new();
    public List<string> Sent { get; } = new();
    public List<CreatedNotification> Created { get; } = new();
    public bool FailNextCreate { get; set; }
    public bool CoordinateConcurrentClaims { get; set; }
    private readonly CountdownEvent _concurrentClaims = new(2);
    private readonly ManualResetEventSlim _releaseConcurrentClaims = new();

    public void EnsureDeadlineReminderSchema()
    {
    }

    public List<DeadlineReminderItem> GetUpcomingDeadlines(int userId, DateTime from, DateTime to)
    {
        return Deadlines
            .Where(item => item.DueAt >= from && item.DueAt <= to)
            .ToList();
    }

    public int CreateDeadlineReminderNotification(
        int userId,
        DeadlineReminderItem item,
        string remindType,
        string title,
        string content)
    {
        if (CoordinateConcurrentClaims)
        {
            _concurrentClaims.Signal();
            if (!_concurrentClaims.Wait(TimeSpan.FromSeconds(3)))
                throw new InvalidOperationException("concurrent reminder claims did not align");
            _releaseConcurrentClaims.Set();
            if (!_releaseConcurrentClaims.Wait(TimeSpan.FromSeconds(3)))
                throw new InvalidOperationException("concurrent reminder claims were not released");
        }

        string key = Key(userId, item.SourceType, item.SourceId, remindType);
        lock (_claimLock)
        {
            if (AlreadySent.Contains(key) || Sent.Contains(key))
                return 0;

            if (FailNextCreate)
            {
                FailNextCreate = false;
                throw new InvalidOperationException("notification create failed");
            }

            Sent.Add(key);
            Created.Add(new CreatedNotification(
                userId,
                title,
                content,
                item.NotificationCategory,
                WorkflowConstants.NotificationType.ActionRequired,
                item.SourceType,
                item.SourceId));
            return Created.Count;
        }
    }

    public static string Key(int userId, string sourceType, int sourceId, string remindType)
    {
        return $"{userId}:{sourceType}:{sourceId}:{remindType}";
    }
}

sealed record CreatedNotification(
    int UserId,
    string Title,
    string Content,
    string Category,
    string NotificationType,
    string? SourceType,
    int? SourceId);

sealed class FakeClassroomSignalService : IClassroomSignalService
{
    private readonly object _broadcastLock = new();
    private int _startCount;
    private readonly ManualResetEventSlim _broadcastCountChanged = new();

    public int StartCount => Volatile.Read(ref _startCount);
    public List<int> BroadcastSessionIds { get; } = new();
    public bool ThrowOnStartListening { get; set; }
    public Action? OnStartListening { get; set; }

    public void StartListening()
    {
        Interlocked.Increment(ref _startCount);
        OnStartListening?.Invoke();
        if (ThrowOnStartListening)
            throw new InvalidOperationException("Start failed");
    }

    public Task BroadcastClassOpened(int sessionId)
    {
        lock (_broadcastLock)
        {
            BroadcastSessionIds.Add(sessionId);
            _broadcastCountChanged.Set();
        }

        return Task.CompletedTask;
    }

    public bool WaitForBroadcastCount(int expectedCount, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            lock (_broadcastLock)
            {
                if (BroadcastSessionIds.Count >= expectedCount)
                    return true;
            }

            TimeSpan remaining = deadline - DateTime.UtcNow;
            _broadcastCountChanged.Wait(remaining > TimeSpan.FromMilliseconds(50) ? TimeSpan.FromMilliseconds(50) : remaining);
            _broadcastCountChanged.Reset();
        }

        lock (_broadcastLock)
            return BroadcastSessionIds.Count >= expectedCount;
    }
}

sealed class TestProfilePage : ProfilePageBase
{
    public Control BuildInput(
        string labelText,
        TextBox textBox,
        bool password = false,
        bool blendWithCard = true,
        int inputWidth = 280,
        bool clearInputTag = true)
    {
        return CreateInputGroup(labelText, textBox, password, blendWithCard, inputWidth, clearInputTag);
    }

    public Control BuildMultiline(
        string labelText,
        TextBox textBox,
        bool blendWithCard = true,
        bool clearTextBoxMargin = false,
        bool clearInputTag = true)
    {
        return CreateMultilineInputGroup(labelText, textBox, blendWithCard, clearTextBoxMargin, clearInputTag);
    }

    public Control BuildCombo(string labelText, ComboBox comboBox, bool blendWithCard = true)
    {
        return CreateComboGroup(labelText, comboBox, blendWithCard);
    }
}

sealed class TestStudentGridPage : StudentGridPageBase
{
    private readonly List<string> _cachedRows = new();
    private readonly List<string> _nextRows = new();
    private bool _failNextLoad;

    public TestStudentGridPage()
        : base(
            "Test page",
            "Builds common grid chrome.",
            "Rows",
            "No rows for this test.",
            hintText: "Only test rows are shown.",
            showSearch: true,
            searchPlaceholder: "Search rows",
            showCourseFilter: true,
            showSearchButton: true)
    {
        AddHeaderAction(ExposedExtraAction);
    }

    public int CreateTableCallCount { get; private set; }
    public Button ExposedExtraAction { get; } = new() { Text = "Extra action" };
    public DataGridView ExposedGrid => Grid;
    public BindingSource ExposedBindingSource => BindingSource;
    public Button ExposedRefreshButton => RefreshButton;
    public TextBox? ExposedSearchBox => SearchBox;
    public ComboBox? ExposedCourseFilter => CourseFilter;
    public Button? ExposedSearchButton => SearchButton;
    public RoundedPanel ExposedGridBody => GridBody;
    public Label ExposedEmptyStateLabel => EmptyStateLabel;

    public Task LoadForTestAsync() => LoadDataAsync();

    public void SetNextRows(params string[] rows)
    {
        _nextRows.Clear();
        _nextRows.AddRange(rows);
    }

    public void FailNextLoad()
    {
        _failNextLoad = true;
    }

    public void RebindCachedRows()
    {
        SetGridTable(CreateTable(_cachedRows));
    }

    protected override Task<DataTable> CreateTableAsync()
    {
        CreateTableCallCount++;
        if (_failNextLoad)
        {
            _failNextLoad = false;
            throw new InvalidOperationException("Simulated refresh failure");
        }

        _cachedRows.Clear();
        _cachedRows.AddRange(_nextRows);
        return Task.FromResult(CreateTable(_cachedRows));
    }

    private static DataTable CreateTable(IEnumerable<string> rows)
    {
        DataTable table = new();
        table.Columns.Add("ID", typeof(int));
        table.Columns.Add("Name", typeof(string));
        int id = 1;
        foreach (string row in rows)
            table.Rows.Add(id++, row);
        return table;
    }
}
