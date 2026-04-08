using System.Data;
using Npgsql;
using CourseGuard.Backend.Models;

namespace CourseGuard.Backend.Data
{
    /// <summary>
    /// Core Data Context for CourseGuard.
    /// Simplified access replacing Repositories.
    /// </summary>
    public class CourseGuardDbContext
    {
        private readonly string _connectionString;

        public CourseGuardDbContext(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _connectionString = "Host=db.crtiwzjkcmpvyoqgdowv.supabase.co;Port=6543;Database=postgres;Username=postgres;Password=testdatabseuit;SSL Mode=Require;Trust Server Certificate=true;Timeout=15;";
            }
            else
            {
                _connectionString = connectionString;
            }
        }

        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // Updated direct query method to include all fields
        public UserModel? GetUserByUsername(string username)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = @"SELECT u.id, u.username, u.password_hash, u.full_name, u.email, r.name as role, u.status 
                            FROM USERS u 
                            JOIN ROLES r ON u.role_id = r.id 
                            WHERE LOWER(u.username) = LOWER(@username)";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new UserModel
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    FullName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Role = reader.GetString(5),
                    Status = reader.GetString(6)
                };
            }

            return null;
        }

        public void InsertUser(UserModel user, string passwordHash)
        {
            using var connection = CreateConnection();
            connection.Open();

            // Resolve role_id first
            string getRoleIdQuery = "SELECT id FROM ROLES WHERE name = @roleName";
            int roleId = 3; // Default to STUDENT
            using (var roleCmd = new NpgsqlCommand(getRoleIdQuery, connection))
            {
                roleCmd.Parameters.AddWithValue("@roleName", (user.Role ?? "STUDENT").ToUpper());
                var result = roleCmd.ExecuteScalar();
                if (result != null) roleId = Convert.ToInt32(result);
            }

            string query = @"INSERT INTO USERS (username, password_hash, full_name, email, role_id, status) 
                            VALUES (@username, @password_hash, @full_name, @email, @role_id, @status)";
            
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@password_hash", passwordHash);
            command.Parameters.AddWithValue("@full_name", user.FullName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@email", user.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@role_id", roleId);
            command.Parameters.AddWithValue("@status", user.Status ?? "PENDING");

            command.ExecuteNonQuery();
        }

        public void UpdateUserStatus(int userId, string status)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = "UPDATE Users SET status = @status WHERE id = @id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@status", status);
            command.Parameters.AddWithValue("@id", userId);

            command.ExecuteNonQuery();
        }

        public void UpdateUserPassword(int userId, string passwordHash)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = "UPDATE Users SET password_hash = @password_hash WHERE id = @id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@password_hash", passwordHash);
            command.Parameters.AddWithValue("@id", userId);

            command.ExecuteNonQuery();
        }

        public List<UserModel> GetUsersByStatus(params string[] statuses)
        {
            var users = new List<UserModel>();
            using var connection = CreateConnection();
            connection.Open();

            string statusParams = string.Join(",", statuses.Select((s, i) => $"@s{i}"));
            string query = $@"SELECT u.id, u.username, u.password_hash, u.full_name, u.email, r.name as role, u.status 
                             FROM USERS u 
                             JOIN ROLES r ON u.role_id = r.id 
                             WHERE u.status IN ({statusParams})";
            
            using var command = new NpgsqlCommand(query, connection);
            for (int i = 0; i < statuses.Length; i++)
            {
                command.Parameters.AddWithValue($"@s{i}", statuses[i]);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new UserModel
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    FullName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Role = reader.GetString(5),
                    Status = reader.GetString(6)
                });
            }

            return users;
        }

        public UserModel? GetUserByUsernameAndEmail(string username, string email)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = @"SELECT u.id, u.username, u.password_hash, u.full_name, u.email, r.name as role, u.status 
                            FROM USERS u 
                            JOIN ROLES r ON u.role_id = r.id 
                            WHERE LOWER(u.username) = LOWER(@username) AND u.email = @email";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@email", email);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new UserModel
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    FullName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Role = reader.GetString(5),
                    Status = reader.GetString(6)
                };
            }

            return null;
        }

        public List<CourseModel> GetAllCourses()
        {
            var courses = new List<CourseModel>();
            using var connection = CreateConnection();
            connection.Open();

            string query = @"SELECT c.id, c.name, c.description, c.teacher_id, u.full_name as teacher_name, c.status, c.start_date, c.end_date 
                             FROM COURSES c 
                             JOIN USERS u ON c.teacher_id = u.id";
            
            using var command = new NpgsqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                courses.Add(new CourseModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    TeacherId = reader.GetInt32(3),
                    TeacherName = reader.IsDBNull(4) ? "Unknown" : reader.GetString(4),
                    Status = reader.GetString(5),
                    StartDate = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
                    EndDate = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7)
                });
            }

            return courses;
        }

        public void InsertCourse(CourseModel course)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = @"INSERT INTO COURSES (name, description, teacher_id, status, start_date, end_date) 
                            VALUES (@name, @description, @teacher_id, @status, @start_date, @end_date)";
            
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", course.Name);
            command.Parameters.AddWithValue("@description", course.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@teacher_id", course.TeacherId);
            command.Parameters.AddWithValue("@status", course.Status ?? "ACTIVE");
            command.Parameters.AddWithValue("@start_date", course.StartDate == DateTime.MinValue ? (object)DBNull.Value : course.StartDate);
            command.Parameters.AddWithValue("@end_date", course.EndDate == DateTime.MinValue ? (object)DBNull.Value : course.EndDate);

            command.ExecuteNonQuery();
        }

        public void UpdateCourse(CourseModel course)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = @"UPDATE COURSES SET name = @name, description = @description, teacher_id = @teacher_id, 
                             status = @status, start_date = @start_date, end_date = @end_date WHERE id = @id";
            
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", course.Name);
            command.Parameters.AddWithValue("@description", course.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@teacher_id", course.TeacherId);
            command.Parameters.AddWithValue("@status", course.Status);
            command.Parameters.AddWithValue("@start_date", course.StartDate == DateTime.MinValue ? (object)DBNull.Value : course.StartDate);
            command.Parameters.AddWithValue("@end_date", course.EndDate == DateTime.MinValue ? (object)DBNull.Value : course.EndDate);
            command.Parameters.AddWithValue("@id", course.Id);

            command.ExecuteNonQuery();
        }

        public void DeleteCourse(int courseId)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = "DELETE FROM COURSES WHERE id = @id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", courseId);

            command.ExecuteNonQuery();
        }

        public void EnrollStudent(int courseId, int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = @"INSERT INTO ENROLLMENTS (course_id, student_id, status) 
                            VALUES (@course_id, @student_id, 'ACTIVE')
                            ON CONFLICT DO NOTHING";
            
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@student_id", studentId);

            command.ExecuteNonQuery();
        }

        public bool UserExists(string username)
        {
            using var connection = CreateConnection();
            connection.Open();
            string query = "SELECT COUNT(*) FROM USERS WHERE LOWER(username) = LOWER(@username)";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            return Convert.ToInt64(command.ExecuteScalar()) > 0;
        }

        public void LogDeviceActivity(int userId, string deviceName, string ipAddress)
        {
            using var connection = CreateConnection();
            connection.Open();
            string query = @"INSERT INTO DEVICES (user_id, device_name, ip_address, status, last_active) 
                            VALUES (@user_id, @device_name, @ip_address, 'ACTIVE', CURRENT_TIMESTAMP)
                            ON CONFLICT (id) DO UPDATE SET last_active = CURRENT_TIMESTAMP, ip_address = @ip_address";
            
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@device_name", deviceName ?? "Unknown Device");
            command.Parameters.AddWithValue("@ip_address", ipAddress ?? "Unknown IP");

            command.ExecuteNonQuery();
        }

        public List<UserModel> SearchUsers(string status, string role)
        {
            var users = new List<UserModel>();
            using var connection = CreateConnection();
            connection.Open();

            string query = @"SELECT u.id, u.username, u.password_hash, u.full_name, u.email, r.name as role, u.status 
                             FROM USERS u 
                             JOIN ROLES r ON u.role_id = r.id 
                             WHERE 1=1";
            
            if (status != "ALL") query += " AND u.status = @status";
            if (role != "ALL") query += " AND UPPER(r.name) = UPPER(@role)";

            using var command = new NpgsqlCommand(query, connection);
            if (status != "ALL") command.Parameters.AddWithValue("@status", status);
            if (role != "ALL") command.Parameters.AddWithValue("@role", role);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new UserModel
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    FullName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Role = reader.GetString(5),
                    Status = reader.GetString(6)
                });
            }

            return users;
        }
    }
}
