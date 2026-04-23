using System;

namespace CourseGuard.Backend.Config
{
    internal static class AppEnvironment
    {
        public static string GetRequired(params string[] keys)
        {
            foreach (string key in keys)
            {
                string? value = Environment.GetEnvironmentVariable(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            throw new InvalidOperationException(
                $"Missing required environment variable. Expected one of: {string.Join(", ", keys)}");
        }

        public static string? GetOptional(params string[] keys)
        {
            foreach (string key in keys)
            {
                string? value = Environment.GetEnvironmentVariable(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }
    }
}
