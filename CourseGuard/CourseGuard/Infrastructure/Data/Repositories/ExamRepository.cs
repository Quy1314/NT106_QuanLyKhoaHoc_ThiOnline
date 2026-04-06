using System;
using System.Collections.Generic;
using System.Data;
using CourseGuard.Infrastructure.Data;

namespace CourseGuard.Infrastructure.Data.Repositories
{
    public class ExamRepository
    {
        public int CountExamsTaken(int studentId)
        {
            string query = "SELECT COUNT(*) FROM EXAM_ATTEMPTS WHERE STUDENT_ID = @studentId";
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@studentId", (SqlDbType.Int, studentId) }
            };
            object result = DatabaseAction.ExecuteScalar(query, parameters);
            return result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public double GetAverageScore(int studentId)
        {
            string query = "SELECT AVG(SCORE) FROM EXAM_ATTEMPTS WHERE STUDENT_ID = @studentId AND SCORE IS NOT NULL";
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@studentId", (SqlDbType.Int, studentId) }
            };
            object result = DatabaseAction.ExecuteScalar(query, parameters);
            return result != DBNull.Value ? Convert.ToDouble(result) : 0.0;
        }
    }
}
