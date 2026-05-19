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
            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.TableWithToolbar);
            try
            {
                await System.Threading.Tasks.Task.Delay(500);
                LoadDataPlaceholder();
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void ApplyTheme()
        {
            // Set base appearance
            this.BackColor = AcademicTheme.AppBackground;
            this.Font = new Font("Segoe UI", 10F);

            // Header Panel
            lblTitle.ForeColor = AcademicTheme.Primary;

            // Action Panel
            txtSearch.BackColor = AcademicTheme.Surface;
            txtSearch.ForeColor = AcademicTheme.TextPrimary;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;

            btnAddExam.BackColor = AcademicTheme.Primary;
            btnAddExam.ForeColor = Color.White;
            RoundedButtonHelper.Apply(btnAddExam, 10);

            // DataGridView Default Style
            dgvExams.BackgroundColor = AcademicTheme.Surface;
            dgvExams.BorderStyle = BorderStyle.None;
            dgvExams.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvExams.GridColor = AcademicTheme.BorderSoft;
            
            dgvExams.EnableHeadersVisualStyles = false;
            dgvExams.ColumnHeadersDefaultCellStyle.BackColor = AcademicTheme.SurfaceLow;
            dgvExams.ColumnHeadersDefaultCellStyle.ForeColor = AcademicTheme.TextSecondary;
            dgvExams.ColumnHeadersDefaultCellStyle.SelectionBackColor = AcademicTheme.SurfaceLow;
            dgvExams.ColumnHeadersDefaultCellStyle.SelectionForeColor = AcademicTheme.TextSecondary;
            dgvExams.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvExams.ColumnHeadersHeight = 36;
            dgvExams.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvExams.DefaultCellStyle.BackColor = Color.White;
            dgvExams.DefaultCellStyle.ForeColor = AcademicTheme.TextPrimary;
            dgvExams.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvExams.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvExams.DefaultCellStyle.SelectionForeColor = AcademicTheme.TextPrimary;
            dgvExams.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            dgvExams.RowTemplate.Height = 38;
            AcademicTheme.StyleGrid(dgvExams);
        }

        private void LoadDataPlaceholder()
        {
            
        }
    }
}
