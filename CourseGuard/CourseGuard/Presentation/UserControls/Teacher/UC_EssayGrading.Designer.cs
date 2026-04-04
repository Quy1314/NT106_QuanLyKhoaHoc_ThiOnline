using CourseGuard.Presentation.Theme;
using System.Drawing;
using System.Windows.Forms;

namespace CourseGuard.Presentation.UserControls.Teacher
{
    partial class UC_EssayGrading
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.dgvStudents = new System.Windows.Forms.DataGridView();
            this.richTextBoxEssay = new System.Windows.Forms.RichTextBox();
            this.panelGrading = new System.Windows.Forms.Panel();
            this.lblScore = new System.Windows.Forms.Label();
            this.txtScore = new System.Windows.Forms.TextBox();
            this.lblComment = new System.Windows.Forms.Label();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.btnSaveGrade = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudents)).BeginInit();
            this.panelGrading.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.dgvStudents);
            this.splitMain.Panel1.Padding = new System.Windows.Forms.Padding(10);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.richTextBoxEssay);
            this.splitMain.Panel2.Controls.Add(this.panelGrading);
            this.splitMain.Panel2.Padding = new System.Windows.Forms.Padding(10);
            this.splitMain.Size = new System.Drawing.Size(950, 690);
            this.splitMain.SplitterDistance = 350;
            this.splitMain.TabIndex = 0;
            // 
            // dgvStudents
            // 
            this.dgvStudents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStudents.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvStudents.EnableHeadersVisualStyles = false;
            this.dgvStudents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStudents.Location = new System.Drawing.Point(10, 10);
            this.dgvStudents.Name = "dgvStudents";
            this.dgvStudents.RowHeadersVisible = false;
            this.dgvStudents.Size = new System.Drawing.Size(330, 670);
            this.dgvStudents.TabIndex = 0;
            // 
            // richTextBoxEssay
            // 
            this.richTextBoxEssay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxEssay.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.richTextBoxEssay.Location = new System.Drawing.Point(10, 10);
            this.richTextBoxEssay.Name = "richTextBoxEssay";
            this.richTextBoxEssay.ReadOnly = true;
            this.richTextBoxEssay.Size = new System.Drawing.Size(576, 520);
            this.richTextBoxEssay.TabIndex = 0;
            this.richTextBoxEssay.Text = "";
            // 
            // panelGrading
            // 
            this.panelGrading.Controls.Add(this.btnSaveGrade);
            this.panelGrading.Controls.Add(this.txtComment);
            this.panelGrading.Controls.Add(this.lblComment);
            this.panelGrading.Controls.Add(this.txtScore);
            this.panelGrading.Controls.Add(this.lblScore);
            this.panelGrading.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelGrading.Location = new System.Drawing.Point(10, 530);
            this.panelGrading.Name = "panelGrading";
            this.panelGrading.Size = new System.Drawing.Size(576, 150);
            this.panelGrading.TabIndex = 1;
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblScore.Location = new System.Drawing.Point(15, 20);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(89, 23);
            this.lblScore.TabIndex = 0;
            this.lblScore.Text = "Điểm số:";
            // 
            // txtScore
            // 
            this.txtScore.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtScore.Location = new System.Drawing.Point(110, 17);
            this.txtScore.Name = "txtScore";
            this.txtScore.Size = new System.Drawing.Size(100, 30);
            this.txtScore.TabIndex = 1;
            // 
            // lblComment
            // 
            this.lblComment.AutoSize = true;
            this.lblComment.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblComment.Location = new System.Drawing.Point(15, 65);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(86, 23);
            this.lblComment.TabIndex = 2;
            this.lblComment.Text = "Nhận xét:";
            // 
            // txtComment
            // 
            this.txtComment.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtComment.Location = new System.Drawing.Point(110, 62);
            this.txtComment.Multiline = true;
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(300, 60);
            this.txtComment.TabIndex = 3;
            // 
            // btnSaveGrade
            // 
            this.btnSaveGrade.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveGrade.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveGrade.Location = new System.Drawing.Point(430, 82);
            this.btnSaveGrade.Name = "btnSaveGrade";
            this.btnSaveGrade.Size = new System.Drawing.Size(120, 40);
            this.btnSaveGrade.TabIndex = 4;
            this.btnSaveGrade.Text = "Lưu điểm";
            this.btnSaveGrade.UseVisualStyleBackColor = true;
            // 
            // UC_EssayGrading
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitMain);
            this.Name = "UC_EssayGrading";
            this.Size = new System.Drawing.Size(950, 690);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudents)).EndInit();
            this.panelGrading.ResumeLayout(false);
            this.panelGrading.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.DataGridView dgvStudents;
        private System.Windows.Forms.RichTextBox richTextBoxEssay;
        private System.Windows.Forms.Panel panelGrading;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.TextBox txtScore;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Button btnSaveGrade;
    }
}
