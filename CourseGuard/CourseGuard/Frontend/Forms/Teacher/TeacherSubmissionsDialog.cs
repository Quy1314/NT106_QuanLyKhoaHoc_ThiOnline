using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public class TeacherSubmissionsDialog : ThemedDialogBase
    {
        private readonly int _teacherId;
        private readonly TeacherController _controller;
        private readonly List<TeacherCourseModel> _courses;
        private List<StudentSubmissionModel> _allSubmissions = new();

        private ComboBox _cboCourse = null!;
        private DataGridView _grid = null!;
        private BindingSource _bindingSource = new();

        // Grading panel
        private Panel _gradingPanel = null!;
        private Label _lblStudentName = null!;
        private Button _btnDownload = null!;
        private NumericUpDown _numScore = null!;
        private TextBox _txtFeedback = null!;
        private Button _btnSaveGrade = null!;
        private StudentSubmissionModel? _selectedSubmission;

        public TeacherSubmissionsDialog(int teacherId, TeacherController controller, List<TeacherCourseModel> courses)
        {
            _teacherId = teacherId;
            _controller = controller;
            _courses = courses;
            
            this.Text = "Danh sách sinh viên nộp bài";
            this.Width = 1000;
            this.Height = 650;

            InitializeComponent();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(0)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // Grading panel
            ContentPanel.Controls.Add(mainLayout);

            // Filter
            var filterPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            filterPanel.Controls.Add(new Label { Text = "Lọc theo khóa học:", AutoSize = true, Font = MetaTheme.Fonts.BodyMdBold(), Margin = new Padding(0, 5, 10, 0) });
            _cboCourse = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Font = MetaTheme.Fonts.BodyMd() };
            _cboCourse.Items.Add(new { Id = 0, Name = "Tất cả khóa học" });
            foreach (var c in _courses) _cboCourse.Items.Add(new { Id = c.Id, Name = c.Name });
            _cboCourse.DisplayMember = "Name";
            _cboCourse.ValueMember = "Id";
            _cboCourse.SelectedIndex = 0;
            _cboCourse.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            filterPanel.Controls.Add(_cboCourse);
            mainLayout.Controls.Add(filterPanel, 0, 0);

            // Grid
            _grid = new DataGridView {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = AppColors.BgBase
            };
            TeacherTabChrome.StyleGrid(_grid);
            _grid.SelectionChanged += Grid_SelectionChanged;
            mainLayout.Controls.Add(_grid, 0, 1);

            // Grading Panel
            _gradingPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.BgCard, Padding = new Padding(10) };
            
            _lblStudentName = new Label { Location = new Point(10, 10), AutoSize = true, Font = MetaTheme.Fonts.SubtitleLg() };
            _gradingPanel.Controls.Add(_lblStudentName);

            _btnDownload = TeacherTabChrome.SecondaryButton("Tải bài làm xuống");
            _btnDownload.Location = new Point(10, 45);
            _btnDownload.Click += BtnDownload_Click;
            _gradingPanel.Controls.Add(_btnDownload);

            _gradingPanel.Controls.Add(new Label { Text = "Điểm số (0-10):", Location = new Point(200, 45), AutoSize = true, Font = MetaTheme.Fonts.BodyMd() });
            _numScore = new NumericUpDown { Location = new Point(200, 70), Width = 100, Font = MetaTheme.Fonts.BodyMd(), Minimum = 0, Maximum = 10, DecimalPlaces = 1, Increment = 0.5m };
            _gradingPanel.Controls.Add(_numScore);

            _gradingPanel.Controls.Add(new Label { Text = "Nhận xét:", Location = new Point(340, 45), AutoSize = true, Font = MetaTheme.Fonts.BodyMd() });
            _txtFeedback = new TextBox { Location = new Point(340, 70), Width = 500, Height = 60, Multiline = true, Font = MetaTheme.Fonts.BodyMd() };
            _gradingPanel.Controls.Add(_txtFeedback);

            _btnSaveGrade = TeacherTabChrome.PrimaryButton("Cập nhật điểm");
            _btnSaveGrade.Location = new Point(340, 140);
            _btnSaveGrade.Width = 150;
            _btnSaveGrade.Click += BtnSaveGrade_Click;
            _gradingPanel.Controls.Add(_btnSaveGrade);

            _gradingPanel.Enabled = false; // Disable until a row is selected
            mainLayout.Controls.Add(_gradingPanel, 0, 2);
            
            var closeBtn = TeacherTabChrome.SecondaryButton("Đóng");
            closeBtn.Click += (s, e) => Close();
            AddFooterButtons(closeBtn);
            CancelButton = closeBtn;
        }

        private void ApplyTheme()
        {
            _lblStudentName.ForeColor = AppColors.TextPrimary;
            _gradingPanel.BackColor = AppColors.BgCard;
            foreach (Control c in _gradingPanel.Controls)
            {
                if (c is Label l) l.ForeColor = AppColors.TextPrimary;
            }
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            int? courseId = null;
            if (_cboCourse.SelectedItem != null)
            {
                dynamic selected = _cboCourse.SelectedItem;
                if (selected.Id > 0) courseId = selected.Id;
            }

            try
            {
                _allSubmissions = await _controller.GetStudentSubmissionsAsync(_teacherId, courseId);
                
                var table = new DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Khóa học", typeof(string));
                table.Columns.Add("Bài tập", typeof(string));
                table.Columns.Add("Sinh viên", typeof(string));
                table.Columns.Add("Thời gian nộp", typeof(string));
                table.Columns.Add("File", typeof(string));
                table.Columns.Add("Trạng thái", typeof(string));
                table.Columns.Add("Điểm", typeof(string));

                foreach (var s in _allSubmissions)
                {
                    table.Rows.Add(s.SubmissionId, s.CourseName, s.AssignmentTitle, s.StudentName, 
                        s.SubmittedAt.ToString("dd/MM/yyyy HH:mm"), s.FileName, 
                        s.Status == "GRADED" ? "Đã chấm" : "Chưa chấm", 
                        s.Score?.ToString("0.0") ?? "");
                }

                _bindingSource.DataSource = table;
                _grid.DataSource = _bindingSource;
                if (_grid.Columns.Contains("Id") && _grid.Columns["Id"] != null) 
                    _grid.Columns["Id"]!.Visible = false;
                
                // Clear selection
                _grid.ClearSelection();
                _gradingPanel.Enabled = false;
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow || _grid.CurrentRow.Cells["Id"].Value == null)
            {
                _gradingPanel.Enabled = false;
                _selectedSubmission = null;
                return;
            }

            int id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
            _selectedSubmission = _allSubmissions.FirstOrDefault(s => s.SubmissionId == id);

            if (_selectedSubmission != null)
            {
                _gradingPanel.Enabled = true;
                _lblStudentName.Text = $"Đang chọn: {_selectedSubmission.StudentName} - {_selectedSubmission.AssignmentTitle}";
                _numScore.Value = _selectedSubmission.Score ?? 0;
                _txtFeedback.Text = _selectedSubmission.Feedback ?? "";
            }
        }

        private async void BtnDownload_Click(object? sender, EventArgs e)
        {
            if (_selectedSubmission == null) return;

            using var saveDialog = new SaveFileDialog
            {
                FileName = _selectedSubmission.FileName,
                Filter = "All files (*.*)|*.*",
                Title = "Lưu bài làm của sinh viên"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _btnDownload.Enabled = false;
                    _btnDownload.Text = "Đang tải...";
                    
                    byte[]? content = await _controller.GetSubmissionContentAsync(_teacherId, _selectedSubmission.SubmissionId);
                    if (content == null || content.Length == 0)
                    {
                        MetaTheme.ShowModernDialog("Không tìm thấy file trên máy chủ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    await File.WriteAllBytesAsync(saveDialog.FileName, content);
                    MetaTheme.ShowModernDialog("Tải file thành công!", "Thành công", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                    catch { /* Ignore if no default app */ }
                }
                catch (Exception ex)
                {
                    MetaTheme.ShowModernDialog("Lỗi khi tải file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _btnDownload.Enabled = true;
                    _btnDownload.Text = "Tải bài làm xuống";
                }
            }
        }

        private async void BtnSaveGrade_Click(object? sender, EventArgs e)
        {
            if (_selectedSubmission == null) return;

            try
            {
                _btnSaveGrade.Enabled = false;
                _btnSaveGrade.Text = "Đang lưu...";

                bool success = await _controller.UpdateGradeAsync(_teacherId, _selectedSubmission.SubmissionId, _numScore.Value, _txtFeedback.Text);
                
                if (success)
                {
                    MetaTheme.ShowModernDialog("Cập nhật điểm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MetaTheme.ShowModernDialog("Cập nhật thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Lỗi khi cập nhật điểm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnSaveGrade.Enabled = true;
                _btnSaveGrade.Text = "Cập nhật điểm";
            }
        }
    }
}
