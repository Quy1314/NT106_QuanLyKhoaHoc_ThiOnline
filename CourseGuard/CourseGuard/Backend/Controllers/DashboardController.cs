using System;
using CourseGuard.Backend.Data;
using System.Collections.Generic;

namespace CourseGuard.Backend.Controllers
{
    public class DashboardController
    {
        private readonly CourseGuardDbContext _dbContext;

        public DashboardController(CourseGuardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public object GetStatistics()
        {
            return new { TotalUsers = 0, TotalCourses = 0 };
        }
    }
}
