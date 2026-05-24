using System.Collections.Generic;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;

namespace CourseGuard.Backend.Controllers
{
    public class TeacherController
    {
        private readonly TeacherRepository _repository;

        public TeacherController(CourseGuardDbContext dbContext)
        {
            _repository = new TeacherRepository(dbContext);
        }

        public TeacherDashboardSummaryModel GetDashboardSummary(int teacherId) =>
            teacherId <= 0 ? new TeacherDashboardSummaryModel() : _repository.GetDashboardSummary(teacherId);

        public TeacherProfileModel? GetTeacherProfile(int teacherId) =>
            teacherId <= 0 ? null : _repository.GetTeacherProfile(teacherId);

        public bool UpsertTeacherProfile(int teacherId, TeacherProfileModel input)
        {
            if (teacherId <= 0)
                return false;

            input.UserId = teacherId;
            return _repository.UpsertTeacherProfile(teacherId, input);
        }

        public List<TeacherCourseModel> GetCourses(int teacherId) =>
            teacherId <= 0 ? new List<TeacherCourseModel>() : _repository.GetCourses(teacherId);

        public int CreateCourse(int teacherId, TeacherCourseModel input) =>
            teacherId <= 0 ? 0 : _repository.CreateCourse(teacherId, input);

        public bool UpdateCourse(int teacherId, TeacherCourseModel input) =>
            teacherId > 0 && _repository.UpdateCourse(teacherId, input);

        public bool SubmitCourseForApproval(int teacherId, int courseId) =>
            teacherId > 0 && _repository.SubmitCourseForApproval(teacherId, courseId);

        public bool DeleteCourse(int teacherId, int courseId) =>
            teacherId > 0 && _repository.DeleteCourse(teacherId, courseId);

        public List<TeacherLessonModel> GetLessons(int teacherId) =>
            teacherId <= 0 ? new List<TeacherLessonModel>() : _repository.GetLessons(teacherId);

        public int CreateLesson(int teacherId, TeacherLessonModel input) =>
            teacherId <= 0 ? 0 : _repository.CreateLesson(teacherId, input);

        public bool UpdateLesson(int teacherId, TeacherLessonModel input) =>
            teacherId > 0 && _repository.UpdateLesson(teacherId, input);

        public bool DeleteLesson(int teacherId, int lessonId) =>
            teacherId > 0 && _repository.DeleteLesson(teacherId, lessonId);

        public List<TeacherAssignmentModel> GetAssignments(int teacherId) =>
            teacherId <= 0 ? new List<TeacherAssignmentModel>() : _repository.GetAssignments(teacherId);

        public int CreateAssignment(int teacherId, TeacherAssignmentModel input) =>
            teacherId <= 0 ? 0 : _repository.CreateAssignment(teacherId, input);

        public bool UpdateAssignment(int teacherId, TeacherAssignmentModel input) =>
            teacherId > 0 && _repository.UpdateAssignment(teacherId, input);

        public bool DeleteAssignment(int teacherId, int assignmentId) =>
            teacherId > 0 && _repository.DeleteAssignment(teacherId, assignmentId);

        public List<TeacherExamModel> GetExams(int teacherId) =>
            teacherId <= 0 ? new List<TeacherExamModel>() : _repository.GetExams(teacherId);

        public int CreateExam(int teacherId, TeacherExamModel input) =>
            teacherId <= 0 ? 0 : _repository.CreateExam(teacherId, input);

        public bool UpdateExam(int teacherId, TeacherExamModel input) =>
            teacherId > 0 && _repository.UpdateExam(teacherId, input);

        public bool CanActivateExam(int teacherId, int examId) =>
            teacherId > 0 && examId > 0 && _repository.CanActivateExam(teacherId, examId);

        public bool DeleteExam(int teacherId, int examId) =>
            teacherId > 0 && _repository.DeleteExam(teacherId, examId);

        public List<TeacherStudentModel> GetPendingEnrollments(int teacherId) =>
            teacherId <= 0 ? new List<TeacherStudentModel>() : _repository.GetPendingEnrollments(teacherId);

        public List<TeacherStudentModel> GetEnrolledStudents(int teacherId, int? courseId = null) =>
            teacherId <= 0 ? new List<TeacherStudentModel>() : _repository.GetEnrolledStudents(teacherId, courseId);

        public bool ApproveEnrollment(int teacherId, int courseId, int studentId) =>
            teacherId > 0 && _repository.ApproveEnrollment(teacherId, courseId, studentId);

        public bool RejectEnrollment(int teacherId, int courseId, int studentId) =>
            teacherId > 0 && _repository.RejectEnrollment(teacherId, courseId, studentId);

        public List<TeacherScoreModel> GetResults(int teacherId, int? courseId = null, int? examId = null) =>
            teacherId <= 0 ? new List<TeacherScoreModel>() : _repository.GetResults(teacherId, courseId, examId);

        public bool UpdateScore(int teacherId, TeacherScoreModel input) =>
            teacherId > 0 && _repository.UpdateScore(teacherId, input);

        public List<TeacherMaterialModel> GetMaterials(int teacherId, int? courseId = null) =>
            teacherId <= 0 ? new List<TeacherMaterialModel>() : _repository.GetMaterials(teacherId, courseId);

        public int CreateMaterial(int teacherId, TeacherMaterialModel input) =>
            teacherId <= 0 ? 0 : _repository.CreateMaterial(teacherId, input);

        public bool UpdateMaterial(int teacherId, TeacherMaterialModel input) =>
            teacherId > 0 && _repository.UpdateMaterial(teacherId, input);

        public bool DeleteMaterial(int teacherId, int materialId) =>
            teacherId > 0 && _repository.DeleteMaterial(teacherId, materialId);

        public List<TeacherScheduleItemModel> GetSchedule(int teacherId) =>
            teacherId <= 0 ? new List<TeacherScheduleItemModel>() : _repository.GetSchedule(teacherId);

        public List<TeacherTeachingTaskModel> GetTeachingTasks(int teacherId) =>
            teacherId <= 0 ? new List<TeacherTeachingTaskModel>() : _repository.GetTeachingTasks(teacherId);

        public int CreateScheduleItem(int teacherId, TeacherScheduleItemModel input) =>
            teacherId <= 0 ? 0 : _repository.CreateScheduleItem(teacherId, input);

        public bool UpdateScheduleItem(int teacherId, TeacherScheduleItemModel input) =>
            teacherId > 0 && _repository.UpdateScheduleItem(teacherId, input);

        public bool DeleteScheduleItem(int teacherId, int scheduleId) =>
            teacherId > 0 && _repository.DeleteScheduleItem(teacherId, scheduleId);

        public List<TeacherActiveExamSessionModel> GetActiveExamSessions(int teacherId) =>
            teacherId <= 0 ? new List<TeacherActiveExamSessionModel>() : _repository.GetActiveExamSessions(teacherId);
    }
}
