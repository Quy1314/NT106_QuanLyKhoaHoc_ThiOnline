using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using CourseGuard.Security;
using CourseGuard.Data;

namespace CourseGuard.UserControls.Admin
{
    public partial class UC_UsersManage : UserControl
    {


        public UC_UsersManage()
        {
            InitializeComponent();
            // Default: Empty grid, only load on search
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            this.btn_search.Click += new System.EventHandler(this.btn_search_Click);
        }

        private void LoadData()
        {
            string username = txt_Username.Text.Trim();
            string fullname = txt_FullName.Text.Trim();

            string query = "SELECT * FROM USERS WHERE 1=1";
            var parameters = new System.Collections.Generic.Dictionary<string, (SqlDbType, object)>();

            if (!string.IsNullOrEmpty(username))
            {
                query += " AND USERNAME LIKE @username";
                parameters.Add("@username", (SqlDbType.NVarChar, "%" + username + "%"));
            }

            if (!string.IsNullOrEmpty(fullname))
            {
                query += " AND FULL_NAME LIKE @fullname";
                parameters.Add("@fullname", (SqlDbType.NVarChar, "%" + fullname + "%"));
            }

            try
            {
                DataTable dt = DatabaseAction.ExecuteQuery(query, parameters);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void btn_insert_Click(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txt_Username.Text) ||
                string.IsNullOrWhiteSpace(txt_Password.Text) ||
                string.IsNullOrWhiteSpace(txt_FullName.Text) ||
                string.IsNullOrWhiteSpace(txt_Email.Text) ||
                string.IsNullOrWhiteSpace(cb_roleID.Text))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.");
                return;
            }

            string username = txt_Username.Text.Trim();
            string password = txt_Password.Text.Trim();
            string fullName = txt_FullName.Text.Trim();
            string email = txt_Email.Text.Trim();
            int roleId = cb_roleID.Text == "Teacher" ? 2 : 3;
            string status = "ACTIVE";

            string hashedPassword = Security.PasswordHasher.HashPassword(password);

            try
            {
                string query = @"
                        INSERT INTO USERS 
                        (USERNAME, PASSWORD_HASH, FULL_NAME, EMAIL, ROLE_ID, STATUS)
                        VALUES
                        (@username, @password_hash, @full_name, @email, @role_id, @status)";

                var parameters = new System.Collections.Generic.Dictionary<string, (SqlDbType, object)>
                {
                    { "@username", (SqlDbType.NVarChar, username) },
                    { "@password_hash", (SqlDbType.NVarChar, hashedPassword) },
                    { "@full_name", (SqlDbType.NVarChar, fullName) },
                    { "@email", (SqlDbType.NVarChar, email) },
                    { "@role_id", (SqlDbType.Int, roleId) },
                    { "@status", (SqlDbType.NVarChar, status) }
                };

                int rowsAffected = CourseGuard.Data.DatabaseAction.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Thêm user thành công.");
                    ClearForm();
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Không thể thêm user.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                try 
                {
                    if (dataGridView1.Columns.Contains("ID")) // Kiem tra cột ID có tồn tại không
                    {
                        var cell = dataGridView1.SelectedRows[0].Cells["ID"]; // Chọn cột ID để lấy giá trị ID 
                        if (cell != null && cell.Value != null)
                        {
                            int userId = Convert.ToInt32(cell.Value);
                            if (MessageBox.Show("Bạn có chắc chắn muốn xóa user này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                string query = "DELETE FROM USERS WHERE ID = @id";
                                var parameters = new System.Collections.Generic.Dictionary<string, (SqlDbType, object)>
                                {
                                    { "@id", (SqlDbType.Int, userId) }
                                };
                                
                                int result = DatabaseAction.ExecuteNonQuery(query, parameters);
                                if (result > 0)
                                {
                                    MessageBox.Show("Xóa thành công!");
                                    LoadData(); // Refresh list to remove deleted user
                                }
                                else
                                {
                                    MessageBox.Show("Xóa thất bại (ID: " + userId + ")");
                                }
                            }
                        }
                    }
                    else 
                    {
                         MessageBox.Show("Không tìm thấy cột ID. Vui lòng kiểm tra lại cấu trúc bảng.");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn user để xóa.");
            }
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void ClearForm()
        {
            txt_Username.Clear();
            txt_Password.Clear();
            txt_FullName.Clear();
            txt_Email.Clear();
            cb_roleID.SelectedIndex = -1;
        }
    }
}
