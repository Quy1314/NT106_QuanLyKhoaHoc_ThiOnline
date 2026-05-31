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
        protected readonly ComboBox CourseCombo = new();
        protected readonly TextBox TitleTextBox = new();
        protected readonly TextBox DetailsTextBox = new();
        protected readonly DateTimePicker DatePicker = new();
        protected readonly ComboBox StatusCombo = new();
        protected TableLayoutPanel? ContentGrid { get; private set; }

        public int CourseId => CourseCombo.SelectedItem is TeacherCourseModel course ? course.Id : 0;
        public string ItemTitle => TitleTextBox.Text.Trim();
        public string Details => DetailsTextBox.Text.Trim();
        public DateTime SelectedDate => DatePicker.Value;
        public string Status => StatusCombo.SelectedItem?.ToString() ?? string.Empty;

        public TeacherSimpleItemDialog(string title, IEnumerable<TeacherCourseModel> courses, string? itemTitle = null, string? details = null, string status = "OPEN", int selectedCourseId = 0)
        {
            Text = title;
            Width = 520;
            Height = 330;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            CourseCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            CourseCombo.DisplayMember = nameof(TeacherCourseModel.Name);
            foreach (var course in courses)
                CourseCombo.Items.Add(course);
            SelectCourse(selectedCourseId);

            TitleTextBox.Text = itemTitle ?? string.Empty;
            DetailsTextBox.Text = details ?? string.Empty;
            DetailsTextBox.Multiline = true;

            DatePicker.Format = DateTimePickerFormat.Custom;
            DatePicker.CustomFormat = "dd/MM/yyyy HH:mm";

            StatusCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            StatusCombo.Items.AddRange(new object[] { "DRAFT", "OPEN", "ACTIVE", "PENDING", "CLOSED" });
            StatusCombo.SelectedItem = status;
            if (StatusCombo.SelectedIndex < 0)
                StatusCombo.SelectedIndex = 1;

            var save = new Button { Text = "Lưu", DialogResult = DialogResult.OK, Width = 90 };
            var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Width = 90 };
            TeacherTabChrome.StylePrimaryButton(save);
            TeacherTabChrome.StyleSecondaryButton(cancel);
            save.Click += (_, _) =>
            {
                if (!ValidateBeforeSave())
                    DialogResult = DialogResult.None;
            };

            var grid = TeacherCourseDialog.CreateGrid();
            ContentGrid = grid;
            grid.RowCount = 5;
            grid.RowStyles.Clear();
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.Controls.Add(new Label { Text = "Khóa học", Dock = DockStyle.Fill }, 0, 0);
            grid.Controls.Add(CourseCombo, 1, 0);
            grid.Controls.Add(new Label { Text = "Tiêu đề", Dock = DockStyle.Fill }, 0, 1);
            grid.Controls.Add(TitleTextBox, 1, 1);
            grid.Controls.Add(new Label { Text = "Nội dung", Dock = DockStyle.Fill }, 0, 2);
            grid.Controls.Add(DetailsTextBox, 1, 2);
            grid.Controls.Add(new Label { Text = "Thời gian", Dock = DockStyle.Fill }, 0, 3);
            grid.Controls.Add(DatePicker, 1, 3);
            grid.Controls.Add(new Label { Text = "Trạng thái", Dock = DockStyle.Fill }, 0, 4);
            grid.Controls.Add(StatusCombo, 1, 4);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(12), BackColor = AppColors.BgBase };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(save);
            Controls.Add(grid);
            Controls.Add(buttons);
            AcceptButton = save;
            CancelButton = cancel;
            AppColors.ApplyTheme(this);
        }

        protected virtual bool ValidateBeforeSave()
        {
            if (CourseId <= 0 || string.IsNullOrWhiteSpace(ItemTitle))
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn khóa học và nhập tiêu đề.");
                return false;
            }

            return true;
        }

        private void SelectCourse(int selectedCourseId)
        {
            if (CourseCombo.Items.Count == 0)
                return;

            if (selectedCourseId > 0)
            {
                for (int i = 0; i < CourseCombo.Items.Count; i++)
                {
                    if (CourseCombo.Items[i] is TeacherCourseModel course && course.Id == selectedCourseId)
                    {
                        CourseCombo.SelectedIndex = i;
                        return;
                    }
                }
            }

            CourseCombo.SelectedIndex = 0;
        }
    }
}
