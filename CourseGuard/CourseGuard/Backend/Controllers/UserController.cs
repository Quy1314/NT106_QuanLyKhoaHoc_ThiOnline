using System;
using System.Collections.Generic;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Services;
namespace CourseGuard.Backend.Controllers
{
    public class UserController
    {
        private readonly CourseGuardDbContext _dbContext;
        private readonly SmtpEmailService _smtpEmailService;
        public string LastErrorMessage { get; private set; } = string.Empty;

        public UserController(CourseGuardDbContext dbContext)
        {
            _dbContext = dbContext;
            _smtpEmailService = new SmtpEmailService();
        }

        // Placeholder methods to replace the former IUserService
        public List<UserModel> SearchUsers(string status, string role)
        {
            return _dbContext.SearchUsers(status, role);
        }

        public List<UserModel> GetByRole(string role)
        {
            return new List<UserModel>();
        }

        public object? GetDashboardData()
        {
            return null;
        }

        public string AddUser(UserModel user, string password)
        {
            // TODO: Implement direct DbContext add logic
            return "Success";
        }

        public bool DeleteUser(int userId)
        {
            LastErrorMessage = string.Empty;
            try
            {
                bool deleted = _dbContext.DeleteUser(userId);
                if (!deleted)
                {
                    LastErrorMessage = "Không thể xóa user (có thể là tài khoản ADMIN hoặc user không tồn tại).";
                }
                return deleted;
            }
            catch (Exception ex)
            {
                LastErrorMessage = $"Không thể xóa user do ràng buộc dữ liệu: {ex.Message}";
                return false;
            }
        }

        public List<UserModel> GetPendingRequests()
        {
            // Lấy danh sách user có status là PENDING hoặc RESET_REQUEST
            return _dbContext.GetUsersByStatus("PENDING", "RESET_REQUEST");
        }

        public AdminDashboardMetricsModel GetAdminDashboardMetrics()
        {
            return _dbContext.GetAdminDashboardMetrics();
        }

        public List<RecentUserActivityModel> GetRecentAuthActivities(int limit = 20)
        {
            return _dbContext.GetRecentUserActivities(limit);
        }

        public bool ApproveUserRequest(int userId, string action)
        {
            LastErrorMessage = string.Empty;

            // Simplified approval logic
            if (action == "APPROVE")
            {
                return ApproveRegistration(userId);
            }
            else if (action == "RESET")
            {
                var user = _dbContext.GetUserById(userId);
                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                {
                    LastErrorMessage = "Không tìm thấy user hoặc user chưa có email.";
                    return false;
                }

                string temporaryPassword = GenerateTemporaryPassword();
                string emailBody =
                    $"Xin chào {user.FullName},\n\n" +
                    "Admin đã duyệt yêu cầu quên mật khẩu cho tài khoản CourseGuard của bạn.\n" +
                    $"Mật khẩu tạm thời của bạn là: {temporaryPassword}\n\n" +
                    "Vui lòng đăng nhập và đổi mật khẩu ngay sau khi vào hệ thống.\n\n" +
                    "CourseGuard Admin";

                bool emailSent = _smtpEmailService.SendEmail(
                    user.Email,
                    "CourseGuard - Cap lai mat khau",
                    emailBody,
                    out string errorMessage);

                if (!emailSent)
                {
                    LastErrorMessage = errorMessage;
                    return false;
                }

                string passwordHash = PasswordHasher.HashPassword(temporaryPassword);
                _dbContext.UpdateUserPassword(userId, passwordHash);
                _dbContext.UpdateUserStatus(userId, "ACTIVE");
                _dbContext.LogUserActivity(userId, "FORGOT_PASSWORD_APPROVED", $"Admin sent temporary password for: {user.Username}", string.Empty);
                return true;
            }
            else if (action == "REJECT")
            {
                _dbContext.UpdateUserStatus(userId, "REJECTED");
                return true;
            }

            LastErrorMessage = "Action không hợp lệ.";
            return false;
        }

        private static string GenerateTemporaryPassword()
        {
            return $"CG{Guid.NewGuid():N}".Substring(0, 10);
        }

        public bool ApproveRegistration(int userId)
        {
            try
            {
                _dbContext.UpdateUserStatus(userId, "ACTIVE");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ResetUserPassword(int userId, string newPassword)
        {
            try
            {
                string passwordHash = PasswordHasher.HashPassword(newPassword);
                _dbContext.UpdateUserPassword(userId, passwordHash);
                _dbContext.UpdateUserStatus(userId, "ACTIVE"); // Chuyển về ACTIVE sau khi reset
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
