using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CourseGuard.Backend.Config;
using Npgsql;
using CourseGuard.Backend.Models;
using System.Collections.Generic;

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
                             WHERE UPPER(u.status) IN ({statusParams})";
            
            using var command = new NpgsqlCommand(query, connection);
            for (int i = 0; i < statuses.Length; i++)
            {
                command.Parameters.AddWithValue($"@s{i}", statuses[i].ToUpperInvariant());
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

                // If deleting a teacher, their courses must be removed first because
                // COURSES.TEACHER_ID is NOT NULL and FK does not cascade.
                if (roleName == "TEACHER")
                {
                    using var tx = connection.BeginTransaction();

                    try
                    {
                        const string deleteCoursesQuery = "DELETE FROM COURSES WHERE TEACHER_ID = @id";
                        using (var deleteCoursesCmd = new NpgsqlCommand(deleteCoursesQuery, connection, tx))
                        {
                            deleteCoursesCmd.Parameters.AddWithValue("@id", userId);
                            deleteCoursesCmd.ExecuteNonQuery();
                        }

                        const string deleteUserQuery = "DELETE FROM USERS WHERE id = @id";
                        using (var deleteUserCmd = new NpgsqlCommand(deleteUserQuery, connection, tx))
                        {
                            deleteUserCmd.Parameters.AddWithValue("@id", userId);
                            int userAffectedRows = deleteUserCmd.ExecuteNonQuery();
                            tx.Commit();
                            return userAffectedRows > 0;
                        }
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { /* ignore */ }
                        throw;
                    }
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

        public HashSet<int> GetStudentEnrolledCourseIds(int studentId)
        {
            var result = new HashSet<int>();
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT COURSE_ID
                FROM ENROLLMENTS
                WHERE STUDENT_ID = @student_id
                  AND UPPER(COALESCE(STATUS, 'ACTIVE')) IN ('ACTIVE', 'APPROVED')";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@student_id", studentId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetInt32(0));
            }

            return result;
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

        public bool EmailExists(string email)
        {
            using var connection = CreateConnection();
            connection.Open();
            const string query = "SELECT COUNT(*) FROM USERS WHERE LOWER(email) = LOWER(@email)";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@email", email);
            return Convert.ToInt64(command.ExecuteScalar()) > 0;
        }

        public List<(string Username, string Email)> GetAllUsernamesAndEmails()
        {
            var result = new List<(string Username, string Email)>();
            using var connection = CreateConnection();
            connection.Open();
            const string query = "SELECT COALESCE(username, ''), COALESCE(email, '') FROM USERS";
            using var command = new NpgsqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add((reader.GetString(0), reader.GetString(1)));
            }
            return result;
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

        private static void EnsureChatSchema(NpgsqlConnection connection)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS MESSAGES (
                    ID SERIAL PRIMARY KEY,
                    COURSE_ID INT NOT NULL,
                    SENDER_ID INT NOT NULL,
                    CONTENT TEXT,
                    SENT_AT TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (COURSE_ID) REFERENCES COURSES(ID) ON DELETE CASCADE,
                    FOREIGN KEY (SENDER_ID) REFERENCES USERS(ID)
                );
                ALTER TABLE MESSAGES ADD COLUMN IF NOT EXISTS MESSAGE_TYPE VARCHAR(20) NOT NULL DEFAULT 'TEXT';
                ALTER TABLE MESSAGES ADD COLUMN IF NOT EXISTS FILE_URL TEXT;
                ALTER TABLE MESSAGES ADD COLUMN IF NOT EXISTS FILE_NAME VARCHAR(255);
                ALTER TABLE MESSAGES ADD COLUMN IF NOT EXISTS FILE_SIZE BIGINT;
                ALTER TABLE MESSAGES ADD COLUMN IF NOT EXISTS MIME_TYPE VARCHAR(100);
                ALTER TABLE MESSAGES ADD COLUMN IF NOT EXISTS REPLY_TO_ID INT NULL;
                ALTER TABLE MESSAGES ADD COLUMN IF NOT EXISTS IS_DELETED BOOLEAN NOT NULL DEFAULT FALSE;
                CREATE INDEX IF NOT EXISTS IDX_MESSAGES_COURSE_SENT_AT ON MESSAGES(COURSE_ID, SENT_AT DESC);
                CREATE INDEX IF NOT EXISTS IDX_MESSAGES_SENDER_ID ON MESSAGES(SENDER_ID);
                CREATE TABLE IF NOT EXISTS CHAT_READS (
                    USER_ID INT NOT NULL,
                    COURSE_ID INT NOT NULL,
                    LAST_READ_MESSAGE_ID INT NULL,
                    UPDATED_AT TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (USER_ID, COURSE_ID),
                    FOREIGN KEY (USER_ID) REFERENCES USERS(ID) ON DELETE CASCADE,
                    FOREIGN KEY (COURSE_ID) REFERENCES COURSES(ID) ON DELETE CASCADE,
                    FOREIGN KEY (LAST_READ_MESSAGE_ID) REFERENCES MESSAGES(ID) ON DELETE SET NULL
                );";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        public List<ChatCourseModel> GetChatCoursesForUser(int userId)
        {
            var result = new List<ChatCourseModel>();
            using var connection = CreateConnection();
            connection.Open();
            EnsureChatSchema(connection);

            const string query = @"
                SELECT c.ID,
                       COALESCE(c.NAME, '') AS COURSE_NAME,
                       COALESCE(c.CLASS_CODE, '') AS CLASS_CODE,
                       (c.TEACHER_ID = @user_id) AS IS_TEACHER
                FROM COURSES c
                LEFT JOIN ENROLLMENTS e ON e.COURSE_ID = c.ID AND e.STUDENT_ID = @user_id
                WHERE c.TEACHER_ID = @user_id
                   OR (e.STUDENT_ID IS NOT NULL AND UPPER(COALESCE(e.STATUS, 'ACTIVE')) IN ('ACTIVE', 'APPROVED'))
                ORDER BY c.ID DESC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new ChatCourseModel
                {
                    CourseId = reader.GetInt32(0),
                    CourseName = reader.GetString(1),
                    ClassCode = reader.GetString(2),
                    IsTeacherCourse = reader.GetBoolean(3)
                });
            }

            return result;
        }

        public async Task<List<ChatCourseModel>> GetChatCoursesForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var result = new List<ChatCourseModel>();
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            EnsureChatSchema(connection);

            const string query = @"
                SELECT c.ID,
                       COALESCE(c.NAME, '') AS COURSE_NAME,
                       COALESCE(c.CLASS_CODE, '') AS CLASS_CODE,
                       (c.TEACHER_ID = @user_id) AS IS_TEACHER
                FROM COURSES c
                LEFT JOIN ENROLLMENTS e ON e.COURSE_ID = c.ID AND e.STUDENT_ID = @user_id
                WHERE c.TEACHER_ID = @user_id
                   OR (e.STUDENT_ID IS NOT NULL AND UPPER(COALESCE(e.STATUS, 'ACTIVE')) IN ('ACTIVE', 'APPROVED'))
                ORDER BY c.ID DESC";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new ChatCourseModel
                {
                    CourseId = reader.GetInt32(0),
                    CourseName = reader.GetString(1),
                    ClassCode = reader.GetString(2),
                    IsTeacherCourse = reader.GetBoolean(3)
                });
            }

            return result;
        }

        public bool CanAccessCourseChat(int userId, int courseId)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureChatSchema(connection);

            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM COURSES c
                    LEFT JOIN ENROLLMENTS e ON e.COURSE_ID = c.ID AND e.STUDENT_ID = @user_id
                    WHERE c.ID = @course_id
                      AND (
                          c.TEACHER_ID = @user_id
                          OR (e.STUDENT_ID IS NOT NULL AND UPPER(COALESCE(e.STATUS, 'ACTIVE')) IN ('ACTIVE', 'APPROVED'))
                      )
                ) THEN 1 ELSE 0 END";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@course_id", courseId);
            return Convert.ToInt32(command.ExecuteScalar()) == 1;
        }

        public int SendChatMessage(int courseId, int senderId, string content)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureChatSchema(connection);

            const string query = @"
                INSERT INTO MESSAGES (COURSE_ID, SENDER_ID, CONTENT, MESSAGE_TYPE, SENT_AT)
                VALUES (@course_id, @sender_id, @content, 'TEXT', CURRENT_TIMESTAMP)
                RETURNING ID";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@sender_id", senderId);
            command.Parameters.AddWithValue("@content", content);
            object? result = command.ExecuteScalar();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public int SendChatFileMessage(int courseId, int senderId, string content, string fileUrl, string fileName, long fileSize, string mimeType)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureChatSchema(connection);

            const string query = @"
                INSERT INTO MESSAGES (COURSE_ID, SENDER_ID, CONTENT, MESSAGE_TYPE, FILE_URL, FILE_NAME, FILE_SIZE, MIME_TYPE, SENT_AT)
                VALUES (@course_id, @sender_id, @content, 'FILE', @file_url, @file_name, @file_size, @mime_type, CURRENT_TIMESTAMP)
                RETURNING ID";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@sender_id", senderId);
            command.Parameters.AddWithValue("@content", content ?? string.Empty);
            command.Parameters.AddWithValue("@file_url", fileUrl ?? string.Empty);
            command.Parameters.AddWithValue("@file_name", fileName ?? string.Empty);
            command.Parameters.AddWithValue("@file_size", fileSize);
            command.Parameters.AddWithValue("@mime_type", mimeType ?? string.Empty);
            object? result = command.ExecuteScalar();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public async Task<int> SendChatMessageAsync(int courseId, int senderId, string content, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            EnsureChatSchema(connection);

            const string query = @"
                INSERT INTO MESSAGES (COURSE_ID, SENDER_ID, CONTENT, MESSAGE_TYPE, SENT_AT)
                VALUES (@course_id, @sender_id, @content, 'TEXT', CURRENT_TIMESTAMP)
                RETURNING ID";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@sender_id", senderId);
            command.Parameters.AddWithValue("@content", content);
            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public async Task<int> SendChatFileMessageAsync(
            int courseId,
            int senderId,
            string content,
            string fileUrl,
            string fileName,
            long fileSize,
            string mimeType,
            CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            EnsureChatSchema(connection);

            const string query = @"
                INSERT INTO MESSAGES (COURSE_ID, SENDER_ID, CONTENT, MESSAGE_TYPE, FILE_URL, FILE_NAME, FILE_SIZE, MIME_TYPE, SENT_AT)
                VALUES (@course_id, @sender_id, @content, 'FILE', @file_url, @file_name, @file_size, @mime_type, CURRENT_TIMESTAMP)
                RETURNING ID";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@sender_id", senderId);
            command.Parameters.AddWithValue("@content", content ?? string.Empty);
            command.Parameters.AddWithValue("@file_url", fileUrl ?? string.Empty);
            command.Parameters.AddWithValue("@file_name", fileName ?? string.Empty);
            command.Parameters.AddWithValue("@file_size", fileSize);
            command.Parameters.AddWithValue("@mime_type", mimeType ?? string.Empty);
            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public List<ChatMessageModel> GetChatMessages(int courseId, int limit = 100)
        {
            var result = new List<ChatMessageModel>();
            using var connection = CreateConnection();
            connection.Open();
            EnsureChatSchema(connection);

            const string query = @"
                SELECT m.ID,
                       m.COURSE_ID,
                       m.SENDER_ID,
                       COALESCE(u.FULL_NAME, u.USERNAME, 'Unknown') AS SENDER_NAME,
                       COALESCE(r.NAME, '') AS SENDER_ROLE,
                       COALESCE(m.CONTENT, '') AS CONTENT,
                       COALESCE(m.MESSAGE_TYPE, 'TEXT') AS MESSAGE_TYPE,
                       COALESCE(m.FILE_URL, '') AS FILE_URL,
                       COALESCE(m.FILE_NAME, '') AS FILE_NAME,
                       COALESCE(m.FILE_SIZE, 0) AS FILE_SIZE,
                       COALESCE(m.MIME_TYPE, '') AS MIME_TYPE,
                       m.SENT_AT
                FROM MESSAGES m
                JOIN USERS u ON u.ID = m.SENDER_ID
                LEFT JOIN ROLES r ON r.ID = u.ROLE_ID
                WHERE m.COURSE_ID = @course_id
                  AND COALESCE(m.IS_DELETED, FALSE) = FALSE
                ORDER BY m.SENT_AT ASC
                LIMIT @limit";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@limit", limit <= 0 ? 100 : limit);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new ChatMessageModel
                {
                    Id = reader.GetInt32(0),
                    CourseId = reader.GetInt32(1),
                    SenderId = reader.GetInt32(2),
                    SenderName = reader.GetString(3),
                    SenderRole = reader.GetString(4),
                    Content = reader.GetString(5),
                    MessageType = reader.GetString(6),
                    FileUrl = reader.GetString(7),
                    FileName = reader.GetString(8),
                    FileSize = reader.GetInt64(9),
                    MimeType = reader.GetString(10),
                    SentAt = reader.GetDateTime(11)
                });
            }

            return result;
        }

        public async Task<List<ChatMessageModel>> GetChatMessagesAsync(int courseId, int limit = 100, CancellationToken cancellationToken = default)
        {
            var result = new List<ChatMessageModel>();
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            EnsureChatSchema(connection);

            const string query = @"
                SELECT m.ID,
                       m.COURSE_ID,
                       m.SENDER_ID,
                       COALESCE(u.FULL_NAME, u.USERNAME, 'Unknown') AS SENDER_NAME,
                       COALESCE(r.NAME, '') AS SENDER_ROLE,
                       COALESCE(m.CONTENT, '') AS CONTENT,
                       COALESCE(m.MESSAGE_TYPE, 'TEXT') AS MESSAGE_TYPE,
                       COALESCE(m.FILE_URL, '') AS FILE_URL,
                       COALESCE(m.FILE_NAME, '') AS FILE_NAME,
                       COALESCE(m.FILE_SIZE, 0) AS FILE_SIZE,
                       COALESCE(m.MIME_TYPE, '') AS MIME_TYPE,
                       m.SENT_AT
                FROM MESSAGES m
                JOIN USERS u ON u.ID = m.SENDER_ID
                LEFT JOIN ROLES r ON r.ID = u.ROLE_ID
                WHERE m.COURSE_ID = @course_id
                  AND COALESCE(m.IS_DELETED, FALSE) = FALSE
                ORDER BY m.SENT_AT ASC
                LIMIT @limit";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@limit", limit <= 0 ? 100 : limit);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new ChatMessageModel
                {
                    Id = reader.GetInt32(0),
                    CourseId = reader.GetInt32(1),
                    SenderId = reader.GetInt32(2),
                    SenderName = reader.GetString(3),
                    SenderRole = reader.GetString(4),
                    Content = reader.GetString(5),
                    MessageType = reader.GetString(6),
                    FileUrl = reader.GetString(7),
                    FileName = reader.GetString(8),
                    FileSize = reader.GetInt64(9),
                    MimeType = reader.GetString(10),
                    SentAt = reader.GetDateTime(11)
                });
            }

            return result;
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

        private static void EnsureLoginDailyStatsSchema(NpgsqlConnection connection)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS LOGIN_DAILY_STATS (
                    STAT_DATE DATE PRIMARY KEY,
                    LOGIN_COUNT INT NOT NULL DEFAULT 0,
                    UPDATED_AT TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
                CREATE INDEX IF NOT EXISTS IDX_LOGIN_DAILY_STATS_DATE
                    ON LOGIN_DAILY_STATS (STAT_DATE DESC);";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        private static void UpsertTodayLoginStats(NpgsqlConnection connection)
        {
            const string sql = @"
                INSERT INTO LOGIN_DAILY_STATS (STAT_DATE, LOGIN_COUNT, UPDATED_AT)
                VALUES (CURRENT_DATE, 1, CURRENT_TIMESTAMP)
                ON CONFLICT (STAT_DATE)
                DO UPDATE SET LOGIN_COUNT = LOGIN_DAILY_STATS.LOGIN_COUNT + 1,
                              UPDATED_AT = CURRENT_TIMESTAMP";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        public void LogUserActivity(int? userId, string action, string details, string ipAddress)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureAuditLogSchema(connection);
            EnsureLoginDailyStatsSchema(connection);

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

            if (string.Equals(action, "LOGIN", StringComparison.OrdinalIgnoreCase))
            {
                UpsertTodayLoginStats(connection);
            }
        }

        public async Task LogUserActivityAsync(int? userId, string action, string details, string ipAddress, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            EnsureAuditLogSchema(connection);
            EnsureLoginDailyStatsSchema(connection);

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

            if (string.Equals(action, "LOGIN", StringComparison.OrdinalIgnoreCase))
            {
                UpsertTodayLoginStats(connection);
            }
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
                WHERE a.ACTION IN (
                    'LOGIN', 'LOGOUT', 'SIGNUP', 'FORGOT_PASSWORD', 'CHANGE_PASSWORD',
                    'COURSE_ENROLL_REQUEST', 'COURSE_ENROLL', 'ONLINE_SESSION_JOIN', 'ONLINE_SESSION_EXIT',
                    'EXAM_JOIN', 'EXAM_SUBMIT', 'EXAM_EXIT',
                    'ADMIN_ADD_USER', 'ADMIN_DELETE_USER', 'ADMIN_APPROVE_REGISTRATION',
                    'ADMIN_REJECT_USER_REQUEST', 'ADMIN_RESET_PASSWORD', 'FORGOT_PASSWORD_APPROVED'
                )
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

        public List<LoginFrequencyModel> GetLoginFrequencyStats(int days = 7)
        {
            var result = new List<LoginFrequencyModel>();
            int safeDays = days <= 0 ? 7 : days;

            using var connection = CreateConnection();
            connection.Open();
            EnsureLoginDailyStatsSchema(connection);

            const string query = @"
                SELECT STAT_DATE, LOGIN_COUNT
                FROM LOGIN_DAILY_STATS
                WHERE STAT_DATE >= CURRENT_DATE - (@days - 1)
                ORDER BY STAT_DATE ASC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@days", safeDays);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new LoginFrequencyModel
                {
                    LoginDate = reader.GetDateTime(0),
                    LoginCount = reader.GetInt32(1)
                });
            }

            return result;
        }

        public async Task<List<LoginFrequencyModel>> GetLoginFrequencyStatsAsync(int days = 7, CancellationToken cancellationToken = default)
        {
            var result = new List<LoginFrequencyModel>();
            int safeDays = days <= 0 ? 7 : days;

            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            EnsureLoginDailyStatsSchema(connection);

            const string query = @"
                SELECT STAT_DATE, LOGIN_COUNT
                FROM LOGIN_DAILY_STATS
                WHERE STAT_DATE >= CURRENT_DATE - (@days - 1)
                ORDER BY STAT_DATE ASC";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@days", safeDays);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new LoginFrequencyModel
                {
                    LoginDate = reader.GetDateTime(0),
                    LoginCount = reader.GetInt32(1)
                });
            }

            return result;
        }

        public List<CourseListItemModel> GetCourseListItems(int limit = 100)
        {
            var result = new List<CourseListItemModel>();
            int safeLimit = limit <= 0 ? 100 : limit;

            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT c.ID,
                       COALESCE(c.NAME, '') AS COURSE_NAME,
                       COALESCE(u.FULL_NAME, u.USERNAME, 'Unknown') AS TEACHER_NAME,
                       COALESCE(c.STATUS, 'UNKNOWN') AS COURSE_STATUS,
                       COUNT(e.ID)::int AS ENROLLMENT_COUNT
                FROM COURSES c
                LEFT JOIN USERS u ON u.ID = c.TEACHER_ID
                LEFT JOIN ENROLLMENTS e ON e.COURSE_ID = c.ID
                GROUP BY c.ID, c.NAME, u.FULL_NAME, u.USERNAME, c.STATUS
                ORDER BY c.ID DESC
                LIMIT @limit";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@limit", safeLimit);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new CourseListItemModel
                {
                    CourseId = reader.GetInt32(0),
                    CourseName = reader.GetString(1),
                    TeacherName = reader.GetString(2),
                    Status = reader.GetString(3),
                    EnrollmentCount = reader.GetInt32(4)
                });
            }

            return result;
        }

        public async Task<List<CourseListItemModel>> GetCourseListItemsAsync(int limit = 100, CancellationToken cancellationToken = default)
        {
            var result = new List<CourseListItemModel>();
            int safeLimit = limit <= 0 ? 100 : limit;

            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string query = @"
                SELECT c.ID,
                       COALESCE(c.NAME, '') AS COURSE_NAME,
                       COALESCE(u.FULL_NAME, u.USERNAME, 'Unknown') AS TEACHER_NAME,
                       COALESCE(c.STATUS, 'UNKNOWN') AS COURSE_STATUS,
                       COUNT(e.ID)::int AS ENROLLMENT_COUNT
                FROM COURSES c
                LEFT JOIN USERS u ON u.ID = c.TEACHER_ID
                LEFT JOIN ENROLLMENTS e ON e.COURSE_ID = c.ID
                GROUP BY c.ID, c.NAME, u.FULL_NAME, u.USERNAME, c.STATUS
                ORDER BY c.ID DESC
                LIMIT @limit";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@limit", safeLimit);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new CourseListItemModel
                {
                    CourseId = reader.GetInt32(0),
                    CourseName = reader.GetString(1),
                    TeacherName = reader.GetString(2),
                    Status = reader.GetString(3),
                    EnrollmentCount = reader.GetInt32(4)
                });
            }

            return result;
        }

        public AccountSummaryModel GetAccountSummary()
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT
                    COUNT(*)::int AS TOTAL_ACCOUNTS,
                    COUNT(*) FILTER (WHERE UPPER(COALESCE(u.STATUS, '')) = 'ACTIVE')::int AS ACTIVE_ACCOUNTS,
                    COUNT(*) FILTER (WHERE UPPER(COALESCE(u.STATUS, '')) = 'PENDING')::int AS PENDING_ACCOUNTS,
                    COUNT(*) FILTER (WHERE UPPER(COALESCE(r.NAME, '')) = 'TEACHER')::int AS TEACHER_ACCOUNTS,
                    COUNT(*) FILTER (WHERE UPPER(COALESCE(r.NAME, '')) = 'STUDENT')::int AS STUDENT_ACCOUNTS
                FROM USERS u
                LEFT JOIN ROLES r ON r.ID = u.ROLE_ID";

            using var command = new NpgsqlCommand(query, connection);
            using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
            if (reader.Read())
            {
                return new AccountSummaryModel
                {
                    TotalAccounts = reader.GetInt32(0),
                    ActiveAccounts = reader.GetInt32(1),
                    PendingAccounts = reader.GetInt32(2),
                    TeacherAccounts = reader.GetInt32(3),
                    StudentAccounts = reader.GetInt32(4)
                };
            }

            return new AccountSummaryModel();
        }

        public async Task<AccountSummaryModel> GetAccountSummaryAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string query = @"
                SELECT
                    COUNT(*)::int AS TOTAL_ACCOUNTS,
                    COUNT(*) FILTER (WHERE UPPER(COALESCE(u.STATUS, '')) = 'ACTIVE')::int AS ACTIVE_ACCOUNTS,
                    COUNT(*) FILTER (WHERE UPPER(COALESCE(u.STATUS, '')) = 'PENDING')::int AS PENDING_ACCOUNTS,
                    COUNT(*) FILTER (WHERE UPPER(COALESCE(r.NAME, '')) = 'TEACHER')::int AS TEACHER_ACCOUNTS,
                    COUNT(*) FILTER (WHERE UPPER(COALESCE(r.NAME, '')) = 'STUDENT')::int AS STUDENT_ACCOUNTS
                FROM USERS u
                LEFT JOIN ROLES r ON r.ID = u.ROLE_ID";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new AccountSummaryModel
                {
                    TotalAccounts = reader.GetInt32(0),
                    ActiveAccounts = reader.GetInt32(1),
                    PendingAccounts = reader.GetInt32(2),
                    TeacherAccounts = reader.GetInt32(3),
                    StudentAccounts = reader.GetInt32(4)
                };
            }

            return new AccountSummaryModel();
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
            
            if (status != "ALL") query += " AND UPPER(u.status) = UPPER(@status)";
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
                           (SELECT id FROM ROLES WHERE UPPER(name) = 'STUDENT'), 'ACTIVE'
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