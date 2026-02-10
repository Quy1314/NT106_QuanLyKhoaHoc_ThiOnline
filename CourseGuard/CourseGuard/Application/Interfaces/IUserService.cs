/*
 * IUserService.cs
 * 
 * Layer: Application (Interfaces)
 * Vai trò: Định nghĩa các chức năng nghiệp vụ liên quan đến User mà ứng dụng cung cấp cho UI (CRUD, tìm kiếm).
 * Sử dụng: Được implement bởi UserService và sử dụng bởi Presentation Layer.
 */
using System.Collections.Generic;
using CourseGuard.Core.Models;

namespace CourseGuard.Application.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Lấy toàn bộ danh sách người dùng.
        /// Sử dụng: Gọi _userRepository.GetAll().
        /// </summary>
        List<UserModel> GetAllUsers();

        /// <summary>
        /// Tìm kiếm người dùng theo tiêu chí.
        /// Sử dụng: Gọi _userRepository.Search().
        /// </summary>
        List<UserModel> SearchUsers(string username, string fullName);

        /// <summary>
        /// Thêm người dùng mới (bao gồm hash password).
        /// Sử dụng: Gọi PasswordHasher.HashPassword() rồi gọi _userRepository.Add().
        /// </summary>
        string AddUser(UserModel user, string password); // Returning success message or error

        /// <summary>
        /// Xóa người dùng.
        /// Sử dụng: Gọi _userRepository.Delete().
        /// </summary>
        bool DeleteUser(int userId);

        /// <summary>
        /// Lấy thông tin người dùng theo ID.
        /// Sử dụng: Gọi _userRepository.GetById().
        /// </summary>
        UserModel? GetUserById(int userId);

        /// <summary>
        /// Lấy danh sách giáo viên.
        /// Sử dụng: Gọi _userRepository.GetByRole("TEACHER").
        /// </summary>
        List<UserModel> GetTeachers();

        /// <summary>
        /// Lấy danh sách người dùng theo vai trò cụ thể.
        /// Sử dụng: Gọi _userRepository.GetByRole().
        /// </summary>
        List<UserModel> GetByRole(string roleName);

        /// <summary>
        /// Lấy dữ liệu hiển thị cho Dashboard admin.
        /// Sử dụng: Gọi _userRepository.GetDashboardData().
        /// </summary>
        List<UserDashboardDto> GetDashboardData();
        // Nhận mật khẩu mới từ UI, trả về thông báo kết quả
        string ChangePassword(int userId, string newPassword);
    }
}
