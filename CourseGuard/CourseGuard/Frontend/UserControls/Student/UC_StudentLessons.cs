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
    public class UC_StudentLessons : UserControl, IStudentSearchTarget
    {
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly BindingSource _bindingSource = new();
        private readonly List<TeacherLessonModel> _lessons = new();

        private Label lblTitle = null!;
        private TextBox txtSearch = null!;
        private ComboBox cboCourse = null!;
        private Button btnSearch = null!;
        private Button btnRefresh = null!;
        private Button btnDetail = null!;
        private DataGridView dgvLessons = null!;
        private Label lblHint = null!;
        private RoundedPanel _lessonsBody = null!;
        private Label _emptyStateLabel = null!;

        public UC_StudentLessons()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyStyle();
            WireEvents();
            LoadLessonsAsync().FireAndForgetSafe(this);
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(20, 15),
                Text = "Bài học & Tài liệu"
            };

            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(20, 65),
                PlaceholderText = "Tìm theo tên bài học hoặc khóa học...",
                Size = new Size(330, 32)
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
                Text = "Xem chi tiết"
            };

            dgvLessons = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Location = new Point(20, 112),
                MultiSelect = false,
                Name = "dgvLessons",
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
                Text = "Chỉ hiển thị bài học thuộc các khóa học bạn đã được duyệt tham gia."
            };

            AutoScaleMode = AutoScaleMode.Font;
            BackColor = MetaTheme.Colors.SurfaceSoft;
            Controls.Add(lblTitle);
            Controls.Add(txtSearch);
            Controls.Add(cboCourse);
            Controls.Add(btnSearch);
            Controls.Add(btnRefresh);
            Controls.Add(btnDetail);
            Controls.Add(dgvLessons);
            Controls.Add(lblHint);
            Name = "UC_StudentLessons";
            Size = new Size(960, 560);

            ResumeLayout(false);
            PerformLayout();
        }

        private void ApplyStyle()
        {
            BackColor = AppColors.BgBase;
            lblTitle.ForeColor = AppColors.TextPrimary;
            lblHint.ForeColor = AppColors.TextSecondary;

            StudentTabChrome.StyleGrid(dgvLessons);
            StudentTabChrome.StylePrimaryButton(btnSearch);
            StudentTabChrome.StyleSecondaryButton(btnRefresh);
            StudentTabChrome.StylePrimaryButton(btnDetail);
            StudentTabChrome.StyleSearchInput(txtSearch);
            StudentTabChrome.StyleInput(cboCourse);
        }

        private void BuildCardLayout()
        {
            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Bài học & Tài liệu",
                "Xem nội dung bài học và tải tài liệu từ khóa học.",
                StudentTabChrome.CreateSearchBox(txtSearch, 330), cboCourse, btnSearch, btnRefresh, btnDetail), 0, 0);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgBase,
                ColumnCount = 1,
                RowCount = 2
            };
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            _lessonsBody = StudentTabChrome.CreateTableBody(dgvLessons, out _emptyStateLabel);
            content.Controls.Add(StudentTabChrome.CreateDataCard("Danh sách bài học", _lessonsBody), 0, 0);
            lblHint.Dock = DockStyle.Fill;
            lblHint.TextAlign = ContentAlignment.MiddleLeft;
            lblHint.Margin = new Padding(0, 12, 0, 0);
            content.Controls.Add(lblHint, 0, 1);
            root.Controls.Add(content, 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvLessons);
        }

        private void WireEvents()
        {
            txtSearch.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplyFilter();
                }
            };
            btnSearch.Click += (_, _) => ApplyFilter();
            btnRefresh.Click += async (_, _) => await LoadLessonsAsync();
            btnDetail.Click += (_, _) => OpenSelectedLesson();
            dgvLessons.CellDoubleClick += (_, _) => OpenSelectedLesson();
            cboCourse.SelectedIndexChanged += (_, _) => ApplyFilter();
        }

        private async Task LoadLessonsAsync()
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

                var rows = await _dbContext.GetStudentLessonsAsync(studentId);
                _lessons.Clear();
                _lessons.AddRange(rows);

                ReloadCourseFilter();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Lỗi tải bài học: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void ReloadCourseFilter()
        {
            string? selected = cboCourse.SelectedItem?.ToString();
            var courses = _lessons
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

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLowerInvariant();
            string courseFilter = cboCourse.SelectedItem?.ToString() ?? "Tất cả khóa học";

            IEnumerable<TeacherLessonModel> filtered = _lessons;

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
            txtSearch.Text = keyword ?? string.Empty;
            if (_lessons.Count > 0)
                ApplyFilter();
        }

        private void BindToGrid(List<TeacherLessonModel> rows)
        {
            DataTable table = new();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Khóa học", typeof(string));
            table.Columns.Add("Tên bài học", typeof(string));
            table.Columns.Add("Ngày đăng", typeof(string));
            table.Columns.Add("Tài liệu đính kèm", typeof(string));
            table.Columns.Add("Kích thước", typeof(string));

            foreach (var row in rows)
            {
                table.Rows.Add(
                    row.Id,
                    row.CourseName,
                    row.Title,
                    row.PublishAt.HasValue ? row.PublishAt.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    row.HasStoredContent ? row.FileName : "Không có",
                    FormatSize(row.FileSize ?? 0)
                );
            }

            _bindingSource.DataSource = table;
            dgvLessons.DataSource = _bindingSource;
            dgvLessons.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            bool hasRows = table.Rows.Count > 0;
            string emptyMessage = string.IsNullOrWhiteSpace(txtSearch.Text)
                ? "Chưa có bài học nào từ giáo viên."
                : "Không tìm thấy bài học phù hợp.";
            
            StudentTabChrome.SetTableState(_lessonsBody, dgvLessons, _emptyStateLabel, hasRows, emptyMessage);
            btnDetail.Enabled = hasRows;

            if (dgvLessons.Columns["ID"] != null)
                dgvLessons.Columns["ID"]!.Visible = false;

            dgvLessons.ClearSelection();
            dgvLessons.CurrentCell = null;
        }

        private void OpenSelectedLesson()
        {
            if (dgvLessons.CurrentRow == null || dgvLessons.CurrentRow.IsNewRow)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một bài học.", "Thông báo");
                return;
            }

            int lessonId = Convert.ToInt32(dgvLessons.CurrentRow.Cells["ID"].Value);
            var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId);
            
            if (lesson == null) return;

            int studentId = UserSessionContext.CurrentUserId ?? 0;
            using var dialog = new CourseGuard.Frontend.Forms.Student.StudentLessonDetailDialog(_dbContext, lesson, studentId);
            dialog.ShowDialog(FindForm());
        }

        private static string FormatSize(long bytes)
        {
            if (bytes <= 0) return "";
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024d:0.#} KB";
            return $"{bytes / 1024d / 1024d:0.#} MB";
        }
    }
}
