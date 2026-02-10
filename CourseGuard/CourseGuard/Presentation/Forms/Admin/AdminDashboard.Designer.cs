namespace CourseGuard.Presentation.Forms.Admin
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
            Sidebar = new Panel();
            btn_Reports = new Button();
            btn_Courses = new Button();
            btn_users = new Button();
            btn_Dashboard = new Button();
            LOGO = new Label();
            Header = new Panel();
            Mainboard = new Panel();
            Sidebar.SuspendLayout();
            SuspendLayout();
            // 
            // Sidebar
            // 
            Sidebar.BackColor = Color.FromArgb(17, 24, 39);
            Sidebar.Controls.Add(btn_Reports);
            Sidebar.Controls.Add(btn_Courses);
            Sidebar.Controls.Add(btn_users);
            Sidebar.Controls.Add(btn_Dashboard);
            Sidebar.Controls.Add(LOGO);
            Sidebar.Dock = DockStyle.Left;
            Sidebar.Location = new Point(0, 0);
            Sidebar.Name = "Sidebar";
            Sidebar.Size = new Size(250, 450);
            Sidebar.TabIndex = 0;
            // 
            // btn_Reports
            // 
            btn_Reports.BackColor = Color.Transparent;
            btn_Reports.FlatAppearance.BorderSize = 0;
            btn_Reports.FlatStyle = FlatStyle.Flat;
            btn_Reports.Font = new Font("Segoe UI", 11F);
            btn_Reports.ForeColor = Color.FromArgb(156, 163, 175);
            btn_Reports.Location = new Point(12, 245);
            btn_Reports.Name = "btn_Reports";
            btn_Reports.Padding = new Padding(10, 0, 0, 0);
            btn_Reports.Size = new Size(226, 45);
            btn_Reports.TabIndex = 6;
            btn_Reports.Text = "📊 Reports";
            btn_Reports.TextAlign = ContentAlignment.MiddleLeft;
            btn_Reports.UseVisualStyleBackColor = true;
            btn_Reports.Click += Sidebar_Btn_Click;
            // 
            // btn_Courses
            // 
            btn_Courses.BackColor = Color.Transparent;
            btn_Courses.FlatAppearance.BorderSize = 0;
            btn_Courses.FlatStyle = FlatStyle.Flat;
            btn_Courses.Font = new Font("Segoe UI", 11F);
            btn_Courses.ForeColor = Color.FromArgb(156, 163, 175);
            btn_Courses.Location = new Point(12, 190);
            btn_Courses.Name = "btn_Courses";
            btn_Courses.Padding = new Padding(10, 0, 0, 0);
            btn_Courses.Size = new Size(226, 45);
            btn_Courses.TabIndex = 5;
            btn_Courses.Text = "📚 Courses Manage";
            btn_Courses.TextAlign = ContentAlignment.MiddleLeft;
            btn_Courses.UseVisualStyleBackColor = true;
            btn_Courses.Click += Sidebar_Btn_Click;
            // 
            // btn_users
            // 
            btn_users.BackColor = Color.Transparent;
            btn_users.FlatAppearance.BorderSize = 0;
            btn_users.FlatStyle = FlatStyle.Flat;
            btn_users.Font = new Font("Segoe UI", 11F);
            btn_users.ForeColor = Color.FromArgb(156, 163, 175);
            btn_users.Location = new Point(12, 135);
            btn_users.Name = "btn_users";
            btn_users.Padding = new Padding(10, 0, 0, 0);
            btn_users.Size = new Size(226, 45);
            btn_users.TabIndex = 4;
            btn_users.Text = "👥 Users Manage";
            btn_users.TextAlign = ContentAlignment.MiddleLeft;
            btn_users.UseVisualStyleBackColor = true;
            btn_users.Click += Sidebar_Btn_Click;
            // 
            // btn_Dashboard
            // 
            btn_Dashboard.BackColor = Color.FromArgb(37, 99, 235);
            btn_Dashboard.FlatAppearance.BorderSize = 0;
            btn_Dashboard.FlatStyle = FlatStyle.Flat;
            btn_Dashboard.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btn_Dashboard.ForeColor = Color.White;
            btn_Dashboard.Location = new Point(12, 80);
            btn_Dashboard.Name = "btn_Dashboard";
            btn_Dashboard.Padding = new Padding(10, 0, 0, 0);
            btn_Dashboard.Size = new Size(226, 45);
            btn_Dashboard.TabIndex = 3;
            btn_Dashboard.Text = "🏠 Dashboard";
            btn_Dashboard.TextAlign = ContentAlignment.MiddleLeft;
            btn_Dashboard.UseVisualStyleBackColor = false;
            btn_Dashboard.Click += Sidebar_Btn_Click;
            // 
            // LOGO
            // 
            LOGO.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            LOGO.ForeColor = Color.White;
            LOGO.Location = new Point(0, 20);
            LOGO.Margin = new Padding(4, 0, 4, 0);
            LOGO.Name = "LOGO";
            LOGO.Size = new Size(250, 40);
            LOGO.TabIndex = 2;
            LOGO.Text = "COURSE GUARD";
            LOGO.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Header
            // 
            Header.BackColor = Color.White;
            Header.Dock = DockStyle.Top;
            Header.Location = new Point(250, 0);
            Header.Name = "Header";
            Header.Size = new Size(550, 60);
            Header.TabIndex = 1;
            // 
            // Mainboard
            // 
            Mainboard.BackColor = Color.FromArgb(242, 244, 248);
            Mainboard.Dock = DockStyle.Fill;
            Mainboard.Location = new Point(250, 60);
            Mainboard.Name = "Mainboard";
            Mainboard.Size = new Size(550, 390);
            Mainboard.TabIndex = 2;
            Mainboard.Paint += Mainboard_Paint;
            // 
            // AdminDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(Mainboard);
            Controls.Add(Header);
            Controls.Add(Sidebar);
            Name = "AdminDashboard";
            Text = "AdminDashboard";
            Sidebar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel Sidebar;
        private Panel Header;
        private Panel Mainboard;
        private Label LOGO;
        private Button btn_Reports;
        private Button btn_Courses;
        private Button btn_users;
        private Button btn_Dashboard;
    }
}