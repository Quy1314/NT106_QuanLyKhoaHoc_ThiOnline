namespace CourseGuard.UserControls.Admin
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
            System.Windows.Forms.DataGridViewCellStyle headerStyle = new System.Windows.Forms.DataGridViewCellStyle();

            tlpMain = new System.Windows.Forms.TableLayoutPanel();
            pnlLeftWrapper = new System.Windows.Forms.Panel();
            dgvCourses = new System.Windows.Forms.DataGridView();
            panelLeftHeader = new System.Windows.Forms.Panel();
            lblListTitle = new System.Windows.Forms.Label();
            
            pnlRightWrapper = new System.Windows.Forms.Panel();
            
            // GroupBoxes
            grpCourseInfo = new System.Windows.Forms.GroupBox();
            grpStudentManage = new System.Windows.Forms.GroupBox();

            // Controls Card 1
            dtpEndDate = new System.Windows.Forms.DateTimePicker();
            dtpStartDate = new System.Windows.Forms.DateTimePicker();
            cboStatus = new System.Windows.Forms.ComboBox();
            cboTeacher = new System.Windows.Forms.ComboBox();
            txtDescription = new System.Windows.Forms.TextBox();
            txtCourseName = new System.Windows.Forms.TextBox();
            lblEndDate = new System.Windows.Forms.Label();
            lblStartDate = new System.Windows.Forms.Label();
            btnDeleteCourse = new System.Windows.Forms.Button();
            btnUpdateCourse = new System.Windows.Forms.Button();
            btnAddCourse = new System.Windows.Forms.Button();
            
            // Controls Card 2
            btnRemoveStudent = new System.Windows.Forms.Button();
            btnApproveStudent = new System.Windows.Forms.Button();
            btnAddStudent = new System.Windows.Forms.Button();
            cboRegStatus = new System.Windows.Forms.ComboBox();
            cboStudent = new System.Windows.Forms.ComboBox();
            cboSelectCourse = new System.Windows.Forms.ComboBox();

            ((System.ComponentModel.ISupportInitialize)(dgvCourses)).BeginInit();
            tlpMain.SuspendLayout();
            pnlLeftWrapper.SuspendLayout();
            panelLeftHeader.SuspendLayout();
            pnlRightWrapper.SuspendLayout();
            grpCourseInfo.SuspendLayout();
            grpStudentManage.SuspendLayout();
            SuspendLayout();

            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tlpMain.Controls.Add(pnlLeftWrapper, 0, 0);
            tlpMain.Controls.Add(pnlRightWrapper, 1, 0);
            tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMain.Location = new System.Drawing.Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 1;
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpMain.Size = new System.Drawing.Size(1000, 600);
            tlpMain.TabIndex = 0;

            // 
            // pnlLeftWrapper
            // 
            pnlLeftWrapper.Controls.Add(dgvCourses);
            pnlLeftWrapper.Controls.Add(panelLeftHeader);
            pnlLeftWrapper.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlLeftWrapper.Padding = new System.Windows.Forms.Padding(10);
            pnlLeftWrapper.Name = "pnlLeftWrapper";

            // 
            // panelLeftHeader
            // 
            panelLeftHeader.Controls.Add(lblListTitle);
            panelLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            panelLeftHeader.Height = 40;
            panelLeftHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            panelLeftHeader.Name = "panelLeftHeader";
            
            lblListTitle.Text = "Danh sách khóa học";
            lblListTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblListTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            lblListTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblListTitle.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            lblListTitle.Name = "lblListTitle";

            // 
            // dgvCourses
            // 
            dgvCourses.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvCourses.BackgroundColor = System.Drawing.Color.White;
            dgvCourses.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            dgvCourses.ColumnHeadersHeight = 35;
            dgvCourses.EnableHeadersVisualStyles = false;
            
            headerStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(138))))); // Navy Blue
            headerStyle.ForeColor = System.Drawing.Color.White;
            headerStyle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dgvCourses.ColumnHeadersDefaultCellStyle = headerStyle;
            dgvCourses.Name = "dgvCourses";

            // 
            // pnlRightWrapper
            // 
            pnlRightWrapper.Controls.Add(grpStudentManage); // Add bottom first (Top Docking Stack) ?? No, Add Top First.
            // WinForms Dock=Top stacks from bottom up if added in reverse order?
            // Actually: The last control added with Dock=Top is at the top? No, usually first added is top.
            // Let's add grpCourseInfo first, then grpStudentManage.
            // Wait, standard behavior: First added (0 index) takes priority?
            // Let's rely on BringToFront if needed. Safe bet: controls added.
            pnlRightWrapper.Controls.Add(grpStudentManage);
            pnlRightWrapper.Controls.Add(grpCourseInfo); // Add CourseInfo last so it's at Top? Let's check logic.
            // Standard: Last Added is at bottom of Z-order but Dock=Top fills from Top. 
            // Usually Control.Add(A); Control.Add(B); -> B is Top if both Dock=Top.
            // Let's set specific Dock Padding to separate them.
            
            pnlRightWrapper.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlRightWrapper.Padding = new System.Windows.Forms.Padding(10);
            pnlRightWrapper.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            pnlRightWrapper.Name = "pnlRightWrapper";
            pnlRightWrapper.AutoScroll = true; // IMPORTANT for small screens

            // 
            // grpCourseInfo (Card 1)
            // 
            grpCourseInfo.Text = "Thông tin khóa học";
            grpCourseInfo.Dock = System.Windows.Forms.DockStyle.Top;
            grpCourseInfo.Height = 310; 
            grpCourseInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            grpCourseInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(138)))));
            grpCourseInfo.BackColor = System.Drawing.Color.White;
            grpCourseInfo.Padding = new System.Windows.Forms.Padding(15);
            grpCourseInfo.Name = "grpCourseInfo";

            // Fields Card 1 (Using Anchor for responsiveness)
            
            // Name
            txtCourseName.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtCourseName.PlaceholderText = "Tên khóa học";
            txtCourseName.Location = new System.Drawing.Point(20, 30);
            txtCourseName.Size = new System.Drawing.Size(250, 30); // Initial size small
            txtCourseName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtCourseName.Name = "txtCourseName";

            // Description
            txtDescription.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtDescription.PlaceholderText = "Mô tả khóa học...";
            txtDescription.Location = new System.Drawing.Point(20, 70);
            txtDescription.Size = new System.Drawing.Size(250, 60);
            txtDescription.Multiline = true;
            txtDescription.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtDescription.Name = "txtDescription";

            // Teacher
            cboTeacher.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboTeacher.Text = "Giáo viên phụ trách";
            cboTeacher.Location = new System.Drawing.Point(20, 140);
            cboTeacher.Size = new System.Drawing.Size(250, 30);
            cboTeacher.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboTeacher.Name = "cboTeacher";

            // Status - Let's place below Teacher if width is constrained?
            // Or use Anchor Right? 
            // If the panel is 30%, it might be narrow (300px). Placing side-by-side relies on Width > 400.
            // Safest: Vertical Stack for right panel inputs.
            cboStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboStatus.Text = "Trạng thái";
            cboStatus.Location = new System.Drawing.Point(20, 180); // Below Teacher
            cboStatus.Size = new System.Drawing.Size(250, 30);
            cboStatus.Items.AddRange(new object[] { "Active", "Closed" });
            cboStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboStatus.Name = "cboStatus";

            // Date Pickers - Stack vertically for safety
            lblStartDate.Text = "Bắt đầu:";
            lblStartDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStartDate.Location = new System.Drawing.Point(20, 225);
            lblStartDate.AutoSize = true;

            dtpStartDate.Font = new System.Drawing.Font("Segoe UI", 10F);
            dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpStartDate.Location = new System.Drawing.Point(80, 220); // Relative
            dtpStartDate.Size = new System.Drawing.Size(190, 30);
            dtpStartDate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dtpStartDate.Name = "dtpStartDate";

            // End Date - Below Start Date
            lblEndDate.Text = "Kết thúc:";
            lblEndDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblEndDate.Location = new System.Drawing.Point(20, 265);
            lblEndDate.AutoSize = true;

            dtpEndDate.Font = new System.Drawing.Font("Segoe UI", 10F);
            dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpEndDate.Location = new System.Drawing.Point(80, 260);
            dtpEndDate.Size = new System.Drawing.Size(190, 30);
            dtpEndDate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dtpEndDate.Name = "dtpEndDate";
            
            // Update Card 1 Height since we stacked items vertically
            grpCourseInfo.Height = 350; // Increased

            // Buttons Card 1 - Bottom of Card
            // Anchored Bottom Right? Or just statically placed?
            // Let's place them in a small FlowLayout or just manually.
            btnAddCourse.Text = "Thêm";
            btnAddCourse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            btnAddCourse.ForeColor = System.Drawing.Color.White;
            btnAddCourse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddCourse.FlatAppearance.BorderSize = 0;
            btnAddCourse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnAddCourse.Location = new System.Drawing.Point(20, 300);
            btnAddCourse.Size = new System.Drawing.Size(80, 35);
            btnAddCourse.Name = "btnAddCourse";

            btnUpdateCourse.Text = "Sửa";
            btnUpdateCourse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(158)))), ((int)(((byte)(11)))));
            btnUpdateCourse.ForeColor = System.Drawing.Color.White;
            btnUpdateCourse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnUpdateCourse.FlatAppearance.BorderSize = 0;
            btnUpdateCourse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnUpdateCourse.Location = new System.Drawing.Point(110, 300);
            btnUpdateCourse.Size = new System.Drawing.Size(80, 35);
            btnUpdateCourse.Name = "btnUpdateCourse";

            btnDeleteCourse.Text = "Xóa";
            btnDeleteCourse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            btnDeleteCourse.ForeColor = System.Drawing.Color.White;
            btnDeleteCourse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDeleteCourse.FlatAppearance.BorderSize = 0;
            btnDeleteCourse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDeleteCourse.Location = new System.Drawing.Point(200, 300);
            btnDeleteCourse.Size = new System.Drawing.Size(80, 35);
            btnDeleteCourse.Name = "btnDeleteCourse";


            grpCourseInfo.Controls.AddRange(new System.Windows.Forms.Control[] {
                txtCourseName, txtDescription, cboTeacher, cboStatus,
                lblStartDate, dtpStartDate, lblEndDate, dtpEndDate,
                btnAddCourse, btnUpdateCourse, btnDeleteCourse
            });


            // 
            // grpStudentManage (Card 2)
            // 
            grpStudentManage.Text = "Quản lý học viên";
            grpStudentManage.Dock = System.Windows.Forms.DockStyle.Top; // Stack below
            grpStudentManage.Height = 220; // Increased
            grpStudentManage.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            grpStudentManage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(138)))));
            grpStudentManage.BackColor = System.Drawing.Color.White;
            grpStudentManage.Padding = new System.Windows.Forms.Padding(15);
            grpStudentManage.Name = "grpStudentManage";

            // Fields Card 2
            cboSelectCourse.Text = "Chọn khóa học";
            cboSelectCourse.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboSelectCourse.Location = new System.Drawing.Point(20, 35);
            cboSelectCourse.Size = new System.Drawing.Size(250, 30);
            cboSelectCourse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboSelectCourse.Name = "cboSelectCourse";

            cboStudent.Text = "Chọn học viên";
            cboStudent.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboStudent.Location = new System.Drawing.Point(20, 80);
            cboStudent.Size = new System.Drawing.Size(250, 30);
            cboStudent.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboStudent.Name = "cboStudent";

            cboRegStatus.Text = "Trạng thái";
            cboRegStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboRegStatus.Location = new System.Drawing.Point(20, 125);
            cboRegStatus.Size = new System.Drawing.Size(250, 30);
            cboRegStatus.Items.AddRange(new object[] { "Pending", "Approved", "Rejected" });
            cboRegStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboRegStatus.Name = "cboRegStatus";

            // Buttons Card 2
            btnAddStudent.Text = "Thêm";
            btnAddStudent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            btnAddStudent.ForeColor = System.Drawing.Color.White;
            btnAddStudent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddStudent.FlatAppearance.BorderSize = 0;
            btnAddStudent.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnAddStudent.Location = new System.Drawing.Point(20, 170);
            btnAddStudent.Size = new System.Drawing.Size(80, 35);
            btnAddStudent.Name = "btnAddStudent";

            btnApproveStudent.Text = "Duyệt";
            btnApproveStudent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            btnApproveStudent.ForeColor = System.Drawing.Color.White;
            btnApproveStudent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnApproveStudent.FlatAppearance.BorderSize = 0;
            btnApproveStudent.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnApproveStudent.Location = new System.Drawing.Point(110, 170);
            btnApproveStudent.Size = new System.Drawing.Size(80, 35);
            btnApproveStudent.Name = "btnApproveStudent";

            btnRemoveStudent.Text = "Xóa";
            btnRemoveStudent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            btnRemoveStudent.ForeColor = System.Drawing.Color.White;
            btnRemoveStudent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRemoveStudent.FlatAppearance.BorderSize = 0;
            btnRemoveStudent.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnRemoveStudent.Location = new System.Drawing.Point(200, 170);
            btnRemoveStudent.Size = new System.Drawing.Size(80, 35);
            btnRemoveStudent.Name = "btnRemoveStudent";

            grpStudentManage.Controls.AddRange(new System.Windows.Forms.Control[] {
                cboSelectCourse, cboStudent, cboRegStatus,
                btnAddStudent, btnApproveStudent, btnRemoveStudent
            });
            

            Controls.Add(tlpMain);
            Name = "UC_CoursesManage";
            Size = new System.Drawing.Size(1000, 600);
            
            ((System.ComponentModel.ISupportInitialize)(dgvCourses)).EndInit();
            tlpMain.ResumeLayout(false);
            pnlLeftWrapper.ResumeLayout(false);
            panelLeftHeader.ResumeLayout(false);
            pnlRightWrapper.ResumeLayout(false);
            grpCourseInfo.ResumeLayout(false);
            grpCourseInfo.PerformLayout();
            grpStudentManage.ResumeLayout(false);
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
