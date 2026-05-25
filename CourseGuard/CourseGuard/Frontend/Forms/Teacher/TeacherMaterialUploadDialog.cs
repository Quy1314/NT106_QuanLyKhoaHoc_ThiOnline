using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public class TeacherMaterialUploadDialog : Form
    {
        private readonly ComboBox _course = new();
        private readonly TextBox _file = new();
        private readonly Button _browse = TeacherTabChrome.SecondaryButton("Chọn file");
        private readonly Button _save = TeacherTabChrome.PrimaryButton("Lưu");
        private readonly Button _cancel = TeacherTabChrome.SecondaryButton("Hủy");
        private readonly List<TeacherCourseModel> _courses;

        public TeacherMaterialUploadDialog(IEnumerable<TeacherCourseModel> courses, int selectedCourseId = 0)
        {
            _courses = courses.ToList();
            Text = "Tải tài liệu";
            Width = 640;
            Height = 230;
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout(selectedCourseId);
            AppColors.ApplyTheme(this);
        }

        public int CourseId => _course.SelectedItem is TeacherCourseModel course ? course.Id : 0;
        public string SelectedFilePath => _file.Text.Trim();

        private void BuildLayout(int selectedCourseId)
        {
            var root = TeacherCourseDialog.CreateGrid();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(18);
            root.RowCount = 3;
            root.RowStyles.Clear();
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            _course.DropDownStyle = ComboBoxStyle.DropDownList;
            _course.DisplayMember = nameof(TeacherCourseModel.Name);
            foreach (TeacherCourseModel course in _courses)
                _course.Items.Add(course);
            int selectedIndex = _courses.FindIndex(c => c.Id == selectedCourseId);
            _course.SelectedIndex = selectedIndex >= 0 ? selectedIndex : (_course.Items.Count > 0 ? 0 : -1);

            _file.ReadOnly = true;
            var filePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filePanel.Controls.Add(_file, 0, 0);
            filePanel.Controls.Add(_browse, 1, 0);

            _browse.Click += (_, _) => BrowseFile();
            _save.Click += (_, _) => Save();
            _cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

            AddRow(root, 0, "Khóa học", _course);
            AddRow(root, 1, "File", filePanel);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            buttons.Controls.Add(_cancel);
            buttons.Controls.Add(_save);
            root.Controls.Add(buttons, 0, 2);
            root.SetColumnSpan(buttons, 2);
            Controls.Add(root);
        }

        private static void AddRow(TableLayoutPanel grid, int row, string label, Control control)
        {
            grid.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            control.Dock = DockStyle.Fill;
            grid.Controls.Add(control, 1, row);
        }

        private void BrowseFile()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Tài liệu (*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.txt;*.zip;*.rar)|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.txt;*.zip;*.rar|Tất cả file (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                _file.Text = dialog.FileName;
        }

        private void Save()
        {
            if (CourseId <= 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn khóa học.", "Thiếu thông tin");
                return;
            }

            if (!File.Exists(SelectedFilePath))
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn file cần tải lên.", "Thiếu thông tin");
                return;
            }

            var info = new FileInfo(SelectedFilePath);
            MaterialFileValidation validation = MaterialFilePolicy.Validate(info.Name, info.Length);
            if (!validation.IsValid)
            {
                MetaTheme.ShowModernDialog(validation.ErrorMessage, "File không hợp lệ");
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
