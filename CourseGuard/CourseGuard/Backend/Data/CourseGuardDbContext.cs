using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CourseGuard.Backend.Config;
using Npgsql;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services;
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

        public void EnsureCourseWorkflowSchema()
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = new NpgsqlCommand(@"
                ALTER TABLE courses
                    ADD COLUMN IF NOT EXISTS rejection_reason TEXT;

                ALTER TABLE notifications
                    ADD COLUMN IF NOT EXISTS category VARCHAR(32) NOT NULL DEFAULT 'SystemAdmin',
                    ADD COLUMN IF NOT EXISTS notification_type VARCHAR(32) NOT NULL DEFAULT 'Informational',
                    ADD COLUMN IF NOT EXISTS source_type VARCHAR(64),
                    ADD COLUMN IF NOT EXISTS source_id INT;", connection);
            command.ExecuteNonQuery();
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

        private static void EnsureStudentProfileSchema(NpgsqlConnection connection)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS student_profiles (
                    user_id INTEGER PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
                    phone TEXT,
                    address TEXT,
                    major TEXT,
                    gender TEXT,
                    birth_date DATE,
                    bio TEXT,
                    avatar_path TEXT,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

            using var command = new NpgsqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public StudentProfileModel? GetStudentProfile(int userId)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureStudentProfileSchema(connection);

            const string query = @"
                SELECT u.id,
                       COALESCE(u.full_name, ''),
                       COALESCE(u.email, ''),
                       COALESCE(sp.phone, ''),
                       COALESCE(sp.address, ''),
                       COALESCE(sp.major, ''),
                       COALESCE(sp.gender, ''),
                       sp.birth_date,
                       COALESCE(sp.bio, ''),
                       COALESCE(sp.avatar_path, '')
                FROM users u
                LEFT JOIN student_profiles sp ON sp.user_id = u.id
                WHERE u.id = @user_id";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId);

            using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
            if (!reader.Read())
                return null;

            return new StudentProfileModel
            {
                UserId = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Email = reader.GetString(2),
                Phone = reader.GetString(3),
                Address = reader.GetString(4),
                Major = reader.GetString(5),
                Gender = reader.GetString(6),
                BirthDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                Bio = reader.GetString(8),
                AvatarPath = reader.GetString(9)
            };
        }

        public bool UpsertStudentProfile(StudentProfileModel profile)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureStudentProfileSchema(connection);

            using var transaction = connection.BeginTransaction();
            try
            {
                const string updateUserSql = @"
                    UPDATE users
                    SET full_name = @full_name,
                        email = @email
                    WHERE id = @user_id";

                using (var updateUser = new NpgsqlCommand(updateUserSql, connection, transaction))
                {
                    updateUser.Parameters.AddWithValue("@user_id", profile.UserId);
                    updateUser.Parameters.AddWithValue("@full_name", string.IsNullOrWhiteSpace(profile.FullName) ? DBNull.Value : profile.FullName);
                    updateUser.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(profile.Email) ? DBNull.Value : profile.Email);
                    updateUser.ExecuteNonQuery();
                }

                const string upsertProfileSql = @"
                    INSERT INTO student_profiles (user_id, phone, address, major, gender, birth_date, bio, avatar_path, updated_at)
                    VALUES (@user_id, @phone, @address, @major, @gender, @birth_date, @bio, @avatar_path, CURRENT_TIMESTAMP)
                    ON CONFLICT (user_id)
                    DO UPDATE SET phone = EXCLUDED.phone,
                                  address = EXCLUDED.address,
                                  major = EXCLUDED.major,
                                  gender = EXCLUDED.gender,
                                  birth_date = EXCLUDED.birth_date,
                                  bio = EXCLUDED.bio,
                                  avatar_path = EXCLUDED.avatar_path,
                                  updated_at = CURRENT_TIMESTAMP";

                using (var upsertProfile = new NpgsqlCommand(upsertProfileSql, connection, transaction))
                {
                    upsertProfile.Parameters.AddWithValue("@user_id", profile.UserId);
                    upsertProfile.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(profile.Phone) ? DBNull.Value : profile.Phone);
                    upsertProfile.Parameters.AddWithValue("@address", string.IsNullOrWhiteSpace(profile.Address) ? DBNull.Value : profile.Address);
                    upsertProfile.Parameters.AddWithValue("@major", string.IsNullOrWhiteSpace(profile.Major) ? DBNull.Value : profile.Major);
                    upsertProfile.Parameters.AddWithValue("@gender", string.IsNullOrWhiteSpace(profile.Gender) ? DBNull.Value : profile.Gender);
                    upsertProfile.Parameters.AddWithValue("@birth_date", profile.BirthDate.HasValue ? profile.BirthDate.Value.Date : DBNull.Value);
                    upsertProfile.Parameters.AddWithValue("@bio", string.IsNullOrWhiteSpace(profile.Bio) ? DBNull.Value : profile.Bio);
                    upsertProfile.Parameters.AddWithValue("@avatar_path", string.IsNullOrWhiteSpace(profile.AvatarPath) ? DBNull.Value : profile.AvatarPath);
                    upsertProfile.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static string GetStudentCodeFromUserId(int userId)
        {
            return $"HS{userId:00000}";
        }

        private static void EnsureStudentScoreSchema(NpgsqlConnection connection)
        {
            const string createTable = @"
                CREATE TABLE IF NOT EXISTS public.student_scores (
                    id      BIGSERIAL PRIMARY KEY,
                    mssv    TEXT   NOT NULL UNIQUE,
                    ho_ten  TEXT   NOT NULL DEFAULT '',
                    lop     TEXT   NOT NULL DEFAULT '',
                    diem_gk FLOAT8 NOT NULL DEFAULT 0,
                    diem_ck FLOAT8 NOT NULL DEFAULT 0
                );";

            using var command = new NpgsqlCommand(createTable, connection);
            command.ExecuteNonQuery();
        }

        private static bool TableExists(NpgsqlConnection connection, string tableName)
        {
            const string query = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND LOWER(table_name) = LOWER(@table_name)
                )";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@table_name", tableName);
            object? result = command.ExecuteScalar();
            return result is bool exists && exists;
        }

        public int CountActiveEnrollments(int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT COUNT(*)
                FROM enrollments
                WHERE student_id = @student_id
                  AND UPPER(status) IN ('ACTIVE', 'APPROVED')";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@student_id", studentId);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public int CountUnreadNotifications(int userId)
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT COUNT(*)
                FROM notifications
                WHERE user_id = @user_id
                  AND is_read = false";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public int CountStudentScoreRecords(int userId)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureStudentScoreSchema(connection);

            const string query = @"
                SELECT COUNT(*)
                FROM public.student_scores
                WHERE mssv = @mssv";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@mssv", GetStudentCodeFromUserId(userId));
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public double? GetStudentAverageScore(int userId)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureStudentScoreSchema(connection);

            const string query = @"
                SELECT AVG((diem_gk * 0.3) + (diem_ck * 0.7))
                FROM public.student_scores
                WHERE mssv = @mssv";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@mssv", GetStudentCodeFromUserId(userId));
            object? result = command.ExecuteScalar();
            return result == null || result == DBNull.Value ? null : Convert.ToDouble(result);
        }

        public double? GetStudentExamAverageScore(int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            if (TableExists(connection, "exam_attempts"))
            {
                const string attemptAverageQuery = @"
                    SELECT AVG(score)
                    FROM exam_attempts
                    WHERE student_id = @student_id
                      AND score IS NOT NULL";

                using var attemptCommand = new NpgsqlCommand(attemptAverageQuery, connection);
                attemptCommand.Parameters.AddWithValue("@student_id", studentId);
                object? attemptAverage = attemptCommand.ExecuteScalar();
                if (attemptAverage != null && attemptAverage != DBNull.Value)
                    return Convert.ToDouble(attemptAverage);
            }

            EnsureStudentScoreSchema(connection);
            const string scoreAverageQuery = @"
                SELECT AVG((diem_gk * 0.3) + (diem_ck * 0.7))
                FROM public.student_scores
                WHERE mssv = @mssv";

            using var scoreCommand = new NpgsqlCommand(scoreAverageQuery, connection);
            scoreCommand.Parameters.AddWithValue("@mssv", GetStudentCodeFromUserId(studentId));
            object? scoreAverage = scoreCommand.ExecuteScalar();
            return scoreAverage == null || scoreAverage == DBNull.Value ? null : Convert.ToDouble(scoreAverage);
        }

        public int CountOpenExamsForStudent(int studentId)
        {
            return CountStudentExamsByAvailability(studentId, openNowOnly: true);
        }

        public int CountAvailableExamsForStudent(int studentId)
        {
            return CountStudentExamsByAvailability(studentId, openNowOnly: false);
        }

        public int CountCompletedExamsForStudent(int studentId)
        {
            using var connection = CreateConnection();
            connection.Open();

            if (TableExists(connection, "exam_attempts"))
            {
                const string attemptQuery = @"
                    SELECT COUNT(*)
                    FROM exam_attempts
                    WHERE student_id = @student_id
                      AND score IS NOT NULL";

                using var attemptCommand = new NpgsqlCommand(attemptQuery, connection);
                attemptCommand.Parameters.AddWithValue("@student_id", studentId);
                int attemptCount = Convert.ToInt32(attemptCommand.ExecuteScalar());
                if (attemptCount > 0)
                    return attemptCount;
            }

            EnsureStudentScoreSchema(connection);
            const string scoreQuery = @"
                SELECT COUNT(*)
                FROM public.student_scores
                WHERE mssv = @mssv";

            using var scoreCommand = new NpgsqlCommand(scoreQuery, connection);
            scoreCommand.Parameters.AddWithValue("@mssv", GetStudentCodeFromUserId(studentId));
            return Convert.ToInt32(scoreCommand.ExecuteScalar());
        }

        public List<StudentExamListItemModel> GetAvailableExamsForStudent(int studentId, int limit = 100)
        {
            var result = new List<StudentExamListItemModel>();
            int safeLimit = limit <= 0 ? 100 : limit;

            using var connection = CreateConnection();
            connection.Open();

            if (!TableExists(connection, "exams") || !TableExists(connection, "enrollments"))
                return result;

            bool hasAttempts = TableExists(connection, "exam_attempts");
            bool hasQuestions = TableExists(connection, "exam_questions");

            string attemptsExpression = hasAttempts
                ? @"(
                    SELECT COUNT(*)
                    FROM exam_attempts ea
                    WHERE ea.exam_id = ex.id
                      AND ea.student_id = @student_id
                )"
                : "0";

            string inProgressAttemptsExpression = hasAttempts
                ? @"(
                    SELECT COUNT(*)
                    FROM exam_attempts ea
                    WHERE ea.exam_id = ex.id
                      AND ea.student_id = @student_id
                      AND UPPER(COALESCE(ea.status, '')) = 'IN_PROGRESS'
                )"
                : "0";

            string questionExpression = hasQuestions
                ? @"(
                    SELECT COUNT(*)
                    FROM exam_questions eq
                    WHERE eq.exam_id = ex.id
                )"
                : "0";

            string query = $@"
                SELECT ex.id,
                       ex.course_id,
                       COALESCE(ex.title, '') AS title,
                       COALESCE(c.name, '') AS course_name,
                       ex.open_time,
                       ex.close_time,
                       COALESCE(ex.duration_minutes, 0) AS duration_minutes,
                       COALESCE(ex.max_attempts, 1) AS max_attempts,
                       {attemptsExpression}::int AS attempt_count,
                       {inProgressAttemptsExpression}::int AS in_progress_attempt_count,
                       {questionExpression}::int AS question_count
                FROM exams ex
                JOIN enrollments en ON en.course_id = ex.course_id
                LEFT JOIN courses c ON c.id = ex.course_id
                WHERE en.student_id = @student_id
                  AND UPPER(COALESCE(en.status, 'ACTIVE')) IN ('ACTIVE', 'APPROVED')
                  AND UPPER(COALESCE(c.status, 'ACTIVE')) IN ('ACTIVE', 'APPROVED', 'OPEN')
                  AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'ACTIVE'
                ORDER BY
                  CASE
                    WHEN (ex.open_time IS NULL OR ex.open_time <= CURRENT_TIMESTAMP)
                     AND (ex.close_time IS NULL OR ex.close_time >= CURRENT_TIMESTAMP) THEN 0
                    WHEN ex.open_time > CURRENT_TIMESTAMP THEN 1
                    ELSE 2
                  END,
                  ex.open_time NULLS FIRST,
                  ex.close_time NULLS LAST
                LIMIT @limit";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@student_id", studentId);
            command.Parameters.AddWithValue("@limit", safeLimit);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new StudentExamListItemModel
                {
                    Id = reader.GetInt32(0),
                    CourseId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    CourseName = reader.GetString(3),
                    OpenTime = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    CloseTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    DurationMinutes = reader.GetInt32(6),
                    MaxAttempts = reader.GetInt32(7),
                    AttemptCount = reader.GetInt32(8),
                    InProgressAttemptCount = reader.GetInt32(9),
                    QuestionCount = reader.GetInt32(10)
                });
            }

            return result;
        }

        public StudentExamTakingModel? StartOrResumeStudentExam(int studentId, int examId)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureStudentExamTakingSchema(connection);
            if (!TableExists(connection, "exam_attempts") || !TableExists(connection, "exam_questions"))
                return null;

            using var transaction = connection.BeginTransaction();
            int attemptId = GetExistingInProgressAttempt(connection, transaction, studentId, examId);
            if (attemptId <= 0)
            {
                if (!CanStartStudentExam(connection, transaction, studentId, examId))
                    return null;

                using var insert = new NpgsqlCommand(@"
                    INSERT INTO exam_attempts (exam_id, student_id, start_time, status)
                    VALUES (@exam_id, @student_id, CURRENT_TIMESTAMP, 'IN_PROGRESS')
                    RETURNING id", connection, transaction);
                insert.Parameters.AddWithValue("@exam_id", examId);
                insert.Parameters.AddWithValue("@student_id", studentId);
                attemptId = Convert.ToInt32(insert.ExecuteScalar());
            }

            StudentExamTakingModel? session = LoadStudentExamSession(connection, transaction, studentId, attemptId);
            transaction.Commit();
            return session;
        }

        public bool SaveStudentExamAnswer(int studentId, int attemptId, int examQuestionId, string selectedOption)
        {
            string option = NormalizeOption(selectedOption);
            if (string.IsNullOrEmpty(option))
                return false;

            using var connection = CreateConnection();
            connection.Open();
            EnsureStudentExamTakingSchema(connection);

            using var command = new NpgsqlCommand(@"
                INSERT INTO exam_attempt_answers (attempt_id, exam_question_id, selected_option, answered_at)
                SELECT @attempt_id, @exam_question_id, @selected_option, CURRENT_TIMESTAMP
                WHERE EXISTS (
                    SELECT 1
                    FROM exam_attempts a
                    JOIN exam_questions eq ON eq.exam_id = a.exam_id
                    WHERE a.id = @attempt_id
                      AND a.student_id = @student_id
                      AND UPPER(COALESCE(a.status, '')) = 'IN_PROGRESS'
                      AND eq.id = @exam_question_id
                )
                ON CONFLICT (attempt_id, exam_question_id)
                DO UPDATE SET selected_option = EXCLUDED.selected_option, answered_at = CURRENT_TIMESTAMP", connection);
            command.Parameters.AddWithValue("@student_id", studentId);
            command.Parameters.AddWithValue("@attempt_id", attemptId);
            command.Parameters.AddWithValue("@exam_question_id", examQuestionId);
            command.Parameters.AddWithValue("@selected_option", option);
            return command.ExecuteNonQuery() > 0;
        }

        public StudentExamSubmitResultModel SubmitStudentExamAttempt(int studentId, int attemptId)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureStudentExamTakingSchema(connection);
            using var transaction = connection.BeginTransaction();

            if (!OwnsInProgressAttempt(connection, transaction, studentId, attemptId))
            {
                return new StudentExamSubmitResultModel
                {
                    Success = false,
                    Message = "Không tìm thấy lượt làm bài đang mở."
                };
            }

            var questions = new List<TeacherExamQuestionModel>();
            using (var questionCommand = new NpgsqlCommand(@"
                SELECT eq.id, COALESCE(eq.correct_option, 'A'), COALESCE(eq.points, 0)
                FROM exam_questions eq
                JOIN exam_attempts a ON a.exam_id = eq.exam_id
                WHERE a.id = @attempt_id
                ORDER BY COALESCE(eq.display_order, 1), eq.id", connection, transaction))
            {
                questionCommand.Parameters.AddWithValue("@attempt_id", attemptId);
                using var reader = questionCommand.ExecuteReader();
                while (reader.Read())
                {
                    questions.Add(new TeacherExamQuestionModel
                    {
                        Id = reader.GetInt32(0),
                        CorrectOption = reader.GetString(1),
                        Points = reader.GetDecimal(2)
                    });
                }
            }

            var selected = new Dictionary<int, string>();
            using (var answerCommand = new NpgsqlCommand(@"
                SELECT exam_question_id, COALESCE(selected_option, '')
                FROM exam_attempt_answers
                WHERE attempt_id = @attempt_id", connection, transaction))
            {
                answerCommand.Parameters.AddWithValue("@attempt_id", attemptId);
                using var reader = answerCommand.ExecuteReader();
                while (reader.Read())
                    selected[reader.GetInt32(0)] = reader.GetString(1);
            }

            decimal score = ExamScoringService.CalculateScore(questions, selected);

            using (var answerScoreCommand = new NpgsqlCommand(@"
                UPDATE exam_attempt_answers aa
                SET is_correct = UPPER(COALESCE(aa.selected_option, '')) = UPPER(COALESCE(eq.correct_option, '')),
                    score = CASE
                        WHEN UPPER(COALESCE(aa.selected_option, '')) = UPPER(COALESCE(eq.correct_option, '')) THEN COALESCE(eq.points, 0)
                        ELSE 0
                    END
                FROM exam_questions eq
                WHERE aa.exam_question_id = eq.id
                  AND aa.attempt_id = @attempt_id", connection, transaction))
            {
                answerScoreCommand.Parameters.AddWithValue("@attempt_id", attemptId);
                answerScoreCommand.ExecuteNonQuery();
            }

            using (var updateAttempt = new NpgsqlCommand(@"
                UPDATE exam_attempts
                SET score = @score, submit_time = CURRENT_TIMESTAMP, status = 'SUBMITTED'
                WHERE id = @attempt_id AND student_id = @student_id", connection, transaction))
            {
                updateAttempt.Parameters.AddWithValue("@attempt_id", attemptId);
                updateAttempt.Parameters.AddWithValue("@student_id", studentId);
                updateAttempt.Parameters.AddWithValue("@score", score);
                updateAttempt.ExecuteNonQuery();
            }

            transaction.Commit();
            return new StudentExamSubmitResultModel
            {
                Success = true,
                Score = score,
                Message = "Đã nộp bài."
            };
        }

        public List<StudentResultCourseFilterModel> GetActiveResultCourseFiltersForStudent(int studentId)
        {
            var result = new List<StudentResultCourseFilterModel>();
            using var connection = CreateConnection();
            connection.Open();
            if (!TableExists(connection, "courses") || !TableExists(connection, "enrollments"))
                return result;

            using var command = new NpgsqlCommand(@"
                SELECT DISTINCT c.id, COALESCE(c.name, '')
                FROM courses c
                JOIN enrollments e ON e.course_id = c.id
                WHERE e.student_id = @student_id
                  AND UPPER(COALESCE(e.status, '')) IN ('ACTIVE', 'APPROVED')
                ORDER BY COALESCE(c.name, '')", connection);
            command.Parameters.AddWithValue("@student_id", studentId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new StudentResultCourseFilterModel
                {
                    CourseId = reader.GetInt32(0),
                    CourseName = reader.GetString(1)
                });
            }
            return result;
        }

        public List<StudentResultListItemModel> GetStudentResultItems(int studentId, int? courseId = null, string? examTitleKeyword = null, int limit = 100)
        {
            var result = new List<StudentResultListItemModel>();
            int safeLimit = limit <= 0 ? 100 : limit;
            bool hasFilter = (courseId.HasValue && courseId.Value > 0) || !string.IsNullOrWhiteSpace(examTitleKeyword);

            using var connection = CreateConnection();
            connection.Open();

            if (TableExists(connection, "exam_attempts") && TableExists(connection, "exams") && TableExists(connection, "enrollments"))
            {
                bool hasQuestions = TableExists(connection, "exam_questions");
                bool hasHiddenResults = TableExists(connection, "student_hidden_results");
                bool hasAttemptAnswers = TableExists(connection, "exam_attempt_answers");
                string questionExpression = hasQuestions
                    ? @"(
                        SELECT COUNT(*)
                        FROM exam_questions eq
                        WHERE eq.exam_id = ex.id
                    )"
                    : "0";
                string correctExpression = hasAttemptAnswers
                    ? @"(
                        SELECT COUNT(*)
                        FROM exam_attempt_answers aa
                        WHERE aa.attempt_id = a.id
                          AND aa.is_correct = TRUE
                    )"
                    : "-1";
                string hiddenJoin = hasHiddenResults
                    ? "LEFT JOIN student_hidden_results shr ON shr.attempt_id = a.id AND shr.student_id = a.student_id"
                    : string.Empty;
                string hiddenFilter = hasHiddenResults
                    ? "AND shr.attempt_id IS NULL"
                    : string.Empty;

                string query = $@"
                    SELECT a.id AS attempt_id,
                           ex.id AS exam_id,
                           ex.course_id,
                           COALESCE(ex.title, '') AS exam_title,
                           COALESCE(c.name, '') AS course_name,
                           COALESCE(a.score, 0)::float8 AS score,
                           COALESCE(a.status, '') AS attempt_status,
                           {questionExpression}::int AS question_count,
                           {correctExpression}::int AS correct_count,
                           COALESCE(ex.status, '') AS exam_status
                    FROM exam_attempts a
                    JOIN exams ex ON ex.id = a.exam_id
                    JOIN enrollments en ON en.course_id = ex.course_id AND en.student_id = a.student_id
                    LEFT JOIN courses c ON c.id = ex.course_id
                    {hiddenJoin}
                    WHERE a.student_id = @student_id
                      AND a.score IS NOT NULL
                      AND UPPER(COALESCE(en.status, '')) IN ('ACTIVE', 'APPROVED')
                      {hiddenFilter}";
                if (courseId.HasValue && courseId.Value > 0)
                    query += " AND ex.course_id = @course_id";
                if (!string.IsNullOrWhiteSpace(examTitleKeyword))
                    query += " AND COALESCE(ex.title, '') ILIKE @keyword";
                query += " ORDER BY COALESCE(a.submit_time, a.start_time) DESC LIMIT @limit";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@student_id", studentId);
                command.Parameters.AddWithValue("@limit", safeLimit);
                if (courseId.HasValue && courseId.Value > 0)
                    command.Parameters.AddWithValue("@course_id", courseId.Value);
                if (!string.IsNullOrWhiteSpace(examTitleKeyword))
                    command.Parameters.AddWithValue("@keyword", $"%{examTitleKeyword.Trim()}%");
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    double score = reader.GetDouble(5);
                    int questionCount = reader.GetInt32(7);
                    int correctCount = reader.GetInt32(8);
                    result.Add(new StudentResultListItemModel
                    {
                        AttemptId = reader.GetInt32(0),
                        ExamId = reader.GetInt32(1),
                        CourseId = reader.GetInt32(2),
                        ExamTitle = reader.GetString(3),
                        CourseName = reader.GetString(4),
                        CorrectAnswersText = questionCount > 0 && correctCount >= 0 ? $"{correctCount}/{questionCount}" : questionCount > 0 ? $"N/A/{questionCount}" : "N/A",
                        Score = score,
                        StatusText = BuildResultStatus(score, reader.GetString(6)),
                        ExamStatus = reader.GetString(9)
                    });
                }
            }

            if (result.Count > 0 || hasFilter)
                return result;

            EnsureStudentScoreSchema(connection);
            const string fallbackQuery = @"
                SELECT lop, diem_gk, diem_ck
                FROM public.student_scores
                WHERE mssv = @mssv
                LIMIT @limit";

            using var fallbackCommand = new NpgsqlCommand(fallbackQuery, connection);
            fallbackCommand.Parameters.AddWithValue("@mssv", GetStudentCodeFromUserId(studentId));
            fallbackCommand.Parameters.AddWithValue("@limit", safeLimit);
            using var fallbackReader = fallbackCommand.ExecuteReader();
            while (fallbackReader.Read())
            {
                double midterm = fallbackReader.GetDouble(1);
                double final = fallbackReader.GetDouble(2);
                double score = Math.Round((midterm * 0.3) + (final * 0.7), 1);
                result.Add(new StudentResultListItemModel
                {
                    ExamTitle = "Bảng điểm tổng hợp",
                    CourseName = fallbackReader.IsDBNull(0) ? string.Empty : fallbackReader.GetString(0),
                    CorrectAnswersText = "N/A",
                    Score = score,
                    StatusText = score < 5.0 ? "Không đạt" : "Đạt"
                });
            }

            return result;
        }

        public bool HideStudentResult(int studentId, int attemptId)
        {
            using var connection = CreateConnection();
            connection.Open();
            EnsureStudentHiddenResultsSchema(connection);
            if (!TableExists(connection, "student_hidden_results"))
                return false;

            using var command = new NpgsqlCommand(@"
                INSERT INTO student_hidden_results (student_id, attempt_id)
                SELECT @student_id, @attempt_id
                WHERE EXISTS (
                    SELECT 1 FROM exam_attempts
                    WHERE id = @attempt_id AND student_id = @student_id
                )
                ON CONFLICT (student_id, attempt_id) DO NOTHING", connection);
            command.Parameters.AddWithValue("@student_id", studentId);
            command.Parameters.AddWithValue("@attempt_id", attemptId);
            return command.ExecuteNonQuery() > 0;
        }

        private static void EnsureStudentHiddenResultsSchema(NpgsqlConnection connection)
        {
            if (!TableExists(connection, "users") || !TableExists(connection, "exam_attempts"))
                return;

            using var command = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS student_hidden_results (
                    student_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                    attempt_id INT NOT NULL REFERENCES exam_attempts(id) ON DELETE CASCADE,
                    hidden_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (student_id, attempt_id)
                );

                CREATE INDEX IF NOT EXISTS idx_student_hidden_results_student ON student_hidden_results(student_id);", connection);
            command.ExecuteNonQuery();
        }

        public StudentExamReviewModel? GetStudentExamReview(int studentId, int attemptId)
        {
            using var connection = CreateConnection();
            connection.Open();
            if (!TableExists(connection, "exam_attempts") || !TableExists(connection, "exams"))
                return null;

            using var headerCommand = new NpgsqlCommand(@"
                SELECT a.id, ex.id, COALESCE(ex.title, ''), COALESCE(c.name, ''),
                       COALESCE(a.score, 0)::float8, COALESCE(a.status, ''),
                       COALESCE(ex.status, '')
                FROM exam_attempts a
                JOIN exams ex ON ex.id = a.exam_id
                LEFT JOIN courses c ON c.id = ex.course_id
                WHERE a.student_id = @student_id
                  AND a.id = @attempt_id
                  AND UPPER(COALESCE(ex.status, '')) = 'CLOSED'", connection);
            headerCommand.Parameters.AddWithValue("@student_id", studentId);
            headerCommand.Parameters.AddWithValue("@attempt_id", attemptId);
            using var reader = headerCommand.ExecuteReader();
            if (!reader.Read())
                return null;

            double score = reader.GetDouble(4);
            var review = new StudentExamReviewModel
            {
                AttemptId = reader.GetInt32(0),
                ExamId = reader.GetInt32(1),
                ExamTitle = reader.GetString(2),
                CourseName = reader.GetString(3),
                Score = score,
                StatusText = BuildResultStatus(score, reader.GetString(5))
            };
            reader.Close();

            if (!TableExists(connection, "exam_questions"))
                return review;

            bool hasAttemptAnswers = TableExists(connection, "exam_attempt_answers");
            string answerJoin = hasAttemptAnswers
                ? "LEFT JOIN exam_attempt_answers aa ON aa.exam_question_id = eq.id AND aa.attempt_id = @attempt_id"
                : string.Empty;
            string selectedExpression = hasAttemptAnswers ? "COALESCE(aa.selected_option, '')" : "''";

            using var questionCommand = new NpgsqlCommand($@"
                SELECT COALESCE(eq.display_order, 1), COALESCE(eq.question_text, ''),
                       COALESCE(eq.option_a, ''), COALESCE(eq.option_b, ''),
                       COALESCE(eq.option_c, ''), COALESCE(eq.option_d, ''),
                       COALESCE(eq.correct_option, ''), COALESCE(eq.points, 0),
                       {selectedExpression}
                FROM exam_questions eq
                {answerJoin}
                WHERE eq.exam_id = @exam_id
                ORDER BY eq.display_order, eq.id", connection);
            questionCommand.Parameters.AddWithValue("@exam_id", review.ExamId);
            if (hasAttemptAnswers)
                questionCommand.Parameters.AddWithValue("@attempt_id", attemptId);
            using var qReader = questionCommand.ExecuteReader();
            while (qReader.Read())
            {
                review.Questions.Add(new StudentExamReviewQuestionModel
                {
                    DisplayOrder = qReader.GetInt32(0),
                    QuestionText = qReader.GetString(1),
                    OptionA = qReader.GetString(2),
                    OptionB = qReader.GetString(3),
                    OptionC = qReader.GetString(4),
                    OptionD = qReader.GetString(5),
                    CorrectOption = qReader.GetString(6),
                    Points = qReader.GetDecimal(7),
                    SelectedOption = qReader.GetString(8)
                });
            }

            return review;
        }

        public List<StudentSearchResultModel> SearchStudentGlobal(int studentId, string keyword, int limitPerGroup = 5)
        {
            var results = new List<StudentSearchResultModel>();
            string term = (keyword ?? string.Empty).Trim();
            if (studentId <= 0 || string.IsNullOrWhiteSpace(term))
                return results;

            int safeLimit = Math.Clamp(limitPerGroup, 1, 20);
            string pattern = $"%{term}%";

            using var connection = CreateConnection();
            connection.Open();

            if (TableExists(connection, "courses") && TableExists(connection, "enrollments"))
            {
                const string courseQuery = @"
                    SELECT DISTINCT COALESCE(c.name, '') AS title,
                           COALESCE(u.full_name, u.username, '') AS teacher_name,
                           CASE
                             WHEN e.id IS NOT NULL THEN 'Khóa học của tôi'
                             ELSE 'Tìm khóa học'
                           END AS page_name
                    FROM courses c
                    LEFT JOIN users u ON u.id = c.teacher_id
                    LEFT JOIN enrollments e ON e.course_id = c.id AND e.student_id = @student_id
                    WHERE UPPER(COALESCE(c.status, '')) = 'ACTIVE'
                      AND (
                           e.id IS NULL
                           OR UPPER(COALESCE(e.status, '')) IN ('ACTIVE', 'APPROVED', 'PENDING')
                      )
                      AND (
                           COALESCE(c.name, '') ILIKE @pattern
                           OR COALESCE(c.description, '') ILIKE @pattern
                           OR COALESCE(u.full_name, u.username, '') ILIKE @pattern
                      )
                    ORDER BY title
                    LIMIT @limit";

                using var command = new NpgsqlCommand(courseQuery, connection);
                command.Parameters.AddWithValue("@student_id", studentId);
                command.Parameters.AddWithValue("@pattern", pattern);
                command.Parameters.AddWithValue("@limit", safeLimit);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new StudentSearchResultModel
                    {
                        Group = "Khóa học",
                        Title = reader.GetString(0),
                        Description = reader.GetString(1),
                        PageName = reader.GetString(2),
                        Keyword = term
                    });
                }
            }

            if (TableExists(connection, "exams") && TableExists(connection, "enrollments"))
            {
                bool hasAttempts = TableExists(connection, "exam_attempts");
                bool hasQuestions = TableExists(connection, "exam_questions");
                string attemptsExpression = hasAttempts
                    ? @"(
                        SELECT COUNT(*)
                        FROM exam_attempts ea
                        WHERE ea.exam_id = ex.id
                          AND ea.student_id = @student_id
                    )"
                    : "0";
                string questionExpression = hasQuestions
                    ? @"(
                        SELECT COUNT(*)
                        FROM exam_questions eq
                        WHERE eq.exam_id = ex.id
                    )"
                    : "0";

                string examQuery = $@"
                    SELECT COALESCE(ex.title, '') AS title,
                           COALESCE(c.name, '') AS course_name
                    FROM exams ex
                    JOIN enrollments en ON en.course_id = ex.course_id
                    LEFT JOIN courses c ON c.id = ex.course_id
                    WHERE en.student_id = @student_id
                      AND UPPER(COALESCE(en.status, 'ACTIVE')) IN ('ACTIVE', 'APPROVED')
                      AND UPPER(COALESCE(c.status, 'ACTIVE')) IN ('ACTIVE', 'APPROVED', 'OPEN')
                      AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'ACTIVE'
                      AND (ex.open_time IS NULL OR ex.open_time <= CURRENT_TIMESTAMP)
                      AND (ex.close_time IS NULL OR ex.close_time >= CURRENT_TIMESTAMP)
                      AND {questionExpression} > 0
                      AND (COALESCE(ex.max_attempts, 1) <= 0 OR {attemptsExpression} < COALESCE(ex.max_attempts, 1))
                      AND (
                           COALESCE(ex.title, '') ILIKE @pattern
                           OR COALESCE(c.name, '') ILIKE @pattern
                      )
                    ORDER BY ex.open_time NULLS FIRST, ex.close_time NULLS LAST
                    LIMIT @limit";

                using var command = new NpgsqlCommand(examQuery, connection);
                command.Parameters.AddWithValue("@student_id", studentId);
                command.Parameters.AddWithValue("@pattern", pattern);
                command.Parameters.AddWithValue("@limit", safeLimit);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new StudentSearchResultModel
                    {
                        Group = "Bài kiểm tra",
                        Title = reader.GetString(0),
                        Description = reader.GetString(1),
                        PageName = "Bài kiểm tra",
                        Keyword = term
                    });
                }
            }

            if (TableExists(connection, "materials") && TableExists(connection, "courses") && TableExists(connection, "enrollments"))
            {
                const string materialQuery = @"
                    SELECT COALESCE(m.file_name, '') AS title,
                           COALESCE(c.name, '') AS course_name
                    FROM materials m
                    JOIN courses c ON c.id = m.course_id
                    JOIN enrollments e ON e.course_id = c.id
                    WHERE e.student_id = @student_id
                      AND UPPER(COALESCE(e.status, '')) IN ('ACTIVE', 'APPROVED')
                      AND (
                           COALESCE(m.file_name, '') ILIKE @pattern
                           OR COALESCE(m.file_path, '') ILIKE @pattern
                           OR COALESCE(c.name, '') ILIKE @pattern
                      )
                    ORDER BY m.uploaded_at DESC, m.id DESC
                    LIMIT @limit";

                using var command = new NpgsqlCommand(materialQuery, connection);
                command.Parameters.AddWithValue("@student_id", studentId);
                command.Parameters.AddWithValue("@pattern", pattern);
                command.Parameters.AddWithValue("@limit", safeLimit);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new StudentSearchResultModel
                    {
                        Group = "Tài liệu",
                        Title = reader.GetString(0),
                        Description = reader.GetString(1),
                        PageName = "Tài liệu",
                        Keyword = term
                    });
                }
            }

            if (TableExists(connection, "notifications"))
            {
                const string notificationQuery = @"
                    SELECT COALESCE(title, '') AS title,
                           COALESCE(content, '') AS content
                    FROM notifications
                    WHERE user_id = @student_id
                      AND (
                           COALESCE(title, '') ILIKE @pattern
                           OR COALESCE(content, '') ILIKE @pattern
                      )
                    ORDER BY created_at DESC
                    LIMIT @limit";

                using var command = new NpgsqlCommand(notificationQuery, connection);
                command.Parameters.AddWithValue("@student_id", studentId);
                command.Parameters.AddWithValue("@pattern", pattern);
                command.Parameters.AddWithValue("@limit", safeLimit);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string content = reader.GetString(1);
                    results.Add(new StudentSearchResultModel
                    {
                        Group = "Thông báo",
                        Title = reader.GetString(0),
                        Description = content.Length > 64 ? content[..64] + "..." : content,
                        PageName = "Thông báo",
                        Keyword = term
                    });
                }
            }

            return results;
        }

        private int CountStudentExamsByAvailability(int studentId, bool openNowOnly)
        {
            using var connection = CreateConnection();
            connection.Open();

            if (!TableExists(connection, "exams") || !TableExists(connection, "enrollments"))
                return 0;

            bool hasAttempts = TableExists(connection, "exam_attempts");
            bool hasQuestions = TableExists(connection, "exam_questions");
            string attemptsExpression = hasAttempts
                ? @"(
                    SELECT COUNT(*)
                    FROM exam_attempts ea
                    WHERE ea.exam_id = ex.id
                      AND ea.student_id = @student_id
                )"
                : "0";
            string questionExpression = hasQuestions
                ? @"(
                    SELECT COUNT(*)
                    FROM exam_questions eq
                    WHERE eq.exam_id = ex.id
                )"
                : "0";

            string timeFilter = openNowOnly
                ? @"AND (ex.open_time IS NULL OR ex.open_time <= CURRENT_TIMESTAMP)
                   AND (ex.close_time IS NULL OR ex.close_time >= CURRENT_TIMESTAMP)"
                : @"AND (ex.open_time IS NULL OR ex.open_time <= CURRENT_TIMESTAMP)
                   AND (ex.close_time IS NULL OR ex.close_time >= CURRENT_TIMESTAMP)";

            string query = $@"
                SELECT COUNT(*)
                FROM exams ex
                JOIN enrollments en ON en.course_id = ex.course_id
                LEFT JOIN courses c ON c.id = ex.course_id
                WHERE en.student_id = @student_id
                  AND UPPER(COALESCE(en.status, 'ACTIVE')) IN ('ACTIVE', 'APPROVED')
                  AND UPPER(COALESCE(c.status, 'ACTIVE')) IN ('ACTIVE', 'APPROVED', 'OPEN')
                  AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'ACTIVE'
                  {timeFilter}
                  AND {questionExpression} > 0
                  AND (COALESCE(ex.max_attempts, 1) <= 0 OR {attemptsExpression} < COALESCE(ex.max_attempts, 1))";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@student_id", studentId);
            object? result = command.ExecuteScalar();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        private static void EnsureStudentExamTakingSchema(NpgsqlConnection connection)
        {
            if (!TableExists(connection, "exam_attempts") || !TableExists(connection, "exam_questions"))
                return;

            using var command = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS exam_attempt_answers (
                    attempt_id INT NOT NULL REFERENCES exam_attempts(id) ON DELETE CASCADE,
                    exam_question_id INT NOT NULL REFERENCES exam_questions(id) ON DELETE CASCADE,
                    selected_option CHAR(1) NOT NULL,
                    is_correct BOOLEAN,
                    score NUMERIC(6,2) NOT NULL DEFAULT 0,
                    answered_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (attempt_id, exam_question_id)
                );

                CREATE INDEX IF NOT EXISTS idx_exam_attempt_answers_attempt ON exam_attempt_answers(attempt_id);
                CREATE INDEX IF NOT EXISTS idx_exam_attempt_answers_question ON exam_attempt_answers(exam_question_id);", connection);
            command.ExecuteNonQuery();
        }

        private static int GetExistingInProgressAttempt(NpgsqlConnection connection, NpgsqlTransaction transaction, int studentId, int examId)
        {
            using var command = new NpgsqlCommand(@"
                SELECT id
                FROM exam_attempts
                WHERE student_id = @student_id
                  AND exam_id = @exam_id
                  AND UPPER(COALESCE(status, '')) = 'IN_PROGRESS'
                ORDER BY start_time DESC, id DESC
                LIMIT 1", connection, transaction);
            command.Parameters.AddWithValue("@student_id", studentId);
            command.Parameters.AddWithValue("@exam_id", examId);
            object? result = command.ExecuteScalar();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        private static bool CanStartStudentExam(NpgsqlConnection connection, NpgsqlTransaction transaction, int studentId, int examId)
        {
            using var command = new NpgsqlCommand(@"
                SELECT COALESCE(ex.max_attempts, 1),
                       (SELECT COUNT(*) FROM exam_attempts a WHERE a.exam_id = ex.id AND a.student_id = @student_id)::int,
                       (SELECT COUNT(*) FROM exam_questions eq WHERE eq.exam_id = ex.id)::int
                FROM exams ex
                JOIN enrollments en ON en.course_id = ex.course_id
                LEFT JOIN courses c ON c.id = ex.course_id
                WHERE ex.id = @exam_id
                  AND en.student_id = @student_id
                  AND UPPER(COALESCE(en.status, '')) IN ('ACTIVE', 'APPROVED')
                  AND UPPER(COALESCE(c.status, 'ACTIVE')) IN ('ACTIVE', 'APPROVED', 'OPEN')
                  AND UPPER(COALESCE(ex.status, 'DRAFT')) = 'ACTIVE'
                  AND (ex.open_time IS NULL OR ex.open_time <= CURRENT_TIMESTAMP)
                  AND (ex.close_time IS NULL OR ex.close_time >= CURRENT_TIMESTAMP)", connection, transaction);
            command.Parameters.AddWithValue("@student_id", studentId);
            command.Parameters.AddWithValue("@exam_id", examId);
            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return false;

            int maxAttempts = reader.GetInt32(0);
            int attemptCount = reader.GetInt32(1);
            int questionCount = reader.GetInt32(2);
            return questionCount > 0 && (maxAttempts <= 0 || attemptCount < maxAttempts);
        }

        private static bool OwnsInProgressAttempt(NpgsqlConnection connection, NpgsqlTransaction transaction, int studentId, int attemptId)
        {
            using var command = new NpgsqlCommand(@"
                SELECT COUNT(*)::int
                FROM exam_attempts
                WHERE id = @attempt_id
                  AND student_id = @student_id
                  AND UPPER(COALESCE(status, '')) = 'IN_PROGRESS'", connection, transaction);
            command.Parameters.AddWithValue("@attempt_id", attemptId);
            command.Parameters.AddWithValue("@student_id", studentId);
            return Convert.ToInt32(command.ExecuteScalar() ?? 0) > 0;
        }

        private static StudentExamTakingModel? LoadStudentExamSession(NpgsqlConnection connection, NpgsqlTransaction transaction, int studentId, int attemptId)
        {
            using var headerCommand = new NpgsqlCommand(@"
                SELECT a.id, ex.id, COALESCE(ex.title, ''), COALESCE(c.name, ''),
                       COALESCE(ex.duration_minutes, 0), COALESCE(a.start_time, CURRENT_TIMESTAMP)
                FROM exam_attempts a
                JOIN exams ex ON ex.id = a.exam_id
                LEFT JOIN courses c ON c.id = ex.course_id
                WHERE a.id = @attempt_id
                  AND a.student_id = @student_id
                  AND UPPER(COALESCE(a.status, '')) = 'IN_PROGRESS'", connection, transaction);
            headerCommand.Parameters.AddWithValue("@attempt_id", attemptId);
            headerCommand.Parameters.AddWithValue("@student_id", studentId);
            using var reader = headerCommand.ExecuteReader();
            if (!reader.Read())
                return null;

            var session = new StudentExamTakingModel
            {
                AttemptId = reader.GetInt32(0),
                ExamId = reader.GetInt32(1),
                ExamTitle = reader.GetString(2),
                CourseName = reader.GetString(3),
                DurationMinutes = reader.GetInt32(4),
                StartTime = reader.GetDateTime(5)
            };
            reader.Close();

            using var questionCommand = new NpgsqlCommand(@"
                SELECT eq.id, COALESCE(eq.display_order, 1), COALESCE(eq.question_text, ''),
                       COALESCE(eq.option_a, ''), COALESCE(eq.option_b, ''),
                       COALESCE(eq.option_c, ''), COALESCE(eq.option_d, ''),
                       COALESCE(aa.selected_option, '')
                FROM exam_questions eq
                JOIN exam_attempts a ON a.exam_id = eq.exam_id
                LEFT JOIN exam_attempt_answers aa ON aa.attempt_id = a.id AND aa.exam_question_id = eq.id
                WHERE a.id = @attempt_id
                ORDER BY COALESCE(eq.display_order, 1), eq.id", connection, transaction);
            questionCommand.Parameters.AddWithValue("@attempt_id", attemptId);
            using var qReader = questionCommand.ExecuteReader();
            while (qReader.Read())
            {
                session.Questions.Add(new StudentExamTakingQuestionModel
                {
                    Id = qReader.GetInt32(0),
                    DisplayOrder = qReader.GetInt32(1),
                    QuestionText = qReader.GetString(2),
                    OptionA = qReader.GetString(3),
                    OptionB = qReader.GetString(4),
                    OptionC = qReader.GetString(5),
                    OptionD = qReader.GetString(6),
                    SelectedOption = qReader.GetString(7)
                });
            }

            return session;
        }

        private static string NormalizeOption(string? value)
        {
            string option = (value ?? string.Empty).Trim().ToUpperInvariant();
            return option is "A" or "B" or "C" or "D" ? option : string.Empty;
        }

        private static string BuildResultStatus(double score, string rawStatus)
        {
            if (!string.IsNullOrWhiteSpace(rawStatus)
                && !string.Equals(rawStatus, "IN_PROGRESS", StringComparison.OrdinalIgnoreCase))
            {
                return rawStatus;
            }

            return score < 5.0 ? "Không đạt" : "Đạt";
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

            string query = @"SELECT c.id, c.name, c.description, c.teacher_id, u.full_name as teacher_name,
                                    c.status, c.start_date, c.end_date, COALESCE(c.rejection_reason, '')
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
                    EndDate = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7),
                    RejectionReason = reader.GetString(8)
                });
            }

            return courses;
        }

        public CourseModel? GetCourseById(int courseId)
        {
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT c.id, c.name, COALESCE(c.description, ''), c.teacher_id,
                       COALESCE(u.full_name, u.username) AS teacher_name,
                       COALESCE(c.status, 'ACTIVE'), c.start_date, c.end_date,
                       COALESCE(c.rejection_reason, '')
                FROM courses c
                JOIN users u ON u.id = c.teacher_id
                WHERE c.id = @id";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", courseId);
            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            return new CourseModel
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                TeacherId = reader.GetInt32(3),
                TeacherName = reader.GetString(4),
                Status = reader.GetString(5),
                StartDate = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
                EndDate = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7),
                RejectionReason = reader.GetString(8)
            };
        }

        public void InsertCourse(CourseModel course)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = @"INSERT INTO COURSES (name, description, teacher_id, status, rejection_reason, start_date, end_date) 
                            VALUES (@name, @description, @teacher_id, @status, @rejection_reason, @start_date, @end_date)";
            
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", course.Name);
            command.Parameters.AddWithValue("@description", course.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@teacher_id", course.TeacherId);
            command.Parameters.AddWithValue("@status", course.Status ?? "ACTIVE");
            command.Parameters.AddWithValue("@rejection_reason", string.IsNullOrWhiteSpace(course.RejectionReason) ? (object)DBNull.Value : course.RejectionReason);
            command.Parameters.AddWithValue("@start_date", course.StartDate == DateTime.MinValue ? (object)DBNull.Value : course.StartDate);
            command.Parameters.AddWithValue("@end_date", course.EndDate == DateTime.MinValue ? (object)DBNull.Value : course.EndDate);

            command.ExecuteNonQuery();
        }

        public void UpdateCourse(CourseModel course)
        {
            using var connection = CreateConnection();
            connection.Open();

            string query = @"UPDATE COURSES SET name = @name, description = @description, teacher_id = @teacher_id, 
                             status = @status, rejection_reason = @rejection_reason, start_date = @start_date, end_date = @end_date WHERE id = @id";
            
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", course.Name);
            command.Parameters.AddWithValue("@description", course.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@teacher_id", course.TeacherId);
            command.Parameters.AddWithValue("@status", course.Status);
            command.Parameters.AddWithValue("@rejection_reason", string.IsNullOrWhiteSpace(course.RejectionReason) ? (object)DBNull.Value : course.RejectionReason);
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

        public List<StudentScheduleItemModel> GetStudentOnlineSessions(int studentId)
        {
            var rows = new List<StudentScheduleItemModel>();
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT os.id, os.course_id, COALESCE(c.name, ''),
                       COALESCE(u.full_name, u.username, ''),
                       COALESCE(os.title, ''), os.start_time, os.end_time,
                       COALESCE(os.meeting_link, '')
                FROM online_sessions os
                JOIN courses c ON c.id = os.course_id
                JOIN enrollments e ON e.course_id = c.id
                JOIN users u ON u.id = c.teacher_id
                WHERE e.student_id = @student_id
                  AND UPPER(COALESCE(e.status, '')) IN ('ACTIVE', 'APPROVED')
                  AND UPPER(COALESCE(c.status, '')) = 'ACTIVE'
                ORDER BY os.start_time ASC NULLS LAST, os.id DESC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@student_id", studentId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new StudentScheduleItemModel
                {
                    SessionId = reader.GetInt32(0),
                    CourseId = reader.GetInt32(1),
                    CourseName = reader.GetString(2),
                    TeacherName = reader.GetString(3),
                    Title = reader.GetString(4),
                    StartTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    EndTime = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    MeetingLink = reader.GetString(7)
                });
            }

            return rows;
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
                UPDATE enrollments
                SET status = 'PENDING', joined_at = CURRENT_TIMESTAMP
                WHERE course_id = @course_id
                  AND student_id = @student_id
                  AND UPPER(COALESCE(status, '')) = 'REJECTED'
                  AND EXISTS (
                      SELECT 1 FROM courses
                      WHERE id = @course_id AND UPPER(COALESCE(status, '')) = 'ACTIVE'
                  );

                INSERT INTO ENROLLMENTS (course_id, student_id, status)
                SELECT @course_id, @student_id, 'PENDING'
                WHERE EXISTS (
                    SELECT 1 FROM courses
                    WHERE id = @course_id AND UPPER(COALESCE(status, '')) = 'ACTIVE'
                )
                  AND NOT EXISTS (
                    SELECT 1 FROM ENROLLMENTS
                    WHERE course_id = @course_id AND student_id = @student_id
                );

                SELECT COUNT(*)
                FROM enrollments
                WHERE course_id = @course_id
                  AND student_id = @student_id
                  AND UPPER(COALESCE(status, '')) = 'PENDING';";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@student_id", studentId);

            return Convert.ToInt64(command.ExecuteScalar()) > 0;
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
                WHERE UPPER(COALESCE(c.status, '')) = 'ACTIVE'
                  AND c.id NOT IN (
                      SELECT course_id FROM ENROLLMENTS
                      WHERE student_id = @student_id
                        AND UPPER(COALESCE(status, '')) IN ('PENDING', 'ACTIVE', 'APPROVED')
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

        public List<EnrollmentModel> GetEnrollmentsByStatus(int courseId, string status)
        {
            var list = new List<EnrollmentModel>();
            using var connection = CreateConnection();
            connection.Open();

            const string query = @"
                SELECT e.id, e.course_id, e.student_id, e.status, e.joined_at,
                       c.name AS course_name, COALESCE(u.full_name, u.username) AS teacher_name,
                       c.status AS course_status, c.start_date, c.end_date, COALESCE(c.description, '') AS description,
                       COALESCE(s.full_name, s.username) AS student_name
                FROM ENROLLMENTS e
                JOIN COURSES c ON e.course_id = c.id
                JOIN USERS u ON c.teacher_id = u.id
                JOIN USERS s ON e.student_id = s.id
                WHERE e.course_id = @course_id AND UPPER(e.status) = UPPER(@status)
                ORDER BY e.joined_at DESC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@course_id", courseId);
            command.Parameters.AddWithValue("@status", status);

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

            const string query = @"
                UPDATE enrollments
                SET status = 'REJECTED'
                WHERE course_id = @cid
                  AND student_id = @sid
                  AND UPPER(COALESCE(status, '')) = 'PENDING'";
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

        public List<RecentUserActivityModel> GetRecentUserActivitiesByUser(int userId, int limit = 10)
        {
            var result = new List<RecentUserActivityModel>();
            int safeLimit = limit <= 0 ? 10 : limit;

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
                WHERE a.USER_ID = @user_id
                  AND a.ACTION IN (
                    'LOGIN', 'LOGOUT', 'SIGNUP', 'FORGOT_PASSWORD', 'CHANGE_PASSWORD',
                    'COURSE_ENROLL_REQUEST', 'COURSE_ENROLL', 'ONLINE_SESSION_JOIN', 'ONLINE_SESSION_EXIT',
                    'EXAM_JOIN', 'EXAM_SUBMIT', 'EXAM_EXIT',
                    'CHAT_USE'
                  )
                ORDER BY a.CREATED_AT DESC
                LIMIT @limit";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@limit", safeLimit);
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
