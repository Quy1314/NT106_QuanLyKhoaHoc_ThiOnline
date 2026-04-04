using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Teacher
{
    public partial class UC_ExamMonitor : UserControl
    {
        public UC_ExamMonitor()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ColorPalette.LightMode.Base;

            // Style Cards
            StyleCard(cardActive, lblActiveTitle, lblActiveValue, ColorPalette.Status.InfoLight);
            StyleCard(cardSubmitted, lblSubmittedTitle, lblSubmittedValue, ColorPalette.Status.SuccessLight);
            StyleCard(cardWarning, lblWarningTitle, lblWarningValue, ColorPalette.Status.ErrorLight);

            // Style DGV
            dgvMonitor.BackgroundColor = ColorPalette.LightMode.Secondary;
            dgvMonitor.ColumnHeadersDefaultCellStyle.BackColor = ColorPalette.LightMode.Border;
            dgvMonitor.ColumnHeadersDefaultCellStyle.ForeColor = ColorPalette.LightMode.TextPrimary;
            dgvMonitor.DefaultCellStyle.BackColor = ColorPalette.LightMode.Secondary;
            dgvMonitor.DefaultCellStyle.ForeColor = ColorPalette.LightMode.TextPrimary;
            dgvMonitor.AlternatingRowsDefaultCellStyle.BackColor = ColorPalette.LightMode.Base;
        }

        private void StyleCard(Panel card, Label title, Label value, Color accentBg)
        {
            card.BackColor = accentBg;
            title.ForeColor = Color.White;
            value.ForeColor = Color.White;
        }
    }
}
