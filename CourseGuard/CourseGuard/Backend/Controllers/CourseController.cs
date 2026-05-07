using System;
using System.Collections.Generic;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;

namespace CourseGuard.Backend.Controllers
{
    public class CourseController
    {
        private readonly CourseGuardDbContext _dbContext;

        public CourseController(CourseGuardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Actual implementations
        public List<CourseModel> GetAllCourses()
        {
            return _dbContext.GetAllCourses();
        }

        public string AddCourse(CourseModel course)
        {
            if (!UserSessionContext.IsAdmin())
            {
                return "Forbidden";
            }

            try
            {
                _dbContext.InsertCourse(course);
                return "Success";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public bool UpdateCourse(CourseModel course)
        {
            if (!UserSessionContext.IsAdmin())
            {
                return false;
            }

            try
            {
                _dbContext.UpdateCourse(course);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteCourse(int courseId)
        {
            if (!UserSessionContext.IsAdmin())
            {
                return false;
            }

            try
            {
                _dbContext.DeleteCourse(courseId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EnrollStudent(int courseId, int studentId)
        {
            if (!UserSessionContext.IsAdmin())
            {
                return false;
            }

            try
            {
                bool enrolled = _dbContext.EnrollStudent(courseId, studentId);
                if (enrolled)
                {
                    _dbContext.LogUserActivity(
                        UserSessionContext.CurrentUserId,
                        "COURSE_ENROLL",
                        $"Admin ghi danh học viên ID={studentId} vào khóa học ID={courseId}",
                        string.Empty);
                }
                return enrolled;
            }
            catch
            {
                return false;
            }
        }

        public HashSet<int> GetStudentEnrolledCourseIds(int studentId)
        {
            if (studentId <= 0)
            {
                return new HashSet<int>();
            }

            return _dbContext.GetStudentEnrolledCourseIds(studentId);
        }

        public bool StudentJoinCourse(int courseId, int studentId)
        {
            if (studentId <= 0)
            {
                return false;
            }

            if (UserSessionContext.CurrentUserId != studentId)
            {
                return false;
            }

            try
            {
                bool enrolled = _dbContext.EnrollStudent(courseId, studentId);
                if (enrolled)
                {
                    _dbContext.LogUserActivity(
                        studentId,
                        "COURSE_ENROLL_REQUEST",
                        $"Học viên ID={studentId} tham gia khóa học ID={courseId}",
                        string.Empty);
                }

                return enrolled;
            }
            catch
            {
                return false;
            }
        }
    }
}
