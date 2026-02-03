namespace CourseGuard.Forms.Login
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
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            LoginPanel = new Panel();
            linkLabel1 = new LinkLabel();
            chkRemember = new CheckBox();
            LoginTitle = new Label();
            LOGO = new Label();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnLogin = new Button();
            LoginPanel.SuspendLayout();
            SuspendLayout();
            // 
            // LoginPanel
            // 
            LoginPanel.BackColor = Color.White;
            LoginPanel.Controls.Add(linkLabel1);
            LoginPanel.Controls.Add(chkRemember);
            LoginPanel.Controls.Add(LoginTitle);
            LoginPanel.Controls.Add(LOGO);
            LoginPanel.Controls.Add(lblUsername);
            LoginPanel.Controls.Add(txtUsername);
            LoginPanel.Controls.Add(lblPassword);
            LoginPanel.Controls.Add(txtPassword);
            LoginPanel.Controls.Add(btnLogin);
            LoginPanel.Location = new Point(0, 0);
            LoginPanel.Margin = new Padding(4, 3, 4, 3);
            LoginPanel.Name = "LoginPanel";
            LoginPanel.Size = new Size(500, 345);
            LoginPanel.TabIndex = 0;
            // 
            // linkLabel1
            // 
            linkLabel1.ActiveLinkColor = Color.RoyalBlue;
            linkLabel1.AutoSize = true;
            linkLabel1.BackColor = Color.White;
            linkLabel1.Font = new Font("Segoe UI", 9F);
            linkLabel1.LinkColor = Color.DodgerBlue;
            linkLabel1.Location = new Point(322, 250);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(127, 20);
            linkLabel1.TabIndex = 10;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Forgot password?";
            linkLabel1.VisitedLinkColor = Color.DodgerBlue;
            // 
            // chkRemember
            // 
            chkRemember.AutoSize = true;
            chkRemember.BackColor = Color.White;
            chkRemember.Font = new Font("Segoe UI", 9F);
            chkRemember.ForeColor = Color.DimGray;
            chkRemember.Location = new Point(25, 243);
            chkRemember.Name = "chkRemember";
            chkRemember.Size = new Size(129, 24);
            chkRemember.TabIndex = 9;
            chkRemember.Text = "Remember me";
            chkRemember.UseVisualStyleBackColor = false;
            // 
            // LoginTitle
            // 
            LoginTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            LoginTitle.ForeColor = Color.FromArgb(56, 113, 224);
            LoginTitle.Location = new Point(0, 20);
            LoginTitle.Margin = new Padding(4, 0, 4, 0);
            LoginTitle.Name = "LoginTitle";
            LoginTitle.Size = new Size(400, 50);
            LoginTitle.TabIndex = 0;
            LoginTitle.Text = "LOGIN";
            LoginTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LOGO
            // 
            LOGO.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            LOGO.ForeColor = Color.Gray;
            LOGO.Location = new Point(0, 75);
            LOGO.Margin = new Padding(4, 0, 4, 0);
            LOGO.Name = "LOGO";
            LOGO.Size = new Size(400, 30);
            LOGO.TabIndex = 1;
            LOGO.Text = "COURSE GUARD";
            LOGO.TextAlign = ContentAlignment.MiddleCenter;
            LOGO.Click += LOGO_Click;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(25, 104);
            lblUsername.Margin = new Padding(4, 0, 4, 0);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(89, 23);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Username";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(25, 132);
            txtUsername.Margin = new Padding(4, 3, 4, 3);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(449, 30);
            txtUsername.TabIndex = 3;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(25, 178);
            lblPassword.Margin = new Padding(4, 0, 4, 0);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(85, 23);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Password";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(25, 207);
            txtPassword.Margin = new Padding(4, 3, 4, 3);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(449, 30);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.DodgerBlue;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(156, 273);
            btnLogin.Margin = new Padding(4, 3, 4, 3);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(188, 46);
            btnLogin.TabIndex = 6;
            btnLogin.Text = "Log In";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // LoginPage
            // 
            AutoScaleDimensions = new SizeF(10F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 242, 245);
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1000, 518);
            Controls.Add(LoginPanel);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Margin = new Padding(4, 3, 4, 3);
            Name = "LoginPage";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login";
            Load += LoginPage_Load_1;
            LoginPanel.ResumeLayout(false);
            LoginPanel.PerformLayout();
            ResumeLayout(false);
        }

        private CheckBox chkRemember;
        private LinkLabel linkLabel1;
    }
}
