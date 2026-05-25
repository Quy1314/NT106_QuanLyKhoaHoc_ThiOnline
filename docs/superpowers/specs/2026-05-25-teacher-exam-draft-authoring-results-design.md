# Teacher Exam Draft Authoring and Student Results Design

## Context

CourseGuard currently lets teachers create exams from the Teacher "Bai kiem tra" page, but the current flow treats the created exam as immediately active in the UI and derives the visible exam status mostly from open and close time. Teachers need a safer two-step workflow:

1. Create the exam shell first.
2. Add the exam questions later.
3. Activate the exam only when it is ready for students.

Student results also need course filtering, exam-title search, closed-exam review, and per-student result hiding without deleting official attempt or score data.

## Goals

- New teacher-created exams start as `DRAFT`.
- Draft exams are visible in the teacher exam table immediately after creation.
- Teachers can select an exam and open a separate question authoring form.
- Teachers can activate an exam manually after adding questions.
- Students can only take exams that are `ACTIVE` and pass the existing availability rules.
- Closed exams can still be reviewed from the student Results page.
- Students can hide a result from their own Results page without deleting exam attempts, scores, teacher records, admin records, or integrity history.
- Student Results supports course filtering and exam-title search.

## Non-Goals

- Essay questions and manual grading are not included in this phase.
- Per-question custom point values are not included in this phase.
- A full exam preview or wizard authoring experience is not included.
- Deleting official exam attempts or scores is not supported.

## Domain Language

An exam has a persisted publication status:

- `DRAFT`: teacher can prepare metadata and questions. Students cannot see or take the exam.
- `ACTIVE`: students can see and take the exam if enrolled, within time limits, and within attempt limits.
- `CLOSED`: students cannot start a new attempt, but can review results from the Results page.

"Hide result" means removing a result from one student's own Results page only. It does not remove official exam data.

## Teacher Workflow

### Create Exam Shell

The teacher clicks `Them` in the Teacher "Bai kiem tra" page and fills in exam metadata such as course, title, content/description where supported, open time, close time, duration, and max attempts.

On save:

- The exam is inserted with status `DRAFT`.
- The exam appears immediately in the Teacher "Bai kiem tra" table.
- The table shows the persisted status from the database, not only a derived time status.

### Author Questions

The Teacher "Bai kiem tra" page gets a new action button: `Soan cau hoi`.

The teacher selects an exam row and clicks `Soan cau hoi`. The app opens a new dialog, for example `TeacherExamQuestionsDialog`, for that `examId`.

The dialog supports multiple-choice questions only:

- Question text.
- Option A.
- Option B.
- Option C.
- Option D.
- Correct option: A, B, C, or D.
- Display order.

Question count is calculated from saved questions. Teachers do not enter question count manually.

### Scoring Rule

Each exam has a default total score of 10.

Question scores are distributed evenly:

- 5 questions means 2.0 points per question.
- 10 questions means 1.0 point per question.
- 40 questions means 0.25 points per question.

When a teacher adds or deletes questions, the app recalculates all question points as `10 / total_question_count`.

Teachers cannot edit per-question points in this phase.

### Activation Rules

Teachers can set an exam status to `ACTIVE` only when it has at least one saved question.

If the teacher tries to activate an exam without questions, the app blocks the action and shows a clear validation message.

Question editing is allowed while the exam is `DRAFT`. Once an exam is `ACTIVE` or `CLOSED`, teachers can view questions but should not modify them in this phase, to avoid changing content after students may have started attempts.

## Student Exam Availability

The Student "Bai kiem tra" page should show only exams that:

- Belong to a course where the student's enrollment is active or approved.
- Have exam status `ACTIVE`.
- Satisfy the existing open time, close time, and max attempt rules.

`DRAFT` exams are invisible to students.

`CLOSED` exams are not available for new attempts.

## Student Results Workflow

### Course Filter

The Student Results page gets a course dropdown.

The first option is `Tat ca khoa hoc`.

Other options come from the courses where the student is currently studying, meaning active/approved enrollment according to the existing project conventions.

The dropdown is not limited to courses that already have results. If a selected course has no result rows, the page shows an empty state instead of treating it as an error.

