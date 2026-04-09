using System;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
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
            Color colorSidebarHover = ColorTranslator.FromHtml("#1F2937");
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
            // TODO: LoadUserControl(new UC_ExamList());
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
            HideAllSubMenus();
            UpdateTitle("Trung Tâm Thông Báo");
            // TODO: LoadUserControl(new UC_TeacherNotifications());
            LoadUserControl(new UC_Notification());
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
                this.Close();
            }
        }
    }
}
