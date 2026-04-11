namespace CourseGuard.Frontend.UserControls.Teacher
{
    partial class UC_EmailCard
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
            lblSender = new Label();
            lblSubject = new Label();
            lblTime = new Label();
            SuspendLayout();
            // 
            // lblSender
            // 
            lblSender.AutoSize = true;
            lblSender.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSender.Location = new Point(12, 10);
            lblSender.Name = "lblSender";
            lblSender.Size = new Size(63, 21);
            lblSender.TabIndex = 0;
            lblSender.Text = "Sender";
            // 
            // lblSubject
            // 
            lblSubject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSubject.AutoEllipsis = true;
            lblSubject.Font = new Font("Segoe UI", 9F);
            lblSubject.Location = new Point(12, 35);
            lblSubject.Name = "lblSubject";
            lblSubject.Size = new Size(461, 25);
            lblSubject.TabIndex = 1;
            lblSubject.Text = "Subject of the email goes here and gets truncated if it is too long";
            // 
            // lblTime
            // 
            lblTime.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTime.AutoSize = true;
            lblTime.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblTime.Location = new Point(375, 12);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(39, 19);
            lblTime.TabIndex = 2;
            lblTime.Text = "Time";
            lblTime.TextAlign = ContentAlignment.TopRight;
            // 
            // UC_EmailCard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(lblTime);
            Controls.Add(lblSubject);
            Controls.Add(lblSender);
            Margin = new Padding(0, 0, 0, 8);
            Name = "UC_EmailCard";
            Size = new Size(485, 84);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblSender;
        private System.Windows.Forms.Label lblSubject;
        private System.Windows.Forms.Label lblTime;
    }
}
