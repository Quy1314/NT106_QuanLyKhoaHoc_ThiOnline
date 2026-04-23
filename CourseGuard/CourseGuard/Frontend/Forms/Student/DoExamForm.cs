using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public partial class DoExamForm : Form
    {
        public DoExamForm()
        {
            InitializeComponent();
            ApplyAcademicExamTheme();
            LoadDummyQuestionsBox();
            btnSubmit.Click += (s, e) => {
                var res = MessageBox.Show("Bạn có chắc chắn muốn nộp bài không?", "Xác nhận", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes) this.Close();
            };

            // Bo góc buttons
            RoundedButtonHelper.Apply(10, btnSubmit, btnPrev, btnNext);
        }

        private void ApplyAcademicExamTheme()
        {
            BackColor = AcademicTheme.AppBackground;
            pnlHeader.BackColor = AcademicTheme.PrimaryStrong;
            pnlMain.BackColor = AcademicTheme.SurfaceLow;
            pnlRightSidebar.BackColor = AcademicTheme.Surface;
            lblExamName.ForeColor = Color.White;
            lblSidebarTitle.ForeColor = AcademicTheme.TextPrimary;
            lblQuestionText.ForeColor = AcademicTheme.TextPrimary;
            btnSubmit.BackColor = Color.FromArgb(34, 197, 94);
            btnNext.BackColor = AcademicTheme.Primary;
            btnNext.ForeColor = Color.White;
            btnPrev.BackColor = AcademicTheme.Surface;
            btnPrev.ForeColor = AcademicTheme.TextSecondary;
            btnPrev.FlatAppearance.BorderColor = AcademicTheme.BorderSoft;
            btnPrev.FlatAppearance.BorderSize = 1;
            chkMark.ForeColor = AcademicTheme.TextSecondary;
        }

        private void LoadDummyQuestionsBox()
        {
            for (int i = 1; i <= 50; i++)
            {
                Button btn = new Button
                {
                    Text = i.ToString(),
                    Width = 40,
                    Height = 40,
                    Margin = new Padding(5),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White
                };
                btn.FlatAppearance.BorderSize = 0;
                if (i <= 5) btn.BackColor = Color.FromArgb(220, 252, 231);
                if (i == 6) btn.BackColor = Color.FromArgb(254, 243, 199);
                RoundedButtonHelper.Apply(btn, 8);
                flpQuestions.Controls.Add(btn);
            }
        }
    }
}
