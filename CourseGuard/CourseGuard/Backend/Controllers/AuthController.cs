using System;
using System.Threading;
using System.Threading.Tasks;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using Npgsql;

namespace CourseGuard.Backend.Controllers
{
    /// <summary>
    /// Authentication Controller carrying direct business logic.
    /// Simplified architecture: No AuthRepository, No AuthService.
    /// </summary>
    public class AuthController
    {
        private readonly CourseGuardDbContext _dbContext;
        public string LastErrorMessage { get; private set; } = string.Empty;

        public AuthController(CourseGuardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public UserModel? Login(string username, string password)
        {
            if (!UserIdentityBloomIndex.UsernameExists(_dbContext, username))
            {
                return null;
            }

            var user = _dbContext.GetUserByUsername(username);
            
            if (user == null)
            {
                return null;
            }

            if (PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task<UserModel?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            if (!UserIdentityBloomIndex.UsernameExists(_dbContext, username))
            {
                return null;
            }

            var user = await _dbContext.GetUserByUsernameAsync(username, cancellationToken);

            if (user == null)
            {
                return null;
            }

            return PasswordHasher.VerifyPassword(password, user.PasswordHash) ? user : null;
        }

        public Task<UserModel?> GetUserProfileAsync(string username, CancellationToken cancellationToken = default)
        {
            return _dbContext.GetUserByUsernameAsync(username, cancellationToken);
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

        public async Task UpdateLoginInfoAsync(int userId, string deviceName, string ipAddress, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.LogDeviceActivityAsync(userId, deviceName, ipAddress, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging error: {ex.Message}");
            }
        }

        public void LogUserActivity(int? userId, string action, string details, string ipAddress)
        {
            _dbContext.LogUserActivity(userId, action, details, ipAddress);
        }

        public Task LogUserActivityAsync(int? userId, string action, string details, string ipAddress, CancellationToken cancellationToken = default)
        {
            return _dbContext.LogUserActivityAsync(userId, action, details, ipAddress, cancellationToken);
        }

        public bool RegisterRequest(UserModel user, string password)
        {
            LastErrorMessage = string.Empty;

            // Simple validation
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(user.Email))
            {
                LastErrorMessage = "Thiếu username/email/password.";
                return false;
            }

            if (password.Length < 6)
            {
                LastErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.";
                return false;
            }

            // 1. Manually check for existing username to provide better UX
            if (UserIdentityBloomIndex.UsernameExists(_dbContext, user.Username))
            {
                Console.WriteLine("User registration failed: Username already exists.");
                LastErrorMessage = "Tên đăng nhập đã tồn tại trong hệ thống.";
                return false; 
            }
            if (UserIdentityBloomIndex.EmailExists(_dbContext, user.Email))
            {
                LastErrorMessage = "Email đã tồn tại trong hệ thống.";
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
                var createdUser = _dbContext.GetUserByUsername(user.Username);
                UserIdentityBloomIndex.RegisterUserIdentity(user.Username, user.Email);
                _dbContext.LogUserActivity(createdUser?.Id, "SIGNUP", $"New signup request: {user.Username}", string.Empty);
                return true;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                LastErrorMessage = "Tên đăng nhập hoặc email đã tồn tại.";
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                LastErrorMessage = $"Lỗi lưu user vào database: {ex.Message}";
                return false;
            }
        }

        public bool ForgotPasswordRequest(string username, string email)
        {
            if (!UserIdentityBloomIndex.UsernameExists(_dbContext, username) ||
                !UserIdentityBloomIndex.EmailExists(_dbContext, email))
            {
                return false;
            }

            var user = _dbContext.GetUserByUsernameAndEmail(username, email);
            if (user != null)
            {
                // Update status to RESET_REQUEST as requested
                _dbContext.UpdateUserStatus(user.Id, "RESET_REQUEST");
                _dbContext.LogUserActivity(user.Id, "FORGOT_PASSWORD", $"Forgot password request: {user.Username}", string.Empty);
                return true;
            }
            return false;
        }

        public void Logout(int? userId = null, string username = "", string ipAddress = "")
        {
            // In a production app, we would clear JWT tokens or Session state.
            // For this local WinForms app, we log the event and navigate back.
            Console.WriteLine("Auth: User session cleared.");
            _dbContext.LogUserActivity(userId, "LOGOUT", $"Logout: {username}", ipAddress);
            UserSessionContext.Clear();
        }
    }
}
