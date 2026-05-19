using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Forms.Student;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_TakeExam : UserControl
    {
        private readonly AuthController _authController = new(new CourseGuardDbContext(""));

        public UC_TakeExam()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyAcademicStyle();
            _ = LoadDataAsync();

            // Bo góc buttons
            RoundedButtonHelper.Apply(btnStartExam, 10);
        }

        private void ApplyAcademicStyle()
        {
            BackColor = AppColors.BgBase;
            StudentTabChrome.StylePrimaryButton(btnStartExam);
            StudentTabChrome.StyleGrid(dgvExams);
        }

        private void BuildCardLayout()
        {
            btnStartExam.Text = "Bắt đầu làm bài";
            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Bài kiểm tra",
                "Theo dõi bài kiểm tra đang mở, thời lượng và trạng thái làm bài.",
                btnStartExam), 0, 0);
            root.Controls.Add(StudentTabChrome.CreateDataCard("Danh sách bài kiểm tra", dgvExams), 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvExams);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.ExamListWithToolbar);
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
            dt.Columns.Add("Kỳ thi", typeof(string));
            dt.Columns.Add("Khóa học", typeof(string));
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Số câu", typeof(string));
            dt.Columns.Add("Tình trạng", typeof(string));

            dt.Rows.Add("Thi giữa kỳ", "Lập trình C#", "60 phút", "50", "Đang mở");
            dt.Rows.Add("Quiz tuần 2", "Mạng máy tính", "15 phút", "10", "Chưa mở");

            dgvExams.DataSource = dt;
            dgvExams.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvExams.ClearSelection();
            dgvExams.CurrentCell = null;
        }

        private void btnStartExam_Click(object sender, EventArgs e)
        {
            if (dgvExams.CurrentRow == null || dgvExams.CurrentRow.IsNewRow)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một bài kiểm tra.", "Thông báo");
                return;
            }

            int? userId = UserSessionContext.CurrentUserId > 0 ? UserSessionContext.CurrentUserId : null;
            string username = UserSessionContext.CurrentUsername ?? "unknown";
            string examName = dgvExams.CurrentRow.Cells.Count > 0
                ? dgvExams.CurrentRow.Cells[0].Value?.ToString() ?? "unknown-exam"
                : "unknown-exam";
            _authController.LogUserActivity(userId, "EXAM_JOIN", $"User {username} joined exam: {examName}", string.Empty);

            // Mở form thi (DoExamForm) modal
            using (var form = new DoExamForm())
            {
                form.ShowDialog();
            }
        }
    }
}
