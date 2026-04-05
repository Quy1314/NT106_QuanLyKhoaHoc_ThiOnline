using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WebService_Demo.Services;

namespace WebService_Demo
{
    public partial class FormMain : Form
    {
        // ApiService.CurrentToken chứa JWT token sau khi đăng nhập
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? Username { get; set; }

        public FormMain()
        {
            InitializeComponent();
        }

        private void CenterControls()
        {
            if (lblWelcome != null && btnLogout != null)
            {
                lblWelcome.Location = new Point((this.ClientSize.Width - lblWelcome.Width) / 2, 70);
                btnLogout.Location = new Point((this.ClientSize.Width - btnLogout.Width) / 2, this.ClientSize.Height - 80);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterControls();
        }

        private async void BtnLogout_Click(object sender, EventArgs e)
        {
            btnLogout.Enabled = false;
            btnLogout.Text = "Đang đăng xuất...";

            await ApiService.LogoutAsync();

            this.Close();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Hiển thị tên user và token (rút gọn) nếu có
            if (!string.IsNullOrWhiteSpace(Username))
            {
                string tokenInfo = ApiService.CurrentToken != null
                    ? $"\nToken: ...{ApiService.CurrentToken[^20..]}" // 20 ký tự cuối
                    : "";
                lblWelcome.Text = $"Xin chào, {Username}!{tokenInfo}";
            }

            CenterControls();
        }
    }
}
