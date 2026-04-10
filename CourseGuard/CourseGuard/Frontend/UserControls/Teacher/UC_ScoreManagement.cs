/*
 * UC_ScoreManagement.cs
 *
 * Layer: Presentation (UserControls / Teacher)
 * Vai trò: Quản lý điểm số sinh viên — hiển thị, tìm kiếm, lọc, import/export CSV.
 * 
 * Chức năng:
 *   - Tải dữ liệu từ Supabase (ScoreRepository)
 *   - Tìm kiếm real-time theo MSSV hoặc Họ Tên
 *   - Lọc sinh viên "Không đạt"
 *   - Import/Export CSV (UPSERT vào DB)
 *   - Tô màu hàng không đạt để dễ nhìn
 *
 * Phụ thuộc:
 *   - CourseGuard.Backend.Data.ScoreRepository  : kết nối Supabase
 *   - CourseGuard.Backend.Models.StudentScoreModel : mô hình dữ liệu
 *   - CourseGuard.Frontend.Theme.ColorPalette   : bảng màu giao diện
 *   - NuGet: Npgsql, CsvHelper
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    /// <summary>
    /// UserControl quản lý điểm số sinh viên, kết nối trực tiếp với Supabase qua Npgsql.
    /// </summary>
    public partial class UC_ScoreManagement : UserControl
    {
        // ── Biến toàn cục ────────────────────────────────────────────────────────

        /// <summary>Repository tương tác với CSDL Supabase.</summary>
        private readonly ScoreRepository _repo = new ScoreRepository();

        /// <summary>Danh sách nguồn chứa toàn bộ điểm số sinh viên (chưa áp bộ lọc).</summary>
        private List<StudentScoreModel> _allScores = new List<StudentScoreModel>();

        /// <summary>Cờ trạng thái: true = đang lọc chỉ hiển thị sinh viên "Không đạt".</summary>
        private bool _isFilteringFailed = false;

        // ── Constructor ──────────────────────────────────────────────────────────

        public UC_ScoreManagement()
        {
            InitializeComponent();

            // Khởi tạo danh sách
            _allScores = new List<StudentScoreModel>();

            // Đăng ký sự kiện
            BindEvents();

            // Áp dụng giao diện theo ColorPalette
            FormatUI();

            // Tải dữ liệu từ Supabase ngay khi control xuất hiện
            this.Load += async (s, e) =>
            {
                await LoadDataFromSupabaseAsync();
            };
        }

        // ── Đăng ký sự kiện ─────────────────────────────────────────────────

        private void BindEvents()
        {
            txtSearch.TextChanged += TxtSearch_TextChanged;
            btnFilterFailed.Click += BtnFilterFailed_Click;
            btnImport.Click += BtnImport_Click;
            btnExport.Click += BtnExport_Click;

            // Tô màu dòng "Không đạt" khi DataGridView render từng ô
            dgvScores.CellFormatting += DgvScores_CellFormatting;
        }

        // ── Định dạng giao diện ──────────────────────────────────────────

        private void FormatUI()
        {
            this.BackColor = ColorPalette.LightMode.Base;

            pnlHeader.BackColor = ColorPalette.LightMode.Secondary;

            // Đồng bộ với UC_TeacherOverview: chữ hoa + màu Accent
            lblTitle.ForeColor = ColorPalette.LightMode.Accent;
            lblTitle.Font = new Font("Segoe UI", lblTitle.Font.Size, FontStyle.Bold);
            lblTitle.Text = lblTitle.Text.ToUpper();

            // Chú thích dưới tiêu đề
            lblSubtitle.ForeColor = ColorPalette.LightMode.TextSecondary;
            lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblSubtitle.Text = "Xem, tìm kiếm, lọc và import/export điểm sinh viên";

            // Nút Export — màu trắng trên nền Accent nổi bật
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.BackColor = ColorPalette.LightMode.Accent;
            btnExport.ForeColor = Color.White;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.FlatAppearance.MouseOverBackColor = ColorPalette.LightMode.Hover;
            btnExport.FlatAppearance.MouseDownBackColor = ColorPalette.LightMode.Active;
            btnExport.Cursor = Cursors.Hand;

            // Nút Import & Filter — phong cách phụ
            StyleSecondaryButton(btnImport);
            StyleSecondaryButton(btnFilterFailed);
            btnFilterFailed.Text = "⚑ Lọc trượt";

            // TextBox tìm kiếm
            txtSearch.BackColor = ColorPalette.LightMode.Secondary;
            txtSearch.ForeColor = ColorPalette.LightMode.TextPrimary;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 9.5F);
            txtSearch.PlaceholderText = "🔍  Tìm theo MSSV hoặc Họ Tên...";

            // DataGridView
            StyleDataGridView();

            // Status bar
            pnlStatus.BackColor = ColorPalette.LightMode.Secondary;
            lblStatus.ForeColor = ColorPalette.LightMode.TextSecondary;
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

        // ── Tìm kiếm thời gian thực ──────────────────────────────────────

        /// <summary>
        /// Lọc danh sách theo MSSV hoặc Họ Tên mỗi khi nội dung TextBox thay đổi.
        /// </summary>
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        // ── Lọc sinh viên không đạt ──────────────────────────────────────

        /// <summary>
        /// Toggle chế độ lọc "Không đạt". Đổi màu và text nút theo trạng thái.
        /// </summary>
        private void BtnFilterFailed_Click(object sender, EventArgs e)
        {
            _isFilteringFailed = !_isFilteringFailed;

            if (_isFilteringFailed)
            {
                // Trạng thái ĐANG LỌC: nút đổi sang màu đỏ cảnh báo
                btnFilterFailed.Text = "✕ Bỏ lọc trượt";
                btnFilterFailed.BackColor = ColorPalette.Status.ErrorLight;
                btnFilterFailed.ForeColor = Color.White;
            }
            else
            {
                // Trạng thái BÌNH THƯỜNG: khôi phục màu gốc
                btnFilterFailed.Text = "⚑ Lọc trượt";
                btnFilterFailed.BackColor = ColorPalette.LightMode.Secondary;
                btnFilterFailed.ForeColor = ColorPalette.LightMode.TextPrimary;
            }

            ApplyFilters();
        }

        // ── Áp dụng tất cả bộ lọc hiện hành ──────────────────────────

        /// <summary>
        /// Kết hợp bộ lọc tìm kiếm (txtSearch) và lọc trượt (_isFilteringFailed)
        /// rồi cập nhật DataGridView.
        /// </summary>
        private void ApplyFilters()
        {
            IEnumerable<StudentScoreModel> result = _allScores;

            // 1) Lọc theo MSSV hoặc Họ Tên (không phân biệt hoa/thường)
            string keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                result = result.Where(s =>
                    s.Mssv.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    s.HoTen.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // 2) Lọc sinh viên "Không đạt"
            if (_isFilteringFailed)
            {
                result = result.Where(s => s.TongKet < 5.0);
            }

            RefreshGrid(result.ToList());
        }

        // ── Cập nhật DataGridView ────────────────────────────────────────

        /// <summary>
        /// Bind danh sách điểm đã lọc vào DataGridView.
        /// Dùng SuspendLayout / ResumeLayout để tránh nhấp nháy.
        /// </summary>
        private void RefreshGrid(List<StudentScoreModel> scores)
        {
            dgvScores.SuspendLayout();
            dgvScores.DataSource = null;

            if (scores.Count > 0)
            {
                var bindingSource = new BindingSource(scores, null);
                dgvScores.DataSource = bindingSource;
                ConfigureColumns();
            }

            dgvScores.ResumeLayout();
        }

        /// <summary>
        /// Cấu hình tiêu đề cột, chiều rộng, căn chỉnh cho DataGridView.
        /// Ưu tiên hiển thị tên và khóa học bằng cách để chúng chiếm không gian còn lại.
        /// Các cột số (điểm, trạng thái) shrink về kích thước tối thiểu.
        /// </summary>
        private void ConfigureColumns()
        {
            // Tắt AutoSizeColumnsMode để cấu hình thủ công từng cột
            dgvScores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            var headers = new Dictionary<string, string>
            {
                { "Mssv",       "MSSV"            },
                { "HoTen",      "Họ và Tên"       },
                { "Lop",        "Khóa Học"        },
                { "DiemGK",     "Điểm Giữa Kỳ"   },
                { "DiemCK",     "Điểm Cuối Kỳ"   },
                { "TongKet",    "Tổng Kết"        },
                { "TrangThai",  "Trạng Thái"      },
            };

            foreach (DataGridViewColumn col in dgvScores.Columns)
            {
                if (headers.ContainsKey(col.Name))
                    col.HeaderText = headers[col.Name];

                // Tắt sort để tránh highlight xanh trên cột đầu tiên
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

                // Căn trái cho các cột text, căn giữa cho các cột số
                if (col.Name == "Mssv" || col.Name == "HoTen" || col.Name == "Lop")
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                else
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // Cấu hình chiều rộng cột
                if (col.Name == "HoTen" || col.Name == "Lop")
                {
                    // Cột tên và khóa học: Fill (chiếm không gian còn lại)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    col.FillWeight = col.Name == "HoTen" ? 150 : 120; // HoTen chiếm 150/(150+120) ≈ 56%
                }
                else if (col.Name == "Mssv" || col.Name == "DiemGK" || col.Name == "DiemCK" || 
                         col.Name == "TongKet" || col.Name == "TrangThai")
                {
                    // Các cột khác: AllCells (shrink về kích thước tối thiểu)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }

            // Ẩn cột ID nếu có
            if (dgvScores.Columns.Contains("Id"))
                dgvScores.Columns["Id"].Visible = false;
        }

        // ── Tô màu dòng theo trạng thái ─────────────────────────────────

        /// <summary>
        /// Tô nền đỏ nhạt cho các dòng sinh viên "Không đạt".
        /// </summary>
        private void DgvScores_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvScores.Rows[e.RowIndex].DataBoundItem is StudentScoreModel score)
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

        // ── Tải dữ liệu từ Supabase ──────────────────────────────────────

        /// <summary>
        /// Tải toàn bộ dữ liệu điểm từ Supabase (view vw_student_scores).
        /// Chạy trên Thread Pool, cập nhật UI trên UI Thread.
        /// </summary>
        private async System.Threading.Tasks.Task LoadDataFromSupabaseAsync()
        {
            SetStatus("⏳  Đang kết nối Supabase và tải dữ liệu...", ColorPalette.LightMode.TextSecondary);
            SetControlsEnabled(false);

            try
            {
                // Chạy query trên background thread để không đóng băng UI
                _allScores = await System.Threading.Tasks.Task.Run(() => _repo.LoadAll());

                // Hiển thị danh sách lên lưới
                ApplyFilters();

                SetStatus($"✅  Đã tải {_allScores.Count} bản ghi.", ColorPalette.Status.SuccessLight);
            }
            catch (Exception ex)
            {
                SetStatus($"❌  Lỗi kết nối DB: {ex.Message}", ColorPalette.Status.ErrorLight);
                MessageBox.Show(
                    $"Không thể tải dữ liệu từ Supabase.\n\nChi tiết:\n{ex.Message}",
                    "Lỗi kết nối",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }
        // ── Import CSV ───────────────────────────────────────────────────

        /// <summary>
        /// Import danh sách điểm từ file CSV và Upsert lên Supabase.
        /// Nếu MSSV đã tồn tại → cập nhật. Nếu chưa → thêm mới.
        /// </summary>
        private void BtnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file CSV điểm số";
                ofd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

                if (ofd.ShowDialog() != DialogResult.OK) return;

                _ = ImportCsvAsync(ofd.FileName);
            }
        }

        private async System.Threading.Tasks.Task ImportCsvAsync(string filePath)
        {
            SetStatus("⏳  Đang đọc và xử lý file CSV...", ColorPalette.LightMode.TextSecondary);
            SetControlsEnabled(false);

            try
            {
                List<StudentScoreModel> importedScores;

                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    HeaderValidated = null,
                    MissingFieldFound = null,
                };

                using (var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                using (var csvReader = new CsvReader(reader, csvConfig))
                {
                    importedScores = csvReader.GetRecords<StudentScoreModel>().ToList();
                }

                if (importedScores.Count == 0)
                {
                    MessageBox.Show("File CSV không có dữ liệu hợp lệ.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SetStatus("⚠️  File CSV rỗng hoặc không hợp lệ.", ColorPalette.Status.WarningLight);
                    return;
                }

                // Xác nhận trước khi upsert
                var confirm = MessageBox.Show(
                    $"Tìm thấy {importedScores.Count} bản ghi trong file CSV.\n\n" +
                    $"Bạn có muốn đồng bộ lên Supabase không?\n" +
                    $"(Nếu MSSV đã tồn tại, điểm sẽ được CẬP NHẬT.)",
                    "Xác nhận Import",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                SetStatus("⏳  Đang đồng bộ lên Supabase...", ColorPalette.LightMode.TextSecondary);

                int affected = await System.Threading.Tasks.Task.Run(() => _repo.UpsertMany(importedScores));

                MessageBox.Show(
                    $"✅  Import thành công!\n\n" +
                    $"• Số bản ghi xử lý: {importedScores.Count}\n" +
                    $"• Số hàng ảnh hưởng: {affected}",
                    "Import CSV thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reload lại dữ liệu
                await LoadDataFromSupabaseAsync();
            }
            catch (CsvHelperException csvEx)
            {
                MessageBox.Show(
                    $"Lỗi đọc file CSV.\n\n" +
                    $"Hãy đảm bảo file có đúng header:\n" +
                    $"MSSV, Ho Ten, Lop, Diem GK, Diem CK\n\n" +
                    $"Chi tiết:\n{csvEx.Message}",
                    "Lỗi đọc CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("❌  Lỗi định dạng CSV.", ColorPalette.Status.ErrorLight);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định:\n{ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"❌  Lỗi: {ex.Message}", ColorPalette.Status.ErrorLight);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        // ── Export CSV ───────────────────────────────────────────────────

        /// <summary>
        /// Xuất dữ liệu đang hiển thị trên DataGridView (đã lọc) ra file CSV.
        /// </summary>
        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Lưu file CSV điểm số";
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = $"Diem_SinhVien_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                _ = ExportCsvAsync(sfd.FileName);
            }
        }

        private async System.Threading.Tasks.Task ExportCsvAsync(string filePath)
        {
            SetStatus("⏳  Đang xuất dữ liệu...", ColorPalette.LightMode.TextSecondary);
            SetControlsEnabled(false);

            try
            {
                var currentSource = dgvScores.DataSource as BindingSource;
                if (currentSource == null)
                    currentSource = new BindingSource(new List<StudentScoreModel>(), null);

                var data = currentSource.DataSource as List<StudentScoreModel>;
                if (data == null || data.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất.",
                        "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SetStatus("⚠️  Không có dữ liệu để xuất.", ColorPalette.Status.WarningLight);
                    return;
                }

                await System.Threading.Tasks.Task.Run(() =>
                {
                    using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true,
                        };

                        using (var csv = new CsvWriter(writer, config))
                        {
                            csv.WriteRecords(data);
                        }
                    }
                });

                MessageBox.Show(
                    $"Export thành công {data.Count} bản ghi.\nFile: {filePath}",
                    "Export CSV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                SetStatus($"✅  Xuất {data.Count} bản ghi ra: {Path.GetFileName(filePath)}",
                    ColorPalette.Status.SuccessLight);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi export:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"❌  Lỗi export: {ex.Message}", ColorPalette.Status.ErrorLight);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        // ── Helper Methods ────────────────────────────────────────────────

        /// <summary>Cập nhật label trạng thái ở thanh dưới (thread-safe).</summary>
        private void SetStatus(string message, Color color)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => SetStatus(message, color)));
                return;
            }
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        /// <summary>Bật/tắt các nút điều khiển (tránh thao tác kép khi đang tải).</summary>
        private void SetControlsEnabled(bool enabled)
        {
            if (btnImport.InvokeRequired)
            {
                btnImport.Invoke(new Action(() => SetControlsEnabled(enabled)));
                return;
            }
            btnImport.Enabled = enabled;
            btnExport.Enabled = enabled;
            btnFilterFailed.Enabled = enabled;
            txtSearch.Enabled = enabled;
        }
    }
}
