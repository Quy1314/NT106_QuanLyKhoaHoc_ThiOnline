using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WebService_Demo.Services;

namespace WebService_Demo
{
    public partial class FormRegister : Form
    {
        public FormRegister()
        {
            InitializeComponent();
        }

        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string pass = txtPassword.Text;
            string confirm = txtConfirm.Text;

            // ── Validate cục bộ ──────────────────────────────
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Vui lòng không để trống các trường!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Email không đúng dạng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (pass != confirm)
            {
                MessageBox.Show("Mật khẩu và Xác nhận mật khẩu phải giống nhau!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ── Gọi API ──────────────────────────────────────
            btnRegister.Enabled = false;
            btnRegister.Text = "Đang đăng ký...";

            var (success, message) = await ApiService.RegisterAsync(email, pass, confirm);

            btnRegister.Enabled = true;
            btnRegister.Text = "Đăng ký";

            if (success)
            {
                MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "Lỗi đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBackLogin_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}


