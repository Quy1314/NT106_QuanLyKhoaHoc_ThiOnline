using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Web_service.Controllers
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

    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly string _connectionString;

        public NotificationController(IConfiguration config)
        {
            // Cố gắng đọc từ cấu hình appsettings.json, nếu chưa có thì dùng LocalDB tạm thời. 
            // Bạn hãy nhớ thêm "ConnectionStrings": { "DefaultConnection": "..." } vào appsettings.json.
            _connectionString = config.GetConnectionString("DefaultConnection") 
                ?? "Server=localhost;Database=CourseGuardDB;Trusted_Connection=True;Encrypt=False;";
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            var notifications = new List<NotificationModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "SELECT Id, Title, Content, Time, Type, IsRead, ActionText FROM NOTIFICATIONS WHERE UserId = @UserId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                notifications.Add(new NotificationModel
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Content = reader.GetString(2),
                                    Time = reader.GetString(3),
                                    Type = (NotificationType)reader.GetInt32(4),
                                    IsRead = reader.GetBoolean(5),
                                    ActionText = reader.IsDBNull(6) ? "" : reader.GetString(6)
                                });
                            }
                        }
                    }
                }
                
                return Ok(notifications);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { message = "Lỗi kết nối CSDL SQL Server: " + ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống Web API: " + ex.Message });
            }
        }
    }
}
