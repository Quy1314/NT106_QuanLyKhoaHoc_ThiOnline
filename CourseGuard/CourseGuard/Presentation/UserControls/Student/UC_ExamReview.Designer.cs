namespace CourseGuard.Presentation.UserControls.Student
{
    partial class UC_ExamReview
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
            this.lblScore = new System.Windows.Forms.Label();
            this.dgvReview = new System.Windows.Forms.DataGridView();
            this.btnBack = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReview)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(138)))));
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(201, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Xem lại chi tiết";
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblScore.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblScore.Location = new System.Drawing.Point(400, 25);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(126, 32);
            this.lblScore.TabIndex = 1;
            this.lblScore.Text = "Điểm: 9.0";
            // 
            // btnBack
            // 
            this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Location = new System.Drawing.Point(780, 22);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(100, 35);
            this.btnBack.TabIndex = 2;
            this.btnBack.Text = "Quay lại";
            this.btnBack.UseVisualStyleBackColor = false;
            // 
            // dgvReview
            // 
            this.dgvReview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvReview.BackgroundColor = System.Drawing.Color.White;
            this.dgvReview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReview.Location = new System.Drawing.Point(20, 80);
            this.dgvReview.Name = "dgvReview";
            this.dgvReview.RowHeadersWidth = 51;
            this.dgvReview.RowTemplate.Height = 29;
            this.dgvReview.Size = new System.Drawing.Size(860, 460);
            this.dgvReview.TabIndex = 3;
            // 
            // UC_ExamReview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.Controls.Add(this.dgvReview);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.lblTitle);
            this.Name = "UC_ExamReview";
            this.Size = new System.Drawing.Size(900, 560);
            ((System.ComponentModel.ISupportInitialize)(this.dgvReview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.DataGridView dgvReview;
        private System.Windows.Forms.Button btnBack;
    }
}
