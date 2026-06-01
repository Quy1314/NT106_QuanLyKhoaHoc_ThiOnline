using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class TeacherAssignmentDialog : ThemedDialogBase
    {
        private readonly ComboBox _course = new();
        private readonly TextBox _title = new();
        private readonly TextBox _details = new();
        private readonly DateTimePicker _date = new();
        private readonly ComboBox _status = new();
        
        private readonly TextBox _file = new();
        private readonly Button _browse = TeacherTabChrome.SecondaryButton("Chọn file");
        private readonly Button _clearFile = TeacherTabChrome.SecondaryButton("Xóa file");

        private class StatusItem
        {
            public string Value { get; }
            public string Display { get; }
            public StatusItem(string value, string display) { Value = value; Display = display; }
            public override string ToString() => Display;
        }

        public int CourseId => _course.SelectedItem is TeacherCourseModel course ? course.Id : 0;
        public string ItemTitle => _title.Text.Trim();
        public string Details => _details.Text.Trim();
        public DateTime SelectedDate => _date.Value;
        public string Status => (_status.SelectedItem as StatusItem)?.Value ?? "OPEN";
        
        public string SelectedFilePath => _file.Text.Trim();

        public TeacherAssignmentDialog(IEnumerable<TeacherCourseModel> courses, TeacherAssignmentModel? existing = null)
        {
            Text = existing == null ? "Tạo Bài Tập" : "Sửa Bài Tập";
            Width = 800;
            Height = 600;

            _course.DropDownStyle = ComboBoxStyle.DropDownList;
            _course.DisplayMember = nameof(TeacherCourseModel.Name);
            foreach (var course in courses)
                _course.Items.Add(course);
            
            if (_course.Items.Count > 0)
                _course.SelectedIndex = 0;

            if (existing != null)
            {
                int idx = courses.ToList().FindIndex(c => c.Id == existing.CourseId);
                if (idx >= 0) _course.SelectedIndex = idx;
                _title.Text = existing.Title;
                _details.Text = existing.Description;
                if (existing.DueAt.HasValue) _date.Value = existing.DueAt.Value;
                _file.Text = existing.FileName;
            }

            _details.Multiline = true;
            _details.ScrollBars = ScrollBars.Vertical;
            _date.Format = DateTimePickerFormat.Custom;
            _date.CustomFormat = "dd/MM/yyyy HH:mm";
            
            _status.DropDownStyle = ComboBoxStyle.DropDownList;
            var statuses = new[]
            {
                new StatusItem("OPEN", "Đã mở"),
                new StatusItem("CLOSED", "Đã đóng")
            };
            _status.Items.AddRange(statuses);
            
            var existingStatus = existing?.Status ?? "OPEN";
            _status.SelectedItem = statuses.FirstOrDefault(s => s.Value == existingStatus) ?? statuses[0];

            _file.ReadOnly = true;
            _browse.Click += (_, _) => BrowseFile();
            _clearFile.Click += (_, _) => _file.Text = string.Empty;

            var save = new Button { Text = "Lưu", DialogResult = DialogResult.OK, Width = 90 };
            var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Width = 90 };
            TeacherTabChrome.StylePrimaryButton(save);
            TeacherTabChrome.StyleSecondaryButton(cancel);

            save.Click += (s, e) =>
            {
                if (CourseId <= 0 || string.IsNullOrWhiteSpace(ItemTitle))
                {
                    MetaTheme.ShowModernDialog("Vui lòng chọn khóa học và nhập tiêu đề.", "Thiếu thông tin");
                    DialogResult = DialogResult.None;
                }
                
                if (!string.IsNullOrEmpty(SelectedFilePath) && File.Exists(SelectedFilePath))
                {
                    var info = new FileInfo(SelectedFilePath);
                    if (info.Length > 10 * 1024 * 1024)
                    {
                        MetaTheme.ShowModernDialog("File đính kèm không được vượt quá 10MB.", "Lỗi dung lượng");
                        DialogResult = DialogResult.None;
                    }
                }
            };

            var grid = TeacherCourseDialog.CreateGrid();
            grid.RowCount = 6;
            grid.RowStyles.Clear();
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            grid.Controls.Add(new Label { Text = "Khóa học", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 0);
            _course.Dock = DockStyle.Fill;
            grid.Controls.Add(_course, 1, 0);
            grid.Controls.Add(new Label { Text = "Tiêu đề", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 1);
            _title.Dock = DockStyle.Fill;
            grid.Controls.Add(_title, 1, 1);
            grid.Controls.Add(new Label { Text = "Mô tả", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 2);
            _details.Dock = DockStyle.Fill;
            grid.Controls.Add(_details, 1, 2);
            
            var filePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Margin = new Padding(0) };
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _file.Dock = DockStyle.Fill;
            filePanel.Controls.Add(_file, 0, 0);
            filePanel.Controls.Add(_browse, 1, 0);
            filePanel.Controls.Add(_clearFile, 2, 0);
            
            grid.Controls.Add(new Label { Text = "File đính kèm", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 3);
            grid.Controls.Add(filePanel, 1, 3);
            
            grid.Controls.Add(new Label { Text = "Hạn nộp", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 4);
            _date.Dock = DockStyle.Fill;
            grid.Controls.Add(_date, 1, 4);
            grid.Controls.Add(new Label { Text = "Trạng thái", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 5);
            _status.Dock = DockStyle.Fill;
            grid.Controls.Add(_status, 1, 5);

            ContentPanel.Controls.Add(grid);
            AddFooterButtons(cancel, save);
            AcceptButton = save;
            CancelButton = cancel;
        }
        
        public void SetSelectedCourse(int courseId)
        {
            int idx = _course.Items.Cast<TeacherCourseModel>().ToList().FindIndex(c => c.Id == courseId);
            if (idx >= 0) _course.SelectedIndex = idx;
        }
        
        public void SetExistingFile(string fileName)
        {
            _file.Text = fileName; // Display existing filename
        }

        private void BrowseFile()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Files (*.pdf;*.doc;*.docx;*.txt)|*.pdf;*.doc;*.docx;*.txt|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                _file.Text = dialog.FileName;
        }
    }
}
