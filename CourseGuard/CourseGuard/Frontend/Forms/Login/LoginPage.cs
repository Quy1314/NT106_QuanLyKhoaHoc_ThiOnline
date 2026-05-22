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

        private FlowLayoutPanel? _socialRow;
        private Button? _btnGoogle;
        private Button? _btnGithub;
        private Button? _btnDiscord;
        private const string UsernamePlaceholder = "Enter username";
        private const string PasswordPlaceholder = "Enter password";
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

            // Panels inside glass card
            foreach (var p in new[] { LoginPanel, RegisterPanel, ForgotPassPanel })
            {
                p.BackColor = MetaTheme.Colors.CardBg;
                p.BorderStyle = BorderStyle.None;
            }

            // Typography
            var titleFont = FuturisticLoginKit.CreateUiFont(28f, FontStyle.Bold);
            var smallBold = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            var inputFont = FuturisticLoginKit.CreateUiFont(11.5f, FontStyle.Regular);
            var buttonFont = FuturisticLoginKit.CreateUiFont(11.5f, FontStyle.Bold);

            // Title
            LoginTitle.Text = "Sign In";
            LoginTitle.Font = titleFont;
            LoginTitle.ForeColor = MetaTheme.Colors.TextPrimary;
            LoginTitle.AutoSize = false;
            LoginTitle.TextAlign = ContentAlignment.MiddleCenter;

            // Brand
            LOGO.Text = "COURSEGUARD";
            LOGO.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            LOGO.ForeColor = MetaTheme.Colors.Accent;
            LOGO.BorderStyle = BorderStyle.None;
            LOGO.TextAlign = ContentAlignment.MiddleCenter;
            LOGO.AutoSize = false;
            LOGO.BackColor = Color.Transparent;

            // Labels (subtle)
            lblUsername.Font = smallBold;
            lblUsername.ForeColor = MetaTheme.Colors.TextSecondary;
            lblPassword.Font = smallBold;
            lblPassword.ForeColor = MetaTheme.Colors.TextSecondary;

            // Input shells (icon left, glow focus)
            txtUsername.Font = inputFont;
            txtPassword.Font = inputFont;
            txtUsername.BorderStyle = BorderStyle.None;
            txtPassword.BorderStyle = BorderStyle.None;
            txtUsername.BackColor = MetaTheme.Colors.InputBg;
            txtPassword.BackColor = MetaTheme.Colors.InputBg;
            txtUsername.ForeColor = MetaTheme.Colors.TextPrimary;
            txtPassword.ForeColor = MetaTheme.Colors.TextPrimary;

            CreateInputShells();

            // Login button
            btnLogin.Text = "Login";
            MetaTheme.StylePrimaryButton(btnLogin);
            btnLogin.Font = buttonFont;

            // Links/Other
            chkRemember.Font = FuturisticLoginKit.CreateUiFont(9.5f, FontStyle.Regular);
            chkRemember.ForeColor = MetaTheme.Colors.TextSecondary;
            chkRemember.BackColor = LoginPanel.BackColor;
            chkRemember.FlatStyle = FlatStyle.Flat;

            linkLabel1.LinkColor = MetaTheme.Colors.Accent;
            linkLabel1.ActiveLinkColor = MetaTheme.Colors.AccentHover;
            linkLabel1.VisitedLinkColor = linkLabel1.LinkColor;
            linkLabel1.BackColor = LoginPanel.BackColor;

            lnkRegister.LinkColor = MetaTheme.Colors.Accent;
            lnkRegister.ActiveLinkColor = MetaTheme.Colors.AccentHover;
            lnkRegister.Text = "Don't have an account? Sign Up";
            lnkRegister.BackColor = LoginPanel.BackColor;

            // Social buttons
            CreateSocialButtons();

            // Register Controls (keep existing logic, just style)
            RegisterTitle.Text = "Create Account";
            RegisterTitle.Font = FuturisticLoginKit.CreateUiFont(22f, FontStyle.Bold);
            RegisterTitle.ForeColor = MetaTheme.Colors.TextPrimary;
            RegisterTitle.BackColor = Color.Transparent;
            lblRegUsername.Text = "Username";
            lblRegFullName.Text = "Full Name";
            lblRegEmail.Text = "Email";
            lblRegPassword.Text = "Password";
            txtRegPassword.UseSystemPasswordChar = true;
            lnkBackToLoginFromReg.Text = "Back to Login";
            lnkBackToLoginFromReg.LinkColor = MetaTheme.Colors.TextSecondary;
            MetaTheme.StylePrimaryButton(btnRegisterSubmit);
            btnRegisterSubmit.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Bold);

            // Register panel eye-friendly contrast tuning
            var fieldLabelColor = MetaTheme.Colors.TextSecondary;
            foreach (var lbl in new[] { lblRegUsername, lblRegFullName, lblRegEmail, lblRegPassword })
            {
                lbl.ForeColor = fieldLabelColor;
                lbl.BackColor = Color.Transparent;
                lbl.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            }
            foreach (var tb in new[] { txtRegUsername, txtRegFullName, txtRegEmail, txtRegPassword })
            {
                tb.BorderStyle = BorderStyle.None;
                tb.BackColor = MetaTheme.Colors.InputBg;
                tb.ForeColor = MetaTheme.Colors.TextPrimary;
                tb.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Regular);
            }
            lnkBackToLoginFromReg.BackColor = Color.Transparent;
            lnkBackToLoginFromReg.ActiveLinkColor = MetaTheme.Colors.AccentHover;
            lnkBackToLoginFromReg.VisitedLinkColor = lnkBackToLoginFromReg.LinkColor;

            // Forgot Controls
            ForgotTitle.Text = "Forgot Password";
            ForgotTitle.Font = FuturisticLoginKit.CreateUiFont(22f, FontStyle.Bold);
            ForgotTitle.ForeColor = MetaTheme.Colors.TextPrimary;
            lblForgotUsername.Text = "Username";
            lblForgotEmail.Text = "Email";
            lnkBackToLoginFromForgot.Text = "Back to Login";
            lnkBackToLoginFromForgot.LinkColor = MetaTheme.Colors.TextSecondary;
            btnForgotSubmit.Text = "Xác nhận quên mật khẩu";
            MetaTheme.StylePrimaryButton(btnForgotSubmit);
            btnForgotSubmit.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Bold);
            ForgotTitle.BackColor = Color.Transparent;
            lblForgotUsername.ForeColor = fieldLabelColor;
            lblForgotEmail.ForeColor = fieldLabelColor;
            lblForgotUsername.BackColor = Color.Transparent;
            lblForgotEmail.BackColor = Color.Transparent;
            lblForgotUsername.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            lblForgotEmail.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            txtForgotUsername.BorderStyle = BorderStyle.None;
            txtForgotEmail.BorderStyle = BorderStyle.None;
            txtForgotUsername.BackColor = MetaTheme.Colors.InputBg;
            txtForgotEmail.BackColor = MetaTheme.Colors.InputBg;
            txtForgotUsername.ForeColor = MetaTheme.Colors.TextPrimary;
            txtForgotEmail.ForeColor = MetaTheme.Colors.TextPrimary;
            txtForgotUsername.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Regular);
            txtForgotEmail.Font = FuturisticLoginKit.CreateUiFont(11f, FontStyle.Regular);
            lnkBackToLoginFromForgot.BackColor = Color.Transparent;
            lnkBackToLoginFromForgot.ActiveLinkColor = MetaTheme.Colors.AccentHover;
            lnkBackToLoginFromForgot.VisitedLinkColor = lnkBackToLoginFromForgot.LinkColor;

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
                CornerRadius = 28f,
                GlassFill = Color.FromArgb(40, 255, 255, 255),
                BorderColor = Color.FromArgb(120, 255, 255, 255),
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
            int newWidth = containerWidth - 60;
            if (newWidth > 520) newWidth = 520;

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

            txtUsername.Width = _usernameShell.Width - 58;
            txtPassword.Width = _passwordShell.Width - 58;

            lblUsername.Left = padding;
            lblPassword.Left = padding;

            // Reposition Y coords to look good
            // Assuming fixed Y positions relative to top for simplicity, or dynamic?
            // Let's stick to the current relative logic or set hard defaults if easier.
            // But CustomizeUI set initial Y. Let's ensure they are consistent.

            // Adjust Y positions for spacing
            LOGO.Top = 44;
            LOGO.Height = 24;

            LoginTitle.Top = 70;
            LoginTitle.Height = 52;

            lblUsername.Top = 140;
            _usernameShell!.Top = 166;

            lblPassword.Top = 226;
            _passwordShell!.Top = 252;

            // Button
            btnLogin.Width = fullWidth;
            btnLogin.Left = padding;
            btnLogin.Height = 48;

            // Hide old labels/controls logic removed since controls are deleted.

            // Re-arrange Checkbox and Forgot Password if they exist
            // My new design didn't account for them explicitly in the Plan but user has them.
            // Let's place them below Password.
            chkRemember.Top = _passwordShell.Bottom + 16;
            linkLabel1.Top = chkRemember.Top;

            chkRemember.Left = padding;
            linkLabel1.Left = LoginPanel.Width - linkLabel1.Width - padding;

            // Shift Button down
            btnLogin.Top = chkRemember.Bottom + 18;

            // Center "Don't have an account? Sign Up" link
            lnkRegister.Left = (LoginPanel.Width - lnkRegister.Width) / 2;
            lnkRegister.Top = btnLogin.Bottom + 14;

            // Social row
            if (_socialRow != null)
            {
                _socialRow.Width = fullWidth;
                _socialRow.Left = padding;
                _socialRow.Top = lnkRegister.Bottom + 18;
                int gap = 10;
                int btnW = (fullWidth - gap * 2) / 3;
                foreach (Control c in _socialRow.Controls)
                {
                    if (c is Button b) b.Width = btnW;
                }
            }

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
            _usernameShell ??= new Panel();
            _passwordShell ??= new Panel();
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
            txtUsername.Width = _usernameShell.Width - 52;
            txtPassword.Width = _passwordShell.Width - 52;

            txtUsername.GotFocus += (_, _) => { _usernameShell.Invalidate(); };
            txtUsername.LostFocus += (_, _) => { _usernameShell.Invalidate(); };
            txtPassword.GotFocus += (_, _) => { _passwordShell.Invalidate(); };
            txtPassword.LostFocus += (_, _) => { _passwordShell.Invalidate(); };

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
            box.BackColor = MetaTheme.Colors.InputBg;
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
                using var glowPen = new Pen(Color.FromArgb(180, 30, 220, 255), 1.4f);
                g.DrawPath(glowPen, path);
            }
            else
            {
                using var pen = new Pen(Color.FromArgb(95, 255, 255, 255), 1.1f);
                g.DrawPath(pen, path);
            }
        }

        private void CreateSocialButtons()
        {
            if (_socialRow != null && LoginPanel.Controls.Contains(_socialRow)) return;

            _socialRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Height = 44,
                BackColor = Color.Transparent
            };

            _btnGoogle = CreateSocialButton("Google", Color.FromArgb(60, 255, 255, 255), Color.FromArgb(200, 255, 80, 80));
            _btnGithub = CreateSocialButton("GitHub", Color.FromArgb(60, 255, 255, 255), Color.FromArgb(200, 180, 180, 180));
            _btnDiscord = CreateSocialButton("Discord", Color.FromArgb(60, 255, 255, 255), Color.FromArgb(200, 140, 70, 255));

            _socialRow.Controls.Add(_btnGoogle);
            _socialRow.Controls.Add(_btnGithub);
            _socialRow.Controls.Add(_btnDiscord);

            LoginPanel.Controls.Add(_socialRow);
        }

        private Button CreateSocialButton(string text, Color baseFill, Color glow)
        {
            var b = new Button
            {
                Text = text,
                Width = 0,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = baseFill,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0),
                Font = FuturisticLoginKit.CreateUiFont(9.5f, FontStyle.Bold),
            };
            b.FlatAppearance.BorderSize = 0;
            RoundedButtonHelper.Apply(10, b);

            b.MouseEnter += (_, _) => { b.BackColor = Color.FromArgb(85, 255, 255, 255); b.Invalidate(); };
            b.MouseLeave += (_, _) => { b.BackColor = baseFill; b.Invalidate(); };
            b.Click += (_, _) => MessageBox.Show("Social login chưa được tích hợp ở phiên bản này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            b.Paint += (_, e) =>
            {
                if (!b.ClientRectangle.Contains(b.PointToClient(Cursor.Position))) return;
                using var p = new Pen(Color.FromArgb(120, glow), 1.2f);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(0.6f, 0.6f, b.Width - 1.2f, b.Height - 1.2f);
                using var path = FuturisticLoginKit.CreateRoundedRect(r, 12f);
                e.Graphics.DrawPath(p, path);
            };

            return b;
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
                string detail = string.IsNullOrWhiteSpace(AuthService.LastErrorMessage)
                    ? "Đăng ký thất bại. Tên đăng nhập có thể đã tồn tại."
                    : AuthService.LastErrorMessage;
                MessageBox.Show(detail, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            string username = _usernamePlaceholderActive ? string.Empty : txtUsername.Text.Trim();
            string password = _passwordPlaceholderActive ? string.Empty : txtPassword.Text.Trim();

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
                UserSessionContext.SetCurrentUser(finalUser.Id, finalUser.Role, finalUser.Username, finalUser.FullName);

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
