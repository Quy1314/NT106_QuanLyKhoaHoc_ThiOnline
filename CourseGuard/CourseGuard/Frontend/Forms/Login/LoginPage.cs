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
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Login
{
    public partial class LoginPage : Form
    {
        private static readonly Lazy<AuthController> SharedAuthController =
            new(() => new AuthController(new CourseGuardDbContext("")));

        private AnimatedFuturisticBackgroundPanel? _background;
        private GlassCardHostPanel? _cardHost;

        private Panel? _usernameShell;
        private Panel? _passwordShell;
        private Label? _userIcon;
        private Label? _passIcon;
        private Label? _lblSubtitle;

        private const string UsernamePlaceholder = "Tên đăng nhập";
        private const string PasswordPlaceholder = "Mật khẩu";
        private bool _usernamePlaceholderActive;
        private bool _passwordPlaceholderActive;

        private readonly System.Windows.Forms.Timer _parallaxTimer = new() { Interval = 16 };
        private Point _lastMouse;
        private Point _cardBase;
        private PointF _cardOffset;

        public LoginPage()
        {
            InitializeComponent();
            CustomizeUI(); // Apply modern UI styles

            // Bo góc đồng bộ (10px) cho tất cả buttons
            CourseGuard.Frontend.Theme.RoundedButtonHelper.Apply(10,
                btnLogin, btnRegisterSubmit, btnForgotSubmit);

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
            SuspendLayout();

            // Form
            BackColor = MetaTheme.Colors.FormBg;
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;
            AcceptButton = btnLogin;

            EnsureFuturisticShell();

            // Panels inside glass card - make transparent to show glassmorphism card background
            foreach (var p in new[] { LoginPanel, RegisterPanel, ForgotPassPanel })
            {
                p.BackColor = Color.Transparent;
                p.BorderStyle = BorderStyle.None;
            }

            // Typography
            var titleFont = FuturisticLoginKit.CreateUiFont(28f, FontStyle.Bold);
            var smallBold = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            var inputFont = FuturisticLoginKit.CreateUiFont(11.5f, FontStyle.Regular);
            var buttonFont = FuturisticLoginKit.CreateUiFont(11.5f, FontStyle.Bold);

            // Title
            LoginTitle.Text = "ĐĂNG NHẬP";
            LoginTitle.Font = titleFont;
            LoginTitle.ForeColor = MetaTheme.Colors.TextPrimary;
            LoginTitle.AutoSize = false;
            LoginTitle.TextAlign = ContentAlignment.MiddleCenter;
            LoginTitle.UseCompatibleTextRendering = false;

            // Subtitle
            _lblSubtitle ??= new Label();
            _lblSubtitle.Text = "Hệ thống bảo mật khóa học thông minh";
            _lblSubtitle.Font = FuturisticLoginKit.CreateUiFont(10f, FontStyle.Regular);
            _lblSubtitle.ForeColor = MetaTheme.Colors.TextSecondary;
            _lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            _lblSubtitle.AutoSize = false;
            _lblSubtitle.BackColor = Color.Transparent;
            _lblSubtitle.UseCompatibleTextRendering = false;
            if (!LoginPanel.Controls.Contains(_lblSubtitle))
            {
                LoginPanel.Controls.Add(_lblSubtitle);
            }

            // Brand
            LOGO.Text = "COURSEGUARD";
            LOGO.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            LOGO.ForeColor = MetaTheme.Colors.Accent;
            LOGO.BorderStyle = BorderStyle.None;
            LOGO.TextAlign = ContentAlignment.MiddleCenter;
            LOGO.AutoSize = false;
            LOGO.BackColor = Color.Transparent;
            LOGO.UseCompatibleTextRendering = false;

            // Labels (subtle) - Hide to match mockup exactly
            lblUsername.Text = "Tên đăng nhập";
            lblUsername.Font = smallBold;
            lblUsername.ForeColor = MetaTheme.Colors.TextSecondary;
            lblUsername.BackColor = Color.Transparent;
            lblUsername.Visible = false;
            lblUsername.UseCompatibleTextRendering = false;

            lblPassword.Text = "Mật khẩu";
            lblPassword.Font = smallBold;
            lblPassword.ForeColor = MetaTheme.Colors.TextSecondary;
            lblPassword.BackColor = Color.Transparent;
            lblPassword.Visible = false;
            lblPassword.UseCompatibleTextRendering = false;

            // Input shells (icon left, glow focus)
            txtUsername.Font = inputFont;
            txtPassword.Font = inputFont;
            txtUsername.BorderStyle = BorderStyle.None;
            txtPassword.BorderStyle = BorderStyle.None;
            txtUsername.BackColor = Color.FromArgb(24, 24, 32);
            txtPassword.BackColor = Color.FromArgb(24, 24, 32);
            txtUsername.ForeColor = MetaTheme.Colors.TextPrimary;
            txtPassword.ForeColor = MetaTheme.Colors.TextPrimary;

            CreateInputShells();

            // Login button
            btnLogin.Text = "ĐĂNG NHẬP";
            MetaTheme.StylePrimaryButton(btnLogin);
            btnLogin.Font = buttonFont;
            btnLogin.ForeColor = ColorTranslator.FromHtml("#002022");

            // Links/Other
            chkRemember.Text = "Ghi nhớ đăng nhập";
            chkRemember.Font = FuturisticLoginKit.CreateUiFont(9.5f, FontStyle.Regular);
            chkRemember.ForeColor = MetaTheme.Colors.TextSecondary;
            chkRemember.BackColor = Color.Transparent;
            chkRemember.FlatStyle = FlatStyle.Flat;
            chkRemember.UseCompatibleTextRendering = false;

            linkLabel1.Text = "Quên mật khẩu?";
            linkLabel1.LinkColor = ColorTranslator.FromHtml("#00D0E9");
            linkLabel1.ActiveLinkColor = MetaTheme.Colors.AccentHover;
            linkLabel1.VisitedLinkColor = linkLabel1.LinkColor;
            linkLabel1.BackColor = Color.Transparent;
            linkLabel1.UseCompatibleTextRendering = false;

            lnkRegister.LinkColor = ColorTranslator.FromHtml("#00D0E9");
            lnkRegister.ActiveLinkColor = MetaTheme.Colors.AccentHover;
            lnkRegister.VisitedLinkColor = lnkRegister.LinkColor;
            lnkRegister.Text = "Chưa có tài khoản? Đăng ký ngay";
            lnkRegister.ForeColor = MetaTheme.Colors.TextSecondary;
            lnkRegister.LinkArea = new LinkArea(19, 12); // "Đăng ký ngay" starts at index 19, length 12
            lnkRegister.BackColor = Color.Transparent;
            lnkRegister.UseCompatibleTextRendering = false;

            // Register Controls (keep existing logic, just style)
            RegisterTitle.Text = "ĐĂNG KÝ TÀI KHOẢN";
            RegisterTitle.Font = FuturisticLoginKit.CreateUiFont(22f, FontStyle.Bold);
            RegisterTitle.ForeColor = MetaTheme.Colors.TextPrimary;
            RegisterTitle.BackColor = Color.Transparent;
            RegisterTitle.UseCompatibleTextRendering = false;
            lblRegUsername.Text = "Tên đăng nhập";
            lblRegFullName.Text = "Họ và tên";
            lblRegEmail.Text = "Địa chỉ Email";
            lblRegPassword.Text = "Mật khẩu";
            txtRegPassword.UseSystemPasswordChar = true;
            lnkBackToLoginFromReg.Text = "Quay lại Đăng nhập";
            lnkBackToLoginFromReg.LinkColor = MetaTheme.Colors.TextSecondary;
            lnkBackToLoginFromReg.ActiveLinkColor = MetaTheme.Colors.AccentHover;
            lnkBackToLoginFromReg.VisitedLinkColor = lnkBackToLoginFromReg.LinkColor;
            lnkBackToLoginFromReg.ForeColor = MetaTheme.Colors.TextSecondary;
            lnkBackToLoginFromReg.LinkArea = new LinkArea(0, lnkBackToLoginFromReg.Text.Length);
            lnkBackToLoginFromReg.UseCompatibleTextRendering = false;
            MetaTheme.StylePrimaryButton(btnRegisterSubmit);
            btnRegisterSubmit.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Bold);

            // Register panel eye-friendly contrast tuning
            var fieldLabelColor = MetaTheme.Colors.TextSecondary;
            foreach (var lbl in new[] { lblRegUsername, lblRegFullName, lblRegEmail, lblRegPassword })
            {
                lbl.ForeColor = fieldLabelColor;
                lbl.BackColor = Color.Transparent;
                lbl.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
                lbl.UseCompatibleTextRendering = false;
            }
            foreach (var tb in new[] { txtRegUsername, txtRegFullName, txtRegEmail, txtRegPassword })
            {
                tb.BorderStyle = BorderStyle.None;
                tb.BackColor = MetaTheme.Colors.InputBg;
                tb.ForeColor = MetaTheme.Colors.TextPrimary;
                tb.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Regular);
            }
            lnkBackToLoginFromReg.BackColor = Color.Transparent;

            // Forgot Controls
            ForgotTitle.Text = "QUÊN MẬT KHẨU";
            ForgotTitle.Font = FuturisticLoginKit.CreateUiFont(22f, FontStyle.Bold);
            ForgotTitle.ForeColor = MetaTheme.Colors.TextPrimary;
            ForgotTitle.UseCompatibleTextRendering = false;
            lblForgotUsername.Text = "Tên đăng nhập";
            lblForgotEmail.Text = "Email khôi phục";
            lnkBackToLoginFromForgot.Text = "Quay lại Đăng nhập";
            lnkBackToLoginFromForgot.LinkColor = MetaTheme.Colors.TextSecondary;
            lnkBackToLoginFromForgot.ActiveLinkColor = MetaTheme.Colors.AccentHover;
            lnkBackToLoginFromForgot.VisitedLinkColor = lnkBackToLoginFromForgot.LinkColor;
            lnkBackToLoginFromForgot.ForeColor = MetaTheme.Colors.TextSecondary;
            lnkBackToLoginFromForgot.LinkArea = new LinkArea(0, lnkBackToLoginFromForgot.Text.Length);
            lnkBackToLoginFromForgot.UseCompatibleTextRendering = false;
            btnForgotSubmit.Text = "GỬI YÊU CẦU";
            MetaTheme.StylePrimaryButton(btnForgotSubmit);
            btnForgotSubmit.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Bold);
            ForgotTitle.BackColor = Color.Transparent;
            lblForgotUsername.ForeColor = fieldLabelColor;
            lblForgotEmail.ForeColor = fieldLabelColor;
            lblForgotUsername.BackColor = Color.Transparent;
            lblForgotEmail.BackColor = Color.Transparent;
            lblForgotUsername.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            lblForgotEmail.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            lblForgotUsername.UseCompatibleTextRendering = false;
            lblForgotEmail.UseCompatibleTextRendering = false;
            txtForgotUsername.BorderStyle = BorderStyle.None;
            txtForgotEmail.BorderStyle = BorderStyle.None;
            txtForgotUsername.BackColor = MetaTheme.Colors.InputBg;
            txtForgotEmail.BackColor = MetaTheme.Colors.InputBg;
            txtForgotUsername.ForeColor = MetaTheme.Colors.TextPrimary;
            txtForgotEmail.ForeColor = MetaTheme.Colors.TextPrimary;
            txtForgotUsername.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Regular);
            txtForgotEmail.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Regular);
            lnkBackToLoginFromForgot.BackColor = Color.Transparent;

            ResumeLayout(true);
        }

        private void EnsureFuturisticShell()
        {
            _background ??= new AnimatedFuturisticBackgroundPanel();
            if (!Controls.Contains(_background))
            {
                Controls.Add(_background);
            }
            _background.BringToFront();

            _cardHost ??= new GlassCardHostPanel
            {
                Size = new Size(560, 610),
                CornerRadius = 32f,
                GlassFill = Color.FromArgb(12, 255, 255, 255), // beautiful semi-transparent glass
                BorderColor = Color.FromArgb(48, 0, 240, 255),  // Cyber Cyan glowing border
            };
            if (!_background.Controls.Contains(_cardHost))
            {
                _background.Controls.Add(_cardHost);
            }
            _cardHost.BringToFront();

            // host existing panels inside glass card
            LoginPanel.Parent = _cardHost;
            RegisterPanel.Parent = _cardHost;
            ForgotPassPanel.Parent = _cardHost;

            // Card parallax/tilt (subtle translation by mouse)
            _parallaxTimer.Tick -= ParallaxTick;
            _parallaxTimer.Tick += ParallaxTick;
            _parallaxTimer.Start();
            _background.MouseMove -= Background_MouseMove;
            _background.MouseMove += Background_MouseMove;
        }

        private void LoginPage_Load(object? sender, EventArgs e)
        {
            CenterPanel();
            ResizePanel();
            ResizeAllControls();
        }

        private void LoginPage_Resize(object? sender, EventArgs e)
        {
            CenterPanel();
            ResizePanel();
            ResizeAllControls();
        }

        private void CenterPanel()
        {
            if (_background == null || _cardHost == null) return;

            int maxW = 620;
            int maxH = 680;
            int w = Math.Min(maxW, Math.Max(420, (int)(ClientSize.Width * 0.54)));
            int h = Math.Min(maxH, Math.Max(540, (int)(ClientSize.Height * 0.80)));

            _cardHost.Size = new Size(w, h);
            _cardBase = new Point((ClientSize.Width - _cardHost.Width) / 2, (ClientSize.Height - _cardHost.Height) / 2);
            _cardHost.Location = _cardBase;

            LoginPanel.Left = (_cardHost.Width - LoginPanel.Width) / 2;
            LoginPanel.Top = (_cardHost.Height - LoginPanel.Height) / 2;

            RegisterPanel.Left = (_cardHost.Width - RegisterPanel.Width) / 2;
            RegisterPanel.Top = (_cardHost.Height - RegisterPanel.Height) / 2;

            ForgotPassPanel.Left = (_cardHost.Width - ForgotPassPanel.Width) / 2;
            ForgotPassPanel.Top = (_cardHost.Height - ForgotPassPanel.Height) / 2;
        }

        private void ResizePanel()
        {
            int containerWidth = _cardHost?.Width ?? this.ClientSize.Width;
            int containerHeight = _cardHost?.Height ?? this.ClientSize.Height;

            int newWidth = containerWidth - 60;
            if (newWidth > 520) newWidth = 520;
            if (newWidth < 380) newWidth = 380;

            int newHeight = containerHeight - 80;
            if (newHeight > 500) newHeight = 500;
            if (newHeight < 420) newHeight = 420;

            var size = new Size(newWidth, newHeight);
            LoginPanel.Size = size;
            RegisterPanel.Size = size;
            ForgotPassPanel.Size = size;

            // Recenter panels in card host
            if (_cardHost != null)
            {
                LoginPanel.Left = (_cardHost.Width - LoginPanel.Width) / 2;
                LoginPanel.Top = (_cardHost.Height - LoginPanel.Height) / 2;

                RegisterPanel.Left = (_cardHost.Width - RegisterPanel.Width) / 2;
                RegisterPanel.Top = (_cardHost.Height - RegisterPanel.Height) / 2;

                ForgotPassPanel.Left = (_cardHost.Width - ForgotPassPanel.Width) / 2;
                ForgotPassPanel.Top = (_cardHost.Height - ForgotPassPanel.Height) / 2;
            }
        }

        private void ResizeAllControls()
        {
            int padding = 34;
            int fullWidth = LoginPanel.Width - padding * 2;

            // Center Title and Logo
            LoginTitle.Width = LoginPanel.Width;
            LOGO.Width = LoginPanel.Width;

            // Textbox full width
            _usernameShell!.Width = fullWidth;
            _passwordShell!.Width = fullWidth;
            _usernameShell.Left = padding;
            _passwordShell.Left = padding;

            txtUsername.Width = _usernameShell.Width - 60;
            txtPassword.Width = _passwordShell.Width - 60;

            lblUsername.Left = padding;
            lblPassword.Left = padding;

            // Reposition Y coords to look good and prevent clipping
            LOGO.Top = 20;
            LOGO.Height = 22;

            LoginTitle.Top = 46;
            LoginTitle.Height = 55; // Sized appropriately for 28f font height + Vietnamese diacritics

            if (_lblSubtitle != null)
            {
                _lblSubtitle.Top = 104;
                _lblSubtitle.Height = 22;
                _lblSubtitle.Width = LoginPanel.Width;
                _lblSubtitle.Left = 0;
            }

            lblUsername.Visible = false;
            lblPassword.Visible = false;

            // Tighten input shells vertical position to match mockup (no username/password label gaps)
            _usernameShell!.Top = 144;
            _passwordShell!.Top = 204;

            // Re-arrange Checkbox and Forgot Password if they exist
            chkRemember.Top = _passwordShell.Bottom + 14;
            linkLabel1.Top = chkRemember.Top;

            chkRemember.Left = padding;
            linkLabel1.Left = LoginPanel.Width - linkLabel1.Width - padding;

            // Shift Button down
            btnLogin.Width = fullWidth;
            btnLogin.Left = padding;
            btnLogin.Height = 48;
            btnLogin.Top = chkRemember.Bottom + 16;

            // Center "Don't have an account? Sign Up" link
            lnkRegister.Left = (LoginPanel.Width - lnkRegister.Width) / 2;
            lnkRegister.Top = btnLogin.Bottom + 14;

            // --- Register Panel Controls ---
            int regPadding = 40;
            int regFullWidth = RegisterPanel.Width - regPadding * 2;
            
            RegisterTitle.Location = new Point(0, 20);
            RegisterTitle.Width = RegisterPanel.Width;
            RegisterTitle.Height = 55; // Sized for 22f font height + Vietnamese diacritics
            RegisterTitle.TextAlign = ContentAlignment.MiddleCenter;
            RegisterTitle.AutoSize = false;

            lblRegUsername.Location = new Point(regPadding, 85);
            txtRegUsername.Location = new Point(regPadding, 110);
            txtRegUsername.Width = regFullWidth;

            lblRegFullName.Location = new Point(regPadding, 150);
            txtRegFullName.Location = new Point(regPadding, 175);
            txtRegFullName.Width = regFullWidth;

            lblRegEmail.Location = new Point(regPadding, 215);
            txtRegEmail.Location = new Point(regPadding, 240);
            txtRegEmail.Width = regFullWidth;

            lblRegPassword.Location = new Point(regPadding, 280);
            txtRegPassword.Location = new Point(regPadding, 305);
            txtRegPassword.Width = regFullWidth;

            btnRegisterSubmit.Location = new Point(regPadding, 365);
            btnRegisterSubmit.Width = regFullWidth;

            lnkBackToLoginFromReg.Location = new Point((RegisterPanel.Width - lnkBackToLoginFromReg.Width) / 2, 425);

            // --- Forgot Panel Controls ---
            int forPadding = 40;
            int forFullWidth = ForgotPassPanel.Width - forPadding * 2;

            ForgotTitle.Location = new Point(0, 20);
            ForgotTitle.Width = ForgotPassPanel.Width;
            ForgotTitle.Height = 55; // Sized for 22f font height + Vietnamese diacritics
            ForgotTitle.TextAlign = ContentAlignment.MiddleCenter;
            ForgotTitle.AutoSize = false;

            lblForgotUsername.Location = new Point(forPadding, 95);
            txtForgotUsername.Location = new Point(forPadding, 120);
            txtForgotUsername.Width = forFullWidth;

            lblForgotEmail.Location = new Point(forPadding, 170);
            txtForgotEmail.Location = new Point(forPadding, 195);
            txtForgotEmail.Width = forFullWidth;

            btnForgotSubmit.Location = new Point(forPadding, 255);
            btnForgotSubmit.Width = forFullWidth;

            lnkBackToLoginFromForgot.Location = new Point((ForgotPassPanel.Width - lnkBackToLoginFromForgot.Width) / 2, 315);

            // Avoid paint artifacts when controls are repositioned.
            LoginPanel.Invalidate(true);
            RegisterPanel.Invalidate(true);
            ForgotPassPanel.Invalidate(true);
        }

        private void Background_MouseMove(object? sender, MouseEventArgs e)
        {
            _lastMouse = e.Location;
        }

        private void ParallaxTick(object? sender, EventArgs e)
        {
            if (_background == null || _cardHost == null) return;

            float nx = (ClientSize.Width <= 0) ? 0f : (_lastMouse.X / (float)ClientSize.Width) * 2f - 1f;
            float ny = (ClientSize.Height <= 0) ? 0f : (_lastMouse.Y / (float)ClientSize.Height) * 2f - 1f;

            // Subtle translation only (WinForms can't tilt child controls nicely)
            var target = new PointF(nx * 8f, ny * 6f);
            _cardOffset = new PointF(_cardOffset.X + (target.X - _cardOffset.X) * 0.08f,
                                     _cardOffset.Y + (target.Y - _cardOffset.Y) * 0.08f);
            _cardHost.Location = new Point(_cardBase.X + (int)_cardOffset.X, _cardBase.Y + (int)_cardOffset.Y);
        }

        private void CreateInputShells()
        {
            _usernameShell ??= new TransparentPanel();
            _passwordShell ??= new TransparentPanel();
            _userIcon ??= new Label();
            _passIcon ??= new Label();

            BuildInputShell(_usernameShell, _userIcon, txtUsername, "\uE77B"); // Contact
            BuildInputShell(_passwordShell, _passIcon, txtPassword, "\uE72E"); // Lock

            if (!LoginPanel.Controls.Contains(_usernameShell)) LoginPanel.Controls.Add(_usernameShell);
            if (!LoginPanel.Controls.Contains(_passwordShell)) LoginPanel.Controls.Add(_passwordShell);

            // Remove textboxes from panel root so we can host inside shells
            if (LoginPanel.Controls.Contains(txtUsername)) LoginPanel.Controls.Remove(txtUsername);
            if (LoginPanel.Controls.Contains(txtPassword)) LoginPanel.Controls.Remove(txtPassword);

            _usernameShell.Controls.Clear();
            _usernameShell.Controls.Add(_userIcon);
            _usernameShell.Controls.Add(txtUsername);

            _passwordShell.Controls.Clear();
            _passwordShell.Controls.Add(_passIcon);
            _passwordShell.Controls.Add(txtPassword);

            txtUsername.Location = new Point(42, 10);
            txtPassword.Location = new Point(42, 10);
            txtUsername.Width = _usernameShell.Width - 60;
            txtPassword.Width = _passwordShell.Width - 60;

            txtUsername.GotFocus += (_, _) => { 
                _usernameShell.Invalidate(); 
                txtUsername.BackColor = Color.FromArgb(32, 32, 40); 
            };
            txtUsername.LostFocus += (_, _) => { 
                _usernameShell.Invalidate(); 
                txtUsername.BackColor = Color.FromArgb(24, 24, 32); 
            };
            txtPassword.GotFocus += (_, _) => { 
                _passwordShell.Invalidate(); 
                txtPassword.BackColor = Color.FromArgb(32, 32, 40); 
            };
            txtPassword.LostFocus += (_, _) => { 
                _passwordShell.Invalidate(); 
                txtPassword.BackColor = Color.FromArgb(24, 24, 32); 
            };

            txtUsername.Enter -= TxtUsername_Enter;
            txtUsername.Leave -= TxtUsername_Leave;
            txtPassword.Enter -= TxtPassword_Enter;
            txtPassword.Leave -= TxtPassword_Leave;
            txtUsername.Enter += TxtUsername_Enter;
            txtUsername.Leave += TxtUsername_Leave;
            txtPassword.Enter += TxtPassword_Enter;
            txtPassword.Leave += TxtPassword_Leave;

            ApplyUsernamePlaceholder();
            ApplyPasswordPlaceholder();
        }

        private void BuildInputShell(Panel shell, Label icon, TextBox box, string glyph)
        {
            shell.BackColor = Color.Transparent;
            shell.Size = new Size(320, 44);
            shell.Left = 34;
            shell.Paint -= InputShell_Paint;
            shell.Paint += InputShell_Paint;
            shell.Cursor = Cursors.IBeam;
            shell.Click += (_, _) => box.Focus();

            icon.Text = glyph;
            icon.Font = new Font("Segoe MDL2 Assets", 16f, FontStyle.Regular);
            icon.ForeColor = MetaTheme.Colors.TextSecondary;
            icon.AutoSize = false;
            icon.Size = new Size(34, 44);
            icon.Location = new Point(10, 0);
            icon.TextAlign = ContentAlignment.MiddleCenter;
            icon.BackColor = Color.Transparent;
            icon.Click += (_, _) => box.Focus();

            box.BorderStyle = BorderStyle.None;
            box.BackColor = Color.FromArgb(24, 24, 32);
            box.ForeColor = MetaTheme.Colors.TextPrimary;
        }

        private void ApplyUsernamePlaceholder()
        {
            if (!string.IsNullOrWhiteSpace(txtUsername.Text)) return;
            _usernamePlaceholderActive = true;
            txtUsername.Text = UsernamePlaceholder;
            txtUsername.ForeColor = MetaTheme.Colors.TextMuted;
        }

        private void ApplyPasswordPlaceholder()
        {
            if (!string.IsNullOrWhiteSpace(txtPassword.Text)) return;
            _passwordPlaceholderActive = true;
            txtPassword.UseSystemPasswordChar = false;
            txtPassword.Text = PasswordPlaceholder;
            txtPassword.ForeColor = MetaTheme.Colors.TextMuted;
        }

        private void TxtUsername_Enter(object? sender, EventArgs e)
        {
            if (!_usernamePlaceholderActive) return;
            _usernamePlaceholderActive = false;
            txtUsername.Text = string.Empty;
            txtUsername.ForeColor = MetaTheme.Colors.TextPrimary;
        }

        private void TxtUsername_Leave(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ApplyUsernamePlaceholder();
            }
        }

        private void TxtPassword_Enter(object? sender, EventArgs e)
        {
            if (!_passwordPlaceholderActive) return;
            _passwordPlaceholderActive = false;
            txtPassword.Text = string.Empty;
            txtPassword.ForeColor = MetaTheme.Colors.TextPrimary;
            txtPassword.UseSystemPasswordChar = true;
        }

        private void TxtPassword_Leave(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ApplyPasswordPlaceholder();
            }
        }

        private void InputShell_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel shell) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0.5f, 0.5f, shell.Width - 1f, shell.Height - 1f);
            using var path = FuturisticLoginKit.CreateRoundedRect(rect, 18f);

            bool focused = shell.ContainsFocus;
            var fill = focused ? Color.FromArgb(44, 255, 255, 255) : Color.FromArgb(28, 255, 255, 255);
            using (var b = new SolidBrush(fill))
                g.FillPath(b, path);

            // glow border on focus
            if (focused)
            {
                using var glowPen = new Pen(MetaTheme.Colors.Accent, 1.4f);
                g.DrawPath(glowPen, path);
            }
            else
            {
                using var pen = new Pen(Color.FromArgb(95, 255, 255, 255), 1.1f);
                g.DrawPath(pen, path);
            }
        }


        private void btnRegisterSubmit_Click(object? sender, EventArgs e)
        {
            string user = txtRegUsername.Text.Trim();
            string name = txtRegFullName.Text.Trim();
            string email = txtRegEmail.Text.Trim();
            string pass = txtRegPassword.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng nhập đầy đủ thông tin đăng ký!");
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
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Yêu cầu đăng ký đã được gửi tới Admin chờ phê duyệt", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearRegisterInputs();
                ShowPanel(LoginPanel);
            }
            else
            {
                string detail = string.IsNullOrWhiteSpace(AuthService.LastErrorMessage)
                    ? "Đăng ký thất bại. Tên đăng nhập có thể đã tồn tại."
                    : AuthService.LastErrorMessage;
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(detail, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng nhập Username và Email!");
                return;
            }

            bool success = AuthService.ForgotPasswordRequest(user, email);

            if (success)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Yêu cầu cấp lại mật khẩu đã được tiếp nhận. Vui lòng liên hệ Admin để duyệt hoặc kiểm tra hòm thư của bạn nếu thông tin chính xác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtForgotUsername.Clear();
                txtForgotEmail.Clear();
                ShowPanel(LoginPanel);
            }
            else
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Thông tin không chính xác hoặc không tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = _usernamePlaceholderActive ? string.Empty : txtUsername.Text.Trim();
            string password = _passwordPlaceholderActive ? string.Empty : txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            try
            {
                SetLoginUiBusy(true);
                ForceChangePassword = false;
                Stopwatch stopwatch = Stopwatch.StartNew();
                LogLoginTiming("start login", stopwatch);

                LoginResultModel loginResult = await AuthService.LoginAsync(username, password, Environment.MachineName);
                LogLoginTiming("auth response", stopwatch);

                if (loginResult.ErrorCode == LoginErrorCodes.DeviceBlocked)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(
                        "Thiết bị truy cập của bạn đã bị khóa bởi Quản trị viên (Admin).\nVui lòng liên hệ Admin để mở khóa thiết bị.",
                        "Thiết bị bị khóa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (loginResult.ErrorCode == LoginErrorCodes.AccountLocked)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(
                        "Tài khoản tạm thời bị khóa do nhập sai mật khẩu quá nhiều lần.",
                        "Tài khoản bị khóa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (loginResult.ErrorCode == LoginErrorCodes.TempPasswordExpired)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(
                        "Mat khau tam thoi da het han. Vui long gui lai yeu cau quen mat khau de Admin cap mat khau moi.",
                        "Mat khau tam thoi het han",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                UserModel? user = loginResult.User;
                if (user == null)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (loginResult.IsMfaRequired)
                {
                    using (var otpDialog = new MfaOtpDialog(user.Id, AuthService))
                    {
                        if (otpDialog.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }
                    }
                    loginResult = AuthService.CompleteMfaLogin(user);
                }

                Task<string> ipAddressTask = GetLocalIPAddressAsync();
                await ipAddressTask;
                LogLoginTiming("load user data", stopwatch);

                UserModel finalUser = user;
                string ipAddress = await ipAddressTask;

                if (!string.Equals(finalUser.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog($"Tài khoản của bạn đang ở trạng thái '{finalUser.Status}'. Vui lòng liên hệ quản trị viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string normalizedRole = (finalUser.Role ?? string.Empty).Trim().ToUpperInvariant();
                if (normalizedRole != "ADMIN" && normalizedRole != "TEACHER" && normalizedRole != "STUDENT")
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog($"Quyền tài khoản không hợp lệ: {finalUser.Role}. Vui lòng liên hệ quản trị viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (loginResult.MustChangePassword && normalizedRole == "ADMIN")
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(
                        "Tai khoan Admin dang dung mat khau tam thoi nhung man hinh Admin hien chua co khu vuc doi mat khau. Vui long nho Admin khac dat lai mat khau chinh thuc.",
                        "Can doi mat khau",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Login Success
                finalUser.Role = normalizedRole;
                CurrentUser = finalUser;
                ForceChangePassword = loginResult.MustChangePassword;
                UserSessionContext.SetCurrentUser(finalUser.Id, finalUser.Role, finalUser.Username, finalUser.FullName, finalUser.AvatarPath);

                // Non-blocking post-login tasks
                RunPostLoginTasksAsync(finalUser, ipAddress).FireAndForgetSafe(this);
                LogLoginTiming("load ui", stopwatch);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi kết nối bộ máy chủ Database!\n\nChi tiết kỹ thuật:\n" + ex.ToString(), "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
                Task updateDeviceTask = AuthService.UpdateLoginInfoAsync(user.Id, Environment.MachineName, ipAddress);
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
        public bool ForceChangePassword { get; private set; }
    }
}
