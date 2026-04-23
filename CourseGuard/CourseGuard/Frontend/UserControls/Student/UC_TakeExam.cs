using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Frontend.Forms.Student;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_TakeExam : UserControl
    {
        public UC_TakeExam()
        {
            InitializeComponent();
            ApplyAcademicStyle();
            LoadDummyData();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnStartExam, 10);
        }

        private void ApplyAcademicStyle()
        {
            BackColor = AcademicTheme.AppBackground;
            lblTitle.ForeColor = AcademicTheme.TextPrimary;
            btnStartExam.BackColor = AcademicTheme.Primary;
            btnStartExam.ForeColor = Color.White;
            btnStartExam.FlatAppearance.BorderSize = 0;
            AcademicTheme.StyleGrid(dgvExams);
        }

        private void LoadDummyData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Kỳ thi", typeof(string));
            dt.Columns.Add("Khóa học", typeof(string));
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Số câu", typeof(string));
            dt.Columns.Add("Tình trạng", typeof(string));

            dt.Rows.Add("Thi giữa kỳ", "Lập trình C#", "60 phút", "50", "Đang mở");
            dt.Rows.Add("Quiz tuần 2", "Mạng máy tính", "15 phút", "10", "Chưa mở");

            dgvExams.DataSource = dt;
            dgvExams.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnStartExam_Click(object sender, EventArgs e)
        {
            // Mở form thi (DoExamForm) modal
            using (var form = new DoExamForm())
            {
                form.ShowDialog();
            }
        }
    }
}