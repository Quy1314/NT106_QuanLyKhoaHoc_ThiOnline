using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CourseGuard.Backend.Config;
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
                _connectionString = AppEnvironment.GetRequired(
                    "COURSEGUARD_DB_CONNECTION",
                    "SUPABASE_DB_CONNECTION",
                    "CONNECTION_STRING");
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

        public async Task<UserModel?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string query = @"SELECT u.id, u.username, u.password_hash, u.full_name, u.email, r.name as role, u.status
                            FROM USERS u
                            JOIN ROLES r ON u.role_id = r.id
                            WHERE LOWER(u.username) = LOWER(@username)";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
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

        public UserModel? GetUserById(int userId)
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"SELECT u.id, u.username, u.password_hash, u.full_name, u.email, r.name as role, u.status
                                   FROM USERS u
                                   JOIN ROLES r ON u.role_id = r.id
                                   WHERE u.id = @id";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", userId);

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

        public bool DeleteUser(int userId)
        {
            using var connection = CreateConnection();
            connection.Open();

            // Avoid deleting admin account to keep system operable.
            const string roleCheckQuery = @"
                SELECT COALESCE(r.NAME, '')
                FROM USERS u
                JOIN ROLES r ON u.ROLE_ID = r.ID
                WHERE u.ID = @id";
            using (var roleCmd = new NpgsqlCommand(roleCheckQuery, connection))
            {
                roleCmd.Parameters.AddWithValue("@id", userId);
                string roleName = (roleCmd.ExecuteScalar()?.ToString() ?? string.Empty).ToUpperInvariant();
                if (roleName == "ADMIN")
                {
                    return false;
                }
            }

            const string deleteQuery = "DELETE FROM USERS WHERE id = @id";
            using var command = new NpgsqlCommand(deleteQuery, connection);
            command.Parameters.AddWithValue("@id", userId);
            int affectedRows = command.ExecuteNonQuery();
            return affectedRows > 0;
        }

        public List<UserModel> GetUsersByRole(string role)
        {
            var users = new List<UserModel>();
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"SELECT u.id, u.username, u.password_hash, u.full_name, u.email, r.name as role, u.status
                                   FROM USERS u
                                   JOIN ROLES r ON u.role_id = r.id
                                   WHERE UPPER(r.name) = UPPER(@role)
                                   ORDER BY u.id DESC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@role", role);

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

        public bool EnrollStudent(int courseId, int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                WITH existing AS (
                    SELECT 1 FROM ENROLLMENTS WHERE course_id = @course_id AND student_id = @student_id
                )
                INSERT INTO ENROLLMENTS (course_id, student_id, status)
                SELECT @course_id, @student_id, 'ACTIVE'
                WHERE NOT EXISTS (SELECT 1 FROM existing)";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@student_id", studentId);

            return command.ExecuteNonQuery() > 0;
        }

        // ════════════════════════════════════════════════════════════════
        //  STUDENT ENROLLMENT METHODS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Lấy danh sách khóa học mà sinh viên đã ghi danh (bất kể trạng thái).
        /// </summary>
        public List<EnrollmentModel> GetEnrollmentsByStudent(int studentId)
        {
            var list = new List<EnrollmentModel>();
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT e.id, e.course_id, e.student_id, e.status, e.joined_at,
                       c.name AS course_name, COALESCE(u.full_name, u.username) AS teacher_name,
                       c.status AS course_status, c.start_date, c.end_date, COALESCE(c.description, '') AS description
                FROM ENROLLMENTS e
                JOIN COURSES c ON e.course_id = c.id
                JOIN USERS u ON c.teacher_id = u.id
                WHERE e.student_id = @student_id
                ORDER BY e.joined_at DESC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@student_id", studentId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new EnrollmentModel
                {
                    Id = reader.GetInt32(0),
                    CourseId = reader.GetInt32(1),
                    StudentId = reader.GetInt32(2),
                    Status = reader.GetString(3),
                    JoinedAt = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                    CourseName = reader.GetString(5),
                    TeacherName = reader.IsDBNull(6) ? "Unknown" : reader.GetString(6),
                    CourseStatus = reader.GetString(7),
                    CourseStartDate = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8),
                    CourseEndDate = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                    CourseDescription = reader.GetString(10)
                });
            }

            return list;
        }

        /// <summary>
        /// Lấy trạng thái enrollment hiện tại của sinh viên trong một khóa học.
        /// Trả về null nếu chưa ghi danh.
        /// </summary>
        public string? GetEnrollmentStatus(int courseId, int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = "SELECT status FROM ENROLLMENTS WHERE course_id = @course_id AND student_id = @student_id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@student_id", studentId);

            var result = command.ExecuteScalar();
            return result?.ToString();
        }

        /// <summary>
        /// Sinh viên tự ghi danh vào khóa học (trạng thái PENDING, chờ Admin/Teacher duyệt).
        /// Trả về true nếu thành công, false nếu đã tồn tại.
        /// </summary>
        public bool SelfEnroll(int courseId, int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                INSERT INTO ENROLLMENTS (course_id, student_id, status)
                SELECT @course_id, @student_id, 'PENDING'
                WHERE NOT EXISTS (
                    SELECT 1 FROM ENROLLMENTS
                    WHERE course_id = @course_id AND student_id = @student_id
                )";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@student_id", studentId);

            return command.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Hủy / rút khỏi khóa học.
        /// Nếu trạng thái là PENDING → xóa hẳn record.
        /// Nếu trạng thái là ACTIVE → chuyển sang DROPPED.
        /// </summary>
        public bool DropEnrollment(int courseId, int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            // Lấy trạng thái hiện tại
            const string statusQuery = "SELECT status FROM ENROLLMENTS WHERE course_id = @cid AND student_id = @sid";
            using var statusCmd = new NpgsqlCommand(statusQuery, connection);
            statusCmd.Parameters.AddWithValue("@cid", courseId);
            statusCmd.Parameters.AddWithValue("@sid", studentId);
            var currentStatus = statusCmd.ExecuteScalar()?.ToString();

            if (string.IsNullOrEmpty(currentStatus)) return false;

            if (currentStatus == "PENDING")
            {
                // Xóa hẳn nếu chưa được duyệt
                const string deleteQuery = "DELETE FROM ENROLLMENTS WHERE course_id = @cid AND student_id = @sid";
                using var deleteCmd = new NpgsqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@cid", courseId);
                deleteCmd.Parameters.AddWithValue("@sid", studentId);
                return deleteCmd.ExecuteNonQuery() > 0;
            }
            else
            {
                // Chuyển sang DROPPED nếu đang ACTIVE
                const string updateQuery = "UPDATE ENROLLMENTS SET status = 'DROPPED' WHERE course_id = @cid AND student_id = @sid AND status = 'ACTIVE'";
                using var updateCmd = new NpgsqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@cid", courseId);
                updateCmd.Parameters.AddWithValue("@sid", studentId);
                return updateCmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Lấy danh sách khóa học ACTIVE mà sinh viên CHƯA ghi danh (để hiển thị "Browse Courses").
        /// </summary>
        public List<CourseModel> GetAvailableCourses(int studentId)
        {
            var courses = new List<CourseModel>();
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT c.id, c.name, COALESCE(c.description, '') AS description, c.teacher_id,
                       COALESCE(u.full_name, u.username) AS teacher_name,
                       c.status, c.start_date, c.end_date
                FROM COURSES c
                JOIN USERS u ON c.teacher_id = u.id
                WHERE UPPER(c.status) = 'ACTIVE'
                  AND c.id NOT IN (
                      SELECT course_id FROM ENROLLMENTS
                      WHERE student_id = @student_id AND status IN ('PENDING', 'ACTIVE')
                  )
                ORDER BY c.created_at DESC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@student_id", studentId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                courses.Add(new CourseModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    TeacherId = reader.GetInt32(3),
                    TeacherName = reader.IsDBNull(4) ? "Unknown" : reader.GetString(4),
                    Status = reader.GetString(5),
                    StartDate = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
                    EndDate = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7)
                });
            }

            return courses;
        }

        /// <summary>
        /// Đếm số sinh viên đang tham gia (ACTIVE hoặc PENDING) trong một khóa học.
        /// </summary>
        public int GetEnrolledCount(int courseId)
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = "SELECT COUNT(*) FROM ENROLLMENTS WHERE course_id = @course_id AND status IN ('ACTIVE', 'PENDING')";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);

            return Convert.ToInt32(command.ExecuteScalar());
        }


        // ════════════════════════════════════════════════════════════════
        //  ADMIN / TEACHER ENROLLMENT METHODS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Lấy danh sách các sinh viên đang có trạng thái PENDING để duyệt.
        /// Có thể lọc theo khóa học (courseId) nếu muốn.
        /// </summary>
        public List<EnrollmentModel> GetPendingEnrollments(int? courseId = null)
        {
            var list = new List<EnrollmentModel>();
            using var connection = CreateConnection();
            connection.Open();

            string query = @"
                SELECT e.id, e.course_id, e.student_id, e.status, e.joined_at,
                       c.name AS course_name, COALESCE(u.full_name, u.username) AS teacher_name,
                       c.status AS course_status, c.start_date, c.end_date, COALESCE(c.description, '') AS description,
                       COALESCE(s.full_name, s.username) AS student_name
                FROM ENROLLMENTS e
                JOIN COURSES c ON e.course_id = c.id
                JOIN USERS u ON c.teacher_id = u.id
                JOIN USERS s ON e.student_id = s.id
                WHERE e.status = 'PENDING'";

            if (courseId.HasValue)
            {
                query += " AND e.course_id = @course_id";
            }
            query += " ORDER BY e.joined_at DESC";

            using var command = new NpgsqlCommand(query, connection);
            if (courseId.HasValue)
            {
                command.Parameters.AddWithValue("@course_id", courseId.Value);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new EnrollmentModel
                {
                    Id = reader.GetInt32(0),
                    CourseId = reader.GetInt32(1),
                    StudentId = reader.GetInt32(2),
                    Status = reader.GetString(3),
                    JoinedAt = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                    CourseName = reader.GetString(5),
                    TeacherName = reader.IsDBNull(6) ? "Unknown" : reader.GetString(6),
                    CourseStatus = reader.GetString(7),
                    CourseStartDate = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8),
                    CourseEndDate = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                    CourseDescription = reader.GetString(10),
                    StudentName = reader.GetString(11)
                });
            }

            return list;
        }

        /// <summary>
        /// Duyệt yêu cầu đăng ký của sinh viên (PENDING -> ACTIVE)
        /// </summary>
        public bool ApproveEnrollment(int courseId, int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = "UPDATE ENROLLMENTS SET status = 'ACTIVE' WHERE course_id = @cid AND student_id = @sid AND status = 'PENDING'";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@cid", courseId);
            command.Parameters.AddWithValue("@sid", studentId);

            return command.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Từ chối/Xóa yêu cầu đăng ký của sinh viên (chuyển DROPPED hoặc xóa)
        /// Ở đây ta xóa luôn record nếu là PENDING để họ có thể đăng ký lại, hoặc chuyển sang DROPPED.
        /// </summary>
        public bool RejectEnrollment(int courseId, int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            // Nếu đang PENDING thì XÓA để sinh viên có thể xin lại sau này nếu muốn
            const string query = "DELETE FROM ENROLLMENTS WHERE course_id = @cid AND student_id = @sid";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@cid", courseId);
            command.Parameters.AddWithValue("@sid", studentId);

            return command.ExecuteNonQuery() > 0;
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
            const string query = @"
                WITH updated AS (
                    UPDATE DEVICES
                    SET last_active = CURRENT_TIMESTAMP,
                        ip_address = @ip_address,
                        status = 'ACTIVE'
                    WHERE user_id = @user_id AND device_name = @device_name
                    RETURNING id
                )
                INSERT INTO DEVICES (user_id, device_name, ip_address, status, last_active)
                SELECT @user_id, @device_name, @ip_address, 'ACTIVE', CURRENT_TIMESTAMP
                WHERE NOT EXISTS (SELECT 1 FROM updated)";
            
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@device_name", deviceName ?? "Unknown Device");
            command.Parameters.AddWithValue("@ip_address", ipAddress ?? "Unknown IP");

            command.ExecuteNonQuery();
        }

        private static void EnsureAuditLogSchema(NpgsqlConnection connection)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS AUDIT_LOGS (
                    ID SERIAL PRIMARY KEY,
                    USER_ID INT NULL,
                    ACTION VARCHAR(200),
                    TARGET_TABLE VARCHAR(100),
                    TARGET_ID INT NULL,
                    CREATED_AT TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                ALTER TABLE AUDIT_LOGS ADD COLUMN IF NOT EXISTS DETAILS TEXT;
                ALTER TABLE AUDIT_LOGS ADD COLUMN IF NOT EXISTS IP_ADDRESS VARCHAR(50);";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        public void LogUserActivity(int? userId, string action, string details, string ipAddress)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureAuditLogSchema(connection);

            const string query = @"
                INSERT INTO AUDIT_LOGS (USER_ID, ACTION, TARGET_TABLE, TARGET_ID, DETAILS, IP_ADDRESS, CREATED_AT)
                VALUES (@user_id, @action, 'USERS', @target_id, @details, @ip_address, CURRENT_TIMESTAMP)";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId.HasValue ? (object)userId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@action", action ?? "UNKNOWN");
            command.Parameters.AddWithValue("@target_id", userId.HasValue ? (object)userId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@details", details ?? string.Empty);
            command.Parameters.AddWithValue("@ip_address", ipAddress ?? string.Empty);
            command.ExecuteNonQuery();
        }

        public async Task LogUserActivityAsync(int? userId, string action, string details, string ipAddress, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            EnsureAuditLogSchema(connection);

            const string query = @"
                INSERT INTO AUDIT_LOGS (USER_ID, ACTION, TARGET_TABLE, TARGET_ID, DETAILS, IP_ADDRESS, CREATED_AT)
                VALUES (@user_id, @action, 'USERS', @target_id, @details, @ip_address, CURRENT_TIMESTAMP)";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId.HasValue ? (object)userId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@action", action ?? "UNKNOWN");
            command.Parameters.AddWithValue("@target_id", userId.HasValue ? (object)userId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@details", details ?? string.Empty);
            command.Parameters.AddWithValue("@ip_address", ipAddress ?? string.Empty);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public AdminDashboardMetricsModel GetAdminDashboardMetrics()
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureAuditLogSchema(connection);

            int ScalarInt(string sql)
            {
                using var cmd = new NpgsqlCommand(sql, connection);
                object? value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
            }

            return new AdminDashboardMetricsModel
            {
                TotalUsers = ScalarInt("SELECT COUNT(*) FROM USERS"),
                ActiveCourses = ScalarInt("SELECT COUNT(*) FROM COURSES WHERE UPPER(COALESCE(STATUS, '')) = 'ACTIVE'"),
                PendingRequests = ScalarInt("SELECT COUNT(*) FROM USERS WHERE UPPER(COALESCE(STATUS, '')) = 'PENDING'"),
                TodayLogins = ScalarInt("SELECT COUNT(*) FROM AUDIT_LOGS WHERE ACTION = 'LOGIN' AND CREATED_AT::date = CURRENT_DATE")
            };
        }

        public List<RecentUserActivityModel> GetRecentUserActivities(int limit = 20)
        {
            var result = new List<RecentUserActivityModel>();
            using var connection = CreateConnection();
            connection.Open();
            EnsureAuditLogSchema(connection);

            const string query = @"
                SELECT a.CREATED_AT,
                       COALESCE(u.USERNAME, 'SYSTEM') AS USERNAME,
                       COALESCE(a.ACTION, '') AS ACTION,
                       COALESCE(a.IP_ADDRESS, '') AS IP_ADDRESS,
                       COALESCE(a.DETAILS, '') AS DETAILS
                FROM AUDIT_LOGS a
                LEFT JOIN USERS u ON u.ID = a.USER_ID
                WHERE a.ACTION IN ('LOGIN', 'LOGOUT', 'SIGNUP', 'FORGOT_PASSWORD')
                ORDER BY a.CREATED_AT DESC
                LIMIT @limit";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@limit", limit);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new RecentUserActivityModel
                {
                    CreatedAt = reader.GetDateTime(0),
                    Username = reader.GetString(1),
                    Action = reader.GetString(2),
                    IpAddress = reader.GetString(3),
                    Details = reader.GetString(4)
                });
            }

            return result;
        }

        public async Task LogDeviceActivityAsync(int userId, string deviceName, string ipAddress, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            const string query = @"
                WITH updated AS (
                    UPDATE DEVICES
                    SET last_active = CURRENT_TIMESTAMP,
                        ip_address = @ip_address,
                        status = 'ACTIVE'
                    WHERE user_id = @user_id AND device_name = @device_name
                    RETURNING id
                )
                INSERT INTO DEVICES (user_id, device_name, ip_address, status, last_active)
                SELECT @user_id, @device_name, @ip_address, 'ACTIVE', CURRENT_TIMESTAMP
                WHERE NOT EXISTS (SELECT 1 FROM updated)";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@device_name", deviceName ?? "Unknown Device");
            command.Parameters.AddWithValue("@ip_address", ipAddress ?? "Unknown IP");

            await command.ExecuteNonQueryAsync(cancellationToken);
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

        /// <summary>
        /// Ensures default seed accounts exist in the database.
        /// Safe to call multiple times (uses ON CONFLICT DO NOTHING).
        /// </summary>
        public void EnsureSeedAccounts()
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();

                // Ensure "student" account exists (password: admin123, role: STUDENT)
                const string seedQuery = @"
                    INSERT INTO USERS (username, password_hash, full_name, email, role_id, status)
                    SELECT @username, @password_hash, @full_name, @email, 
                           (SELECT id FROM ROLES WHERE UPPER(name) = 'STUDENT'), 'active'
                    WHERE NOT EXISTS (SELECT 1 FROM USERS WHERE LOWER(username) = LOWER(@username))";

                using var command = new NpgsqlCommand(seedQuery, connection);
                command.Parameters.AddWithValue("@username", "student");
                command.Parameters.AddWithValue("@password_hash", "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9");
                command.Parameters.AddWithValue("@full_name", "Student Test");
                command.Parameters.AddWithValue("@email", "student@courseguard.local");
                command.ExecuteNonQuery();
            }
            catch
            {
                // Silently ignore seed errors to avoid blocking app startup
            }
        }
    }
}