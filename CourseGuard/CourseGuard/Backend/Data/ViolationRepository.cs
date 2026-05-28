using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseGuard.Backend.Config;
using CourseGuard.Backend.Models;
using Npgsql;

namespace CourseGuard.Backend.Data
{
    public class ViolationRepository
    {
        private readonly string _connectionString = AppEnvironment.GetRequired(
            "COURSEGUARD_DB_CONNECTION",
            "SUPABASE_DB_CONNECTION",
            "CONNECTION_STRING");

        public async Task<int> InsertViolationAsync(ViolationModel violation)
        {
            const string sql = @"
                INSERT INTO violations (user_id, exam_attempt_id, type, image_url)
                VALUES (@userId, @attemptId, @type, @imageUrl)
                RETURNING id;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", violation.UserId);
            cmd.Parameters.AddWithValue("@attemptId", violation.ExamAttemptId.HasValue ? violation.ExamAttemptId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@type", violation.Type);
            cmd.Parameters.AddWithValue("@imageUrl", string.IsNullOrEmpty(violation.ImageUrl) ? DBNull.Value : violation.ImageUrl);

            var id = await cmd.ExecuteScalarAsync();
            return id != null ? Convert.ToInt32(id) : 0;
        }

        public async Task<List<ViolationModel>> GetViolationsByAttemptIdAsync(int attemptId)
        {
            var list = new List<ViolationModel>();
            const string sql = @"
                SELECT id, user_id, exam_attempt_id, type, image_url, created_at
                FROM violations
                WHERE exam_attempt_id = @attemptId
                ORDER BY created_at DESC;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@attemptId", attemptId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ViolationModel
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    ExamAttemptId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    Type = reader.GetString(3),
                    ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }

            return list;
        }
    }
}
