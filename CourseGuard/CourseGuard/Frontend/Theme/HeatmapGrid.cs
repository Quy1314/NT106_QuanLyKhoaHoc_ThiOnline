/*
 * HeatmapGrid.cs
 *
 * Layer: Presentation (Theme)
 * Heatmap grid representing activity or sales density.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    public class HeatmapGrid : UserControl
    {
        private bool[,] _data = new bool[6, 7];
        private string[] _rowLabels = { "Laptop", "Monitor", "Audio", "Sound", "keyboard", "Mouse" };
        private string[] _colLabels = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool[,] Data 
        { 
            get => _data; 
            set { _data = value; Invalidate(); } 
        }

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] RowLabels 
        { 
            get => _rowLabels; 
            set { _rowLabels = value; Invalidate(); } 
        }

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] ColLabels 
        { 
            get => _colLabels; 
            set { _colLabels = value; Invalidate(); } 
        }

        public HeatmapGrid()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);
            
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Size = new Size(350, 250);

            // Populate some fake data for designer preview
            _data[1, 5] = true; _data[1, 6] = true;
            _data[2, 3] = true; _data[2, 4] = true; _data[2, 5] = true; _data[2, 6] = true;
            _data[3, 3] = true; _data[3, 4] = true;
            _data[4, 1] = true; _data[4, 2] = true; _data[4, 6] = true;
            _data[5, 3] = true; _data[5, 4] = true; _data[5, 5] = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // Background card
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            GraphicsHelpers.FillRoundedRect(g, bounds, 12, AppColors.BgCard);
            GraphicsHelpers.DrawRoundedBorder(g, bounds, 12, AppColors.Border, 1f);

            // Title
            using (SolidBrush titleBrush = new SolidBrush(AppColors.TextPrimary))
            {
                g.DrawString("Top Products", AppFonts.CardTitle, titleBrush, new PointF(15, 15));
            }

            if (_rowLabels == null || _colLabels == null || _data == null) return;
            
            int rows = Math.Min(6, _rowLabels.Length);
            int cols = Math.Min(7, _colLabels.Length);

            // Layout metrics
            int leftMargin = 70; // Space for row labels
            int topMargin = 55;  // Space for title + col labels
            int rightMargin = 20;
            int bottomMargin = 20;
            int gap = 4;
            
            int availableWidth = Width - leftMargin - rightMargin; 
            int availableHeight = Height - topMargin - bottomMargin; 

            float cellW = (availableWidth - (cols - 1) * gap) / (float)cols;
            float cellH = (availableHeight - (rows - 1) * gap) / (float)rows;

            if (cellW < 2 || cellH < 2) return;

            using (SolidBrush textMutedBrush = new SolidBrush(AppColors.TextMuted))
            using (SolidBrush textSecBrush = new SolidBrush(AppColors.TextSecondary))
            {
                StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
                StringFormat sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

                // 1. Draw ColLabels at top (just above the grid cells)
                for (int c = 0; c < cols; c++)
                {
                    float x = leftMargin + c * (cellW + gap);
                    RectangleF lblRect = new RectangleF(x, topMargin - 20, cellW, 20);
                    g.DrawString(_colLabels[c], AppFonts.Caption, textMutedBrush, lblRect, sfCenter);
                }

                // Draw rows
                for (int r = 0; r < rows; r++)
                {
                    float y = topMargin + r * (cellH + gap);

                    // 2. Draw RowLabels on left
                    RectangleF rowLblRect = new RectangleF(15, y, leftMargin - 20, cellH);
                    g.DrawString(_rowLabels[r], AppFonts.Caption, textSecBrush, rowLblRect, sfLeft);

                    // 3. Draw cells
                    for (int c = 0; c < cols; c++)
                    {
                        float x = leftMargin + c * (cellW + gap);
                        Rectangle cellBounds = new Rectangle((int)x, (int)y, (int)cellW, (int)cellH);
                        
                        // Bounds safety check
                        if (r < _data.GetLength(0) && c < _data.GetLength(1))
                        {
                            bool isActive = _data[r, c];
                            // Note: mapped to HeatmapOn/Off in AppColors
                            Color cellColor = isActive ? AppColors.HeatmapOn : AppColors.HeatmapOff;
                            
                            GraphicsHelpers.FillRoundedRect(g, cellBounds, 4, cellColor);
                        }
                    }
                }
            }
        }
    }
}
