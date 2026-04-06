using System;
using System.Collections.Generic;
using System.Data;
using CourseGuard.Infrastructure.Data;

namespace CourseGuard.Infrastructure.Data.Repositories
{
    public class NotificationRepository
    {
        public int CountNotifications(int studentId)
        {
            string query = "SELECT COUNT(*) FROM NOTIFICATIONS WHERE USER_ID = @userId AND IS_READ = 0";
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@userId", (SqlDbType.Int, studentId) }
            };
            object result = DatabaseAction.ExecuteScalar(query, parameters);
            return result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }
}
