using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services;
using CourseGuard.Backend.Services.Monitoring;
using CourseGuard.Backend.Services.Realtime;
using CourseGuard.Frontend.Forms.Student;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls;
using CourseGuard.Frontend.UserControls.Student;
using CourseGuard.Frontend.UserControls.Teacher;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
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

Console.WriteLine("Feature tests passed.");

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
