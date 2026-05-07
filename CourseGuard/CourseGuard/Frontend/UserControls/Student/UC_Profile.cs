using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Profile : UserControl
    {
        private readonly AuthController _authController = new(new CourseGuardDbContext(""));

        public UC_Profile()
        {
            InitializeComponent();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnSave, 10);

            btnSave.Click += (s, e) => {
                if (!UserSessionContext.CurrentUserId.HasValue || UserSessionContext.CurrentUserId.Value <= 0)
                {
                    MessageBox.Show("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string oldPassword = txtOldPassword.Text.Trim();
                string newPassword = txtNewPassword.Text.Trim();
                bool changed = _authController.ChangePassword(UserSessionContext.CurrentUserId.Value, oldPassword, newPassword);
                if (!changed)
                {
                    string detail = string.IsNullOrWhiteSpace(_authController.LastErrorMessage)
                        ? "Đổi mật khẩu thất bại."
                        : _authController.LastErrorMessage;
                    MessageBox.Show(detail, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtNewPassword.Clear();
                txtOldPassword.Clear();
            };

            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnSave.PerformClick();
                }
            };

            txtFullName.KeyDown += enterHandler;
            txtEmail.KeyDown += enterHandler;
            txtOldPassword.KeyDown += enterHandler;
            txtNewPassword.KeyDown += enterHandler;
        }
    }
}
