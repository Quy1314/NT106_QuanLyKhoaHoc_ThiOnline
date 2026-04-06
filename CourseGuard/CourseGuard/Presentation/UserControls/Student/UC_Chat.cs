using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Student
{
    public partial class UC_Chat : UserControl
    {
        public UC_Chat()
        {
            InitializeComponent();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnSend, 10);

            btnSend.Click += (s, e) => {
                if (!string.IsNullOrEmpty(txtInput.Text))
                {
                    txtMessages.AppendText("Bạn: " + txtInput.Text + "\r\n");
                    txtInput.Clear();
                }
            };

            txtInput.KeyDown += (s, e) => 
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnSend.PerformClick();
                }
            };
        }
    }
}