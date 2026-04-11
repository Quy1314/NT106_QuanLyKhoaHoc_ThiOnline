/*
 * UC_Notification.Designer.cs
 *
 * QUAN TRỌNG: File này được thiết lập theo hướng "Designer-compatible".
 * Mọi control đều được khai báo ở đây để Designer của Visual Studio có thể nhận diện,
 * nhưng toàn bộ style/layout được áp dụng trong file .cs chính để dễ bảo trì.
 */
namespace CourseGuard.Frontend.UserControls.Teacher
{
    partial class UC_Notification
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
        private System.Windows.Forms.Button btnFilterUnread;
        private System.Windows.Forms.FlowLayoutPanel flpNotifications;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.Label lblDetailSender;
        private System.Windows.Forms.Label lblDetailDate;
        private System.Windows.Forms.RichTextBox rtbDetailBody;
        private System.Windows.Forms.Button btnMarkAsReadDetail;
        private System.Windows.Forms.Button btnDeleteDetail;
        private System.Windows.Forms.Label lblEmptyDetail;

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
            lblTitle = new System.Windows.Forms.Label();
            lblSubtitle = new System.Windows.Forms.Label();
            pnlHeader = new System.Windows.Forms.Panel();
            pnlToolbar = new System.Windows.Forms.Panel();
            btnFilterUnread = new System.Windows.Forms.Button();
            txtSearch = new System.Windows.Forms.TextBox();
            flpNotifications = new System.Windows.Forms.FlowLayoutPanel();
            pnlStatus = new System.Windows.Forms.Panel();
            lblStatus = new System.Windows.Forms.Label();
            splitContainer = new System.Windows.Forms.SplitContainer();
            pnlDetail = new System.Windows.Forms.Panel();
            lblDetailTitle = new System.Windows.Forms.Label();
            lblDetailSender = new System.Windows.Forms.Label();
            lblDetailDate = new System.Windows.Forms.Label();
            rtbDetailBody = new System.Windows.Forms.RichTextBox();
            btnMarkAsReadDetail = new System.Windows.Forms.Button();
            btnDeleteDetail = new System.Windows.Forms.Button();
            lblEmptyDetail = new System.Windows.Forms.Label();
            pnlHeader.SuspendLayout();
            pnlToolbar.SuspendLayout();
            pnlStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            pnlDetail.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitle.Location = new System.Drawing.Point(23, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(1074, 50);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Thông báo";
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSubtitle.Location = new System.Drawing.Point(23, 58);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new System.Drawing.Size(1074, 25);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Xem và Quản lý các thông báo của bạn";
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new System.Windows.Forms.Padding(23, 0, 23, 0);
            pnlHeader.Size = new System.Drawing.Size(1120, 110);
            pnlHeader.TabIndex = 2;
            // 
            // pnlToolbar
            // 
            pnlToolbar.Controls.Add(btnFilterUnread);
            pnlToolbar.Controls.Add(txtSearch);
            pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            pnlToolbar.Location = new System.Drawing.Point(0, 110);
            pnlToolbar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new System.Windows.Forms.Padding(23, 20, 23, 20);
            pnlToolbar.Size = new System.Drawing.Size(1120, 86);
            pnlToolbar.TabIndex = 1;
            // 
            // btnFilterUnread
            // 
            btnFilterUnread.Cursor = System.Windows.Forms.Cursors.Hand;
            btnFilterUnread.Dock = System.Windows.Forms.DockStyle.Right;
            btnFilterUnread.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFilterUnread.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnFilterUnread.Location = new System.Drawing.Point(948, 20);
            btnFilterUnread.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnFilterUnread.Name = "btnFilterUnread";
            btnFilterUnread.Size = new System.Drawing.Size(149, 46);
            btnFilterUnread.TabIndex = 0;
            btnFilterUnread.Text = "⚑ Chưa đọc";
            // 
            // txtSearch
            // 
            txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtSearch.Location = new System.Drawing.Point(23, 29);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new System.Drawing.Size(306, 30);
            txtSearch.TabIndex = 1;
            // 
            // splitContainer
            // 
            splitContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer.Location = new System.Drawing.Point(0, 196);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(flpNotifications);
            splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(10, 10, 5, 10);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(lblEmptyDetail);
            splitContainer.Panel2.Controls.Add(pnlDetail);
            splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(5, 10, 10, 10);
            splitContainer.Size = new System.Drawing.Size(1120, 595);
            splitContainer.SplitterDistance = 448;
            splitContainer.TabIndex = 5;
            // 
            // flpNotifications
            // 
            flpNotifications.AutoScroll = true;
            flpNotifications.Dock = System.Windows.Forms.DockStyle.Fill;
            flpNotifications.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flpNotifications.Location = new System.Drawing.Point(10, 10);
            flpNotifications.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            flpNotifications.Name = "flpNotifications";
            flpNotifications.Size = new System.Drawing.Size(433, 575);
            flpNotifications.TabIndex = 4;
            flpNotifications.WrapContents = false;
            // 
            // pnlDetail
            // 
            pnlDetail.Controls.Add(btnDeleteDetail);
            pnlDetail.Controls.Add(btnMarkAsReadDetail);
            pnlDetail.Controls.Add(rtbDetailBody);
            pnlDetail.Controls.Add(lblDetailDate);
            pnlDetail.Controls.Add(lblDetailSender);
            pnlDetail.Controls.Add(lblDetailTitle);
            pnlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlDetail.Location = new System.Drawing.Point(5, 10);
            pnlDetail.Name = "pnlDetail";
            pnlDetail.Padding = new System.Windows.Forms.Padding(20);
            pnlDetail.Size = new System.Drawing.Size(653, 575);
            pnlDetail.TabIndex = 0;
            pnlDetail.Visible = false;
            // 
            // lblDetailTitle
            // 
            lblDetailTitle.AutoSize = true;
            lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblDetailTitle.Location = new System.Drawing.Point(20, 20);
            lblDetailTitle.Name = "lblDetailTitle";
            lblDetailTitle.Size = new System.Drawing.Size(75, 37);
            lblDetailTitle.TabIndex = 0;
            lblDetailTitle.Text = "Title";
            // 
            // lblDetailSender
            // 
            lblDetailSender.AutoSize = true;
            lblDetailSender.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblDetailSender.Location = new System.Drawing.Point(23, 65);
            lblDetailSender.Name = "lblDetailSender";
            lblDetailSender.Size = new System.Drawing.Size(66, 23);
            lblDetailSender.TabIndex = 1;
            lblDetailSender.Text = "Sender";
            // 
            // lblDetailDate
            // 
            lblDetailDate.AutoSize = true;
            lblDetailDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblDetailDate.Location = new System.Drawing.Point(24, 90);
            lblDetailDate.Name = "lblDetailDate";
            lblDetailDate.Size = new System.Drawing.Size(41, 20);
            lblDetailDate.TabIndex = 2;
            lblDetailDate.Text = "Date";
            // 
            // rtbDetailBody
            // 
            rtbDetailBody.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rtbDetailBody.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtbDetailBody.Font = new System.Drawing.Font("Segoe UI", 10F);
            rtbDetailBody.Location = new System.Drawing.Point(28, 130);
            rtbDetailBody.Name = "rtbDetailBody";
            rtbDetailBody.ReadOnly = true;
            rtbDetailBody.Size = new System.Drawing.Size(602, 365);
            rtbDetailBody.TabIndex = 3;
            rtbDetailBody.Text = "";
            // 
            // btnMarkAsReadDetail
            // 
            btnMarkAsReadDetail.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnMarkAsReadDetail.Cursor = System.Windows.Forms.Cursors.Hand;
            btnMarkAsReadDetail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnMarkAsReadDetail.Location = new System.Drawing.Point(344, 515);
            btnMarkAsReadDetail.Name = "btnMarkAsReadDetail";
            btnMarkAsReadDetail.Size = new System.Drawing.Size(140, 40);
            btnMarkAsReadDetail.TabIndex = 4;
            btnMarkAsReadDetail.Text = "✓ Đánh dấu";
            btnMarkAsReadDetail.UseVisualStyleBackColor = true;
            // 
            // btnDeleteDetail
            // 
            btnDeleteDetail.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnDeleteDetail.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDeleteDetail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDeleteDetail.Location = new System.Drawing.Point(490, 515);
            btnDeleteDetail.Name = "btnDeleteDetail";
            btnDeleteDetail.Size = new System.Drawing.Size(140, 40);
            btnDeleteDetail.TabIndex = 5;
            btnDeleteDetail.Text = "✕ Xóa";
            btnDeleteDetail.UseVisualStyleBackColor = true;
            // 
            // lblEmptyDetail
            // 
            lblEmptyDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            lblEmptyDetail.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Italic);
            lblEmptyDetail.Location = new System.Drawing.Point(5, 10);
            lblEmptyDetail.Name = "lblEmptyDetail";
            lblEmptyDetail.Size = new System.Drawing.Size(653, 575);
            lblEmptyDetail.TabIndex = 1;
            lblEmptyDetail.Text = "Chọn một thông báo để xem chi tiết";
            lblEmptyDetail.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlStatus
            // 
            pnlStatus.Controls.Add(lblStatus);
            pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlStatus.Location = new System.Drawing.Point(0, 791);
            pnlStatus.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Padding = new System.Windows.Forms.Padding(23, 0, 23, 0);
            pnlStatus.Size = new System.Drawing.Size(1120, 40);
            pnlStatus.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.Location = new System.Drawing.Point(23, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(1074, 40);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Loading notifications...";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UC_Notification
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(pnlStatus);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "UC_Notification";
            Size = new System.Drawing.Size(1120, 831);
            pnlHeader.ResumeLayout(false);
            pnlToolbar.ResumeLayout(false);
            pnlToolbar.PerformLayout();
            pnlStatus.ResumeLayout(false);
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            pnlDetail.ResumeLayout(false);
            pnlDetail.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
