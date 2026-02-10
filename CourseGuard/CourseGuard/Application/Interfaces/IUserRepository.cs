/*
 * IUserRepository.cs
 * 
 * Layer: Application (Interfaces)
 * Vai trò: Định nghĩa hợp đồng (contract) cho Repository User, giúp Service không phụ thuộc trực tiếp vào Repository cụ thể.
 * Sử dụng: Được implement bởi UserRepository và sử dụng bởi UserService/AuthService.
 */
using System.Collections.Generic;
using CourseGuard.Core.Models;

namespace CourseGuard.Application.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// Lấy tất cả người dùng trong hệ thống.
        /// Sử dụng: SELECT * FROM USERS (thông qua DatabaseAction.ExecuteQuery).
        /// </summary>
        List<UserModel> GetAll();

        /// <summary>
        /// Tìm kiếm người dùng theo tên đăng nhập hoặc họ tên.
        /// Sử dụng: SELECT ... WHERE ... LIKE ... (thông qua DatabaseAction.ExecuteQuery).
        /// </summary>
        List<UserModel> Search(string username, string fullName);

        /// <summary>
        /// Thêm người dùng mới vào database.
        /// Sử dụng: INSERT INTO USERS (thông qua DatabaseAction.ExecuteNonQuery).
        /// </summary>
        int Add(UserModel user, string passwordHash);

        /// <summary>
        /// Xóa người dùng theo ID.
        /// Sử dụng: DELETE FROM USERS WHERE ID = ... (thông qua DatabaseAction.ExecuteNonQuery).
        /// </summary>
        bool Delete(int id);

        /// <summary>
        /// Lấy thông tin chi tiết người dùng theo ID.
        /// Sử dụng: SELECT ... WHERE ID = ... (thông qua DatabaseAction.ExecuteQuery).
        /// </summary>
        UserModel? GetById(int id);

        /// <summary>
        /// Lấy thông tin người dùng theo Tên đăng nhập.
        /// Sử dụng: SELECT ... WHERE USERNAME = ... (thông qua DatabaseAction.ExecuteQuery).
        /// </summary>
        UserModel? GetByUsername(string username);

        /// <summary>
        /// Lấy danh sách người dùng theo Vai trò (Role).
        /// Sử dụng: SELECT ... JOIN ROLES ... WHERE NAME = ... (thông qua DatabaseAction.ExecuteQuery).
        /// </summary>
        List<UserModel> GetByRole(string roleName);

        /// <summary>
        /// Lấy dữ liệu cho trang Dashboard (thông tin user, last login, ip).
        /// Sử dụng: Phức thợp query SELECT với subquery lấy từ bảng DEVICES.
        /// </summary>
        List<UserDashboardDto> GetDashboardData();

        /// <summary>
        /// Cập nhật thông tin thiết bị/IP khi đăng nhập.
        /// Sử dụng: Kiểm tra tồn tại trong bảng DEVICES, nếu có thì UPDATE, chưa có thì INSERT.
        /// </summary>
        bool UpdateDevice(int userId, string deviceName, string ipAddress);
        /// <summary>
        /// Cập nhật mật khẩu người dùng.
        /// </summary>
        bool UpdatePassword(int userId, string newPasswordHash);
    }
}
