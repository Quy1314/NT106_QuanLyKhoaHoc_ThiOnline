using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public class TeacherExamQuestionsDialog : Form
    {
        private readonly int _teacherId;
        private readonly int _examId;
        private readonly TeacherController _controller = new(new CourseGuardDbContext(""));
        private readonly DataGridView _grid = new();
        private readonly TextBox _question = new();
        private readonly TextBox _a = new();
        private readonly TextBox _b = new();
        private readonly TextBox _c = new();
        private readonly TextBox _d = new();
        private readonly ComboBox _correct = new();
        private readonly Label _points = new();
        private readonly Button _importExcel = TeacherTabChrome.SecondaryButton("Nhập từ file (Excel)");
        private readonly Button _questionBank = TeacherTabChrome.SecondaryButton("Ngân hàng câu hỏi");
        private readonly Button _add = TeacherTabChrome.PrimaryButton("Thêm câu");
        private readonly Button _save = TeacherTabChrome.SecondaryButton("Lưu sửa");
        private readonly Button _delete = TeacherTabChrome.DangerButton("Xóa câu");
        private readonly Button _close = TeacherTabChrome.SecondaryButton("Đóng");
        private readonly bool _canEdit;
        private readonly int _courseId;

        public TeacherExamQuestionsDialog(int teacherId, int examId, string examTitle, int courseId)
        {
            _teacherId = teacherId;
            _examId = examId;
            _courseId = courseId;
            _canEdit = string.Equals(_controller.GetExamStatus(teacherId, examId), WorkflowConstants.ExamStatus.Draft, StringComparison.OrdinalIgnoreCase);
            Text = $"Soạn câu hỏi - {examTitle}";
            Width = 980;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            WireEvents();
            LoadQuestions();
            AppColors.ApplyTheme(this);
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(16), BackColor = AppColors.BgBase };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

            var leftPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            TeacherTabChrome.StyleGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            leftPanel.Controls.Add(_grid, 0, 0);

            var leftButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            leftButtons.Controls.Add(_importExcel);
            leftButtons.Controls.Add(_questionBank);
            leftPanel.Controls.Add(leftButtons, 0, 1);

            root.Controls.Add(leftPanel, 0, 0);

            var editor = TeacherCourseDialog.CreateGrid();
            editor.RowCount = 8;
            editor.RowStyles.Clear();
            editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++)
                editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            _question.Multiline = true;
            _correct.DropDownStyle = ComboBoxStyle.DropDownList;
            _correct.Items.AddRange(new object[] { "A", "B", "C", "D" });
            _correct.SelectedIndex = 0;
            _points.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            AddRow(editor, 0, "Câu hỏi", _question);
            AddRow(editor, 1, "A", _a);
            AddRow(editor, 2, "B", _b);
            AddRow(editor, 3, "C", _c);
            AddRow(editor, 4, "D", _d);
            AddRow(editor, 5, "Đáp án đúng", _correct);
            AddRow(editor, 6, "Điểm/câu", _points);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            buttons.Controls.Add(_close);
            buttons.Controls.Add(_delete);
            buttons.Controls.Add(_save);
            buttons.Controls.Add(_add);
            editor.Controls.Add(buttons, 0, 7);
            editor.SetColumnSpan(buttons, 2);
            root.Controls.Add(editor, 1, 0);
            Controls.Add(root);
        }

        private static void AddRow(TableLayoutPanel grid, int row, string label, Control control)
        {
            grid.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, row);
            control.Dock = DockStyle.Fill;
            grid.Controls.Add(control, 1, row);
        }

        private void WireEvents()
        {
            _grid.SelectionChanged += (_, _) => LoadSelectedQuestion();
            _add.Click += (_, _) => SaveNewQuestion();
            _save.Click += (_, _) => SaveExistingQuestion();
            _delete.Click += (_, _) => DeleteSelectedQuestion();
            _close.Click += (_, _) => Close();
            _importExcel.Click += async (_, _) => await ImportFromExcelAsync();
            _questionBank.Click += (_, _) => OpenQuestionBank();
            _add.Enabled = _save.Enabled = _delete.Enabled = _importExcel.Enabled = _questionBank.Enabled = _canEdit;
        }

        private async System.Threading.Tasks.Task ImportFromExcelAsync()
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Chọn file Excel mẫu",
                Multiselect = false
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                _importExcel.Enabled = false;
                _importExcel.Text = "Đang nhập...";

                var questions = await _controller.ParseAndValidateExcelAsync(ofd.FileName);
                if (questions == null || questions.Count == 0)
                {
                    MetaTheme.ShowModernDialog("File không có dữ liệu hoặc không đúng định dạng.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // In-memory data validation
                var errors = new System.Collections.Generic.List<string>();
                for (int i = 0; i < questions.Count; i++)
                {
                    var q = questions[i];
                    if (string.IsNullOrWhiteSpace(q.QuestionText) || 
                        string.IsNullOrWhiteSpace(q.OptionA) || string.IsNullOrWhiteSpace(q.OptionB) || 
                        string.IsNullOrWhiteSpace(q.OptionC) || string.IsNullOrWhiteSpace(q.OptionD))
                    {
                        errors.Add($"Dòng {i + 2}: Thiếu câu hỏi hoặc đáp án.");
                    }
                    if (string.IsNullOrWhiteSpace(q.CorrectOption) || !new[] { "A", "B", "C", "D" }.Contains(q.CorrectOption.Trim().ToUpperInvariant()))
                    {
                        errors.Add($"Dòng {i + 2}: Đáp án đúng phải là A, B, C hoặc D.");
                    }
                    if (q.Points <= 0)
                    {
                        errors.Add($"Dòng {i + 2}: Điểm số phải lớn hơn 0.");
                    }
                }

                if (errors.Count > 0)
                {
                    MetaTheme.ShowModernDialog("Dữ liệu không hợp lệ:\n" + string.Join("\n", errors.Take(5)) + (errors.Count > 5 ? "\n..." : ""), "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                await _controller.ImportQuestionsToExamAsync(_teacherId, _examId, _courseId, questions);
                MetaTheme.ShowModernDialog($"Nhập thành công {questions.Count} câu hỏi!", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadQuestions();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể đọc file. Vui lòng đóng file nếu đang mở.\nLỗi: " + ex.Message, "Lỗi đọc file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _importExcel.Enabled = _canEdit;
                _importExcel.Text = "Nhập từ file (Excel)";
            }
        }

        private void OpenQuestionBank()
        {
            using var qbd = new QuestionBankDialog(_teacherId, _examId, _courseId);
            qbd.ShowDialog(this);
            if (qbd.QuestionsAdded)
            {
                LoadQuestions();
            }
        }

        private void LoadQuestions()
        {
            DataTable table = new();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("STT", typeof(int));
            table.Columns.Add("Câu hỏi", typeof(string));
            table.Columns.Add("Đáp án", typeof(string));
            table.Columns.Add("Điểm", typeof(string));

            var questions = _controller.GetExamQuestions(_teacherId, _examId);
            foreach (var q in questions)
                table.Rows.Add(q.Id, q.DisplayOrder, q.QuestionText, q.CorrectOption, q.Points.ToString("0.##", CultureInfo.InvariantCulture));

            _grid.DataSource = table;
            if (_grid.Columns["Id"] != null)
                _grid.Columns["Id"]!.Visible = false;
            _points.Text = questions.Count == 0 ? "0" : (10m / questions.Count).ToString("0.##", CultureInfo.InvariantCulture);
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            ClearEditor();
        }

        private void LoadSelectedQuestion()
        {
            int id = CurrentQuestionId();
            if (id <= 0)
                return;
            var item = _controller.GetExamQuestions(_teacherId, _examId).FirstOrDefault(q => q.Id == id);
            if (item == null)
                return;
            _question.Text = item.QuestionText;
            _a.Text = item.OptionA;
            _b.Text = item.OptionB;
            _c.Text = item.OptionC;
            _d.Text = item.OptionD;
            _correct.SelectedItem = item.CorrectOption;
        }

        private void SaveNewQuestion()
        {
            if (!TryBuildQuestion(out var model))
                return;
            _controller.CreateExamQuestion(_teacherId, model);
            LoadQuestions();
        }

        private void SaveExistingQuestion()
        {
            int id = CurrentQuestionId();
            if (id <= 0 || !TryBuildQuestion(out var model))
                return;
            model.Id = id;
            _controller.UpdateExamQuestion(_teacherId, model);
            LoadQuestions();
        }

        private void DeleteSelectedQuestion()
        {
            int id = CurrentQuestionId();
            if (id <= 0)
                return;
            if (MetaTheme.ShowModernDialog("Xóa câu hỏi này khỏi bài kiểm tra?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _controller.DeleteExamQuestion(_teacherId, _examId, id);
            LoadQuestions();
        }

        private bool TryBuildQuestion(out TeacherExamQuestionModel model)
        {
            model = new TeacherExamQuestionModel();
            if (string.IsNullOrWhiteSpace(_question.Text) || string.IsNullOrWhiteSpace(_a.Text) || string.IsNullOrWhiteSpace(_b.Text) || string.IsNullOrWhiteSpace(_c.Text) || string.IsNullOrWhiteSpace(_d.Text))
            {
                MetaTheme.ShowModernDialog("Vui lòng nhập câu hỏi và đủ 4 lựa chọn.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            model.ExamId = _examId;
            model.QuestionText = _question.Text;
            model.OptionA = _a.Text;
            model.OptionB = _b.Text;
            model.OptionC = _c.Text;
            model.OptionD = _d.Text;
            model.CorrectOption = _correct.SelectedItem?.ToString() ?? "A";
            return true;
        }

        private int CurrentQuestionId()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow)
                return 0;
            return Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        }

        private void ClearEditor()
        {
            _question.Clear();
            _a.Clear();
            _b.Clear();
            _c.Clear();
            _d.Clear();
            _correct.SelectedIndex = 0;
        }
    }
}
