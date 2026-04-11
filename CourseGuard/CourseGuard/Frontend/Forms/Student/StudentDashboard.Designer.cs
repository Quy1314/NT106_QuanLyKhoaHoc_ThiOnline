namespace CourseGuard.Frontend.Forms.Student
{
    partial class StudentDashboard
    {
        private System.ComponentModel.IContainer components = null;

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
            this.sidebar = new System.Windows.Forms.Panel();
            this.mainboard = new System.Windows.Forms.Panel();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.btnCourses = new System.Windows.Forms.Button();
            this.btnExam = new System.Windows.Forms.Button();
            this.btnResult = new System.Windows.Forms.Button();
            this.btnSchedule = new System.Windows.Forms.Button();
            this.btnChat = new System.Windows.Forms.Button();
            this.btnNotify = new System.Windows.Forms.Button();
            this.btnProfile = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.sidebar.SuspendLayout();
            this.SuspendLayout();
            // 
            // sidebar
            // 
            this.sidebar.BackColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.Secondary;
            this.sidebar.Controls.Add(this.btnLogout);
            this.sidebar.Controls.Add(this.btnDashboard);
            this.sidebar.Controls.Add(this.btnCourses);
            this.sidebar.Controls.Add(this.btnExam);
            this.sidebar.Controls.Add(this.btnResult);
            this.sidebar.Controls.Add(this.btnSchedule);
            this.sidebar.Controls.Add(this.btnChat);
            this.sidebar.Controls.Add(this.btnNotify);
            this.sidebar.Controls.Add(this.btnProfile);
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
            // btnCourses
            // 
            this.btnCourses.BackColor = System.Drawing.Color.Transparent;
            this.btnCourses.FlatAppearance.BorderSize = 0;
            this.btnCourses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCourses.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnCourses.Location = new System.Drawing.Point(0, 50);
            this.btnCourses.Name = "btnCourses";
            this.btnCourses.Size = new System.Drawing.Size(200, 50);
            this.btnCourses.TabIndex = 1;
            this.btnCourses.Text = "Courses";
            this.btnCourses.UseVisualStyleBackColor = false;
            // 
            // btnExam
            // 
            this.btnExam.BackColor = System.Drawing.Color.Transparent;
            this.btnExam.FlatAppearance.BorderSize = 0;
            this.btnExam.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExam.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnExam.Location = new System.Drawing.Point(0, 100);
            this.btnExam.Name = "btnExam";
            this.btnExam.Size = new System.Drawing.Size(200, 50);
            this.btnExam.TabIndex = 2;
            this.btnExam.Text = "Take Exam";
            this.btnExam.UseVisualStyleBackColor = false;
            // 
            // btnResult
            // 
            this.btnResult.BackColor = System.Drawing.Color.Transparent;
            this.btnResult.FlatAppearance.BorderSize = 0;
            this.btnResult.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResult.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnResult.Location = new System.Drawing.Point(0, 150);
            this.btnResult.Name = "btnResult";
            this.btnResult.Size = new System.Drawing.Size(200, 50);
            this.btnResult.TabIndex = 3;
            this.btnResult.Text = "Result";
            this.btnResult.UseVisualStyleBackColor = false;
            // 
            // btnSchedule
            // 
            this.btnSchedule.BackColor = System.Drawing.Color.Transparent;
            this.btnSchedule.FlatAppearance.BorderSize = 0;
            this.btnSchedule.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSchedule.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnSchedule.Location = new System.Drawing.Point(0, 200);
            this.btnSchedule.Name = "btnSchedule";
            this.btnSchedule.Size = new System.Drawing.Size(200, 50);
            this.btnSchedule.TabIndex = 4;
            this.btnSchedule.Text = "Schedule";
            this.btnSchedule.UseVisualStyleBackColor = false;
            // 
            // btnChat
            // 
            this.btnChat.BackColor = System.Drawing.Color.Transparent;
            this.btnChat.FlatAppearance.BorderSize = 0;
            this.btnChat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChat.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnChat.Location = new System.Drawing.Point(0, 250);
            this.btnChat.Name = "btnChat";
            this.btnChat.Size = new System.Drawing.Size(200, 50);
            this.btnChat.TabIndex = 5;
            this.btnChat.Text = "Chat";
            this.btnChat.UseVisualStyleBackColor = false;
            // 
            // btnNotify
            // 
            this.btnNotify.BackColor = System.Drawing.Color.Transparent;
            this.btnNotify.FlatAppearance.BorderSize = 0;
            this.btnNotify.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNotify.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnNotify.Location = new System.Drawing.Point(0, 300);
            this.btnNotify.Name = "btnNotify";
            this.btnNotify.Size = new System.Drawing.Size(200, 50);
            this.btnNotify.TabIndex = 6;
            this.btnNotify.Text = "Notification";
            this.btnNotify.UseVisualStyleBackColor = false;
            // 
            // btnProfile
            // 
            this.btnProfile.BackColor = System.Drawing.Color.Transparent;
            this.btnProfile.FlatAppearance.BorderSize = 0;
            this.btnProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProfile.ForeColor = CourseGuard.Frontend.Theme.ColorPalette.DarkMode.TextPrimary;
            this.btnProfile.Location = new System.Drawing.Point(0, 350);
            this.btnProfile.Name = "btnProfile";
            this.btnProfile.Size = new System.Drawing.Size(200, 50);
            this.btnProfile.TabIndex = 7;
            this.btnProfile.Text = "Profile";
            this.btnProfile.UseVisualStyleBackColor = false;
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
            this.btnLogout.TabIndex = 8;
            this.btnLogout.Text = "Logout";
            this.btnLogout.UseVisualStyleBackColor = false;
            // 
            // StudentDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.mainboard);
            this.Controls.Add(this.sidebar);
            this.Name = "StudentDashboard";
            this.Text = "Student Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.sidebar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel sidebar;
        private System.Windows.Forms.Panel mainboard;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Button btnCourses;
        private System.Windows.Forms.Button btnExam;
        private System.Windows.Forms.Button btnResult;
        private System.Windows.Forms.Button btnSchedule;
        private System.Windows.Forms.Button btnChat;
        private System.Windows.Forms.Button btnNotify;
        private System.Windows.Forms.Button btnProfile;
        private System.Windows.Forms.Button btnLogout;
    }
}
