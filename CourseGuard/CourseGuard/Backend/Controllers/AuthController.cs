using System;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;

namespace CourseGuard.Backend.Controllers
{
    /// <summary>
    /// Authentication Controller carrying direct business logic.
    /// Simplified architecture: No AuthRepository, No AuthService.
    /// </summary>
    public class AuthController
    {
        private readonly CourseGuardDbContext _dbContext;

        public AuthController(CourseGuardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public UserModel? Login(string username, string password)
        {
            var user = _dbContext.GetUserByUsername(username);
            
            if (user == null)
            {
                return null;
            }

            string inputHash = PasswordHasher.HashPassword(password);
            
            if (user.PasswordHash == inputHash)
            {
                return user;
            }

            return null;
        }

        public void UpdateLoginInfo(int userId, string deviceName, string ipAddress)
        {
            try
            {
                _dbContext.LogDeviceActivity(userId, deviceName, ipAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging error: {ex.Message}");
            }
        }

        public bool RegisterRequest(UserModel user, string password)
        {
            // Simple validation
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(password))
                return false;

            // 1. Manually check for existing username to provide better UX
            if (_dbContext.UserExists(user.Username))
            {
                Console.WriteLine("User registration failed: Username already exists.");
                return false; 
            }

            // Hash password
            string passwordHash = PasswordHasher.HashPassword(password);
            
            // Set default status and role as requested
            user.Status = "PENDING";
            user.Role = "STUDENT";

            try
            {
                _dbContext.InsertUser(user, passwordHash);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public bool ForgotPasswordRequest(string username, string email)
        {
            var user = _dbContext.GetUserByUsernameAndEmail(username, email);
            if (user != null)
            {
                // Update status to RESET_REQUEST as requested
                _dbContext.UpdateUserStatus(user.Id, "RESET_REQUEST");
                return true;
            }
            return false;
        }

        public void Logout()
        {
            // In a production app, we would clear JWT tokens or Session state.
            // For this local WinForms app, we log the event and navigate back.
            Console.WriteLine("Auth: User session cleared.");
        }
    }
}
