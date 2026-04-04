using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Teacher
{
    public partial class UC_EssayGrading : UserControl
    {
        public UC_EssayGrading()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ColorPalette.LightMode.Base;
            splitMain.BackColor = ColorPalette.LightMode.Border; // Divider color
            splitMain.Panel1.BackColor = ColorPalette.LightMode.Base;
            splitMain.Panel2.BackColor = ColorPalette.LightMode.Base;

            panelGrading.BackColor = ColorPalette.LightMode.Secondary;
            richTextBoxEssay.BackColor = ColorPalette.LightMode.Secondary;
            richTextBoxEssay.ForeColor = ColorPalette.LightMode.TextPrimary;
            richTextBoxEssay.BorderStyle = BorderStyle.None;

            lblScore.ForeColor = ColorPalette.LightMode.TextPrimary;
            lblComment.ForeColor = ColorPalette.LightMode.TextPrimary;
            
            txtScore.BackColor = ColorPalette.LightMode.Base;
            txtComment.BackColor = ColorPalette.LightMode.Base;

            btnSaveGrade.BackColor = ColorPalette.LightMode.Accent;
            btnSaveGrade.ForeColor = Color.White;
            btnSaveGrade.FlatAppearance.BorderSize = 0;

            // DGV Styling
            dgvStudents.BackgroundColor = ColorPalette.LightMode.Secondary;
            dgvStudents.ColumnHeadersDefaultCellStyle.BackColor = ColorPalette.LightMode.Border;
            dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor = ColorPalette.LightMode.TextPrimary;
            dgvStudents.DefaultCellStyle.BackColor = ColorPalette.LightMode.Secondary;
            dgvStudents.DefaultCellStyle.ForeColor = ColorPalette.LightMode.TextPrimary;
            dgvStudents.AlternatingRowsDefaultCellStyle.BackColor = ColorPalette.LightMode.Base;
        }
    }
}
