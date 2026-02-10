using System;
/*
 * UserService.cs
 * 
 * Layer: Application (Services)
 * Vai trò: Thực thi logic nghiệp vụ cho User. Nhận yêu cầu từ UI, xử lý logic (nếu có), rồi gọi Repository.
 * Phụ thuộc: IUserRepository (thông qua Dependency Injection).
 */
using System.Collections.Generic;
using CourseGuard.Application.Interfaces;
using CourseGuard.Core.Models;
using CourseGuard.Core.Security;

namespace CourseGuard.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Lấy tất cả user.
        /// Sử dụng: Gọi _userRepository.GetAll().
        /// </summary>
        public List<UserModel> GetAllUsers()
        {
            return _userRepository.GetAll();
        }

        /// <summary>
        /// Tìm kiếm user.
        /// Sử dụng: Gọi _userRepository.Search().
        /// </summary>
        public List<UserModel> SearchUsers(string username, string fullName)
        {
            return _userRepository.Search(username, fullName);
        }

        /// <summary>
        /// Thêm user mới với logic nghiệp vụ.
        /// Sử dụng: 
        /// 1. Validate input.
        /// 2. Kiểm tra trùng username bằng GetByUsername().
        /// 3. Hash mật khẩu bằng PasswordHasher.HashPassword().
        /// 4. Gọi _userRepository.Add().
        /// </summary>
        public string AddUser(UserModel user, string password)
        {
            // Business Validation
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(password))
            {
                return "Username and Password are required.";
            }

            // Check if user exists
            var existingUser = _userRepository.GetByUsername(user.Username);
            if (existingUser != null)
            {
                return "Username already exists.";
            }

            // Hash Password
            string passwordHash = PasswordHasher.HashPassword(password);

            // Add to Repo
            int result = _userRepository.Add(user, passwordHash);
            
            return result > 0 ? "Success" : "Failed to add user.";
        }

        /// <summary>
        /// Xóa user.
        /// Sử dụng: Gọi _userRepository.Delete().
        /// </summary>
        public bool DeleteUser(int userId)
        {
            return _userRepository.Delete(userId);
        }

        /// <summary>
        /// Lấy user theo ID.
        /// Sử dụng: Gọi _userRepository.GetById().
        /// </summary>
        public UserModel? GetUserById(int userId)
        {
            return _userRepository.GetById(userId);
        }

        /// <summary>
        /// Lấy danh sách giáo viên.
        /// Sử dụng: Gọi _userRepository.GetByRole("TEACHER").
        /// </summary>
        public List<UserModel> GetTeachers()
        {
            return _userRepository.GetByRole("TEACHER");
        }

        /// <summary>
        /// Lấy user theo role.
        /// Sử dụng: Gọi _userRepository.GetByRole().
        /// </summary>
        public List<UserModel> GetByRole(string roleName)
        {
            return _userRepository.GetByRole(roleName);
        }

        /// <summary>
        /// Lấy dữ liệu Dashboard.
        /// Sử dụng: Gọi _userRepository.GetDashboardData().
        /// </summary>
        public List<UserDashboardDto> GetDashboardData()
        {
            return _userRepository.GetDashboardData();
        }
        public string ChangePassword(int userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return "New password cannot be empty.";
            }
            string newHashedPassword = PasswordHasher.HashPassword(newPassword);
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                return "User not found.";
            }
            bool updateResult = _userRepository.UpdatePassword(userId, newHashedPassword);
            return updateResult ? "Password updated successfully." : "Failed to update password.";
        }
    }
}
