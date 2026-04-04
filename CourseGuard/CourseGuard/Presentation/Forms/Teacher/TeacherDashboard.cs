using System;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.Forms.Teacher
{
    public partial class TeacherDashboard : Form
    {
        private Button currentBtn;

        public TeacherDashboard()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // Apply Colors via ColorPalette
            Sidebar.BackColor = ColorPalette.DarkMode.Base;
            LOGO.ForeColor = ColorPalette.DarkMode.TextPrimary;

            Header.BackColor = ColorPalette.LightMode.Secondary;
            lbl_Title.ForeColor = ColorPalette.LightMode.TextPrimary;

            Mainboard.BackColor = ColorPalette.LightMode.Base;

            // Apply styling to all buttons in sidebar
            foreach (Control control in Sidebar.Controls)
            {
                if (control is Button btn)
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = ColorPalette.DarkMode.TextSecondary;
                    btn.FlatAppearance.MouseOverBackColor = ColorPalette.DarkMode.Secondary;
                    btn.FlatAppearance.MouseDownBackColor = ColorPalette.DarkMode.Active;
                }
            }

            // Set default active button
            if (btn_Dashboard != null)
            {
                ActivateButton(btn_Dashboard);
            }
        }

        private void ActivateButton(Button senderBtn)
        {
            if (senderBtn != null)
            {
                DisableButton();
                currentBtn = senderBtn;
                currentBtn.BackColor = ColorPalette.DarkMode.Accent;
                currentBtn.ForeColor = ColorPalette.DarkMode.TextPrimary;
                lbl_Title.Text = currentBtn.Text.Substring(currentBtn.Text.IndexOf(" ") + 1).Trim(); // Extract title without emoji
            }
        }

        private void DisableButton()
        {
            if (currentBtn != null)
            {
                currentBtn.BackColor = Color.Transparent;
                currentBtn.ForeColor = ColorPalette.DarkMode.TextSecondary;
            }
        }

        public void LoadUserControl(UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            Mainboard.Controls.Clear();
            Mainboard.Controls.Add(uc);
            uc.BringToFront();
        }

        private void Sidebar_Btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            ActivateButton(btn);

            // TODO: Load corresponding UserControl based on the clicked button
            // Example:
            // if (btn == btn_ExamConfig) { LoadUserControl(new UserControls.Teacher.UC_ExamConfig()); }
        }
    }
}