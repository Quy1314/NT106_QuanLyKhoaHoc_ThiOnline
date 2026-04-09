/*
 * UC_ScoreManagement.cs
 *
 * Layer: Presentation (UserControls/Teacher)
 * Vai trò: Quản lý điểm số sinh viên — hiển thị, tìm kiếm, lọc, import/export CSV.
 * Phụ thuộc:
 *   - CourseGuard.Presentation.Theme.ColorPalette  : bảng màu giao diện
 *   - CsvHelper (NuGet)                             : đọc/ghi file CSV
 *
 * Ghi chú: Dữ liệu hiện tại dùng mock data để demo giao diện.
 *          Thay thế LoadMockData() bằng query DB khi tích hợp thật.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using CourseGuard.Presentation.Theme;

namespace CourseGuard.Presentation.UserControls.Teacher
{
    public partial class UC_ScoreManagement : UserControl
    {
        // ── Biến toàn cục ────────────────────────────────────────────────────────

        /// <summary>
        /// Danh sách nguồn chứa toàn bộ điểm số sinh viên (chưa áp bộ lọc).
        /// </summary>
        private List<StudentScore> _allScores;

        /// <summary>
        /// Cờ trạng thái: true = đang lọc chỉ hiển thị sinh viên "Không đạt".
        /// </summary>
        private bool _isFilteringFailed = false;

        // ── Constructor ──────────────────────────────────────────────────────────

        public UC_ScoreManagement()
        {
            InitializeComponent();

            // Khởi tạo danh sách và nạp dữ liệu mẫu
            _allScores = new List<StudentScore>();
            LoadMockData();

            // Đăng ký sự kiện
            BindEvents();

            // Áp dụng giao diện theo ColorPalette
            FormatUI();

            // Hiển thị toàn bộ dữ liệu ban đầu
            RefreshGrid(_allScores);
        }

        // ── Dữ liệu mẫu ─────────────────────────────────────────────────────────

        /// <summary>
        /// Nạp danh sách điểm mẫu để demo giao diện.
        /// Bao gồm nhiều trường hợp: Đạt, Không đạt, điểm cao/thấp.
        /// </summary>
        private void LoadMockData()
        {
            _allScores = new List<StudentScore>
            {
                new StudentScore { MSSV = "24521001", HoTen = "Vũ Trường Giang",   Lop = "Lập Trình Mạng Nâng Cao", DiemGK = 6.5, DiemCK = 7.0 },
                new StudentScore { MSSV = "24521002", HoTen = "Đỗ Thị Hoa",        Lop = "Lập Trình Mạng Nâng Cao", DiemGK = 5.0, DiemCK = 4.0 },
                new StudentScore { MSSV = "24521003", HoTen = "Bùi Quốc Khánh",    Lop = "Lập Trình Mạng Nâng Cao", DiemGK = 7.0, DiemCK = 7.5 },
                new StudentScore { MSSV = "24521004", HoTen = "Ngô Tùng Lâm",      Lop = "Lập Trình Mạng Nâng Cao", DiemGK = 4.0, DiemCK = 3.5 },
                new StudentScore { MSSV = "24521005", HoTen = "Trần Trà My",        Lop = "Lập Trình Mạng Nâng Cao", DiemGK = 8.0, DiemCK = 8.5 },
                new StudentScore { MSSV = "24521006", HoTen = "Nguyễn Thành Nam",  Lop = "Lập Trình Mạng Nâng Cao", DiemGK = 5.5, DiemCK = 6.0 },
                new StudentScore { MSSV = "24521007", HoTen = "Lê Kiều Oanh",      Lop = "An Toàn Mạng Máy Tính",   DiemGK = 9.5, DiemCK = 9.0 },
                new StudentScore { MSSV = "24521008", HoTen = "Phạm Minh Phương",  Lop = "An Toàn Mạng Máy Tính",   DiemGK = 3.0, DiemCK = 3.5 },
                new StudentScore { MSSV = "24521009", HoTen = "Hồ Nhật Quang",     Lop = "An Toàn Mạng Máy Tính",   DiemGK = 7.5, DiemCK = 8.0 },
                new StudentScore { MSSV = "24521010", HoTen = "Vũ Thái Sơn",       Lop = "An Toàn Mạng Máy Tính",   DiemGK = 5.5, DiemCK = 6.0 },
            };
        }

        // ── Đăng ký sự kiện ─────────────────────────────────────────────────────

        private void BindEvents()
        {
            txtSearch.TextChanged += txtSearch_TextChanged;
            btnFilter.Click += btnFilter_Click;
            btnImport.Click += btnImport_Click;
            btnExport.Click += btnExport_Click;

            // Tô màu dòng "Không đạt" khi DataGridView render từng ô
            dgvScores.CellFormatting += dgvScores_CellFormatting;
        }

        // ── Tìm kiếm thời gian thực ──────────────────────────────────────────────

        /// <summary>
        /// Lọc danh sách theo MSSV hoặc Họ Tên mỗi khi nội dung TextBox thay đổi.
        /// </summary>
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        // ── Lọc sinh viên không đạt ──────────────────────────────────────────────

        /// <summary>
        /// Toggle chế độ lọc "Không đạt". Đổi màu và text nút theo trạng thái.
        /// </summary>
        private void btnFilter_Click(object sender, EventArgs e)
        {
            _isFilteringFailed = !_isFilteringFailed;

            if (_isFilteringFailed)
            {
                // Trạng thái ĐANG LỌC: nút đổi sang màu đỏ cảnh báo
                btnFilter.Text = "✕ Bỏ lọc trượt";
                btnFilter.BackColor = ColorPalette.Status.ErrorLight;
                btnFilter.ForeColor = Color.White;
                btnFilter.FlatAppearance.BorderColor = ColorPalette.Status.ErrorLight;
            }
            else
            {
                // Trạng thái BÌNH THƯỜNG: khôi phục màu gốc
                btnFilter.Text = "⚑ Lọc trượt";
                btnFilter.BackColor = ColorPalette.LightMode.Secondary;
                btnFilter.ForeColor = ColorPalette.LightMode.TextPrimary;
                btnFilter.FlatAppearance.BorderColor = ColorPalette.LightMode.Border;
            }

            ApplyFilters();
        }

        // ── Áp dụng tất cả bộ lọc hiện hành ────────────────────────────────────

        /// <summary>
        /// Kết hợp bộ lọc tìm kiếm (txtSearch) và lọc trượt (_isFilteringFailed)
        /// rồi cập nhật DataGridView.
        /// </summary>
        private void ApplyFilters()
        {
            IEnumerable<StudentScore> result = _allScores;

            // 1) Lọc theo MSSV hoặc Họ Tên (không phân biệt hoa/thường)
            string keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                result = result.Where(s =>
                    s.MSSV.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    s.HoTen.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // 2) Lọc sinh viên "Không đạt"
            if (_isFilteringFailed)
            {
                result = result.Where(s => s.TongKet < 5.0);
            }

            RefreshGrid(result.ToList());
        }

        // ── Cập nhật DataGridView ────────────────────────────────────────────────

        /// <summary>
        /// Bind danh sách điểm đã lọc vào DataGridView.
        /// Dùng SuspendLayout / ResumeLayout để tránh nhấp nháy.
        /// </summary>
        private void RefreshGrid(List<StudentScore> scores)
        {
            dgvScores.SuspendLayout();
            dgvScores.DataSource = null;
            dgvScores.DataSource = scores;

            if (dgvScores.Columns.Count > 0)
                SetColumnHeaders();

            dgvScores.ResumeLayout();
        }

        /// <summary>
        /// Ánh xạ tên thuộc tính C# sang tiêu đề cột tiếng Việt.
        /// </summary>
        private void SetColumnHeaders()
        {
            var headers = new Dictionary<string, string>
            {
                { "MSSV",      "MSSV"            },
                { "HoTen",     "Họ và Tên"       },
                { "Lop",       "Khóa Học"        },
                { "DiemGK",    "Điểm Giữa Kỳ"   },
                { "DiemCK",    "Điểm Cuối Kỳ"   },
                { "TongKet",   "Tổng Kết"        },
                { "TrangThai", "Trạng Thái"      },
            };

            foreach (DataGridViewColumn col in dgvScores.Columns)
            {
                if (headers.ContainsKey(col.Name))
                    col.HeaderText = headers[col.Name];

                // Tắt sort để tránh highlight xanh trên cột đầu tiên
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        // ── Tô màu dòng theo trạng thái ─────────────────────────────────────────

        /// <summary>
        /// Tô nền đỏ nhạt cho các dòng sinh viên "Không đạt".
        /// </summary>
        private void dgvScores_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvScores.Rows[e.RowIndex].DataBoundItem is StudentScore score)
            {
                if (score.TrangThai == "Không đạt")
                {
                    dgvScores.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 239, 239);
                    dgvScores.Rows[e.RowIndex].DefaultCellStyle.ForeColor = ColorPalette.Status.ErrorLight;
                    dgvScores.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 220, 220);
                }
                else
                {
                    dgvScores.Rows[e.RowIndex].DefaultCellStyle.BackColor = ColorPalette.LightMode.Secondary;
                    dgvScores.Rows[e.RowIndex].DefaultCellStyle.ForeColor = ColorPalette.LightMode.TextPrimary;
                    dgvScores.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
                }
            }
        }

        // ── Import CSV ───────────────────────────────────────────────────────────

        /// <summary>
        /// Mở hộp thoại chọn file CSV, đọc bằng CsvHelper và nạp vào _allScores.
        /// </summary>
        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file CSV điểm số";
                ofd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (var reader = new StreamReader(ofd.FileName, System.Text.Encoding.UTF8))
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true,
                            TrimOptions = TrimOptions.Trim,
                            MissingFieldFound = null,
                        };

                        using (var csv = new CsvReader(reader, config))
                        {
                            csv.Context.RegisterClassMap<StudentScoreCsvMap>();
                            _allScores = csv.GetRecords<StudentScore>().ToList();
                        }
                    }

                    ApplyFilters();

                    MessageBox.Show(
                        $"Import thành công {_allScores.Count} bản ghi.",
                        "Import CSV",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (HeaderValidationException hex)
                {
                    MessageBox.Show(
                        "Lỗi định dạng file CSV:\nKhông tìm thấy các cột bắt buộc (MSSV, HoTen, DiemGK, DiemCK).\n\n" + hex.Message,
                        "Lỗi Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (ReaderException rex)
                {
                    MessageBox.Show(
                        "Lỗi đọc dữ liệu CSV:\nKiểm tra lại định dạng số trong file.\n\n" + rex.Message,
                        "Lỗi Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Đã xảy ra lỗi khi import:\n" + ex.Message,
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Export CSV ───────────────────────────────────────────────────────────

        /// <summary>
        /// Xuất dữ liệu đang hiển thị trên DataGridView (đã lọc) ra file CSV.
        /// </summary>
        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Lưu file CSV điểm số";
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = $"Diem_SinhVien_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var currentData = dgvScores.DataSource as List<StudentScore>;
                    if (currentData == null || currentData.Count == 0)
                    {
                        MessageBox.Show("Không có dữ liệu để xuất.",
                            "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (var writer = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true,
                        };

                        using (var csv = new CsvWriter(writer, config))
                        {
                            csv.Context.RegisterClassMap<StudentScoreCsvMap>();
                            csv.WriteRecords(currentData);
                        }
                    }

                    MessageBox.Show(
                        $"Export thành công {currentData.Count} bản ghi.\nFile: {sfd.FileName}",
                        "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi khi export:\n" + ex.Message,
                        "Lỗi Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Định dạng giao diện ──────────────────────────────────────────────────

        private void FormatUI()
        {
            this.BackColor = ColorPalette.LightMode.Base;

            panelHeader.BackColor = ColorPalette.LightMode.Secondary;

            // Đồng bộ với UC_TeacherOverview: chữ hoa + màu Accent
            lblTitle.ForeColor = ColorPalette.LightMode.Accent;
            lblTitle.Font = new Font("Segoe UI", lblTitle.Font.Size, FontStyle.Bold);
            lblTitle.Text = lblTitle.Text.ToUpper();

            lblSubtitle.ForeColor = Color.FromArgb(96, 130, 182); // xanh nhạt hơn Accent

            // Nút Export — màu Accent nổi bật
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.BackColor = ColorPalette.LightMode.Accent;
            btnExport.ForeColor = ColorPalette.LightMode.Secondary;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.FlatAppearance.MouseOverBackColor = ColorPalette.LightMode.Hover;
            btnExport.FlatAppearance.MouseDownBackColor = ColorPalette.LightMode.Active;
            btnExport.Cursor = Cursors.Hand;

            // Nút Import & Filter — phong cách phụ
            StyleSecondaryButton(btnImport);
            StyleSecondaryButton(btnFilter);
            btnFilter.Text = "⚑ Lọc trượt";

            // TextBox tìm kiếm
            txtSearch.BackColor = ColorPalette.LightMode.Secondary;
            txtSearch.ForeColor = ColorPalette.LightMode.TextPrimary;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 9.5F);

            // DataGridView
            StyleDataGridView();
        }

        private void StyleSecondaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = ColorPalette.LightMode.Secondary;
            btn.ForeColor = ColorPalette.LightMode.TextPrimary;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = ColorPalette.LightMode.Border;
            btn.FlatAppearance.MouseOverBackColor = ColorPalette.LightMode.Base;
            btn.Cursor = Cursors.Hand;
        }

        private void StyleDataGridView()
        {
            dgvScores.BackgroundColor = ColorPalette.LightMode.Secondary;
            dgvScores.BorderStyle = BorderStyle.None;
            dgvScores.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvScores.GridColor = ColorPalette.LightMode.Border;
            dgvScores.RowHeadersVisible = false;
            dgvScores.AllowUserToAddRows = false;
            dgvScores.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvScores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvScores.ScrollBars = ScrollBars.Both;

            dgvScores.EnableHeadersVisualStyles = false;
            dgvScores.ColumnHeadersDefaultCellStyle.BackColor = ColorPalette.LightMode.Base;
            dgvScores.ColumnHeadersDefaultCellStyle.ForeColor = ColorPalette.LightMode.TextSecondary;
            // SelectionBackColor = cùng màu nền → header không đổi màu khi cột được chọn
            dgvScores.ColumnHeadersDefaultCellStyle.SelectionBackColor = ColorPalette.LightMode.Base;
            dgvScores.ColumnHeadersDefaultCellStyle.SelectionForeColor = ColorPalette.LightMode.TextSecondary;
            dgvScores.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvScores.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            dgvScores.ColumnHeadersHeight = 36;
            dgvScores.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvScores.DefaultCellStyle.BackColor = ColorPalette.LightMode.Secondary;
            dgvScores.DefaultCellStyle.ForeColor = ColorPalette.LightMode.TextPrimary;
            dgvScores.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvScores.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvScores.DefaultCellStyle.SelectionForeColor = ColorPalette.LightMode.TextPrimary;
            dgvScores.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            dgvScores.RowTemplate.Height = 38;
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Data Model
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Model điểm số một sinh viên.
    /// TongKet và TrangThai là thuộc tính tính toán (get-only).
    /// </summary>
    internal class StudentScore
    {
        public string MSSV { get; set; }
        public string HoTen { get; set; }
        public string Lop { get; set; }
        public double DiemGK { get; set; }
        public double DiemCK { get; set; }

        /// <summary>GK×30% + CK×70%, làm tròn 1 chữ số thập phân.</summary>
        public double TongKet => Math.Round((DiemGK * 0.3) + (DiemCK * 0.7), 1);

        /// <summary>"Đạt" nếu TongKet >= 5.0, ngược lại "Không đạt".</summary>
        public string TrangThai => TongKet >= 5.0 ? "Đạt" : "Không đạt";
    }

    // ════════════════════════════════════════════════════════════════════════════
    // CsvHelper Class Map
    // ════════════════════════════════════════════════════════════════════════════

    internal sealed class StudentScoreCsvMap : ClassMap<StudentScore>
    {
        public StudentScoreCsvMap()
        {
            Map(m => m.MSSV).Name("MSSV");
            Map(m => m.HoTen).Name("HoTen");
            Map(m => m.Lop).Name("Lop");
            Map(m => m.DiemGK).Name("DiemGK");
            Map(m => m.DiemCK).Name("DiemCK");
            // TongKet và TrangThai là computed — chỉ export, không import
            Map(m => m.TongKet).Name("TongKet").Ignore();
            Map(m => m.TrangThai).Name("TrangThai").Ignore();
        }
    }
}
