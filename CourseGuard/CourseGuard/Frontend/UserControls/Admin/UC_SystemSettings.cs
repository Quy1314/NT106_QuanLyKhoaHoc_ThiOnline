using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public class UC_SystemSettings : UserControl
    {
        private readonly SettingsController _settingsController;
        private TextBox _txtHost = new();
        private TextBox _txtPort = new();
        private TextBox _txtUser = new();
        private TextBox _txtPass = new();
        private TextBox _txtFromEmail = new();
        private TextBox _txtFromName = new();
        private NumericUpDown _numBruteForceLimit = new();
        private NumericUpDown _numLockoutMinutes = new();
        private NumericUpDown _numOtpExpiry = new();
        private Button _btnSave = new();
        private Button _btnReset = new();

        public UC_SystemSettings()
        {
            _settingsController = new SettingsController();
            BuildLayout();
            LoadSettings();
        }

        private void BuildLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = MetaTheme.Colors.FormBg;
            this.Padding = new Padding(20);

            // Scrollable Panel Container
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = MetaTheme.Colors.FormBg
            };

            // Title
            var lblTitle = new Label
            {
                Text = "Cài đặt & Cấu hình hệ thống",
                Font = MetaTheme.Fonts.HeadingLg(),
                ForeColor = MetaTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Table layout for structured form input
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 12,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 10)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            // SMTP Group Header
            AddHeaderRow(table, "1. CẤU HÌNH GỬI THƯ (SMTP EMAIL SERVER)");

            AddFormRow(table, "SMTP Host Server:", _txtHost);
            AddFormRow(table, "SMTP Port (e.g. 587, 465):", _txtPort);
            AddFormRow(table, "SMTP Username (Email):", _txtUser);
            AddFormRow(table, "SMTP Password (App Password):", _txtPass, isPassword: true);
            AddFormRow(table, "Email gửi đi (From Email):", _txtFromEmail);
            AddFormRow(table, "Tên người gửi (From Name):", _txtFromName);

            // Security Group Header
            AddHeaderRow(table, "2. CHÍNH SÁCH BẢO MẬT & XÁC THỰC");

            _numBruteForceLimit.Minimum = 3;
            _numBruteForceLimit.Maximum = 20;
            _numBruteForceLimit.Value = 5;
            _numBruteForceLimit.Font = MetaTheme.Fonts.BodyMd();
            _numBruteForceLimit.ForeColor = MetaTheme.Colors.TextPrimary;
            _numBruteForceLimit.BackColor = MetaTheme.Colors.InputBg;
            AddFormRow(table, "Giới hạn đăng nhập sai (Brute Force):", _numBruteForceLimit);

            _numLockoutMinutes.Minimum = 1;
            _numLockoutMinutes.Maximum = 1440; // 1 day
            _numLockoutMinutes.Value = 15;
            _numLockoutMinutes.Font = MetaTheme.Fonts.BodyMd();
            _numLockoutMinutes.ForeColor = MetaTheme.Colors.TextPrimary;
            _numLockoutMinutes.BackColor = MetaTheme.Colors.InputBg;
            AddFormRow(table, "Thời gian khóa tài khoản (Phút):", _numLockoutMinutes);

            _numOtpExpiry.Minimum = 1;
            _numOtpExpiry.Maximum = 60; // 1 hour
            _numOtpExpiry.Value = 5;
            _numOtpExpiry.Font = MetaTheme.Fonts.BodyMd();
            _numOtpExpiry.ForeColor = MetaTheme.Colors.TextPrimary;
            _numOtpExpiry.BackColor = MetaTheme.Colors.InputBg;
            AddFormRow(table, "Hạn sử dụng mã xác thực OTP (Phút):", _numOtpExpiry);

            // Buttons row panel
            var pnlButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 15, 0, 0)
            };

            _btnSave.Text = "Lưu cài đặt";
            _btnSave.Width = 140;
            _btnSave.Height = 40;
            _btnSave.Location = new Point(0, 15);
            _btnSave.Cursor = Cursors.Hand;
            MetaTheme.StylePrimaryButton(_btnSave);
            _btnSave.Click += BtnSave_Click;

            _btnReset.Text = "Khôi phục mặc định";
            _btnReset.Width = 160;
            _btnReset.Height = 40;
            _btnReset.Location = new Point(155, 15);
            _btnReset.Cursor = Cursors.Hand;
            MetaTheme.StyleGhostButton(_btnReset);
            _btnReset.Click += BtnReset_Click;

            RoundedButtonHelper.Apply(8, _btnSave, _btnReset);

            pnlButtons.Controls.Add(_btnSave);
            pnlButtons.Controls.Add(_btnReset);

            // Assemble
            scrollPanel.Controls.Add(pnlButtons);
            scrollPanel.Controls.Add(table);
            
            this.Controls.Add(scrollPanel);
            this.Controls.Add(lblTitle);
        }

        private void AddHeaderRow(TableLayoutPanel table, string title)
        {
            var lbl = new Label
            {
                Text = title,
                Font = MetaTheme.Fonts.SubtitleLg(),
                ForeColor = MetaTheme.Colors.Accent,
                AutoSize = true,
                Margin = new Padding(0, 20, 0, 10)
            };
            table.Controls.Add(lbl);
            table.SetColumnSpan(lbl, 2);
        }

        private void AddFormRow(TableLayoutPanel table, string labelText, Control control, bool isPassword = false)
        {
            var lbl = new Label
            {
                Text = labelText,
                Font = MetaTheme.Fonts.BodyMd(),
                ForeColor = MetaTheme.Colors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 6, 10, 6)
            };

            if (control is TextBox txt)
            {
                txt.Dock = DockStyle.Fill;
                txt.Font = MetaTheme.Fonts.BodyMd();
                txt.ForeColor = MetaTheme.Colors.TextPrimary;
                txt.BackColor = MetaTheme.Colors.InputBg;
                txt.BorderStyle = BorderStyle.FixedSingle;
                txt.Margin = new Padding(0, 6, 0, 6);
                if (isPassword)
                {
                    txt.UseSystemPasswordChar = true;
                }
            }
            else
            {
                control.Dock = DockStyle.Left;
                control.Width = 120;
                control.Margin = new Padding(0, 6, 0, 6);
            }

            table.Controls.Add(lbl);
            table.Controls.Add(control);
        }

        private void LoadSettings()
        {
            var settings = _settingsController.LoadSettings();

            _txtHost.Text = settings.GetValueOrDefault("SMTP_HOST", "smtp.gmail.com");
            _txtPort.Text = settings.GetValueOrDefault("SMTP_PORT", "587");
            _txtUser.Text = settings.GetValueOrDefault("SMTP_USER", "");
            _txtPass.Text = settings.GetValueOrDefault("SMTP_PASS", "");
            _txtFromEmail.Text = settings.GetValueOrDefault("SMTP_FROM_EMAIL", "");
            _txtFromName.Text = settings.GetValueOrDefault("SMTP_FROM_NAME", "CourseGuard Admin");

            if (int.TryParse(settings.GetValueOrDefault("BRUTE_FORCE_LIMIT", "5"), out int limit))
                _numBruteForceLimit.Value = Math.Clamp(limit, 3, 20);

            if (int.TryParse(settings.GetValueOrDefault("BRUTE_FORCE_LOCKOUT_MINUTES", "15"), out int lockout))
                _numLockoutMinutes.Value = Math.Clamp(lockout, 1, 1440);

            if (int.TryParse(settings.GetValueOrDefault("OTP_EXPIRY_MINUTES", "5"), out int otp))
                _numOtpExpiry.Value = Math.Clamp(otp, 1, 60);
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            // Validations
            if (string.IsNullOrWhiteSpace(_txtHost.Text) ||
                string.IsNullOrWhiteSpace(_txtPort.Text) ||
                string.IsNullOrWhiteSpace(_txtUser.Text) ||
                string.IsNullOrWhiteSpace(_txtPass.Text))
            {
                MetaTheme.ShowModernDialog("Vui lòng điền đầy đủ các thông tin SMTP Server.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _btnSave.Enabled = false;
            _btnSave.Text = "Đang lưu...";

            var newSettings = new Dictionary<string, string>
            {
                { "SMTP_HOST", _txtHost.Text.Trim() },
                { "SMTP_PORT", _txtPort.Text.Trim() },
                { "SMTP_USER", _txtUser.Text.Trim() },
                { "SMTP_PASS", _txtPass.Text.Trim() },
                { "SMTP_FROM_EMAIL", _txtFromEmail.Text.Trim() },
                { "SMTP_FROM_NAME", _txtFromName.Text.Trim() },
                { "BRUTE_FORCE_LIMIT", _numBruteForceLimit.Value.ToString() },
                { "BRUTE_FORCE_LOCKOUT_MINUTES", _numLockoutMinutes.Value.ToString() },
                { "OTP_EXPIRY_MINUTES", _numOtpExpiry.Value.ToString() }
            };

            bool success = await Task.Run(() => _settingsController.SaveSettings(newSettings));
            if (success)
            {
                MetaTheme.ShowModernDialog("Lưu cấu hình hệ thống thành công!", "Thành công");
            }
            else
            {
                MetaTheme.ShowModernDialog("Lưu cấu hình hệ thống thất bại. Hãy kiểm tra quyền truy cập file.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            _btnSave.Enabled = true;
            _btnSave.Text = "Lưu cài đặt";
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            if (MetaTheme.ShowModernDialog("Bạn có chắc chắn muốn khôi phục các giá trị mặc định của hệ thống?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _txtHost.Text = "smtp.gmail.com";
                _txtPort.Text = "587";
                _txtUser.Text = "";
                _txtPass.Text = "";
                _txtFromEmail.Text = "";
                _txtFromName.Text = "CourseGuard Admin";
                _numBruteForceLimit.Value = 5;
                _numLockoutMinutes.Value = 15;
                _numOtpExpiry.Value = 5;
            }
        }
    }
}
