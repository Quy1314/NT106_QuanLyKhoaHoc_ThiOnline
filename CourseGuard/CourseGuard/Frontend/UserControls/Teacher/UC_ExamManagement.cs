using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_ExamManagement : UserControl
    {
        public UC_ExamManagement()
        {
            InitializeComponent();
            ApplyTheme();
            LoadDataPlaceholder();
        }

        private void ApplyTheme()
        {
            // Set base appearance
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 10F);

            // Header Panel
            lblTitle.ForeColor = ColorPalette.LightMode.Accent; // Using Accent as the primary blue color

            // Action Panel
            txtSearch.BackColor = ColorPalette.LightMode.Secondary;
            txtSearch.ForeColor = ColorPalette.LightMode.TextPrimary;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;

            btnAddExam.BackColor = ColorPalette.LightMode.Accent;
            btnAddExam.ForeColor = Color.White;
            RoundedButtonHelper.Apply(btnAddExam, 15);

            // DataGridView Default Style
            dgvExams.BackgroundColor = Color.White;
            dgvExams.BorderStyle = BorderStyle.None;
            dgvExams.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvExams.GridColor = ColorPalette.LightMode.Border;
            
            dgvExams.EnableHeadersVisualStyles = false;
            dgvExams.ColumnHeadersDefaultCellStyle.BackColor = ColorPalette.LightMode.Base;
            dgvExams.ColumnHeadersDefaultCellStyle.ForeColor = ColorPalette.LightMode.TextSecondary;
            dgvExams.ColumnHeadersDefaultCellStyle.SelectionBackColor = ColorPalette.LightMode.Base;
            dgvExams.ColumnHeadersDefaultCellStyle.SelectionForeColor = ColorPalette.LightMode.TextSecondary;
            dgvExams.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvExams.ColumnHeadersHeight = 36;
            dgvExams.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvExams.DefaultCellStyle.BackColor = Color.White;
            dgvExams.DefaultCellStyle.ForeColor = ColorPalette.LightMode.TextPrimary;
            dgvExams.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvExams.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvExams.DefaultCellStyle.SelectionForeColor = ColorPalette.LightMode.TextPrimary;
            dgvExams.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            dgvExams.RowTemplate.Height = 38;
        }

        private void LoadDataPlaceholder()
        {
            
        }
    }
}
