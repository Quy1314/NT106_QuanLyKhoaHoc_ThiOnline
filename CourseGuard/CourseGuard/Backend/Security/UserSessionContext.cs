namespace CourseGuard.Backend.Security
{
    public static class UserSessionContext
    {
        public static int? CurrentUserId { get; private set; }
        public static string CurrentRole { get; private set; } = string.Empty;
        public static string CurrentUsername { get; private set; } = string.Empty;

        public static void SetCurrentUser(int? userId, string? role, string? username)
        {
            CurrentUserId = userId;
            CurrentRole = (role ?? string.Empty).Trim().ToUpperInvariant();
            CurrentUsername = username ?? string.Empty;
        }

        public static void Clear()
        {
            CurrentUserId = null;
            CurrentRole = string.Empty;
            CurrentUsername = string.Empty;
        }

        public static bool IsAdmin()
        {
            return CurrentRole == "ADMIN";
        }
    }
}
