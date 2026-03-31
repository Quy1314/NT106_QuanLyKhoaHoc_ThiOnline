using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Demo_Firebase
{
    public partial class FormLogin : Form
    {
        // ✅ Dùng chung 1 instance FirebaseService (không tạo mới mỗi lần click)
        private static readonly FirebaseService _firebase = new FirebaseService(AppConfig.FirebaseUrl);

        public FormLogin()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Email/Username và Password!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Đang xử lý...";

            try
            {
                bool isLoginSuccess = await _firebase.Login(email, password);

                if (isLoginSuccess)
                {
                    txtPassword.Clear(); // ✅ Clear trước khi ẩn form
                    this.Hide();
                    new FormMain().ShowDialog();
                    this.Show();
                }
                else
                {
                    MessageBox.Show("Tài khoản hoặc mật khẩu không chính xác!",
                        "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối Firebase: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng nhập";
            }
        }

        private void BtnGoRegister_Click(object sender, EventArgs e)
        {
            this.Hide();
            new FormRegister().ShowDialog();
            this.Show();
        }
    }
}
