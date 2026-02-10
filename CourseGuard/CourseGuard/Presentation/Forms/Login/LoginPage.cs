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
using CourseGuard.Application.Services;
using CourseGuard.Core.Models;

namespace CourseGuard.Presentation.Forms.Login
{
    public partial class LoginPage : Form
    {
        public LoginPage()
        {
            InitializeComponent();
            CustomizeUI(); // Apply modern UI styles
            this.Load += LoginPage_Load;
            this.Resize += LoginPage_Resize;
        }

        private void CustomizeUI()
        {
            // Form Background
            this.BackColor = Color.FromArgb(242, 244, 248); // Light Gray
            this.FormBorderStyle = FormBorderStyle.Sizable; // User requested Sizable

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
            AuthService authService = new AuthService(new CourseGuard.Infrastructure.Data.Repositories.UserRepository());
            UserModel? user = authService.Login(username, password);
            if (user == null)
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!");
                return;
            }

            if (user.Status != "ACTIVE")
            {
                MessageBox.Show("Tài khoản của bạn không hoạt động. Vui lòng liên hệ quản trị viên.");
                return;
            }

            // Login Success
            CurrentUser = user;

            try
            {
                // Update DEVICES table
                string ipAddress = GetLocalIPAddress();
                string deviceName = user.Username; // As requested

                // Check if device entry exists for this user and IP, or just insert new session?
                // Request says: "update table Devices trong đó @DEVICE_NAME là username ... tự động lấy IP_ADDRESS để update"
                // It implies we should Insert a new record or Update the latest one. 
                // Let's Insert/Update based on UserID. 
                // However, if a user logs in from multiple devices, we might want unexpected behavior if we just update.
                // But typically for "tracking last login", we often just Insert or Update a "Current Session".
                // Let's upsert: If UserID exists, update? No, a user can have multiple devices. 
                // Let's Insert a new record for every login to track history? 
                // Or Update the record where DEVICE_NAME = username?
                // The prompt says "update table Devices".
                // Let's try to UPDATE if exists (by UserID and maybe IP?), ELSE INSERT.
                // Given the fields: ID, USER_ID, DEVICE_NAME, IP_ADDRESS, STATUS, LAST_ACTIVE
                
                // Strategy: Update the record for this UserID where DEVICE_NAME is the username (if we treat username as device name as requested).
                // If not found, insert.
                
                string queryCheck = "SELECT COUNT(*) FROM DEVICES WHERE USER_ID = @uid AND DEVICE_NAME = @dname";
                var paramsCheck = new System.Collections.Generic.Dictionary<string, (System.Data.SqlDbType, object)>
                {
                    { "@uid", (System.Data.SqlDbType.Int, user.Id) },
                    { "@dname", (System.Data.SqlDbType.NVarChar, deviceName) }
                };
                
                int count = (int)CourseGuard.Infrastructure.Data.DatabaseAction.ExecuteScalar(queryCheck, paramsCheck);

                if (count > 0)
                {
                    // Update
                    string queryUpdate = @"
                        UPDATE DEVICES 
                        SET IP_ADDRESS = @ip, LAST_ACTIVE = GETDATE(), STATUS = 'ACTIVE'
                        WHERE USER_ID = @uid AND DEVICE_NAME = @dname";
                    
                    var paramsUpdate = new System.Collections.Generic.Dictionary<string, (System.Data.SqlDbType, object)>
                    {
                        { "@ip", (System.Data.SqlDbType.NVarChar, ipAddress) },
                        { "@uid", (System.Data.SqlDbType.Int, user.Id) },
                        { "@dname", (System.Data.SqlDbType.NVarChar, deviceName) }
                    };
                    CourseGuard.Infrastructure.Data.DatabaseAction.ExecuteNonQuery(queryUpdate, paramsUpdate);
                }
                else
                {
                    // Insert
                    string queryInsert = @"
                        INSERT INTO DEVICES (USER_ID, DEVICE_NAME, IP_ADDRESS, STATUS, LAST_ACTIVE)
                        VALUES (@uid, @dname, @ip, 'ACTIVE', GETDATE())";
                    
                    var paramsInsert = new System.Collections.Generic.Dictionary<string, (System.Data.SqlDbType, object)>
                    {
                        { "@uid", (System.Data.SqlDbType.Int, user.Id) },
                        { "@dname", (System.Data.SqlDbType.NVarChar, deviceName) },
                        { "@ip", (System.Data.SqlDbType.NVarChar, ipAddress) }
                    };
                    CourseGuard.Infrastructure.Data.DatabaseAction.ExecuteNonQuery(queryInsert, paramsInsert);
                }
            }
            catch (Exception ex)
            {
                // Log error but allow login? Or show message? 
                // Let's show friendly error but proceed or just silent?
                // Usually silent for logging features, but valid for debugging.
                MessageBox.Show("Cảnh báo: Không thể cập nhật thông tin thiết bị. " + ex.Message);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "127.0.0.1";
            }
            catch
            {
                return "Unknown";
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
