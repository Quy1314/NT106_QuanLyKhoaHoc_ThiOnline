namespace CourseGuard
{
    partial class LoginPage
    {
        private System.ComponentModel.IContainer components = null;

        private Panel LoginPanel;
        private Label LoginTitle;
        private Label LOGO;
        private Label lblUsername;
        private Label lblPassword;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            LoginPanel = new Panel();
            LoginTitle = new Label();
            LOGO = new Label();
            lblUsername = new Label();
            lblPassword = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();

            LoginPanel.SuspendLayout();
            SuspendLayout();

            // Panel
            LoginPanel.BackColor = Color.White;
            LoginPanel.Size = new Size(400, 300);

            // Title
            LoginTitle.AutoSize = true;
            LoginTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            LoginTitle.Location = new Point(20, 15);
            LoginTitle.Text = "LOGIN";
            // LOGO
            LOGO.AutoSize = false;
            LOGO.Size = new Size(150, 35);
            LOGO.Location = new Point(220, 20);
            LOGO.TextAlign = ContentAlignment.MiddleCenter;
            LOGO.Text = "COURSE GUARD";
            LOGO.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            LOGO.ForeColor = Color.White;
            LOGO.BackColor = Color.DodgerBlue;
            LOGO.BorderStyle = BorderStyle.FixedSingle;


            // Username
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(20, 90);
            lblUsername.Text = "Username";

            txtUsername.Location = new Point(20, 115);
            txtUsername.Size = new Size(360, 27);

            // Password
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(20, 155);
            lblPassword.Text = "Password";

            txtPassword.Location = new Point(20, 180);
            txtPassword.Size = new Size(360, 27);
            txtPassword.UseSystemPasswordChar = true;

            // Button
            btnLogin.BackColor = Color.DodgerBlue;
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Size = new Size(150, 40);
            btnLogin.Location = new Point(125, 230);
            btnLogin.Text = "Log In";

            // Add controls
            LoginPanel.Controls.Add(LoginTitle);
            LoginPanel.Controls.Add(LOGO);
            LoginPanel.Controls.Add(lblUsername);
            LoginPanel.Controls.Add(txtUsername);
            LoginPanel.Controls.Add(lblPassword);
            LoginPanel.Controls.Add(txtPassword);
            LoginPanel.Controls.Add(btnLogin);

            // Form
            BackColor = Color.FromArgb(240, 242, 245);
            ClientSize = new Size(800, 450);
            Controls.Add(LoginPanel);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;

            LoginPanel.ResumeLayout(false);
            LoginPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
