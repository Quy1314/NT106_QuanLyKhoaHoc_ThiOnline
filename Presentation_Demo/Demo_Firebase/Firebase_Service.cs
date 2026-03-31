using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Demo_Firebase
{
    /// <summary>
    /// Service tương tác với Firebase Realtime Database qua REST API.
    /// HttpClient được dùng chung (static) để tránh socket exhaustion.
    /// </summary>
    public class FirebaseService
    {
        // ✅ Dùng static HttpClient để tránh socket exhaustion
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly string _baseUrl;

        public FirebaseService(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        // ─────────────────────────────────────────────────────────────
        // 1. Đăng ký user (POST → Firebase tự tạo key)
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> RegisterUser(UserModel user)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(user),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/users.json", content);
            return response.IsSuccessStatusCode;
        }

        // ─────────────────────────────────────────────────────────────
        // 2. Lấy tất cả users (dưới dạng Dictionary<firebaseKey, user>)
        // ─────────────────────────────────────────────────────────────
        public async Task<Dictionary<string, UserModel>> GetAllUsers()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/users.json");

            if (!response.IsSuccessStatusCode)
                return [];

            var json = await response.Content.ReadAsStringAsync();

            if (json == "null")
                return [];

            return JsonSerializer.Deserialize<Dictionary<string, UserModel>>(json) ?? [];
        }

        // ─────────────────────────────────────────────────────────────
        // 3. Tìm user theo username
        // ─────────────────────────────────────────────────────────────
        public async Task<(string? userId, UserModel? user)> FindUser(string username)
        {
            var users = await GetAllUsers();

            foreach (var (key, user) in users)
            {
                if (user.Username == username)
                    return (key, user);
            }

            return (null, null);
        }

        // ─────────────────────────────────────────────────────────────
        // 4. Đăng nhập (kiểm tra username + password)
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> Login(string username, string password)
        {
            var (_, user) = await FindUser(username);
            return user?.Password == password;
        }

        // ─────────────────────────────────────────────────────────────
        // 5. Xóa user theo Firebase key
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> DeleteUser(string userId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/users/{userId}.json");
            return response.IsSuccessStatusCode;
        }
    }
}