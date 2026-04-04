using CourseGuard.Presentation.Theme;
using System.Drawing;
using System.Windows.Forms;

namespace CourseGuard.Presentation.UserControls.Teacher
{
    partial class UC_ExamMonitor
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.cardActive = new System.Windows.Forms.Panel();
            this.lblActiveValue = new System.Windows.Forms.Label();
            this.lblActiveTitle = new System.Windows.Forms.Label();
            this.cardSubmitted = new System.Windows.Forms.Panel();
            this.lblSubmittedValue = new System.Windows.Forms.Label();
            this.lblSubmittedTitle = new System.Windows.Forms.Label();
            this.cardWarning = new System.Windows.Forms.Panel();
            this.lblWarningValue = new System.Windows.Forms.Label();
            this.lblWarningTitle = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgvMonitor = new System.Windows.Forms.DataGridView();

            this.panelTop.SuspendLayout();
            this.cardActive.SuspendLayout();
            this.cardSubmitted.SuspendLayout();
            this.cardWarning.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.cardActive);
            this.panelTop.Controls.Add(this.cardSubmitted);
            this.panelTop.Controls.Add(this.cardWarning);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(20);
            this.panelTop.Size = new System.Drawing.Size(950, 150);
            this.panelTop.TabIndex = 0;
            // 
            // cardActive
            // 
            this.cardActive.Controls.Add(this.lblActiveValue);
            this.cardActive.Controls.Add(this.lblActiveTitle);
            this.cardActive.Location = new System.Drawing.Point(20, 20);
            this.cardActive.Name = "cardActive";
            this.cardActive.Size = new System.Drawing.Size(250, 110);
            this.cardActive.TabIndex = 0;
            // 
            // lblActiveValue
            // 
            this.lblActiveValue.AutoSize = true;
            this.lblActiveValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblActiveValue.Location = new System.Drawing.Point(15, 45);
            this.lblActiveValue.Name = "lblActiveValue";
            this.lblActiveValue.Size = new System.Drawing.Size(68, 54);
            this.lblActiveValue.TabIndex = 1;
            this.lblActiveValue.Text = "42";
            // 
            // lblActiveTitle
            // 
            this.lblActiveTitle.AutoSize = true;
            this.lblActiveTitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblActiveTitle.Location = new System.Drawing.Point(15, 15);
            this.lblActiveTitle.Name = "lblActiveTitle";
            this.lblActiveTitle.Size = new System.Drawing.Size(99, 23);
            this.lblActiveTitle.TabIndex = 0;
            this.lblActiveTitle.Text = "Đang làm bài";
            // 
            // cardSubmitted
            // 
            this.cardSubmitted.Controls.Add(this.lblSubmittedValue);
            this.cardSubmitted.Controls.Add(this.lblSubmittedTitle);
            this.cardSubmitted.Location = new System.Drawing.Point(290, 20);
            this.cardSubmitted.Name = "cardSubmitted";
            this.cardSubmitted.Size = new System.Drawing.Size(250, 110);
            this.cardSubmitted.TabIndex = 1;
            // 
            // lblSubmittedValue
            // 
            this.lblSubmittedValue.AutoSize = true;
            this.lblSubmittedValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblSubmittedValue.Location = new System.Drawing.Point(15, 45);
            this.lblSubmittedValue.Name = "lblSubmittedValue";
            this.lblSubmittedValue.Size = new System.Drawing.Size(68, 54);
            this.lblSubmittedValue.TabIndex = 1;
            this.lblSubmittedValue.Text = "15";
            // 
            // lblSubmittedTitle
            // 
            this.lblSubmittedTitle.AutoSize = true;
            this.lblSubmittedTitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSubmittedTitle.Location = new System.Drawing.Point(15, 15);
            this.lblSubmittedTitle.Name = "lblSubmittedTitle";
            this.lblSubmittedTitle.Size = new System.Drawing.Size(96, 23);
            this.lblSubmittedTitle.TabIndex = 0;
            this.lblSubmittedTitle.Text = "Đã nộp bài";
            // 
            // cardWarning
            // 
            this.cardWarning.Controls.Add(this.lblWarningValue);
            this.cardWarning.Controls.Add(this.lblWarningTitle);
            this.cardWarning.Location = new System.Drawing.Point(560, 20);
            this.cardWarning.Name = "cardWarning";
            this.cardWarning.Size = new System.Drawing.Size(250, 110);
            this.cardWarning.TabIndex = 2;
            // 
            // lblWarningValue
            // 
            this.lblWarningValue.AutoSize = true;
            this.lblWarningValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblWarningValue.Location = new System.Drawing.Point(15, 45);
            this.lblWarningValue.Name = "lblWarningValue";
            this.lblWarningValue.Size = new System.Drawing.Size(45, 54);
            this.lblWarningValue.TabIndex = 1;
            this.lblWarningValue.Text = "3";
            // 
            // lblWarningTitle
            // 
            this.lblWarningTitle.AutoSize = true;
            this.lblWarningTitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblWarningTitle.Location = new System.Drawing.Point(15, 15);
            this.lblWarningTitle.Name = "lblWarningTitle";
            this.lblWarningTitle.Size = new System.Drawing.Size(126, 23);
            this.lblWarningTitle.TabIndex = 0;
            this.lblWarningTitle.Text = "Cảnh báo vi phạm";
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgvMonitor);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 150);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(20);
            this.panelBottom.Size = new System.Drawing.Size(950, 540);
            this.panelBottom.TabIndex = 1;
            // 
            // dgvMonitor
            // 
            this.dgvMonitor.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMonitor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvMonitor.EnableHeadersVisualStyles = false;
            this.dgvMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMonitor.Location = new System.Drawing.Point(20, 20);
            this.dgvMonitor.Name = "dgvMonitor";
            this.dgvMonitor.RowHeadersVisible = false;
            this.dgvMonitor.Size = new System.Drawing.Size(910, 500);
            this.dgvMonitor.TabIndex = 0;
            // 
            // UC_ExamMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Name = "UC_ExamMonitor";
            this.Size = new System.Drawing.Size(950, 690);
            this.panelTop.ResumeLayout(false);
            this.cardActive.ResumeLayout(false);
            this.cardActive.PerformLayout();
            this.cardSubmitted.ResumeLayout(false);
            this.cardSubmitted.PerformLayout();
            this.cardWarning.ResumeLayout(false);
            this.cardWarning.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitor)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel cardActive;
        private System.Windows.Forms.Label lblActiveValue;
        private System.Windows.Forms.Label lblActiveTitle;
        private System.Windows.Forms.Panel cardSubmitted;
        private System.Windows.Forms.Label lblSubmittedValue;
        private System.Windows.Forms.Label lblSubmittedTitle;
        private System.Windows.Forms.Panel cardWarning;
        private System.Windows.Forms.Label lblWarningValue;
        private System.Windows.Forms.Label lblWarningTitle;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.DataGridView dgvMonitor;
    }
}
