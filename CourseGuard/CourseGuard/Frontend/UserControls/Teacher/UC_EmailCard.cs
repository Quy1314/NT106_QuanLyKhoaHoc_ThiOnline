using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    /// <summary>
    /// User Control for displaying a single mock email inside the Email Dropdown.
    /// </summary>
    public partial class UC_EmailCard : UserControl
    {
        public UC_EmailCard(string senderName, string subject, string snippet, string timeAgo)
        {
            InitializeComponent();

            lblSender.Text = senderName;
            lblSubject.Text = string.IsNullOrWhiteSpace(snippet) ? subject : $"{subject} - {snippet}";
            lblTime.Text = timeAgo;

            ApplyTheme();
            AttachHoverEvents();
        }

        // --- Apply Global Theme Colors ---
        private void ApplyTheme()
        {
            this.BackColor = ColorPalette.LightMode.Secondary; // White background
            lblSender.ForeColor = ColorPalette.LightMode.TextPrimary;
            lblSubject.ForeColor = ColorPalette.LightMode.TextSecondary;
            lblTime.ForeColor = ColorPalette.LightMode.TextSecondary;
        }

        // --- Hover Effects ---
        // Adding simple subtle background change for the card when hovered.
        private void AttachHoverEvents()
        {
            this.MouseEnter += OnCardMouseEnter;
            this.MouseLeave += OnCardMouseLeave;

            // Also attach to child controls so hover isn't lost when moving over text
            foreach (Control c in this.Controls)
            {
                c.MouseEnter += OnCardMouseEnter;
                c.MouseLeave += OnCardMouseLeave;
            }
        }

        private void OnCardMouseEnter(object sender, EventArgs e)
        {
            this.BackColor = ColorPalette.LightMode.Base; // Slightly gray indicating hover
            this.Cursor = Cursors.Hand;
        }

        private void OnCardMouseLeave(object sender, EventArgs e)
        {
            this.BackColor = ColorPalette.LightMode.Secondary;
            this.Cursor = Cursors.Default;
        }
    }
}
