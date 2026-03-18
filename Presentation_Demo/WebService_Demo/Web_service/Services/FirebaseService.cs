using System.Text.Json;

public class FirebaseService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl = "https://couresguard-default-rtdb.asia-southeast1.firebasedatabase.app";

    public FirebaseService(HttpClient http)
    {
        _http = http;
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
    public async Task<UserModel?> FindUser(string username)
    {
        var users = await GetUsers();

        return users.Values.FirstOrDefault(x => x.username == username);
    }
}
