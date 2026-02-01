using System;
using System.Drawing;
using System.Windows.Forms;

namespace CourseGuard
{
    public partial class LoginPage : Form
    {
        public LoginPage()
        {
            InitializeComponent();
            this.Load += LoginPage_Load;
            this.Resize += LoginPage_Resize;
        }

        private void LoginPage_Load(object? sender, EventArgs e)
        {
            ResizePanel();
            CenterPanel();
            ResizeAllControls();
        }

        private void LoginPage_Resize(object? sender, EventArgs e)
        {
            ResizePanel();
            CenterPanel();
            ResizeAllControls();
        }

        private void CenterPanel()
        {
            LoginPanel.Left = (this.ClientSize.Width - LoginPanel.Width) / 2;
            LoginPanel.Top = (this.ClientSize.Height - LoginPanel.Height) / 2;
        }

        private void ResizePanel()
        {
            int newWidth = this.ClientSize.Width / 2;

            int newHeight = (int)(this.ClientSize.Height / 1.5);
            if (newHeight > 450)
                newHeight = 450;

            if (newWidth < 400)
                newWidth = 400;

            LoginPanel.Size = new Size(newWidth, newHeight);
        }

        private void ResizeAllControls()
        {
            int padding = 25;
            int fullWidth = LoginPanel.Width - padding * 2;

            // Textbox full width
            txtUsername.Width = fullWidth;
            txtPassword.Width = fullWidth;

            txtUsername.Left = padding;
            txtPassword.Left = padding;

            lblUsername.Left = padding;
            lblPassword.Left = padding;

            // Logo bám phải
            LOGO.Left = LoginPanel.Width - LOGO.Width - padding;

            // Forgot password bám phải
            linkLabel1.Left = LoginPanel.Width - linkLabel1.Width - padding;

            // Checkbox giữ trái
            chkRemember.Left = padding;

            // Button luôn ở giữa
            btnLogin.Left = (LoginPanel.Width - btnLogin.Width) / 2;
            // Căn logo ngang với LoginTitle
            LOGO.Top = LoginTitle.Top;

        }
    }
}
