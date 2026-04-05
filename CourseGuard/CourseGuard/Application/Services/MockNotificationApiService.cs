using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseGuard.Application.Services
{
    public class MockNotificationApiService
    {
        public static bool IsApiOnline = true;

        private List<NotificationModel> mockData = new List<NotificationModel>
        {
            new NotificationModel { Id = 1, Title = "Cảnh báo hệ thống", Content = "Hệ thống sẽ bảo trì vào lúc 00:00 ngày mai.", Time = "10 phút trước", Type = NotificationType.Alert, IsRead = false, ActionText = "Xem chi tiết" },
            new NotificationModel { Id = 2, Title = "Phân công giảng dạy mới", Content = "Bạn vừa được phân công dạy lớp Lập trình mạng nâng cao.", Time = "1 giờ trước", Type = NotificationType.Info, IsRead = false, ActionText = "" },
            new NotificationModel { Id = 3, Title = "Hoàn thành chấm thi", Content = "Hệ thống đã ghi nhận điểm thi lớp Cơ sở dữ liệu.", Time = "Hôm qua", Type = NotificationType.Success, IsRead = true, ActionText = "" },
            new NotificationModel { Id = 4, Title = "Thông báo họp bộ môn", Content = "Cuộc họp bộ môn sẽ diễn ra vào sáng thứ 2 tuần sau lúc 8:00.", Time = "2 ngày trước", Type = NotificationType.Info, IsRead = true, ActionText = "Xác nhận tham gia" },
            new NotificationModel { Id = 5, Title = "Phản hồi từ sinh viên", Content = "Có 5 tin nhắn mới từ sinh viên lớp Kỹ thuật phần mềm.", Time = "3 ngày trước", Type = NotificationType.Info, IsRead = true, ActionText = "Xem tin nhắn" }
        };

        public async Task<List<NotificationModel>> GetNotificationsAsync(int userId)
        {
            await Task.Delay(1500); // Giả lập mạng chậm

            if (!IsApiOnline)
            {
                throw new Exception("500 Internal Server Error: Không thể kết nối đến máy chủ.");
            }

            return mockData;
        }
    }
}
