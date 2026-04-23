using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;
namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class TeacherDashboard : Form
    {
        private UserControl activeUserControl = null;
        private int _currentTeacherId = 0;
        private string _currentTeacherUsername = string.Empty;

        public TeacherDashboard()
        {
            InitializeComponent();
            ApplyAdminLikeTheme();
            HideAllSubMenus();
            AttachHoverEvents();
            InitializeEmailDropdown();
        }

        private void ApplyAdminLikeTheme()
        {
            BackColor = AcademicTheme.AppBackground;
            pnlSidebar.BackColor = AcademicTheme.Surface;
            pnlLogo.BackColor = AcademicTheme.Surface;
            pnlSidebarBottom.BackColor = AcademicTheme.Surface;
            pnlDivider.BackColor = AcademicTheme.BorderSoft;
            pnlTopHeader.BackColor = AcademicTheme.Surface;
            pnlMainboard.BackColor = AcademicTheme.AppBackground;
            pnlSubMenuCourseDocs.BackColor = AcademicTheme.AppBackground;
            pnlSubMenuTesting.BackColor = AcademicTheme.AppBackground;
            pnlSubMenuMonitoring.BackColor = AcademicTheme.AppBackground;

            lblLogoText.ForeColor = AcademicTheme.TextPrimary;
            lblLogoIcon.ForeColor = AcademicTheme.Primary;
            lblEmailHeader.ForeColor = AcademicTheme.Primary;
            btnMail.ForeColor = AcademicTheme.TextSecondary;

            StyleButtonTree(pnlSidebar);
            btnLogout.BackColor = Color.FromArgb(220, 38, 38);
            btnLogout.ForeColor = Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
        }

        private static void StyleButtonTree(Control root)
        {
            foreach (Control c in root.Controls)
            {
                if (c is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 0;
                    if (b.Name != "btnLogout")
                    {
                        b.BackColor = Color.Transparent;
                        b.ForeColor = AcademicTheme.TextSecondary;
                    }
                }

                if (c.HasChildren)
                {
                    StyleButtonTree(c);
                }
            }
        }

        public TeacherDashboard(UserModel user) : this()
        {
            _currentTeacherId = user?.Id ?? 0;
            _currentTeacherUsername = user?.Username ?? string.Empty;
            // Load màn hình Tổng quan mặc định khi vừa mở
            // TODO: LoadUserControl(new UC_TeacherOverview(_currentTeacherId));
            UpdateTitle("Tổng Quan");
        }

        // ---------------------------------------------------------------
        // Phương thức trung tâm để hiển thị UserControl trong pnlMainboard
        // ---------------------------------------------------------------
        public void LoadUserControl(UserControl uc)
        {
            if (activeUserControl != null)
            {
                pnlMainboard.Controls.Remove(activeUserControl);
                activeUserControl.Dispose();
            }

            activeUserControl = uc;
            uc.Dock = DockStyle.Fill;
            pnlMainboard.Controls.Add(uc);
            uc.BringToFront();
        }

        // ---------------------------------------------------------------
        // Hover effect cho các nút sidebar
        // ---------------------------------------------------------------
        private void AttachHoverEvents()
        {
            Color colorSidebarHover = Color.FromArgb(222, 224, 255);
            Color colorLogoutHover  = ColorTranslator.FromHtml("#EF4444");

            foreach (Control c in pnlSidebar.Controls)
            {
                if (c is Button btn && btn != btnLogout)
                    btn.FlatAppearance.MouseOverBackColor = colorSidebarHover;
                else if (c is Panel pnl)
                {
                    foreach (Control subC in pnl.Controls)
                    {
                        if (subC is Button subBtn)
                            subBtn.FlatAppearance.MouseOverBackColor = colorSidebarHover;
                    }
                }
            }
            btnLogout.FlatAppearance.MouseOverBackColor = colorLogoutHover;
        }

        // ---------------------------------------------------------------
        // Quản lý trạng thái ẩn/hiện sub-menu accordion
        // ---------------------------------------------------------------
        private void HideAllSubMenus()
        {
            pnlSubMenuCourseDocs.Visible  = false;
            pnlSubMenuTesting.Visible     = false;
            pnlSubMenuMonitoring.Visible  = false;
        }

        private void ShowSubMenu(Panel subMenu)
        {
            if (!subMenu.Visible)
            {
                HideAllSubMenus();
                subMenu.Visible = true;
            }
            else
            {
                subMenu.Visible = false;
            }
        }

        private void UpdateTitle(string title)
        {
            // Tiêu đề được hiển thị bởi mỗi UserControl nên không cần cập nhật header
        }

        // ===============================================================
        //  SỰ KIỆN CLICK CÁC NÚT SIDEBAR
        // ===============================================================

        // --- Tổng Quan ---
        private void btnOverview_Click(object sender, EventArgs e)
        {
            HideAllSubMenus();
            UpdateTitle("Tổng Quan");
            SetActiveMainButton(btnOverview);
            // TODO: LoadUserControl(new UC_TeacherOverview(_currentTeacherId));
        }

        // --- Nhóm: Học Liệu & Lớp Học ---
        private void btnGroupCourseDocs_Click(object sender, EventArgs e)
        {
            ShowSubMenu(pnlSubMenuCourseDocs);
        }

        private void btnAssignedCourses_Click(object sender, EventArgs e)
        {
            UpdateTitle("Quản lý khóa học phân công");
            // TODO: LoadUserControl(new UC_AssignedCourses());
        }

        private void btnOnlineClasses_Click(object sender, EventArgs e)
        {
            UpdateTitle("Lớp học trực tuyến");
            // TODO: LoadUserControl(new UC_OnlineClasses());
        }

        // --- Nhóm: Khảo Thí & Đề Thi ---
        private void btnGroupTesting_Click(object sender, EventArgs e)
        {
            ShowSubMenu(pnlSubMenuTesting);
        }

        private void btnQuestionBank_Click(object sender, EventArgs e)
        {
            UpdateTitle("Ngân hàng câu hỏi");
            // TODO: LoadUserControl(new UC_QuestionBank());
        }

        private void btnExamConfig_Click(object sender, EventArgs e)
        {
            UpdateTitle("Cấu hình đề thi");
            // TODO: LoadUserControl(new UC_ExamConfig());
        }

        private void btnExamList_Click(object sender, EventArgs e)
        {
            UpdateTitle("Quản lý kỳ thi");
            LoadUserControl(new UC_ExamManagement());
        }

        // --- Nhóm: Giám Sát & Chấm Điểm ---
        private void btnGroupMonitoring_Click(object sender, EventArgs e)
        {
            ShowSubMenu(pnlSubMenuMonitoring);
        }

        private void btnLiveMonitor_Click(object sender, EventArgs e)
        {
            UpdateTitle("Giám sát Live");
            // TODO: LoadUserControl(new UC_ExamMonitor());
        }

        private void btnEssayGrading_Click(object sender, EventArgs e)
        {
            UpdateTitle("Chấm tự luận");
            // TODO: LoadUserControl(new UC_EssayGrading());
        }

        private void btnScoreManagement_Click(object sender, EventArgs e)
        {
            UpdateTitle("Quản lý điểm số");
            LoadUserControl(new UC_ScoreManagement());
        }

        // --- Thông Báo ---
        private void btnNotifications_Click(object sender, EventArgs e)
        {
            UpdateTitle("Trung Tâm Thông Báo");
            SetActiveMainButton(btnNotifications);
            LoadUserControl(new UC_Notification(_currentTeacherId));
        }

        private void SetActiveMainButton(Button activeButton)
        {
            Button[] mainButtons = { btnOverview, btnGroupCourseDocs, btnGroupTesting, btnGroupMonitoring, btnNotifications };
            foreach (var btn in mainButtons)
            {
                if (btn == activeButton)
                {
                    btn.BackColor = AcademicTheme.Primary;
                    btn.ForeColor = Color.White;
                }
                else
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = AcademicTheme.TextSecondary;
                }
            }
        }

        // --- Đăng Xuất ---
        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống không?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                var authService = new CourseGuard.Backend.Controllers.AuthController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
                authService.Logout(_currentTeacherId, _currentTeacherUsername);
                this.Close();
            }
        }
        // ===============================================================
        //  MAIL DROPDOWN SỰ KIỆN VÀ LOGIC
        // ===============================================================

        private void InitializeEmailDropdown()
        {
            // Add custom paint to pnlEmailDropdown to mimic slightly rounded corners since it's a panel
            pnlEmailDropdown.Paint += (s, e) =>
            {
                Panel pnl = (Panel)s;
                int radius = 15;
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(pnl.Width - radius - 1, 0, radius, radius, 270, 90);
                path.AddArc(pnl.Width - radius - 1, pnl.Height - radius - 1, radius, radius, 0, 90);
                path.AddArc(0, pnl.Height - radius - 1, radius, radius, 90, 90);
                path.CloseFigure();
                pnl.Region = new System.Drawing.Region(path);

                // Draw a subtle border
                using (Pen pen = new Pen(ColorPalette.LightMode.Border, 1.5f))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // Call async method to load emails without blocking the UI
            _ = LoadEmailsAsync();
        }

        private async System.Threading.Tasks.Task LoadEmailsAsync()
        {
            try
            {
                var gmailHelper = new CourseGuard.Backend.Services.GmailServiceHelper();
                var emails = await gmailHelper.GetLatestEmailsAsync(10);
                
                // Ensure we update UI on the UI thread
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => PopulateEmails(emails)));
                }
                else
                {
                    PopulateEmails(emails);
                }
            }
            catch (Exception ex)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => ShowEmailError(ex.Message)));
                }
                else
                {
                    ShowEmailError(ex.Message);
                }
            }
        }

        private void PopulateEmails(System.Collections.Generic.List<CourseGuard.Backend.Services.EmailItem> emails)
        {
            flpEmails.SuspendLayout();
            flpEmails.Controls.Clear();
            foreach (var email in emails)
            {
                flpEmails.Controls.Add(new UC_EmailCard(email.Sender, email.Subject, email.Snippet, email.Date));
            }
            flpEmails.ResumeLayout();
        }

        private void ShowEmailError(string errorMsg)
        {
            flpEmails.Controls.Clear();
            var errorLabel = new Label 
            { 
                Text = "Could not load emails: " + errorMsg, 
                AutoSize = true, 
                ForeColor = Color.Red, 
                Padding = new Padding(10) 
            };
            flpEmails.Controls.Add(errorLabel);
        }

        private void btnMail_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Toggle the visibility of the email dropdown panel. 
            /// Ensure it sits on the absolute top of the Z-index by using BringToFront.
            /// </summary>
            pnlEmailDropdown.Visible = !pnlEmailDropdown.Visible;
            if (pnlEmailDropdown.Visible)
            {
                pnlEmailDropdown.BringToFront();
            }
        }

        private void btnMail_MouseEnter(object sender, EventArgs e)
        {
            btnMail.ForeColor = ColorPalette.LightMode.Hover; // Highlight on hover
        }

        private void btnMail_MouseLeave(object sender, EventArgs e)
        {
            btnMail.ForeColor = ColorPalette.LightMode.TextSecondary; // Revert original color
        }
    }
}
