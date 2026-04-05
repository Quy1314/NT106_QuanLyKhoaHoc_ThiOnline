using System.Windows.Forms;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Student
{
    public partial class UC_Profile : UserControl
    {
        public UC_Profile()
        {
            InitializeComponent();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnSave, 10);

            btnSave.Click += (s, e) => {
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtNewPassword.Clear();
                txtOldPassword.Clear();
            };

            KeyEventHandler enterHandler = (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnSave.PerformClick();
                }
            };

            txtFullName.KeyDown += enterHandler;
            txtEmail.KeyDown += enterHandler;
            txtOldPassword.KeyDown += enterHandler;
            txtNewPassword.KeyDown += enterHandler;
        }
    }
}
