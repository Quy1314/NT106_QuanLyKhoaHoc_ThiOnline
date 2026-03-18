using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo_Firebase
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
                MessageBox.Show("Vui lòng nhập đầy đủ Email/Username và Password!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Đổi URL này thành URL REALTIME DATABASE CỦA BẠN (VD: https://my-app.firebaseio.com/)
            Firebase_Service firebase = new Firebase_Service("https://couresguard-default-rtdb.asia-southeast1.firebasedatabase.app/");
            
            btnLogin.Enabled = false;
            btnLogin.Text = "Đang xử lý...";

            try
            {
                bool isLoginSuccess = await firebase.Login(txtEmail.Text.Trim(), txtPassword.Text);

                if (isLoginSuccess)
                {
                    FormMain formMain = new FormMain();
                    this.Hide();
                    formMain.ShowDialog();
                    
                    txtPassword.Clear();
                    this.Show();
                }
                else
                {
                    MessageBox.Show("Tài khoản hoặc mật khẩu không chính xác!", "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối Firebase: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng nhập";
            }
        }

        private void BtnGoRegister_Click(object sender, EventArgs e)
        {
            FormRegister formRegister = new FormRegister();
            this.Hide();
            formRegister.ShowDialog();
            this.Show();
        }
    }
}
