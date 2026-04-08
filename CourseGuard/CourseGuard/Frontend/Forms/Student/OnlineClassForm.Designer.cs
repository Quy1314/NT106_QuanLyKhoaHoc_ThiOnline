namespace CourseGuard.Frontend.Forms.Student
{
    partial class OnlineClassForm
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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.picVideo = new System.Windows.Forms.PictureBox();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.btnLeave = new System.Windows.Forms.Button();
            this.flpControls = new System.Windows.Forms.FlowLayoutPanel();
            this.btnMic = new System.Windows.Forms.Button();
            this.btnCam = new System.Windows.Forms.Button();
            this.btnSpeaker = new System.Windows.Forms.Button();
            this.btnShareScreen = new System.Windows.Forms.Button();
            this.btnRaiseHand = new System.Windows.Forms.Button();
            this.btnToggleChat = new System.Windows.Forms.Button();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pnlChat = new System.Windows.Forms.Panel();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.txtChatHistory = new System.Windows.Forms.TextBox();
            this.lblChatTitle = new System.Windows.Forms.Label();
            this.pnlParticipants = new System.Windows.Forms.Panel();
            this.lstParticipants = new System.Windows.Forms.ListBox();
            this.lblPartTitle = new System.Windows.Forms.Label();
            this.tlpMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picVideo)).BeginInit();
            this.pnlToolbar.SuspendLayout();
            this.flpControls.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlChat.SuspendLayout();
            this.pnlParticipants.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpMain.Controls.Add(this.pnlLeft, 0, 0);
            this.tlpMain.Controls.Add(this.pnlRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(1200, 700);
            this.tlpMain.TabIndex = 0;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.pnlLeft.Controls.Add(this.picVideo);
            this.pnlLeft.Controls.Add(this.pnlToolbar);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.Location = new System.Drawing.Point(3, 3);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(894, 694);
            this.pnlLeft.TabIndex = 0;
            // 
            // picVideo
            // 
            this.picVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picVideo.Location = new System.Drawing.Point(0, 0);
            this.picVideo.Name = "picVideo";
            this.picVideo.Size = new System.Drawing.Size(894, 614);
            this.picVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picVideo.TabIndex = 1;
            this.picVideo.TabStop = false;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(55)))));
            this.pnlToolbar.Controls.Add(this.btnLeave);
            this.pnlToolbar.Controls.Add(this.flpControls);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 614);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(894, 80);
            this.pnlToolbar.TabIndex = 0;
            // 
            // btnLeave
            // 
            this.btnLeave.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnLeave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.btnLeave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnLeave.ForeColor = System.Drawing.Color.White;
            this.btnLeave.Location = new System.Drawing.Point(744, 18);
            this.btnLeave.Name = "btnLeave";
            this.btnLeave.Size = new System.Drawing.Size(120, 45);
            this.btnLeave.TabIndex = 1;
            this.btnLeave.Text = "Rời phòng";
            this.btnLeave.UseVisualStyleBackColor = false;
            // 
            // flpControls
            // 
            this.flpControls.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.flpControls.Controls.Add(this.btnMic);
            this.flpControls.Controls.Add(this.btnCam);
            this.flpControls.Controls.Add(this.btnSpeaker);
            this.flpControls.Controls.Add(this.btnShareScreen);
            this.flpControls.Controls.Add(this.btnRaiseHand);
            this.flpControls.Controls.Add(this.btnToggleChat);
            this.flpControls.Location = new System.Drawing.Point(120, 15);
            this.flpControls.Name = "flpControls";
            this.flpControls.Size = new System.Drawing.Size(650, 50);
            this.flpControls.TabIndex = 0;
            // 
            // btnMic
            // 
            this.btnMic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnMic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMic.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnMic.ForeColor = System.Drawing.Color.White;
            this.btnMic.Location = new System.Drawing.Point(3, 3);
            this.btnMic.Name = "btnMic";
            this.btnMic.Size = new System.Drawing.Size(85, 45);
            this.btnMic.TabIndex = 0;
            this.btnMic.Text = "Mute";
            this.btnMic.UseVisualStyleBackColor = false;
            // 
            // btnCam
            // 
            this.btnCam.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnCam.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCam.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCam.ForeColor = System.Drawing.Color.White;
            this.btnCam.Location = new System.Drawing.Point(94, 3);
            this.btnCam.Name = "btnCam";
            this.btnCam.Size = new System.Drawing.Size(100, 45);
            this.btnCam.TabIndex = 1;
            this.btnCam.Text = "Tắt Camera";
            this.btnCam.UseVisualStyleBackColor = false;
            // 
            // btnSpeaker
            // 
            this.btnSpeaker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnSpeaker.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSpeaker.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSpeaker.ForeColor = System.Drawing.Color.White;
            this.btnSpeaker.Location = new System.Drawing.Point(200, 3);
            this.btnSpeaker.Name = "btnSpeaker";
            this.btnSpeaker.Size = new System.Drawing.Size(100, 45);
            this.btnSpeaker.TabIndex = 2;
            this.btnSpeaker.Text = "Tắt Loa";
            this.btnSpeaker.UseVisualStyleBackColor = false;
            // 
            // btnShareScreen
            // 
            this.btnShareScreen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnShareScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShareScreen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnShareScreen.ForeColor = System.Drawing.Color.White;
            this.btnShareScreen.Location = new System.Drawing.Point(306, 3);
            this.btnShareScreen.Name = "btnShareScreen";
            this.btnShareScreen.Size = new System.Drawing.Size(110, 45);
            this.btnShareScreen.TabIndex = 3;
            this.btnShareScreen.Text = "Share Screen";
            this.btnShareScreen.UseVisualStyleBackColor = false;
            // 
            // btnRaiseHand
            // 
            this.btnRaiseHand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnRaiseHand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRaiseHand.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnRaiseHand.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(211)))), ((int)(((byte)(77)))));
            this.btnRaiseHand.Location = new System.Drawing.Point(422, 3);
            this.btnRaiseHand.Name = "btnRaiseHand";
            this.btnRaiseHand.Size = new System.Drawing.Size(100, 45);
            this.btnRaiseHand.TabIndex = 4;
            this.btnRaiseHand.Text = "Giơ tay ✋";
            this.btnRaiseHand.UseVisualStyleBackColor = false;
            // 
            // btnToggleChat
            // 
            this.btnToggleChat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnToggleChat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleChat.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnToggleChat.ForeColor = System.Drawing.Color.White;
            this.btnToggleChat.Location = new System.Drawing.Point(528, 3);
            this.btnToggleChat.Name = "btnToggleChat";
            this.btnToggleChat.Size = new System.Drawing.Size(110, 45);
            this.btnToggleChat.TabIndex = 5;
            this.btnToggleChat.Text = "Ẩn/Hiện Chat";
            this.btnToggleChat.UseVisualStyleBackColor = false;
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.White;
            this.pnlRight.Controls.Add(this.pnlChat);
            this.pnlRight.Controls.Add(this.pnlParticipants);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(903, 3);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(294, 694);
            this.pnlRight.TabIndex = 1;
            // 
            // pnlChat
            // 
            this.pnlChat.Controls.Add(this.txtInput);
            this.pnlChat.Controls.Add(this.txtChatHistory);
            this.pnlChat.Controls.Add(this.lblChatTitle);
            this.pnlChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlChat.Location = new System.Drawing.Point(0, 300);
            this.pnlChat.Name = "pnlChat";
            this.pnlChat.Padding = new System.Windows.Forms.Padding(10);
            this.pnlChat.Size = new System.Drawing.Size(294, 394);
            this.pnlChat.TabIndex = 1;
            // 
            // txtInput
            // 
            this.txtInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtInput.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtInput.Location = new System.Drawing.Point(10, 352);
            this.txtInput.Name = "txtInput";
            this.txtInput.PlaceholderText = "Nhập tin nhắn... (Enter để gửi)";
            this.txtInput.Size = new System.Drawing.Size(274, 32);
            this.txtInput.TabIndex = 2;
            // 
            // txtChatHistory
            // 
            this.txtChatHistory.BackColor = System.Drawing.Color.White;
            this.txtChatHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtChatHistory.Location = new System.Drawing.Point(10, 42);
            this.txtChatHistory.Multiline = true;
            this.txtChatHistory.Name = "txtChatHistory";
            this.txtChatHistory.ReadOnly = true;
            this.txtChatHistory.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtChatHistory.Size = new System.Drawing.Size(274, 342);
            this.txtChatHistory.TabIndex = 1;
            this.txtChatHistory.Text = "GV: Các em chú ý bài tập nhé!\r\n";
            // 
            // lblChatTitle
            // 
            this.lblChatTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblChatTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblChatTitle.Location = new System.Drawing.Point(10, 10);
            this.lblChatTitle.Name = "lblChatTitle";
            this.lblChatTitle.Size = new System.Drawing.Size(274, 32);
            this.lblChatTitle.TabIndex = 0;
            this.lblChatTitle.Text = "Chat Room";
            // 
            // pnlParticipants
            // 
            this.pnlParticipants.Controls.Add(this.lstParticipants);
            this.pnlParticipants.Controls.Add(this.lblPartTitle);
            this.pnlParticipants.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlParticipants.Location = new System.Drawing.Point(0, 0);
            this.pnlParticipants.Name = "pnlParticipants";
            this.pnlParticipants.Padding = new System.Windows.Forms.Padding(10);
            this.pnlParticipants.Size = new System.Drawing.Size(294, 300);
            this.pnlParticipants.TabIndex = 0;
            // 
            // lstParticipants
            // 
            this.lstParticipants.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstParticipants.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lstParticipants.FormattingEnabled = true;
            this.lstParticipants.ItemHeight = 25;
            this.lstParticipants.Items.AddRange(new object[] {
            "[GV] Nguyễn Văn A",
            "Bạn",
            "Lê Văn B",
            "Trần Thị C ✋",
            "Phạm Văn D (Muted)"});
            this.lstParticipants.Location = new System.Drawing.Point(10, 42);
            this.lstParticipants.Name = "lstParticipants";
            this.lstParticipants.Size = new System.Drawing.Size(274, 248);
            this.lstParticipants.TabIndex = 1;
            // 
            // lblPartTitle
            // 
            this.lblPartTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPartTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblPartTitle.Location = new System.Drawing.Point(10, 10);
            this.lblPartTitle.Name = "lblPartTitle";
            this.lblPartTitle.Size = new System.Drawing.Size(274, 32);
            this.lblPartTitle.TabIndex = 0;
            this.lblPartTitle.Text = "Người tham gia (5)";
            // 
            // OnlineClassForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.tlpMain);
            this.Name = "OnlineClassForm";
            this.Text = "Lớp Học Trực Tuyến";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tlpMain.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picVideo)).EndInit();
            this.pnlToolbar.ResumeLayout(false);
            this.flpControls.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.pnlChat.ResumeLayout(false);
            this.pnlChat.PerformLayout();
            this.pnlParticipants.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.FlowLayoutPanel flpControls;
        private System.Windows.Forms.Button btnMic;
        private System.Windows.Forms.Button btnCam;
        private System.Windows.Forms.Button btnSpeaker;
        private System.Windows.Forms.Button btnShareScreen;
        private System.Windows.Forms.Button btnRaiseHand;
        private System.Windows.Forms.Button btnToggleChat;
        private System.Windows.Forms.Button btnLeave;
        private System.Windows.Forms.PictureBox picVideo;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlChat;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.TextBox txtChatHistory;
        private System.Windows.Forms.Label lblChatTitle;
        private System.Windows.Forms.Panel pnlParticipants;
        private System.Windows.Forms.ListBox lstParticipants;
        private System.Windows.Forms.Label lblPartTitle;
    }
}
