using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    internal static class AcademicTheme
    {
        public static readonly Color AppBackground = Color.FromArgb(247, 249, 252);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceLow = Color.FromArgb(242, 244, 247);
        public static readonly Color Primary = Color.FromArgb(63, 81, 181);
        public static readonly Color PrimaryStrong = Color.FromArgb(36, 56, 156);
        public static readonly Color TextPrimary = Color.FromArgb(25, 28, 30);
        public static readonly Color TextSecondary = Color.FromArgb(69, 70, 82);
        public static readonly Color BorderSoft = Color.FromArgb(224, 227, 230);

        public static void StyleCard(Panel panel, int radius = 12)
        {
            panel.BackColor = Surface;
            panel.Paint -= PanelRoundedPaint;
            panel.Paint += PanelRoundedPaint;
            ApplyRoundedRegion(panel, radius);
            panel.Resize -= PanelResizeRound;
            panel.Resize += PanelResizeRound;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = BorderSoft;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceLow;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceLow;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextSecondary;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(222, 224, 255);
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private static void PanelResizeRound(object? sender, System.EventArgs e)
        {
            if (sender is Panel panel)
            {
                ApplyRoundedRegion(panel, 12);
            }
        }

        private static void ApplyRoundedRegion(Panel panel, int radius)
        {
            if (panel.Width <= 0 || panel.Height <= 0) return;
            using GraphicsPath path = GetRoundedRect(new Rectangle(0, 0, panel.Width, panel.Height), radius);
            panel.Region = new Region(path);
        }

        private static void PanelRoundedPaint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen p = new Pen(BorderSoft, 1f);
            using GraphicsPath path = GetRoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 12);
            e.Graphics.DrawPath(p, path);
        }

        private static GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
