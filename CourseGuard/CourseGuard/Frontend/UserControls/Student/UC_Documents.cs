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
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;
using Npgsql;

namespace CourseGuard.Frontend.UserControls.Student
{
    public sealed class UC_Documents : StudentGridPageBase, IStudentSearchTarget
    {
        private const string AllCoursesText = "Tất cả khóa học";
        private const string DefaultHintText = "Chỉ hiển thị tài liệu thuộc các khóa học bạn đã được duyệt tham gia.";
        private const string EmptyHintText = "Không có tài liệu phù hợp. Tài liệu chỉ hiển thị khi bạn đã được duyệt tham gia khóa học.";

        private readonly CourseGuardDbContext _dbContext = new("");
        private readonly List<StudentDocumentRow> _documents = new();
        private readonly Button _openButton = new() { Text = "Tải xuống" };

        public UC_Documents()
            : base(
                "Tài liệu khóa học",
                "Tìm kiếm, lọc và mở tài liệu từ các khóa học đã tham gia.",
                "Danh sách tài liệu",
                "Chưa có tài liệu trong các khóa học đang học.",
                hintText: DefaultHintText,
                showSearch: true,
                searchPlaceholder: "Tìm theo tên tài liệu hoặc khóa học...",
                showCourseFilter: true,
                showSearchButton: true)
        {
            Name = "UC_Documents";
            Size = new Size(960, 560);

            _openButton.Enabled = false;
            StudentTabChrome.StylePrimaryButton(_openButton);
            AddHeaderAction(_openButton);

            _openButton.Click += (_, _) => OpenSelectedDocument();

            LoadDataAsync().FireAndForgetSafe(this);
        }

        protected override string LoadErrorMessagePrefix => "Lỗi tải tài liệu";

        protected override async Task<DataTable> CreateTableAsync()
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            if (studentId == 0)
            {
                MetaTheme.ShowModernDialog(
                    "Không xác định được tài khoản. Vui lòng đăng nhập lại.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _documents.Clear();
                ReloadCourseFilter();
                return CreateDocumentsTable(new List<StudentDocumentRow>());
            }

            var rows = await Task.Run(() => QueryDocuments(studentId));
            _documents.Clear();
            _documents.AddRange(rows);

            ReloadCourseFilter();
            return CreateDocumentsTable(GetFilteredDocuments().ToList());
        }

        protected override string GetEmptyMessage()
        {
            return string.IsNullOrWhiteSpace(SearchBox?.Text)
                ? "Chưa có tài liệu trong các khóa học đang học."
                : "Không tìm thấy tài liệu phù hợp.";
        }

        protected override void OnSearchRequested() => ApplyFilter();

        protected override void OnCourseFilterChanged() => ApplyFilter();

        protected override void OnTableBound(DataTable table, bool hasRows)
        {
            _openButton.Enabled = hasRows;
            HideColumn("Đường dẫn");
            HideColumn("HasContent");

            if (HintLabel != null)
                HintLabel.Text = hasRows ? DefaultHintText : EmptyHintText;
        }

        public void ApplyGlobalSearch(string keyword)
        {
            if (SearchBox != null)
                SearchBox.Text = keyword ?? string.Empty;

            if (_documents.Count > 0)
                ApplyFilter();
        }

