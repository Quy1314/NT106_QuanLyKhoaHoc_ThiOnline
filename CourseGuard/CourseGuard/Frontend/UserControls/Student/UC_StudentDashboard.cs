using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Student
{
    public partial class UC_StudentDashboard : UserControl
    {
        private TableLayoutPanel _rootGrid = null!;
        private TableLayoutPanel _statsGrid = null!;
        private TableLayoutPanel _contentGrid = null!;
        private RoundedPanel _noticePanel = null!;
        private RoundedPanel _activityPanel = null!;
        private FlowLayoutPanel _activityList = null!;

        public UC_StudentDashboard()
        {
            InitializeComponent();
            BuildOverviewLayout();
            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.StudentOverviewDashboard);
            try
            {
                await System.Threading.Tasks.Task.Delay(600);
                LoadDummyData();
                ApplyAcademicStyle();
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void BuildOverviewLayout()
        {
            SuspendLayout();

            Controls.Clear();
            BackColor = AppColors.BgBase;
            Padding = Padding.Empty;

            _rootGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgBase,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(24)
            };
            _rootGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _rootGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));
            _rootGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 168f));
            _rootGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblTitle.AutoSize = false;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Margin = Padding.Empty;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            lblTitle.ForeColor = AppColors.TextPrimary;

            _statsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = AppColors.BgBase,
                Margin = new Padding(0, 8, 0, 16)
            };
            for (int i = 0; i < 4; i++)
                _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            _statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            AddStatCard(0, "Khóa học", "4", "course", "Đang tham gia", true);
            AddStatCard(1, "Bài thi", "2", "exam", "Sắp tới", true);
            AddStatCard(2, "Thông báo", "3", "notice", "Mới gần đây", true);
            AddStatCard(3, "Điểm TB", "8.4", "score", "Học kỳ này", true);

            _contentGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppColors.BgBase,
                Margin = Padding.Empty
            };
            _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            _contentGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _noticePanel = CreatePanelCard();
            _noticePanel.Margin = new Padding(0, 0, 12, 0);
            BuildNoticePanel();

            _activityPanel = CreatePanelCard();
            _activityPanel.Margin = new Padding(12, 0, 0, 0);
            BuildActivityPanel();

            _contentGrid.Controls.Add(_noticePanel, 0, 0);
            _contentGrid.Controls.Add(_activityPanel, 1, 0);

            _rootGrid.Controls.Add(lblTitle, 0, 0);
            _rootGrid.Controls.Add(_statsGrid, 0, 1);
            _rootGrid.Controls.Add(_contentGrid, 0, 2);
            Controls.Add(_rootGrid);

            Resize -= UC_StudentDashboard_Resize;
            Resize += UC_StudentDashboard_Resize;
            ResumeLayout(true);
        }

        private void AddStatCard(int column, string title, string value, string icon, string caption, bool trendUp)
        {
            var card = new StatCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 8, 0),
                Title = title,
                Value = value,
                IconChar = icon,
                TrendPercent = trendUp ? "Ổn định" : "Cần chú ý",
                TrendUp = trendUp,
                Caption = caption
            };

            _statsGrid.Controls.Add(card, column, 0);
        }

        private void BuildNoticePanel()
        {
            _noticePanel.Padding = new Padding(18);

            var panelGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            panelGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblRecentNotices.AutoSize = false;
            lblRecentNotices.Dock = DockStyle.Fill;
            lblRecentNotices.Margin = Padding.Empty;
            lblRecentNotices.TextAlign = ContentAlignment.MiddleLeft;
            lblRecentNotices.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblRecentNotices.ForeColor = AppColors.TextPrimary;

            dgvRecentNotices.Dock = DockStyle.Fill;
            dgvRecentNotices.Margin = Padding.Empty;
            dgvRecentNotices.ReadOnly = true;
            dgvRecentNotices.AllowUserToAddRows = false;
            dgvRecentNotices.AllowUserToDeleteRows = false;
            dgvRecentNotices.AllowUserToResizeRows = false;
            dgvRecentNotices.RowHeadersVisible = false;
            dgvRecentNotices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecentNotices.MultiSelect = false;

            panelGrid.Controls.Add(lblRecentNotices, 0, 0);
            panelGrid.Controls.Add(dgvRecentNotices, 0, 1);
            _noticePanel.Controls.Add(panelGrid);
        }

        private void BuildActivityPanel()
        {
            _activityPanel.Padding = new Padding(18);

            var panelGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            panelGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            panelGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var title = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Hoạt động gần đây",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            _activityList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };

            AddActivityItem("Nộp bài OOP", "Hôm nay", AppColors.Success);
            AddActivityItem("Lịch thi Mạng máy tính", "01/04/2026", AppColors.Warning);
            AddActivityItem("Tài liệu C# mới", "28/03/2026", AppColors.AccentBlue);

            panelGrid.Controls.Add(title, 0, 0);
            panelGrid.Controls.Add(_activityList, 0, 1);
            _activityPanel.Controls.Add(panelGrid);
        }

        private void AddActivityItem(string title, string time, Color accent)
        {
            var item = new Panel
            {
                Width = 280,
                Height = 58,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.Transparent
            };
            item.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using SolidBrush dot = new SolidBrush(accent);
                e.Graphics.FillEllipse(dot, 2, 11, 10, 10);
            };

            var lbl = new Label
            {
                Text = title,
                Location = new Point(22, 4),
                Size = new Size(240, 24),
                Font = AppFonts.Button,
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };
            var meta = new Label
            {
                Text = time,
                Location = new Point(22, 30),
                Size = new Size(240, 20),
                Font = AppFonts.Caption,
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            item.Controls.Add(lbl);
            item.Controls.Add(meta);
            _activityList.Controls.Add(item);
        }

        private static RoundedPanel CreatePanelCard()
        {
            return new RoundedPanel
            {
                Dock = DockStyle.Fill,
                FillColor = AppColors.BgCard,
                BorderColor = AppColors.Border,
                CornerRadius = 16
            };
        }

        private void ApplyAcademicStyle()
        {
            BackColor = AppColors.BgBase;
            if (_rootGrid != null)
                _rootGrid.BackColor = AppColors.BgBase;
            if (_statsGrid != null)
                _statsGrid.BackColor = AppColors.BgBase;
            if (_contentGrid != null)
                _contentGrid.BackColor = AppColors.BgBase;

            foreach (RoundedPanel panel in new[] { _noticePanel, _activityPanel })
            {
                panel.FillColor = AppColors.BgCard;
                panel.BorderColor = AppColors.Border;
                panel.Invalidate();
            }

            lblTitle.ForeColor = AppColors.TextPrimary;
            lblRecentNotices.ForeColor = AppColors.TextPrimary;
            AcademicTheme.StyleGrid(dgvRecentNotices);
            AppColors.ApplyTheme(this);
            dgvRecentNotices.ClearSelection();
        }

        private void UC_StudentDashboard_Resize(object? sender, EventArgs e)
        {
            bool compact = Width < 980;
            _statsGrid.ColumnCount = compact ? 2 : 4;
            _contentGrid.ColumnCount = compact ? 1 : 2;
            _rootGrid.RowStyles[1].Height = compact ? 336f : 168f;

            RebuildColumnStyles(_statsGrid, compact ? 2 : 4);
            RebuildContentGrid(compact);
        }

        private void RebuildContentGrid(bool compact)
        {
            _contentGrid.SuspendLayout();
            _contentGrid.Controls.Clear();
            _contentGrid.ColumnStyles.Clear();
            _contentGrid.RowStyles.Clear();

            if (compact)
            {
                _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                _contentGrid.RowCount = 2;
                _contentGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 58f));
                _contentGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));
                _noticePanel.Margin = new Padding(0, 0, 0, 12);
                _activityPanel.Margin = new Padding(0, 12, 0, 0);
                _contentGrid.Controls.Add(_noticePanel, 0, 0);
                _contentGrid.Controls.Add(_activityPanel, 0, 1);
            }
            else
            {
                _contentGrid.RowCount = 1;
                _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
                _contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
                _contentGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                _noticePanel.Margin = new Padding(0, 0, 12, 0);
                _activityPanel.Margin = new Padding(12, 0, 0, 0);
                _contentGrid.Controls.Add(_noticePanel, 0, 0);
                _contentGrid.Controls.Add(_activityPanel, 1, 0);
            }

            _contentGrid.ResumeLayout(true);
        }

        private static void RebuildColumnStyles(TableLayoutPanel grid, int columns)
        {
            grid.SuspendLayout();
            grid.ColumnStyles.Clear();
            for (int i = 0; i < columns; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));

            for (int i = 0; i < grid.Controls.Count; i++)
            {
                Control control = grid.Controls[i];
                int column = i % columns;
                int row = i / columns;
                grid.SetColumn(control, column);
                grid.SetRow(control, row);
            }

            grid.RowCount = columns == 2 ? 2 : 1;
            grid.RowStyles.Clear();
            for (int i = 0; i < grid.RowCount; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / grid.RowCount));
            grid.ResumeLayout(true);
        }

        private void LoadDummyData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Tiêu đề", typeof(string));
            dt.Columns.Add("Khóa học", typeof(string));

            dt.Rows.Add("02/04/2026", "Bài tập mới: OOP Cơ bản", "Lập trình C#");
            dt.Rows.Add("01/04/2026", "Nhắc nhở: Lịch thi giữa kỳ", "Mạng máy tính");
            dt.Rows.Add("28/03/2026", "Cập nhật tài liệu mới", "Lập trình C#");

            dgvRecentNotices.DataSource = dt;
            dgvRecentNotices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRecentNotices.ClearSelection();
        }
    }
}
