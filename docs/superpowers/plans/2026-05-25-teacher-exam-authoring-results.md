# Teacher Exam Authoring and Results Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the two-step teacher exam workflow, multiple-choice question authoring, persisted exam status, and improved student Results filtering/review/hide behavior.

**Architecture:** Keep the existing WinForms + controller + raw SQL repository style. Add small model classes for exam questions, student result filters, and read-only review data; keep teacher authoring in a dedicated dialog so `UC_TeacherExams` stays a table/action surface.

**Tech Stack:** .NET 10 Windows Forms, C#, Npgsql raw SQL, PostgreSQL/Supabase schema scripts.

---

## Scope Check

This plan covers one connected feature area: exam lifecycle and result visibility. It touches both teacher and student screens because persisted exam status drives both sides of the workflow.

## File Structure

- Create `CourseGuard/CourseGuard/Backend/Models/TeacherExamQuestionModel.cs`
  - Holds one multiple-choice question for teacher authoring.
- Create `CourseGuard/CourseGuard/Backend/Models/StudentResultCourseFilterModel.cs`
  - Holds one active/approved course option for the student Results dropdown.
- Create `CourseGuard/CourseGuard/Backend/Models/StudentExamReviewModel.cs`
  - Holds read-only exam review metadata and questions.
- Modify `CourseGuard/CourseGuard/Backend/Models/TeacherExamModel.cs`
  - Add persisted `Status`.
- Modify `CourseGuard/CourseGuard/Backend/Models/StudentResultListItemModel.cs`
  - Add hidden identifiers and exam status for UI actions.
- Modify `CourseGuard/CourseGuard/Backend/Models/WorkflowConstants.cs`
  - Add `ExamStatus`.
- Create `CourseGuard/CourseGuard/Backend/Database/Scripts/Migrations/20260525_exam_authoring_results.sql`
  - Adds exam status, authoring columns, and per-student hidden results table.
- Modify `CourseGuard/CourseGuard/Backend/Data/TeacherRepository.cs`
  - Add schema ensure, exam status persistence, activation validation, question CRUD, point recalculation.
- Modify `CourseGuard/CourseGuard/Backend/Controllers/TeacherController.cs`
  - Expose question CRUD and status validation methods.
- Modify `CourseGuard/CourseGuard/Backend/Data/CourseGuardDbContext.cs`
  - Filter student exams by `ACTIVE`, add student result filtering, hide result, and review data.
- Modify `CourseGuard/CourseGuard/Frontend/Forms/Teacher/TeacherExamDialog.cs`
  - Default new exams to `DRAFT`.
- Create `CourseGuard/CourseGuard/Frontend/Forms/Teacher/TeacherExamQuestionsDialog.cs`
  - Teacher multiple-choice question authoring UI.
- Create `CourseGuard/CourseGuard/Frontend/Forms/Student/StudentExamReviewForm.cs`
  - Read-only student review form.
- Modify `CourseGuard/CourseGuard/Frontend/UserControls/Teacher/UC_TeacherExams.cs`
  - Add `Soạn câu hỏi`, persist status, block invalid activation.
- Modify `CourseGuard/CourseGuard/Frontend/UserControls/Student/UC_TakeExam.cs`
  - Student list inherits backend `ACTIVE` filtering.
- Modify `CourseGuard/CourseGuard/Frontend/UserControls/Student/UC_Result.cs`
  - Add course dropdown, search, hide result, review action.
- Modify `CourseGuard/CourseGuard/Frontend/UserControls/Student/UC_Result.Designer.cs`
  - Declare `btnHideResult` if keeping designer-managed controls.

---

### Task 1: Add Domain Models and Exam Status Constants

**Files:**
- Create: `CourseGuard/CourseGuard/Backend/Models/TeacherExamQuestionModel.cs`
- Create: `CourseGuard/CourseGuard/Backend/Models/StudentResultCourseFilterModel.cs`
- Create: `CourseGuard/CourseGuard/Backend/Models/StudentExamReviewModel.cs`
- Modify: `CourseGuard/CourseGuard/Backend/Models/TeacherExamModel.cs`
- Modify: `CourseGuard/CourseGuard/Backend/Models/StudentResultListItemModel.cs`
- Modify: `CourseGuard/CourseGuard/Backend/Models/WorkflowConstants.cs`

- [ ] **Step 1: Add exam status constants**

In `CourseGuard/CourseGuard/Backend/Models/WorkflowConstants.cs`, add this nested class after `EnrollmentStatus`:

```csharp
public static class ExamStatus
{
    public const string Draft = "DRAFT";
    public const string Active = "ACTIVE";
    public const string Closed = "CLOSED";
}
```

- [ ] **Step 2: Add persisted status to teacher exam model**

In `CourseGuard/CourseGuard/Backend/Models/TeacherExamModel.cs`, replace the class body with:

```csharp
public class TeacherExamModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? OpenTime { get; set; }
    public DateTime? CloseTime { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public int QuestionCount { get; set; }
    public string Status { get; set; } = WorkflowConstants.ExamStatus.Draft;
    public string StatusText { get; set; } = WorkflowConstants.ExamStatus.Draft;
}
```

- [ ] **Step 3: Create teacher question model**

Create `CourseGuard/CourseGuard/Backend/Models/TeacherExamQuestionModel.cs`:

```csharp
namespace CourseGuard.Backend.Models
{
    public class TeacherExamQuestionModel
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = "A";
        public decimal Points { get; set; }
        public int DisplayOrder { get; set; }
    }
}
```

- [ ] **Step 4: Create student result course filter model**

Create `CourseGuard/CourseGuard/Backend/Models/StudentResultCourseFilterModel.cs`:

```csharp
namespace CourseGuard.Backend.Models
{
    public class StudentResultCourseFilterModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        public override string ToString() => CourseName;
    }
}
```

- [ ] **Step 5: Create student exam review models**

Create `CourseGuard/CourseGuard/Backend/Models/StudentExamReviewModel.cs`:

```csharp
using System.Collections.Generic;

namespace CourseGuard.Backend.Models
{
    public class StudentExamReviewModel
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public double Score { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public List<StudentExamReviewQuestionModel> Questions { get; } = new();
    }

    public class StudentExamReviewQuestionModel
    {
        public int DisplayOrder { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty;
        public decimal Points { get; set; }
    }
}
```

- [ ] **Step 6: Add action identifiers to student result item**

In `CourseGuard/CourseGuard/Backend/Models/StudentResultListItemModel.cs`, replace the class with:

```csharp
namespace CourseGuard.Backend.Models
{
    public class StudentResultListItemModel
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public int CourseId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CorrectAnswersText { get; set; } = "N/A";
        public double Score { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string ExamStatus { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 7: Build after model changes**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: build fails only if a namespace/property mismatch was introduced. Fix any compile errors in the touched model files before continuing.

- [ ] **Step 8: Commit models**

Run:

```powershell
git add CourseGuard/CourseGuard/Backend/Models/WorkflowConstants.cs CourseGuard/CourseGuard/Backend/Models/TeacherExamModel.cs CourseGuard/CourseGuard/Backend/Models/StudentResultListItemModel.cs CourseGuard/CourseGuard/Backend/Models/TeacherExamQuestionModel.cs CourseGuard/CourseGuard/Backend/Models/StudentResultCourseFilterModel.cs CourseGuard/CourseGuard/Backend/Models/StudentExamReviewModel.cs
git commit -m "feat: add exam authoring domain models"
```

---

### Task 2: Add Database Schema for Exam Status, Questions, and Hidden Results

**Files:**
- Create: `CourseGuard/CourseGuard/Backend/Database/Scripts/Migrations/20260525_exam_authoring_results.sql`
- Modify: `CourseGuard/CourseGuard/Backend/Data/TeacherRepository.cs`

- [ ] **Step 1: Create migration script**

Create `CourseGuard/CourseGuard/Backend/Database/Scripts/Migrations/20260525_exam_authoring_results.sql`:

```sql
-- 2026-05-25: exam draft authoring, MCQ questions, and per-student result hiding

