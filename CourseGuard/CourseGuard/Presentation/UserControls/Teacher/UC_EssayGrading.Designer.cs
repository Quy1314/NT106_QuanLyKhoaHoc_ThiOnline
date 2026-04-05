namespace CourseGuard.Presentation.UserControls.Teacher
{
    partial class UC_EssayGrading
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
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.dgvStudents = new System.Windows.Forms.DataGridView();
            this.lblStudentListTitle = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.rtbEssayViewer = new System.Windows.Forms.RichTextBox();
            this.pnlGradingControls = new System.Windows.Forms.Panel();
            this.lblFeedback = new System.Windows.Forms.Label();
            this.txtFeedback = new System.Windows.Forms.TextBox();
            this.lblScore = new System.Windows.Forms.Label();
            this.nudScore = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudents)).BeginInit();
            this.pnlRight.SuspendLayout();
            this.pnlGradingControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScore)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.dgvStudents);
            this.pnlLeft.Controls.Add(this.lblStudentListTitle);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(10);
            this.pnlLeft.Size = new System.Drawing.Size(300, 600);
            this.pnlLeft.TabIndex = 0;
            // 
            // lblStudentListTitle
            // 
            this.lblStudentListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStudentListTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblStudentListTitle.Location = new System.Drawing.Point(10, 10);
            this.lblStudentListTitle.Name = "lblStudentListTitle";
            this.lblStudentListTitle.Size = new System.Drawing.Size(280, 40);
            this.lblStudentListTitle.TabIndex = 0;
            this.lblStudentListTitle.Text = "Danh sách bài nộp";
            this.lblStudentListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvStudents
            // 
            this.dgvStudents.AllowUserToAddRows = false;
            this.dgvStudents.AllowUserToDeleteRows = false;
            this.dgvStudents.BackgroundColor = System.Drawing.Color.White;
            this.dgvStudents.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvStudents.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvStudents.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvStudents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStudents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStudents.EnableHeadersVisualStyles = false;
            this.dgvStudents.Location = new System.Drawing.Point(10, 50);
            this.dgvStudents.Name = "dgvStudents";
            this.dgvStudents.ReadOnly = true;
            this.dgvStudents.RowHeadersVisible = false;
            this.dgvStudents.RowTemplate.Height = 40;
            this.dgvStudents.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStudents.Size = new System.Drawing.Size(280, 540);
            this.dgvStudents.TabIndex = 1;
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.White;
            this.pnlRight.Controls.Add(this.rtbEssayViewer);
            this.pnlRight.Controls.Add(this.pnlGradingControls);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(310, 0); // Giữ lề 10px với pnlLeft nếu cần
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(10);
            this.pnlRight.Size = new System.Drawing.Size(580, 600);
            this.pnlRight.TabIndex = 1;
            // 
            // rtbEssayViewer
            // 
            this.rtbEssayViewer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.rtbEssayViewer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbEssayViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbEssayViewer.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rtbEssayViewer.Location = new System.Drawing.Point(10, 10);
            this.rtbEssayViewer.Name = "rtbEssayViewer";
            this.rtbEssayViewer.ReadOnly = true;
            this.rtbEssayViewer.Size = new System.Drawing.Size(560, 430);
            this.rtbEssayViewer.TabIndex = 0;
            this.rtbEssayViewer.Text = "Nội dung bài làm của sinh viên sẽ hiển thị ở đây...";
            // 
            // pnlGradingControls
            // 
            this.pnlGradingControls.Controls.Add(this.lblFeedback);
            this.pnlGradingControls.Controls.Add(this.txtFeedback);
            this.pnlGradingControls.Controls.Add(this.lblScore);
            this.pnlGradingControls.Controls.Add(this.nudScore);
            this.pnlGradingControls.Controls.Add(this.btnSave);
            this.pnlGradingControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlGradingControls.Location = new System.Drawing.Point(10, 440);
            this.pnlGradingControls.Name = "pnlGradingControls";
            this.pnlGradingControls.Size = new System.Drawing.Size(560, 150);
            this.pnlGradingControls.TabIndex = 1;
            // 
            // lblFeedback
            // 
            this.lblFeedback.AutoSize = true;
            this.lblFeedback.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblFeedback.Location = new System.Drawing.Point(0, 10);
            this.lblFeedback.Name = "lblFeedback";
            this.lblFeedback.Size = new System.Drawing.Size(73, 19);
            this.lblFeedback.TabIndex = 0;
            this.lblFeedback.Text = "Nhận xét:";
            // 
            // txtFeedback
            // 
            this.txtFeedback.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFeedback.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtFeedback.Location = new System.Drawing.Point(0, 35);
            this.txtFeedback.Multiline = true;
            this.txtFeedback.Name = "txtFeedback";
            this.txtFeedback.Size = new System.Drawing.Size(380, 100);
            this.txtFeedback.TabIndex = 1;
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblScore.Location = new System.Drawing.Point(400, 10);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(49, 19);
            this.lblScore.TabIndex = 2;
            this.lblScore.Text = "Điểm:";
            // 
            // nudScore
            // 
            this.nudScore.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nudScore.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nudScore.Location = new System.Drawing.Point(400, 35);
            this.nudScore.Name = "nudScore";
            this.nudScore.Size = new System.Drawing.Size(100, 29);
            this.nudScore.TabIndex = 3;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(400, 85);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 50);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Lưu";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // UC_EssayGrading
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Name = "UC_EssayGrading";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0); // Lề phải nếu muốn
            this.Size = new System.Drawing.Size(900, 600);
            this.pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudents)).EndInit();
            this.pnlRight.ResumeLayout(false);
            this.pnlGradingControls.ResumeLayout(false);
            this.pnlGradingControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScore)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Label lblStudentListTitle;
        private System.Windows.Forms.DataGridView dgvStudents;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.RichTextBox rtbEssayViewer;
        private System.Windows.Forms.Panel pnlGradingControls;
        private System.Windows.Forms.Label lblFeedback;
        private System.Windows.Forms.TextBox txtFeedback;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.NumericUpDown nudScore;
        private System.Windows.Forms.Button btnSave;
    }
}
