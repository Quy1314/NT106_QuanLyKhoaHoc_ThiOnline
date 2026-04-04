using CourseGuard.Presentation.Theme;
using System.Drawing;
using System.Windows.Forms;

namespace CourseGuard.Presentation.UserControls.Teacher
{
    partial class UC_ExamConfig
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.panelConfig = new System.Windows.Forms.Panel();
            this.panelQuestions = new System.Windows.Forms.Panel();
            this.dgvQuestions = new System.Windows.Forms.DataGridView();
            this.lblTime = new System.Windows.Forms.Label();
            this.txtTime = new System.Windows.Forms.TextBox();
            this.lblNumQuestions = new System.Windows.Forms.Label();
            this.txtNumQuestions = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();

            this.tableLayoutPanelMain.SuspendLayout();
            this.panelConfig.SuspendLayout();
            this.panelQuestions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuestions)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 2;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMain.Controls.Add(this.panelConfig, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.panelQuestions, 1, 0);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 1;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(950, 690);
            this.tableLayoutPanelMain.TabIndex = 0;
            // 
            // panelConfig
            // 
            this.panelConfig.Controls.Add(this.btnSave);
            this.panelConfig.Controls.Add(this.txtNumQuestions);
            this.panelConfig.Controls.Add(this.lblNumQuestions);
            this.panelConfig.Controls.Add(this.txtTime);
            this.panelConfig.Controls.Add(this.lblTime);
            this.panelConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelConfig.Location = new System.Drawing.Point(3, 3);
            this.panelConfig.Name = "panelConfig";
            this.panelConfig.Padding = new System.Windows.Forms.Padding(20);
            this.panelConfig.Size = new System.Drawing.Size(469, 684);
            this.panelConfig.TabIndex = 0;
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTime.Location = new System.Drawing.Point(23, 20);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(155, 23);
            this.lblTime.TabIndex = 0;
            this.lblTime.Text = "Thời gian (phút):";
            // 
            // txtTime
            // 
            this.txtTime.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTime.Location = new System.Drawing.Point(27, 46);
            this.txtTime.Name = "txtTime";
            this.txtTime.Size = new System.Drawing.Size(420, 30);
            this.txtTime.TabIndex = 1;
            // 
            // lblNumQuestions
            // 
            this.lblNumQuestions.AutoSize = true;
            this.lblNumQuestions.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblNumQuestions.Location = new System.Drawing.Point(23, 100);
            this.lblNumQuestions.Name = "lblNumQuestions";
            this.lblNumQuestions.Size = new System.Drawing.Size(95, 23);
            this.lblNumQuestions.TabIndex = 2;
            this.lblNumQuestions.Text = "Số câu hỏi:";
            // 
            // txtNumQuestions
            // 
            this.txtNumQuestions.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNumQuestions.Location = new System.Drawing.Point(27, 126);
            this.txtNumQuestions.Name = "txtNumQuestions";
            this.txtNumQuestions.Size = new System.Drawing.Size(420, 30);
            this.txtNumQuestions.TabIndex = 3;
            // 
            // btnSave
            // 
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(27, 200);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(150, 40);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Lưu cấu hình";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // panelQuestions
            // 
            this.panelQuestions.Controls.Add(this.dgvQuestions);
            this.panelQuestions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelQuestions.Location = new System.Drawing.Point(478, 3);
            this.panelQuestions.Name = "panelQuestions";
            this.panelQuestions.Padding = new System.Windows.Forms.Padding(10);
            this.panelQuestions.Size = new System.Drawing.Size(469, 684);
            this.panelQuestions.TabIndex = 1;
            // 
            // dgvQuestions
            // 
            this.dgvQuestions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvQuestions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvQuestions.EnableHeadersVisualStyles = false;
            this.dgvQuestions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvQuestions.Location = new System.Drawing.Point(10, 10);
            this.dgvQuestions.Name = "dgvQuestions";
            this.dgvQuestions.RowHeadersVisible = false;
            this.dgvQuestions.Size = new System.Drawing.Size(449, 664);
            this.dgvQuestions.TabIndex = 0;
            // 
            // UC_ExamConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Name = "UC_ExamConfig";
            this.Size = new System.Drawing.Size(950, 690);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.panelConfig.ResumeLayout(false);
            this.panelConfig.PerformLayout();
            this.panelQuestions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuestions)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Panel panelConfig;
        private System.Windows.Forms.Panel panelQuestions;
        private System.Windows.Forms.DataGridView dgvQuestions;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.TextBox txtTime;
        private System.Windows.Forms.Label lblNumQuestions;
        private System.Windows.Forms.TextBox txtNumQuestions;
        private System.Windows.Forms.Button btnSave;
    }
}
