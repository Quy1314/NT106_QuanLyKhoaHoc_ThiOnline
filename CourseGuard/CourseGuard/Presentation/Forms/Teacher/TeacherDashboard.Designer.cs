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
            pnlSidebar = new Panel();
            btnLogout = new Button();
            btnNotifications = new Button();
            pnlSubMenuMonitoring = new Panel();
            btnScoreManagement = new Button();
            btnEssayGrading = new Button();
            btnLiveMonitor = new Button();
            btnGroupMonitoring = new Button();
            pnlSubMenuTesting = new Panel();
            btnExamList = new Button();
            btnExamConfig = new Button();
            btnQuestionBank = new Button();
            btnGroupTesting = new Button();
            pnlSubMenuCourseDocs = new Panel();
            btnOnlineClasses = new Button();
            btnAssignedCourses = new Button();
            btnGroupCourseDocs = new Button();
            btnOverview = new Button();
            pnlLogo = new Panel();
            lblLogo = new Label();
            pnlMainboard = new Panel();
            pnlSidebar.SuspendLayout();
            pnlSubMenuMonitoring.SuspendLayout();
            pnlSubMenuTesting.SuspendLayout();
            pnlSubMenuCourseDocs.SuspendLayout();
            pnlLogo.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(17, 24, 39);
            pnlSidebar.Controls.Add(btnLogout);
            pnlSidebar.Controls.Add(btnNotifications);
            pnlSidebar.Controls.Add(pnlSubMenuMonitoring);
            pnlSidebar.Controls.Add(btnGroupMonitoring);
            pnlSidebar.Controls.Add(pnlSubMenuTesting);
            pnlSidebar.Controls.Add(btnGroupTesting);
            pnlSidebar.Controls.Add(pnlSubMenuCourseDocs);
            pnlSidebar.Controls.Add(btnGroupCourseDocs);
            pnlSidebar.Controls.Add(btnOverview);
            pnlSidebar.Controls.Add(pnlLogo);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Margin = new Padding(3, 4, 3, 4);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(297, 960);
            pnlSidebar.TabIndex = 0;
            // 
            // btnLogout
            // 
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 11F);
            btnLogout.ForeColor = Color.White;
            btnLogout.Location = new Point(0, 893);
            btnLogout.Margin = new Padding(3, 4, 3, 4);
            btnLogout.Name = "btnLogout";
            btnLogout.Padding = new Padding(17, 0, 0, 0);
            btnLogout.Size = new Size(297, 67);
            btnLogout.TabIndex = 9;
            btnLogout.Text = "🚪 Đăng Xuất";
            btnLogout.TextAlign = ContentAlignment.MiddleLeft;
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // btnNotifications
            // 
            btnNotifications.Cursor = Cursors.Hand;
            btnNotifications.Dock = DockStyle.Top;
            btnNotifications.FlatAppearance.BorderSize = 0;
            btnNotifications.FlatStyle = FlatStyle.Flat;
            btnNotifications.Font = new Font("Segoe UI", 11F);
            btnNotifications.ForeColor = Color.White;
            btnNotifications.Location = new Point(0, 752);
            btnNotifications.Margin = new Padding(3, 4, 3, 4);
            btnNotifications.Name = "btnNotifications";
            btnNotifications.Padding = new Padding(17, 0, 0, 0);
            btnNotifications.Size = new Size(297, 67);
            btnNotifications.TabIndex = 8;
            btnNotifications.Text = "🔔 Trung Tâm Thông Báo";
            btnNotifications.TextAlign = ContentAlignment.MiddleLeft;
            btnNotifications.UseVisualStyleBackColor = true;
            btnNotifications.Click += btnNotifications_Click;
            // 
            // pnlSubMenuMonitoring
            // 
            pnlSubMenuMonitoring.AutoSize = true;
            pnlSubMenuMonitoring.BackColor = Color.FromArgb(31, 41, 55);
            pnlSubMenuMonitoring.Controls.Add(btnScoreManagement);
            pnlSubMenuMonitoring.Controls.Add(btnEssayGrading);
            pnlSubMenuMonitoring.Controls.Add(btnLiveMonitor);
            pnlSubMenuMonitoring.Dock = DockStyle.Top;
            pnlSubMenuMonitoring.Location = new Point(0, 593);
            pnlSubMenuMonitoring.Margin = new Padding(3, 4, 3, 4);
            pnlSubMenuMonitoring.Name = "pnlSubMenuMonitoring";
            pnlSubMenuMonitoring.Size = new Size(297, 159);
            pnlSubMenuMonitoring.TabIndex = 7;
            pnlSubMenuMonitoring.Visible = false;
            // 
            // btnScoreManagement
            // 
            btnScoreManagement.Cursor = Cursors.Hand;
            btnScoreManagement.Dock = DockStyle.Top;
            btnScoreManagement.FlatAppearance.BorderSize = 0;
            btnScoreManagement.FlatStyle = FlatStyle.Flat;
            btnScoreManagement.Font = new Font("Segoe UI", 10F);
            btnScoreManagement.ForeColor = Color.LightCyan;
            btnScoreManagement.Location = new Point(0, 106);
            btnScoreManagement.Margin = new Padding(3, 4, 3, 4);
            btnScoreManagement.Name = "btnScoreManagement";
            btnScoreManagement.Padding = new Padding(46, 0, 0, 0);
            btnScoreManagement.Size = new Size(297, 53);
            btnScoreManagement.TabIndex = 2;
            btnScoreManagement.Text = "📊 Quản lý điểm số";
            btnScoreManagement.TextAlign = ContentAlignment.MiddleLeft;
            btnScoreManagement.UseVisualStyleBackColor = true;
            btnScoreManagement.Click += btnScoreManagement_Click;
            // 
            // btnEssayGrading
            // 
            btnEssayGrading.Cursor = Cursors.Hand;
            btnEssayGrading.Dock = DockStyle.Top;
            btnEssayGrading.FlatAppearance.BorderSize = 0;
            btnEssayGrading.FlatStyle = FlatStyle.Flat;
            btnEssayGrading.Font = new Font("Segoe UI", 10F);
            btnEssayGrading.ForeColor = Color.LightCyan;
            btnEssayGrading.Location = new Point(0, 53);
            btnEssayGrading.Margin = new Padding(3, 4, 3, 4);
            btnEssayGrading.Name = "btnEssayGrading";
            btnEssayGrading.Padding = new Padding(46, 0, 0, 0);
            btnEssayGrading.Size = new Size(297, 53);
            btnEssayGrading.TabIndex = 1;
            btnEssayGrading.Text = "✍️ Chấm tự luận";
            btnEssayGrading.TextAlign = ContentAlignment.MiddleLeft;
            btnEssayGrading.UseVisualStyleBackColor = true;
            btnEssayGrading.Click += btnEssayGrading_Click;
            // 
            // btnLiveMonitor
            // 
            btnLiveMonitor.Cursor = Cursors.Hand;
            btnLiveMonitor.Dock = DockStyle.Top;
            btnLiveMonitor.FlatAppearance.BorderSize = 0;
            btnLiveMonitor.FlatStyle = FlatStyle.Flat;
            btnLiveMonitor.Font = new Font("Segoe UI", 10F);
            btnLiveMonitor.ForeColor = Color.LightCyan;
            btnLiveMonitor.Location = new Point(0, 0);
            btnLiveMonitor.Margin = new Padding(3, 4, 3, 4);
            btnLiveMonitor.Name = "btnLiveMonitor";
            btnLiveMonitor.Padding = new Padding(46, 0, 0, 0);
            btnLiveMonitor.Size = new Size(297, 53);
            btnLiveMonitor.TabIndex = 0;
            btnLiveMonitor.Text = "📹 Giám sát Live";
            btnLiveMonitor.TextAlign = ContentAlignment.MiddleLeft;
            btnLiveMonitor.UseVisualStyleBackColor = true;
            btnLiveMonitor.Click += btnLiveMonitor_Click;
            // 
            // btnGroupMonitoring
            // 
            btnGroupMonitoring.Cursor = Cursors.Hand;
            btnGroupMonitoring.Dock = DockStyle.Top;
            btnGroupMonitoring.FlatAppearance.BorderSize = 0;
            btnGroupMonitoring.FlatStyle = FlatStyle.Flat;
            btnGroupMonitoring.Font = new Font("Segoe UI", 11F);
            btnGroupMonitoring.ForeColor = Color.White;
            btnGroupMonitoring.Location = new Point(0, 526);
            btnGroupMonitoring.Margin = new Padding(3, 4, 3, 4);
            btnGroupMonitoring.Name = "btnGroupMonitoring";
            btnGroupMonitoring.Padding = new Padding(17, 0, 0, 0);
            btnGroupMonitoring.Size = new Size(297, 67);
            btnGroupMonitoring.TabIndex = 6;
            btnGroupMonitoring.Text = "⏱️ Giám Sát && Chấm Điểm";
            btnGroupMonitoring.TextAlign = ContentAlignment.MiddleLeft;
            btnGroupMonitoring.UseVisualStyleBackColor = true;
            btnGroupMonitoring.Click += btnGroupMonitoring_Click;
            // 
            // pnlSubMenuTesting
            // 
            pnlSubMenuTesting.AutoSize = true;
            pnlSubMenuTesting.BackColor = Color.FromArgb(31, 41, 55);
            pnlSubMenuTesting.Controls.Add(btnExamList);
            pnlSubMenuTesting.Controls.Add(btnExamConfig);
            pnlSubMenuTesting.Controls.Add(btnQuestionBank);
            pnlSubMenuTesting.Dock = DockStyle.Top;
            pnlSubMenuTesting.Location = new Point(0, 367);
            pnlSubMenuTesting.Margin = new Padding(3, 4, 3, 4);
            pnlSubMenuTesting.Name = "pnlSubMenuTesting";
            pnlSubMenuTesting.Size = new Size(297, 159);
            pnlSubMenuTesting.TabIndex = 5;
            pnlSubMenuTesting.Visible = false;
            // 
            // btnExamList
            // 
            btnExamList.Cursor = Cursors.Hand;
            btnExamList.Dock = DockStyle.Top;
            btnExamList.FlatAppearance.BorderSize = 0;
            btnExamList.FlatStyle = FlatStyle.Flat;
            btnExamList.Font = new Font("Segoe UI", 10F);
            btnExamList.ForeColor = Color.LightCyan;
            btnExamList.Location = new Point(0, 106);
            btnExamList.Margin = new Padding(3, 4, 3, 4);
            btnExamList.Name = "btnExamList";
            btnExamList.Padding = new Padding(46, 0, 0, 0);
            btnExamList.Size = new Size(297, 53);
            btnExamList.TabIndex = 2;
            btnExamList.Text = "📋 Quản lý kỳ thi";
            btnExamList.TextAlign = ContentAlignment.MiddleLeft;
            btnExamList.UseVisualStyleBackColor = true;
            btnExamList.Click += btnExamList_Click;
            // 
            // btnExamConfig
            // 
            btnExamConfig.Cursor = Cursors.Hand;
            btnExamConfig.Dock = DockStyle.Top;
            btnExamConfig.FlatAppearance.BorderSize = 0;
            btnExamConfig.FlatStyle = FlatStyle.Flat;
            btnExamConfig.Font = new Font("Segoe UI", 10F);
            btnExamConfig.ForeColor = Color.LightCyan;
            btnExamConfig.Location = new Point(0, 53);
            btnExamConfig.Margin = new Padding(3, 4, 3, 4);
            btnExamConfig.Name = "btnExamConfig";
            btnExamConfig.Padding = new Padding(46, 0, 0, 0);
            btnExamConfig.Size = new Size(297, 53);
            btnExamConfig.TabIndex = 1;
            btnExamConfig.Text = "⚙️ Cấu hình đề thi";
            btnExamConfig.TextAlign = ContentAlignment.MiddleLeft;
            btnExamConfig.UseVisualStyleBackColor = true;
            btnExamConfig.Click += btnExamConfig_Click;
            // 
            // btnQuestionBank
            // 
            btnQuestionBank.Cursor = Cursors.Hand;
            btnQuestionBank.Dock = DockStyle.Top;
            btnQuestionBank.FlatAppearance.BorderSize = 0;
            btnQuestionBank.FlatStyle = FlatStyle.Flat;
            btnQuestionBank.Font = new Font("Segoe UI", 10F);
            btnQuestionBank.ForeColor = Color.LightCyan;
            btnQuestionBank.Location = new Point(0, 0);
            btnQuestionBank.Margin = new Padding(3, 4, 3, 4);
            btnQuestionBank.Name = "btnQuestionBank";
            btnQuestionBank.Padding = new Padding(46, 0, 0, 0);
            btnQuestionBank.Size = new Size(297, 53);
            btnQuestionBank.TabIndex = 0;
            btnQuestionBank.Text = "📂 Ngân hàng câu hỏi";
            btnQuestionBank.TextAlign = ContentAlignment.MiddleLeft;
            btnQuestionBank.UseVisualStyleBackColor = true;
            btnQuestionBank.Click += btnQuestionBank_Click;
            // 
            // btnGroupTesting
            // 
            btnGroupTesting.Cursor = Cursors.Hand;
            btnGroupTesting.Dock = DockStyle.Top;
            btnGroupTesting.FlatAppearance.BorderSize = 0;
            btnGroupTesting.FlatStyle = FlatStyle.Flat;
            btnGroupTesting.Font = new Font("Segoe UI", 11F);
            btnGroupTesting.ForeColor = Color.White;
            btnGroupTesting.Location = new Point(0, 300);
            btnGroupTesting.Margin = new Padding(3, 4, 3, 4);
            btnGroupTesting.Name = "btnGroupTesting";
            btnGroupTesting.Padding = new Padding(17, 0, 0, 0);
            btnGroupTesting.Size = new Size(297, 67);
            btnGroupTesting.TabIndex = 4;
            btnGroupTesting.Text = "📝 Khảo Thí && Đề Thi";
            btnGroupTesting.TextAlign = ContentAlignment.MiddleLeft;
            btnGroupTesting.UseVisualStyleBackColor = true;
            btnGroupTesting.Click += btnGroupTesting_Click;
            // 
            // pnlSubMenuCourseDocs
            // 
            pnlSubMenuCourseDocs.AutoSize = true;
            pnlSubMenuCourseDocs.BackColor = Color.FromArgb(31, 41, 55);
            pnlSubMenuCourseDocs.Controls.Add(btnOnlineClasses);
            pnlSubMenuCourseDocs.Controls.Add(btnAssignedCourses);
            pnlSubMenuCourseDocs.Dock = DockStyle.Top;
            pnlSubMenuCourseDocs.Location = new Point(0, 194);
            pnlSubMenuCourseDocs.Margin = new Padding(3, 4, 3, 4);
            pnlSubMenuCourseDocs.Name = "pnlSubMenuCourseDocs";
            pnlSubMenuCourseDocs.Size = new Size(297, 106);
            pnlSubMenuCourseDocs.TabIndex = 3;
            pnlSubMenuCourseDocs.Visible = false;
            // 
            // btnOnlineClasses
            // 
            btnOnlineClasses.Cursor = Cursors.Hand;
            btnOnlineClasses.Dock = DockStyle.Top;
            btnOnlineClasses.FlatAppearance.BorderSize = 0;
            btnOnlineClasses.FlatStyle = FlatStyle.Flat;
            btnOnlineClasses.Font = new Font("Segoe UI", 10F);
            btnOnlineClasses.ForeColor = Color.LightCyan;
            btnOnlineClasses.Location = new Point(0, 53);
            btnOnlineClasses.Margin = new Padding(3, 4, 3, 4);
            btnOnlineClasses.Name = "btnOnlineClasses";
            btnOnlineClasses.Padding = new Padding(46, 0, 0, 0);
            btnOnlineClasses.Size = new Size(297, 53);
            btnOnlineClasses.TabIndex = 1;
            btnOnlineClasses.Text = "🌐 Lớp học trực tuyến";
            btnOnlineClasses.TextAlign = ContentAlignment.MiddleLeft;
            btnOnlineClasses.UseVisualStyleBackColor = true;
            btnOnlineClasses.Click += btnOnlineClasses_Click;
            // 
            // btnAssignedCourses
            // 
            btnAssignedCourses.Cursor = Cursors.Hand;
            btnAssignedCourses.Dock = DockStyle.Top;
            btnAssignedCourses.FlatAppearance.BorderSize = 0;
            btnAssignedCourses.FlatStyle = FlatStyle.Flat;
            btnAssignedCourses.Font = new Font("Segoe UI", 10F);
            btnAssignedCourses.ForeColor = Color.LightCyan;
            btnAssignedCourses.Location = new Point(0, 0);
            btnAssignedCourses.Margin = new Padding(3, 4, 3, 4);
            btnAssignedCourses.Name = "btnAssignedCourses";
            btnAssignedCourses.Padding = new Padding(46, 0, 0, 0);
            btnAssignedCourses.Size = new Size(297, 53);
            btnAssignedCourses.TabIndex = 0;
            btnAssignedCourses.Text = "📖 Khóa học phân công";
            btnAssignedCourses.TextAlign = ContentAlignment.MiddleLeft;
            btnAssignedCourses.UseVisualStyleBackColor = true;
            btnAssignedCourses.Click += btnAssignedCourses_Click;
            // 
            // btnGroupCourseDocs
            // 
            btnGroupCourseDocs.Cursor = Cursors.Hand;
            btnGroupCourseDocs.Dock = DockStyle.Top;
            btnGroupCourseDocs.FlatAppearance.BorderSize = 0;
            btnGroupCourseDocs.FlatStyle = FlatStyle.Flat;
            btnGroupCourseDocs.Font = new Font("Segoe UI", 11F);
            btnGroupCourseDocs.ForeColor = Color.White;
            btnGroupCourseDocs.Location = new Point(0, 127);
            btnGroupCourseDocs.Margin = new Padding(3, 4, 3, 4);
            btnGroupCourseDocs.Name = "btnGroupCourseDocs";
            btnGroupCourseDocs.Padding = new Padding(17, 0, 0, 0);
            btnGroupCourseDocs.Size = new Size(297, 67);
            btnGroupCourseDocs.TabIndex = 2;
            btnGroupCourseDocs.Text = "📚 Học Liệu && Lớp Học";
            btnGroupCourseDocs.TextAlign = ContentAlignment.MiddleLeft;
            btnGroupCourseDocs.UseVisualStyleBackColor = true;
            btnGroupCourseDocs.Click += btnGroupCourseDocs_Click;
            // 
            // btnOverview
            // 
            btnOverview.Cursor = Cursors.Hand;
            btnOverview.Dock = DockStyle.Top;
            btnOverview.FlatAppearance.BorderSize = 0;
            btnOverview.FlatStyle = FlatStyle.Flat;
            btnOverview.Font = new Font("Segoe UI", 11F);
            btnOverview.ForeColor = Color.White;
            btnOverview.Location = new Point(0, 60);
            btnOverview.Margin = new Padding(3, 4, 3, 4);
            btnOverview.Name = "btnOverview";
            btnOverview.Padding = new Padding(17, 0, 0, 0);
            btnOverview.Size = new Size(297, 67);
            btnOverview.TabIndex = 1;
            btnOverview.Text = "🏠 Tổng Quan";
            btnOverview.TextAlign = ContentAlignment.MiddleLeft;
            btnOverview.UseVisualStyleBackColor = true;
            btnOverview.Click += btnOverview_Click;
            // 
            // pnlLogo
            // 
            pnlLogo.Controls.Add(lblLogo);
            pnlLogo.Dock = DockStyle.Top;
            pnlLogo.Location = new Point(0, 0);
            pnlLogo.Margin = new Padding(3, 4, 3, 4);
            pnlLogo.Name = "pnlLogo";
            pnlLogo.Size = new Size(297, 60);
            pnlLogo.TabIndex = 0;
            // 
            // lblLogo
            // 
            lblLogo.Dock = DockStyle.Fill;
            lblLogo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblLogo.ForeColor = Color.White;
            lblLogo.Location = new Point(0, 0);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(297, 60);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "COURSE GUARD";
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlMainboard
            // 
            pnlMainboard.BackColor = Color.FromArgb(243, 244, 246);
            pnlMainboard.Dock = DockStyle.Fill;
            pnlMainboard.Location = new Point(297, 0);
            pnlMainboard.Margin = new Padding(3, 4, 3, 4);
            pnlMainboard.Name = "pnlMainboard";
            pnlMainboard.Size = new Size(1166, 960);
            pnlMainboard.TabIndex = 2;
            // 
            // TeacherDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1463, 960);
            Controls.Add(pnlMainboard);
            Controls.Add(pnlSidebar);
            Margin = new Padding(3, 4, 3, 4);
            Name = "TeacherDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Teacher Dashboard - CourseGuard";
            pnlSidebar.ResumeLayout(false);
            pnlSidebar.PerformLayout();
            pnlSubMenuMonitoring.ResumeLayout(false);
            pnlSubMenuTesting.ResumeLayout(false);
            pnlSubMenuCourseDocs.ResumeLayout(false);
            pnlLogo.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlMainboard;
        private System.Windows.Forms.Panel pnlLogo;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Button btnOverview;
        private System.Windows.Forms.Button btnGroupCourseDocs;
        private System.Windows.Forms.Panel pnlSubMenuCourseDocs;
        private System.Windows.Forms.Button btnOnlineClasses;
        private System.Windows.Forms.Button btnAssignedCourses;
        private System.Windows.Forms.Panel pnlSubMenuTesting;
        private System.Windows.Forms.Button btnExamList;
        private System.Windows.Forms.Button btnExamConfig;
        private System.Windows.Forms.Button btnQuestionBank;
        private System.Windows.Forms.Button btnGroupTesting;
        private System.Windows.Forms.Panel pnlSubMenuMonitoring;
        private System.Windows.Forms.Button btnScoreManagement;
        private System.Windows.Forms.Button btnEssayGrading;
        private System.Windows.Forms.Button btnLiveMonitor;
        private System.Windows.Forms.Button btnGroupMonitoring;
        private System.Windows.Forms.Button btnNotifications;
        private System.Windows.Forms.Button btnLogout;
    }
}
