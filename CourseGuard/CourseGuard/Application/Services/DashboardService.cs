using CourseGuard.Infrastructure.Data.Repositories;

namespace CourseGuard.Application.Services
{
    public class DashboardService
    {
        private readonly CourseRepository _courseRepo;
        private readonly ExamRepository _examRepo;
        private readonly NotificationRepository _notiRepo;

        public DashboardService()
        {
            _courseRepo = new CourseRepository();
            _examRepo = new ExamRepository();
            _notiRepo = new NotificationRepository();
        }

        public int GetTotalCourses(int studentId)
        {
            return _courseRepo.CountCoursesByStudent(studentId);
        }

        public int GetTotalExams(int studentId)
        {
            return _examRepo.CountExamsTaken(studentId);
        }

        public double GetAverageScore(int studentId)
        {
            return _examRepo.GetAverageScore(studentId);
        }

        public int GetNotifications(int studentId)
        {
            return _notiRepo.CountNotifications(studentId);
        }
    }
}