/*
 * UC_ScoreManagement.Designer.cs
 *
 * QUAN TRỌNG: File này được thiết lập theo hướng "Designer-compatible".
 * Mọi control đều được khai báo ở đây để Designer của Visual Studio có thể nhận diện,
 * nhưng toàn bộ style/layout được áp dụng trong file .cs chính để dễ bảo trì.
 */
namespace CourseGuard.Frontend.UserControls.Teacher
{
    partial class UC_ScoreManagement
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // --- Khai báo các control ---
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnFilterFailed;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.DataGridView dgvScores;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblStatus;

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
            lblTitle = new Label();
            lblSubtitle = new Label();
            pnlHeader = new Panel();
            pnlToolbar = new Panel();
            btnExport = new Button();
            btnImport = new Button();
            btnFilterFailed = new Button();
            txtSearch = new TextBox();
            dgvScores = new DataGridView();
            pnlStatus = new Panel();
            lblStatus = new Label();
            pnlHeader.SuspendLayout();
            pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvScores).BeginInit();
            pnlStatus.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(23, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1074, 50);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "QUẢN LÝ ĐIỂM SỐ";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblSubtitle.Location = new Point(23, 58);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(1074, 25);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Xem, tìm kiếm, lọc và import/export điểm sinh viên";
            lblSubtitle.TextAlign = ContentAlignment.TopLeft;
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(23, 0, 23, 0);
            pnlHeader.Size = new Size(1120, 110);
            pnlHeader.TabIndex = 2;
            // 
            // pnlToolbar
            // 
            pnlToolbar.Controls.Add(btnExport);
            pnlToolbar.Controls.Add(btnImport);
            pnlToolbar.Controls.Add(btnFilterFailed);
            pnlToolbar.Controls.Add(txtSearch);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 110);
            pnlToolbar.Margin = new Padding(3, 4, 3, 4);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new Padding(23, 20, 23, 20);
            pnlToolbar.Size = new Size(1120, 86);
            pnlToolbar.TabIndex = 1;
            // 
            // btnExport
            // 
            btnExport.Cursor = Cursors.Hand;
            btnExport.Dock = DockStyle.Right;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExport.Location = new Point(799, 20);
            btnExport.Margin = new Padding(3, 4, 3, 4);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(149, 46);
            btnExport.TabIndex = 0;
            btnExport.Text = "⬇  Export CSV";
            // 
            // btnImport
            // 
            btnImport.Cursor = Cursors.Hand;
            btnImport.Dock = DockStyle.Right;
            btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnImport.Location = new Point(948, 20);
            btnImport.Margin = new Padding(3, 4, 3, 4);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(149, 46);
            btnImport.TabIndex = 1;
            btnImport.Text = "⬆  Import CSV";
            // 
            // btnFilterFailed
            // 
            btnFilterFailed.Cursor = Cursors.Hand;
            btnFilterFailed.FlatStyle = FlatStyle.Flat;
            btnFilterFailed.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnFilterFailed.Location = new Point(335, 23);
            btnFilterFailed.Name = "btnFilterFailed";
            btnFilterFailed.Size = new Size(124, 46);
            btnFilterFailed.TabIndex = 2;
            btnFilterFailed.Text = "Lọc trượt";
            // 
            // txtSearch
            // 
            txtSearch.Font = new Font("Segoe UI", 10F);
            txtSearch.Location = new Point(23, 29);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(306, 30);
            txtSearch.TabIndex = 3;
            // 
            // dgvScores
            // 
            dgvScores.AllowUserToAddRows = false;
            dgvScores.AllowUserToDeleteRows = false;
            dgvScores.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvScores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvScores.ColumnHeadersHeight = 42;
            dgvScores.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvScores.Location = new Point(23, 206);
            dgvScores.Margin = new Padding(3, 4, 3, 4);
            dgvScores.MultiSelect = false;
            dgvScores.Name = "dgvScores";
            dgvScores.ReadOnly = true;
            dgvScores.RowHeadersVisible = false;
            dgvScores.RowHeadersWidth = 51;
            dgvScores.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvScores.Size = new Size(1074, 596);
            dgvScores.TabIndex = 0;
            // 
            // pnlStatus
            // 
            pnlStatus.Controls.Add(lblStatus);
            pnlStatus.Dock = DockStyle.Bottom;
            pnlStatus.Location = new Point(0, 787);
            pnlStatus.Margin = new Padding(3, 4, 3, 4);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Padding = new Padding(23, 0, 23, 0);
            pnlStatus.Size = new Size(1120, 40);
            pnlStatus.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.Location = new Point(23, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1074, 40);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Đang tải dữ liệu...";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // UC_ScoreManagement
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dgvScores);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);
            Controls.Add(pnlStatus);
            Margin = new Padding(3, 4, 3, 4);
            Name = "UC_ScoreManagement";
            Size = new Size(1120, 827);
            pnlHeader.ResumeLayout(false);
            pnlToolbar.ResumeLayout(false);
            pnlToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvScores).EndInit();
            pnlStatus.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
