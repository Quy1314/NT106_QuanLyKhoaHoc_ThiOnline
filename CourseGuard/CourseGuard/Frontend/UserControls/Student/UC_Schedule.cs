using System.Data;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Schedule : UserControl
    {
        public UC_Schedule()
        {
            InitializeComponent();
            cboTimeFilter.SelectedIndex = 0;
            LoadDummyData();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnJoinOnline, 10);
        }

        private void LoadDummyData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Ngày giờ", typeof(string));
            dt.Columns.Add("Môn học", typeof(string));
            dt.Columns.Add("Giảng viên", typeof(string));
            dt.Columns.Add("Link học", typeof(string));

            dt.Rows.Add("02/04 - 08:00 AM", "Lập trình C#", "Nguyễn Văn A", "zoom.us/j/1234");
            dt.Rows.Add("04/04 - 01:00 PM", "Mạng máy tính", "Trần Thị B", "meet.google.com/abc");

            dgvSchedule.DataSource = dt;
            dgvSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            btnJoinOnline.Click += (s, e) => {
                CourseGuard.Frontend.Forms.Student.OnlineClassForm frm = new CourseGuard.Frontend.Forms.Student.OnlineClassForm();
                frm.Show();
            };
        }
    }
}