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
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlLogo = new System.Windows.Forms.Panel();
            this.lblLogoIcon = new System.Windows.Forms.Label();
            this.lblLogoText = new System.Windows.Forms.Label();
            this.btnOverview = new System.Windows.Forms.Button();
            this.btnGroupCourseDocs = new System.Windows.Forms.Button();
            this.pnlSubMenuCourseDocs = new System.Windows.Forms.Panel();
            this.btnAssignedCourses = new System.Windows.Forms.Button();
            this.btnOnlineClasses = new System.Windows.Forms.Button();
            this.btnGroupTesting = new System.Windows.Forms.Button();
            this.pnlSubMenuTesting = new System.Windows.Forms.Panel();
            this.btnQuestionBank = new System.Windows.Forms.Button();
            this.btnExamConfig = new System.Windows.Forms.Button();
            this.btnExamList = new System.Windows.Forms.Button();
            this.btnGroupMonitoring = new System.Windows.Forms.Button();
            this.pnlSubMenuMonitoring = new System.Windows.Forms.Panel();
            this.btnLiveMonitor = new System.Windows.Forms.Button();
            this.btnEssayGrading = new System.Windows.Forms.Button();
            this.btnScoreManagement = new System.Windows.Forms.Button();
            this.btnNotifications = new System.Windows.Forms.Button();
            this.pnlSidebarBottom = new System.Windows.Forms.Panel();
            this.pnlDivider = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.pnlMainboard = new System.Windows.Forms.Panel();

            this.pnlSidebar.SuspendLayout();
            this.pnlLogo.SuspendLayout();
            this.pnlSubMenuCourseDocs.SuspendLayout();
            this.pnlSubMenuTesting.SuspendLayout();
            this.pnlSubMenuMonitoring.SuspendLayout();
            this.pnlSidebarBottom.SuspendLayout();
            this.SuspendLayout();

            // =========================================================
            // pnlSidebar  (toàn bộ sidebar bên trái)
            // =========================================================
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.pnlSidebar.Controls.Add(this.pnlSidebarBottom);   // đáy (logout)
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
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(260, 900);
            this.pnlSidebar.TabIndex = 0;

            // =========================================================
            // pnlLogo  (logo phần trên cùng sidebar)
            // =========================================================
            this.pnlLogo.BackColor = System.Drawing.Color.FromArgb(11, 17, 28);
            this.pnlLogo.Controls.Add(this.lblLogoIcon);
            this.pnlLogo.Controls.Add(this.lblLogoText);
            this.pnlLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogo.Name = "pnlLogo";
            this.pnlLogo.Size = new System.Drawing.Size(260, 85);
            this.pnlLogo.TabIndex = 0;

            // lblLogoIcon  (biểu tượng khiên 🛡️)
            this.lblLogoIcon.AutoSize = false;
            this.lblLogoIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblLogoIcon.Font = new System.Drawing.Font("Segoe UI Emoji", 20F);
            this.lblLogoIcon.ForeColor = System.Drawing.Color.FromArgb(99, 179, 237);
            this.lblLogoIcon.Name = "lblLogoIcon";
            this.lblLogoIcon.Size = new System.Drawing.Size(50, 85);
            this.lblLogoIcon.Text = "🛡";
            this.lblLogoIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLogoIcon.TabIndex = 0;

            // lblLogoText  (tên ứng dụng)
            this.lblLogoText.AutoSize = true;
            this.lblLogoText.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblLogoText.ForeColor = System.Drawing.Color.White;
            this.lblLogoText.Location = new System.Drawing.Point(55, 28);
            this.lblLogoText.Name = "lblLogoText";
            this.lblLogoText.Text = "CourseGuard";
            this.lblLogoText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblLogoText.TabIndex = 1;

            // =========================================================
            // btnOverview  — Tổng Quan
            // =========================================================
            this.btnOverview.BackColor = System.Drawing.Color.Transparent;
            this.btnOverview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOverview.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnOverview.FlatAppearance.BorderSize = 0;
            this.btnOverview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOverview.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnOverview.ForeColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.btnOverview.Name = "btnOverview";
            this.btnOverview.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.btnOverview.Size = new System.Drawing.Size(260, 48);
            this.btnOverview.TabIndex = 1;
            this.btnOverview.Text = "🏠  Tổng Quan";
            this.btnOverview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOverview.UseVisualStyleBackColor = false;
            this.btnOverview.Click += new System.EventHandler(this.btnOverview_Click);

            // =========================================================
            // btnGroupCourseDocs  — Học Liệu & Lớp Học  (nhóm cha)
            // =========================================================
            this.btnGroupCourseDocs.BackColor = System.Drawing.Color.Transparent;
            this.btnGroupCourseDocs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGroupCourseDocs.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnGroupCourseDocs.FlatAppearance.BorderSize = 0;
            this.btnGroupCourseDocs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGroupCourseDocs.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnGroupCourseDocs.ForeColor = System.Drawing.Color.White;
            this.btnGroupCourseDocs.Name = "btnGroupCourseDocs";
            this.btnGroupCourseDocs.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.btnGroupCourseDocs.Size = new System.Drawing.Size(260, 48);
            this.btnGroupCourseDocs.TabIndex = 2;
            this.btnGroupCourseDocs.Text = "📚  Học Liệu && Lớp Học  ›";
            this.btnGroupCourseDocs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGroupCourseDocs.UseVisualStyleBackColor = false;
            this.btnGroupCourseDocs.Click += new System.EventHandler(this.btnGroupCourseDocs_Click);

            // =========================================================
            // pnlSubMenuCourseDocs  (sub-menu ẩn, accordion)
            // =========================================================
            this.pnlSubMenuCourseDocs.AutoSize = true;
            this.pnlSubMenuCourseDocs.BackColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.pnlSubMenuCourseDocs.Controls.Add(this.btnOnlineClasses);
            this.pnlSubMenuCourseDocs.Controls.Add(this.btnAssignedCourses);
            this.pnlSubMenuCourseDocs.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSubMenuCourseDocs.Name = "pnlSubMenuCourseDocs";
            this.pnlSubMenuCourseDocs.Size = new System.Drawing.Size(260, 80);
            this.pnlSubMenuCourseDocs.TabIndex = 3;
            this.pnlSubMenuCourseDocs.Visible = false;   // ẩn lúc thiết kế; runtime HideAllSubMenus() xử lý

            // btnAssignedCourses
            this.btnAssignedCourses.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAssignedCourses.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAssignedCourses.FlatAppearance.BorderSize = 0;
            this.btnAssignedCourses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAssignedCourses.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnAssignedCourses.ForeColor = System.Drawing.Color.FromArgb(147, 210, 255);
            this.btnAssignedCourses.Name = "btnAssignedCourses";
            this.btnAssignedCourses.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnAssignedCourses.Size = new System.Drawing.Size(260, 40);
            this.btnAssignedCourses.TabIndex = 0;
            this.btnAssignedCourses.Text = "   📖  Khóa học phân công";
            this.btnAssignedCourses.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAssignedCourses.UseVisualStyleBackColor = true;
            this.btnAssignedCourses.Click += new System.EventHandler(this.btnAssignedCourses_Click);

            // btnOnlineClasses
            this.btnOnlineClasses.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOnlineClasses.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnOnlineClasses.FlatAppearance.BorderSize = 0;
            this.btnOnlineClasses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOnlineClasses.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnOnlineClasses.ForeColor = System.Drawing.Color.FromArgb(147, 210, 255);
            this.btnOnlineClasses.Name = "btnOnlineClasses";
            this.btnOnlineClasses.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnOnlineClasses.Size = new System.Drawing.Size(260, 40);
            this.btnOnlineClasses.TabIndex = 1;
            this.btnOnlineClasses.Text = "   🌐  Lớp học trực tuyến";
            this.btnOnlineClasses.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOnlineClasses.UseVisualStyleBackColor = true;
            this.btnOnlineClasses.Click += new System.EventHandler(this.btnOnlineClasses_Click);

            // =========================================================
            // btnGroupTesting  — Khảo Thí & Đề Thi  (nhóm cha)
            // =========================================================
            this.btnGroupTesting.BackColor = System.Drawing.Color.Transparent;
            this.btnGroupTesting.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGroupTesting.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnGroupTesting.FlatAppearance.BorderSize = 0;
            this.btnGroupTesting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGroupTesting.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnGroupTesting.ForeColor = System.Drawing.Color.White;
            this.btnGroupTesting.Name = "btnGroupTesting";
            this.btnGroupTesting.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.btnGroupTesting.Size = new System.Drawing.Size(260, 48);
            this.btnGroupTesting.TabIndex = 4;
            this.btnGroupTesting.Text = "📝  Khảo Thí && Đề Thi  ›";
            this.btnGroupTesting.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGroupTesting.UseVisualStyleBackColor = false;
            this.btnGroupTesting.Click += new System.EventHandler(this.btnGroupTesting_Click);

            // =========================================================
            // pnlSubMenuTesting
            // =========================================================
            this.pnlSubMenuTesting.AutoSize = true;
            this.pnlSubMenuTesting.BackColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.pnlSubMenuTesting.Controls.Add(this.btnExamList);
            this.pnlSubMenuTesting.Controls.Add(this.btnExamConfig);
            this.pnlSubMenuTesting.Controls.Add(this.btnQuestionBank);
            this.pnlSubMenuTesting.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSubMenuTesting.Name = "pnlSubMenuTesting";
            this.pnlSubMenuTesting.Size = new System.Drawing.Size(260, 120);
            this.pnlSubMenuTesting.TabIndex = 5;
            this.pnlSubMenuTesting.Visible = false;

            // btnQuestionBank
            this.btnQuestionBank.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuestionBank.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnQuestionBank.FlatAppearance.BorderSize = 0;
            this.btnQuestionBank.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuestionBank.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnQuestionBank.ForeColor = System.Drawing.Color.FromArgb(147, 210, 255);
            this.btnQuestionBank.Name = "btnQuestionBank";
            this.btnQuestionBank.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnQuestionBank.Size = new System.Drawing.Size(260, 40);
            this.btnQuestionBank.TabIndex = 0;
            this.btnQuestionBank.Text = "   📂  Ngân hàng câu hỏi";
            this.btnQuestionBank.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnQuestionBank.UseVisualStyleBackColor = true;
            this.btnQuestionBank.Click += new System.EventHandler(this.btnQuestionBank_Click);

            // btnExamConfig
            this.btnExamConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExamConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnExamConfig.FlatAppearance.BorderSize = 0;
            this.btnExamConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExamConfig.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnExamConfig.ForeColor = System.Drawing.Color.FromArgb(147, 210, 255);
            this.btnExamConfig.Name = "btnExamConfig";
            this.btnExamConfig.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnExamConfig.Size = new System.Drawing.Size(260, 40);
            this.btnExamConfig.TabIndex = 1;
            this.btnExamConfig.Text = "   ⚙️  Cấu hình đề thi";
            this.btnExamConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExamConfig.UseVisualStyleBackColor = true;
            this.btnExamConfig.Click += new System.EventHandler(this.btnExamConfig_Click);

            // btnExamList
            this.btnExamList.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExamList.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnExamList.FlatAppearance.BorderSize = 0;
            this.btnExamList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExamList.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnExamList.ForeColor = System.Drawing.Color.FromArgb(147, 210, 255);
            this.btnExamList.Name = "btnExamList";
            this.btnExamList.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnExamList.Size = new System.Drawing.Size(260, 40);
            this.btnExamList.TabIndex = 2;
            this.btnExamList.Text = "   📋  Quản lý kỳ thi";
            this.btnExamList.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExamList.UseVisualStyleBackColor = true;
            this.btnExamList.Click += new System.EventHandler(this.btnExamList_Click);

            // =========================================================
            // btnGroupMonitoring  — Giám Sát & Chấm Điểm  (nhóm cha)
            // =========================================================
            this.btnGroupMonitoring.BackColor = System.Drawing.Color.Transparent;
            this.btnGroupMonitoring.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGroupMonitoring.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnGroupMonitoring.FlatAppearance.BorderSize = 0;
            this.btnGroupMonitoring.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGroupMonitoring.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnGroupMonitoring.ForeColor = System.Drawing.Color.White;
            this.btnGroupMonitoring.Name = "btnGroupMonitoring";
            this.btnGroupMonitoring.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.btnGroupMonitoring.Size = new System.Drawing.Size(260, 48);
            this.btnGroupMonitoring.TabIndex = 6;
            this.btnGroupMonitoring.Text = "⏱️  Giám Sát && Chấm Điểm  ›";
            this.btnGroupMonitoring.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGroupMonitoring.UseVisualStyleBackColor = false;
            this.btnGroupMonitoring.Click += new System.EventHandler(this.btnGroupMonitoring_Click);

            // =========================================================
            // pnlSubMenuMonitoring
            // =========================================================
            this.pnlSubMenuMonitoring.AutoSize = true;
            this.pnlSubMenuMonitoring.BackColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.pnlSubMenuMonitoring.Controls.Add(this.btnScoreManagement);
            this.pnlSubMenuMonitoring.Controls.Add(this.btnEssayGrading);
            this.pnlSubMenuMonitoring.Controls.Add(this.btnLiveMonitor);
            this.pnlSubMenuMonitoring.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSubMenuMonitoring.Name = "pnlSubMenuMonitoring";
            this.pnlSubMenuMonitoring.Size = new System.Drawing.Size(260, 120);
            this.pnlSubMenuMonitoring.TabIndex = 7;
            this.pnlSubMenuMonitoring.Visible = false;

            // btnLiveMonitor
            this.btnLiveMonitor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLiveMonitor.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnLiveMonitor.FlatAppearance.BorderSize = 0;
            this.btnLiveMonitor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLiveMonitor.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnLiveMonitor.ForeColor = System.Drawing.Color.FromArgb(147, 210, 255);
            this.btnLiveMonitor.Name = "btnLiveMonitor";
            this.btnLiveMonitor.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnLiveMonitor.Size = new System.Drawing.Size(260, 40);
            this.btnLiveMonitor.TabIndex = 0;
            this.btnLiveMonitor.Text = "   📹  Giám sát Live";
            this.btnLiveMonitor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLiveMonitor.UseVisualStyleBackColor = true;
            this.btnLiveMonitor.Click += new System.EventHandler(this.btnLiveMonitor_Click);

            // btnEssayGrading
            this.btnEssayGrading.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEssayGrading.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnEssayGrading.FlatAppearance.BorderSize = 0;
            this.btnEssayGrading.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEssayGrading.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnEssayGrading.ForeColor = System.Drawing.Color.FromArgb(147, 210, 255);
            this.btnEssayGrading.Name = "btnEssayGrading";
            this.btnEssayGrading.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnEssayGrading.Size = new System.Drawing.Size(260, 40);
            this.btnEssayGrading.TabIndex = 1;
            this.btnEssayGrading.Text = "   ✍️  Chấm tự luận";
            this.btnEssayGrading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEssayGrading.UseVisualStyleBackColor = true;
            this.btnEssayGrading.Click += new System.EventHandler(this.btnEssayGrading_Click);

            // btnScoreManagement
            this.btnScoreManagement.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnScoreManagement.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnScoreManagement.FlatAppearance.BorderSize = 0;
            this.btnScoreManagement.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScoreManagement.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnScoreManagement.ForeColor = System.Drawing.Color.FromArgb(147, 210, 255);
            this.btnScoreManagement.Name = "btnScoreManagement";
            this.btnScoreManagement.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.btnScoreManagement.Size = new System.Drawing.Size(260, 40);
            this.btnScoreManagement.TabIndex = 2;
            this.btnScoreManagement.Text = "   📊  Quản lý điểm số";
            this.btnScoreManagement.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnScoreManagement.UseVisualStyleBackColor = true;
            this.btnScoreManagement.Click += new System.EventHandler(this.btnScoreManagement_Click);

            // =========================================================
            // btnNotifications  — Thông Báo
            // =========================================================
            this.btnNotifications.BackColor = System.Drawing.Color.Transparent;
            this.btnNotifications.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNotifications.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNotifications.FlatAppearance.BorderSize = 0;
            this.btnNotifications.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNotifications.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnNotifications.ForeColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.btnNotifications.Name = "btnNotifications";
            this.btnNotifications.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.btnNotifications.Size = new System.Drawing.Size(260, 48);
            this.btnNotifications.TabIndex = 8;
            this.btnNotifications.Text = "🔔  Trung Tâm Thông Báo";
            this.btnNotifications.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNotifications.UseVisualStyleBackColor = false;
            this.btnNotifications.Click += new System.EventHandler(this.btnNotifications_Click);

            // =========================================================
            // pnlSidebarBottom  — chứa đường kẻ phân cách + nút Đăng xuất
            // =========================================================
            this.pnlSidebarBottom.BackColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.pnlSidebarBottom.Controls.Add(this.btnLogout);
            this.pnlSidebarBottom.Controls.Add(this.pnlDivider);
            this.pnlSidebarBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSidebarBottom.Name = "pnlSidebarBottom";
            this.pnlSidebarBottom.Size = new System.Drawing.Size(260, 65);
            this.pnlSidebarBottom.TabIndex = 9;

            // pnlDivider  (đường kẻ ngang mỏng)
            this.pnlDivider.BackColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.pnlDivider.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDivider.Height = 1;
            this.pnlDivider.Name = "pnlDivider";
            this.pnlDivider.TabIndex = 0;

            // btnLogout
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnLogout.ForeColor = System.Drawing.Color.FromArgb(252, 165, 165);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.btnLogout.Size = new System.Drawing.Size(260, 64);
            this.btnLogout.TabIndex = 1;
            this.btnLogout.Text = "🚪  Đăng Xuất";
            this.btnLogout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            // =========================================================
            // pnlMainboard  — vùng nội dung chính (bên phải sidebar)
            // =========================================================
            this.pnlMainboard.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.pnlMainboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainboard.Name = "pnlMainboard";
            this.pnlMainboard.TabIndex = 1;

            // =========================================================
            // TeacherDashboard  (Form chính)
            // =========================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.ClientSize = new System.Drawing.Size(1380, 860);
            this.Controls.Add(this.pnlMainboard);   // Fill — thêm trước
            this.Controls.Add(this.pnlSidebar);     // Left — thêm sau để đẩy mainboard sang phải
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1000, 650);
            this.Name = "TeacherDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Teacher Dashboard — CourseGuard";

            this.pnlSidebar.ResumeLayout(false);
            this.pnlLogo.ResumeLayout(false);
            this.pnlSubMenuCourseDocs.ResumeLayout(false);
            this.pnlSubMenuTesting.ResumeLayout(false);
            this.pnlSubMenuMonitoring.ResumeLayout(false);
            this.pnlSidebarBottom.ResumeLayout(false);
            this.ResumeLayout(false);
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
