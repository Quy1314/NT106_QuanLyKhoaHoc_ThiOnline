namespace CourseGuard.Frontend.Forms.Admin
{
    partial class AdminDashboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.sidebar = new System.Windows.Forms.Panel();
            this.mainboard = new System.Windows.Forms.Panel();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.btnUsers = new System.Windows.Forms.Button();
            this.btnCourses = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnDeviceMonitoring = new System.Windows.Forms.Button();
            this.btnAuditLogs = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.sidebar.SuspendLayout();
            this.SuspendLayout();
            // 
            // sidebar
            // 
            this.sidebar.BackColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.Secondary;
            this.sidebar.Controls.Add(this.btnLogout);
            this.sidebar.Controls.Add(this.btnDashboard);
            this.sidebar.Controls.Add(this.btnUsers);
            this.sidebar.Controls.Add(this.btnCourses);
            this.sidebar.Controls.Add(this.btnReports);
            this.sidebar.Controls.Add(this.btnDeviceMonitoring);
            this.sidebar.Controls.Add(this.btnAuditLogs);
            this.sidebar.Controls.Add(this.btnSettings);
            this.sidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebar.Location = new System.Drawing.Point(0, 0);
            this.sidebar.Name = "sidebar";
            this.sidebar.Size = new System.Drawing.Size(200, 600);
            this.sidebar.TabIndex = 0;
            // 
            // mainboard
            // 
            this.mainboard.BackColor = CourseGuard.Frontend.Theme.ColorPalette.LightMode.Base;
            this.mainboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainboard.Location = new System.Drawing.Point(200, 0);
            this.mainboard.Name = "mainboard";
            this.mainboard.Size = new System.Drawing.Size(800, 600);
            this.mainboard.TabIndex = 1;
            // 
            // btnDashboard
            // 
            this.btnDashboard.BackColor = System.Drawing.Color.Transparent;
            this.btnDashboard.FlatAppearance.BorderSize = 0;
            this.btnDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashboard.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnDashboard.Location = new System.Drawing.Point(0, 0);
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.Size = new System.Drawing.Size(200, 50);
            this.btnDashboard.TabIndex = 0;
            this.btnDashboard.Text = "Dashboard";
            this.btnDashboard.UseVisualStyleBackColor = false;
            // 
            // btnUsers
            // 
            this.btnUsers.BackColor = System.Drawing.Color.Transparent;
            this.btnUsers.FlatAppearance.BorderSize = 0;
            this.btnUsers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUsers.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnUsers.Location = new System.Drawing.Point(0, 50);
            this.btnUsers.Name = "btnUsers";
            this.btnUsers.Size = new System.Drawing.Size(200, 50);
            this.btnUsers.TabIndex = 1;
            this.btnUsers.Text = "User Management";
            this.btnUsers.UseVisualStyleBackColor = false;
            // 
            // btnCourses
            // 
            this.btnCourses.BackColor = System.Drawing.Color.Transparent;
            this.btnCourses.FlatAppearance.BorderSize = 0;
            this.btnCourses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCourses.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnCourses.Location = new System.Drawing.Point(0, 100);
            this.btnCourses.Name = "btnCourses";
            this.btnCourses.Size = new System.Drawing.Size(200, 50);
            this.btnCourses.TabIndex = 2;
            this.btnCourses.Text = "Course Management";
            this.btnCourses.UseVisualStyleBackColor = false;
            // 
            // btnReports
            // 
            this.btnReports.BackColor = System.Drawing.Color.Transparent;
            this.btnReports.FlatAppearance.BorderSize = 0;
            this.btnReports.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReports.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnReports.Location = new System.Drawing.Point(0, 150);
            this.btnReports.Name = "btnReports";
            this.btnReports.Size = new System.Drawing.Size(200, 50);
            this.btnReports.TabIndex = 3;
            this.btnReports.Text = "Reports";
            this.btnReports.UseVisualStyleBackColor = false;
            // 
            // btnDeviceMonitoring
            // 
            this.btnDeviceMonitoring.BackColor = System.Drawing.Color.Transparent;
            this.btnDeviceMonitoring.FlatAppearance.BorderSize = 0;
            this.btnDeviceMonitoring.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeviceMonitoring.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnDeviceMonitoring.Location = new System.Drawing.Point(0, 200);
            this.btnDeviceMonitoring.Name = "btnDeviceMonitoring";
            this.btnDeviceMonitoring.Size = new System.Drawing.Size(200, 50);
            this.btnDeviceMonitoring.TabIndex = 4;
            this.btnDeviceMonitoring.Text = "Device Monitoring";
            this.btnDeviceMonitoring.UseVisualStyleBackColor = false;
            // 
            // btnAuditLogs
            // 
            this.btnAuditLogs.BackColor = System.Drawing.Color.Transparent;
            this.btnAuditLogs.FlatAppearance.BorderSize = 0;
            this.btnAuditLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAuditLogs.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnAuditLogs.Location = new System.Drawing.Point(0, 250);
            this.btnAuditLogs.Name = "btnAuditLogs";
            this.btnAuditLogs.Size = new System.Drawing.Size(200, 50);
            this.btnAuditLogs.TabIndex = 5;
            this.btnAuditLogs.Text = "Audit Logs";
            this.btnAuditLogs.UseVisualStyleBackColor = false;
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.Transparent;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnSettings.Location = new System.Drawing.Point(0, 300);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(200, 50);
            this.btnSettings.TabIndex = 6;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = false;
            // 
            // btnLogout
            // 
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnLogout.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.ForeColor = System.Drawing.Color.White;
            this.btnLogout.Location = new System.Drawing.Point(0, 550);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(200, 50);
            this.btnLogout.TabIndex = 7;
            this.btnLogout.Text = "Logout";
            this.btnLogout.UseVisualStyleBackColor = false;
            // 
            // AdminDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.mainboard);
            this.Controls.Add(this.sidebar);
            this.Name = "AdminDashboard";
            this.Text = "Admin Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.sidebar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel sidebar;
        private System.Windows.Forms.Panel mainboard;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Button btnUsers;
        private System.Windows.Forms.Button btnCourses;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button btnDeviceMonitoring;
        private System.Windows.Forms.Button btnAuditLogs;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnLogout;
    }
}