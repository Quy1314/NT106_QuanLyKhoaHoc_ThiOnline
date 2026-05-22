/*
 * NotificationRepository.cs
 *
 * Layer: Infrastructure / Data
 * Vai trò: Xử lý tất cả thao tác CSDL liên quan đến thông báo (notifications).
 *   - LoadByUserId: Lấy tất cả thông báo của một người dùng.
 *   - MarkAsRead: Đánh dấu thông báo là đã đọc.
 *   - Delete: Xóa thông báo.
 * Sử dụng: Được gọi bởi UC_Notification.
 *
 * Lưu ý:
 *   - Sử dụng DatabaseAction cho tất cả thao tác thực thi query.
 *   - Không trực tiếp khởi tạo kết nối CSDL tại đây.
 */
using System;
using System.Collections.Generic;
using System.Data;
using CourseGuard.Backend.Models;

namespace CourseGuard.Backend.Data
{
    /// <summary>
    /// Lớp Repository quản lý thao tác CSDL cho bảng notifications.
    /// </summary>
    public class NotificationRepository
    {
        /// <summary>
        /// Lấy tất cả thông báo, sắp xếp theo CreatedAt (mới nhất trước).
        /// </summary>
        /// <returns>Danh sách <see cref="NotificationModel"/> sắp xếp theo thời gian tạo (giảm dần).</returns>
        public List<NotificationModel> LoadAll()
        {
            var list = new List<NotificationModel>();

            const string sql = @"
                SELECT id, user_id, title, content, is_read, created_at
                FROM notifications
                ORDER BY created_at DESC;";

            DataTable dt = DatabaseAction.ExecuteQuery(sql, null);

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(MapRow(row));
                }
            }

            return list;
        }

        /// <summary>
        /// Lấy tất cả thông báo của một người dùng, sắp xếp theo CreatedAt (mới nhất trước).
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <returns>Danh sách <see cref="NotificationModel"/> sắp xếp theo thời gian tạo (giảm dần).</returns>
        public List<NotificationModel> LoadByUserId(int userId)
        {
            var list = new List<NotificationModel>();

            const string sql = @"
                SELECT id, user_id, title, content, is_read, created_at
                FROM notifications
                WHERE user_id = @userId
                ORDER BY created_at DESC;";

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@userId", (SqlDbType.Int, userId) }
            };

            DataTable dt = DatabaseAction.ExecuteQuery(sql, parameters);

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(MapRow(row));
                }
            }

            return list;
        }

        /// <summary>
        /// Đánh dấu một thông báo là đã đọc.
        /// </summary>
        /// <param name="notificationId">ID của thông báo cần đánh dấu.</param>
        /// <returns>true nếu cập nhật thành công, false nếu không tìm thấy thông báo.</returns>
        public bool MarkAsRead(int notificationId)
        {
            const string sql = @"
                UPDATE notifications
                SET is_read = true
                WHERE id = @id;";

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@id", (SqlDbType.Int, notificationId) }
            };

            int affected = DatabaseAction.ExecuteNonQuery(sql, parameters);

            return affected > 0;
        }

        /// <summary>
        /// Xóa một thông báo.
        /// </summary>
        /// <param name="notificationId">ID của thông báo cần xóa.</param>
        /// <returns>true nếu xóa thành công, false nếu không tìm thấy thông báo.</returns>
        public bool Delete(int notificationId)
        {
            const string sql = @"
                DELETE FROM notifications
                WHERE id = @id;";

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@id", (SqlDbType.Int, notificationId) }
            };

            int affected = DatabaseAction.ExecuteNonQuery(sql, parameters);

            return affected > 0;
        }

        /// <summary>
        /// Ánh xạ một hàng từ DataRow vào NotificationModel.
        /// </summary>
        private static NotificationModel MapRow(DataRow row)
        {
            return new NotificationModel
            {
                Id = Convert.ToInt32(row["id"]),
                UserId = Convert.ToInt32(row["user_id"]),
                Title = row["title"] == DBNull.Value ? string.Empty : Convert.ToString(row["title"]) ?? string.Empty,
                Content = row["content"] == DBNull.Value ? string.Empty : Convert.ToString(row["content"]) ?? string.Empty,
                IsRead = Convert.ToBoolean(row["is_read"]),
                CreatedAt = Convert.ToDateTime(row["created_at"])
            };
        }
    }
}
