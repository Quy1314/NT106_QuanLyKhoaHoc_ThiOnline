using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Login
{
    public class MfaOtpDialog : Form
    {
        private readonly int _userId;
        private readonly AuthController _authController;

        private Panel _cardPanel = null!;
        private Label _lblTitle = null!;
        private Label _lblInstructions = null!;
        private TextBox _txtOtp = null!;
        private Panel _otpShell = null!;
        private Button _btnConfirm = null!;
        private Button _btnCancel = null!;

        public MfaOtpDialog(int userId, AuthController authController)
        {
            _userId = userId;
            _authController = authController;

            InitializeForm();
            BuildControls();
            StyleControls();
        }

        private void InitializeForm()
        {
            this.Text = "Xác thực hai lớp (MFA) - CourseGuard";
            this.Size = new Size(420, 320);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = MetaTheme.Colors.FormBg;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;
        }

        private void BuildControls()
        {
            // Card Host
            _cardPanel = new Panel
            {
                Location = new Point(15, 15),
                Size = new Size(390, 290),
                BackColor = Color.FromArgb(15, 255, 255, 255) // Semi-transparent glass card
            };
            this.Controls.Add(_cardPanel);

            // Title
            _lblTitle = new Label
            {
                Text = "XÁC THỰC OTP",
                Font = FuturisticLoginKit.CreateUiFont(18f, FontStyle.Bold),
                ForeColor = MetaTheme.Colors.TextPrimary,
                Location = new Point(20, 20),
                Size = new Size(350, 32),
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = false
            };
            _cardPanel.Controls.Add(_lblTitle);

            // Instructions
            _lblInstructions = new Label
            {
                Text = "Mã OTP gồm 6 chữ số đã được gửi tới email của bạn. Vui lòng kiểm tra và nhập mã xác thực vào ô dưới đây:",
                Font = FuturisticLoginKit.CreateUiFont(9.5f, FontStyle.Regular),
                ForeColor = MetaTheme.Colors.TextSecondary,
                Location = new Point(25, 60),
                Size = new Size(340, 50),
                TextAlign = ContentAlignment.TopCenter,
                UseCompatibleTextRendering = false
            };
            _cardPanel.Controls.Add(_lblInstructions);

            // OTP Shell (glowing focus container)
            _otpShell = new Panel
            {
                Size = new Size(240, 44),
                Location = new Point(75, 125),
                BackColor = Color.FromArgb(24, 24, 32)
            };
            _cardPanel.Controls.Add(_otpShell);

            _txtOtp = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(24, 24, 32),
                ForeColor = MetaTheme.Colors.TextPrimary,
                Font = FuturisticLoginKit.CreateUiFont(16f, FontStyle.Bold),
                Location = new Point(10, 8),
                Size = new Size(220, 28),
                TextAlign = HorizontalAlignment.Center,
                MaxLength = 6
            };
            _otpShell.Controls.Add(_txtOtp);

            // Wire shell paint for focus border
            _otpShell.Paint += OtpShell_Paint;
            _txtOtp.GotFocus += (s, e) => { _otpShell.Invalidate(); _txtOtp.BackColor = Color.FromArgb(32, 32, 40); };
            _txtOtp.LostFocus += (s, e) => { _otpShell.Invalidate(); _txtOtp.BackColor = Color.FromArgb(24, 24, 32); };

            // Buttons
            _btnConfirm = new Button
            {
                Text = "XÁC NHẬN",
                Size = new Size(130, 42),
                Location = new Point(55, 195)
            };
            _btnConfirm.Click += BtnConfirm_Click;
            _cardPanel.Controls.Add(_btnConfirm);

            _btnCancel = new Button
            {
                Text = "HỦY",
                Size = new Size(130, 42),
                Location = new Point(205, 195)
            };
            _btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            _cardPanel.Controls.Add(_btnCancel);

            this.AcceptButton = _btnConfirm;
        }

        private void StyleControls()
        {
            MetaTheme.StyleCard(_cardPanel, 16);
            MetaTheme.StylePrimaryButton(_btnConfirm);
            MetaTheme.StyleSecondaryButton(_btnCancel);

            _btnConfirm.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            _btnCancel.Font = FuturisticLoginKit.CreateUiFont(10.5f, FontStyle.Bold);
            _btnConfirm.ForeColor = ColorTranslator.FromHtml("#002022");
            _btnCancel.ForeColor = MetaTheme.Colors.TextPrimary;

            CourseGuard.Frontend.Theme.RoundedButtonHelper.Apply(8, _btnConfirm, _btnCancel);
        }

        private void OtpShell_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var rect = new RectangleF(0.5f, 0.5f, _otpShell.Width - 1f, _otpShell.Height - 1f);
            using var path = FuturisticLoginKit.CreateRoundedRect(rect, 10f);

            bool focused = _txtOtp.Focused;
            var fill = focused ? Color.FromArgb(44, 255, 255, 255) : Color.FromArgb(28, 255, 255, 255);
            using (var b = new SolidBrush(fill))
                g.FillPath(b, path);

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

        private async void BtnConfirm_Click(object? sender, EventArgs e)
        {
            string otp = _txtOtp.Text.Trim();
            if (string.IsNullOrWhiteSpace(otp))
            {
                MetaTheme.ShowModernDialog("Vui lòng nhập mã OTP!");
                _txtOtp.Focus();
                return;
            }

            if (otp.Length != 6)
            {
                MetaTheme.ShowModernDialog("Mã OTP phải có đúng 6 chữ số!");
                _txtOtp.Focus();
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                bool mfaSuccess = await Task.Run(() => _authController.VerifyMfaOtp(_userId, otp));
                if (mfaSuccess)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MetaTheme.ShowModernDialog("Mã OTP không chính xác hoặc đã hết hạn!", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _txtOtp.Focus();
                    _txtOtp.SelectAll();
                }
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Lỗi xác thực OTP: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Draw outer border
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = FuturisticLoginKit.CreateRoundedRect(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), 18f);
            using var pen = new Pen(Color.FromArgb(64, 0, 240, 255), 1.5f); // Cyber Cyan glowing border
            e.Graphics.DrawPath(pen, path);
        }
    }
}
