/*
 * DatabaseAction.cs
 * 
 * Layer: Infrastructure
 * Vai trò: Lớp tiện ích cấp thấp (Helper), chứa chuỗi kết nối và các hàm thực thi SQL cơ bản.
 * Sử dụng: Các Repository sẽ gọi class này để chạy lệnh SQL.
 */
using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace CourseGuard.Backend.Data
{
    public static class DatabaseAction
    {
        private static readonly string connectionString =
            "Host=db.crtiwzjkcmpvyoqgdowv.supabase.co;Port=6543;Database=postgres;Username=postgres;Password=testdatabseuit;SSL Mode=Require;Trust Server Certificate=true;Timeout=15;";

        public static int ExecuteNonQuery(
            string query,
            Dictionary<string, (SqlDbType Type, object Value)>? parameters = null)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value.Value ?? DBNull.Value);
                    }
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object? ExecuteScalar(
            string query,
            Dictionary<string, (SqlDbType Type, object Value)>? parameters = null)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value.Value ?? DBNull.Value);
                    }
                }

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static DataTable ExecuteQuery(
            string query,
            Dictionary<string, (SqlDbType Type, object Value)>? parameters = null)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value.Value ?? DBNull.Value);
                    }
                }

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
