using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Core.Models;
using CourseGuard.Presentation.UserControls.Teacher;

namespace CourseGuard.Presentation.Forms.Teacher
{
    public partial class TeacherDashboard : Form
    {
        private UserControl activeUserControl = null;
        private int _currentTeacherId = 0;

        public TeacherDashboard()
        {
            InitializeComponent();
            HideAllSubMenus();
            AttachHoverEvents();
        }

        public TeacherDashboard(UserModel user) : this()
        {
            _currentTeacherId = user?.Id ?? 0;
            // Load màn hình Tổng quan mặc định khi vừa mở
            LoadUserControl(new UC_TeacherOverview(_currentTeacherId));
            UpdateTitle("Tổng Quan");
        }

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

        private void AttachHoverEvents()
        {
            Color colorSidebarHover = ColorTranslator.FromHtml("#1F2937");
            Color colorLogoutHover = ColorTranslator.FromHtml("#EF4444");

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

        private void HideAllSubMenus()
        {
            pnlSubMenuCourseDocs.Visible = false;
            pnlSubMenuTesting.Visible = false;
            pnlSubMenuMonitoring.Visible = false;
        }

        private void ShowSubMenu(Panel subMenu)
        {
            if (subMenu.Visible == false)
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
            // pnlHeader đã được xóa; tiêu đề được hiển thị bởi mỗi UserControl
        }

        private void btnGroupCourseDocs_Click(object sender, EventArgs e)
        {
            ShowSubMenu(pnlSubMenuCourseDocs);
        }

        private void btnAssignedCourses_Click(object sender, EventArgs e)
        {
            UpdateTitle("Quản lý khóa học phân công");
            LoadUserControl(new UC_AssignedCourses());
        }

        private void btnOnlineClasses_Click(object sender, EventArgs e)
        {
            UpdateTitle("Lớp học trực tuyến");
        }

        private void btnGroupTesting_Click(object sender, EventArgs e)
        {
            ShowSubMenu(pnlSubMenuTesting);
        }

        private void btnQuestionBank_Click(object sender, EventArgs e)
        {
            UpdateTitle("Ngân hàng câu hỏi");
        }

        private void btnExamConfig_Click(object sender, EventArgs e)
        {
            UpdateTitle("Cấu hình đề thi");
        }

        private void btnExamList_Click(object sender, EventArgs e)
        {
            UpdateTitle("Quản lý kỳ thi");
        }

        private void btnGroupMonitoring_Click(object sender, EventArgs e)
        {
            ShowSubMenu(pnlSubMenuMonitoring);
        }

        private void btnLiveMonitor_Click(object sender, EventArgs e)
        {
            UpdateTitle("Giám sát Live");
        }

        private void btnEssayGrading_Click(object sender, EventArgs e)
        {
            UpdateTitle("Chấm tự luận");
            LoadUserControl(new UC_EssayGrading());
        }

        private void btnScoreManagement_Click(object sender, EventArgs e)
        {
            pnlMainboard.Controls.Clear();

            // 2. Khởi tạo UserControl Quản lý điểm số (hoặc Đề thi) mà bạn vừa tạo
            UC_ScoreManagement ucScore = new UC_ScoreManagement();

            // 3. Thiết lập để UC này tự động tràn đầy diện tích của Panel chính
            ucScore.Dock = DockStyle.Fill;

            // 4. Thêm UC vào Panel để hiển thị lên màn hình
            pnlMainboard.Controls.Add(ucScore);
        }

        private void btnOverview_Click(object sender, EventArgs e)
        {
            HideAllSubMenus();
            UpdateTitle("Tổng Quan");
            LoadUserControl(new UC_TeacherOverview(_currentTeacherId));
        }

        private void btnNotifications_Click(object sender, EventArgs e)
        {
            HideAllSubMenus();
            UpdateTitle("Trung Tâm Thông Báo");
            LoadUserControl(new UC_TeacherNotifications());
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Hiển thị hộp thoại xác nhận với icon Question
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống không?", // Nội dung thông báo
                "Xác nhận đăng xuất",                                  // Tiêu đề hộp thoại
                MessageBoxButtons.YesNo,                               // Nút Yes và No
                MessageBoxIcon.Question                                // Icon dấu chấm hỏi
            );

            // Kiểm tra lựa chọn của người dùng
            if (result == DialogResult.Yes)
            {
                // Nếu chọn Yes, thực hiện đóng form
                this.Close();
            }
        }
    }
}
