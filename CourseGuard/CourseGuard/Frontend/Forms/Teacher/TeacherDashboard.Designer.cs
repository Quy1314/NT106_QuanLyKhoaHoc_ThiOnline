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
            pnlSubMenuMonitoring = new Panel();
            btnScoreManagement = new Button();
            btnEssayGrading = new Button();
            btnLiveMonitor = new Button();
            pnlSubMenuTesting = new Panel();
            btnExamList = new Button();
            btnExamConfig = new Button();
            btnQuestionBank = new Button();
            pnlSubMenuCourseDocs = new Panel();
            btnOnlineClasses = new Button();
            btnAssignedCourses = new Button();
            pnlMainboard = new Panel();
            btnMail = new Button();
            pnlEmailDropdown = new Panel();
            flpEmails = new FlowLayoutPanel();
            lblEmailHeader = new Label();
            pnlSubMenuMonitoring.SuspendLayout();
            pnlSubMenuTesting.SuspendLayout();
            pnlSubMenuCourseDocs.SuspendLayout();
            pnlEmailDropdown.SuspendLayout();
            SuspendLayout();
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
            // pnlMainboard
            // 
            pnlMainboard.Dock = DockStyle.Fill;
            pnlMainboard.Location = new Point(260, 70);
            pnlMainboard.Name = "pnlMainboard";
            pnlMainboard.Size = new Size(1120, 790);
            pnlMainboard.TabIndex = 1;

            // 
            // btnMail
            // 
            btnMail.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMail.Cursor = Cursors.Hand;
            btnMail.FlatAppearance.BorderSize = 0;
            btnMail.FlatStyle = FlatStyle.Flat;
            btnMail.Font = new Font("Segoe UI Emoji", 14F);
            btnMail.ForeColor = Color.FromArgb(107, 114, 128);
            btnMail.Location = new Point(1060, 15);
            btnMail.Name = "btnMail";
            btnMail.Size = new Size(40, 40);
            btnMail.TabIndex = 0;
            btnMail.Text = "✉️";
            btnMail.UseVisualStyleBackColor = true;
            btnMail.Click += btnMail_Click;
            btnMail.MouseEnter += btnMail_MouseEnter;
            btnMail.MouseLeave += btnMail_MouseLeave;
            // 
            // pnlEmailDropdown
            // 
            pnlEmailDropdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlEmailDropdown.Controls.Add(flpEmails);
            pnlEmailDropdown.Controls.Add(lblEmailHeader);
            pnlEmailDropdown.Location = new Point(1020, 70);
            pnlEmailDropdown.Name = "pnlEmailDropdown";
            pnlEmailDropdown.Padding = new Padding(10);
            pnlEmailDropdown.Size = new Size(350, 420);
            pnlEmailDropdown.TabIndex = 3;
            pnlEmailDropdown.Visible = false;
            // 
            // flpEmails
            // 
            flpEmails.AutoScroll = true;
            flpEmails.Dock = DockStyle.Fill;
            flpEmails.FlowDirection = FlowDirection.TopDown;
            flpEmails.Location = new Point(10, 50);
            flpEmails.Name = "flpEmails";
            flpEmails.Size = new Size(330, 360);
            flpEmails.TabIndex = 1;
            flpEmails.WrapContents = false;
            // 
            // lblEmailHeader
            // 
            lblEmailHeader.Dock = DockStyle.Top;
            lblEmailHeader.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            lblEmailHeader.ForeColor = Color.FromArgb(37, 99, 235);
            lblEmailHeader.Location = new Point(10, 10);
            lblEmailHeader.Name = "lblEmailHeader";
            lblEmailHeader.Padding = new Padding(5, 5, 5, 10);
            lblEmailHeader.Size = new Size(330, 40);
            lblEmailHeader.TabIndex = 0;
            lblEmailHeader.Text = "Hộp thư cá nhân";
            lblEmailHeader.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // TeacherDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 24, 39);
            ClientSize = new Size(1380, 860);
            Controls.Add(pnlEmailDropdown);
            Controls.Add(pnlMainboard);
            Controls.Add(btnMail);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1000, 650);
            Name = "TeacherDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Teacher Dashboard — CourseGuard";
            WindowState = FormWindowState.Maximized;
            pnlSubMenuMonitoring.ResumeLayout(false);
            pnlSubMenuTesting.ResumeLayout(false);
            pnlSubMenuCourseDocs.ResumeLayout(false);
            pnlEmailDropdown.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // ---------------------------------------------------------------
        //  Khai báo các control (bắt buộc để VS Designer nhận diện)
        // ---------------------------------------------------------------
        private System.Windows.Forms.Panel pnlSubMenuCourseDocs;
        private System.Windows.Forms.Button btnAssignedCourses;
        private System.Windows.Forms.Button btnOnlineClasses;
        private System.Windows.Forms.Panel pnlSubMenuTesting;
        private System.Windows.Forms.Button btnQuestionBank;
        private System.Windows.Forms.Button btnExamConfig;
        private System.Windows.Forms.Button btnExamList;
        private System.Windows.Forms.Panel pnlSubMenuMonitoring;
        private System.Windows.Forms.Button btnLiveMonitor;
        private System.Windows.Forms.Button btnEssayGrading;
        private System.Windows.Forms.Button btnScoreManagement;
        private System.Windows.Forms.Panel pnlMainboard;
        private System.Windows.Forms.Button btnMail;
        private System.Windows.Forms.Panel pnlEmailDropdown;
        private System.Windows.Forms.Label lblEmailHeader;
        private System.Windows.Forms.FlowLayoutPanel flpEmails;
    }
}
