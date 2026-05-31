using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;
using CourseGuard.Frontend.UserControls.Teacher;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public class QuestionBankDialog : Form
    {
        private readonly int _teacherId;
        private readonly int _examId;
        private readonly int _courseId;
        private readonly TeacherController _controller = new(new CourseGuardDbContext(""));
        private readonly DataGridView _grid = new();
        private readonly Button _add = TeacherTabChrome.PrimaryButton("Thêm đã chọn");
        private readonly Button _close = TeacherTabChrome.SecondaryButton("Đóng");

        public bool QuestionsAdded { get; private set; }

        public QuestionBankDialog(int teacherId, int examId, int courseId)
        {
            _teacherId = teacherId;
            _examId = examId;
            _courseId = courseId;
            Text = "Ngân hàng câu hỏi khóa học";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            WireEvents();
            LoadData();
            AppColors.ApplyTheme(this);
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16), BackColor = AppColors.BgBase };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            TeacherTabChrome.StyleGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.ReadOnly = false; // Allow checking boxes
            root.Controls.Add(_grid, 0, 0);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            buttons.Controls.Add(_close);
            buttons.Controls.Add(_add);
            root.Controls.Add(buttons, 0, 1);

            Controls.Add(root);
        }

        private void WireEvents()
        {
            _close.Click += (_, _) => Close();
            _add.Click += async (_, _) => await AddSelectedQuestionsAsync();
        }

        private void LoadData()
        {
            _grid.Columns.Clear();
            
            var checkCol = new DataGridViewCheckBoxColumn
            {
                Name = "Select",
                HeaderText = "Chọn",
                Width = 50,
                ReadOnly = false
            };
            _grid.Columns.Add(checkCol);

            var idCol = new DataGridViewTextBoxColumn { Name = "Id", Visible = false, ReadOnly = true };
            _grid.Columns.Add(idCol);

            var textCol = new DataGridViewTextBoxColumn { Name = "Câu hỏi", HeaderText = "Câu hỏi", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true };
            _grid.Columns.Add(textCol);

            var ansCol = new DataGridViewTextBoxColumn { Name = "Đáp án", HeaderText = "Đáp án", Width = 80, ReadOnly = true };
            _grid.Columns.Add(ansCol);

            var ptsCol = new DataGridViewTextBoxColumn { Name = "Điểm", HeaderText = "Điểm", Width = 80, ReadOnly = true };
            _grid.Columns.Add(ptsCol);

            var questions = _controller.GetCourseQuestionBank(_teacherId, _courseId);
            foreach (var q in questions)
            {
                _grid.Rows.Add(false, q.Id, q.QuestionText, q.CorrectOption, q.Points.ToString("0.##"));
            }

            _grid.ClearSelection();
        }

        private async System.Threading.Tasks.Task AddSelectedQuestionsAsync()
        {
            var selectedIds = new List<int>();
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Cells["Select"].Value is bool isSelected && isSelected)
                {
                    selectedIds.Add(Convert.ToInt32(row.Cells["Id"].Value));
                }
            }

            if (selectedIds.Count == 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn ít nhất một câu hỏi.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _add.Enabled = false;
                await _controller.AddQuestionsFromBankAsync(_teacherId, _examId, selectedIds);
                QuestionsAdded = true;
                MetaTheme.ShowModernDialog($"Đã thêm {selectedIds.Count} câu hỏi vào bài thi thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _add.Enabled = true;
            }
        }
    }
}
