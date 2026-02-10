/*
 * UserRepository.cs
 * 
 * Layer: Infrastructure
 * Vai trò: Chứa các câu lệnh SQL cụ thể cho bảng USERS (SELECT, INSERT, UPDATE, DELETE).
 * Sử dụng: Nhận yêu cầu từ Service, gọi DatabaseAction để lấy dữ liệu, rồi chuyển đổi (Map) dữ liệu từ SQL thành UserModel.
 */
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CourseGuard.Application.Interfaces;
using CourseGuard.Core.Models;
using CourseGuard.Infrastructure.Data;

namespace CourseGuard.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// Lấy tất cả người dùng.
        /// Sử dụng: Gọi hàm Search với tham số rỗng để lấy toàn bộ danh sách.
        /// </summary>
        public List<UserModel> GetAll()
        {
            return Search(string.Empty, string.Empty);
        }

        /// <summary>
        /// Tìm kiếm người dùng theo username và fullname.
        /// Sử dụng: Xây dựng câu lệnh SQL động với mệnh đề LIKE. Thực thi bằng DatabaseAction.ExecuteQuery.
        /// </summary>
        public List<UserModel> Search(string username, string fullName)
        {
            var users = new List<UserModel>();
            string query = @"
                SELECT u.*, r.NAME as ROLE_NAME 
                FROM USERS u 
                LEFT JOIN ROLES r ON u.ROLE_ID = r.ID 
                WHERE 1=1";
            var parameters = new Dictionary<string, (SqlDbType, object)>();

            if (!string.IsNullOrEmpty(username))
            {
                query += " AND USERNAME LIKE @username";
                parameters.Add("@username", (SqlDbType.NVarChar, "%" + username + "%"));
            }

            if (!string.IsNullOrEmpty(fullName))
            {
                query += " AND FULL_NAME LIKE @fullname";
                parameters.Add("@fullname", (SqlDbType.NVarChar, "%" + fullName + "%"));
            }

            DataTable dt = DatabaseAction.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
            {
                users.Add(MapToUser(row));
            }
            return users;
        }

        /// <summary>
        /// Thêm người dùng mới.
        /// Sử dụng: Thực thi lệnh INSERT INTO USERS. Dùng subquery để lấy ROLE_ID từ tên Role. Thực thi bằng DatabaseAction.ExecuteNonQuery.
        /// </summary>
        public int Add(UserModel user, string passwordHash)
        {
            string query = @"
                INSERT INTO USERS 
                (USERNAME, PASSWORD_HASH, FULL_NAME, EMAIL, ROLE_ID, STATUS, CREATED_AT)
                VALUES
                (@username, @password_hash, @full_name, @email, 
                (SELECT ID FROM ROLES WHERE NAME = @role_name), @status, GETDATE())";
            
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@username", (SqlDbType.NVarChar, user.Username) },
                { "@password_hash", (SqlDbType.NVarChar, passwordHash) },
                { "@full_name", (SqlDbType.NVarChar, user.FullName) },
                { "@email", (SqlDbType.NVarChar, user.Email) },
                { "@role_name", (SqlDbType.NVarChar, user.Role.ToUpper()) }, 
                { "@status", (SqlDbType.NVarChar, user.Status) }
            };

            return DatabaseAction.ExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// Xóa người dùng.
        /// Sử dụng: Thực thi lệnh DELETE FROM USERS. Thực thi bằng DatabaseAction.ExecuteNonQuery.
        /// </summary>
        public bool Delete(int id)
        {
            string query = "DELETE FROM USERS WHERE ID = @id";
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@id", (SqlDbType.Int, id) }
            };
            return DatabaseAction.ExecuteNonQuery(query, parameters) > 0;
        }

        /// <summary>
        /// Lấy người dùng theo ID.
        /// Sử dụng: Thực thi lệnh SELECT với điều kiện WHERE ID = @id. Thực thi bằng DatabaseAction.ExecuteQuery.
        /// </summary>
        public UserModel? GetById(int id)
        {
            string query = @"
                SELECT u.*, r.NAME as ROLE_NAME 
                FROM USERS u 
                JOIN ROLES r ON u.ROLE_ID = r.ID 
                WHERE u.ID = @id";
            
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@id", (SqlDbType.Int, id) }
            };

            DataTable dt = DatabaseAction.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                return MapToUser(dt.Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// Lấy người dùng theo Username (dùng cho Login/Check Duplicate).
        /// Sử dụng: Thực thi lệnh SELECT với điều kiện WHERE USERNAME = @username. Thực thi bằng DatabaseAction.ExecuteQuery.
        /// </summary>
        public UserModel? GetByUsername(string username)
        {
               string query = @"
                SELECT u.*, r.NAME as ROLE_NAME 
                FROM USERS u 
                JOIN ROLES r ON u.ROLE_ID = r.ID 
                WHERE u.USERNAME = @username";
            
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@username", (SqlDbType.NVarChar, username) }
            };

            DataTable dt = DatabaseAction.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                return MapToUser(dt.Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// Lấy danh sách người dùng theo Role Name.
        /// Sử dụng: Thực thi lệnh SELECT kết hợp JOIN ROLES để lọc theo NAME. Thực thi bằng DatabaseAction.ExecuteQuery.
        /// </summary>
        public List<UserModel> GetByRole(string roleName)
        {
            var users = new List<UserModel>();
            string query = @"
                SELECT u.*, r.NAME as ROLE_NAME 
                FROM USERS u 
                JOIN ROLES r ON u.ROLE_ID = r.ID 
                WHERE r.NAME = @roleName";
            
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@roleName", (SqlDbType.NVarChar, roleName) }
            };

            DataTable dt = DatabaseAction.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
            {
                users.Add(MapToUser(row));
            }
            return users;
        }

        private UserModel MapToUser(DataRow row)
        {
            string role = "Unknown";
            if (row.Table.Columns.Contains("ROLE_NAME") && row["ROLE_NAME"] != DBNull.Value)
            {
                role = row["ROLE_NAME"].ToString() ?? "Unknown";
            }
            
            return new UserModel
            {
                Id = row["ID"] != DBNull.Value ? Convert.ToInt32(row["ID"]) : 0,
                Username = row["USERNAME"]?.ToString() ?? string.Empty,
                PasswordHash = row["PASSWORD_HASH"] != DBNull.Value ? row["PASSWORD_HASH"].ToString() ?? string.Empty : string.Empty,
                FullName = row["FULL_NAME"] != DBNull.Value ? row["FULL_NAME"].ToString() ?? string.Empty : string.Empty,
                Email = row["EMAIL"] != DBNull.Value ? row["EMAIL"].ToString() ?? string.Empty : string.Empty,
                Status = row["STATUS"] != DBNull.Value ? row["STATUS"].ToString() ?? "Inactive" : "Inactive",
                Role = role
            };
        }

        /// <summary>
        /// Lấy dữ liệu cho Dashboard.
        /// Sử dụng: Thực thi lệnh SELECT phức hợp bao gồm JOIN và SUBQUERY để lấy Last Login/Last IP từ bảng DEVICES. Thực thi bằng DatabaseAction.ExecuteQuery.
        /// </summary>
        public List<UserDashboardDto> GetDashboardData()
        {
            var users = new List<UserDashboardDto>();
            string query = @"
                SELECT 
                    u.ID, 
                    u.USERNAME, 
                    u.FULL_NAME, 
                    u.EMAIL, 
                    r.NAME AS ROLE, 
                    u.STATUS,
                    (SELECT TOP 1 d.LAST_ACTIVE 
                     FROM DEVICES d 
                     WHERE d.USER_ID = u.ID 
                     ORDER BY d.LAST_ACTIVE DESC) AS LAST_LOGIN,
                     (SELECT TOP 1 d.IP_ADDRESS 
                     FROM DEVICES d 
                     WHERE d.USER_ID = u.ID 
                     ORDER BY d.LAST_ACTIVE DESC) AS LAST_IP
                FROM USERS u
                JOIN ROLES r ON u.ROLE_ID = r.ID
                ORDER BY LAST_LOGIN DESC";

            DataTable dt = DatabaseAction.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                users.Add(new UserDashboardDto
                {
                    Id = row["ID"] != DBNull.Value ? Convert.ToInt32(row["ID"]) : 0,
                    Username = row["USERNAME"]?.ToString() ?? string.Empty,
                    FullName = row["FULL_NAME"] != DBNull.Value ? row["FULL_NAME"].ToString() ?? string.Empty : string.Empty,
                    Email = row["EMAIL"] != DBNull.Value ? row["EMAIL"].ToString() ?? string.Empty : string.Empty,
                    Role = row["ROLE"] != DBNull.Value ? row["ROLE"].ToString() ?? "Unknown" : "Unknown",
                    Status = row["STATUS"] != DBNull.Value ? row["STATUS"].ToString() ?? "Inactive" : "Inactive",
                    LastLogin = row["LAST_LOGIN"] != DBNull.Value ? Convert.ToDateTime(row["LAST_LOGIN"]) : (DateTime?)null,
                    LastIp = row["LAST_IP"] != DBNull.Value ? row["LAST_IP"].ToString() ?? string.Empty : string.Empty
                });
            }
            return users;
        }

        /// <summary>
        /// Cập nhật thông tin thiết bị/IP khi đăng nhập.
        /// Sử dụng: Kiểm tra tồn tại trong bảng DEVICES, nếu có thì UPDATE, chưa có thì INSERT.
        /// </summary>
        public bool UpdateDevice(int userId, string deviceName, string ipAddress)
        {
            try 
            {
                string queryCheck = "SELECT COUNT(*) FROM DEVICES WHERE USER_ID = @uid AND DEVICE_NAME = @dname";
                var paramsCheck = new Dictionary<string, (SqlDbType, object)>
                {
                    { "@uid", (SqlDbType.Int, userId) },
                    { "@dname", (SqlDbType.NVarChar, deviceName) }
                };
                
                int count = Convert.ToInt32(DatabaseAction.ExecuteScalar(queryCheck, paramsCheck));

                if (count > 0)
                {
                    // Update
                    string queryUpdate = @"
                        UPDATE DEVICES 
                        SET IP_ADDRESS = @ip, LAST_ACTIVE = GETDATE(), STATUS = 'ACTIVE'
                        WHERE USER_ID = @uid AND DEVICE_NAME = @dname";
                    
                    var paramsUpdate = new Dictionary<string, (SqlDbType, object)>
                    {
                        { "@ip", (SqlDbType.NVarChar, ipAddress) },
                        { "@uid", (SqlDbType.Int, userId) },
                        { "@dname", (SqlDbType.NVarChar, deviceName) }
                    };
                    return DatabaseAction.ExecuteNonQuery(queryUpdate, paramsUpdate) > 0;
                }
                else
                {
                    // Insert
                    string queryInsert = @"
                        INSERT INTO DEVICES (USER_ID, DEVICE_NAME, IP_ADDRESS, STATUS, LAST_ACTIVE)
                        VALUES (@uid, @dname, @ip, 'ACTIVE', GETDATE())";
                    
                    var paramsInsert = new Dictionary<string, (SqlDbType, object)>
                    {
                        { "@uid", (SqlDbType.Int, userId) },
                        { "@dname", (SqlDbType.NVarChar, deviceName) },
                        { "@ip", (SqlDbType.NVarChar, ipAddress) }
                    };
                    return DatabaseAction.ExecuteNonQuery(queryInsert, paramsInsert) > 0;
                }
            }
            catch
            {
                return false;
            }

        }
        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            string query = "UPDATE USERS SET PASSWORD_HASH = @password_hash WHERE ID = @id";
            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@password_hash", (SqlDbType.NVarChar, newPasswordHash) },
                { "@id", (SqlDbType.Int, userId) }
            };
            return DatabaseAction.ExecuteNonQuery(query, parameters) > 0;
        }
    }
}
