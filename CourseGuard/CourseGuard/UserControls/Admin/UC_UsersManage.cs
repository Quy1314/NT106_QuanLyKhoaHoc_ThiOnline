using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using CourseGuard.Security;

namespace CourseGuard.UserControls.Admin
{
    public partial class UC_UsersManage : UserControl
    {


        public UC_UsersManage()
        {
            InitializeComponent();
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
