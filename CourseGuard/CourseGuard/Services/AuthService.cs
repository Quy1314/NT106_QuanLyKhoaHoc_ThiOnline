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
        private readonly string connectionString =
            "Server=localhost;Database=CourseGuardDB;Trusted_Connection=True;TrustServerCertificate=True";

        public UserModel? Login(string username, string password) // Cho phép return null nếu đăng nhập thất bại
        {
            string hashedPassword = PasswordHasher.HashPassword(password);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT u.id, u.username, u.status, r.name AS role_name
                    FROM users u
                    JOIN roles r ON u.role_id = r.id
                    WHERE u.username = @username 
                    AND u.password_hash = @password_hash";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", hashedPassword);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["username"]?.ToString() ?? string.Empty,
                                Role = reader["role_name"]?.ToString() ?? string.Empty,
                                Status = reader["status"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
