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
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public partial class UC_UsersManage : UserControl
    {
        private readonly CourseGuard.Backend.Controllers.UserController _userService;

        public UC_UsersManage()
        {
            InitializeComponent();
            _userService = new CourseGuard.Backend.Controllers.UserController(new CourseGuardDbContext(""));
            ApplyDarkStyle();

            // Rounded buttons (matching Courses tab style)
            RoundedButtonHelper.Apply(10,
                btn_insert, btn_delete, btn_search, btn_Approve);

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

        private void ApplyDarkStyle()
        {
            BackColor = MetaTheme.Colors.FormBg;

            // Primary indigo buttons
            MetaTheme.StylePrimaryButton(btn_insert);
            MetaTheme.StylePrimaryButton(btn_search);

            // Ghost approve button
            MetaTheme.StyleGhostButton(btn_Approve);

            // Critical-red delete button
            btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete.FlatAppearance.BorderSize = 0;
            btn_delete.BackColor = MetaTheme.Colors.LogoutRed;
            btn_delete.ForeColor = MetaTheme.Colors.TextPrimary;
            btn_delete.Font = MetaTheme.Fonts.ButtonMd();
            btn_delete.Cursor = System.Windows.Forms.Cursors.Hand;
            btn_delete.FlatAppearance.MouseOverBackColor = MetaTheme.Colors.LogoutRedHover;

            MetaTheme.StyleGrid(dataGridView1);
        }

        private async void LoadData()
        {
            string status = cb_StatusFilter.SelectedItem?.ToString() ?? "ALL";
            string role = (cb_roleID.SelectedItem == null || cb_roleID.Text == "Select Role") ? "ALL" : cb_roleID.Text.ToUpper();

            this.ShowSkeleton(SkeletonType.FormWithTable);
            try
            {
                var users = await Task.Run(() => _userService.SearchUsers(status, role));
                dataGridView1.DataSource = users;
                
                // Hide Password Hash if present in grid?
                if (dataGridView1.Columns.Contains("PasswordHash"))
                {
                    DataGridViewColumn? passwordColumn = dataGridView1.Columns["PasswordHash"];
                    if (passwordColumn != null)
                    {
                        passwordColumn.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi tải dữ liệu: " + ex.Message);
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private async void btn_insert_Click(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txt_Username.Text) ||
                string.IsNullOrWhiteSpace(txt_Password.Text) ||
                string.IsNullOrWhiteSpace(txt_FullName.Text) ||
                string.IsNullOrWhiteSpace(txt_Email.Text) ||
                string.IsNullOrWhiteSpace(cb_roleID.Text))
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng điền đầy đủ thông tin.");
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

            this.ShowSkeleton(SkeletonType.FormWithTable);
            try
            {
                string result = await Task.Run(() => _userService.AddUser(user, password));
                if (result == "Success")
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Thêm user thành công.");
                    ClearForm();
                    LoadData();
                }
                else
                {
                    string detail = !string.IsNullOrWhiteSpace(_userService.LastErrorMessage)
                        ? _userService.LastErrorMessage
                        : result;
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Thêm thất bại: " + detail);
                }
            }
            catch (Exception ex)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi: " + ex.Message);
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private async void btn_delete_Click(object? sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                try 
                {
                    var cell = dataGridView1.SelectedRows[0].Cells["Id"]; // UserModel property is "Id"
                    if (cell != null && cell.Value != null)
                    {
                        int userId = Convert.ToInt32(cell.Value);
                        if (CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Bạn có chắc chắn muốn xóa user này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            this.ShowSkeleton(SkeletonType.FormWithTable);
                            try
                            {
                                bool success = await Task.Run(() => _userService.DeleteUser(userId));
                                if (success)
                                {
                                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Xóa thành công!");
                                    LoadData(); 
                                }
                                else
                                {
                                    string detail = string.IsNullOrWhiteSpace(_userService.LastErrorMessage)
                                        ? $"Xóa thất bại (ID: {userId})"
                                        : _userService.LastErrorMessage;
                                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(detail);
                                }
                            }
                            finally
                            {
                                this.HideSkeleton();
                            }
                        }
                    }
                    else 
                    {
                         CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Không chọn được ID.");
                    }
                }
                catch(Exception ex)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi khi xóa: " + ex.Message);
                }
            }
            else
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn user để xóa.");
            }
        }

        private void btn_search_Click(object? sender, EventArgs e)
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

        private async void btn_Approve_Click(object? sender, EventArgs e)
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

                        bool isResetRequest = currentStatus.Equals("RESET_REQUEST", StringComparison.OrdinalIgnoreCase);
                        string action = isResetRequest ? "RESET" : "APPROVE";
                        string confirmMsg = isResetRequest
                            ? "Bạn muốn gửi email cấp lại mật khẩu cho user này?"
                            : "Bạn muốn kích hoạt tài khoản này?";

                        if (CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(confirmMsg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            this.ShowSkeleton(SkeletonType.FormWithTable);
                            try
                            {
                                bool success = await Task.Run(() => _userService.ApproveUserRequest(userId, action));
                                if (success)
                                {
                                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(action == "RESET"
                                        ? "Đã gửi mật khẩu tạm thời qua email cho user."
                                        : "Thực hiện thành công!");
                                    LoadData();
                                }
                                else
                                {
                                    string detail = string.IsNullOrWhiteSpace(_userService.LastErrorMessage)
                                        ? "Kiểm tra email user hoặc cấu hình SMTP_USER/SMTP_PASS."
                                        : _userService.LastErrorMessage;
                                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog($"Thao tác thất bại.\nChi tiết: {detail}");
                                }
                            }
                            finally
                            {
                                this.HideSkeleton();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi: " + ex.Message);
                }
            }
            else
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn user cần phê duyệt.");
            }
        }
    }
}
