using System;
using System.Drawing;
using System.Windows.Forms;

namespace Demo_Firebase
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void CenterControls()
        {
            if (lblWelcome != null && btnLogout != null)
            {
                lblWelcome.Location = new Point((this.ClientSize.Width - lblWelcome.Width) / 2, 70);
                btnLogout.Location = new Point((this.ClientSize.Width - btnLogout.Width) / 2, this.ClientSize.Height - 80);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterControls();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            CenterControls();
        }
    }
}
