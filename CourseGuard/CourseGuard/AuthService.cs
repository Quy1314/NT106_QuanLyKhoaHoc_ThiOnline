using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace CourseGuard
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
    internal class AuthService
    {
        private readonly string connectionString =
            "Server=localhost;Database=CourseGuardDB;Trusted_Connection=True;TrustServerCertificate=True";

        public UserModel? Login(string username, string password) // Cho phép return null nếu đăng nhập thất bại
        {
            string hashedPassword = HashPassword(password);

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

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }
    }
}
