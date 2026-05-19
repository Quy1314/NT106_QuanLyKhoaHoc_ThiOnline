using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Notification : UserControl
    {
        public UC_Notification()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyAcademicStyle();
            _ = LoadDataAsync();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnMarkAsRead, 10);
        }

        private void ApplyAcademicStyle()
        {
            BackColor = AppColors.BgBase;
            StudentTabChrome.StylePrimaryButton(btnMarkAsRead);
            StudentTabChrome.StyleGrid(dgvNotifications);
        }

        private void BuildCardLayout()
        {
            btnMarkAsRead.Text = "Đánh dấu đã đọc";
            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Thông báo",
                "Các nhắc lịch, bài kiểm tra, tài liệu mới và phản hồi từ hệ thống.",
                btnMarkAsRead), 0, 0);
            root.Controls.Add(StudentTabChrome.CreateDataCard("Thông báo gần đây", dgvNotifications), 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvNotifications);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.NotificationList);
            try
            {
                await System.Threading.Tasks.Task.Delay(500);
                LoadDummyData();
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void LoadDummyData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Tiêu đề", typeof(string));
            dt.Columns.Add("Nội dung", typeof(string));
            dt.Columns.Add("Trạng thái", typeof(string));

            dt.Rows.Add("02/04 - 08:00 AM", "Kỳ thi giữa kỳ", "Bài thi giữa kỳ môn Lập trình C# đã mở.", "Chưa đọc");
            dt.Rows.Add("01/04 - 01:00 PM", "Báo nghỉ", "Cô giáo nghỉ buổi học chiều nay.", "Đã đọc");

            dgvNotifications.DataSource = dt;
            dgvNotifications.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}
