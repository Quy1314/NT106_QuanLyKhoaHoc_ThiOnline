namespace CourseGuard.Frontend.Forms.Teacher
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
            pnlSidebarBottom = new Panel();
            btnLogout = new Button();
            pnlDivider = new Panel();
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
            lblLogoIcon = new Label();
            lblLogoText = new Label();
            pnlMainboard = new Panel();
            pnlSidebar.SuspendLayout();
            pnlSidebarBottom.SuspendLayout();
            pnlSubMenuMonitoring.SuspendLayout();
            pnlSubMenuTesting.SuspendLayout();
            pnlSubMenuCourseDocs.SuspendLayout();
            pnlLogo.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(17, 24, 39);
            pnlSidebar.Controls.Add(pnlSidebarBottom);
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
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(260, 860);
            pnlSidebar.TabIndex = 0;
            // 
            // pnlSidebarBottom
            // 
            pnlSidebarBottom.BackColor = Color.FromArgb(17, 24, 39);
            pnlSidebarBottom.Controls.Add(btnLogout);
            pnlSidebarBottom.Controls.Add(pnlDivider);
            pnlSidebarBottom.Dock = DockStyle.Bottom;
            pnlSidebarBottom.Location = new Point(0, 795);
            pnlSidebarBottom.Name = "pnlSidebarBottom";
            pnlSidebarBottom.Size = new Size(260, 65);
            pnlSidebarBottom.TabIndex = 9;
            // 
            // btnLogout
            // 
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.Dock = DockStyle.Fill;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 10.5F);
            btnLogout.ForeColor = Color.FromArgb(252, 165, 165);
            btnLogout.Location = new Point(0, 1);
            btnLogout.Name = "btnLogout";
            btnLogout.Padding = new Padding(16, 0, 0, 0);
            btnLogout.Size = new Size(260, 64);
            btnLogout.TabIndex = 1;
            btnLogout.Text = "🚪  Đăng Xuất";
            btnLogout.TextAlign = ContentAlignment.MiddleLeft;
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // pnlDivider
            // 
            pnlDivider.BackColor = Color.FromArgb(55, 65, 81);
            pnlDivider.Dock = DockStyle.Top;
            pnlDivider.Location = new Point(0, 0);
            pnlDivider.Name = "pnlDivider";
            pnlDivider.Size = new Size(260, 1);
            pnlDivider.TabIndex = 0;
            // 
            // btnNotifications
            // 
            btnNotifications.BackColor = Color.Transparent;
            btnNotifications.Cursor = Cursors.Hand;
            btnNotifications.Dock = DockStyle.Top;
            btnNotifications.FlatAppearance.BorderSize = 0;
            btnNotifications.FlatStyle = FlatStyle.Flat;
            btnNotifications.Font = new Font("Segoe UI", 10.5F);
            btnNotifications.ForeColor = Color.FromArgb(209, 213, 219);
            btnNotifications.Location = new Point(0, 597);
            btnNotifications.Name = "btnNotifications";
            btnNotifications.Padding = new Padding(16, 0, 0, 0);
            btnNotifications.Size = new Size(260, 48);
            btnNotifications.TabIndex = 8;
            btnNotifications.Text = "🔔  Trung Tâm Thông Báo";
            btnNotifications.TextAlign = ContentAlignment.MiddleLeft;
            btnNotifications.UseVisualStyleBackColor = false;
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
            pnlSubMenuMonitoring.Location = new Point(0, 477);
            pnlSubMenuMonitoring.Name = "pnlSubMenuMonitoring";
            pnlSubMenuMonitoring.Size = new Size(260, 120);
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
            btnScoreManagement.ForeColor = Color.FromArgb(147, 210, 255);
            btnScoreManagement.Location = new Point(0, 80);
            btnScoreManagement.Name = "btnScoreManagement";
            btnScoreManagement.Padding = new Padding(40, 0, 0, 0);
            btnScoreManagement.Size = new Size(260, 40);
            btnScoreManagement.TabIndex = 2;
            btnScoreManagement.Text = "   📊  Quản lý điểm số";
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
            btnEssayGrading.ForeColor = Color.FromArgb(147, 210, 255);
            btnEssayGrading.Location = new Point(0, 40);
            btnEssayGrading.Name = "btnEssayGrading";
            btnEssayGrading.Padding = new Padding(40, 0, 0, 0);
            btnEssayGrading.Size = new Size(260, 40);
            btnEssayGrading.TabIndex = 1;
            btnEssayGrading.Text = "   ✍️  Chấm tự luận";
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
            btnLiveMonitor.ForeColor = Color.FromArgb(147, 210, 255);
            btnLiveMonitor.Location = new Point(0, 0);
            btnLiveMonitor.Name = "btnLiveMonitor";
            btnLiveMonitor.Padding = new Padding(40, 0, 0, 0);
            btnLiveMonitor.Size = new Size(260, 40);
            btnLiveMonitor.TabIndex = 0;
            btnLiveMonitor.Text = "   📹  Giám sát Live";
            btnLiveMonitor.TextAlign = ContentAlignment.MiddleLeft;
            btnLiveMonitor.UseVisualStyleBackColor = true;
            btnLiveMonitor.Click += btnLiveMonitor_Click;
            // 
            // btnGroupMonitoring
            // 
            btnGroupMonitoring.BackColor = Color.Transparent;
            btnGroupMonitoring.Cursor = Cursors.Hand;
            btnGroupMonitoring.Dock = DockStyle.Top;
            btnGroupMonitoring.FlatAppearance.BorderSize = 0;
            btnGroupMonitoring.FlatStyle = FlatStyle.Flat;
            btnGroupMonitoring.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnGroupMonitoring.ForeColor = Color.White;
            btnGroupMonitoring.Location = new Point(0, 429);
            btnGroupMonitoring.Name = "btnGroupMonitoring";
            btnGroupMonitoring.Padding = new Padding(16, 0, 0, 0);
            btnGroupMonitoring.Size = new Size(260, 48);
            btnGroupMonitoring.TabIndex = 6;
            btnGroupMonitoring.Text = "⏱️  Giám Sát && Chấm Điểm  ›";
            btnGroupMonitoring.TextAlign = ContentAlignment.MiddleLeft;
            btnGroupMonitoring.UseVisualStyleBackColor = false;
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
            pnlSubMenuTesting.Location = new Point(0, 309);
            pnlSubMenuTesting.Name = "pnlSubMenuTesting";
            pnlSubMenuTesting.Size = new Size(260, 120);
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
            btnExamList.ForeColor = Color.FromArgb(147, 210, 255);
            btnExamList.Location = new Point(0, 80);
            btnExamList.Name = "btnExamList";
            btnExamList.Padding = new Padding(40, 0, 0, 0);
            btnExamList.Size = new Size(260, 40);
            btnExamList.TabIndex = 2;
            btnExamList.Text = "   📋  Quản lý kỳ thi";
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
            btnExamConfig.ForeColor = Color.FromArgb(147, 210, 255);
            btnExamConfig.Location = new Point(0, 40);
            btnExamConfig.Name = "btnExamConfig";
            btnExamConfig.Padding = new Padding(40, 0, 0, 0);
            btnExamConfig.Size = new Size(260, 40);
            btnExamConfig.TabIndex = 1;
            btnExamConfig.Text = "   ⚙️  Cấu hình đề thi";
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
            btnQuestionBank.ForeColor = Color.FromArgb(147, 210, 255);
            btnQuestionBank.Location = new Point(0, 0);
            btnQuestionBank.Name = "btnQuestionBank";
            btnQuestionBank.Padding = new Padding(40, 0, 0, 0);
            btnQuestionBank.Size = new Size(260, 40);
            btnQuestionBank.TabIndex = 0;
            btnQuestionBank.Text = "   📂  Ngân hàng câu hỏi";
            btnQuestionBank.TextAlign = ContentAlignment.MiddleLeft;
            btnQuestionBank.UseVisualStyleBackColor = true;
            btnQuestionBank.Click += btnQuestionBank_Click;
            // 
            // btnGroupTesting
            // 
            btnGroupTesting.BackColor = Color.Transparent;
            btnGroupTesting.Cursor = Cursors.Hand;
            btnGroupTesting.Dock = DockStyle.Top;
            btnGroupTesting.FlatAppearance.BorderSize = 0;
            btnGroupTesting.FlatStyle = FlatStyle.Flat;
            btnGroupTesting.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnGroupTesting.ForeColor = Color.White;
            btnGroupTesting.Location = new Point(0, 261);
            btnGroupTesting.Name = "btnGroupTesting";
            btnGroupTesting.Padding = new Padding(16, 0, 0, 0);
            btnGroupTesting.Size = new Size(260, 48);
            btnGroupTesting.TabIndex = 4;
            btnGroupTesting.Text = "📝  Khảo Thí && Đề Thi  ›";
            btnGroupTesting.TextAlign = ContentAlignment.MiddleLeft;
            btnGroupTesting.UseVisualStyleBackColor = false;
            btnGroupTesting.Click += btnGroupTesting_Click;
            // 
            // pnlSubMenuCourseDocs
            // 
            pnlSubMenuCourseDocs.AutoSize = true;
            pnlSubMenuCourseDocs.BackColor = Color.FromArgb(31, 41, 55);
            pnlSubMenuCourseDocs.Controls.Add(btnOnlineClasses);
            pnlSubMenuCourseDocs.Controls.Add(btnAssignedCourses);
            pnlSubMenuCourseDocs.Dock = DockStyle.Top;
            pnlSubMenuCourseDocs.Location = new Point(0, 181);
            pnlSubMenuCourseDocs.Name = "pnlSubMenuCourseDocs";
            pnlSubMenuCourseDocs.Size = new Size(260, 80);
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
            btnOnlineClasses.ForeColor = Color.FromArgb(147, 210, 255);
            btnOnlineClasses.Location = new Point(0, 40);
            btnOnlineClasses.Name = "btnOnlineClasses";
            btnOnlineClasses.Padding = new Padding(40, 0, 0, 0);
            btnOnlineClasses.Size = new Size(260, 40);
            btnOnlineClasses.TabIndex = 1;
            btnOnlineClasses.Text = "   🌐  Lớp học trực tuyến";
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
            btnAssignedCourses.ForeColor = Color.FromArgb(147, 210, 255);
            btnAssignedCourses.Location = new Point(0, 0);
            btnAssignedCourses.Name = "btnAssignedCourses";
            btnAssignedCourses.Padding = new Padding(40, 0, 0, 0);
            btnAssignedCourses.Size = new Size(260, 40);
            btnAssignedCourses.TabIndex = 0;
            btnAssignedCourses.Text = "   📖  Phân công lớp";
            btnAssignedCourses.TextAlign = ContentAlignment.MiddleLeft;
            btnAssignedCourses.UseVisualStyleBackColor = true;
            btnAssignedCourses.Click += btnAssignedCourses_Click;
            // 
            // btnGroupCourseDocs
            // 
            btnGroupCourseDocs.BackColor = Color.Transparent;
            btnGroupCourseDocs.Cursor = Cursors.Hand;
            btnGroupCourseDocs.Dock = DockStyle.Top;
            btnGroupCourseDocs.FlatAppearance.BorderSize = 0;
            btnGroupCourseDocs.FlatStyle = FlatStyle.Flat;
            btnGroupCourseDocs.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnGroupCourseDocs.ForeColor = Color.White;
            btnGroupCourseDocs.Location = new Point(0, 133);
            btnGroupCourseDocs.Name = "btnGroupCourseDocs";
            btnGroupCourseDocs.Padding = new Padding(16, 0, 0, 0);
            btnGroupCourseDocs.Size = new Size(260, 48);
            btnGroupCourseDocs.TabIndex = 2;
            btnGroupCourseDocs.Text = "📚  Học Liệu && Lớp Học  ›";
            btnGroupCourseDocs.TextAlign = ContentAlignment.MiddleLeft;
            btnGroupCourseDocs.UseVisualStyleBackColor = false;
            btnGroupCourseDocs.Click += btnGroupCourseDocs_Click;
            // 
            // btnOverview
            // 
            btnOverview.BackColor = Color.Transparent;
            btnOverview.Cursor = Cursors.Hand;
            btnOverview.Dock = DockStyle.Top;
            btnOverview.FlatAppearance.BorderSize = 0;
            btnOverview.FlatStyle = FlatStyle.Flat;
            btnOverview.Font = new Font("Segoe UI", 10.5F);
            btnOverview.ForeColor = Color.FromArgb(209, 213, 219);
            btnOverview.Location = new Point(0, 85);
            btnOverview.Name = "btnOverview";
            btnOverview.Padding = new Padding(16, 0, 0, 0);
            btnOverview.Size = new Size(260, 48);
            btnOverview.TabIndex = 1;
            btnOverview.Text = "🏠  Tổng Quan";
            btnOverview.TextAlign = ContentAlignment.MiddleLeft;
            btnOverview.UseVisualStyleBackColor = false;
            btnOverview.Click += btnOverview_Click;
            // 
            // pnlLogo
            // 
            pnlLogo.BackColor = Color.FromArgb(11, 17, 28);
            pnlLogo.Controls.Add(lblLogoIcon);
            pnlLogo.Controls.Add(lblLogoText);
            pnlLogo.Dock = DockStyle.Top;
            pnlLogo.Location = new Point(0, 0);
            pnlLogo.Name = "pnlLogo";
            pnlLogo.Size = new Size(260, 85);
            pnlLogo.TabIndex = 0;
            // 
            // lblLogoIcon
            // 
            lblLogoIcon.Dock = DockStyle.Left;
            lblLogoIcon.Font = new Font("Segoe UI Emoji", 20F);
            lblLogoIcon.ForeColor = Color.FromArgb(99, 179, 237);
            lblLogoIcon.Location = new Point(0, 0);
            lblLogoIcon.Name = "lblLogoIcon";
            lblLogoIcon.Size = new Size(50, 85);
            lblLogoIcon.TabIndex = 0;
            lblLogoIcon.Text = "🛡";
            lblLogoIcon.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblLogoText
            // 
            lblLogoText.AutoSize = true;
            lblLogoText.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblLogoText.ForeColor = Color.White;
            lblLogoText.Location = new Point(55, 28);
            lblLogoText.Name = "lblLogoText";
            lblLogoText.Size = new Size(163, 32);
            lblLogoText.TabIndex = 1;
            lblLogoText.Text = "CourseGuard";
            lblLogoText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlMainboard
            // 
            pnlMainboard.BackColor = Color.FromArgb(243, 244, 246);
            pnlMainboard.Dock = DockStyle.Fill;
            pnlMainboard.Location = new Point(260, 0);
            pnlMainboard.Name = "pnlMainboard";
            pnlMainboard.Size = new Size(1120, 860);
            pnlMainboard.TabIndex = 1;
            // 
            // TeacherDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 24, 39);
            ClientSize = new Size(1380, 860);
            Controls.Add(pnlMainboard);
            Controls.Add(pnlSidebar);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1000, 650);
            Name = "TeacherDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Teacher Dashboard — CourseGuard";
            pnlSidebar.ResumeLayout(false);
            pnlSidebar.PerformLayout();
            pnlSidebarBottom.ResumeLayout(false);
            pnlSubMenuMonitoring.ResumeLayout(false);
            pnlSubMenuTesting.ResumeLayout(false);
            pnlSubMenuCourseDocs.ResumeLayout(false);
            pnlLogo.ResumeLayout(false);
            pnlLogo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // ---------------------------------------------------------------
        //  Khai báo các control (bắt buộc để VS Designer nhận diện)
        // ---------------------------------------------------------------
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlLogo;
        private System.Windows.Forms.Label lblLogoIcon;
        private System.Windows.Forms.Label lblLogoText;
        private System.Windows.Forms.Button btnOverview;
        private System.Windows.Forms.Button btnGroupCourseDocs;
        private System.Windows.Forms.Panel pnlSubMenuCourseDocs;
        private System.Windows.Forms.Button btnAssignedCourses;
        private System.Windows.Forms.Button btnOnlineClasses;
        private System.Windows.Forms.Button btnGroupTesting;
        private System.Windows.Forms.Panel pnlSubMenuTesting;
        private System.Windows.Forms.Button btnQuestionBank;
        private System.Windows.Forms.Button btnExamConfig;
        private System.Windows.Forms.Button btnExamList;
        private System.Windows.Forms.Button btnGroupMonitoring;
        private System.Windows.Forms.Panel pnlSubMenuMonitoring;
        private System.Windows.Forms.Button btnLiveMonitor;
        private System.Windows.Forms.Button btnEssayGrading;
        private System.Windows.Forms.Button btnScoreManagement;
        private System.Windows.Forms.Button btnNotifications;
        private System.Windows.Forms.Panel pnlSidebarBottom;
        private System.Windows.Forms.Panel pnlDivider;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Panel pnlMainboard;
    }
}
