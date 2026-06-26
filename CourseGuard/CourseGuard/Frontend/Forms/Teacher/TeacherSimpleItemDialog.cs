using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public class TeacherSimpleItemDialog : ThemedDialogBase
    {
        protected readonly ComboBox CourseCombo = new();
        protected readonly TextBox TitleTextBox = new();
        protected readonly TextBox DetailsTextBox = new();
        protected readonly DateTimePicker DatePicker = new();
        protected readonly DateTimePicker StartTimePicker = new();
        protected readonly DateTimePicker EndTimePicker = new();
        protected readonly ComboBox StatusCombo = new();
        protected TableLayoutPanel? ContentGrid { get; private set; }

        private readonly bool _enableTimeRange;
        private readonly Label _validationSummary = new();

        public int CourseId => CourseCombo.SelectedItem is TeacherCourseModel course ? course.Id : 0;
        public string ItemTitle => TitleTextBox.Text.Trim();
        public string Details => DetailsTextBox.Text.Trim();
        public DateTime SelectedDate => _enableTimeRange ? SelectedStartTime : DatePicker.Value;
        public DateTime SelectedStartTime => DatePicker.Value.Date.Add(StartTimePicker.Value.TimeOfDay);
        public DateTime SelectedEndTime => DatePicker.Value.Date.Add(EndTimePicker.Value.TimeOfDay);
        public string Status => StatusCombo.SelectedItem?.ToString() ?? string.Empty;

        public TeacherSimpleItemDialog(
            string title,
            IEnumerable<TeacherCourseModel> courses,
            string? itemTitle = null,
            string? details = null,
            string status = "OPEN",
            int selectedCourseId = 0,
            bool enableTimeRange = false,
            DateTime? selectedStartTime = null,
            DateTime? selectedEndTime = null)
        {
            _enableTimeRange = enableTimeRange;
            Text = title;
            Width = enableTimeRange ? 700 : 560;
            Height = enableTimeRange ? 460 : 350;

            ConfigureInput(CourseCombo);
            CourseCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            CourseCombo.DisplayMember = nameof(TeacherCourseModel.Name);
            foreach (var course in courses)
                CourseCombo.Items.Add(course);
            SelectCourse(selectedCourseId);

            ConfigureInput(TitleTextBox);
            TitleTextBox.Text = itemTitle ?? string.Empty;

            ConfigureInput(DetailsTextBox);
            DetailsTextBox.Text = details ?? string.Empty;
            DetailsTextBox.Multiline = true;

            DateTime initialStart = selectedStartTime ?? DateTime.Now;
            DateTime initialEnd = selectedEndTime ?? initialStart.AddHours(2);
            ConfigureDatePicker(DatePicker, enableTimeRange ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm");
            DatePicker.Value = initialStart;

            ConfigureTimePicker(StartTimePicker, initialStart);
            ConfigureTimePicker(EndTimePicker, initialEnd);

            ConfigureInput(StatusCombo);
            StatusCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            StatusCombo.Items.AddRange(new object[] { "DRAFT", "OPEN", "ACTIVE", "PENDING", "CLOSED" });
            StatusCombo.SelectedItem = status;
            if (StatusCombo.SelectedIndex < 0)
                StatusCombo.SelectedIndex = 1;

            _validationSummary.Dock = DockStyle.Fill;
            _validationSummary.TextAlign = ContentAlignment.MiddleLeft;
            _validationSummary.Font = MetaTheme.Fonts.BodyMd();
            _validationSummary.ForeColor = AppColors.TextSecondary;
            _validationSummary.AutoEllipsis = true;
            _validationSummary.Text = string.Empty;
            _validationSummary.Visible = false;
            HookValidationInputs();

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
            ConfigureGrid(grid, enableTimeRange);

            grid.Controls.Add(CreateFieldLabel("Khóa học"), 0, 0);
            grid.Controls.Add(CourseCombo, 1, 0);
            grid.Controls.Add(CreateFieldLabel("Tiêu đề"), 0, 1);
            grid.Controls.Add(TitleTextBox, 1, 1);
            grid.Controls.Add(CreateFieldLabel(enableTimeRange ? "Link lớp học" : "Nội dung"), 0, 2);
            grid.Controls.Add(DetailsTextBox, 1, 2);

            if (enableTimeRange)
            {
                grid.Controls.Add(CreateFieldLabel("Ngày học"), 0, 3);
                grid.Controls.Add(DatePicker, 1, 3);
                grid.Controls.Add(CreateFieldLabel("Thời lượng"), 0, 4);
                grid.Controls.Add(CreateTimeRangePanel(), 1, 4);
                grid.Controls.Add(CreateFieldLabel("Trạng thái"), 0, 5);
                grid.Controls.Add(StatusCombo, 1, 5);
            }
            else
            {
                grid.Controls.Add(CreateFieldLabel("Thời gian"), 0, 3);
                grid.Controls.Add(DatePicker, 1, 3);
                grid.Controls.Add(CreateFieldLabel("Trạng thái"), 0, 4);
                grid.Controls.Add(StatusCombo, 1, 4);
            }

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty
            };
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.Controls.Add(_validationSummary, 0, 0);
            content.Controls.Add(grid, 0, 1);

            ContentPanel.Controls.Add(content);
            AddFooterButtons(cancel, save);
            AcceptButton = save;
            CancelButton = cancel;
        }

        protected virtual bool ValidateBeforeSave()
        {
            if (CourseId <= 0)
                return ShowValidationSummary("Vui lòng chọn khóa học.");

            if (string.IsNullOrWhiteSpace(ItemTitle))
                return ShowValidationSummary("Vui lòng nhập tiêu đề.");

            if (_enableTimeRange && SelectedEndTime <= SelectedStartTime)
                return ShowValidationSummary("Thời gian kết thúc phải sau thời gian bắt đầu.");

            ClearValidationSummary();
            return true;
        }

        private bool ShowValidationSummary(string message)
        {
            _validationSummary.Text = message;
            _validationSummary.ForeColor = AppColors.Danger;
            _validationSummary.Visible = true;
            return false;
        }

        private void HookValidationInputs()
        {
            CourseCombo.SelectedIndexChanged += (_, _) => RefreshValidationSummaryIfVisible();
            TitleTextBox.TextChanged += (_, _) => RefreshValidationSummaryIfVisible();
            DatePicker.ValueChanged += (_, _) => RefreshValidationSummaryIfVisible();
            StartTimePicker.ValueChanged += (_, _) => RefreshValidationSummaryIfVisible();
            EndTimePicker.ValueChanged += (_, _) => RefreshValidationSummaryIfVisible();
        }

        private void RefreshValidationSummaryIfVisible()
        {
            if (!_validationSummary.Visible)
                return;

            if (CourseId <= 0)
            {
                ShowValidationSummary("Vui lòng chọn khóa học.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ItemTitle))
            {
                ShowValidationSummary("Vui lòng nhập tiêu đề.");
                return;
            }

            if (_enableTimeRange && SelectedEndTime <= SelectedStartTime)
            {
                ShowValidationSummary("Thời gian kết thúc phải sau thời gian bắt đầu.");
                return;
            }

            ClearValidationSummary();
        }

        private void ClearValidationSummary()
        {
            _validationSummary.Text = string.Empty;
            _validationSummary.ForeColor = AppColors.TextSecondary;
            _validationSummary.Visible = false;
        }

        private static void ConfigureGrid(TableLayoutPanel grid, bool enableTimeRange)
        {
            grid.BackColor = AppColors.BgCard;
            grid.Padding = new Padding(28, 10, 28, 10);
            grid.ColumnStyles.Clear();
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.RowStyles.Clear();
            grid.RowCount = enableTimeRange ? 6 : 5;
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            if (enableTimeRange)
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        }

        private static void ConfigureInput(Control control)
        {
            control.Dock = DockStyle.Fill;
            control.Font = AppFonts.Body;
            control.ForeColor = AppColors.TextPrimary;
            control.BackColor = AppColors.BgInput;
            control.Margin = new Padding(0, 6, 0, 6);
            if (control is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;
            if (control is ComboBox cb) cb.FlatStyle = FlatStyle.Flat;
        }

        private static void ConfigureDatePicker(DateTimePicker picker, string customFormat)
        {
            ConfigureInput(picker);
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = customFormat;
            picker.CalendarForeColor = AppColors.TextPrimary;
            picker.CalendarMonthBackground = AppColors.BgCard;
            picker.CalendarTitleBackColor = AppColors.BgCard;
            picker.CalendarTitleForeColor = AppColors.TextPrimary;
        }

        private static void ConfigureTimePicker(DateTimePicker picker, DateTime value)
        {
            ConfigureDatePicker(picker, "HH:mm");
            picker.ShowUpDown = true;
            picker.Value = value;
        }

        private static Label CreateFieldLabel(string text) => new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = AppFonts.Semibold(9f),
            ForeColor = AppColors.TextPrimary,
            BackColor = AppColors.BgCard,
            Margin = new Padding(0, 4, 12, 4)
        };

        private Control CreateTimeRangePanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = AppColors.BgCard,
                Margin = Padding.Empty
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1));

            var arrow = new Label
            {
                Text = "đến",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = AppFonts.Caption,
                ForeColor = AppColors.TextSecondary,
                BackColor = AppColors.BgCard
            };

            panel.Controls.Add(StartTimePicker, 0, 0);
            panel.Controls.Add(arrow, 1, 0);
            panel.Controls.Add(EndTimePicker, 2, 0);
            StartTimePicker.Dock = DockStyle.Fill;
            EndTimePicker.Dock = DockStyle.Fill;
            return panel;
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

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            catch (Exception)
            {
                // Suppress exception during GC finalization of uninitialized objects in tests
            }
        }
    }
}
