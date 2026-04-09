namespace CourseGuard.Presentation.UserControls.Teacher
{
    partial class UC_ScoreManagement
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            panelToolbar = new Panel();
            btnExport = new Button();
            btnImport = new Button();
            btnFilter = new Button();
            txtSearch = new TextBox();
            dgvScores = new DataGridView();
            panelHeader.SuspendLayout();
            panelToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvScores).BeginInit();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Margin = new Padding(3, 4, 3, 4);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(23, 0, 23, 0);
            panelHeader.Size = new Size(1029, 96);
            panelHeader.TabIndex = 6;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Font = new Font("Segoe UI", 9F);
            lblSubtitle.Location = new Point(26, 65);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(686, 27);
            lblSubtitle.TabIndex = 0;
            lblSubtitle.Text = "Xem, tìm kiếm, lọc và import/export điểm sinh viên.";
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.DodgerBlue;
            lblTitle.Location = new Point(26, 17);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(686, 48);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Quản Lý Điểm Số";
            // 
            // panelToolbar
            // 
            panelToolbar.Controls.Add(btnExport);
            panelToolbar.Controls.Add(btnImport);
            panelToolbar.Controls.Add(btnFilter);
            panelToolbar.Controls.Add(txtSearch);
            panelToolbar.Dock = DockStyle.Top;
            panelToolbar.Location = new Point(0, 96);
            panelToolbar.Margin = new Padding(3, 4, 3, 4);
            panelToolbar.Name = "panelToolbar";
            panelToolbar.Padding = new Padding(23, 13, 23, 13);
            panelToolbar.Size = new Size(1029, 75);
            panelToolbar.TabIndex = 5;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(624, 16);
            btnExport.Margin = new Padding(3, 4, 3, 4);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(137, 43);
            btnExport.TabIndex = 3;
            btnExport.Text = "⬇ Export CSV";
            btnExport.UseVisualStyleBackColor = false;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(473, 16);
            btnImport.Margin = new Padding(3, 4, 3, 4);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(137, 43);
            btnImport.TabIndex = 2;
            btnImport.Text = "⬆ Import CSV";
            btnImport.UseVisualStyleBackColor = false;
            // 
            // btnFilter
            // 
            btnFilter.Location = new Point(311, 16);
            btnFilter.Margin = new Padding(3, 4, 3, 4);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(149, 43);
            btnFilter.TabIndex = 1;
            btnFilter.Text = "⚑ Lọc trượt";
            btnFilter.UseVisualStyleBackColor = false;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(23, 19);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "🔍  Tìm theo MSSV hoặc Họ Tên...";
            txtSearch.Size = new Size(274, 27);
            txtSearch.TabIndex = 0;
            // 
            // dgvScores
            // 
            dgvScores.ColumnHeadersHeight = 29;
            dgvScores.Dock = DockStyle.Fill;
            dgvScores.Location = new Point(0, 171);
            dgvScores.Margin = new Padding(3, 4, 3, 4);
            dgvScores.Name = "dgvScores";
            dgvScores.ReadOnly = true;
            dgvScores.RowHeadersWidth = 51;
            dgvScores.Size = new Size(1029, 629);
            dgvScores.TabIndex = 4;
            // 
            // UC_ScoreManagement
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dgvScores);
            Controls.Add(panelToolbar);
            Controls.Add(panelHeader);
            Margin = new Padding(3, 4, 3, 4);
            Name = "UC_ScoreManagement";
            Size = new Size(1029, 800);
            panelHeader.ResumeLayout(false);
            panelToolbar.ResumeLayout(false);
            panelToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvScores).EndInit();
            ResumeLayout(false);
        }

        #endregion

        // ── Control declarations ─────────────────────────────────────────────────
        private System.Windows.Forms.Panel      panelHeader;
        private System.Windows.Forms.Panel      panelToolbar;
        private System.Windows.Forms.Label      lblTitle;
        private System.Windows.Forms.Label      lblSubtitle;
        private System.Windows.Forms.TextBox    txtSearch;
        private System.Windows.Forms.Button     btnFilter;
        private System.Windows.Forms.Button     btnImport;
        private System.Windows.Forms.Button     btnExport;
        private System.Windows.Forms.DataGridView dgvScores;
    }
}
