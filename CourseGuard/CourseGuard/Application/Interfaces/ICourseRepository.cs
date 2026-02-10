/*
 * ICourseRepository.cs
 * 
 * Layer: Application (Interfaces)
 * Vai trò: Định nghĩa hợp đồng (contract) cho Repository Course, giúp Service không phụ thuộc trực tiếp vào Repository cụ thể.
 * Sử dụng: Được implement bởi CourseRepository và sử dụng bởi CourseService.
 */
using System.Collections.Generic;
using CourseGuard.Core.Models;

namespace CourseGuard.Application.Interfaces
{
    public interface ICourseRepository
    {
        /// <summary>
        /// Lấy danh sách tất cả khóa học.
        /// Sử dụng: SELECT * FROM COURSES (thông qua DatabaseAction.ExecuteQuery).
        /// </summary>
        List<CourseModel> GetAll();

        /// <summary>
        /// Lấy thông tin chi tiết khóa học theo ID.
        /// Sử dụng: SELECT ... WHERE ID = ... (thông qua DatabaseAction.ExecuteQuery).
        /// </summary>
        CourseModel GetById(int id);

        /// <summary>
        /// Thêm khóa học mới vào database.
        /// Sử dụng: INSERT INTO COURSES (thông qua DatabaseAction.ExecuteScalar để lấy ID).
        /// </summary>
        int Add(CourseModel course);

        /// <summary>
        /// Cập nhật thông tin khóa học.
        /// Sử dụng: UPDATE COURSES ... WHERE ID = ... (thông qua DatabaseAction.ExecuteNonQuery).
        /// </summary>
        bool Update(CourseModel course);

        /// <summary>
        /// Xóa khóa học khỏi database.
        /// Sử dụng: DELETE FROM COURSES WHERE ID = ... (thông qua DatabaseAction.ExecuteNonQuery).
        /// </summary>
        bool Delete(int id);

        /// <summary>
        /// Tìm khóa học theo tên và ID giáo viên (để kiểm tra trùng lặp).
        /// Sử dụng: SELECT ... WHERE NAME = ... AND TEACHER_ID = ... (thông qua DatabaseAction.ExecuteQuery).
        /// </summary>
        CourseModel GetByNameAndTeacher(string name, int teacherId);

        /// <summary>
        /// Thêm học viên vào khóa học (Ghi danh).
        /// Sử dụng: INSERT INTO ENROLLMENTS (trước đó có kiểm tra tồn tại bằng SELECT COUNT).
        /// </summary>
        bool AddEnrollment(int courseId, int studentId, string status = "PENDING");
    }
}
