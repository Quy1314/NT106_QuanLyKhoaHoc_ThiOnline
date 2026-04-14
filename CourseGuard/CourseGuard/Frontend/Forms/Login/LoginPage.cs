/*
 * LoginPage.cs
 * 
 * Layer: Presentation (Forms)
 * Vai trò: Form đăng nhập. Nhận User/Pass, gọi Service để kiểm tra, nếu đúng thì chuyển sang Dashboard.
 * Phụ thuộc: AuthService.
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;

namespace CourseGuard.Frontend.Forms.Login
{
    public partial class LoginPage : Form
    {
        private static readonly Lazy<AuthController> SharedAuthController =
            new(() => new AuthController(new CourseGuardDbContext("")));

        private Panel? _leftShellPanel;
        private Panel? _rightVisualPanel;
        private Label? _brandLabel;
        private Label? _welcomeLabel;
        private Label? _subtitleLabel;

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

        private static AuthController AuthService => SharedAuthController.Value;

        private void ShowPanel(Panel panelToShow)
        {
            LoginPanel.Visible = (panelToShow == LoginPanel);
            RegisterPanel.Visible = (panelToShow == RegisterPanel);
            ForgotPassPanel.Visible = (panelToShow == ForgotPassPanel);

            CenterPanel();
        }

        private void CustomizeUI()
        {
            // Form Background / Shell
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.FormBorderStyle = FormBorderStyle.Sizable; // User requested Sizable
            this.AcceptButton = btnLogin;
            EnsureModernShell();

            // Panel Style
            LoginPanel.BackColor = Color.White;
            LoginPanel.BorderStyle = BorderStyle.None;
            RegisterPanel.BackColor = Color.White;
            RegisterPanel.BorderStyle = BorderStyle.None;
            ForgotPassPanel.BackColor = Color.White;
            ForgotPassPanel.BorderStyle = BorderStyle.None;

            // Title Style
            LoginTitle.Text = "Sign In";
            LoginTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            LoginTitle.ForeColor = Color.FromArgb(15, 23, 42);
            LoginTitle.AutoSize = false;
            LoginTitle.Size = new Size(LoginPanel.Width, 50);
            LoginTitle.TextAlign = ContentAlignment.MiddleCenter;
            LoginTitle.Location = new Point(0, 100);

            // LOGO Style
            LOGO.Text = "COURSEGUARD";
            LOGO.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            LOGO.ForeColor = Color.FromArgb(71, 85, 105);
            LOGO.BorderStyle = BorderStyle.None;
            LOGO.TextAlign = ContentAlignment.MiddleCenter;
            LOGO.AutoSize = false;
            LOGO.Size = new Size(LoginPanel.Width, 30);
            LOGO.Location = new Point(0, 72);

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
            btnLogin.Text = "Sign In";
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.BackColor = Color.FromArgb(37, 99, 235); // Royal Blue
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Size = new Size(320, 45); // Bigger button

            // Links/Other
            linkLabel1.LinkColor = Color.FromArgb(37, 99, 235);
            chkRemember.Font = new Font("Segoe UI", 9F);
            lnkRegister.LinkColor = Color.FromArgb(37, 99, 235);
            lnkRegister.Text = "Don't have an account? Sign Up";

            // Register Controls
            RegisterTitle.Text = "Create Account";
            RegisterTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblRegUsername.Text = "Username";
            lblRegFullName.Text = "Full Name";
            lblRegEmail.Text = "Email";
            lblRegPassword.Text = "Password";
            txtRegPassword.UseSystemPasswordChar = true;
            lnkBackToLoginFromReg.Text = "Back to Login";
            lnkBackToLoginFromReg.LinkColor = Color.FromArgb(37, 99, 235);
            btnRegisterSubmit.BackColor = Color.FromArgb(37, 99, 235);
            btnRegisterSubmit.FlatStyle = FlatStyle.Flat;
            btnRegisterSubmit.FlatAppearance.BorderSize = 0;
            btnRegisterSubmit.ForeColor = Color.White;

            // Forgot Controls
            ForgotTitle.Text = "Forgot Password";
            ForgotTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblForgotUsername.Text = "Username";
            lblForgotEmail.Text = "Email";
            lnkBackToLoginFromForgot.Text = "Back to Login";
            lnkBackToLoginFromForgot.LinkColor = Color.FromArgb(37, 99, 235);
            btnForgotSubmit.BackColor = Color.FromArgb(37, 99, 235);
            btnForgotSubmit.FlatStyle = FlatStyle.Flat;
            btnForgotSubmit.FlatAppearance.BorderSize = 0;
            btnForgotSubmit.ForeColor = Color.White;
        }

        private void EnsureModernShell()
        {
            _leftShellPanel ??= new Panel();
            _rightVisualPanel ??= new Panel();
            _brandLabel ??= new Label();
            _welcomeLabel ??= new Label();
            _subtitleLabel ??= new Label();

            if (!Controls.Contains(_leftShellPanel))
            {
                Controls.Add(_leftShellPanel);
            }
            if (!Controls.Contains(_rightVisualPanel))
            {
                Controls.Add(_rightVisualPanel);
            }

            _leftShellPanel.BackColor = Color.White;
            _leftShellPanel.BringToFront();

            _rightVisualPanel.BackColor = Color.FromArgb(15, 23, 42);
            _rightVisualPanel.Paint -= RightVisualPanel_Paint;
            _rightVisualPanel.Paint += RightVisualPanel_Paint;
            _rightVisualPanel.BringToFront();

            _brandLabel.Text = "CourseGuard";
            _brandLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            _brandLabel.ForeColor = Color.White;
            _brandLabel.AutoSize = true;

            _welcomeLabel.Text = "Welcome to CourseGuard";
            _welcomeLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            _welcomeLabel.ForeColor = Color.White;
            _welcomeLabel.AutoSize = true;

            _subtitleLabel.Text = "Sign in to access your enterprise dashboard";
            _subtitleLabel.Font = new Font("Segoe UI", 10F);
            _subtitleLabel.ForeColor = Color.FromArgb(148, 163, 184);
            _subtitleLabel.AutoSize = true;

            if (!_rightVisualPanel.Controls.Contains(_brandLabel)) _rightVisualPanel.Controls.Add(_brandLabel);
            if (!_rightVisualPanel.Controls.Contains(_welcomeLabel)) _rightVisualPanel.Controls.Add(_welcomeLabel);
            if (!_rightVisualPanel.Controls.Contains(_subtitleLabel)) _rightVisualPanel.Controls.Add(_subtitleLabel);

            LoginPanel.Parent = _leftShellPanel;
            RegisterPanel.Parent = _leftShellPanel;
            ForgotPassPanel.Parent = _leftShellPanel;
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
            if (_leftShellPanel == null || _rightVisualPanel == null) return;

            int leftWidth = Math.Max(420, (int)(ClientSize.Width * 0.35));
            if (leftWidth > 540) leftWidth = 540;

            _leftShellPanel.Bounds = new Rectangle(0, 0, leftWidth, ClientSize.Height);
            _rightVisualPanel.Bounds = new Rectangle(leftWidth, 0, ClientSize.Width - leftWidth, ClientSize.Height);

            LoginPanel.Left = (_leftShellPanel.Width - LoginPanel.Width) / 2;
            LoginPanel.Top = (_leftShellPanel.Height - LoginPanel.Height) / 2;

            RegisterPanel.Left = (_leftShellPanel.Width - RegisterPanel.Width) / 2;
            RegisterPanel.Top = (_leftShellPanel.Height - RegisterPanel.Height) / 2;

            ForgotPassPanel.Left = (_leftShellPanel.Width - ForgotPassPanel.Width) / 2;
            ForgotPassPanel.Top = (_leftShellPanel.Height - ForgotPassPanel.Height) / 2;

            _brandLabel!.Location = new Point(36, 36);
            _welcomeLabel!.Location = new Point(36, 88);
            _subtitleLabel!.Location = new Point(36, 138);
            _rightVisualPanel.Invalidate();
        }

        private void ResizePanel()
        {
            int containerWidth = _leftShellPanel?.Width ?? this.ClientSize.Width;
            int newWidth = containerWidth - 40;
            if (newWidth > 500) newWidth = 500;

            // Ensure height captures all content (Title + Logo + Inputs + Checkbox + Button + Pasdding)
            // Content requires at least ~380px.
            int newHeight = Math.Max(400, (int)(this.ClientSize.Height / 1.5));

            if (newHeight > 550)
                newHeight = 550;

            if (newWidth < 380) newWidth = 380;

            LoginPanel.Size = new Size(newWidth, newHeight);

            // Adjust elements that depend on Panel Width (Title, Logo) if they are not anchored
            LoginTitle.Width = LoginPanel.Width;
            LOGO.Width = LoginPanel.Width;
        }

        private void ResizeAllControls()
        {
            int padding = 30; // Increased padding
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
            lblUsername.Top = 150;
            txtUsername.Top = 176;

            lblPassword.Top = 224;
            txtPassword.Top = 250;

            // Button
            btnLogin.Width = fullWidth;
            btnLogin.Left = padding;
            btnLogin.Top = 290;

            // Hide old labels/controls logic removed since controls are deleted.

            // Re-arrange Checkbox and Forgot Password if they exist
            // My new design didn't account for them explicitly in the Plan but user has them.
            // Let's place them below Password.
            chkRemember.Top = 288;
            linkLabel1.Top = 288;

            chkRemember.Left = padding;
            linkLabel1.Left = LoginPanel.Width - linkLabel1.Width - padding;

            // Shift Button down
            btnLogin.Top = 328;

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

        private void RightVisualPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(90, 34, 211, 238), 1.2f);
            using var centerPen = new Pen(Color.FromArgb(140, 34, 211, 238), 1.8f);
            using var nodeBrush = new SolidBrush(Color.FromArgb(170, 34, 211, 238));

            int cx = panel.Width / 2;
            int cy = panel.Height / 2 + 20;
            int size = Math.Min(panel.Width, panel.Height) / 3;

            Point[] outer =
            {
                new(cx, cy - size),
                new(cx + size / 2, cy - size / 2),
                new(cx + size / 2, cy + size / 2),
                new(cx, cy + size),
                new(cx - size / 2, cy + size / 2),
                new(cx - size / 2, cy - size / 2),
            };

            Point[] inner =
            {
                new(cx, cy - size + 40),
                new(cx + size / 3, cy - size / 3),
                new(cx + size / 3, cy + size / 3),
                new(cx, cy + size - 40),
                new(cx - size / 3, cy + size / 3),
                new(cx - size / 3, cy - size / 3),
            };

            e.Graphics.DrawPolygon(centerPen, outer);
            e.Graphics.DrawPolygon(pen, inner);

            for (int x = 0; x < panel.Width; x += 70)
            {
                e.Graphics.DrawLine(pen, x, 0, x, panel.Height);
            }
            for (int y = 0; y < panel.Height; y += 70)
            {
                e.Graphics.DrawLine(pen, 0, y, panel.Width, y);
            }

            foreach (var p in outer)
            {
                e.Graphics.FillEllipse(nodeBrush, p.X - 3, p.Y - 3, 6, 6);
            }
            e.Graphics.FillEllipse(nodeBrush, cx - 4, cy - 4, 8, 8);
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

            var newUser = new UserModel
            {
                Username = user,
                FullName = name,
                Email = email
            };
            
            bool success = AuthService.RegisterRequest(newUser, pass);

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

            bool success = AuthService.ForgotPasswordRequest(user, email);

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

        private async void btnLogin_Click(object sender, EventArgs e)
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
                SetLoginUiBusy(true);
                Stopwatch stopwatch = Stopwatch.StartNew();
                LogLoginTiming("start login", stopwatch);

                UserModel? user = await AuthService.LoginAsync(username, password);
                LogLoginTiming("auth response", stopwatch);

                if (user == null)
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Task<string> ipAddressTask = GetLocalIPAddressAsync();
                await ipAddressTask;
                LogLoginTiming("load user data", stopwatch);

                UserModel finalUser = user;
                string ipAddress = await ipAddressTask;

                if (!string.Equals(finalUser.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Tài khoản của bạn đang ở trạng thái '{finalUser.Status}'. Vui lòng liên hệ quản trị viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string normalizedRole = (finalUser.Role ?? string.Empty).Trim().ToUpperInvariant();
                if (normalizedRole != "ADMIN" && normalizedRole != "TEACHER" && normalizedRole != "STUDENT")
                {
                    MessageBox.Show($"Quyền tài khoản không hợp lệ: {finalUser.Role}. Vui lòng liên hệ quản trị viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Login Success
                finalUser.Role = normalizedRole;
                CurrentUser = finalUser;

                // Non-blocking post-login tasks
                _ = RunPostLoginTasksAsync(finalUser, ipAddress);
                LogLoginTiming("load ui", stopwatch);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối bộ máy chủ Database!\n\nChi tiết kỹ thuật:\n" + ex.ToString(), "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                SetLoginUiBusy(false);
            }
        }

        private Task<string> GetLocalIPAddressAsync()
        {
            return Task.Run(() =>
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
            });
        }

        private async Task RunPostLoginTasksAsync(UserModel user, string ipAddress)
        {
            try
            {
                Task updateDeviceTask = AuthService.UpdateLoginInfoAsync(user.Id, user.Username, ipAddress);
                Task logLoginTask = AuthService.LogUserActivityAsync(user.Id, "LOGIN", $"Login success: {user.Username}", ipAddress);
                await Task.WhenAll(updateDeviceTask, logLoginTask);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LOGIN_PERF] post-login task error: {ex.Message}");
            }
        }

        private void SetLoginUiBusy(bool isBusy)
        {
            btnLogin.Enabled = !isBusy;
            Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
        }

        private static void LogLoginTiming(string stage, Stopwatch stopwatch)
        {
            Debug.WriteLine($"[LOGIN_PERF] {stage}: {stopwatch.ElapsedMilliseconds} ms");
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
