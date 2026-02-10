/*
 * CourseService.cs
 * 
 * Layer: Application (Services)
 * Vai trò: Thực thi logic nghiệp vụ cho Course. Validate dữ liệu khóa học trước khi lưu xuống DB.
 * Phụ thuộc: ICourseRepository (thông qua Dependency Injection).
 */
using System.Collections.Generic;
using CourseGuard.Application.Interfaces;
using CourseGuard.Core.Models;

namespace CourseGuard.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        /// <summary>
        /// Lấy danh sách khóa học.
        /// Sử dụng: Gọi _courseRepository.GetAll().
        /// </summary>
        public List<CourseModel> GetAllCourses()
        {
            return _courseRepository.GetAll();
        }

        /// <summary>
        /// Lấy khóa học theo ID.
        /// Sử dụng: Gọi _courseRepository.GetById().
        /// </summary>
        public CourseModel GetCourseById(int id)
        {
            return _courseRepository.GetById(id);
        }

        /// <summary>
        /// Thêm khóa học mới có kiểm tra trùng lặp.
        /// Sử dụng: Gọi _courseRepository.GetByNameAndTeacher() để kiểm tra. Nếu hợp lệ gọi _courseRepository.Add().
        /// </summary>
        public int AddCourse(CourseModel course)
        {
            // Check for duplicate name for the SAME teacher
            var existingCourse = _courseRepository.GetByNameAndTeacher(course.Name, course.TeacherId);
            if (existingCourse != null)
            {
                throw new System.Exception("Giáo viên này đã có khóa học với tên tương tự. Vui lòng chọn tên khác.");
            }

            return _courseRepository.Add(course);
        }

        /// <summary>
        /// Cập nhật khóa học.
        /// Sử dụng: Gọi _courseRepository.Update().
        /// </summary>
        public bool UpdateCourse(CourseModel course)
        {
            return _courseRepository.Update(course);
        }

        /// <summary>
        /// Xóa khóa học.
        /// Sử dụng: Gọi _courseRepository.Delete().
        /// </summary>
        public bool DeleteCourse(int id)
        {
            return _courseRepository.Delete(id);
        }

        /// <summary>
        /// Ghi danh học viên.
        /// Sử dụng: Gọi _courseRepository.AddEnrollment() với trạng thái 'APPROVED'.
        /// </summary>
        public bool EnrollStudent(int courseId, int studentId)
        {
            // Default status "PENDING" or "APPROVED" depending on requirement. 
            // User didn't specify, but existing DB default is PENDING.
            // Let's default to APPROVED for now since Admin is adding them manually? 
            // Use "APPROVED" for Admin adds usually.
            return _courseRepository.AddEnrollment(courseId, studentId, "APPROVED");
        }
    }
}
