using System.Data;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_ExamReview : UserControl
    {
        public UC_ExamReview()
        {
            InitializeComponent();
            LoadDummyData();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnBack, 10);
        }

        private void LoadDummyData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Câu hỏi", typeof(string));
            dt.Columns.Add("Đáp án của bạn", typeof(string));
            dt.Columns.Add("Đáp án đúng", typeof(string));
            dt.Columns.Add("Đánh giá", typeof(string));

            dt.Rows.Add("Câu 1: Lớp trong C# là gì?", "A. Là bản thiết kế", "A. Là bản thiết kế", "Đúng");
            dt.Rows.Add("Câu 2: Kiểu dữ liệu int chiếm bao nhiêu byte?", "B. 8 byte", "A. 4 byte", "Sai");

            dgvReview.DataSource = dt;
            dgvReview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}