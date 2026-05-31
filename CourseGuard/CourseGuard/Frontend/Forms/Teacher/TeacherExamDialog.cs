using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CourseGuard.Backend.Models;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class TeacherExamDialog : TeacherSimpleItemDialog
    {
        private readonly DateTimePicker _closeTime = new();
        private readonly NumericUpDown _durationMinutes = new();
        private readonly NumericUpDown _maxAttempts = new();

        public DateTime OpenTime => SelectedDate;
        public DateTime CloseTime => _closeTime.Value;
        public int DurationMinutes => (int)_durationMinutes.Value;
        public int MaxAttempts => (int)_maxAttempts.Value;

        public TeacherExamDialog(IEnumerable<TeacherCourseModel> courses)
            : this(courses, new TeacherExamModel())
        {
        }

        public TeacherExamDialog(IEnumerable<TeacherCourseModel> courses, TeacherExamModel exam)
            : base("Bài kiểm tra", courses, exam.Title, string.Empty, exam.Status, exam.CourseId)
        {
            Width = 560;
            Height = 400;

            DatePicker.Value = exam.OpenTime ?? DateTime.Now;
            _closeTime.Format = DateTimePickerFormat.Custom;
            _closeTime.CustomFormat = "dd/MM/yyyy HH:mm";
            _closeTime.Value = exam.CloseTime ?? DatePicker.Value.AddHours(1);

            _durationMinutes.Minimum = 1;
            _durationMinutes.Maximum = 1440;
            _durationMinutes.Value = Clamp(exam.DurationMinutes <= 0 ? 60 : exam.DurationMinutes, _durationMinutes.Minimum, _durationMinutes.Maximum);

            _maxAttempts.Minimum = 1;
            _maxAttempts.Maximum = 99;
            _maxAttempts.Value = Clamp(exam.MaxAttempts <= 0 ? 1 : exam.MaxAttempts, _maxAttempts.Minimum, _maxAttempts.Maximum);

            StatusCombo.Items.Clear();
            StatusCombo.Items.AddRange(new object[] { WorkflowConstants.ExamStatus.Draft, WorkflowConstants.ExamStatus.Active, WorkflowConstants.ExamStatus.Closed });
            StatusCombo.SelectedItem = string.IsNullOrWhiteSpace(exam.Status) ? WorkflowConstants.ExamStatus.Draft : exam.Status;
            if (StatusCombo.SelectedIndex < 0)
                StatusCombo.SelectedItem = WorkflowConstants.ExamStatus.Draft;

            RebuildExamGrid();
        }

        protected override bool ValidateBeforeSave()
        {
            if (!base.ValidateBeforeSave())
                return false;

            if (CloseTime <= OpenTime)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Thời gian đóng phải sau thời gian mở.");
                return false;
            }

            return true;
        }

        private void RebuildExamGrid()
        {
            if (ContentGrid == null)
                return;

            ContentGrid.Controls.Clear();
            ContentGrid.RowStyles.Clear();
            ContentGrid.RowCount = 6;
            for (int i = 0; i < ContentGrid.RowCount; i++)
                ContentGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            ContentGrid.Controls.Add(new Label { Text = "Khóa học", Dock = DockStyle.Fill }, 0, 0);
            ContentGrid.Controls.Add(CourseCombo, 1, 0);
            ContentGrid.Controls.Add(new Label { Text = "Tiêu đề", Dock = DockStyle.Fill }, 0, 1);
            ContentGrid.Controls.Add(TitleTextBox, 1, 1);
            ContentGrid.Controls.Add(new Label { Text = "Mở lúc", Dock = DockStyle.Fill }, 0, 2);
            ContentGrid.Controls.Add(DatePicker, 1, 2);
            ContentGrid.Controls.Add(new Label { Text = "Đóng lúc", Dock = DockStyle.Fill }, 0, 3);
            ContentGrid.Controls.Add(_closeTime, 1, 3);
            ContentGrid.Controls.Add(new Label { Text = "Thời lượng (phút)", Dock = DockStyle.Fill }, 0, 4);

            var numbers = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = Padding.Empty
            };
            numbers.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            numbers.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            numbers.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            numbers.Controls.Add(_durationMinutes, 0, 0);
            numbers.Controls.Add(new Label { Text = "Lượt tối đa", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter }, 1, 0);
            numbers.Controls.Add(_maxAttempts, 2, 0);
            ContentGrid.Controls.Add(numbers, 1, 4);

            ContentGrid.Controls.Add(new Label { Text = "Trạng thái", Dock = DockStyle.Fill }, 0, 5);
            ContentGrid.Controls.Add(StatusCombo, 1, 5);
        }

        private static decimal Clamp(int value, decimal min, decimal max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }
}
