using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Student
{
    public partial class UC_Result : UserControl
    {
        public UC_Result()
        {
            this.BackColor = ColorPalette.LightMode.Base;

            Label lbl = new Label
            {
                Text = "Result",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 50)
            };

            this.Controls.Add(lbl);
        }
    }
}