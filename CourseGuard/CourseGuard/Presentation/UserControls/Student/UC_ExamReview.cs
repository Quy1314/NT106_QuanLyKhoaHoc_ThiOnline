using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Student
{
    public class UC_ExamReview : UserControl
    {
        public UC_ExamReview()
        {
            this.BackColor = ColorPalette.LightMode.Base;

            Label lbl = new Label
            {
                Text = "Exam Review",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 50)
            };

            this.Controls.Add(lbl);
        }
    }
}