using System;
using System.Collections.Generic;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
namespace CourseGuard.Backend.Controllers
{
    public class UserController
    {
        private readonly CourseGuardDbContext _dbContext;

        public UserController(CourseGuardDbContext dbContext)
        {
            _dbContext = dbContext;
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
            // TODO: Implement direct DbContext delete logic
            return true;
        }

        public List<UserModel> GetPendingRequests()
        {
            // Lấy danh sách user có status là PENDING hoặc RESET_REQUEST
            return _dbContext.GetUsersByStatus("PENDING", "RESET_REQUEST");
        }

        public bool ApproveUserRequest(int userId, string action)
        {
            // Simplified approval logic
            if (action == "APPROVE")
            {
                return ApproveRegistration(userId);
            }
            else if (action == "REJECT")
            {
                _dbContext.UpdateUserStatus(userId, "REJECTED");
                return true;
            }
            return false;
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