Empty states:

- For `Tat ca khoa hoc` with no results: `Ban chua co ket qua bai kiem tra nao.`
- For a selected course with no results: `Khoa hoc nay chua co ket qua bai kiem tra.`

### Search

The Results page gets a search box.

Search runs after the course filter and searches only by exam title. It does not search course name because course is already selected by the dropdown.

If no result matches the current filter and search term, the page shows: `Khong tim thay bai thi phu hop.`

### Results Data

The visible results table keeps the current user-facing columns and includes hidden identifiers needed for actions:

- `AttemptId`
- `ExamId`
- `CourseId`
- `ExamStatus`

Only results not hidden by the current student are displayed.

### Review Closed Exams

Students can review closed exams from the Results page in read-only mode.

The first implementation can show exam metadata, score, and the stored multiple-choice questions with correct answers if question data is available. It should not allow editing or resubmission.

### Hide Result

The Results page gets an `An khoi Ket qua` action.

When clicked:

- The app confirms the action.
- The selected result is hidden only for the current student.
- The exam attempt, score, teacher view, admin view, and integrity history remain intact.
- After hiding, the page refreshes and the row no longer appears for that student.

The `Xem lai bai` and `An khoi Ket qua` actions are disabled when no valid result row is selected.

## Data Model Changes

Minimum schema changes:

- Add `status` to `exams`, default `DRAFT`.
- Add or ensure `exam_questions` stores the multiple-choice question body and options:
  - `id`
  - `exam_id`
  - `question_text`
  - `option_a`
  - `option_b`
  - `option_c`
  - `option_d`
  - `correct_option`
  - `points`
  - `display_order`
- Add a per-student result hiding mechanism, either a dedicated table keyed by `student_id` and `attempt_id`, or an equivalent column if the current schema supports it without corrupting shared attempt data.

The dedicated table is preferred because result hiding is a student-specific view preference, not a property of the attempt itself.

## Backend Changes

Teacher repository/controller:

- Create exams with `DRAFT` status.
- Read and display persisted exam status.
- Update exam status with validation.
- Block `ACTIVE` when no questions exist.
- Add CRUD methods for exam questions.
- Recalculate question points after question add/delete.

Student data access:

- Filter available exams by `exams.status = 'ACTIVE'`.
- Return active/approved enrolled courses for the Results dropdown.
- Return results filtered by course, exam title search, current student, and not hidden.
- Add a method to hide a result for the current student.
- Allow closed exam review from results.

## UI Changes

Teacher:

- Update `TeacherExamDialog` default status to `DRAFT`.
- Update Teacher "Bai kiem tra" table to show persisted status.
- Add `Soan cau hoi` action.
- Add `TeacherExamQuestionsDialog` for multiple-choice authoring.
- Disable question modification for non-draft exams.

Student:

- Add course dropdown to `UC_Result`.
- Add search box to `UC_Result`.
- Add `An khoi Ket qua` action.
- Wire `Xem lai bai` to a read-only review flow.
- Show empty states for no results or no search matches.

## Testing Strategy

Build verification:

- `dotnet build CourseGuard/CourseGuard/CourseGuard.csproj`

Teacher workflow checks:

- Creating an exam produces `DRAFT`.
- Draft exam appears in the teacher table.
- Draft exam does not appear in the student exam list.
- Adding questions updates `QuestionCount`.
- Adding/deleting questions recalculates total points to 10.
- Activating an exam without questions is blocked.
- Activating an exam with questions succeeds.

Student workflow checks:

- Student exam list shows only `ACTIVE` exams that pass availability rules.
- Closed exams are not available for new attempts.
- Closed exams can be opened from Results for review.
- Results dropdown includes active/approved enrolled courses even when a course has no results.
- Search filters by exam title only.
- Hiding a result removes it only from that student's Results page.

## Open Implementation Notes

The current codebase uses raw SQL through `NpgsqlCommand`; new data access should follow that pattern.

The current student exam taking form still uses dummy question UI. This design introduces stored multiple-choice questions so future implementation can replace the dummy question rendering with database-backed questions.
