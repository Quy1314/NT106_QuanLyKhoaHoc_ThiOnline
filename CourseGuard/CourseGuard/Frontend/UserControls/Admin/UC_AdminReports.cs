/*
 * UC_AdminReports.cs
 * 
 * Layer: Presentation (UserControls)
 * Vai trò: Màn hình báo cáo thống kê. Hiển thị các biểu đồ hoặc số liệu tổng hợp.
 * Phụ thuộc: (Chưa implement logic sâu).
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using System.IO;
using System.Drawing.Printing;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public partial class UC_AdminReports : UserControl
    {
        private bool _hasLoaded;

        public UC_AdminReports()
        {
            InitializeComponent();

            // Bo góc + cursor tay cho tất cả buttons
            CourseGuard.Frontend.Theme.RoundedButtonHelper.Apply(10,
                btnFilter, btnExportCSV, btnExportExcel, btnExportPDF);
            
            // Default Date Range: Last 30 days
            dtpStartDate.Value = DateTime.Now.AddDays(-30);
            dtpEndDate.Value = DateTime.Now;

            // Wire up events
            btnFilter.Click += BtnFilter_Click;
            btnExportCSV.Click += BtnExportCSV_Click;
            btnExportExcel.Click += BtnExportExcel_Click;
            btnExportPDF.Click += BtnExportPDF_Click;
            this.VisibleChanged += UC_AdminReports_VisibleChanged;

            // Initial Load
            cboReportType.Items.Clear();
            cboReportType.Items.Add("Danh sách học viên");
            cboReportType.Items.Add("Danh sách giảng viên");
            // Placeholder for future:
            // cboReportType.Items.Add("Danh sách vi phạm");
            // cboReportType.Items.Add("Danh sách đăng nhập");

            cboReportType.SelectedIndex = 0; 
        }

        private void UC_AdminReports_VisibleChanged(object? sender, EventArgs e)
        {
            if (!Visible || _hasLoaded) return;

            try
            {
                LoadData();
                _hasLoaded = true;
            }
            catch (ObjectDisposedException)
            {
                // Ignore when control is being disposed during view switching.
            }
        }

        private void BtnFilter_Click(object? sender, EventArgs e)
        {
            LoadData();
        }

        private async void LoadData()
        {
            string? reportType = cboReportType.SelectedItem?.ToString();
            DateTime start = dtpStartDate.Value.Date;
            DateTime end = dtpEndDate.Value.Date.AddDays(1).AddTicks(-1); // End of day

            string query = "";
            
            if (reportType == "Danh sách học viên")
            {
                query = @"
                    SELECT u.ID, u.USERNAME, u.FULL_NAME, u.EMAIL, u.CREATED_AT, u.STATUS
                    FROM USERS u
                    JOIN ROLES r ON u.ROLE_ID = r.ID
                    WHERE r.NAME = 'STUDENT'
                    AND u.CREATED_AT BETWEEN @start AND @end
                    ORDER BY u.CREATED_AT DESC";
            }
            else if (reportType == "Danh sách giảng viên")
            {
                query = @"
                    SELECT u.ID, u.USERNAME, u.FULL_NAME, u.EMAIL, u.CREATED_AT, u.STATUS
                    FROM USERS u
                    JOIN ROLES r ON u.ROLE_ID = r.ID
                    WHERE r.NAME = 'TEACHER'
                    AND u.CREATED_AT BETWEEN @start AND @end
                    ORDER BY u.CREATED_AT DESC";
            }
            // Future implementation for "Danh sách vi phạm" and "Danh sách đăng nhập"
             /*
            else if (reportType == "Danh sách vi phạm") { ... }
            else if (reportType == "Danh sách đăng nhập") { ... }
             */
            else
            {
                // Fallback or empty
                dataGridView1.DataSource = null;
                return;
            }

            var parameters = new Dictionary<string, (SqlDbType, object)>
            {
                { "@start", (SqlDbType.DateTime, start) },
                { "@end", (SqlDbType.DateTime, end) }
            };

            this.ShowSkeleton(SkeletonType.TableWithToolbar);
            try
            {
                DataTable dt = await Task.Run(() => DatabaseAction.ExecuteQuery(query, parameters));
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi tải dữ liệu báo cáo: " + ex.Message);
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        // Export to CSV
        private void BtnExportCSV_Click(object? sender, EventArgs e)
        {
            ExportToTextFile(",", "CSV File (*.csv)|*.csv");
        }

        // Export to Excel (Using CSV format but opening as Excel)
        private void BtnExportExcel_Click(object? sender, EventArgs e)
        {
            // Providing Tab-delimited or generic CSV usually works for Excel
            ExportToTextFile("\t", "Excel File (*.xls)|*.xls"); 
            // Note: Saving as .xls with tab delimiter handles basic data well. Use .csv for strict CSV.
        }

        private async void ExportToTextFile(string separator, string filter)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Không có dữ liệu để xuất.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = filter;
            sfd.FileName = "Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                this.ShowSkeleton(SkeletonType.FormWithTable);
                try
                {
                    // 1. Extract data from UI on UI thread (safe and fast)
                    var headers = new List<string>();
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        headers.Add(dataGridView1.Columns[i].HeaderText);
                    }

                    var rowData = new List<string[]>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            var cells = new string[dataGridView1.Columns.Count];
                            for (int i = 0; i < dataGridView1.Columns.Count; i++)
                            {
                                cells[i] = row.Cells[i].Value?.ToString() ?? "";
                            }
                            rowData.Add(cells);
                        }
                    }

                    string filePath = sfd.FileName;

                    // 2. Perform string building and file writing on background thread
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        StringBuilder sb = new StringBuilder();

                        // Headers
                        for (int i = 0; i < headers.Count; i++)
                        {
                            sb.Append(headers[i]);
                            if (i < headers.Count - 1) sb.Append(separator);
                        }
                        sb.AppendLine();

                        // Rows
                        foreach (var cells in rowData)
                        {
                            for (int i = 0; i < cells.Length; i++)
                            {
                                string value = cells[i].Replace("\n", " ").Replace("\r", " ");
                                if (value.Contains(separator))
                                {
                                    value = "\"" + value + "\"";
                                }
                                sb.Append(value);
                                if (i < cells.Length - 1) sb.Append(separator);
                            }
                            sb.AppendLine();
                        }

                        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                    });

                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Xuất file thành công!");
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    catch { /* Ignore if no default app */ }
                }
                catch (Exception ex)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi xuất file: " + ex.Message);
                }
                finally
                {
                    this.HideSkeleton();
                }
            }
        }

        // Export to PDF (Printing)
        private void BtnExportPDF_Click(object? sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Không có dữ liệu để in.");
                return;
            }

            PrintDocument printDoc = new PrintDocument();
            printDoc.DocumentName = "Report Output";
            printDoc.PrintPage += PrintDoc_PrintPage;

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDoc;
            
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                try 
                {
                    printDoc.Print();
                }
                catch(Exception ex)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi in: " + ex.Message);
                }
            }
        }

        // Simple Grid Printing Logic
        private int _printRowIndex = 0;
        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            int startX = e.MarginBounds.Left;
            int startY = e.MarginBounds.Top;
            int tableWidth = e.MarginBounds.Width;
            int rowHeight = dataGridView1.RowTemplate.Height;
            
            int cellWidth = tableWidth / Math.Max(1, dataGridView1.Columns.Count);
            
            Font font = new Font("Segoe UI", 9);
            Font headerFont = new Font("Segoe UI", 10, FontStyle.Bold);
            Brush brush = Brushes.Black;

            // Draw Headers (Only on first page or repeated?) Let's repeat.
            int currentY = startY;
            
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                e.Graphics?.DrawRectangle(Pens.Black, startX + (i * cellWidth), currentY, cellWidth, rowHeight);
                e.Graphics?.DrawString(dataGridView1.Columns[i].HeaderText, headerFont, brush, 
                    new RectangleF(startX + (i * cellWidth) + 2, currentY + 5, cellWidth - 4, rowHeight - 4));
            }
            currentY += rowHeight;

            while (_printRowIndex < dataGridView1.Rows.Count)
            {
                DataGridViewRow row = dataGridView1.Rows[_printRowIndex];
                
                // Check if we reached bottom of page
                if (currentY + rowHeight > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return; // Continue on next page
                }

                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    string val = row.Cells[i].Value?.ToString() ?? "";
                    e.Graphics?.DrawRectangle(Pens.Black, startX + (i * cellWidth), currentY, cellWidth, rowHeight);
                    e.Graphics?.DrawString(val, font, brush, 
                        new RectangleF(startX + (i * cellWidth) + 2, currentY + 5, cellWidth - 4, rowHeight - 4));
                }
                
                currentY += rowHeight;
                _printRowIndex++;
            }

            e.HasMorePages = false;
            _printRowIndex = 0; // Reset for next print
        }
    }
}
