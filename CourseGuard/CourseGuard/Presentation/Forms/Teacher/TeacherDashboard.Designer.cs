namespace CourseGuard.Presentation.Forms.Teacher
{
    partial class TeacherDashboard
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
            this.Sidebar = new System.Windows.Forms.Panel();
            this.btn_Notifications = new System.Windows.Forms.Button();
            this.btn_Chat = new System.Windows.Forms.Button();
            this.btn_OnlineClass = new System.Windows.Forms.Button();
            this.btn_Grades = new System.Windows.Forms.Button();
            this.btn_EssayGrading = new System.Windows.Forms.Button();
            this.btn_ExamMonitor = new System.Windows.Forms.Button();
            this.btn_QuestionBank = new System.Windows.Forms.Button();
            this.btn_ExamConfig = new System.Windows.Forms.Button();
            this.btn_ManageExams = new System.Windows.Forms.Button();
            this.btn_TeacherCourses = new System.Windows.Forms.Button();
            this.btn_Dashboard = new System.Windows.Forms.Button();
            this.LOGO = new System.Windows.Forms.Label();
            this.Header = new System.Windows.Forms.Panel();
            this.lbl_Title = new System.Windows.Forms.Label();
            this.Mainboard = new System.Windows.Forms.Panel();
            this.Sidebar.SuspendLayout();
            this.Header.SuspendLayout();
            this.SuspendLayout();
            // 
            // Sidebar
            // 
            this.Sidebar.Controls.Add(this.btn_Notifications);
            this.Sidebar.Controls.Add(this.btn_Chat);
            this.Sidebar.Controls.Add(this.btn_OnlineClass);
            this.Sidebar.Controls.Add(this.btn_Grades);
            this.Sidebar.Controls.Add(this.btn_EssayGrading);
            this.Sidebar.Controls.Add(this.btn_ExamMonitor);
            this.Sidebar.Controls.Add(this.btn_QuestionBank);
            this.Sidebar.Controls.Add(this.btn_ExamConfig);
            this.Sidebar.Controls.Add(this.btn_ManageExams);
            this.Sidebar.Controls.Add(this.btn_TeacherCourses);
            this.Sidebar.Controls.Add(this.btn_Dashboard);
            this.Sidebar.Controls.Add(this.LOGO);
            this.Sidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.Sidebar.Location = new System.Drawing.Point(0, 0);
            this.Sidebar.Name = "Sidebar";
            this.Sidebar.Size = new System.Drawing.Size(250, 750);
            this.Sidebar.TabIndex = 0;
            // 
            // btn_Notifications
            // 
            this.btn_Notifications.FlatAppearance.BorderSize = 0;
            this.btn_Notifications.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Notifications.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_Notifications.Location = new System.Drawing.Point(0, 580);
            this.btn_Notifications.Name = "btn_Notifications";
            this.btn_Notifications.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_Notifications.Size = new System.Drawing.Size(250, 45);
            this.btn_Notifications.TabIndex = 11;
            this.btn_Notifications.Text = "🔔 Thông báo";
            this.btn_Notifications.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_Notifications.UseVisualStyleBackColor = true;
            this.btn_Notifications.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_Chat
            // 
            this.btn_Chat.FlatAppearance.BorderSize = 0;
            this.btn_Chat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Chat.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_Chat.Location = new System.Drawing.Point(0, 530);
            this.btn_Chat.Name = "btn_Chat";
            this.btn_Chat.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_Chat.Size = new System.Drawing.Size(250, 45);
            this.btn_Chat.TabIndex = 10;
            this.btn_Chat.Text = "💬 Chat";
            this.btn_Chat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_Chat.UseVisualStyleBackColor = true;
            this.btn_Chat.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_OnlineClass
            // 
            this.btn_OnlineClass.FlatAppearance.BorderSize = 0;
            this.btn_OnlineClass.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_OnlineClass.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_OnlineClass.Location = new System.Drawing.Point(0, 480);
            this.btn_OnlineClass.Name = "btn_OnlineClass";
            this.btn_OnlineClass.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_OnlineClass.Size = new System.Drawing.Size(250, 45);
            this.btn_OnlineClass.TabIndex = 9;
            this.btn_OnlineClass.Text = "💻 Lớp Online";
            this.btn_OnlineClass.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_OnlineClass.UseVisualStyleBackColor = true;
            this.btn_OnlineClass.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_Grades
            // 
            this.btn_Grades.FlatAppearance.BorderSize = 0;
            this.btn_Grades.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Grades.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_Grades.Location = new System.Drawing.Point(0, 430);
            this.btn_Grades.Name = "btn_Grades";
            this.btn_Grades.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_Grades.Size = new System.Drawing.Size(250, 45);
            this.btn_Grades.TabIndex = 8;
            this.btn_Grades.Text = "📊 Điểm số";
            this.btn_Grades.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_Grades.UseVisualStyleBackColor = true;
            this.btn_Grades.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_EssayGrading
            // 
            this.btn_EssayGrading.FlatAppearance.BorderSize = 0;
            this.btn_EssayGrading.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_EssayGrading.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_EssayGrading.Location = new System.Drawing.Point(0, 380);
            this.btn_EssayGrading.Name = "btn_EssayGrading";
            this.btn_EssayGrading.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_EssayGrading.Size = new System.Drawing.Size(250, 45);
            this.btn_EssayGrading.TabIndex = 7;
            this.btn_EssayGrading.Text = "✍️ Chấm thi";
            this.btn_EssayGrading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_EssayGrading.UseVisualStyleBackColor = true;
            this.btn_EssayGrading.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_ExamMonitor
            // 
            this.btn_ExamMonitor.FlatAppearance.BorderSize = 0;
            this.btn_ExamMonitor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_ExamMonitor.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_ExamMonitor.Location = new System.Drawing.Point(0, 330);
            this.btn_ExamMonitor.Name = "btn_ExamMonitor";
            this.btn_ExamMonitor.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_ExamMonitor.Size = new System.Drawing.Size(250, 45);
            this.btn_ExamMonitor.TabIndex = 6;
            this.btn_ExamMonitor.Text = "👁️ Giám sát";
            this.btn_ExamMonitor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_ExamMonitor.UseVisualStyleBackColor = true;
            this.btn_ExamMonitor.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_QuestionBank
            // 
            this.btn_QuestionBank.FlatAppearance.BorderSize = 0;
            this.btn_QuestionBank.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_QuestionBank.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_QuestionBank.Location = new System.Drawing.Point(0, 280);
            this.btn_QuestionBank.Name = "btn_QuestionBank";
            this.btn_QuestionBank.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_QuestionBank.Size = new System.Drawing.Size(250, 45);
            this.btn_QuestionBank.TabIndex = 5;
            this.btn_QuestionBank.Text = "🗄️ Ngân hàng CH";
            this.btn_QuestionBank.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_QuestionBank.UseVisualStyleBackColor = true;
            this.btn_QuestionBank.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_ExamConfig
            // 
            this.btn_ExamConfig.FlatAppearance.BorderSize = 0;
            this.btn_ExamConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_ExamConfig.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_ExamConfig.Location = new System.Drawing.Point(0, 230);
            this.btn_ExamConfig.Name = "btn_ExamConfig";
            this.btn_ExamConfig.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_ExamConfig.Size = new System.Drawing.Size(250, 45);
            this.btn_ExamConfig.TabIndex = 4;
            this.btn_ExamConfig.Text = "⚙️ Cấu hình đề";
            this.btn_ExamConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_ExamConfig.UseVisualStyleBackColor = true;
            this.btn_ExamConfig.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_ManageExams
            // 
            this.btn_ManageExams.FlatAppearance.BorderSize = 0;
            this.btn_ManageExams.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_ManageExams.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_ManageExams.Location = new System.Drawing.Point(0, 180);
            this.btn_ManageExams.Name = "btn_ManageExams";
            this.btn_ManageExams.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_ManageExams.Size = new System.Drawing.Size(250, 45);
            this.btn_ManageExams.TabIndex = 3;
            this.btn_ManageExams.Text = "📝 Kỳ thi";
            this.btn_ManageExams.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_ManageExams.UseVisualStyleBackColor = true;
            this.btn_ManageExams.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_TeacherCourses
            // 
            this.btn_TeacherCourses.FlatAppearance.BorderSize = 0;
            this.btn_TeacherCourses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_TeacherCourses.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btn_TeacherCourses.Location = new System.Drawing.Point(0, 130);
            this.btn_TeacherCourses.Name = "btn_TeacherCourses";
            this.btn_TeacherCourses.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_TeacherCourses.Size = new System.Drawing.Size(250, 45);
            this.btn_TeacherCourses.TabIndex = 2;
            this.btn_TeacherCourses.Text = "📚 Khóa học";
            this.btn_TeacherCourses.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_TeacherCourses.UseVisualStyleBackColor = true;
            this.btn_TeacherCourses.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // btn_Dashboard
            // 
            this.btn_Dashboard.FlatAppearance.BorderSize = 0;
            this.btn_Dashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Dashboard.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btn_Dashboard.Location = new System.Drawing.Point(0, 80);
            this.btn_Dashboard.Name = "btn_Dashboard";
            this.btn_Dashboard.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_Dashboard.Size = new System.Drawing.Size(250, 45);
            this.btn_Dashboard.TabIndex = 1;
            this.btn_Dashboard.Text = "🏠 Dashboard";
            this.btn_Dashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_Dashboard.UseVisualStyleBackColor = true;
            this.btn_Dashboard.Click += new System.EventHandler(this.Sidebar_Btn_Click);
            // 
            // LOGO
            // 
            this.LOGO.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.LOGO.Location = new System.Drawing.Point(0, 20);
            this.LOGO.Name = "LOGO";
            this.LOGO.Size = new System.Drawing.Size(250, 40);
            this.LOGO.TabIndex = 0;
            this.LOGO.Text = "COURSE GUARD";
            this.LOGO.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Header
            // 
            this.Header.Controls.Add(this.lbl_Title);
            this.Header.Dock = System.Windows.Forms.DockStyle.Top;
            this.Header.Location = new System.Drawing.Point(250, 0);
            this.Header.Name = "Header";
            this.Header.Size = new System.Drawing.Size(950, 60);
            this.Header.TabIndex = 1;
            // 
            // lbl_Title
            // 
            this.lbl_Title.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lbl_Title.Location = new System.Drawing.Point(30, 15);
            this.lbl_Title.Name = "lbl_Title";
            this.lbl_Title.Size = new System.Drawing.Size(400, 30);
            this.lbl_Title.TabIndex = 0;
            this.lbl_Title.Text = "Dashboard";
            this.lbl_Title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Mainboard
            // 
            this.Mainboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Mainboard.Location = new System.Drawing.Point(250, 60);
            this.Mainboard.Name = "Mainboard";
            this.Mainboard.Size = new System.Drawing.Size(950, 690);
            this.Mainboard.TabIndex = 2;
            // 
            // TeacherDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 750);
            this.Controls.Add(this.Mainboard);
            this.Controls.Add(this.Header);
            this.Controls.Add(this.Sidebar);
            this.Name = "TeacherDashboard";
            this.Text = "Teacher Dashboard";
            this.Sidebar.ResumeLayout(false);
            this.Header.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel Sidebar;
        private System.Windows.Forms.Panel Header;
        private System.Windows.Forms.Panel Mainboard;
        private System.Windows.Forms.Label LOGO;
        private System.Windows.Forms.Button btn_Dashboard;
        private System.Windows.Forms.Button btn_Notifications;
        private System.Windows.Forms.Button btn_Chat;
        private System.Windows.Forms.Button btn_OnlineClass;
        private System.Windows.Forms.Button btn_Grades;
        private System.Windows.Forms.Button btn_EssayGrading;
        private System.Windows.Forms.Button btn_ExamMonitor;
        private System.Windows.Forms.Button btn_QuestionBank;
        private System.Windows.Forms.Button btn_ExamConfig;
        private System.Windows.Forms.Button btn_ManageExams;
        private System.Windows.Forms.Button btn_TeacherCourses;
        private System.Windows.Forms.Label lbl_Title;
    }
}

