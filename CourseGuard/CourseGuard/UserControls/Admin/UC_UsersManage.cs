using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using CourseGuard.Security;

namespace CourseGuard.UserControls.Admin
{
    public partial class UC_UsersManage : UserControl
    {
        private readonly string connectionString =
            "Server=localhost;Database=CourseGuardDB;Trusted_Connection=True;TrustServerCertificate=True";

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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        INSERT INTO USERS 
                        (USERNAME, PASSWORD_HASH, FULL_NAME, EMAIL, ROLE_ID, STATUS)
                        VALUES
                        (@username, @password_hash, @full_name, @email, @role_id, @status)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username;
                        cmd.Parameters.Add("@password_hash", SqlDbType.NVarChar, 255).Value = hashedPassword;
                        cmd.Parameters.Add("@full_name", SqlDbType.NVarChar, 100).Value = fullName;
                        cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = email;
                        cmd.Parameters.Add("@role_id", SqlDbType.Int).Value = roleId;
                        cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;

                        int rowsAffected = cmd.ExecuteNonQuery();

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
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL: " + ex.Message);
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
