namespace CourseGuard.Presentation.UserControls.Student
{
    partial class UC_Schedule
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

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.cboTimeFilter = new System.Windows.Forms.ComboBox();
            this.dgvSchedule = new System.Windows.Forms.DataGridView();
            this.btnJoinOnline = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSchedule)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(138)))));
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(126, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Lịch học";
            // 
            // cboTimeFilter
            // 
            this.cboTimeFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTimeFilter.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cboTimeFilter.FormattingEnabled = true;
            this.cboTimeFilter.Items.AddRange(new object[] {
            "Tuần này",
            "Tháng này",
            "Tất cả"});
            this.cboTimeFilter.Location = new System.Drawing.Point(20, 70);
            this.cboTimeFilter.Name = "cboTimeFilter";
            this.cboTimeFilter.Size = new System.Drawing.Size(200, 33);
            this.cboTimeFilter.TabIndex = 1;
            // 
            // btnJoinOnline
            // 
            this.btnJoinOnline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnJoinOnline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnJoinOnline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnJoinOnline.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnJoinOnline.ForeColor = System.Drawing.Color.White;
            this.btnJoinOnline.Location = new System.Drawing.Point(730, 68);
            this.btnJoinOnline.Name = "btnJoinOnline";
            this.btnJoinOnline.Size = new System.Drawing.Size(150, 35);
            this.btnJoinOnline.TabIndex = 2;
            this.btnJoinOnline.Text = "Tham gia Online";
            this.btnJoinOnline.UseVisualStyleBackColor = false;
            // 
            // dgvSchedule
            // 
            this.dgvSchedule.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSchedule.BackgroundColor = System.Drawing.Color.White;
            this.dgvSchedule.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSchedule.Location = new System.Drawing.Point(20, 120);
            this.dgvSchedule.Name = "dgvSchedule";
            this.dgvSchedule.RowHeadersWidth = 51;
            this.dgvSchedule.RowTemplate.Height = 29;
            this.dgvSchedule.Size = new System.Drawing.Size(860, 420);
            this.dgvSchedule.TabIndex = 3;
            // 
            // UC_Schedule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.Controls.Add(this.dgvSchedule);
            this.Controls.Add(this.btnJoinOnline);
            this.Controls.Add(this.cboTimeFilter);
            this.Controls.Add(this.lblTitle);
            this.Name = "UC_Schedule";
            this.Size = new System.Drawing.Size(900, 560);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSchedule)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ComboBox cboTimeFilter;
        private System.Windows.Forms.DataGridView dgvSchedule;
        private System.Windows.Forms.Button btnJoinOnline;
    }
}
