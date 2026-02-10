/*
 * CourseRepository.cs
 * 
 * Layer: Infrastructure
 * Vai trò: Tương tự như UserRepository nhưng dành cho bảng COURSES. Chứa logic JOIN bảng để lấy tên giáo viên.
 * Sử dụng: Cung cấp dữ liệu khóa học cho CourseService.
 */
using System;
using System.Collections.Generic;
using System.Data;
using CourseGuard.Application.Interfaces;
using CourseGuard.Core.Models;
using CourseGuard.Infrastructure.Data;

namespace CourseGuard.Infrastructure.Data.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        /// <summary>
        /// Lấy tất cả khóa học, sắp xếp theo ngày tạo mới nhất.
        /// Sử dụng: Thực thi lệnh SELECT kết hợp JOIN USERS (để lấy tên Teacher). Thực thi bằng DatabaseAction.ExecuteQuery.
        /// </summary>
        public List<CourseModel> GetAll()
        {
            var courses = new List<CourseModel>();
            string query = @"
                SELECT c.*, u.FULL_NAME as TEACHER_NAME 
                FROM COURSES c 
                LEFT JOIN USERS u ON c.TEACHER_ID = u.ID
                ORDER BY c.CREATED_AT DESC";

            DataTable dt = DatabaseAction.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                courses.Add(MapToCourse(row));
            }
            return courses;
        }

        /// <summary>
        /// Lấy khóa học theo ID.
        /// Sử dụng: Thực thi lệnh SELECT với điều kiện WHERE ID = @id. Thực thi bằng DatabaseAction.ExecuteQuery.
        /// </summary>
        public CourseModel GetById(int id)
        {
            string query = @"
                SELECT c.*, u.FULL_NAME as TEACHER_NAME 
                FROM COURSES c 
                LEFT JOIN USERS u ON c.TEACHER_ID = u.ID
                WHERE c.ID = @id";

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@id", (SqlDbType.Int, id) }
            };

            DataTable dt = DatabaseAction.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                return MapToCourse(dt.Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// Tìm khóa học theo tên và giáo viên (Check Duplicate).
        /// Sử dụng: Thực thi lệnh SELECT với điều kiện WHERE NAME = @name AND TEACHER_ID = @teacherId. Thực thi bằng DatabaseAction.ExecuteQuery.
        /// </summary>
        public CourseModel GetByNameAndTeacher(string name, int teacherId)
        {
            string query = @"
                SELECT c.*, u.FULL_NAME as TEACHER_NAME 
                FROM COURSES c 
                LEFT JOIN USERS u ON c.TEACHER_ID = u.ID
                WHERE c.NAME = @name AND c.TEACHER_ID = @teacherId";

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@name", (SqlDbType.NVarChar, name) },
                { "@teacherId", (SqlDbType.Int, teacherId) }
            };

            DataTable dt = DatabaseAction.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                return MapToCourse(dt.Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// Thêm khóa học mới.
        /// Sử dụng: Thực thi lệnh INSERT INTO COURSES. Sử dụng OUTPUT INSERTED.ID để lấy ID vừa tạo (ExecuteScalar).
        /// </summary>
        public int Add(CourseModel course)
        {
            string query = @"
                INSERT INTO COURSES (NAME, DESCRIPTION, TEACHER_ID, STATUS, START_DATE, END_DATE, CREATED_AT)
                OUTPUT INSERTED.ID
                VALUES (@name, @description, @teacherId, @status, @startDate, @endDate, GETDATE())";

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@name", (SqlDbType.NVarChar, course.Name) },
                { "@description", (SqlDbType.NVarChar, course.Description ?? (object)DBNull.Value) },
                { "@teacherId", (SqlDbType.Int, course.TeacherId) },
                { "@status", (SqlDbType.NVarChar, course.Status) },
                { "@startDate", (SqlDbType.Date, course.StartDate) },
                { "@endDate", (SqlDbType.Date, course.EndDate) }
            };

            object result = DatabaseAction.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Cập nhật thông tin khóa học.
        /// Sử dụng: Thực thi lệnh UPDATE COURSES theo ID. Thực thi bằng DatabaseAction.ExecuteNonQuery.
        /// </summary>
        public bool Update(CourseModel course)
        {
            string query = @"
                UPDATE COURSES 
                SET NAME = @name, 
                    DESCRIPTION = @description, 
                    TEACHER_ID = @teacherId, 
                    STATUS = @status, 
                    START_DATE = @startDate, 
                    END_DATE = @endDate
                WHERE ID = @id";

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@id", (SqlDbType.Int, course.Id) },
                { "@name", (SqlDbType.NVarChar, course.Name) },
                { "@description", (SqlDbType.NVarChar, course.Description ?? (object)DBNull.Value) },
                { "@teacherId", (SqlDbType.Int, course.TeacherId) },
                { "@status", (SqlDbType.NVarChar, course.Status) },
                { "@startDate", (SqlDbType.Date, course.StartDate) },
                { "@endDate", (SqlDbType.Date, course.EndDate) }
            };

            int rowsAffected = DatabaseAction.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Xóa khóa học.
        /// Sử dụng: Thực thi lệnh DELETE FROM COURSES theo ID. Thực thi bằng DatabaseAction.ExecuteNonQuery.
        /// </summary>
        public bool Delete(int id)
        {
            // Note: Soft delete is safer, but Schema implies hard delete or Status update.
            // For now, let's assume direct delete but better to check for constraints (Enrollments etc).
            // However, Schema has ON DELETE CASCADE for Enrollments, Exams, Materials, Messages, etc.
            // So hard delete is supported by DB constraints.
            
            string query = "DELETE FROM COURSES WHERE ID = @id";
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@id", (SqlDbType.Int, id) }
            };

            int rowsAffected = DatabaseAction.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        private CourseModel MapToCourse(DataRow row)
        {
            return new CourseModel
            {
                Id = row["ID"] != DBNull.Value ? Convert.ToInt32(row["ID"]) : 0,
                Name = row["NAME"]?.ToString() ?? string.Empty,
                Description = row["DESCRIPTION"] != DBNull.Value ? row["DESCRIPTION"].ToString() : string.Empty,
                TeacherId = row["TEACHER_ID"] != DBNull.Value ? Convert.ToInt32(row["TEACHER_ID"]) : 0,
                TeacherName = row.Table.Columns.Contains("TEACHER_NAME") && row["TEACHER_NAME"] != DBNull.Value ? row["TEACHER_NAME"].ToString() : string.Empty,
                Status = row["STATUS"] != DBNull.Value ? row["STATUS"].ToString() : "Inactive",
                StartDate = row["START_DATE"] != DBNull.Value ? Convert.ToDateTime(row["START_DATE"]) : DateTime.MinValue,
                EndDate = row["END_DATE"] != DBNull.Value ? Convert.ToDateTime(row["END_DATE"]) : DateTime.MinValue,
                CreatedAt = row["CREATED_AT"] != DBNull.Value ? Convert.ToDateTime(row["CREATED_AT"]) : DateTime.MinValue
            };
        }

        /// <summary>
        /// Thêm Enrollments.
        /// Sử dụng: Kiểm tra tồn tại bằng SELECT COUNT (ExecuteScalar), sau đó INSERT (ExecuteNonQuery).
        /// </summary>
        public bool AddEnrollment(int courseId, int studentId, string status = "PENDING")
        {
            try
            {
                // Check if already enrolled
                string checkQuery = "SELECT COUNT(*) FROM ENROLLMENTS WHERE COURSE_ID = @courseId AND STUDENT_ID = @studentId";
                var checkParams = new Dictionary<string, (SqlDbType, object)>
                {
                    { "@courseId", (SqlDbType.Int, courseId) },
                    { "@studentId", (SqlDbType.Int, studentId) }
                };
                
                int count = Convert.ToInt32(DatabaseAction.ExecuteScalar(checkQuery, checkParams));
                if (count > 0) return false; // Already enrolled

                string query = @"
                    INSERT INTO ENROLLMENTS (COURSE_ID, STUDENT_ID, STATUS, JOINED_AT)
                    VALUES (@courseId, @studentId, @status, GETDATE())";
                
                var parameters = new Dictionary<string, (SqlDbType, object)>
                {
                    { "@courseId", (SqlDbType.Int, courseId) },
                    { "@studentId", (SqlDbType.Int, studentId) },
                    { "@status", (SqlDbType.NVarChar, status) }
                };

                return DatabaseAction.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
