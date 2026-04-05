using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WebService_Demo.Services;

namespace WebService_Demo
{
    public partial class FormForgotPassword : Form
    {
        public FormForgotPassword()
        {
            InitializeComponent();
        }

        private async void btnConfirm_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string newPass = txtNewPassword.Text;
            string confirm = txtConfirmPassword.Text;

            // ── Validate cục bộ ───────────────────────────────
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Vui lòng không để trống các trường!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Email không đúng dạng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass != confirm)
            {
                MessageBox.Show("Mật khẩu mới và Xác nhận mật khẩu phải giống nhau!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ── Gọi API ───────────────────────────────────────
            btnConfirm.Enabled = false;
            btnConfirm.Text = "Đang xử lý...";

            var (success, message) = await ApiService.ForgotPasswordAsync(email, newPass, confirm);

            btnConfirm.Enabled = true;
            btnConfirm.Text = "Xác nhận";

            if (success)
            {
                MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
