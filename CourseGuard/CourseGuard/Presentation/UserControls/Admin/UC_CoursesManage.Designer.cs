namespace CourseGuard.Presentation.UserControls.Admin
{
    partial class UC_CoursesManage
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            tlpMain = new TableLayoutPanel();
            pnlLeftWrapper = new Panel();
            dgvCourses = new DataGridView();
            panelLeftHeader = new Panel();
            lblListTitle = new Label();
            pnlRightWrapper = new Panel();
            grpStudentManage = new GroupBox();
            cboSelectCourse = new ComboBox();
            cboStudent = new ComboBox();
            cboRegStatus = new ComboBox();
            btnAddStudent = new Button();
            btnApproveStudent = new Button();
            btnRemoveStudent = new Button();
            grpCourseInfo = new GroupBox();
            txtCourseName = new TextBox();
            txtDescription = new TextBox();
            cboTeacher = new ComboBox();
            cboStatus = new ComboBox();
            lblStartDate = new Label();
            dtpStartDate = new DateTimePicker();
            lblEndDate = new Label();
            dtpEndDate = new DateTimePicker();
            btnAddCourse = new Button();
            btnUpdateCourse = new Button();
            btnDeleteCourse = new Button();
            tlpMain.SuspendLayout();
            pnlLeftWrapper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCourses).BeginInit();
            panelLeftHeader.SuspendLayout();
            pnlRightWrapper.SuspendLayout();
            grpStudentManage.SuspendLayout();
            grpCourseInfo.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpMain.Controls.Add(pnlLeftWrapper, 0, 0);
            tlpMain.Controls.Add(pnlRightWrapper, 1, 0);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 1;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Size = new Size(1000, 600);
            tlpMain.TabIndex = 0;
            // 
            // pnlLeftWrapper
            // 
            pnlLeftWrapper.Controls.Add(dgvCourses);
            pnlLeftWrapper.Controls.Add(panelLeftHeader);
            pnlLeftWrapper.Dock = DockStyle.Fill;
            pnlLeftWrapper.Location = new Point(3, 3);
            pnlLeftWrapper.Name = "pnlLeftWrapper";
            pnlLeftWrapper.Padding = new Padding(10);
            pnlLeftWrapper.Size = new Size(644, 594);
            pnlLeftWrapper.TabIndex = 0;
            // 
            // dgvCourses
            // 
            dgvCourses.BackgroundColor = Color.White;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(30, 58, 138);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dgvCourses.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvCourses.ColumnHeadersHeight = 35;
            dgvCourses.Dock = DockStyle.Fill;
            dgvCourses.EnableHeadersVisualStyles = false;
            dgvCourses.Location = new Point(10, 50);
            dgvCourses.Name = "dgvCourses";
            dgvCourses.RowHeadersWidth = 51;
            dgvCourses.Size = new Size(624, 534);
            dgvCourses.TabIndex = 0;
            // 
            // panelLeftHeader
            // 
            panelLeftHeader.BackColor = Color.FromArgb(249, 250, 251);
            panelLeftHeader.Controls.Add(lblListTitle);
            panelLeftHeader.Dock = DockStyle.Top;
            panelLeftHeader.Location = new Point(10, 10);
            panelLeftHeader.Name = "panelLeftHeader";
            panelLeftHeader.Size = new Size(624, 40);
            panelLeftHeader.TabIndex = 1;
            // 
            // lblListTitle
            // 
            lblListTitle.Dock = DockStyle.Fill;
            lblListTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblListTitle.ForeColor = Color.FromArgb(17, 24, 39);
            lblListTitle.Location = new Point(0, 0);
            lblListTitle.Name = "lblListTitle";
            lblListTitle.Padding = new Padding(10, 0, 0, 0);
            lblListTitle.Size = new Size(624, 40);
            lblListTitle.TabIndex = 0;
            lblListTitle.Text = "Danh sách khóa học";
            lblListTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlRightWrapper
            // 
            pnlRightWrapper.AutoScroll = true;
            pnlRightWrapper.BackColor = Color.FromArgb(242, 244, 248);
            pnlRightWrapper.Controls.Add(grpStudentManage);
            pnlRightWrapper.Controls.Add(grpCourseInfo);
            pnlRightWrapper.Dock = DockStyle.Fill;
            pnlRightWrapper.Location = new Point(653, 3);
            pnlRightWrapper.Name = "pnlRightWrapper";
            pnlRightWrapper.Padding = new Padding(10);
            pnlRightWrapper.Size = new Size(344, 594);
            pnlRightWrapper.TabIndex = 1;
            // 
            // grpStudentManage
            // 
            grpStudentManage.BackColor = Color.White;
            grpStudentManage.Controls.Add(cboSelectCourse);
            grpStudentManage.Controls.Add(cboStudent);
            grpStudentManage.Controls.Add(cboRegStatus);
            grpStudentManage.Controls.Add(btnAddStudent);
            grpStudentManage.Controls.Add(btnApproveStudent);
            grpStudentManage.Controls.Add(btnRemoveStudent);
            grpStudentManage.Dock = DockStyle.Top;
            grpStudentManage.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpStudentManage.ForeColor = Color.FromArgb(30, 58, 138);
            grpStudentManage.Location = new Point(10, 360);
            grpStudentManage.Name = "grpStudentManage";
            grpStudentManage.Padding = new Padding(15);
            grpStudentManage.Size = new Size(324, 220);
            grpStudentManage.TabIndex = 0;
            grpStudentManage.TabStop = false;
            grpStudentManage.Text = "Quản lý học viên";
            // 
            // cboSelectCourse
            // 
            cboSelectCourse.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboSelectCourse.Font = new Font("Segoe UI", 10F);
            cboSelectCourse.Location = new Point(20, 35);
            cboSelectCourse.Name = "cboSelectCourse";
            cboSelectCourse.Size = new Size(374, 31);
            cboSelectCourse.TabIndex = 0;
            cboSelectCourse.Text = "Chọn khóa học";
            // 
            // cboStudent
            // 
            cboStudent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboStudent.Font = new Font("Segoe UI", 10F);
            cboStudent.Location = new Point(20, 80);
            cboStudent.Name = "cboStudent";
            cboStudent.Size = new Size(374, 31);
            cboStudent.TabIndex = 1;
            cboStudent.Text = "Chọn học viên";
            // 
            // cboRegStatus
            // 
            cboRegStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboRegStatus.Font = new Font("Segoe UI", 10F);
            cboRegStatus.Items.AddRange(new object[] { "Pending", "Approved", "Rejected" });
            cboRegStatus.Location = new Point(20, 125);
            cboRegStatus.Name = "cboRegStatus";
            cboRegStatus.Size = new Size(374, 31);
            cboRegStatus.TabIndex = 2;
            cboRegStatus.Text = "Trạng thái";
            // 
            // btnAddStudent
            // 
            btnAddStudent.BackColor = Color.FromArgb(37, 99, 235);
            btnAddStudent.FlatAppearance.BorderSize = 0;
            btnAddStudent.FlatStyle = FlatStyle.Flat;
            btnAddStudent.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddStudent.ForeColor = Color.White;
            btnAddStudent.Location = new Point(20, 170);
            btnAddStudent.Name = "btnAddStudent";
            btnAddStudent.Size = new Size(80, 35);
            btnAddStudent.TabIndex = 3;
            btnAddStudent.Text = "Thêm";
            btnAddStudent.UseVisualStyleBackColor = false;
            // 
            // btnApproveStudent
            // 
            btnApproveStudent.BackColor = Color.FromArgb(16, 185, 129);
            btnApproveStudent.FlatAppearance.BorderSize = 0;
            btnApproveStudent.FlatStyle = FlatStyle.Flat;
            btnApproveStudent.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnApproveStudent.ForeColor = Color.White;
            btnApproveStudent.Location = new Point(110, 170);
            btnApproveStudent.Name = "btnApproveStudent";
            btnApproveStudent.Size = new Size(80, 35);
            btnApproveStudent.TabIndex = 4;
            btnApproveStudent.Text = "Duyệt";
            btnApproveStudent.UseVisualStyleBackColor = false;
            // 
            // btnRemoveStudent
            // 
            btnRemoveStudent.BackColor = Color.FromArgb(220, 38, 38);
            btnRemoveStudent.FlatAppearance.BorderSize = 0;
            btnRemoveStudent.FlatStyle = FlatStyle.Flat;
            btnRemoveStudent.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRemoveStudent.ForeColor = Color.White;
            btnRemoveStudent.Location = new Point(200, 170);
            btnRemoveStudent.Name = "btnRemoveStudent";
            btnRemoveStudent.Size = new Size(80, 35);
            btnRemoveStudent.TabIndex = 5;
            btnRemoveStudent.Text = "Xóa";
            btnRemoveStudent.UseVisualStyleBackColor = false;
            // 
            // grpCourseInfo
            // 
            grpCourseInfo.BackColor = Color.White;
            grpCourseInfo.Controls.Add(txtCourseName);
            grpCourseInfo.Controls.Add(txtDescription);
            grpCourseInfo.Controls.Add(cboTeacher);
            grpCourseInfo.Controls.Add(cboStatus);
            grpCourseInfo.Controls.Add(lblStartDate);
            grpCourseInfo.Controls.Add(dtpStartDate);
            grpCourseInfo.Controls.Add(lblEndDate);
            grpCourseInfo.Controls.Add(dtpEndDate);
            grpCourseInfo.Controls.Add(btnAddCourse);
            grpCourseInfo.Controls.Add(btnUpdateCourse);
            grpCourseInfo.Controls.Add(btnDeleteCourse);
            grpCourseInfo.Dock = DockStyle.Top;
            grpCourseInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpCourseInfo.ForeColor = Color.FromArgb(30, 58, 138);
            grpCourseInfo.Location = new Point(10, 10);
            grpCourseInfo.Name = "grpCourseInfo";
            grpCourseInfo.Padding = new Padding(15);
            grpCourseInfo.Size = new Size(324, 350);
            grpCourseInfo.TabIndex = 1;
            grpCourseInfo.TabStop = false;
            grpCourseInfo.Text = "Thông tin khóa học";
            // 
            // txtCourseName
            // 
            txtCourseName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCourseName.Font = new Font("Segoe UI", 10F);
            txtCourseName.Location = new Point(20, 30);
            txtCourseName.Name = "txtCourseName";
            txtCourseName.PlaceholderText = "Tên khóa học";
            txtCourseName.Size = new Size(301, 30);
            txtCourseName.TabIndex = 0;
            // 
            // txtDescription
            // 
            txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDescription.Font = new Font("Segoe UI", 10F);
            txtDescription.Location = new Point(20, 70);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.PlaceholderText = "Mô tả khóa học...";
            txtDescription.Size = new Size(301, 60);
            txtDescription.TabIndex = 1;
            // 
            // cboTeacher
            // 
            cboTeacher.Anchor = AnchorStyles.Top | AnchorStyles.Left; // Removed Right anchor to prevent overflow/stretching
            cboTeacher.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTeacher.Font = new Font("Segoe UI", 10F);
            cboTeacher.Location = new Point(20, 140);
            cboTeacher.Name = "cboTeacher";
            cboTeacher.Size = new Size(250, 31); // Reduced width to fit
            cboTeacher.TabIndex = 2;
            // 
            // cboStatus
            // 
            cboStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left; // Match cboTeacher
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Font = new Font("Segoe UI", 10F);
            cboStatus.Items.AddRange(new object[] { "Active", "Closed" });
            cboStatus.Location = new Point(20, 177);
            cboStatus.Name = "cboStatus";
            cboStatus.Size = new Size(250, 31); // Match cboTeacher width
            cboStatus.TabIndex = 3;
            cboStatus.Text = "Trạng thái";
            // 
            // lblStartDate
            // 
            lblStartDate.AutoSize = true;
            lblStartDate.Font = new Font("Segoe UI", 9F);
            lblStartDate.Location = new Point(11, 228);
            lblStartDate.Name = "lblStartDate";
            lblStartDate.Size = new Size(63, 20);
            lblStartDate.TabIndex = 4;
            lblStartDate.Text = "Bắt đầu:";
            // 
            // dtpStartDate
            // 
            dtpStartDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtpStartDate.Font = new Font("Segoe UI", 10F);
            dtpStartDate.Format = DateTimePickerFormat.Short;
            dtpStartDate.Location = new Point(80, 220);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(244, 30);
            dtpStartDate.TabIndex = 5;
            // 
            // lblEndDate
            // 
            lblEndDate.AutoSize = true;
            lblEndDate.Font = new Font("Segoe UI", 9F);
            lblEndDate.Location = new Point(20, 265);
            lblEndDate.Name = "lblEndDate";
            lblEndDate.Size = new Size(66, 20);
            lblEndDate.TabIndex = 6;
            lblEndDate.Text = "Kết thúc:";
            // 
            // dtpEndDate
            // 
            dtpEndDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtpEndDate.Font = new Font("Segoe UI", 10F);
            dtpEndDate.Format = DateTimePickerFormat.Short;
            dtpEndDate.Location = new Point(80, 260);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(314, 30);
            dtpEndDate.TabIndex = 7;
            // 
            // btnAddCourse
            // 
            btnAddCourse.BackColor = Color.FromArgb(37, 99, 235);
            btnAddCourse.FlatAppearance.BorderSize = 0;
            btnAddCourse.FlatStyle = FlatStyle.Flat;
            btnAddCourse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddCourse.ForeColor = Color.White;
            btnAddCourse.Location = new Point(20, 300);
            btnAddCourse.Name = "btnAddCourse";
            btnAddCourse.Size = new Size(80, 35);
            btnAddCourse.TabIndex = 8;
            btnAddCourse.Text = "Thêm";
            btnAddCourse.UseVisualStyleBackColor = false;
            // 
            // btnUpdateCourse
            // 
            btnUpdateCourse.BackColor = Color.FromArgb(245, 158, 11);
            btnUpdateCourse.FlatAppearance.BorderSize = 0;
            btnUpdateCourse.FlatStyle = FlatStyle.Flat;
            btnUpdateCourse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnUpdateCourse.ForeColor = Color.White;
            btnUpdateCourse.Location = new Point(110, 300);
            btnUpdateCourse.Name = "btnUpdateCourse";
            btnUpdateCourse.Size = new Size(80, 35);
            btnUpdateCourse.TabIndex = 9;
            btnUpdateCourse.Text = "Sửa";
            btnUpdateCourse.UseVisualStyleBackColor = false;
            // 
            // btnDeleteCourse
            // 
            btnDeleteCourse.BackColor = Color.FromArgb(220, 38, 38);
            btnDeleteCourse.FlatAppearance.BorderSize = 0;
            btnDeleteCourse.FlatStyle = FlatStyle.Flat;
            btnDeleteCourse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDeleteCourse.ForeColor = Color.White;
            btnDeleteCourse.Location = new Point(200, 300);
            btnDeleteCourse.Name = "btnDeleteCourse";
            btnDeleteCourse.Size = new Size(80, 35);
            btnDeleteCourse.TabIndex = 10;
            btnDeleteCourse.Text = "Xóa";
            btnDeleteCourse.UseVisualStyleBackColor = false;
            // 
            // UC_CoursesManage
            // 
            Controls.Add(tlpMain);
            Name = "UC_CoursesManage";
            Size = new Size(1000, 600);
            tlpMain.ResumeLayout(false);
            pnlLeftWrapper.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCourses).EndInit();
            panelLeftHeader.ResumeLayout(false);
            pnlRightWrapper.ResumeLayout(false);
            grpStudentManage.ResumeLayout(false);
            grpCourseInfo.ResumeLayout(false);
            grpCourseInfo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel pnlLeftWrapper;
        private System.Windows.Forms.Panel pnlRightWrapper;
        private System.Windows.Forms.DataGridView dgvCourses;
        private System.Windows.Forms.Panel panelLeftHeader;
        private System.Windows.Forms.Label lblListTitle;
        
        // Card 1
        private System.Windows.Forms.GroupBox grpCourseInfo;
        private System.Windows.Forms.TextBox txtCourseName;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.ComboBox cboTeacher;
        private System.Windows.Forms.ComboBox cboStatus;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label lblStartDate;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.Button btnAddCourse;
        private System.Windows.Forms.Button btnUpdateCourse;
        private System.Windows.Forms.Button btnDeleteCourse;

        // Card 2
        private System.Windows.Forms.GroupBox grpStudentManage;
        private System.Windows.Forms.ComboBox cboSelectCourse;
        private System.Windows.Forms.ComboBox cboStudent;
        private System.Windows.Forms.ComboBox cboRegStatus;
        private System.Windows.Forms.Button btnAddStudent;
        private System.Windows.Forms.Button btnApproveStudent;
        private System.Windows.Forms.Button btnRemoveStudent;
    }
}
