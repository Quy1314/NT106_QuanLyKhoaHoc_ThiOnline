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
using Microsoft.Data.SqlClient;
using CourseGuard.Core.Models;
using CourseGuard.Core.Security;
using CourseGuard.Infrastructure.Data;
using CourseGuard.Application.Services;
using CourseGuard.Application.Interfaces;
using CourseGuard.Infrastructure.Data.Repositories;

namespace CourseGuard.Presentation.UserControls.Admin
{
    public partial class UC_UsersManage : UserControl
    {
        private readonly IUserService _userService;

        public UC_UsersManage()
        {
            InitializeComponent();
            _userService = new UserService(new UserRepository());

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

            try
            {
                var users = _userService.SearchUsers(username, fullname);
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
    }
}
