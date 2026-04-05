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
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnNotifications = new System.Windows.Forms.Button();
            this.pnlSubMenuMonitoring = new System.Windows.Forms.Panel();
            this.btnScoreManagement = new System.Windows.Forms.Button();
            this.btnEssayGrading = new System.Windows.Forms.Button();
            this.btnLiveMonitor = new System.Windows.Forms.Button();
            this.btnGroupMonitoring = new System.Windows.Forms.Button();
            this.pnlSubMenuTesting = new System.Windows.Forms.Panel();
            this.btnExamList = new System.Windows.Forms.Button();
            this.btnExamConfig = new System.Windows.Forms.Button();
            this.btnQuestionBank = new System.Windows.Forms.Button();
            this.btnGroupTesting = new System.Windows.Forms.Button();
            this.pnlSubMenuCourseDocs = new System.Windows.Forms.Panel();
            this.btnOnlineClasses = new System.Windows.Forms.Button();
            this.btnAssignedCourses = new System.Windows.Forms.Button();
            this.btnGroupCourseDocs = new System.Windows.Forms.Button();
            this.btnOverview = new System.Windows.Forms.Button();
            this.pnlLogo = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlMainboard = new System.Windows.Forms.Panel();
            this.pnlSidebar.SuspendLayout();
            this.pnlSubMenuMonitoring.SuspendLayout();
            this.pnlSubMenuTesting.SuspendLayout();
            this.pnlSubMenuCourseDocs.SuspendLayout();
            this.pnlLogo.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.pnlSidebar.Controls.Add(this.btnLogout);
            this.pnlSidebar.Controls.Add(this.btnNotifications);
            this.pnlSidebar.Controls.Add(this.pnlSubMenuMonitoring);
            this.pnlSidebar.Controls.Add(this.btnGroupMonitoring);
            this.pnlSidebar.Controls.Add(this.pnlSubMenuTesting);
            this.pnlSidebar.Controls.Add(this.btnGroupTesting);
            this.pnlSidebar.Controls.Add(this.pnlSubMenuCourseDocs);
            this.pnlSidebar.Controls.Add(this.btnGroupCourseDocs);
            this.pnlSidebar.Controls.Add(this.btnOverview);
            this.pnlSidebar.Controls.Add(this.pnlLogo);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(260, 720);
            this.pnlSidebar.TabIndex = 0;
            // 
            // btnLogout
            // 
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnLogout.ForeColor = System.Drawing.Color.White;
            this.btnLogout.Location = new System.Drawing.Point(0, 670);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnLogout.Size = new System.Drawing.Size(260, 50);
            this.btnLogout.TabIndex = 9;
            this.btnLogout.Text = "🚪 Đăng Xuất";
            this.btnLogout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // btnNotifications
            // 
            this.btnNotifications.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNotifications.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNotifications.FlatAppearance.BorderSize = 0;
            this.btnNotifications.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNotifications.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnNotifications.ForeColor = System.Drawing.Color.White;
            this.btnNotifications.Location = new System.Drawing.Point(0, 565);
            this.btnNotifications.Name = "btnNotifications";
            this.btnNotifications.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNotifications.Size = new System.Drawing.Size(260, 50);
            this.btnNotifications.TabIndex = 8;
            this.btnNotifications.Text = "🔔 Trung Tâm Thông Báo";
            this.btnNotifications.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNotifications.UseVisualStyleBackColor = true;
            this.btnNotifications.Click += new System.EventHandler(this.btnNotifications_Click);
            // 
            // pnlSubMenuMonitoring
            // 
            this.pnlSubMenuMonitoring.AutoSize = true;
            this.pnlSubMenuMonitoring.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.pnlSubMenuMonitoring.Controls.Add(this.btnScoreManagement);
            this.pnlSubMenuMonitoring.Controls.Add(this.btnEssayGrading);
            this.pnlSubMenuMonitoring.Controls.Add(this.btnLiveMonitor);
            this.pnlSubMenuMonitoring.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSubMenuMonitoring.Location = new System.Drawing.Point(0, 445);
            this.pnlSubMenuMonitoring.Name = "pnlSubMenuMonitoring";
            this.pnlSubMenuMonitoring.Size = new System.Drawing.Size(260, 120);
            this.pnlSubMenuMonitoring.TabIndex = 7;
            this.pnlSubMenuMonitoring.Visible = false;
            // 
            // btnScoreManagement
            // 
            this.btnScoreManagement.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnScoreManagement.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnScoreManagement.FlatAppearance.BorderSize = 0;
            this.btnScoreManagement.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScoreManagement.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnScoreManagement.ForeColor = System.Drawing.Color.LightCyan;
            this.btnScoreManagement.Location = new System.Drawing.Point(0, 80);
            this.btnScoreManagement.Name = "btnScoreManagement";
            this.btnScoreManagement.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnScoreManagement.Size = new System.Drawing.Size(260, 40);
            this.btnScoreManagement.TabIndex = 2;
            this.btnScoreManagement.Text = "📊 Quản lý điểm số";
            this.btnScoreManagement.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnScoreManagement.UseVisualStyleBackColor = true;
            this.btnScoreManagement.Click += new System.EventHandler(this.btnScoreManagement_Click);
            // 
            // btnEssayGrading
            // 
            this.btnEssayGrading.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEssayGrading.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnEssayGrading.FlatAppearance.BorderSize = 0;
            this.btnEssayGrading.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEssayGrading.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnEssayGrading.ForeColor = System.Drawing.Color.LightCyan;
            this.btnEssayGrading.Location = new System.Drawing.Point(0, 40);
            this.btnEssayGrading.Name = "btnEssayGrading";
            this.btnEssayGrading.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnEssayGrading.Size = new System.Drawing.Size(260, 40);
            this.btnEssayGrading.TabIndex = 1;
            this.btnEssayGrading.Text = "✍️ Chấm tự luận";
            this.btnEssayGrading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEssayGrading.UseVisualStyleBackColor = true;
            this.btnEssayGrading.Click += new System.EventHandler(this.btnEssayGrading_Click);
            // 
            // btnLiveMonitor
            // 
            this.btnLiveMonitor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLiveMonitor.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnLiveMonitor.FlatAppearance.BorderSize = 0;
            this.btnLiveMonitor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLiveMonitor.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnLiveMonitor.ForeColor = System.Drawing.Color.LightCyan;
            this.btnLiveMonitor.Location = new System.Drawing.Point(0, 0);
            this.btnLiveMonitor.Name = "btnLiveMonitor";
            this.btnLiveMonitor.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnLiveMonitor.Size = new System.Drawing.Size(260, 40);
            this.btnLiveMonitor.TabIndex = 0;
            this.btnLiveMonitor.Text = "📹 Giám sát Live";
            this.btnLiveMonitor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLiveMonitor.UseVisualStyleBackColor = true;
            this.btnLiveMonitor.Click += new System.EventHandler(this.btnLiveMonitor_Click);
            // 
            // btnGroupMonitoring
            // 
            this.btnGroupMonitoring.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGroupMonitoring.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnGroupMonitoring.FlatAppearance.BorderSize = 0;
            this.btnGroupMonitoring.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGroupMonitoring.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnGroupMonitoring.ForeColor = System.Drawing.Color.White;
            this.btnGroupMonitoring.Location = new System.Drawing.Point(0, 395);
            this.btnGroupMonitoring.Name = "btnGroupMonitoring";
            this.btnGroupMonitoring.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnGroupMonitoring.Size = new System.Drawing.Size(260, 50);
            this.btnGroupMonitoring.TabIndex = 6;
            this.btnGroupMonitoring.Text = "⏱️ Giám Sát && Chấm Điểm";
            this.btnGroupMonitoring.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGroupMonitoring.UseVisualStyleBackColor = true;
            this.btnGroupMonitoring.Click += new System.EventHandler(this.btnGroupMonitoring_Click);
            // 
            // pnlSubMenuTesting
            // 
            this.pnlSubMenuTesting.AutoSize = true;
            this.pnlSubMenuTesting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.pnlSubMenuTesting.Controls.Add(this.btnExamList);
            this.pnlSubMenuTesting.Controls.Add(this.btnExamConfig);
            this.pnlSubMenuTesting.Controls.Add(this.btnQuestionBank);
            this.pnlSubMenuTesting.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSubMenuTesting.Location = new System.Drawing.Point(0, 275);
            this.pnlSubMenuTesting.Name = "pnlSubMenuTesting";
            this.pnlSubMenuTesting.Size = new System.Drawing.Size(260, 120);
            this.pnlSubMenuTesting.TabIndex = 5;
            this.pnlSubMenuTesting.Visible = false;
            // 
            // btnExamList
            // 
            this.btnExamList.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExamList.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnExamList.FlatAppearance.BorderSize = 0;
            this.btnExamList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExamList.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnExamList.ForeColor = System.Drawing.Color.LightCyan;
            this.btnExamList.Location = new System.Drawing.Point(0, 80);
            this.btnExamList.Name = "btnExamList";
            this.btnExamList.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnExamList.Size = new System.Drawing.Size(260, 40);
            this.btnExamList.TabIndex = 2;
            this.btnExamList.Text = "📋 Quản lý kỳ thi";
            this.btnExamList.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExamList.UseVisualStyleBackColor = true;
            this.btnExamList.Click += new System.EventHandler(this.btnExamList_Click);
            // 
            // btnExamConfig
            // 
            this.btnExamConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExamConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnExamConfig.FlatAppearance.BorderSize = 0;
            this.btnExamConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExamConfig.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnExamConfig.ForeColor = System.Drawing.Color.LightCyan;
            this.btnExamConfig.Location = new System.Drawing.Point(0, 40);
            this.btnExamConfig.Name = "btnExamConfig";
            this.btnExamConfig.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnExamConfig.Size = new System.Drawing.Size(260, 40);
            this.btnExamConfig.TabIndex = 1;
            this.btnExamConfig.Text = "⚙️ Cấu hình đề thi";
            this.btnExamConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExamConfig.UseVisualStyleBackColor = true;
            this.btnExamConfig.Click += new System.EventHandler(this.btnExamConfig_Click);
            // 
            // btnQuestionBank
            // 
            this.btnQuestionBank.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuestionBank.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnQuestionBank.FlatAppearance.BorderSize = 0;
            this.btnQuestionBank.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuestionBank.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnQuestionBank.ForeColor = System.Drawing.Color.LightCyan;
            this.btnQuestionBank.Location = new System.Drawing.Point(0, 0);
            this.btnQuestionBank.Name = "btnQuestionBank";
            this.btnQuestionBank.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnQuestionBank.Size = new System.Drawing.Size(260, 40);
            this.btnQuestionBank.TabIndex = 0;
            this.btnQuestionBank.Text = "📂 Ngân hàng câu hỏi";
            this.btnQuestionBank.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnQuestionBank.UseVisualStyleBackColor = true;
            this.btnQuestionBank.Click += new System.EventHandler(this.btnQuestionBank_Click);
            // 
            // btnGroupTesting
            // 
            this.btnGroupTesting.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGroupTesting.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnGroupTesting.FlatAppearance.BorderSize = 0;
            this.btnGroupTesting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGroupTesting.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnGroupTesting.ForeColor = System.Drawing.Color.White;
            this.btnGroupTesting.Location = new System.Drawing.Point(0, 225);
            this.btnGroupTesting.Name = "btnGroupTesting";
            this.btnGroupTesting.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnGroupTesting.Size = new System.Drawing.Size(260, 50);
            this.btnGroupTesting.TabIndex = 4;
            this.btnGroupTesting.Text = "📝 Khảo Thí && Đề Thi";
            this.btnGroupTesting.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGroupTesting.UseVisualStyleBackColor = true;
            this.btnGroupTesting.Click += new System.EventHandler(this.btnGroupTesting_Click);
            // 
            // pnlSubMenuCourseDocs
            // 
            this.pnlSubMenuCourseDocs.AutoSize = true;
            this.pnlSubMenuCourseDocs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.pnlSubMenuCourseDocs.Controls.Add(this.btnOnlineClasses);
            this.pnlSubMenuCourseDocs.Controls.Add(this.btnAssignedCourses);
            this.pnlSubMenuCourseDocs.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSubMenuCourseDocs.Location = new System.Drawing.Point(0, 145);
            this.pnlSubMenuCourseDocs.Name = "pnlSubMenuCourseDocs";
            this.pnlSubMenuCourseDocs.Size = new System.Drawing.Size(260, 80);
            this.pnlSubMenuCourseDocs.TabIndex = 3;
            this.pnlSubMenuCourseDocs.Visible = false;
            // 
            // btnOnlineClasses
            // 
            this.btnOnlineClasses.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOnlineClasses.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnOnlineClasses.FlatAppearance.BorderSize = 0;
            this.btnOnlineClasses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOnlineClasses.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnOnlineClasses.ForeColor = System.Drawing.Color.LightCyan;
            this.btnOnlineClasses.Location = new System.Drawing.Point(0, 40);
            this.btnOnlineClasses.Name = "btnOnlineClasses";
            this.btnOnlineClasses.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnOnlineClasses.Size = new System.Drawing.Size(260, 40);
            this.btnOnlineClasses.TabIndex = 1;
            this.btnOnlineClasses.Text = "🌐 Lớp học trực tuyến";
            this.btnOnlineClasses.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOnlineClasses.UseVisualStyleBackColor = true;
            this.btnOnlineClasses.Click += new System.EventHandler(this.btnOnlineClasses_Click);
            // 
            // btnAssignedCourses
            // 
            this.btnAssignedCourses.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAssignedCourses.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAssignedCourses.FlatAppearance.BorderSize = 0;
            this.btnAssignedCourses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAssignedCourses.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnAssignedCourses.ForeColor = System.Drawing.Color.LightCyan;
            this.btnAssignedCourses.Location = new System.Drawing.Point(0, 0);
            this.btnAssignedCourses.Name = "btnAssignedCourses";
            this.btnAssignedCourses.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnAssignedCourses.Size = new System.Drawing.Size(260, 40);
            this.btnAssignedCourses.TabIndex = 0;
            this.btnAssignedCourses.Text = "📖 KH phân công";
            this.btnAssignedCourses.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAssignedCourses.UseVisualStyleBackColor = true;
            this.btnAssignedCourses.Click += new System.EventHandler(this.btnAssignedCourses_Click);
            // 
            // btnGroupCourseDocs
            // 
            this.btnGroupCourseDocs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGroupCourseDocs.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnGroupCourseDocs.FlatAppearance.BorderSize = 0;
            this.btnGroupCourseDocs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGroupCourseDocs.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnGroupCourseDocs.ForeColor = System.Drawing.Color.White;
            this.btnGroupCourseDocs.Location = new System.Drawing.Point(0, 95);
            this.btnGroupCourseDocs.Name = "btnGroupCourseDocs";
            this.btnGroupCourseDocs.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnGroupCourseDocs.Size = new System.Drawing.Size(260, 50);
            this.btnGroupCourseDocs.TabIndex = 2;
            this.btnGroupCourseDocs.Text = "📚 Học Liệu && Lớp Học";
            this.btnGroupCourseDocs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGroupCourseDocs.UseVisualStyleBackColor = true;
            this.btnGroupCourseDocs.Click += new System.EventHandler(this.btnGroupCourseDocs_Click);
            // 
            // btnOverview
            // 
            this.btnOverview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOverview.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnOverview.FlatAppearance.BorderSize = 0;
            this.btnOverview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOverview.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnOverview.ForeColor = System.Drawing.Color.White;
            this.btnOverview.Location = new System.Drawing.Point(0, 45);
            this.btnOverview.Name = "btnOverview";
            this.btnOverview.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnOverview.Size = new System.Drawing.Size(260, 50);
            this.btnOverview.TabIndex = 1;
            this.btnOverview.Text = "🏠 Tổng Quan";
            this.btnOverview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOverview.UseVisualStyleBackColor = true;
            this.btnOverview.Click += new System.EventHandler(this.btnOverview_Click);
            // 
            // pnlLogo
            // 
            this.pnlLogo.Controls.Add(this.lblLogo);
            this.pnlLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogo.Location = new System.Drawing.Point(0, 0);
            this.pnlLogo.Name = "pnlLogo";
            this.pnlLogo.Size = new System.Drawing.Size(260, 45);
            this.pnlLogo.TabIndex = 0;
            // 
            // lblLogo
            // 
            this.lblLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLogo.ForeColor = System.Drawing.Color.White;
            this.lblLogo.Location = new System.Drawing.Point(0, 0);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(260, 45);
            this.lblLogo.TabIndex = 0;
            this.lblLogo.Text = "COURSE GUARD";
            this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(260, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1020, 60);
            this.pnlHeader.TabIndex = 1;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(110, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Tổng Quan";
            // 
            // pnlMainboard
            // 
            this.pnlMainboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.pnlMainboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainboard.Location = new System.Drawing.Point(260, 60);
            this.pnlMainboard.Name = "pnlMainboard";
            this.pnlMainboard.Size = new System.Drawing.Size(1020, 660);
            this.pnlMainboard.TabIndex = 2;
            // 
            // TeacherDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.pnlMainboard);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlSidebar);
            this.Name = "TeacherDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Teacher Dashboard - CourseGuard";
            this.pnlSidebar.ResumeLayout(false);
            this.pnlSidebar.PerformLayout();
            this.pnlSubMenuMonitoring.ResumeLayout(false);
            this.pnlSubMenuTesting.ResumeLayout(false);
            this.pnlSubMenuCourseDocs.ResumeLayout(false);
            this.pnlLogo.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlHeader;
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
        private System.Windows.Forms.Label lblTitle;
    }
}
