using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public class TeacherSimpleItemDialog : Form
    {
        private readonly ComboBox _course = new();
        private readonly TextBox _title = new();
        private readonly TextBox _details = new();
        private readonly DateTimePicker _date = new();
        private readonly ComboBox _status = new();

        public int CourseId => _course.SelectedItem is TeacherCourseModel course ? course.Id : 0;
        public string ItemTitle => _title.Text.Trim();
        public string Details => _details.Text.Trim();
        public DateTime SelectedDate => _date.Value;
        public string Status => _status.SelectedItem?.ToString() ?? string.Empty;

        public TeacherSimpleItemDialog(string title, IEnumerable<TeacherCourseModel> courses, string? itemTitle = null, string? details = null, string status = "OPEN")
        {
            Text = title;
            Width = 520;
            Height = 330;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            _course.DropDownStyle = ComboBoxStyle.DropDownList;
            _course.DisplayMember = nameof(TeacherCourseModel.Name);
            foreach (var course in courses)
                _course.Items.Add(course);
            if (_course.Items.Count > 0)
                _course.SelectedIndex = 0;
            _title.Text = itemTitle ?? string.Empty;
            _details.Text = details ?? string.Empty;
            _details.Multiline = true;
            _date.Format = DateTimePickerFormat.Custom;
            _date.CustomFormat = "dd/MM/yyyy HH:mm";
            _status.DropDownStyle = ComboBoxStyle.DropDownList;
            _status.Items.AddRange(new object[] { "DRAFT", "OPEN", "ACTIVE", "PENDING", "CLOSED" });
            _status.SelectedItem = status;
            if (_status.SelectedIndex < 0)
                _status.SelectedIndex = 1;

            var save = new Button { Text = "Lưu", DialogResult = DialogResult.OK, Width = 90 };
            var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Width = 90 };
            TeacherTabChrome.StylePrimaryButton(save);
            TeacherTabChrome.StyleSecondaryButton(cancel);
            save.Click += (_, _) =>
            {
                if (CourseId <= 0 || string.IsNullOrWhiteSpace(ItemTitle))
                {
                    MessageBox.Show("Vui lòng chọn khóa học và nhập tiêu đề.");
                    DialogResult = DialogResult.None;
                }
            };

            var grid = TeacherCourseDialog.CreateGrid();
            grid.RowCount = 5;
            grid.RowStyles.Clear();
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.Controls.Add(new Label { Text = "Khóa học", Dock = DockStyle.Fill }, 0, 0);
            grid.Controls.Add(_course, 1, 0);
            grid.Controls.Add(new Label { Text = "Tiêu đề", Dock = DockStyle.Fill }, 0, 1);
            grid.Controls.Add(_title, 1, 1);
            grid.Controls.Add(new Label { Text = "Nội dung", Dock = DockStyle.Fill }, 0, 2);
            grid.Controls.Add(_details, 1, 2);
            grid.Controls.Add(new Label { Text = "Thời gian", Dock = DockStyle.Fill }, 0, 3);
            grid.Controls.Add(_date, 1, 3);
            grid.Controls.Add(new Label { Text = "Trạng thái", Dock = DockStyle.Fill }, 0, 4);
            grid.Controls.Add(_status, 1, 4);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(12), BackColor = AppColors.BgBase };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(save);
            Controls.Add(grid);
            Controls.Add(buttons);
            AcceptButton = save;
            CancelButton = cancel;
            AppColors.ApplyTheme(this);
        }
    }
}
