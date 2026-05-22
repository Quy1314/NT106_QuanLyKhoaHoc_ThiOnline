using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_Result : UserControl
    {
        private readonly CourseGuardDbContext _dbContext = new("");

        public UC_Result()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyAcademicStyle();
            _ = LoadDataAsync();

            RoundedButtonHelper.Apply(btnReview, 10);
        }

        private void ApplyAcademicStyle()
        {
            BackColor = AppColors.BgBase;
            StudentTabChrome.StylePrimaryButton(btnReview);
            StudentTabChrome.StyleGrid(dgvResults);
        }

        private void BuildCardLayout()
        {
            btnReview.Text = "Xem lại bài";
            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Kết quả học tập",
                "Xem điểm, trạng thái chấm và mở lại bài làm khi được phép.",
                btnReview), 0, 0);
            root.Controls.Add(StudentTabChrome.CreateDataCard("Bảng điểm bài kiểm tra", dgvResults), 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvResults);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.ResultTable);
            try
            {
                DataTable table = await System.Threading.Tasks.Task.Run(LoadResultTable);
                BindResultTable(table);
            }
            catch (Exception ex)
            {
                BindResultTable(CreateMessageTable($"Không thể tải kết quả: {ex.Message}"));
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private DataTable LoadResultTable()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (studentId <= 0)
                return CreateMessageTable("Không xác định được tài khoản học sinh.");

            var results = _dbContext.GetStudentResultItems(studentId);
            DataTable dt = CreateResultTableSchema();

            if (results.Count == 0)
            {
                dt.Rows.Add("Chưa có kết quả học tập", "", "", "", "");
                return dt;
            }

            foreach (StudentResultListItemModel item in results)
            {
                dt.Rows.Add(
                    item.ExamTitle,
                    item.CourseName,
                    item.CorrectAnswersText,
                    item.Score.ToString("0.0", CultureInfo.InvariantCulture),
                    item.StatusText);
            }

            return dt;
        }

        private void BindResultTable(DataTable table)
        {
            dgvResults.DataSource = table;
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResults.ClearSelection();
            dgvResults.CurrentCell = null;
        }

        private static DataTable CreateResultTableSchema()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Kỳ thi", typeof(string));
            dt.Columns.Add("Khóa học", typeof(string));
            dt.Columns.Add("Số câu đúng", typeof(string));
            dt.Columns.Add("Điểm", typeof(string));
            dt.Columns.Add("Xếp loại", typeof(string));
            return dt;
        }

        private static DataTable CreateMessageTable(string message)
        {
            DataTable dt = CreateResultTableSchema();
            dt.Rows.Add(message, "", "", "", "");
            return dt;
        }
    }
}