        private List<StudentDocumentRow> QueryDocuments(int studentId)
        {
            var result = new List<StudentDocumentRow>();

            using var connection = _dbContext.CreateConnection();
            connection.Open();
            EnsureMaterialContentSchema(connection);

            const string query = @"
                SELECT m.ID,
                       c.NAME AS COURSE_NAME,
                       COALESCE(m.FILE_NAME, '') AS FILE_NAME,
                       COALESCE(m.FILE_PATH, '') AS FILE_PATH,
                       COALESCE(u.FULL_NAME, u.USERNAME, 'Không rõ') AS UPLOADED_BY,
                       m.UPLOADED_AT,
                       COALESCE(m.CONTENT_TYPE, '') AS CONTENT_TYPE,
                       COALESCE(m.FILE_SIZE, 0) AS FILE_SIZE,
                       m.FILE_CONTENT IS NOT NULL AS HAS_CONTENT
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
                    UploadedAt = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5),
                    ContentType = reader.GetString(6),
                    FileSize = reader.GetInt64(7),
                    HasStoredContent = reader.GetBoolean(8)
                });
            }

            return result;
        }

        private void ReloadCourseFilter()
        {
            if (CourseFilter == null)
                return;

            string? selected = CourseFilter.SelectedItem?.ToString();
            var courses = _documents
                .Select(d => d.CourseName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            CourseFilter.Items.Clear();
            CourseFilter.Items.Add(AllCoursesText);
            foreach (string course in courses)
                CourseFilter.Items.Add(course);

            int selectedIndex = selected != null ? CourseFilter.Items.IndexOf(selected) : -1;
            CourseFilter.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
        }

        private void ApplyFilter()
        {
            SetGridTable(CreateDocumentsTable(GetFilteredDocuments().ToList()));
        }

        private IEnumerable<StudentDocumentRow> GetFilteredDocuments()
        {
            string keyword = SearchBox?.Text.Trim().ToLowerInvariant() ?? string.Empty;
            string courseFilter = CourseFilter?.SelectedItem?.ToString() ?? AllCoursesText;

            IEnumerable<StudentDocumentRow> filtered = _documents;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filtered = filtered.Where(d =>
                    d.FileName.ToLowerInvariant().Contains(keyword) ||
                    d.CourseName.ToLowerInvariant().Contains(keyword) ||
                    d.UploadedBy.ToLowerInvariant().Contains(keyword));
            }

            if (courseFilter != AllCoursesText)
                filtered = filtered.Where(d => d.CourseName == courseFilter);

            return filtered;
        }

        private static DataTable CreateDocumentsTable(List<StudentDocumentRow> rows)
        {
            DataTable table = new();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Khóa học", typeof(string));
            table.Columns.Add("Tên tài liệu", typeof(string));
            table.Columns.Add("Loại", typeof(string));
            table.Columns.Add("Kích thước", typeof(string));
            table.Columns.Add("Người đăng", typeof(string));
            table.Columns.Add("Ngày đăng", typeof(string));
            table.Columns.Add("Đường dẫn", typeof(string));
            table.Columns.Add("HasContent", typeof(bool));

            foreach (var row in rows)
            {
                table.Rows.Add(
                    row.Id,
                    row.CourseName,
                    row.FileName,
                    row.FileType,
                    FormatSize(row.FileSize),
                    row.UploadedBy,
                    row.UploadedAt == DateTime.MinValue ? "" : row.UploadedAt.ToString("dd/MM/yyyy HH:mm"),
                    row.FilePath,
                    row.HasStoredContent);
            }

            return table;
        }

        private void OpenSelectedDocument()
        {
            int materialId = CurrentInt("ID");
            if (materialId <= 0 || Grid.CurrentRow == null)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một tài liệu.", "Thông báo");
                return;
            }

            string fileName = CurrentString("Tên tài liệu");
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "tailieu";

            bool hasContent = Convert.ToBoolean(Grid.CurrentRow.Cells["HasContent"].Value);
            string path = CurrentString("Đường dẫn");

            if (hasContent)
            {
                byte[]? content = LoadDocumentContent(materialId);
                if (content == null || content.Length == 0)
                {
                    MetaTheme.ShowModernDialog("Không tìm thấy nội dung file để tải.", "Thông báo");
                    return;
                }

                using var save = new SaveFileDialog
                {
                    FileName = fileName,
                    Filter = "Tất cả file (*.*)|*.*"
                };
                if (save.ShowDialog(FindForm()) != DialogResult.OK)
                    return;

                File.WriteAllBytes(save.FileName, content);
                MetaTheme.ShowModernDialog("Đã tải tài liệu.", "Thông báo");
                try
                {
                    Process.Start(new ProcessStartInfo(save.FileName) { UseShellExecute = true });
                }
                catch
                {
                    // Ignore if no default app is available.
                }
                return;
            }

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
                MetaTheme.ShowModernDialog(
                    "Không thể mở hoặc tải tài liệu: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private byte[]? LoadDocumentContent(int materialId)
        {
            int studentId = UserSessionContext.CurrentUserId ?? 0;
            using var connection = _dbContext.CreateConnection();
            connection.Open();
            EnsureMaterialContentSchema(connection);
            using var command = new NpgsqlCommand(@"
                SELECT m.file_content
                FROM materials m
                JOIN courses c ON c.id = m.course_id
                JOIN enrollments e ON e.course_id = c.id
                WHERE m.id = @material_id
                  AND e.student_id = @student_id
                  AND UPPER(COALESCE(e.status, '')) IN ('ACTIVE', 'APPROVED')
                  AND UPPER(COALESCE(c.status, '')) = 'ACTIVE'", connection);
            command.Parameters.AddWithValue("@material_id", materialId);
            command.Parameters.AddWithValue("@student_id", studentId);
            object? value = command.ExecuteScalar();
            return value == null || value == DBNull.Value ? null : (byte[])value;
        }

        private static void EnsureMaterialContentSchema(NpgsqlConnection connection)
        {
            using var command = new NpgsqlCommand(@"
                ALTER TABLE materials
                    ADD COLUMN IF NOT EXISTS content_type VARCHAR(120),
                    ADD COLUMN IF NOT EXISTS file_size BIGINT NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS file_content BYTEA;", connection);
            command.ExecuteNonQuery();
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

        private static string FormatSize(long bytes)
        {
            if (bytes <= 0)
                return "";
            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024d:0.#} KB";
            return $"{bytes / 1024d / 1024d:0.#} MB";
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
            public string ContentType { get; init; } = string.Empty;
            public long FileSize { get; init; }
            public bool HasStoredContent { get; init; }
        }
    }
}
