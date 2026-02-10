/*
 * ICourseService.cs
 * 
 * Layer: Application (Interfaces)
 * Vai trò: Định nghĩa các chức năng nghiệp vụ liên quan đến Course mà ứng dụng cung cấp cho UI (CRUD).
 * Sử dụng: Được implement bởi CourseService và sử dụng bởi Presentation Layer.
 */
using System.Collections.Generic;
using CourseGuard.Core.Models;

namespace CourseGuard.Application.Interfaces
{
    public interface ICourseService
    {
        /// <summary>
        /// Lấy danh sách tất cả khóa học.
        /// Sử dụng: Gọi _courseRepository.GetAll().
        /// </summary>
        List<CourseModel> GetAllCourses();

        /// <summary>
        /// Lấy khóa học theo ID.
        /// Sử dụng: Gọi _courseRepository.GetById().
        /// </summary>
        CourseModel GetCourseById(int id);

        /// <summary>
        /// Thêm khóa học mới (kèm validation trùng lặp).
        /// Sử dụng: Validate tên trùng bằng _courseRepository.GetByNameAndTeacher(), sau đó gọi _courseRepository.Add().
        /// </summary>
        int AddCourse(CourseModel course);

        /// <summary>
        /// Cập nhật thông tin khóa học.
        /// Sử dụng: Gọi _courseRepository.Update().
        /// </summary>
        bool UpdateCourse(CourseModel course);

        /// <summary>
        /// Xóa khóa học.
        /// Sử dụng: Gọi _courseRepository.Delete().
        /// </summary>
        bool DeleteCourse(int id);

        /// <summary>
        /// Ghi danh học viên vào khóa học.
        /// Sử dụng: Gọi _courseRepository.AddEnrollment().
        /// </summary>
        bool EnrollStudent(int courseId, int studentId);
    }
}
