using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Profile : UserControl
    {
        private readonly AuthController _authController = new(new CourseGuardDbContext(""));

        public UC_Profile()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyCardStyle();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnSave, 10);

            Load += async (s, e) => {
                this.ShowSkeleton(SkeletonType.ProfileForm);
                await System.Threading.Tasks.Task.Delay(500);
                this.HideSkeleton();
            };

            btnSave.Click += (s, e) => {
                if (!UserSessionContext.CurrentUserId.HasValue || UserSessionContext.CurrentUserId.Value <= 0)
                {
                    MessageBox.Show("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string oldPassword = txtOldPassword.Text.Trim();
                string newPassword = txtNewPassword.Text.Trim();
                bool changed = _authController.ChangePassword(UserSessionContext.CurrentUserId.Value, oldPassword, newPassword);
                if (!changed)
                {
                    string detail = string.IsNullOrWhiteSpace(_authController.LastErrorMessage)
                        ? "Đổi mật khẩu thất bại."
                        : _authController.LastErrorMessage;
                    MessageBox.Show(detail, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void BuildCardLayout()
        {
            btnSave.Text = "Đổi mật khẩu";
            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Hồ sơ cá nhân",
                "Xem thông tin tài khoản và cập nhật mật khẩu đăng nhập.",
                btnSave), 0, 0);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgBase,
                ColumnCount = 2,
                RowCount = 1
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var infoCard = StudentTabChrome.CreateCard();
            infoCard.Padding = new Padding(22);
            infoCard.Margin = new Padding(0, 0, 12, 0);
            var infoGrid = CreateFormGrid("Thông tin học sinh");
            AddField(infoGrid, 1, lblFullName, txtFullName);
            AddField(infoGrid, 3, lblEmail, txtEmail);
            infoCard.Controls.Add(infoGrid);

            var passwordCard = StudentTabChrome.CreateCard();
            passwordCard.Padding = new Padding(22);
            passwordCard.Margin = new Padding(12, 0, 0, 0);
            var passwordGrid = CreateFormGrid("Đổi mật khẩu");
            AddField(passwordGrid, 1, lblOldPassword, txtOldPassword);
            AddField(passwordGrid, 3, lblNewPassword, txtNewPassword);
            passwordCard.Controls.Add(passwordGrid);

            content.Controls.Add(infoCard, 0, 0);
            content.Controls.Add(passwordCard, 1, 0);
            root.Controls.Add(content, 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this);
        }

        private static TableLayoutPanel CreateFormGrid(string title)
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = System.Drawing.Color.Transparent,
                ColumnCount = 1,
                RowCount = 5
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            grid.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = System.Drawing.Color.Transparent,
                Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Text = title,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            }, 0, 0);
            return grid;
        }

        private static void AddField(TableLayoutPanel grid, int labelRow, Label label, TextBox textBox)
        {
            label.Dock = DockStyle.Fill;
            label.AutoSize = false;
            label.Margin = Padding.Empty;
            label.ForeColor = AppColors.TextSecondary;
            label.BackColor = System.Drawing.Color.Transparent;

            textBox.Dock = DockStyle.Fill;
            textBox.Margin = new Padding(0, 0, 0, 14);

            grid.Controls.Add(label, 0, labelRow);
            grid.Controls.Add(textBox, 0, labelRow + 1);
        }

        private void ApplyCardStyle()
        {
            BackColor = AppColors.BgBase;
            StudentTabChrome.StylePrimaryButton(btnSave);
            foreach (TextBox textBox in new[] { txtFullName, txtEmail, txtOldPassword, txtNewPassword })
            {
                textBox.BackColor = AppColors.BgCard;
                textBox.ForeColor = AppColors.TextPrimary;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
        }
    }
}
