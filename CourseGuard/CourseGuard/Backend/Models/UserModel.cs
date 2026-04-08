/*
 * UserModel.cs
 * 
 * Layer: Core
 * Vai trò: Định nghĩa đối tượng Người dùng (User) với các thuộc tính như Id, Username, PasswordHash, Role, v.v.
 * Sử dụng: Được dùng ở tất cả các layer để truyền dữ liệu người dùng đi khắp hệ thống.
 */
using System;

namespace CourseGuard.Backend.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
