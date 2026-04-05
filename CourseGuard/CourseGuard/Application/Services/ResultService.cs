using System.Data;
using CourseGuard.Infrastructure.Data.Repositories;

namespace CourseGuard.Application.Services
{
    public class ResultService
    {
        private readonly ResultRepository _repo;

        public ResultService()
        {
            _repo = new ResultRepository();
        }

        public DataTable GetResults(int studentId)
        {
            return _repo.GetResultsByStudent(studentId);
        }
    }
}