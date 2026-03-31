using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace WebService_Demo.Services
{
    public static class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5199/")
        };

        // Token hiện tại sau khi đăng nhập
        public static string? CurrentToken { get; private set; }

        // Lưu token vào header Authorization
        public static void SetToken(string token)
        {
            CurrentToken = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Xóa token khi đăng xuất
        public static void ClearToken()
        {
            CurrentToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // ──────────────────────────────────────────────
        //  Đăng nhập
        // ──────────────────────────────────────────────
        public static async Task<(bool success, string? token, string message)> LoginAsync(
            string username, string password)
        {
            try
            {
                var payload = new { Username = username, Password = password };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/auth/login", content);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                bool success = root.TryGetProperty("success", out var s) && s.GetBoolean();
                string message = root.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";
                string? token = root.TryGetProperty("token", out var t) ? t.GetString() : null;

                // ✅ Lưu token vào ApiService sau khi đăng nhập thành công
                if (success && !string.IsNullOrEmpty(token))
                    SetToken(token);

                return (success, token, message);
            }
            catch (HttpRequestException)
            {
                return (false, null, "Không thể kết nối tới máy chủ. Vui lòng kiểm tra Web API đã chạy chưa.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        //  Đăng ký
        // ──────────────────────────────────────────────
        public static async Task<(bool success, string message)> RegisterAsync(
            string email, string password, string confirmPassword)
        {
            try
            {
                var payload = new
                {
                    Email = email,
                    Password = password,
                    ConfirmPassword = confirmPassword
                };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/auth/register", content);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                bool success = root.TryGetProperty("success", out var s) && s.GetBoolean();
                string message = root.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";

                return (success, message);
            }
            catch (HttpRequestException)
            {
                return (false, "Không thể kết nối tới máy chủ. Vui lòng kiểm tra Web API đã chạy chưa.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        //  Quên mật khẩu (đổi mật khẩu qua email)
        // ──────────────────────────────────────────────
        public static async Task<(bool success, string message)> ForgotPasswordAsync(
            string email, string newPassword, string confirmNewPassword)
        {
            try
            {
                var payload = new
                {
                    Email = email,
                    NewPassword = newPassword,
                    ConfirmNewPassword = confirmNewPassword
                };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/auth/forgot-password", content);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                bool success = root.TryGetProperty("success", out var s) && s.GetBoolean();
                string message = root.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";

                return (success, message);
            }
            catch (HttpRequestException)
            {
                return (false, "Không thể kết nối tới máy chủ. Vui lòng kiểm tra Web API đã chạy chưa.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        //  Đăng xuất
        // ──────────────────────────────────────────────
        public static async Task<(bool success, string message)> LogoutAsync()
        {
            try
            {
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/auth/logout", content);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                bool success = root.TryGetProperty("success", out var s) && s.GetBoolean();
                string message = root.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";

                // ✅ Xóa token sau khi đăng xuất thành công
                if (success)
                    ClearToken();

                return (success, message);
            }
            catch (HttpRequestException)
            {
                // Nếu API offline vẫn cho phép đăng xuất phía client
                ClearToken();
                return (true, "Đăng xuất thành công!");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }
    }
}