using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Security;
using CourseGuard.Frontend.Theme;
using Npgsql;

namespace CourseGuard.Frontend.UserControls.Student
{
    public class UC_Documents : UserControl, IStudentSearchTarget
    {
        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly BindingSource _bindingSource = new();
        private readonly List<StudentDocumentRow> _documents = new();

        private Label lblTitle = null!;
        private TextBox txtSearch = null!;
        private ComboBox cboCourse = null!;
        private Button btnSearch = null!;
        private Button btnRefresh = null!;
        private Button btnOpen = null!;
        private DataGridView dgvDocuments = null!;
        private Label lblHint = null!;

        public UC_Documents()
        {
            InitializeComponent();
            BuildCardLayout();
            ApplyStyle();
            WireEvents();
            _ = LoadDocumentsAsync();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(20, 15),
                Text = "Tài liệu khóa học"
            };

            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(20, 65),
                PlaceholderText = "Tìm theo tên tài liệu hoặc khóa học...",
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

            btnOpen = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(820, 63),
                Size = new Size(120, 35),
                Text = "Mở/Tải"
            };

            dgvDocuments = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Location = new Point(20, 112),
                MultiSelect = false,
                Name = "dgvDocuments",
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
                Text = "Chỉ hiển thị tài liệu thuộc các khóa học bạn đã được duyệt tham gia."
            };

            AutoScaleMode = AutoScaleMode.Font;
            BackColor = MetaTheme.Colors.SurfaceSoft;
            Controls.Add(lblTitle);
            Controls.Add(txtSearch);
            Controls.Add(cboCourse);
            Controls.Add(btnSearch);
            Controls.Add(btnRefresh);
            Controls.Add(btnOpen);
            Controls.Add(dgvDocuments);
            Controls.Add(lblHint);
            Name = "UC_Documents";
            Size = new Size(960, 560);

            ResumeLayout(false);
            PerformLayout();
        }

        private void ApplyStyle()
        {
            BackColor = AppColors.BgBase;
            lblTitle.ForeColor = AppColors.TextPrimary;
            lblHint.ForeColor = AppColors.TextSecondary;

            StudentTabChrome.StyleGrid(dgvDocuments);
            StudentTabChrome.StylePrimaryButton(btnSearch);
            StudentTabChrome.StyleSecondaryButton(btnRefresh);
            StudentTabChrome.StylePrimaryButton(btnOpen);
            StudentTabChrome.StyleSearchInput(txtSearch);
            StudentTabChrome.StyleInput(cboCourse);
        }

        private void BuildCardLayout()
        {
            btnRefresh.Text = "Tải lại";
            btnOpen.Text = "Mở/Tải";

            var root = StudentTabChrome.CreateRoot(this);
            root.Controls.Add(StudentTabChrome.CreateHeader(
                "Tài liệu khóa học",
                "Tìm kiếm, lọc và mở tài liệu từ các khóa học đã tham gia.",
                StudentTabChrome.CreateSearchBox(txtSearch, 330), cboCourse, btnSearch, btnRefresh, btnOpen), 0, 0);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgBase,
                ColumnCount = 1,
                RowCount = 2
            };
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            content.Controls.Add(StudentTabChrome.CreateDataCard("Danh sách tài liệu", dgvDocuments), 0, 0);
            lblHint.Dock = DockStyle.Fill;
            lblHint.TextAlign = ContentAlignment.MiddleLeft;
            lblHint.Margin = new Padding(0, 12, 0, 0);
            content.Controls.Add(lblHint, 0, 1);
            root.Controls.Add(content, 0, 1);
            StudentTabChrome.EnableNaturalFocusClear(this, dgvDocuments);
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
            btnRefresh.Click += async (_, _) => await LoadDocumentsAsync();
            btnOpen.Click += (_, _) => OpenSelectedDocument();
            cboCourse.SelectedIndexChanged += (_, _) => ApplyFilter();
        }

        private async Task LoadDocumentsAsync()
        {
            this.ShowSkeleton(SkeletonType.TableWithToolbar);

            try
            {
                int studentId = UserSessionContext.CurrentUserId ?? 0;
                if (studentId == 0)
                {
                    MetaTheme.ShowModernDialog("Không xác định được tài khoản. Vui lòng đăng nhập lại.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var rows = await Task.Run(() => QueryDocuments(studentId));
                _documents.Clear();
                _documents.AddRange(rows);

                ReloadCourseFilter();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Lỗi tải tài liệu: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private List<StudentDocumentRow> QueryDocuments(int studentId)
        {
            var result = new List<StudentDocumentRow>();

            using var connection = _dbContext.CreateConnection();
            connection.Open();

            const string query = @"
                SELECT m.ID,
                       c.NAME AS COURSE_NAME,
                       COALESCE(m.FILE_NAME, '') AS FILE_NAME,
                       COALESCE(m.FILE_PATH, '') AS FILE_PATH,
                       COALESCE(u.FULL_NAME, u.USERNAME, 'Không rõ') AS UPLOADED_BY,
                       m.UPLOADED_AT
                FROM MATERIALS m
                JOIN COURSES c ON c.ID = m.COURSE_ID
                JOIN ENROLLMENTS e ON e.COURSE_ID = c.ID
                LEFT JOIN USERS u ON u.ID = m.UPLOADED_BY
                WHERE e.STUDENT_ID = @student_id
                  AND UPPER(COALESCE(e.STATUS, '')) IN ('ACTIVE', 'APPROVED')
                  AND UPPER(COALESCE(c.STATUS, '')) = 'ACTIVE'
                ORDER BY m.UPLOADED_AT DESC, m.ID DESC";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@student_id", studentId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string path = reader.GetString(3);
                result.Add(new StudentDocumentRow
                {
                    Id = reader.GetInt32(0),
                    CourseName = reader.GetString(1),
                    FileName = reader.GetString(2),
                    FilePath = path,
                    FileType = GetFileType(path, reader.GetString(2)),
                    UploadedBy = reader.GetString(4),
                    UploadedAt = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5)
                });
            }

            return result;
        }

        private void ReloadCourseFilter()
        {
            string? selected = cboCourse.SelectedItem?.ToString();
            var courses = _documents
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

            IEnumerable<StudentDocumentRow> filtered = _documents;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filtered = filtered.Where(d =>
                    d.FileName.ToLowerInvariant().Contains(keyword) ||
                    d.CourseName.ToLowerInvariant().Contains(keyword) ||
                    d.UploadedBy.ToLowerInvariant().Contains(keyword));
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
            if (_documents.Count > 0)
                ApplyFilter();
        }

        private void BindToGrid(List<StudentDocumentRow> rows)
        {
            DataTable table = new();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Khóa học", typeof(string));
            table.Columns.Add("Tên tài liệu", typeof(string));
            table.Columns.Add("Loại", typeof(string));
            table.Columns.Add("Người đăng", typeof(string));
            table.Columns.Add("Ngày đăng", typeof(string));
            table.Columns.Add("Đường dẫn", typeof(string));

            foreach (var row in rows)
            {
                table.Rows.Add(
                    row.Id,
                    row.CourseName,
                    row.FileName,
                    row.FileType,
                    row.UploadedBy,
                    row.UploadedAt == DateTime.MinValue ? "" : row.UploadedAt.ToString("dd/MM/yyyy HH:mm"),
                    row.FilePath);
            }

            _bindingSource.DataSource = table;
            dgvDocuments.DataSource = _bindingSource;
            dgvDocuments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            lblHint.Text = rows.Count == 0
                ? "Không có tài liệu phù hợp. Tài liệu chỉ hiển thị khi bạn đã được duyệt tham gia khóa học."
                : "Chỉ hiển thị tài liệu thuộc các khóa học bạn đã được duyệt tham gia.";

            DataGridViewColumn? idColumn = dgvDocuments.Columns["ID"];
            if (idColumn != null)
                idColumn.Visible = false;

            dgvDocuments.ClearSelection();
            dgvDocuments.CurrentCell = null;
        }

        private void OpenSelectedDocument()
        {
            if (dgvDocuments.CurrentRow == null || dgvDocuments.CurrentRow.IsNewRow)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một tài liệu.", "Thông báo");
                return;
            }

            string path = dgvDocuments.CurrentRow.Cells["Đường dẫn"].Value?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                MetaTheme.ShowModernDialog("Tài liệu này chưa có đường dẫn file.", "Thông báo");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể mở hoặc tải tài liệu: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetFileType(string path, string fileName)
        {
            string extension = Path.GetExtension(path);
            if (string.IsNullOrWhiteSpace(extension))
                extension = Path.GetExtension(fileName);

            return string.IsNullOrWhiteSpace(extension)
                ? "Không rõ"
                : extension.TrimStart('.').ToUpperInvariant();
        }

        private sealed class StudentDocumentRow
        {
            public int Id { get; init; }
            public string CourseName { get; init; } = string.Empty;
            public string FileName { get; init; } = string.Empty;
            public string FilePath { get; init; } = string.Empty;
            public string FileType { get; init; } = string.Empty;
            public string UploadedBy { get; init; } = string.Empty;
            public DateTime UploadedAt { get; init; }
        }
    }
}
