using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Demo_Firebase
{
    public partial class FormRegister : Form
    {
        // ✅ Dùng chung 1 instance FirebaseService (không tạo mới mỗi lần click)
        private static readonly FirebaseService _firebase = new FirebaseService(AppConfig.FirebaseUrl);

        public FormRegister()
        {
            InitializeComponent();
        }

        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string pass = txtPassword.Text;
            string confirm = txtConfirm.Text;

            // Validation
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Vui lòng không để trống các trường!",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Email không đúng dạng!",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (pass != confirm)
            {
                MessageBox.Show("Mật khẩu và Xác nhận mật khẩu phải giống nhau!",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnRegister.Enabled = false;
            btnRegister.Text = "Đang xử lý...";

            try
            {
                // 1. Kiểm tra tài khoản trùng lặp
                var (_, existingUser) = await _firebase.FindUser(email);
                if (existingUser != null)
                {
                    MessageBox.Show("Tên tài khoản (Email) này đã tồn tại!",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2. Tạo User mới
                bool isSuccess = await _firebase.RegisterUser(new UserModel
                {
                    Username = email,
                    Password = pass
                });

                if (isSuccess)
                {
                    MessageBox.Show("Đăng ký thành công!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Lỗi Firebase: Đăng ký thất bại!",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối Firebase: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "Đăng ký";
            }
        }

        private void BtnBackLogin_Click(object sender, EventArgs e) => this.Close();
    }
}
