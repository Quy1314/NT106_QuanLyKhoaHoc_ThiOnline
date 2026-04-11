/*
 * LoginPage.cs
 * 
 * Layer: Presentation (Forms)
 * Vai trò: Form đăng nhập. Nhận User/Pass, gọi Service để kiểm tra, nếu đúng thì chuyển sang Dashboard.
 * Phụ thuộc: AuthService.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Models;

namespace CourseGuard.Frontend.Forms.Login
{
    public partial class LoginPage : Form
    {
        public LoginPage()
        {
            InitializeComponent();
            CustomizeUI(); // Apply modern UI styles
            this.Load += LoginPage_Load;
            this.Resize += LoginPage_Resize;
            AttachEvents();
        }

        private void AttachEvents()
        {
            lnkRegister.LinkClicked += (s, e) => ShowPanel(RegisterPanel);
            linkLabel1.LinkClicked += (s, e) => ShowPanel(ForgotPassPanel);
            lnkBackToLoginFromReg.LinkClicked += (s, e) => ShowPanel(LoginPanel);
            lnkBackToLoginFromForgot.LinkClicked += (s, e) => ShowPanel(LoginPanel);

            btnRegisterSubmit.Click += btnRegisterSubmit_Click;
            btnForgotSubmit.Click += btnForgotSubmit_Click;
        }

        private void ShowPanel(Panel panelToShow)
        {
            LoginPanel.Visible = (panelToShow == LoginPanel);
            RegisterPanel.Visible = (panelToShow == RegisterPanel);
            ForgotPassPanel.Visible = (panelToShow == ForgotPassPanel);

            CenterPanel();
        }

        private void CustomizeUI()
        {
            // Form Background
            this.BackColor = Color.FromArgb(242, 244, 248); // Light Gray
            this.FormBorderStyle = FormBorderStyle.Sizable; // User requested Sizable
            this.AcceptButton = btnLogin;

            // Panel Style
            LoginPanel.BackColor = Color.White;
            LoginPanel.BorderStyle = BorderStyle.None;
            // LoginPanel size is managed by ResizePanel, but we can set defaults

            // Title Style
            LoginTitle.Text = "LOGIN";
            LoginTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            LoginTitle.ForeColor = Color.FromArgb(56, 113, 224); // Royal Blue
            LoginTitle.AutoSize = false;
            LoginTitle.Size = new Size(LoginPanel.Width, 50);
            LoginTitle.TextAlign = ContentAlignment.MiddleCenter;
            LoginTitle.Location = new Point(0, 20);

            // LOGO Style
            LOGO.Text = "COURSE GUARD";
            LOGO.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            LOGO.ForeColor = Color.Gray;
            LOGO.BorderStyle = BorderStyle.None;
            LOGO.TextAlign = ContentAlignment.MiddleCenter;
            LOGO.AutoSize = false;
            LOGO.Size = new Size(LoginPanel.Width, 30);
            LOGO.Location = new Point(0, 75);

            // Labels
            lblUsername.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(64, 64, 64);

            lblPassword.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(64, 64, 64);

            // TextBoxes
            txtUsername.Font = new Font("Segoe UI", 11F);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;

            txtPassword.Font = new Font("Segoe UI", 11F);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;

            // Button
            btnLogin.Text = "Log In";
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.BackColor = Color.FromArgb(56, 113, 224); // Royal Blue
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Size = new Size(320, 45); // Bigger button

            // Links/Other
            linkLabel1.LinkColor = Color.FromArgb(56, 113, 224);
            chkRemember.Font = new Font("Segoe UI", 9F);
            lnkRegister.LinkColor = Color.FromArgb(56, 113, 224);

            // Register Controls
            lblRegUsername.Text = "Username";
            lblRegFullName.Text = "Full Name";
            lblRegEmail.Text = "Email";
            lblRegPassword.Text = "Password";
            txtRegPassword.UseSystemPasswordChar = true;
            lnkBackToLoginFromReg.Text = "Back to Login";

            // Forgot Controls
            lblForgotUsername.Text = "Username";
            lblForgotEmail.Text = "Email";
            lnkBackToLoginFromForgot.Text = "Back to Login";
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

            RegisterPanel.Left = (this.ClientSize.Width - RegisterPanel.Width) / 2;
            RegisterPanel.Top = (this.ClientSize.Height - RegisterPanel.Height) / 2;

            ForgotPassPanel.Left = (this.ClientSize.Width - ForgotPassPanel.Width) / 2;
            ForgotPassPanel.Top = (this.ClientSize.Height - ForgotPassPanel.Height) / 2;
        }

        private void ResizePanel()
        {
            int newWidth = this.ClientSize.Width / 2;

            // Ensure height captures all content (Title + Logo + Inputs + Checkbox + Button + Pasdding)
            // Content requires at least ~380px.
            int newHeight = Math.Max(400, (int)(this.ClientSize.Height / 1.5));

            if (newHeight > 550)
                newHeight = 550;

            if (newWidth < 400)
                newWidth = 400;

            LoginPanel.Size = new Size(newWidth, newHeight);

            // Adjust elements that depend on Panel Width (Title, Logo) if they are not anchored
            LoginTitle.Width = LoginPanel.Width;
            LOGO.Width = LoginPanel.Width;
        }

        private void ResizeAllControls()
        {
            int padding = 40; // Increased padding
            int fullWidth = LoginPanel.Width - padding * 2;

            // Center Title and Logo
            LoginTitle.Width = LoginPanel.Width;
            LOGO.Width = LoginPanel.Width;

            // Textbox full width
            txtUsername.Width = fullWidth;
            txtPassword.Width = fullWidth;

            txtUsername.Left = padding;
            txtPassword.Left = padding;

            lblUsername.Left = padding;
            lblPassword.Left = padding;

            // Reposition Y coords to look good
            // Assuming fixed Y positions relative to top for simplicity, or dynamic?
            // Let's stick to the current relative logic or set hard defaults if easier.
            // But CustomizeUI set initial Y. Let's ensure they are consistent.

            // Adjust Y positions for spacing
            lblUsername.Top = 130;
            txtUsername.Top = 155;

            lblPassword.Top = 205;
            txtPassword.Top = 230;

            // Button
            btnLogin.Width = fullWidth;
            btnLogin.Left = padding;
            btnLogin.Top = 290;

            // Hide old labels/controls logic removed since controls are deleted.

            // Re-arrange Checkbox and Forgot Password if they exist
            // My new design didn't account for them explicitly in the Plan but user has them.
            // Let's place them below Password.
            chkRemember.Top = 265;
            linkLabel1.Top = 265;

            chkRemember.Left = padding;
            linkLabel1.Left = LoginPanel.Width - linkLabel1.Width - padding;

            // Shift Button down
            btnLogin.Top = 310;

            // --- Register Panel Controls ---
            int regPadding = 40;
            int regFullWidth = RegisterPanel.Width - regPadding * 2;
            
            lblRegUsername.Location = new Point(regPadding, 80);
            txtRegUsername.Location = new Point(regPadding, 105);
            txtRegUsername.Width = regFullWidth;

            lblRegFullName.Location = new Point(regPadding, 145);
            txtRegFullName.Location = new Point(regPadding, 170);
            txtRegFullName.Width = regFullWidth;

            lblRegEmail.Location = new Point(regPadding, 210);
            txtRegEmail.Location = new Point(regPadding, 235);
            txtRegEmail.Width = regFullWidth;

            lblRegPassword.Location = new Point(regPadding, 275);
            txtRegPassword.Location = new Point(regPadding, 300);
            txtRegPassword.Width = regFullWidth;

            btnRegisterSubmit.Location = new Point(regPadding, 360);
            btnRegisterSubmit.Width = regFullWidth;

            lnkBackToLoginFromReg.Location = new Point((RegisterPanel.Width - lnkBackToLoginFromReg.Width) / 2, 420);

            // --- Forgot Panel Controls ---
            int forPadding = 40;
            int forFullWidth = ForgotPassPanel.Width - forPadding * 2;

            lblForgotUsername.Location = new Point(forPadding, 100);
            txtForgotUsername.Location = new Point(forPadding, 125);
            txtForgotUsername.Width = forFullWidth;

            lblForgotEmail.Location = new Point(forPadding, 175);
            txtForgotEmail.Location = new Point(forPadding, 200);
            txtForgotEmail.Width = forFullWidth;

            btnForgotSubmit.Location = new Point(forPadding, 260);
            btnForgotSubmit.Width = forFullWidth;

            lnkBackToLoginFromForgot.Location = new Point((ForgotPassPanel.Width - lnkBackToLoginFromForgot.Width) / 2, 320);
        }

        private void btnRegisterSubmit_Click(object? sender, EventArgs e)
        {
            string user = txtRegUsername.Text.Trim();
            string name = txtRegFullName.Text.Trim();
            string email = txtRegEmail.Text.Trim();
            string pass = txtRegPassword.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin đăng ký!");
                return;
            }

            var authService = new CourseGuard.Backend.Controllers.AuthController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
            
            var newUser = new UserModel
            {
                Username = user,
                FullName = name,
                Email = email
            };
            
            bool success = authService.RegisterRequest(newUser, pass);

            if (success)
            {
                MessageBox.Show("Yêu cầu đăng ký đã được gửi tới Admin chờ phê duyệt", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearRegisterInputs();
                ShowPanel(LoginPanel);
            }
            else
            {
                MessageBox.Show("Đăng ký thất bại. Tên đăng nhập có thể đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearRegisterInputs()
        {
            txtRegUsername.Clear();
            txtRegFullName.Clear();
            txtRegEmail.Clear();
            txtRegPassword.Clear();
        }

        private void btnForgotSubmit_Click(object? sender, EventArgs e)
        {
            string user = txtForgotUsername.Text.Trim();
            string email = txtForgotEmail.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nhập Username và Email!");
                return;
            }

            var authService = new CourseGuard.Backend.Controllers.AuthController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
            bool success = authService.ForgotPasswordRequest(user, email);

            if (success)
            {
                MessageBox.Show("Yêu cầu cấp lại mật khẩu đã được báo cáo lên Admin", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtForgotUsername.Clear();
                txtForgotEmail.Clear();
                ShowPanel(LoginPanel);
            }
            else
            {
                MessageBox.Show("Thông tin không chính xác hoặc không tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            try
            {
                CourseGuard.Backend.Controllers.AuthController authService = new CourseGuard.Backend.Controllers.AuthController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
                UserModel? user = authService.Login(username, password);

                if (user == null)
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!string.Equals(user.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Tài khoản của bạn đang ở trạng thái '{user.Status}'. Vui lòng liên hệ quản trị viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Login Success
                CurrentUser = user;


                // Update Device Info (IP)
                string ipAddress = GetLocalIPAddress();
                string deviceName = user.Username; // Using Username as DeviceName as per previous logic request
                authService.UpdateLoginInfo(user.Id, deviceName, ipAddress);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối bộ máy chủ Database!\n\nChi tiết kỹ thuật:\n" + ex.ToString(), "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                // Thay thế Dns.GetHostEntry (dễ bị treo) bằng cách lấy từ NetworkInterface
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                    {
                        foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(ip.Address))
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        private void LOGO_Click(object sender, EventArgs e)
        {

        }

        private void LoginPage_Load_1(object sender, EventArgs e)
        {

        }

        public UserModel? CurrentUser { get; private set; }
    }
}
