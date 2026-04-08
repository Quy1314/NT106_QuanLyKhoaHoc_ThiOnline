using System.Data;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Result : UserControl
    {
        public UC_Result()
        {
            InitializeComponent();
            LoadDummyData();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnReview, 10);
        }

        private void LoadDummyData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Kỳ thi", typeof(string));
            dt.Columns.Add("Khóa học", typeof(string));
            dt.Columns.Add("Số câu đúng", typeof(string));
            dt.Columns.Add("Điểm", typeof(string));
            dt.Columns.Add("Xếp loại", typeof(string));

            dt.Rows.Add("Thi giữa kỳ", "Lập trình C#", "45/50", "9.0", "Giỏi");
            dt.Rows.Add("Quiz tuần 1", "Mạng máy tính", "8/10", "8.0", "Khá");

            dgvResults.DataSource = dt;
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}