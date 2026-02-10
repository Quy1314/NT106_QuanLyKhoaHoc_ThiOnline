/*
 * AuthService.cs
 * 
 * Layer: Application (Services)
 * Vai trò: Xử lý logic Đăng nhập. Kiểm tra user tồn tại và mật khẩu trùng khớp (dùng PasswordHasher).
 * Phụ thuộc: IUserRepository.
 */
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using CourseGuard.Core.Models;
using CourseGuard.Core.Security; 
using CourseGuard.Infrastructure.Data;
using CourseGuard.Application.Interfaces;

namespace CourseGuard.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Đăng nhập hệ thống.
        /// Sử dụng: 
        /// 1. Tìm user bằng _userRepository.GetByUsername().
        /// 2. Hash mật khẩu đầu vào bằng PasswordHasher.HashPassword().
        /// 3. So sánh Hash đầu vào với Hash trong database.
        /// </summary>
        public UserModel? Login(string username, string password)
        {
            // 1. Get User by Username
            var user = _userRepository.GetByUsername(username);
            if (user == null)
            {
                return null;
            }

            // 2. Hash Password and Compare
            // Note: In a real app, we should compare hash of input with stored hash.
            // The previous logic hashed the input and checked if it matches DB.
            // But we need to know if the stored password in DB is the hash or if we need to verify.
            // Previous code: WHERE u.password_hash = @password_hash
            // So we just hash the input and compare strings.
            
            string inputHash = PasswordHasher.HashPassword(password);
            
            // We need to fetch the password hash from DB to compare.
            // But UserModel (Core) might not have PasswordHash property unless I added it?
            // I added PasswordHash to UserModel in previous step!
            
            if (user.PasswordHash == inputHash)
            {
                return user;
            }

            return null;
        }
    }
}
