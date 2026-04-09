namespace CourseGuard.Frontend.UserControls.Teacher
{
    partial class UC_Chat
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
            pnlChat = new Panel();
            lblTitle = new Label();
            txtMessage = new TextBox();
            btnSend = new Button();
            lstChat = new ListBox();
            pnlChat.SuspendLayout();
            SuspendLayout();

            // ============= TOP PANEL =============
            pnlChat.BackColor = System.Drawing.Color.White;
            pnlChat.BorderStyle = BorderStyle.FixedSingle;
            pnlChat.Controls.Add(lblTitle);
            pnlChat.Dock = DockStyle.Top;
            pnlChat.Location = new System.Drawing.Point(0, 0);
            pnlChat.Name = "pnlChat";
            pnlChat.Size = new System.Drawing.Size(764, 50);
            pnlChat.TabIndex = 0;

            // ============= TITLE LABEL =============
            lblTitle.AutoSize = false;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(33, 33, 33);
            lblTitle.Location = new System.Drawing.Point(15, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(300, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "💬 Trò chuyện";
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ============= LIST BOX - MESSAGE DISPLAY =============
            lstChat.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            lstChat.Font = new System.Drawing.Font("Segoe UI", 10F);
            lstChat.ForeColor = System.Drawing.Color.FromArgb(33, 33, 33);
            lstChat.Location = new System.Drawing.Point(0, 50);
            lstChat.Name = "lstChat";
            lstChat.Size = new System.Drawing.Size(764, 480);
            lstChat.TabIndex = 1;

            // ============= TEXT BOX - INPUT =============
            txtMessage.BackColor = System.Drawing.Color.White;
            txtMessage.BorderStyle = BorderStyle.FixedSingle;
            txtMessage.Dock = DockStyle.Bottom;
            txtMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtMessage.ForeColor = System.Drawing.Color.FromArgb(33, 33, 33);
            txtMessage.Location = new System.Drawing.Point(0, 530);
            txtMessage.Multiline = false;
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new System.Drawing.Size(704, 30);
            txtMessage.TabIndex = 2;

            // ============= SEND BUTTON =============
            btnSend.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnSend.Dock = DockStyle.Bottom;
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSend.ForeColor = System.Drawing.Color.White;
            btnSend.Location = new System.Drawing.Point(704, 530);
            btnSend.Name = "btnSend";
            btnSend.Size = new System.Drawing.Size(60, 30);
            btnSend.TabIndex = 3;
            btnSend.Text = "Gửi";
            btnSend.UseVisualStyleBackColor = false;

            // ============= UC_CHAT =============
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnSend);
            Controls.Add(txtMessage);
            Controls.Add(lstChat);
            Controls.Add(pnlChat);
            Name = "UC_Chat";
            Size = new System.Drawing.Size(764, 560);

            pnlChat.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlChat;
        private Label lblTitle;
        private TextBox txtMessage;
        private Button btnSend;
        private ListBox lstChat;
    }
}
