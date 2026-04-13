namespace CourseGuard.Frontend.UserControls.Teacher
{
    partial class UC_ExamManagement
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlAction;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnAddExam;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.DataGridView dgvExams;

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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblTitle = new Label();
            pnlAction = new Panel();
            btnAddExam = new Button();
            txtSearch = new TextBox();
            pnlContent = new Panel();
            dgvExams = new DataGridView();
            pnlHeader.SuspendLayout();
            pnlAction.SuspendLayout();
            pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvExams).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(23, 10, 23, 10);
            pnlHeader.Size = new Size(1120, 60);
            pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Dock = DockStyle.Left;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(23, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(252, 41);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "QUẢN LÝ KỲ THI";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlAction
            // 
            pnlAction.Controls.Add(btnAddExam);
            pnlAction.Controls.Add(txtSearch);
            pnlAction.Dock = DockStyle.Top;
            pnlAction.Location = new Point(0, 60);
            pnlAction.Name = "pnlAction";
            pnlAction.Size = new Size(1120, 50);
            pnlAction.TabIndex = 1;
            // 
            // btnAddExam
            // 
            btnAddExam.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddExam.Cursor = Cursors.Hand;
            btnAddExam.FlatStyle = FlatStyle.Flat;
            btnAddExam.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddExam.Location = new Point(947, 5);
            btnAddExam.Name = "btnAddExam";
            btnAddExam.Size = new Size(150, 40);
            btnAddExam.TabIndex = 1;
            btnAddExam.Text = "Thêm kỳ thi";
            btnAddExam.UseVisualStyleBackColor = true;
            // 
            // txtSearch
            // 
            txtSearch.Font = new Font("Segoe UI", 10F);
            txtSearch.Location = new Point(23, 10);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Tìm kiếm kỳ thi...";
            txtSearch.Size = new Size(306, 30);
            txtSearch.TabIndex = 0;
            // 
            // pnlContent
            // 
            pnlContent.Controls.Add(dgvExams);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 110);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(23, 10, 23, 20);
            pnlContent.Size = new Size(1120, 717);
            pnlContent.TabIndex = 2;
            // 
            // dgvExams
            // 
            dgvExams.AllowUserToAddRows = false;
            dgvExams.AllowUserToDeleteRows = false;
            dgvExams.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvExams.BackgroundColor = Color.White;
            dgvExams.BorderStyle = BorderStyle.None;
            dgvExams.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvExams.Dock = DockStyle.Fill;
            dgvExams.Location = new Point(23, 10);
            dgvExams.Name = "dgvExams";
            dgvExams.ReadOnly = true;
            dgvExams.RowHeadersVisible = false;
            dgvExams.RowHeadersWidth = 51;
            dgvExams.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvExams.Size = new Size(1074, 687);
            dgvExams.TabIndex = 0;
            // 
            // UC_ExamManagement
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            Controls.Add(pnlContent);
            Controls.Add(pnlAction);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 10F);
            Name = "UC_ExamManagement";
            Size = new Size(1120, 827);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlAction.ResumeLayout(false);
            pnlAction.PerformLayout();
            pnlContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvExams).EndInit();
            ResumeLayout(false);

        }

        #endregion
    }
}
