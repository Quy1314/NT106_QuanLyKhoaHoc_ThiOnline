using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.Forms.Student
{
    public partial class DoExamForm : Form
    {
        public DoExamForm()
        {
            InitializeComponent();
            LoadDummyQuestionsBox();
            btnSubmit.Click += (s, e) => {
                var res = MessageBox.Show("Bạn có chắc chắn muốn nộp bài không?", "Xác nhận", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes) this.Close();
            };

            // Bo góc buttons
            RoundedButtonHelper.Apply(10, btnSubmit, btnPrev, btnNext);
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
                if (i <= 5) btn.BackColor = Color.LightGreen;
                if (i == 6) btn.BackColor = Color.Orange;
                RoundedButtonHelper.Apply(btn, 8);
                flpQuestions.Controls.Add(btn);
            }
        }
    }
}
