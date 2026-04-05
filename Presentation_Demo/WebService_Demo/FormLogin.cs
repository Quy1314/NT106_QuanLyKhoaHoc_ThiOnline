using System;
using System.Windows.Forms;
using WebService_Demo.Services;

namespace WebService_Demo
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Email/Username và Password!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Disable nút khi đang gọi API
            btnLogin.Enabled = false;
            btnLogin.Text = "Đang đăng nhập...";

            var (success, token, message) = await ApiService.LoginAsync(
                txtEmail.Text.Trim(), txtPassword.Text);

            btnLogin.Enabled = true;
            btnLogin.Text = "Đăng nhập";

            if (!success)
            {
                MessageBox.Show(message, "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Đăng nhập thành công → token đã được lưu tự động trong ApiService.CurrentToken
            // Mở FormMain
            FormMain formMain = new FormMain
            {
                Username = txtEmail.Text.Trim()
            };

            txtPassword.Clear();
            this.Hide();
            formMain.ShowDialog();
            this.Show();
        }

        private void BtnGoRegister_Click(object sender, EventArgs e)
        {
            FormRegister formRegister = new FormRegister();
            this.Hide();
            formRegister.ShowDialog();
            this.Show();
        }

        private void BtnForgotPassword_Click(object sender, EventArgs e)
        {
            FormForgotPassword formForgot = new FormForgotPassword();
            formForgot.ShowDialog();
        }
    }
}

