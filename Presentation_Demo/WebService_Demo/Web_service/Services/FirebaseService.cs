using System.Text.Json;

public class FirebaseService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl = "https://couresguard-default-rtdb.asia-southeast1.firebasedatabase.app";

    public FirebaseService(HttpClient http)
    {
        _http = http;
    }

    // ➕ Đăng ký (POST → auto key)
    public async Task<bool> RegisterUser(UserModel user)
    {
        var jsonContent = JsonSerializer.Serialize(user);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await _http.PostAsync($"{_baseUrl}/users.json", content);
        return response.IsSuccessStatusCode;
    }

    // 📥 Lấy tất cả users
    public async Task<Dictionary<string, UserModel>> GetUsers()
    {
        var res = await _http.GetAsync($"{_baseUrl}/users.json");

        if (!res.IsSuccessStatusCode)
            return new Dictionary<string, UserModel>();

        var json = await res.Content.ReadAsStringAsync();

        if (json == "null")
            return new Dictionary<string, UserModel>();

        return JsonSerializer.Deserialize<Dictionary<string, UserModel>>(json)
               ?? new Dictionary<string, UserModel>();
    }

    // 🔍 Tìm user
    public async Task<(string? userId, UserModel? user)> FindUser(string username)
    {
        var users = await GetUsers();

        foreach (var item in users)
        {
            if (item.Value.username == username)
            {
                return (item.Key, item.Value);
            }
        }

        return (null, null);
    }

    // ✏️ Cập nhật password (dùng PATCH để chỉ update field password)
    public async Task<bool> UpdatePassword(string userId, string newPassword)
    {
        var patch = new { password = newPassword };
        var jsonContent = JsonSerializer.Serialize(patch);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_baseUrl}/users/{userId}.json")
        {
            Content = content
        };

        var response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}
