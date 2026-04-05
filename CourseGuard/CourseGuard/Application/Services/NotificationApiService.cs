using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseGuard.Application.Services
{
    public enum NotificationType { Alert, Info, Success }

    public class NotificationModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Time { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public string ActionText { get; set; }
    }

    public class NotificationApiService
    {
        private readonly HttpClient _httpClient;

        public NotificationApiService()
        {
            _httpClient = new HttpClient();
            // Dựa vào Port 5248 từ file appsettings.json của Web_service
            _httpClient.BaseAddress = new Uri("http://localhost:5248/");
        }

        public async Task<List<NotificationModel>> GetNotificationsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/notification/{userId}");
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<NotificationModel>>(json, options);
        }
    }
}
