using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

class Program {
    static async Task Main() {
        // Hash the string to verify
        using (SHA256 sha256 = SHA256.Create()) {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("admin123"));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes) {
                builder.Append(b.ToString("x2"));
            }
            Console.WriteLine($"Hash of admin123: {builder.ToString()}");
        }

        string connStr = "Host=aws-1-ap-northeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.crtiwzjkcmpvyoqgdowv;Password=testdatabseuit;SSL Mode=Require;Trust Server Certificate=true;Timeout=15;Pooling=false;";
        
        try {
            using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            
            // Generate standard password hash
            string hash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9";
            
            // Get STUDENT role ID
            int roleId = 3; // From SupabaseSchema.sql

            string query = @"INSERT INTO USERS (username, password_hash, full_name, email, role_id, status) 
                            VALUES (@username, @password_hash, @full_name, @email, @role_id, 'ACTIVE')
                            ON CONFLICT (username) DO UPDATE SET password_hash = @password_hash, role_id = @role_id, status = 'ACTIVE'";
            
            using var command = new NpgsqlCommand(query, conn);
            command.Parameters.AddWithValue("@username", "student");
            command.Parameters.AddWithValue("@password_hash", hash);
            command.Parameters.AddWithValue("@full_name", "Test Student");
            command.Parameters.AddWithValue("@email", "student@courseguard.local");
            command.Parameters.AddWithValue("@role_id", roleId);
            
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Student user securely created in the database!");
        } catch (Exception ex) {
            Console.WriteLine($"Database Error: {ex.Message}");
        }
    }
}
