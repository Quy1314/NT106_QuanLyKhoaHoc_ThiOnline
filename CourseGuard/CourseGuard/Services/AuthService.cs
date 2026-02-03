using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using CourseGuard.Models;
using CourseGuard.Security; 

namespace CourseGuard.Services
{
    internal class AuthService
    {
        public UserModel? Login(string username, string password) // Cho phép return null nếu đăng nhập thất bại
        {
            string hashedPassword = PasswordHasher.HashPassword(password);

            string query = @"
                SELECT u.id, u.username, u.status, r.name AS role_name
                FROM users u
                JOIN roles r ON u.role_id = r.id
                WHERE u.username = @username 
                AND u.password_hash = @password_hash";

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@username", (SqlDbType.NVarChar, username) },
                { "@password_hash", (SqlDbType.NVarChar, hashedPassword) }
            };

            DataTable dt = CourseGuard.Data.DatabaseAction.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new UserModel
                {
                    Id = Convert.ToInt32(row["id"]),
                    Username = row["username"]?.ToString() ?? string.Empty,
                    Role = row["role_name"]?.ToString() ?? string.Empty,
                    Status = row["status"]?.ToString() ?? string.Empty
                };
            }

            return null;
        }
    }
}
