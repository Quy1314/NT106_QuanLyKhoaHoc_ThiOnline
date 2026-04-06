using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.Forms.Student
{
    public partial class OnlineClassForm : Form
    {
        private bool isMicOn = true;
        private bool isCamOn = true;
        private bool isSpeakerOn = true;
        private bool isScreenSharing = false;
        private bool isHandRaised = false;

        // Font dùng cho Paint event (tránh memory leak)
        private readonly Font _videoFont = new Font("Segoe UI", 16, FontStyle.Bold);

        public OnlineClassForm()
        {
            InitializeComponent();
            SetupEventHandlers();
            
            // Vẽ giao diện giả cho Video Playback
            picVideo.Paint += (s, e) => {
                string msg = isCamOn ? "Teacher's Camera Stream" : "Teacher has turned off camera.";
                if (isScreenSharing) msg = "You are sharing your screen.";
                
                SizeF sz = e.Graphics.MeasureString(msg, _videoFont);
                e.Graphics.DrawString(msg, _videoFont, Brushes.White, (picVideo.Width - sz.Width) / 2, (picVideo.Height - sz.Height) / 2);
            };

            // Bo góc tất cả buttons
            RoundedButtonHelper.Apply(12, btnLeave, btnMic, btnCam, btnSpeaker,
                btnShareScreen, btnRaiseHand, btnToggleChat);
        }

        private void SetupEventHandlers()
        {
            btnLeave.Click += (s, e) => {
                var res = MessageBox.Show("Bạn có muốn rời phòng chứ?", "Xác nhận", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes) this.Close();
            };

            btnMic.Click += (s, e) => {
                isMicOn = !isMicOn;
                btnMic.Text = isMicOn ? "Mute" : "Unmute";
                btnMic.BackColor = isMicOn ? Color.FromArgb(55, 65, 81) : Color.IndianRed;
            };

            btnCam.Click += (s, e) => {
                isCamOn = !isCamOn;
                btnCam.Text = isCamOn ? "Tắt Camera" : "Bật Camera";
                btnCam.BackColor = isCamOn ? Color.FromArgb(55, 65, 81) : Color.IndianRed;
                picVideo.Invalidate();
            };

            btnSpeaker.Click += (s, e) => {
                isSpeakerOn = !isSpeakerOn;
                btnSpeaker.Text = isSpeakerOn ? "Tắt Loa" : "Bật Loa";
                btnSpeaker.BackColor = isSpeakerOn ? Color.FromArgb(55, 65, 81) : Color.IndianRed;
            };

            btnShareScreen.Click += (s, e) => {
                isScreenSharing = !isScreenSharing;
                btnShareScreen.Text = isScreenSharing ? "Stop Sharing" : "Share Screen";
                btnShareScreen.BackColor = isScreenSharing ? Color.IndianRed : Color.FromArgb(16, 185, 129);
                picVideo.Invalidate();
            };

            btnRaiseHand.Click += (s, e) => {
                isHandRaised = !isHandRaised;
                if (isHandRaised) {
                    btnRaiseHand.BackColor = Color.FromArgb(245, 158, 11); // Orange
                    btnRaiseHand.ForeColor = Color.White;
                } else {
                    btnRaiseHand.BackColor = Color.FromArgb(55, 65, 81); // Dark Gray
                    btnRaiseHand.ForeColor = Color.FromArgb(252, 211, 77);
                }
            };

            btnToggleChat.Click += (s, e) => {
                pnlChat.Visible = !pnlChat.Visible;
                if (pnlChat.Visible)
                {
                    pnlParticipants.Dock = DockStyle.Top;
                    pnlParticipants.Height = 300;
                }
                else
                {
                    pnlParticipants.Dock = DockStyle.Fill;
                }
            };
            
            // Xử lý chat enter
            txtInput.KeyDown += (s, e) => 
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    if (!string.IsNullOrEmpty(txtInput.Text))
                    {
                        txtChatHistory.AppendText("Bạn: " + txtInput.Text + "\r\n");
                        txtInput.Clear();
                    }
                }
            };
        }
    }
}
