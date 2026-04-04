using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Teacher
{
    public partial class UC_ExamConfig : UserControl
    {
        public UC_ExamConfig()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ColorPalette.LightMode.Base;
            
            lblTime.ForeColor = ColorPalette.LightMode.TextPrimary;
            lblNumQuestions.ForeColor = ColorPalette.LightMode.TextPrimary;

            btnSave.BackColor = ColorPalette.LightMode.Accent;
            btnSave.ForeColor = Color.White;
            btnSave.FlatAppearance.BorderSize = 0;

            // DGV Styling
            dgvQuestions.BackgroundColor = ColorPalette.LightMode.Base;
            dgvQuestions.ColumnHeadersDefaultCellStyle.BackColor = ColorPalette.LightMode.Border;
            dgvQuestions.ColumnHeadersDefaultCellStyle.ForeColor = ColorPalette.LightMode.TextPrimary;
            dgvQuestions.DefaultCellStyle.BackColor = ColorPalette.LightMode.Secondary;
            dgvQuestions.DefaultCellStyle.ForeColor = ColorPalette.LightMode.TextPrimary;
            dgvQuestions.AlternatingRowsDefaultCellStyle.BackColor = ColorPalette.DarkMode.TextPrimary; // F9FAFB equivalent from theme
        }
    }
}
