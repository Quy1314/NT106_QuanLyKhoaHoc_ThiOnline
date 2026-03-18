using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Demo_Firebase
{
    public class Firebase_Service
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public Firebase_Service(string baseUrl)
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl.TrimEnd('/');
        }

        // ➕ 1. Đăng ký (POST → auto key)
        public async Task<bool> RegisterUser(UserModel user)
        {
            var jsonContent = JsonSerializer.Serialize(user);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/users.json", content);
            return response.IsSuccessStatusCode;
        }

        // 📥 2. Lấy tất cả user
        public async Task<Dictionary<string, UserModel>> GetUsersWithId()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/users.json");

            if (!response.IsSuccessStatusCode)
                return new Dictionary<string, UserModel>();

            var json = await response.Content.ReadAsStringAsync();

            if (json == "null")
                return new Dictionary<string, UserModel>();

            var result = JsonSerializer.Deserialize<Dictionary<string, UserModel>>(json);
            return result ?? new Dictionary<string, UserModel>();
        }

        // 🔍 3. Tìm user theo username
        public async Task<(string? userId, UserModel? user)> FindUser(string username)
        {
            var users = await GetUsersWithId();

            foreach (var item in users)
            {
                if (item.Value.username == username)
                {
                    return (item.Key, item.Value);
                }
            }

            return (null, null);
        }

        // 🔐 4. Login
        public async Task<bool> Login(string username, string password)
        {
            var result = await FindUser(username);

            if (result.user == null)
                return false;

            return result.user.password == password;
        }

        // ❌ 5. Xóa user
        public async Task<bool> DeleteUser(string userId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/users/{userId}.json");
            return response.IsSuccessStatusCode;
        }
    }
}