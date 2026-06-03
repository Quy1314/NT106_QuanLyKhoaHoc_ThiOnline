using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public class UC_StudentAssignments : UserControl, IStudentSearchTarget
    {
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly BindingSource _bindingSource = new();
        private readonly List<StudentAssignmentRow> _assignments = new();

        private Label lblTitle = null!;
        private ComboBox cboCourse = null!;
        private Button btnSearch = null!;
        private Button btnRefresh = null!;
        private Button btnDetail = null!;
        private DataGridView dgvAssignments = null!;
        private Label lblHint = null!;
        private RoundedPanel _assignmentsBody = null!;
        private Label _emptyStateLabel = null!;

        public UC_StudentAssignments()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyStyle();
            WireEvents();
            LoadAssignmentsAsync().FireAndForgetSafe(this);
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(20, 15),
                Text = "Bài tập của tôi"
            };

            cboCourse = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(365, 65),
                Size = new Size(220, 31)
            };

            btnSearch = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(600, 63),
                Size = new Size(105, 35),
                Text = "Tìm kiếm"
            };

            btnRefresh = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(715, 63),
                Size = new Size(95, 35),
                Text = "Tải lại"
            };

            btnDetail = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(820, 63),
                Size = new Size(120, 35),
                Text = "Chi tiết / Nộp bài"
            };

            dgvAssignments = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Location = new Point(20, 112),
                MultiSelect = false,
                Name = "dgvAssignments",
                ReadOnly = true,
                RowHeadersWidth = 30,
                RowTemplate = { Height = 32 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Size = new Size(920, 375)
            };

            lblHint = new Label
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                Location = new Point(20, 500),
                Size = new Size(920, 42),
                Text = "Chỉ hiển thị các bài tập từ những khóa học bạn đang tham gia."
            };

            AutoScaleMode = AutoScaleMode.Font;
            BackColor = MetaTheme.Colors.SurfaceSoft;
            Controls.Add(lblTitle);
            Controls.Add(cboCourse);
            Controls.Add(btnSearch);
            Controls.Add(btnRefresh);
            Controls.Add(btnDetail);
            Controls.Add(dgvAssignments);
            Controls.Add(lblHint);
            Name = "UC_StudentAssignments";
            Size = new Size(960, 560);

            ResumeLayout(false);
            PerformLayout();
        }

        private void ApplyStyle()
        {
            BackColor = AppColors.BgBase;
            lblTitle.ForeColor = AppColors.TextPrimary;
            lblHint.ForeColor = AppColors.TextSecondary;

            StudentTabChrome.StyleGrid(dgvAssignments);
            StudentTabChrome.StylePrimaryButton(btnSearch);
            StudentTabChrome.StyleSecondaryButton(btnRefresh);
            StudentTabChrome.StylePrimaryButton(btnDetail);
            StudentTabChrome.StyleInput(cboCourse);
        }

        private void BuildCardLayout()
        {
            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Bài tập",
                "Xem và nộp các bài tập từ khóa học của bạn.",
                cboCourse, btnSearch, btnRefresh, btnDetail), 0, 0);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgBase,
                ColumnCount = 1,
                RowCount = 2
            };
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            _assignmentsBody = StudentTabChrome.CreateTableBody(dgvAssignments, out _emptyStateLabel);
            content.Controls.Add(StudentTabChrome.CreateDataCard("Danh sách bài tập", _assignmentsBody), 0, 0);
            
            lblHint.Dock = DockStyle.Fill;
            lblHint.TextAlign = ContentAlignment.MiddleLeft;
            lblHint.Margin = new Padding(0, 12, 0, 0);
            content.Controls.Add(lblHint, 0, 1);
            
            root.Controls.Add(content, 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvAssignments);
        }

        private void WireEvents()
        {
            btnSearch.Click += (_, _) => ApplyFilter();
            btnRefresh.Click += async (_, _) => await LoadAssignmentsAsync();
            btnDetail.Click += (_, _) => OpenSelectedAssignment();
            cboCourse.SelectedIndexChanged += (_, _) => ApplyFilter();
            dgvAssignments.CellDoubleClick += (_, _) => OpenSelectedAssignment();
        }

        private async Task LoadAssignmentsAsync()
        {
            this.ShowSkeleton(SkeletonType.TableWithToolbar);

            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                if (studentId == 0)
                {
                    MetaTheme.ShowModernDialog("Không xác định được tài khoản. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var rows = await _dbContext.GetStudentAssignmentsAsync(studentId);
                _assignments.Clear();
                _assignments.AddRange(rows);

                ReloadCourseFilter();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Lỗi tải bài tập: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void ReloadCourseFilter()
        {
            string? selected = cboCourse.SelectedItem?.ToString();
            var courses = _assignments
                .Select(d => d.CourseName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            cboCourse.Items.Clear();
            cboCourse.Items.Add("Tất cả khóa học");
            foreach (string course in courses)
                cboCourse.Items.Add(course);

            int selectedIndex = selected != null ? cboCourse.Items.IndexOf(selected) : -1;
            cboCourse.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
        }

        private string _globalSearchKeyword = string.Empty;

        private void ApplyFilter()
        {
            string keyword = _globalSearchKeyword.Trim().ToLowerInvariant();
            string courseFilter = cboCourse.SelectedItem?.ToString() ?? "Tất cả khóa học";

            IEnumerable<StudentAssignmentRow> filtered = _assignments;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filtered = filtered.Where(d =>
                    d.Title.ToLowerInvariant().Contains(keyword) ||
                    d.CourseName.ToLowerInvariant().Contains(keyword));
            }

            if (courseFilter != "Tất cả khóa học")
            {
                filtered = filtered.Where(d => d.CourseName == courseFilter);
            }

            BindToGrid(filtered.ToList());
        }

        public void ApplyGlobalSearch(string keyword)
        {
            _globalSearchKeyword = keyword ?? string.Empty;
            if (_assignments.Count > 0)
                ApplyFilter();
        }

        private void BindToGrid(List<StudentAssignmentRow> rows)
        {
            DataTable table = new();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Khóa học", typeof(string));
            table.Columns.Add("Tiêu đề", typeof(string));
            table.Columns.Add("Hạn nộp", typeof(string));
            table.Columns.Add("Trạng thái", typeof(string));
            table.Columns.Add("Đã nộp?", typeof(string));
            table.Columns.Add("Điểm", typeof(string));

            foreach (var row in rows)
            {
                string statusText = row.Status == "OPEN" ? "Đã mở" : "Đã đóng";
                string submittedText = row.IsSubmitted ? "Đã nộp" : "Chưa nộp";
                string scoreText = row.Score.HasValue ? row.Score.Value.ToString("0.##") : "Chưa chấm";

                table.Rows.Add(
                    row.AssignmentId,
                    row.CourseName,
                    row.Title,
                    row.DueDate.ToString("dd/MM/yyyy HH:mm"),
                    statusText,
                    submittedText,
                    scoreText
                );
            }

            _bindingSource.DataSource = table;
            dgvAssignments.DataSource = _bindingSource;
            dgvAssignments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            bool hasRows = table.Rows.Count > 0;
            string emptyMessage = string.IsNullOrWhiteSpace(_globalSearchKeyword)
                ? "Không có bài tập nào."
                : "Không tìm thấy bài tập phù hợp.";
            
            StudentTabChrome.SetTableState(_assignmentsBody, dgvAssignments, _emptyStateLabel, hasRows, emptyMessage);
            btnDetail.Enabled = hasRows;

            if (dgvAssignments.Columns["ID"] != null)
                dgvAssignments.Columns["ID"]!.Visible = false;

            dgvAssignments.ClearSelection();
            dgvAssignments.CurrentCell = null;
        }

        private void OpenSelectedAssignment()
        {
            if (dgvAssignments.CurrentRow == null || dgvAssignments.CurrentRow.IsNewRow)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một bài tập.", "Thông báo");
                return;
            }

            int assignmentId = Convert.ToInt32(dgvAssignments.CurrentRow.Cells["ID"].Value);
            var assignment = _assignments.FirstOrDefault(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                MetaTheme.ShowModernDialog("Không tìm thấy thông tin bài tập.", "Lỗi");
                return;
            }

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            using var dialog = new CourseGuard.Frontend.Forms.Student.StudentAssignmentSubmitDialog(_dbContext, assignment, studentId);
            
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                LoadAssignmentsAsync().FireAndForgetSafe(this); // Reload after submit
                
                if (FindForm() is CourseGuard.Frontend.Forms.Student.StudentDashboard dashboard)
                {
                    dashboard.RefreshNotificationSummary();
                }
            }
        }
    }
}
