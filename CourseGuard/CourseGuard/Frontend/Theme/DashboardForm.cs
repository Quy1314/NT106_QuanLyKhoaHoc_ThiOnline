/*
 * DashboardForm.cs
 *
 * Layer: Presentation (Theme)
 * Final assembly of the dark modern dashboard.
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ScottPlot.WinForms;

namespace CourseGuard.Frontend.Theme
{
    public class DashboardForm : Form
    {
        // P/Invoke for form dragging
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private SidebarPanel _sidebar;
        private TopbarPanel _topbar;
        private Panel _pnlMain;
        private Panel _pnlContent;

        public DashboardForm(CourseGuard.Backend.Models.UserModel user = null)
        {
            SearchFocusManager.Install(this);
            InitializeLayout();
            if (user != null)
            {
                _topbar.UserName = string.IsNullOrWhiteSpace(user.FullName) 
                    ? user.Username 
                    : user.FullName;
            }
            WireEvents();
        }

        private void InitializeLayout()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(1440, 900);
            this.MinimumSize = new Size(1200, 700);
            this.DoubleBuffered = true;
            this.BackColor = AppColors.BgBase;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "CourseGuard Dashboard";

            // 1. Sidebar
            _sidebar = new SidebarPanel { Dock = DockStyle.Left };
            
            // 2. Main Area (contains Topbar and Content)
            _pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.BgBase };
            
            // 3. Topbar
            _topbar = new TopbarPanel { Dock = DockStyle.Top };
            
            // 4. Content Area
            _pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppColors.BgBase,
                Padding = new Padding(24)
            };

            // Setup Layouts inside Content Area
            SetupContentRows();

            // Assemble
            _pnlMain.Controls.Add(_pnlContent);
            _pnlMain.Controls.Add(_topbar); // Topbar added second so it docks to top above content
            
            this.Controls.Add(_pnlMain);
            this.Controls.Add(_sidebar); // Sidebar docks left of everything
        }

        private void SetupContentRows()
        {
            // We use a vertical TableLayoutPanel to stack the rows
            TableLayoutPanel mainGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0)
            };
            mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 160f)); // Row 1 + margin
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 340f)); // Row 2 + margin
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 280f)); // Row 3 + margin

            // --- Row 1 (Height 140) ---
            TableLayoutPanel row1 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 20)
            };
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            var stat1 = new StatCard 
            { 
                Title = "Total Revenue", Value = "$48,295", TrendPercent = "47%", TrendUp = true, IconChar = "$", 
                Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0)
            };
            var stat2 = new StatCard 
            { 
                Title = "Total Orders", Value = "1,284", TrendPercent = "47%", TrendUp = true, IconChar = "📦", 
                Dock = DockStyle.Fill, Margin = new Padding(10, 0, 10, 0)
            };
            var gauge = new SemiCircleGauge 
            { 
                Percent = 0.65f, TargetLabel = "Target: $250,000", AchievedLabel = "Achieved: $155,000", 
                Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0)
            };
            
            row1.Controls.Add(stat1, 0, 0);
            row1.Controls.Add(stat2, 1, 0);
            row1.Controls.Add(gauge, 2, 0);

            // --- Row 2 (Height 320) ---
            TableLayoutPanel row2 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 20)
            };
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));

            // Col 1: Analytics
            RoundedPanel pnlAnalytics = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0), FillColor = AppColors.BgCard };
            Label lblAna = new Label { Text = "Sales Analytics", Font = AppFonts.CardTitle, ForeColor = AppColors.TextSecondary, Location = new Point(20, 20), AutoSize = true };
            
            // Real ScottPlot Line Chart
            FormsPlot chartPlot = new FormsPlot 
            { 
                Location = new Point(20, 60), 
                Size = new Size(pnlAnalytics.Width - 40, 240), 
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Configure style
            var spBgCard = new ScottPlot.Color(AppColors.BgCard.R, AppColors.BgCard.G, AppColors.BgCard.B);
            var spTextMuted = new ScottPlot.Color(AppColors.TextMuted.R, AppColors.TextMuted.G, AppColors.TextMuted.B);
            var spGridLine = new ScottPlot.Color(255, 255, 255, 15); // rgba(255,255,255,0.06) is ~15/255

            chartPlot.Plot.FigureBackground.Color = spBgCard;
            chartPlot.Plot.DataBackground.Color = spBgCard;
            chartPlot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = spTextMuted;
            chartPlot.Plot.Axes.Left.TickLabelStyle.ForeColor = spTextMuted;
            chartPlot.Plot.Axes.Bottom.FrameLineStyle.Color = spGridLine;
            chartPlot.Plot.Axes.Left.FrameLineStyle.Color = spGridLine;
            
            chartPlot.Plot.Grid.MajorLineColor = spGridLine;

            // Sample data
            double[] dataX = { 0, 1, 2, 3, 4, 5, 6 };
            double[] dataY = { 2500, 3200, 3800, 3500, 2800, 3100, 3600 };
            string[] labels = { "10 Apr", "11 Apr", "12 Apr", "13 Apr", "14 Apr", "15 Apr", "16 Apr" };

            // Line
            var scatter = chartPlot.Plot.Add.Scatter(dataX, dataY);
            scatter.Color = new ScottPlot.Color(59, 130, 246);
            scatter.LineWidth = 2;

            // Fill area (16% opacity of #3B82F6 -> alpha ~41)
            double[] dataBottom = new double[dataX.Length];
            var fill = chartPlot.Plot.Add.FillY(dataX, dataY, dataBottom);
            fill.FillColor = new ScottPlot.Color(59, 130, 246, 41);

            // X Axis Labels
            ScottPlot.TickGenerators.NumericManual tickGen = new();
            for (int i = 0; i < dataX.Length; i++)
            {
                tickGen.AddMajor(dataX[i], labels[i]);
            }
            chartPlot.Plot.Axes.Bottom.TickGenerator = tickGen;

            // Layout padding
            chartPlot.Plot.Layout.Frameless();

            pnlAnalytics.Controls.Add(lblAna);
            pnlAnalytics.Controls.Add(chartPlot);

            // Col 2: Heatmap
            HeatmapGrid heatmap = new HeatmapGrid { Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0) };

            row2.Controls.Add(pnlAnalytics, 0, 0);
            row2.Controls.Add(heatmap, 1, 0);

            // --- Row 3 (Height 260) ---
            TableLayoutPanel row3 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 20)
            };
            row3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            row3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            row3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            // Col 1: Budget
            RoundedPanel pnlBudget = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0), FillColor = AppColors.BgCard };
            Label lblBdg = new Label { Text = "Budget Usage", Font = AppFonts.CardTitle, ForeColor = AppColors.TextSecondary, Location = new Point(20, 20), AutoSize = true };
            
            Label lblMeta = new Label { Text = "Meta Ads", Font = AppFonts.Caption, ForeColor = AppColors.TextPrimary, Location = new Point(20, 60), AutoSize = true };
            RoundedProgressBar pbMeta = new RoundedProgressBar { Location = new Point(20, 85), Width = 380, Maximum = 100, Value = 65, DisplayText = "65%", BarColor = AppColors.AccentBlue, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            
            Label lblTok = new Label { Text = "TikTok Ads", Font = AppFonts.Caption, ForeColor = AppColors.TextPrimary, Location = new Point(20, 130), AutoSize = true };
            RoundedProgressBar pbTok = new RoundedProgressBar { Location = new Point(20, 155), Width = 380, Maximum = 100, Value = 72, DisplayText = "72%", BarColor = Color.FromArgb(236, 72, 153), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            
            pnlBudget.Controls.AddRange(new Control[] { lblBdg, lblMeta, pbMeta, lblTok, pbTok });


            // Col 2: Reviews
            RoundedPanel pnlRev = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(10, 0, 10, 0), FillColor = AppColors.BgCard };
            Label lblR = new Label { Text = "Customer Review", Font = AppFonts.CardTitle, ForeColor = AppColors.TextSecondary, Location = new Point(20, 20), AutoSize = true };
            
            Label lblRev1Name = new Label { Text = "Sarah Connor", Font = AppFonts.Button, ForeColor = AppColors.TextPrimary, Location = new Point(20, 60), AutoSize = true };
            Label lblRev1Stars = new Label { Text = "★★★★★", Font = AppFonts.Caption, ForeColor = AppColors.Warning, Location = new Point(130, 62), AutoSize = true };
            Label lblRev1Desc = new Label { Text = "Great quality, highly recommended!", Font = AppFonts.Body, ForeColor = AppColors.TextSecondary, Location = new Point(20, 85), AutoSize = true };
            
            Label lblRev2Name = new Label { Text = "John Wick", Font = AppFonts.Button, ForeColor = AppColors.TextPrimary, Location = new Point(20, 130), AutoSize = true };
            Label lblRev2Stars = new Label { Text = "★★★★☆", Font = AppFonts.Caption, ForeColor = AppColors.Warning, Location = new Point(130, 132), AutoSize = true };
            Label lblRev2Desc = new Label { Text = "Good, but shipping took a while.", Font = AppFonts.Body, ForeColor = AppColors.TextSecondary, Location = new Point(20, 155), AutoSize = true };
            
            pnlRev.Controls.AddRange(new Control[] { lblR, lblRev1Name, lblRev1Stars, lblRev1Desc, lblRev2Name, lblRev2Stars, lblRev2Desc });


            // Col 3: Stock
            RoundedPanel pnlStock = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0), FillColor = AppColors.BgCard };
            Label lblS = new Label { Text = "Low Stock Alert", Font = AppFonts.CardTitle, ForeColor = AppColors.TextSecondary, Location = new Point(20, 20), AutoSize = true };
            
            Label lblProd = new Label { Text = "Wireless Noise-Cancelling Headphones", Font = AppFonts.Button, ForeColor = AppColors.Danger, Location = new Point(20, 60), AutoSize = true };
            Label lblStockDesc = new Label { Text = "Only 2 items left in inventory.\nRestock immediately to avoid losing sales.", Font = AppFonts.Body, ForeColor = AppColors.TextSecondary, Location = new Point(20, 85), AutoSize = true };
            
            Button btnRestock = new Button 
            {
                Text = "Restock Now",
                Location = new Point(20, 140),
                Size = new Size(150, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.AccentBlue,
                ForeColor = Color.White,
                Font = AppFonts.Button,
                Cursor = Cursors.Hand
            };
            btnRestock.FlatAppearance.BorderSize = 0;
            
            pnlStock.Controls.AddRange(new Control[] { lblS, lblProd, lblStockDesc, btnRestock });

            row3.Controls.Add(pnlBudget, 0, 0);
            row3.Controls.Add(pnlRev, 1, 0);
            row3.Controls.Add(pnlStock, 2, 0);

            // Add rows to main grid
            mainGrid.Controls.Add(row1, 0, 0);
            mainGrid.Controls.Add(row2, 0, 1);
            mainGrid.Controls.Add(row3, 0, 2);

            _pnlContent.Controls.Add(mainGrid);
        }

        private void WireEvents()
        {
            // Allow window dragging from Topbar
            _topbar.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            // Sync sidebar clicks with topbar title
            _sidebar.NavItemClicked += (s, pageName) => {
                if (pageName == "Logout")
                {
                    Application.Exit();
                }
                else
                {
                    _topbar.PageTitle = pageName;
                    _topbar.Subtitle = $"Welcome to the {pageName} dashboard.";
                }
            };
        }
    }
}