ALTER TABLE exams
    ADD COLUMN IF NOT EXISTS status VARCHAR(20) NOT NULL DEFAULT 'DRAFT';

UPDATE exams
SET status = CASE
    WHEN close_time IS NOT NULL AND close_time < CURRENT_TIMESTAMP THEN 'CLOSED'
    ELSE 'DRAFT'
END
WHERE status IS NULL OR TRIM(status) = '';

CREATE TABLE IF NOT EXISTS exam_questions (
    id SERIAL PRIMARY KEY,
    exam_id INT NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
    question_text TEXT NOT NULL,
    option_a TEXT NOT NULL,
    option_b TEXT NOT NULL,
    option_c TEXT NOT NULL,
    option_d TEXT NOT NULL,
    correct_option CHAR(1) NOT NULL DEFAULT 'A',
    points NUMERIC(6,2) NOT NULL DEFAULT 0,
    display_order INT NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

ALTER TABLE exam_questions
    ADD COLUMN IF NOT EXISTS id SERIAL,
    ADD COLUMN IF NOT EXISTS question_text TEXT,
    ADD COLUMN IF NOT EXISTS option_a TEXT,
    ADD COLUMN IF NOT EXISTS option_b TEXT,
    ADD COLUMN IF NOT EXISTS option_c TEXT,
    ADD COLUMN IF NOT EXISTS option_d TEXT,
    ADD COLUMN IF NOT EXISTS correct_option CHAR(1) DEFAULT 'A',
    ADD COLUMN IF NOT EXISTS points NUMERIC(6,2) DEFAULT 0,
    ADD COLUMN IF NOT EXISTS display_order INT DEFAULT 1;

CREATE INDEX IF NOT EXISTS idx_exams_status ON exams(status);
CREATE INDEX IF NOT EXISTS idx_exam_questions_exam ON exam_questions(exam_id);
-- Do not enforce unique display_order because legacy rows may not have stable ordering yet.

CREATE TABLE IF NOT EXISTS student_hidden_results (
    student_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    attempt_id INT NOT NULL REFERENCES exam_attempts(id) ON DELETE CASCADE,
    hidden_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (student_id, attempt_id)
);

CREATE INDEX IF NOT EXISTS idx_student_hidden_results_student ON student_hidden_results(student_id);
```

- [ ] **Step 2: Extend runtime schema ensure**

In `CourseGuard/CourseGuard/Backend/Data/TeacherRepository.cs`, inside `EnsureTeacherSchema()`, after teacher assignment indexes, add:

```csharp
using (var command = new NpgsqlCommand(@"
    ALTER TABLE exams
        ADD COLUMN IF NOT EXISTS status VARCHAR(20) NOT NULL DEFAULT 'DRAFT';

    CREATE TABLE IF NOT EXISTS exam_questions (
        id SERIAL PRIMARY KEY,
        exam_id INT NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
        question_text TEXT NOT NULL,
        option_a TEXT NOT NULL,
        option_b TEXT NOT NULL,
        option_c TEXT NOT NULL,
        option_d TEXT NOT NULL,
        correct_option CHAR(1) NOT NULL DEFAULT 'A',
        points NUMERIC(6,2) NOT NULL DEFAULT 0,
        display_order INT NOT NULL DEFAULT 1,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

    ALTER TABLE exam_questions
        ADD COLUMN IF NOT EXISTS id SERIAL,
        ADD COLUMN IF NOT EXISTS question_text TEXT,
        ADD COLUMN IF NOT EXISTS option_a TEXT,
        ADD COLUMN IF NOT EXISTS option_b TEXT,
        ADD COLUMN IF NOT EXISTS option_c TEXT,
        ADD COLUMN IF NOT EXISTS option_d TEXT,
        ADD COLUMN IF NOT EXISTS correct_option CHAR(1) DEFAULT 'A',
        ADD COLUMN IF NOT EXISTS points NUMERIC(6,2) DEFAULT 0,
        ADD COLUMN IF NOT EXISTS display_order INT DEFAULT 1;

    CREATE INDEX IF NOT EXISTS idx_exams_status ON exams(status);
    CREATE INDEX IF NOT EXISTS idx_exam_questions_exam ON exam_questions(exam_id);
    -- Do not enforce unique display_order because legacy rows may not have stable ordering yet.

    CREATE TABLE IF NOT EXISTS student_hidden_results (
        student_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        attempt_id INT NOT NULL REFERENCES exam_attempts(id) ON DELETE CASCADE,
        hidden_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        PRIMARY KEY (student_id, attempt_id)
    );", connection))
    command.ExecuteNonQuery();
```

- [ ] **Step 3: Build schema changes**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit schema**

Run:

```powershell
git add CourseGuard/CourseGuard/Backend/Database/Scripts/Migrations/20260525_exam_authoring_results.sql CourseGuard/CourseGuard/Backend/Data/TeacherRepository.cs
git commit -m "feat: add exam authoring schema"
```

---

### Task 3: Persist Teacher Exam Status and Activation Rules

**Files:**
- Modify: `CourseGuard/CourseGuard/Backend/Data/TeacherRepository.cs`
- Modify: `CourseGuard/CourseGuard/Backend/Controllers/TeacherController.cs`
- Modify: `CourseGuard/CourseGuard/Frontend/Forms/Teacher/TeacherExamDialog.cs`
- Modify: `CourseGuard/CourseGuard/Frontend/UserControls/Teacher/UC_TeacherExams.cs`

- [ ] **Step 1: Update create/update SQL for exams**

In `TeacherRepository.CreateExam`, replace the SQL with:

```csharp
@"INSERT INTO exams (course_id, title, open_time, close_time, duration_minutes, max_attempts, created_by, status)
  SELECT @course_id, @title, @open_time, @close_time, @duration_minutes, @max_attempts, @teacher_id, @status
  WHERE EXISTS (SELECT 1 FROM courses WHERE id = @course_id AND teacher_id = @teacher_id)
  RETURNING id"
```

In `TeacherRepository.UpdateExam`, replace the SQL with:

```csharp
@"UPDATE exams ex
  SET title = @title, open_time = @open_time, close_time = @close_time,
      duration_minutes = @duration_minutes, max_attempts = @max_attempts,
      status = @status
  FROM courses c
  WHERE ex.course_id = c.id AND c.teacher_id = @teacher_id AND ex.id = @id AND ex.course_id = @course_id"
```

- [ ] **Step 2: Replace AddExamParameters**

Replace `AddExamParameters` with:

```csharp
private static void AddExamParameters(NpgsqlCommand command, TeacherExamModel input, bool includeId)
{
    if (includeId)
        command.Parameters.AddWithValue("@id", input.Id);

    command.Parameters.AddWithValue("@title", input.Title);
    command.Parameters.AddWithValue("@open_time", input.OpenTime.HasValue ? input.OpenTime.Value : DBNull.Value);
    command.Parameters.AddWithValue("@close_time", input.CloseTime.HasValue ? input.CloseTime.Value : DBNull.Value);
    command.Parameters.AddWithValue("@duration_minutes", input.DurationMinutes);
    command.Parameters.AddWithValue("@max_attempts", input.MaxAttempts <= 0 ? 1 : input.MaxAttempts);
    command.Parameters.AddWithValue("@status", NormalizeExamStatus(input.Status));
}

private static string NormalizeExamStatus(string? status)
{
    string value = (status ?? string.Empty).Trim().ToUpperInvariant();
    return value switch
    {
        WorkflowConstants.ExamStatus.Active => WorkflowConstants.ExamStatus.Active,
        WorkflowConstants.ExamStatus.Closed => WorkflowConstants.ExamStatus.Closed,
        _ => WorkflowConstants.ExamStatus.Draft
    };
}
```

- [ ] **Step 3: Replace QueryExams status projection**

In `QueryExams`, change the select list to:

```sql
SELECT ex.id, ex.course_id, COALESCE(c.name, ''), COALESCE(ex.title, ''),
       ex.open_time, ex.close_time, COALESCE(ex.duration_minutes, 0),
       COALESCE(ex.max_attempts, 1), COUNT(eq.id)::int,
       COALESCE(ex.status, 'DRAFT')
```

Change `StatusText = BuildExamStatus(open, close)` to:

```csharp
Status = reader.GetString(9),
StatusText = reader.GetString(9)
```

- [ ] **Step 4: Add activation validation repository method**

Add this public method near `UpdateExam`:

```csharp
public bool CanActivateExam(int teacherId, int examId)
{
    using var connection = _dbContext.CreateConnection();
    connection.Open();
    using var command = new NpgsqlCommand(@"
        SELECT COUNT(eq.id)
        FROM exams ex
        JOIN courses c ON c.id = ex.course_id
        LEFT JOIN exam_questions eq ON eq.exam_id = ex.id
        WHERE c.teacher_id = @teacher_id
          AND ex.id = @exam_id", connection);
    command.Parameters.AddWithValue("@teacher_id", teacherId);
    command.Parameters.AddWithValue("@exam_id", examId);
    return Convert.ToInt32(command.ExecuteScalar()) > 0;
}
```

- [ ] **Step 5: Expose activation validation in controller**

In `TeacherController`, add:

```csharp
public bool CanActivateExam(int teacherId, int examId) =>
    teacherId > 0 && examId > 0 && _repository.CanActivateExam(teacherId, examId);
```

- [ ] **Step 6: Default new exam dialog to draft**

In `TeacherExamDialog.cs`, replace the constructor with:

```csharp
public TeacherExamDialog(IEnumerable<TeacherCourseModel> courses)
    : base("Bài kiểm tra", courses, status: WorkflowConstants.ExamStatus.Draft)
{
}
```

Add `using CourseGuard.Backend.Models;` if the file does not already have it.

- [ ] **Step 7: Persist status from teacher exam UI**

In `UC_TeacherExams.AddAsync`, create the model with:

```csharp
new TeacherExamModel
{
    CourseId = dialog.CourseId,
    Title = dialog.ItemTitle,
    OpenTime = dialog.SelectedDate,
    CloseTime = dialog.SelectedDate.AddHours(1),
    DurationMinutes = 60,
    MaxAttempts = 1,
    Status = WorkflowConstants.ExamStatus.Draft
}
```

In `UC_TeacherExams.EditAsync`, pass the current status into the dialog:

```csharp
using var dialog = new TeacherSimpleItemDialog(
    "Sửa bài kiểm tra",
    Controller.GetCourses(TeacherId),
    CurrentString("Tên kỳ thi"),
    string.Empty,
    CurrentString("Trạng thái"));
```

Before `Controller.UpdateExam`, add:

```csharp
if (dialog.Status == WorkflowConstants.ExamStatus.Active && !Controller.CanActivateExam(TeacherId, id))
{
    MetaTheme.ShowModernDialog(
        "Bài kiểm tra cần có ít nhất 1 câu hỏi trước khi kích hoạt.",
        "Chưa thể kích hoạt",
        System.Windows.Forms.MessageBoxButtons.OK,
        System.Windows.Forms.MessageBoxIcon.Warning);
    return;
}
```

Then set `Status = dialog.Status` in the update model.

- [ ] **Step 8: Build status flow**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 9: Commit status flow**

Run:

```powershell
git add CourseGuard/CourseGuard/Backend/Data/TeacherRepository.cs CourseGuard/CourseGuard/Backend/Controllers/TeacherController.cs CourseGuard/CourseGuard/Frontend/Forms/Teacher/TeacherExamDialog.cs CourseGuard/CourseGuard/Frontend/UserControls/Teacher/UC_TeacherExams.cs
git commit -m "feat: persist teacher exam status"
```

---

### Task 4: Add Teacher Question CRUD Backend

**Files:**
- Modify: `CourseGuard/CourseGuard/Backend/Data/TeacherRepository.cs`
- Modify: `CourseGuard/CourseGuard/Backend/Controllers/TeacherController.cs`

- [ ] **Step 1: Add question query methods to repository**

Add these public methods near existing exam methods:

```csharp
public List<TeacherExamQuestionModel> GetExamQuestions(int teacherId, int examId)
{
    var rows = new List<TeacherExamQuestionModel>();
    using var connection = _dbContext.CreateConnection();
    connection.Open();
    using var command = new NpgsqlCommand(@"
        SELECT eq.id, eq.exam_id, COALESCE(eq.question_text, ''),
               COALESCE(eq.option_a, ''), COALESCE(eq.option_b, ''),
               COALESCE(eq.option_c, ''), COALESCE(eq.option_d, ''),
               COALESCE(eq.correct_option, 'A'), COALESCE(eq.points, 0),
               COALESCE(eq.display_order, 1)
        FROM exam_questions eq
        JOIN exams ex ON ex.id = eq.exam_id
        JOIN courses c ON c.id = ex.course_id
        WHERE c.teacher_id = @teacher_id
          AND eq.exam_id = @exam_id
        ORDER BY eq.display_order, eq.id", connection);
    command.Parameters.AddWithValue("@teacher_id", teacherId);
    command.Parameters.AddWithValue("@exam_id", examId);
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        rows.Add(new TeacherExamQuestionModel
        {
            Id = reader.GetInt32(0),
            ExamId = reader.GetInt32(1),
            QuestionText = reader.GetString(2),
            OptionA = reader.GetString(3),
            OptionB = reader.GetString(4),
            OptionC = reader.GetString(5),
            OptionD = reader.GetString(6),
            CorrectOption = reader.GetString(7),
            Points = reader.GetDecimal(8),
            DisplayOrder = reader.GetInt32(9)
        });
    }
    return rows;
}

public string GetExamStatus(int teacherId, int examId)
{
    using var connection = _dbContext.CreateConnection();
    connection.Open();
    using var command = new NpgsqlCommand(@"
        SELECT COALESCE(ex.status, 'DRAFT')
        FROM exams ex
        JOIN courses c ON c.id = ex.course_id
        WHERE c.teacher_id = @teacher_id
          AND ex.id = @exam_id", connection);
    command.Parameters.AddWithValue("@teacher_id", teacherId);
    command.Parameters.AddWithValue("@exam_id", examId);
    return command.ExecuteScalar()?.ToString() ?? WorkflowConstants.ExamStatus.Draft;
}
```

- [ ] **Step 2: Add create/update/delete question methods**

Add:

```csharp
public int CreateExamQuestion(int teacherId, TeacherExamQuestionModel input)
{
    using var connection = _dbContext.CreateConnection();
    connection.Open();
    using var transaction = connection.BeginTransaction();
    int nextOrder = GetNextQuestionOrder(connection, transaction, teacherId, input.ExamId);
    using var command = new NpgsqlCommand(@"
        INSERT INTO exam_questions
            (exam_id, question_text, option_a, option_b, option_c, option_d, correct_option, points, display_order)
        SELECT @exam_id, @question_text, @option_a, @option_b, @option_c, @option_d, @correct_option, 0, @display_order
        WHERE EXISTS (
            SELECT 1 FROM exams ex
            JOIN courses c ON c.id = ex.course_id
            WHERE ex.id = @exam_id
              AND c.teacher_id = @teacher_id
              AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'DRAFT'
        )
        RETURNING id", connection, transaction);
    AddQuestionParameters(command, teacherId, input, nextOrder);
    object? result = command.ExecuteScalar();
    int id = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    RecalculateQuestionPoints(connection, transaction, input.ExamId);
    transaction.Commit();
    return id;
}

public bool UpdateExamQuestion(int teacherId, TeacherExamQuestionModel input)
{
    using var connection = _dbContext.CreateConnection();
    connection.Open();
    using var command = new NpgsqlCommand(@"
        UPDATE exam_questions eq
        SET question_text = @question_text,
            option_a = @option_a,
            option_b = @option_b,
            option_c = @option_c,
            option_d = @option_d,
            correct_option = @correct_option
        FROM exams ex
        JOIN courses c ON c.id = ex.course_id
        WHERE eq.exam_id = ex.id
          AND c.teacher_id = @teacher_id
          AND eq.id = @id
          AND eq.exam_id = @exam_id
          AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'DRAFT'", connection);
    command.Parameters.AddWithValue("@id", input.Id);
    AddQuestionParameters(command, teacherId, input, input.DisplayOrder <= 0 ? 1 : input.DisplayOrder);
    return command.ExecuteNonQuery() > 0;
}

public bool DeleteExamQuestion(int teacherId, int examId, int questionId)
{
    using var connection = _dbContext.CreateConnection();
    connection.Open();
    using var transaction = connection.BeginTransaction();
    using var command = new NpgsqlCommand(@"
        DELETE FROM exam_questions eq
        USING exams ex, courses c
        WHERE eq.exam_id = ex.id
          AND c.id = ex.course_id
          AND c.teacher_id = @teacher_id
          AND eq.exam_id = @exam_id
          AND eq.id = @id
          AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'DRAFT'", connection, transaction);
    command.Parameters.AddWithValue("@teacher_id", teacherId);
    command.Parameters.AddWithValue("@exam_id", examId);
    command.Parameters.AddWithValue("@id", questionId);
    bool deleted = command.ExecuteNonQuery() > 0;
    if (deleted)
        RecalculateQuestionPoints(connection, transaction, examId);
    transaction.Commit();
    return deleted;
}
```

- [ ] **Step 3: Add question helper methods**

Add these private methods near other private helpers:

```csharp
private static void AddQuestionParameters(NpgsqlCommand command, int teacherId, TeacherExamQuestionModel input, int displayOrder)
{
    command.Parameters.AddWithValue("@teacher_id", teacherId);
    command.Parameters.AddWithValue("@exam_id", input.ExamId);
    command.Parameters.AddWithValue("@question_text", input.QuestionText.Trim());
    command.Parameters.AddWithValue("@option_a", input.OptionA.Trim());
    command.Parameters.AddWithValue("@option_b", input.OptionB.Trim());
    command.Parameters.AddWithValue("@option_c", input.OptionC.Trim());
    command.Parameters.AddWithValue("@option_d", input.OptionD.Trim());
    command.Parameters.AddWithValue("@correct_option", NormalizeCorrectOption(input.CorrectOption));
    command.Parameters.AddWithValue("@display_order", displayOrder);
}

private static string NormalizeCorrectOption(string? value)
{
    string option = (value ?? "A").Trim().ToUpperInvariant();
    return option is "A" or "B" or "C" or "D" ? option : "A";
}

private static int GetNextQuestionOrder(NpgsqlConnection connection, NpgsqlTransaction transaction, int teacherId, int examId)
{
    using var command = new NpgsqlCommand(@"
        SELECT COALESCE(MAX(eq.display_order), 0) + 1
        FROM exams ex
        JOIN courses c ON c.id = ex.course_id
        LEFT JOIN exam_questions eq ON eq.exam_id = ex.id
        WHERE c.teacher_id = @teacher_id
          AND ex.id = @exam_id", connection, transaction);
    command.Parameters.AddWithValue("@teacher_id", teacherId);
    command.Parameters.AddWithValue("@exam_id", examId);
    return Convert.ToInt32(command.ExecuteScalar());
}

private static void RecalculateQuestionPoints(NpgsqlConnection connection, NpgsqlTransaction transaction, int examId)
{
    using var countCommand = new NpgsqlCommand("SELECT COUNT(*) FROM exam_questions WHERE exam_id = @exam_id", connection, transaction);
    countCommand.Parameters.AddWithValue("@exam_id", examId);
    int count = Convert.ToInt32(countCommand.ExecuteScalar());
    if (count <= 0)
        return;

    decimal points = Math.Round(10m / count, 2, MidpointRounding.AwayFromZero);
    using var updateCommand = new NpgsqlCommand("UPDATE exam_questions SET points = @points WHERE exam_id = @exam_id", connection, transaction);
    updateCommand.Parameters.AddWithValue("@points", points);
    updateCommand.Parameters.AddWithValue("@exam_id", examId);
    updateCommand.ExecuteNonQuery();
}
```

- [ ] **Step 4: Expose question methods in controller**

Add to `TeacherController`:

```csharp
public List<TeacherExamQuestionModel> GetExamQuestions(int teacherId, int examId) =>
    teacherId <= 0 || examId <= 0 ? new List<TeacherExamQuestionModel>() : _repository.GetExamQuestions(teacherId, examId);

public string GetExamStatus(int teacherId, int examId) =>
    teacherId <= 0 || examId <= 0 ? WorkflowConstants.ExamStatus.Draft : _repository.GetExamStatus(teacherId, examId);

public int CreateExamQuestion(int teacherId, TeacherExamQuestionModel input) =>
    teacherId <= 0 ? 0 : _repository.CreateExamQuestion(teacherId, input);

public bool UpdateExamQuestion(int teacherId, TeacherExamQuestionModel input) =>
    teacherId > 0 && _repository.UpdateExamQuestion(teacherId, input);

public bool DeleteExamQuestion(int teacherId, int examId, int questionId) =>
    teacherId > 0 && _repository.DeleteExamQuestion(teacherId, examId, questionId);
```

- [ ] **Step 5: Build question backend**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 6: Commit question backend**

Run:

```powershell
git add CourseGuard/CourseGuard/Backend/Data/TeacherRepository.cs CourseGuard/CourseGuard/Backend/Controllers/TeacherController.cs
git commit -m "feat: add teacher exam question backend"
```

---

### Task 5: Add Teacher Question Authoring Dialog

**Files:**
- Create: `CourseGuard/CourseGuard/Frontend/Forms/Teacher/TeacherExamQuestionsDialog.cs`
- Modify: `CourseGuard/CourseGuard/Frontend/UserControls/Teacher/UC_TeacherExams.cs`

- [ ] **Step 1: Create question authoring dialog**

Create `TeacherExamQuestionsDialog.cs` with this implementation:

```csharp
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public class TeacherExamQuestionsDialog : Form
    {
        private readonly int _teacherId;
        private readonly int _examId;
        private readonly TeacherController _controller = new(new CourseGuardDbContext(""));
        private readonly DataGridView _grid = new();
        private readonly TextBox _question = new();
        private readonly TextBox _a = new();
        private readonly TextBox _b = new();
        private readonly TextBox _c = new();
        private readonly TextBox _d = new();
        private readonly ComboBox _correct = new();
        private readonly Label _points = new();
        private readonly Button _add = TeacherTabChrome.PrimaryButton("Thêm câu");
        private readonly Button _save = TeacherTabChrome.SecondaryButton("Lưu sửa");
        private readonly Button _delete = TeacherTabChrome.DangerButton("Xóa câu");
        private readonly Button _close = TeacherTabChrome.SecondaryButton("Đóng");
        private readonly bool _canEdit;

        public TeacherExamQuestionsDialog(int teacherId, int examId, string examTitle)
        {
            _teacherId = teacherId;
            _examId = examId;
            _canEdit = string.Equals(_controller.GetExamStatus(teacherId, examId), WorkflowConstants.ExamStatus.Draft, StringComparison.OrdinalIgnoreCase);
            Text = $"Soạn câu hỏi - {examTitle}";
            Width = 980;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            WireEvents();
            LoadQuestions();
            AppColors.ApplyTheme(this);
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(16), BackColor = AppColors.BgBase };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

            TeacherTabChrome.StyleGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            root.Controls.Add(_grid, 0, 0);

            var editor = TeacherCourseDialog.CreateGrid();
            editor.RowCount = 8;
            editor.RowStyles.Clear();
            editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++)
                editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            _question.Multiline = true;
            _correct.DropDownStyle = ComboBoxStyle.DropDownList;
            _correct.Items.AddRange(new object[] { "A", "B", "C", "D" });
            _correct.SelectedIndex = 0;
            _points.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            AddRow(editor, 0, "Câu hỏi", _question);
            AddRow(editor, 1, "A", _a);
            AddRow(editor, 2, "B", _b);
            AddRow(editor, 3, "C", _c);
            AddRow(editor, 4, "D", _d);
            AddRow(editor, 5, "Đáp án đúng", _correct);
            AddRow(editor, 6, "Điểm/câu", _points);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            buttons.Controls.Add(_close);
            buttons.Controls.Add(_delete);
            buttons.Controls.Add(_save);
            buttons.Controls.Add(_add);
            editor.Controls.Add(buttons, 0, 7);
            editor.SetColumnSpan(buttons, 2);
            root.Controls.Add(editor, 1, 0);
            Controls.Add(root);
        }

        private static void AddRow(TableLayoutPanel grid, int row, string label, Control control)
        {
            grid.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, row);
            control.Dock = DockStyle.Fill;
            grid.Controls.Add(control, 1, row);
        }

        private void WireEvents()
        {
            _grid.SelectionChanged += (_, _) => LoadSelectedQuestion();
            _add.Click += (_, _) => SaveNewQuestion();
            _save.Click += (_, _) => SaveExistingQuestion();
            _delete.Click += (_, _) => DeleteSelectedQuestion();
            _close.Click += (_, _) => Close();
            _add.Enabled = _save.Enabled = _delete.Enabled = _canEdit;
        }

        private void LoadQuestions()
        {
            DataTable table = new();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("STT", typeof(int));
            table.Columns.Add("Câu hỏi", typeof(string));
            table.Columns.Add("Đáp án", typeof(string));
            table.Columns.Add("Điểm", typeof(string));

            var questions = _controller.GetExamQuestions(_teacherId, _examId);
            foreach (var q in questions)
                table.Rows.Add(q.Id, q.DisplayOrder, q.QuestionText, q.CorrectOption, q.Points.ToString("0.##", CultureInfo.InvariantCulture));

            _grid.DataSource = table;
            if (_grid.Columns["Id"] != null)
                _grid.Columns["Id"]!.Visible = false;
            _points.Text = questions.Count == 0 ? "0" : (10m / questions.Count).ToString("0.##", CultureInfo.InvariantCulture);
            ClearEditor();
        }

        private void LoadSelectedQuestion()
        {
            int id = CurrentQuestionId();
            if (id <= 0)
                return;
            var item = _controller.GetExamQuestions(_teacherId, _examId).FirstOrDefault(q => q.Id == id);
            if (item == null)
                return;
            _question.Text = item.QuestionText;
            _a.Text = item.OptionA;
            _b.Text = item.OptionB;
            _c.Text = item.OptionC;
            _d.Text = item.OptionD;
            _correct.SelectedItem = item.CorrectOption;
        }

        private void SaveNewQuestion()
        {
            if (!TryBuildQuestion(out var model))
                return;
            _controller.CreateExamQuestion(_teacherId, model);
            LoadQuestions();
        }

        private void SaveExistingQuestion()
        {
            int id = CurrentQuestionId();
            if (id <= 0 || !TryBuildQuestion(out var model))
                return;
            model.Id = id;
            _controller.UpdateExamQuestion(_teacherId, model);
            LoadQuestions();
        }

        private void DeleteSelectedQuestion()
        {
            int id = CurrentQuestionId();
            if (id <= 0)
                return;
            if (MetaTheme.ShowModernDialog("Ẩn câu hỏi này khỏi bài kiểm tra?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _controller.DeleteExamQuestion(_teacherId, _examId, id);
            LoadQuestions();
        }

        private bool TryBuildQuestion(out TeacherExamQuestionModel model)
        {
            model = new TeacherExamQuestionModel();
            if (string.IsNullOrWhiteSpace(_question.Text) || string.IsNullOrWhiteSpace(_a.Text) || string.IsNullOrWhiteSpace(_b.Text) || string.IsNullOrWhiteSpace(_c.Text) || string.IsNullOrWhiteSpace(_d.Text))
            {
                MetaTheme.ShowModernDialog("Vui lòng nhập câu hỏi và đủ 4 lựa chọn.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            model.ExamId = _examId;
            model.QuestionText = _question.Text;
            model.OptionA = _a.Text;
            model.OptionB = _b.Text;
            model.OptionC = _c.Text;
            model.OptionD = _d.Text;
            model.CorrectOption = _correct.SelectedItem?.ToString() ?? "A";
            return true;
        }

        private int CurrentQuestionId()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow)
                return 0;
            return Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        }

        private void ClearEditor()
        {
            _question.Clear();
            _a.Clear();
            _b.Clear();
            _c.Clear();
            _d.Clear();
            _correct.SelectedIndex = 0;
        }
    }
}
```

- [ ] **Step 2: Add button to Teacher Exams page**

In `UC_TeacherExams`, add using:

```csharp
using CourseGuard.Frontend.Theme;
```

Add a constructor body:

```csharp
public UC_TeacherExams(int teacherId) : base(teacherId, "Bài kiểm tra", "Tạo và quản lý kỳ thi. Giám sát nằm ở tab riêng.", "Danh sách bài kiểm tra")
{
    var questionsButton = TeacherTabChrome.SecondaryButton("Soạn câu hỏi");
    questionsButton.Click += async (_, _) => await EditQuestionsAsync();
    AddHeaderAction(questionsButton);
}
```

Add method:

```csharp
private async Task EditQuestionsAsync()
{
    int id = CurrentInt("Id");
    if (id <= 0)
    {
        MetaTheme.ShowModernDialog("Vui lòng chọn một bài kiểm tra.", "Thông báo");
        return;
    }

    using var dialog = new TeacherExamQuestionsDialog(TeacherId, id, CurrentString("Tên kỳ thi"));
    dialog.ShowDialog(FindForm());
    await LoadDataAsync();
}
```

- [ ] **Step 3: Build teacher question UI**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit teacher question UI**

Run:

```powershell
git add CourseGuard/CourseGuard/Frontend/Forms/Teacher/TeacherExamQuestionsDialog.cs CourseGuard/CourseGuard/Frontend/UserControls/Teacher/UC_TeacherExams.cs
git commit -m "feat: add teacher exam question authoring"
```

---

### Task 6: Filter Student Exam Availability by Active Status

**Files:**
- Modify: `CourseGuard/CourseGuard/Backend/Data/CourseGuardDbContext.cs`

- [ ] **Step 1: Update student available exams query**

In `GetAvailableExamsForStudent`, add this condition to the `WHERE` clause:

```sql
AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'ACTIVE'
```

Keep existing enrollment, course, close time, and attempt-limit filters.

- [ ] **Step 2: Update student exam count query**

In `CountStudentExamsByAvailability`, add the same condition to the `WHERE` clause:

```sql
AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'ACTIVE'
```

- [ ] **Step 3: Update global search exam query**

In the exam section of `SearchStudentContent`, add:

```sql
AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'ACTIVE'
```

- [ ] **Step 4: Build availability filtering**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit student availability**

Run:

```powershell
git add CourseGuard/CourseGuard/Backend/Data/CourseGuardDbContext.cs
git commit -m "feat: filter student exams by active status"
```

---

### Task 7: Add Student Results Backend Filtering, Hide, and Review

**Files:**
- Modify: `CourseGuard/CourseGuard/Backend/Data/CourseGuardDbContext.cs`

- [ ] **Step 1: Add active enrolled course options**

Add public method:

```csharp
public List<StudentResultCourseFilterModel> GetActiveResultCourseFiltersForStudent(int studentId)
{
    var result = new List<StudentResultCourseFilterModel>();
    using var connection = CreateConnection();
    connection.Open();
    if (!TableExists(connection, "courses") || !TableExists(connection, "enrollments"))
        return result;

    using var command = new NpgsqlCommand(@"
        SELECT DISTINCT c.id, COALESCE(c.name, '')
        FROM courses c
        JOIN enrollments e ON e.course_id = c.id
        WHERE e.student_id = @student_id
          AND UPPER(COALESCE(e.status, '')) IN ('ACTIVE', 'APPROVED')
        ORDER BY COALESCE(c.name, '')", connection);
    command.Parameters.AddWithValue("@student_id", studentId);
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        result.Add(new StudentResultCourseFilterModel
        {
            CourseId = reader.GetInt32(0),
            CourseName = reader.GetString(1)
        });
    }
    return result;
}
```

- [ ] **Step 2: Replace GetStudentResultItems signature and query**

Change signature to:

```csharp
public List<StudentResultListItemModel> GetStudentResultItems(int studentId, int? courseId = null, string? examTitleKeyword = null, int limit = 100)
```

Inside the `exam_attempts` branch, use this query:

```csharp
string query = $@"
    SELECT a.id AS attempt_id,
           ex.id AS exam_id,
           ex.course_id,
           COALESCE(ex.title, '') AS exam_title,
           COALESCE(c.name, '') AS course_name,
           COALESCE(a.score, 0)::float8 AS score,
           COALESCE(a.status, '') AS attempt_status,
           {questionExpression}::int AS question_count,
           COALESCE(ex.status, '') AS exam_status
    FROM exam_attempts a
    JOIN exams ex ON ex.id = a.exam_id
    JOIN enrollments en ON en.course_id = ex.course_id AND en.student_id = a.student_id
    LEFT JOIN courses c ON c.id = ex.course_id
    LEFT JOIN student_hidden_results shr ON shr.attempt_id = a.id AND shr.student_id = a.student_id
    WHERE a.student_id = @student_id
      AND a.score IS NOT NULL
      AND shr.attempt_id IS NULL
      AND UPPER(COALESCE(en.status, '')) IN ('ACTIVE', 'APPROVED')";
if (courseId.HasValue && courseId.Value > 0)
    query += " AND ex.course_id = @course_id";
if (!string.IsNullOrWhiteSpace(examTitleKeyword))
    query += " AND COALESCE(ex.title, '') ILIKE @keyword";
query += " ORDER BY COALESCE(a.submit_time, a.start_time) DESC LIMIT @limit";
```

Add parameters:

```csharp
command.Parameters.AddWithValue("@student_id", studentId);
command.Parameters.AddWithValue("@limit", safeLimit);
if (courseId.HasValue && courseId.Value > 0)
    command.Parameters.AddWithValue("@course_id", courseId.Value);
if (!string.IsNullOrWhiteSpace(examTitleKeyword))
    command.Parameters.AddWithValue("@keyword", $"%{examTitleKeyword.Trim()}%");
```

Populate:

```csharp
AttemptId = reader.GetInt32(0),
ExamId = reader.GetInt32(1),
CourseId = reader.GetInt32(2),
ExamTitle = reader.GetString(3),
CourseName = reader.GetString(4),
CorrectAnswersText = questionCount > 0 ? $"N/A/{questionCount}" : "N/A",
Score = score,
StatusText = BuildResultStatus(score, reader.GetString(6)),
ExamStatus = reader.GetString(8)
```

- [ ] **Step 3: Add hide result method**

Add:

```csharp
public bool HideStudentResult(int studentId, int attemptId)
{
    using var connection = CreateConnection();
    connection.Open();
    if (!TableExists(connection, "student_hidden_results"))
        return false;

    using var command = new NpgsqlCommand(@"
        INSERT INTO student_hidden_results (student_id, attempt_id)
        SELECT @student_id, @attempt_id
        WHERE EXISTS (
            SELECT 1 FROM exam_attempts
            WHERE id = @attempt_id AND student_id = @student_id
        )
        ON CONFLICT (student_id, attempt_id) DO NOTHING", connection);
    command.Parameters.AddWithValue("@student_id", studentId);
    command.Parameters.AddWithValue("@attempt_id", attemptId);
    return command.ExecuteNonQuery() > 0;
}
```

- [ ] **Step 4: Add read-only review method**

Add:

```csharp
public StudentExamReviewModel? GetStudentExamReview(int studentId, int attemptId)
{
    using var connection = CreateConnection();
    connection.Open();
    if (!TableExists(connection, "exam_attempts") || !TableExists(connection, "exams"))
        return null;

    using var headerCommand = new NpgsqlCommand(@"
        SELECT a.id, ex.id, COALESCE(ex.title, ''), COALESCE(c.name, ''),
               COALESCE(a.score, 0)::float8, COALESCE(a.status, ''),
               COALESCE(ex.status, '')
        FROM exam_attempts a
        JOIN exams ex ON ex.id = a.exam_id
        LEFT JOIN courses c ON c.id = ex.course_id
        WHERE a.student_id = @student_id
          AND a.id = @attempt_id
          AND UPPER(COALESCE(ex.status, '')) = 'CLOSED'", connection);
    headerCommand.Parameters.AddWithValue("@student_id", studentId);
    headerCommand.Parameters.AddWithValue("@attempt_id", attemptId);
    using var reader = headerCommand.ExecuteReader();
    if (!reader.Read())
        return null;
    double score = reader.GetDouble(4);
    var review = new StudentExamReviewModel
    {
        AttemptId = reader.GetInt32(0),
        ExamId = reader.GetInt32(1),
        ExamTitle = reader.GetString(2),
        CourseName = reader.GetString(3),
        Score = score,
        StatusText = BuildResultStatus(score, reader.GetString(5))
    };
    reader.Close();

    if (!TableExists(connection, "exam_questions"))
        return review;

    using var questionCommand = new NpgsqlCommand(@"
        SELECT COALESCE(display_order, 1), COALESCE(question_text, ''),
               COALESCE(option_a, ''), COALESCE(option_b, ''),
               COALESCE(option_c, ''), COALESCE(option_d, ''),
               COALESCE(correct_option, ''), COALESCE(points, 0)
        FROM exam_questions
        WHERE exam_id = @exam_id
        ORDER BY display_order, id", connection);
    questionCommand.Parameters.AddWithValue("@exam_id", review.ExamId);
    using var qReader = questionCommand.ExecuteReader();
    while (qReader.Read())
    {
        review.Questions.Add(new StudentExamReviewQuestionModel
        {
            DisplayOrder = qReader.GetInt32(0),
            QuestionText = qReader.GetString(1),
            OptionA = qReader.GetString(2),
            OptionB = qReader.GetString(3),
            OptionC = qReader.GetString(4),
            OptionD = qReader.GetString(5),
            CorrectOption = qReader.GetString(6),
            Points = qReader.GetDecimal(7)
        });
    }

    return review;
}
```

- [ ] **Step 5: Build results backend**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 6: Commit results backend**

Run:

```powershell
git add CourseGuard/CourseGuard/Backend/Data/CourseGuardDbContext.cs
git commit -m "feat: add student result filtering and review data"
```

---

### Task 8: Add Student Exam Review Form

**Files:**
- Create: `CourseGuard/CourseGuard/Frontend/Forms/Student/StudentExamReviewForm.cs`

- [ ] **Step 1: Create read-only review form**

Create `StudentExamReviewForm.cs`:

```csharp
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public class StudentExamReviewForm : Form
    {
        public StudentExamReviewForm(StudentExamReviewModel review)
        {
            Text = "Xem lại bài";
            Width = 820;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;

            var text = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = AppFonts.Body,
                Text = BuildReviewText(review)
            };

            Controls.Add(text);
            AppColors.ApplyTheme(this);
        }

        private static string BuildReviewText(StudentExamReviewModel review)
        {
            var sb = new StringBuilder();
            sb.AppendLine(review.ExamTitle);
            sb.AppendLine(review.CourseName);
            sb.AppendLine($"Điểm: {review.Score.ToString("0.0", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Trạng thái: {review.StatusText}");
            sb.AppendLine();

            if (review.Questions.Count == 0)
            {
                sb.AppendLine("Bài thi này chưa có dữ liệu câu hỏi để xem lại.");
                return sb.ToString();
            }

            foreach (var q in review.Questions)
            {
                sb.AppendLine($"Câu {q.DisplayOrder} ({q.Points.ToString("0.##", CultureInfo.InvariantCulture)} điểm)");
                sb.AppendLine(q.QuestionText);
                sb.AppendLine($"A. {q.OptionA}");
                sb.AppendLine($"B. {q.OptionB}");
                sb.AppendLine($"C. {q.OptionC}");
                sb.AppendLine($"D. {q.OptionD}");
                sb.AppendLine($"Đáp án đúng: {q.CorrectOption}");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
```

- [ ] **Step 2: Build review form**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit review form**

Run:

```powershell
git add CourseGuard/CourseGuard/Frontend/Forms/Student/StudentExamReviewForm.cs
git commit -m "feat: add student exam review form"
```

---

### Task 9: Add Student Results UI Filter, Search, Hide, and Review Actions

**Files:**
- Modify: `CourseGuard/CourseGuard/Frontend/UserControls/Student/UC_Result.cs`
- Modify: `CourseGuard/CourseGuard/Frontend/UserControls/Student/UC_Result.Designer.cs`

- [ ] **Step 1: Add fields to UC_Result**

At the top of `UC_Result`, add:

```csharp
private readonly ComboBox _courseFilter = new();
private readonly TextBox _searchBox = new();
private readonly Button _hideResult = StudentTabChrome.SecondaryButton("Ẩn khỏi Kết quả");
private readonly BindingSource _bindingSource = new();
```

- [ ] **Step 2: Update constructor actions**

In constructor, after `RoundedButtonHelper.Apply(btnReview, 10);`, add:

```csharp
RoundedButtonHelper.Apply(_hideResult, 10);
btnReview.Click += (_, _) => ReviewSelectedResult();
_hideResult.Click += async (_, _) => await HideSelectedResultAsync();
_courseFilter.SelectedIndexChanged += async (_, _) => await LoadDataAsync();
_searchBox.TextChanged += async (_, _) => await LoadDataAsync();
```

- [ ] **Step 3: Build toolbar in card layout**

In `BuildCardLayout`, replace header call with:

```csharp
var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
_courseFilter.Width = 220;
_searchBox.Width = 240;
_searchBox.PlaceholderText = "Tìm theo tên bài thi";
actions.Controls.Add(_courseFilter);
actions.Controls.Add(_searchBox);
actions.Controls.Add(btnReview);
actions.Controls.Add(_hideResult);

root.Controls.Add(StudentTabChrome.CreateHeader(
    "Kết quả học tập",
    "Xem điểm, trạng thái chấm và mở lại bài làm khi được phép.",
    actions), 0, 0);
```

- [ ] **Step 4: Load course dropdown on the UI thread**

Add method:

```csharp
private async System.Threading.Tasks.Task EnsureCourseFilterLoadedAsync(int studentId)
{
    if (_courseFilter.Items.Count > 0)
        return;

    var courses = await System.Threading.Tasks.Task.Run(() => _dbContext.GetActiveResultCourseFiltersForStudent(studentId));

    _courseFilter.DisplayMember = nameof(StudentResultCourseFilterModel.CourseName);
    _courseFilter.ValueMember = nameof(StudentResultCourseFilterModel.CourseId);
    _courseFilter.Items.Add(new StudentResultCourseFilterModel { CourseId = 0, CourseName = "Tất cả khóa học" });
    foreach (var course in courses)
        _courseFilter.Items.Add(course);
    _courseFilter.SelectedIndex = 0;
}
```

- [ ] **Step 5: Replace LoadDataAsync and LoadResultTable filter logic**

Replace `LoadDataAsync` with:

```csharp
private async System.Threading.Tasks.Task LoadDataAsync()
{
    this.ShowSkeleton(SkeletonType.ResultTable);
    try
    {
        int studentId = UserSessionContext.CurrentUserId ?? 0;
        if (studentId <= 0)
        {
            BindResultTable(CreateMessageTable("Không xác định được tài khoản học sinh."));
            return;
        }

        await EnsureCourseFilterLoadedAsync(studentId);
        int selectedCourseId = _courseFilter.SelectedItem is StudentResultCourseFilterModel selected ? selected.CourseId : 0;
        string keyword = _searchBox.Text.Trim();
        DataTable table = await System.Threading.Tasks.Task.Run(() => LoadResultTable(studentId, selectedCourseId, keyword));
        BindResultTable(table);
    }
    catch (Exception ex)
    {
        BindResultTable(CreateMessageTable($"Không thể tải kết quả: {ex.Message}"));
    }
    finally
    {
        this.HideSkeleton();
    }
}
```

Change `LoadResultTable` signature to:

```csharp
private DataTable LoadResultTable(int studentId, int selectedCourseId, string keyword)
```

Replace the top of `LoadResultTable` with:

```csharp
var results = _dbContext.GetStudentResultItems(studentId, selectedCourseId > 0 ? selectedCourseId : null, keyword);
DataTable dt = CreateResultTableSchema();

if (results.Count == 0)
{
    string message = !string.IsNullOrWhiteSpace(keyword)
        ? "Không tìm thấy bài thi phù hợp."
        : selectedCourseId > 0
            ? "Khóa học này chưa có kết quả bài kiểm tra."
            : "Bạn chưa có kết quả bài kiểm tra nào.";
    dt.Rows.Add(0, 0, 0, "", message, "", "", "", "");
    return dt;
}
```

Then add rows:

```csharp
dt.Rows.Add(
    item.AttemptId,
    item.ExamId,
    item.CourseId,
    item.ExamStatus,
    item.ExamTitle,
    item.CourseName,
    item.CorrectAnswersText,
    item.Score.ToString("0.0", CultureInfo.InvariantCulture),
    item.StatusText);
```

- [ ] **Step 6: Replace result table schema**

Replace `CreateResultTableSchema` with:

```csharp
private static DataTable CreateResultTableSchema()
{
    DataTable dt = new DataTable();
    dt.Columns.Add("AttemptId", typeof(int));
    dt.Columns.Add("ExamId", typeof(int));
    dt.Columns.Add("CourseId", typeof(int));
    dt.Columns.Add("ExamStatus", typeof(string));
    dt.Columns.Add("Kỳ thi", typeof(string));
    dt.Columns.Add("Khóa học", typeof(string));
    dt.Columns.Add("Số câu đúng", typeof(string));
    dt.Columns.Add("Điểm", typeof(string));
    dt.Columns.Add("Xếp loại", typeof(string));
    return dt;
}
```

Update `CreateMessageTable` row to:

```csharp
dt.Rows.Add(0, 0, 0, "", message, "", "", "", "");
```

- [ ] **Step 7: Hide internal columns**

In `BindResultTable`, after setting `DataSource`, add:

```csharp
foreach (string columnName in new[] { "AttemptId", "ExamId", "CourseId", "ExamStatus" })
{
    if (dgvResults.Columns[columnName] != null)
        dgvResults.Columns[columnName]!.Visible = false;
}
```

- [ ] **Step 8: Add review and hide handlers**

Add:

```csharp
private void ReviewSelectedResult()
{
    int attemptId = CurrentInt("AttemptId");
    string examStatus = CurrentString("ExamStatus");
    if (attemptId <= 0 || !string.Equals(examStatus, WorkflowConstants.ExamStatus.Closed, StringComparison.OrdinalIgnoreCase))
    {
        MetaTheme.ShowModernDialog("Chỉ có thể xem lại bài kiểm tra đã đóng.", "Thông báo");
        return;
    }

    int studentId = UserSessionContext.CurrentUserId ?? 0;
    var review = _dbContext.GetStudentExamReview(studentId, attemptId);
    if (review == null)
    {
        MetaTheme.ShowModernDialog("Không tìm thấy dữ liệu xem lại cho bài kiểm tra này.", "Thông báo");
        return;
    }

    using var form = new CourseGuard.Frontend.Forms.Student.StudentExamReviewForm(review);
    form.ShowDialog(FindForm());
}

private async System.Threading.Tasks.Task HideSelectedResultAsync()
{
    int attemptId = CurrentInt("AttemptId");
    if (attemptId <= 0)
    {
        MetaTheme.ShowModernDialog("Vui lòng chọn một kết quả hợp lệ.", "Thông báo");
        return;
    }

    if (MetaTheme.ShowModernDialog("Ẩn kết quả này khỏi trang Kết quả của bạn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        return;

    int studentId = UserSessionContext.CurrentUserId ?? 0;
    if (_dbContext.HideStudentResult(studentId, attemptId))
        await LoadDataAsync();
}

private int CurrentInt(string columnName)
{
    if (dgvResults.CurrentRow == null || dgvResults.CurrentRow.IsNewRow)
        return 0;
    object? value = dgvResults.CurrentRow.Cells[columnName].Value;
    return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
}

private string CurrentString(string columnName)
{
    if (dgvResults.CurrentRow == null || dgvResults.CurrentRow.IsNewRow)
        return string.Empty;
    return dgvResults.CurrentRow.Cells[columnName].Value?.ToString() ?? string.Empty;
}
```

- [ ] **Step 9: Designer declaration**

If `UC_Result.Designer.cs` still declares only `btnReview`, leave `btnReview` unchanged. Do not add `_hideResult` to the designer because it is created in code.

- [ ] **Step 10: Build result UI**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 11: Commit result UI**

Run:

```powershell
git add CourseGuard/CourseGuard/Frontend/UserControls/Student/UC_Result.cs CourseGuard/CourseGuard/Frontend/UserControls/Student/UC_Result.Designer.cs
git commit -m "feat: improve student result filtering"
```

---

### Task 10: End-to-End Verification

**Files:**
- Verify: all touched CourseGuard files

- [ ] **Step 1: Build the app**

Run:

```powershell
dotnet build CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected: `Build succeeded.`

- [ ] **Step 2: Manual teacher workflow check**

Run the app from Visual Studio or:

```powershell
dotnet run --project CourseGuard/CourseGuard/CourseGuard.csproj
```

Expected teacher checks:

- Teacher opens `Bài kiểm tra`.
- Teacher clicks `Thêm`, saves an exam.
- New exam appears with `DRAFT`.
- Teacher selects it and clicks `Soạn câu hỏi`.
- Teacher adds 5 questions.
- Table refresh shows `Câu hỏi = 5`.
- Reopen `Soạn câu hỏi`; each row shows `Điểm = 2`.
- Teacher edits exam and changes status to `ACTIVE`.
- Status changes to `ACTIVE`.

- [ ] **Step 3: Manual activation validation check**

Expected:

- Create another exam and do not add questions.
- Edit it and choose `ACTIVE`.
- App shows `Bài kiểm tra cần có ít nhất 1 câu hỏi trước khi kích hoạt.`
- Status remains `DRAFT`.

- [ ] **Step 4: Manual student availability check**

Expected:

- Student opens `Bài kiểm tra`.
- Draft exams do not appear.
- Active exams appear if enrolled and within existing availability rules.
- Closed exams do not appear as startable exams.

- [ ] **Step 5: Manual student Results check**

Expected:

- Student opens `Kết quả`.
- Dropdown includes `Tất cả khóa học` and active/approved enrolled courses.
- Selecting a course with no results shows `Khóa học này chưa có kết quả bài kiểm tra.`
- Searching by exam title filters the current course selection.
- Searching for a missing title shows `Không tìm thấy bài thi phù hợp.`
- Selecting a `CLOSED` result and clicking `Xem lại bài` opens the read-only review form.
- Clicking `Ẩn khỏi Kết quả` removes the row from that student's Results page.

- [ ] **Step 6: Check git status**

Run:

```powershell
git status --short
```

Expected: only intentional local files are modified or untracked. Do not stage `.env`, `bin`, `obj`, `.vs`, or unrelated artifacts.
