/*
 * PasswordHasher.cs
 * 
 * Layer: Core
 * Vai trò: Cung cấp hàm băm mật khẩu (SHA256) để bảo mật password người dùng.
 * Sử dụng: Được gọi bởi Service khi tạo user mới hoặc kiểm tra đăng nhập.
 */
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CourseGuard.Core.Security
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
