using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class TeacherExamDialog : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private readonly ComboBox _course = new();
        private readonly TextBox _title = new();
        private readonly ComboBox _status = new();
        private readonly DarkDateTimePicker _openTime = new();
        private readonly DarkDateTimePicker _closeTime = new();
        private readonly DarkNumericInput _duration = new();
        private readonly DarkNumericInput _maxAttempts = new();

        public int CourseId => _course.SelectedItem is TeacherCourseModel course ? course.Id : 0;
        public string ItemTitle => _title.Text.Trim();
        public string Status => _status.SelectedItem?.ToString() ?? string.Empty;
        public DateTime? OpenTime => _openTime.Checked ? _openTime.Value : null;
        public DateTime? CloseTime => _closeTime.Checked ? _closeTime.Value : null;
        public int DurationMinutes => _duration.Value;
        public int MaxAttempts => _maxAttempts.Value;

        public TeacherExamDialog(IEnumerable<TeacherCourseModel> courses, TeacherExamModel? existing = null)
        {
            Text = existing == null ? "Thêm bài kiểm tra" : "Sửa bài kiểm tra";
            Width = 700;
            Height = 350;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = AppColors.BgBase;
            ForeColor = AppColors.TextPrimary;

            _course.DropDownStyle = ComboBoxStyle.DropDownList;
            _course.DisplayMember = nameof(TeacherCourseModel.Name);
            foreach (var course in courses)
                _course.Items.Add(course);

            _status.DropDownStyle = ComboBoxStyle.DropDownList;
            _status.Items.AddRange(new object[] { "DRAFT", "OPEN", "ACTIVE", "PENDING", "CLOSED" });
            
            if (existing != null)
            {
                foreach (TeacherCourseModel item in _course.Items)
                {
                    if (item.Id == existing.CourseId)
                    {
                        _course.SelectedItem = item;
                        break;
                    }
                }
                _title.Text = existing.Title;
                if (existing.OpenTime.HasValue) _openTime.Value = existing.OpenTime.Value;
                else _openTime.Clear();
                
                if (existing.CloseTime.HasValue) _closeTime.Value = existing.CloseTime.Value;
                else _closeTime.Clear();

                _duration.Value = existing.DurationMinutes > 0 ? existing.DurationMinutes : 60;
                _maxAttempts.Value = existing.MaxAttempts > 0 ? existing.MaxAttempts : 1;
                _status.SelectedItem = existing.Status;
            }
            else
            {
                if (_course.Items.Count > 0) _course.SelectedIndex = 0;
                _closeTime.Value = DateTime.Now.AddHours(1);
                _openTime.Value = DateTime.Now;
                _duration.Value = 60;
                _maxAttempts.Value = 1;
                _status.SelectedIndex = 0;
            }

            var save = new Button { Text = "Lưu", DialogResult = DialogResult.OK, Width = 100, Height = 36, Cursor = Cursors.Hand };
            var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Width = 100, Height = 36, Cursor = Cursors.Hand };
            
            save.Tag = "primary";
            cancel.Tag = "secondary";
            
            save.Click += (s, e) =>
            {
                if (CourseId <= 0 || string.IsNullOrWhiteSpace(ItemTitle))
                {
                    MessageBox.Show("Vui lòng chọn khóa học và nhập tiêu đề.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
                if (_openTime.Checked && _closeTime.Checked && _closeTime.Value <= _openTime.Value)
                {
                    MessageBox.Show("Thời gian đóng phải sau thời gian mở.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
            };

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, Padding = new Padding(20, 20, 20, 10) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            for (int i = 0; i < 4; i++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

            grid.Controls.Add(CreateLabel("Khóa học"), 0, 0);
            grid.Controls.Add(StyleInput(_course), 1, 0);
            grid.Controls.Add(CreateLabel("Thời gian mở"), 2, 0);
            grid.Controls.Add(_openTime, 3, 0);

            grid.Controls.Add(CreateLabel("Tiêu đề"), 0, 1);
            grid.Controls.Add(StyleInput(_title), 1, 1);
            grid.Controls.Add(CreateLabel("Thời gian đóng"), 2, 1);
            grid.Controls.Add(_closeTime, 3, 1);

            grid.Controls.Add(CreateLabel("Trạng thái"), 0, 2);
            grid.Controls.Add(StyleInput(_status), 1, 2);
            grid.Controls.Add(CreateLabel("Thời lượng (phút)"), 2, 2);
            grid.Controls.Add(_duration, 3, 2);

            grid.Controls.Add(CreateLabel("Số lần làm bài"), 2, 3);
            grid.Controls.Add(_maxAttempts, 3, 3);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(20, 10, 20, 10) };
            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);
            
            Controls.Add(grid);
            Controls.Add(buttons);
            AcceptButton = save;
            CancelButton = cancel;

            AppColors.ApplyTheme(this);
            RoundedButtonHelper.Apply(8, save, cancel);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (Environment.OSVersion.Version.Major >= 10 && AppColors.IsDarkMode)
            {
                int useImmersiveDarkMode = 1;
                DwmSetWindowAttribute(Handle, 20, ref useImmersiveDarkMode, sizeof(int));
            }
        }

        private Label CreateLabel(string text)
        {
            return new Label 
            { 
                Text = text, 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppColors.TextSecondary
            };
        }

        private Control StyleInput(Control c)
        {
            c.Dock = DockStyle.Fill;
            c.BackColor = AppColors.BgInput;
            c.ForeColor = AppColors.TextPrimary;
            c.Font = new Font("Segoe UI", 10F);
            if (c is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;
            if (c is ComboBox cb) cb.FlatStyle = FlatStyle.Flat;
            return c;
        }
    }

    public class DarkNumericInput : TextBox
    {
        public DarkNumericInput()
        {
            BackColor = AppColors.BgInput;
            ForeColor = AppColors.TextPrimary;
            BorderStyle = BorderStyle.FixedSingle;
            Font = new Font("Segoe UI", 10F);
            Dock = DockStyle.Fill;
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int Value
        {
            get => int.TryParse(Text, out int v) ? v : 0;
            set => Text = value.ToString();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
            base.OnKeyPress(e);
        }
    }

    public class DarkDateTimePicker : Panel
    {
        private TextBox _textBox = new TextBox();
        private Button _btn = new Button();
        private DateTime? _value;

        public bool Checked => _value.HasValue;
        
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public DateTime Value 
        { 
            get => _value ?? DateTime.Now; 
            set { _value = value; UpdateText(); }
        }

        public DarkDateTimePicker()
        {
            Height = 28;
            Dock = DockStyle.Fill;
            Padding = new Padding(1);
            BackColor = AppColors.Border; // Border
            
            var innerPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.BgInput };
            
            _textBox.BorderStyle = BorderStyle.None;
            _textBox.BackColor = AppColors.BgInput;
            _textBox.ForeColor = AppColors.TextPrimary;
            _textBox.Font = new Font("Segoe UI", 10F);
            _textBox.Location = new Point(5, 4);
            _textBox.Width = 130;
            _textBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            _textBox.TextChanged += (s, e) => {
                if (DateTime.TryParseExact(_textBox.Text, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime dt))
                    _value = dt;
            };

            _btn.Text = "▼";
            _btn.Dock = DockStyle.Right;
            _btn.Width = 30;
            _btn.FlatStyle = FlatStyle.Flat;
            _btn.FlatAppearance.BorderSize = 0;
            _btn.BackColor = AppColors.BgInput;
            _btn.ForeColor = AppColors.TextSecondary;
            _btn.Cursor = Cursors.Hand;
            _btn.Click += Btn_Click;

            innerPanel.Controls.Add(_textBox);
            innerPanel.Controls.Add(_btn);
            Controls.Add(innerPanel);
        }

        public void Clear()
        {
            _value = null;
            _textBox.Text = "";
        }

        private void UpdateText()
        {
            _textBox.Text = _value.HasValue ? _value.Value.ToString("dd/MM/yyyy HH:mm") : "";
        }

        private void Btn_Click(object? sender, EventArgs e)
        {
            using var popup = new Form { 
                StartPosition = FormStartPosition.Manual, 
                FormBorderStyle = FormBorderStyle.FixedToolWindow, 
                Width = 250, Height = 250,
                Text = "Chọn ngày giờ"
            };
            popup.Location = PointToScreen(new Point(0, Height));
            var dtp = new DateTimePicker { 
                Format = DateTimePickerFormat.Custom, 
                CustomFormat = "dd/MM/yyyy HH:mm", 
                Dock = DockStyle.Top 
            };
            var mc = new MonthCalendar { Dock = DockStyle.Fill, MaxSelectionCount = 1 };
            
            if (_value.HasValue) {
                dtp.Value = _value.Value;
                mc.SetDate(_value.Value);
            }
            
            mc.DateChanged += (s, ev) => {
                dtp.Value = new DateTime(ev.Start.Year, ev.Start.Month, ev.Start.Day, dtp.Value.Hour, dtp.Value.Minute, 0);
            };
            
            var btnOk = new Button { Text = "Xác nhận", Dock = DockStyle.Bottom, DialogResult = DialogResult.OK, Height = 35 };
            btnOk.Tag = "primary";
            RoundedButtonHelper.Apply(8, btnOk);
            AppColors.ApplyTheme(popup);
            
            popup.Controls.Add(mc);
            popup.Controls.Add(dtp);
            popup.Controls.Add(btnOk);
            popup.AcceptButton = btnOk;
            
            if (popup.ShowDialog() == DialogResult.OK)
                Value = dtp.Value;
        }
    }
}
