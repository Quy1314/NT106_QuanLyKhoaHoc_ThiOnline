/*
 * UC_UsersManage.cs
 * 
 * Layer: Presentation (UserControls)
 * Vai trò: Màn hình quản lý người dùng (CRUD). Hiển thị danh sách, thêm/xóa/sửa user.
 * Phụ thuộc: UserService.
 */
using System;
using System.Data; // Keep for rare cases, but mostly replaced
using System.Windows.Forms;
// Remove SqlClient
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Data;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public partial class UC_UsersManage : UserControl
    {
        private readonly CourseGuard.Backend.Controllers.UserController _userService;

        public UC_UsersManage()
        {
            InitializeComponent();
            _userService = new CourseGuard.Backend.Controllers.UserController(new CourseGuardDbContext(""));

            // Default: Empty grid, only load on search
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            this.btn_search.Click += new System.EventHandler(this.btn_search_Click);
            this.btn_Approve.Click += btn_Approve_Click;
            this.cb_StatusFilter.SelectedIndex = 0; // "ALL"
        }

        private void LoadData()
        {
            string status = cb_StatusFilter.SelectedItem?.ToString() ?? "ALL";
            string role = (cb_roleID.SelectedItem == null || cb_roleID.Text == "Select Role") ? "ALL" : cb_roleID.Text.ToUpper();

            try
            {
                var users = _userService.SearchUsers(status, role);
                dataGridView1.DataSource = users;
                
                // Hide Password Hash if present in grid?
                if (dataGridView1.Columns.Contains("PasswordHash"))
                {
                    dataGridView1.Columns["PasswordHash"].Visible = false;
                }
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

            var user = new UserModel
            {
                Username = txt_Username.Text.Trim(),
                FullName = txt_FullName.Text.Trim(),
                Email = txt_Email.Text.Trim(),
                Role = cb_roleID.Text, // "Teacher" or "Student"
                Status = "ACTIVE"
            };

            string password = txt_Password.Text.Trim();

            try
            {
                string result = _userService.AddUser(user, password);
                if (result == "Success")
                {
                    MessageBox.Show("Thêm user thành công.");
                    ClearForm();
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Thêm thất bại: " + result);
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
                    var cell = dataGridView1.SelectedRows[0].Cells["Id"]; // UserModel property is "Id"
                    if (cell != null && cell.Value != null)
                    {
                        int userId = Convert.ToInt32(cell.Value);
                        if (MessageBox.Show("Bạn có chắc chắn muốn xóa user này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            bool success = _userService.DeleteUser(userId);
                            if (success)
                            {
                                MessageBox.Show("Xóa thành công!");
                                LoadData(); 
                            }
                            else
                            {
                                MessageBox.Show("Xóa thất bại (ID: " + userId + ")");
                            }
                        }
                    }
                    else 
                    {
                         MessageBox.Show("Không chọn được ID.");
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

        private void btn_Approve_Click(object? sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                try
                {
                    var userIdCell = dataGridView1.SelectedRows[0].Cells["Id"];
                    var statusCell = dataGridView1.SelectedRows[0].Cells["Status"];

                    if (userIdCell != null && userIdCell.Value != null)
                    {
                        int userId = Convert.ToInt32(userIdCell.Value);
                        string currentStatus = statusCell?.Value?.ToString() ?? "";

                        string action = currentStatus == "RESET_PASSWORD" ? "RESET" : "APPROVE";
                        string confirmMsg = currentStatus == "RESET_PASSWORD" ? 
                            "Bạn muốn đặt lại mật khẩu mặc định cho user này?" : 
                            "Bạn muốn kích hoạt tài khoản này?";

                        if (MessageBox.Show(confirmMsg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            bool success = _userService.ApproveUserRequest(userId, action);
                            if (success)
                            {
                                MessageBox.Show("Thực hiện thành công!");
                                LoadData();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn user cần phê duyệt.");
            }
        }
    }
}
