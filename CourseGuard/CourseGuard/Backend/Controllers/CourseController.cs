using System;
using System.Collections.Generic;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;

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
            try
            {
                _dbContext.EnrollStudent(courseId, studentId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
