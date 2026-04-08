using System.Data;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Notification : UserControl
    {
        public UC_Notification()
        {
            InitializeComponent();
            LoadDummyData();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnMarkAsRead, 10);
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