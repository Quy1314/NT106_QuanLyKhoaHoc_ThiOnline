using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CourseGuard.Data
{
    public static class DatabaseAction
    {
        private static readonly string connectionString =
            "Server=localhost;Database=CourseGuardDB;Trusted_Connection=True;TrustServerCertificate=True";

        public static int ExecuteNonQuery(
            string query,
            Dictionary<string, (SqlDbType Type, object Value)> parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param.Key, param.Value.Type).Value =
                            param.Value.Value ?? DBNull.Value;
                    }
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(
            string query,
            Dictionary<string, (SqlDbType Type, object Value)> parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param.Key, param.Value.Type).Value =
                            param.Value.Value ?? DBNull.Value;
                    }
                }

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static DataTable ExecuteQuery(
            string query,
            Dictionary<string, (SqlDbType Type, object Value)> parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param.Key, param.Value.Type).Value =
                            param.Value.Value ?? DBNull.Value;
                    }
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
