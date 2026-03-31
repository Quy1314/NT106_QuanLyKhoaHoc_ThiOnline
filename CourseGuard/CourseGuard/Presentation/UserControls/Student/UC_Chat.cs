using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Student
{
    public class UC_Chat : UserControl
    {
        public UC_Chat()
        {
            this.BackColor = ColorPalette.LightMode.Base;

            Label lbl = new Label
            {
                Text = "Chat",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 50)
            };

            this.Controls.Add(lbl);
        }
    }
}