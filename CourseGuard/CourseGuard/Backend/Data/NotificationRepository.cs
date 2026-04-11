/*
 * NotificationRepository.cs
 *
 * Layer: Infrastructure / Data
 * Vai trò: Xử lý tất cả thao tác CSDL liên quan đến thông báo (NOTIFICATIONS).
 *   - LoadByUserId: Lấy tất cả thông báo của một người dùng.
 *   - MarkAsRead: Đánh dấu thông báo là đã đọc.
 *   - Delete: Xóa thông báo.
 * Sử dụng: Được gọi bởi UC_Notification.
 *
 * Lưu ý:
 *   - Chuỗi kết nối sử dụng cùng endpoint Supabase như các Repository khác.
 *   - Hỗ trợ lấy dữ liệu và cập nhật trạng thái (is_read).
 */
using System;
using System.Collections.Generic;
using Npgsql;
using CourseGuard.Backend.Models;

namespace CourseGuard.Backend.Data
{
    /// <summary>
    /// Lớp Repository quản lý thao tác CSDL cho bảng NOTIFICATIONS.
    /// </summary>
    public class NotificationRepository
    {
        // --- Chuỗi kết nối Supabase ---
        // Sử dụng cùng một chuỗi kết nối với phần còn lại của dự án.
        // Port 6543 là Transaction Pooler của Supabase (phù hợp cho ứng dụng desktop).
        private readonly string _connectionString =
            "Host=db.crtiwzjkcmpvyoqgdowv.supabase.co;Port=6543;Database=postgres;Username=postgres;Password=testdatabseuit;SSL Mode=Require;Trust Server Certificate=true;Timeout=15;";

        /// <summary>
        /// Lấy tất cả thông báo của một người dùng, sắp xếp theo CreatedAt (mới nhất trước).
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <returns>Danh sách <see cref="NotificationModel"/> sắp xếp theo thời gian tạo (giảm dần).</returns>
        public List<NotificationModel> LoadByUserId(int userId)
        {
            var list = new List<NotificationModel>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT id, user_id, title, content, is_read, created_at
                FROM NOTIFICATIONS
                WHERE user_id = @userId
                ORDER BY created_at DESC;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapRow(reader));
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
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                UPDATE NOTIFICATIONS
                SET is_read = true
                WHERE id = @id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", notificationId);
            int affected = cmd.ExecuteNonQuery();

            return affected > 0;
        }

        /// <summary>
        /// Xóa một thông báo.
        /// </summary>
        /// <param name="notificationId">ID của thông báo cần xóa.</param>
        /// <returns>true nếu xóa thành công, false nếu không tìm thấy thông báo.</returns>
        public bool Delete(int notificationId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                DELETE FROM NOTIFICATIONS
                WHERE id = @id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", notificationId);
            int affected = cmd.ExecuteNonQuery();

            return affected > 0;
        }

        /// <summary>
        /// Ánh xạ một hàng từ DataReader vào NotificationModel.
        /// </summary>
        private static NotificationModel MapRow(NpgsqlDataReader reader)
        {
            return new NotificationModel
            {
                Id = reader.GetInt32(0),                      // id
                UserId = reader.GetInt32(1),                  // user_id
                Title = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),      // title
                Content = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),    // content
                IsRead = reader.GetBoolean(4),               // is_read
                CreatedAt = reader.GetDateTime(5)            // created_at
            };
        }
    }
}
