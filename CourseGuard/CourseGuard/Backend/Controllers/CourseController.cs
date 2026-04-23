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
                return _dbContext.EnrollStudent(courseId, studentId);
            }
            catch
            {
                return false;
            }
        }
    }
}
