using System;
using System.Collections.Generic;
using System.Data;
using CourseGuard.Infrastructure.Data;

namespace CourseGuard.Infrastructure.Data.Repositories
{
    public class ResultRepository
    {
        public DataTable GetResultsByStudent(int studentId)
        {
            string query = @"
                SELECT 
                    c.NAME AS CourseName, 
                    e.TITLE AS ExamTitle, 
                    ea.SCORE AS Score, 
                    ea.SUBMIT_TIME AS SubmitTime,
                    ea.STATUS AS Status
                FROM EXAM_ATTEMPTS ea
                JOIN EXAMS e ON ea.EXAM_ID = e.ID
                JOIN COURSES c ON e.COURSE_ID = c.ID
                WHERE ea.STUDENT_ID = @studentId
                ORDER BY ea.SUBMIT_TIME DESC";

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@studentId", (SqlDbType.Int, studentId) }
            };
            return DatabaseAction.ExecuteQuery(query, parameters);
        }
    }
}
