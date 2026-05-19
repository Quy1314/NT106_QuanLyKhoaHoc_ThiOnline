/*
 * StatCard.cs
 *
 * Layer: Presentation (Theme)
 * Reusable metric card with icon, trend indicator, and mini bar chart.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    public class StatCard : UserControl
    {
        private string _title = "Total Revenue";
        private string _value = "$48,295";
        private string _trendPercent = "47%";
        private bool _trendUp = true;
        private string _iconChar = "$";
        private string _caption = "So với tháng trước";
        private bool _isHovered = false;

        [Category("Data")]
        [DefaultValue("Total Revenue")]
        public string Title { get => _title; set { _title = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue("$48,295")]
        public string Value { get => _value; set { _value = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue("47%")]
        public string TrendPercent { get => _trendPercent; set { _trendPercent = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue(true)]
        public bool TrendUp { get => _trendUp; set { _trendUp = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue("$")]
        public string IconChar { get => _iconChar; set { _iconChar = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue("So với tháng trước")]
        public string Caption { get => _caption; set { _caption = value; Invalidate(); } }

        public StatCard()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Size = new Size(300, 120);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            Color bgColor = _isHovered ? AppColors.BgCardHover : AppColors.BgCard;

            // 1. Rounded background
            GraphicsHelpers.FillRoundedRect(g, bounds, 12, bgColor);
            GraphicsHelpers.DrawRoundedBorder(g, bounds, 12, AppColors.Border, 1f);

            int padding = 22;
            int iconSize = 42;
            int chartWidth = 58;
            int chartHeight = 54;
            int gap = 16;

            Rectangle chartRect = new Rectangle(
                bounds.Right - padding - chartWidth,
                bounds.Top + (bounds.Height - chartHeight) / 2,
                chartWidth,
                chartHeight);

            int left = bounds.Left + padding;
            int top = bounds.Top + padding;
            int rightLimit = chartRect.Left - gap;

            Rectangle iconRect = new Rectangle(left, top, iconSize, iconSize);
            using (SolidBrush iconBgBrush = new SolidBrush(AppColors.AccentSoft))
            {
                g.FillEllipse(iconBgBrush, iconRect);
            }

            DrawMetricIcon(g, iconRect, _iconChar, AppColors.AccentBlue);

            int textLeft = iconRect.Right + 12;
            int textWidth = Math.Max(40, rightLimit - textLeft);
            RectangleF titleRect = new RectangleF(textLeft, top + 3, textWidth, 18);
            RectangleF valueRect = new RectangleF(textLeft, top + 30, Math.Max(80, rightLimit - textLeft), 36);
            RectangleF statusRect = new RectangleF(textLeft, bounds.Bottom - padding - 18, Math.Max(90, rightLimit - textLeft), 18);

            using (SolidBrush titleBrush = new SolidBrush(AppColors.TextSecondary))
            {
                using StringFormat titleFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                g.DrawString(_title, AppFonts.CardTitle, titleBrush, titleRect, titleFormat);
            }

            using (SolidBrush valueBrush = new SolidBrush(AppColors.TextPrimary))
            {
                using StringFormat valueFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                g.DrawString(_value, AppFonts.Metric, valueBrush, valueRect, valueFormat);
            }

            Color trendColor = _trendUp ? AppColors.Success : AppColors.Danger;
            string trendArrow = _trendUp ? "↑" : "↓";
            
            using (SolidBrush trendBrush = new SolidBrush(trendColor))
            using (SolidBrush mutedBrush = new SolidBrush(AppColors.TextMuted))
            {
                string trendText = $"{trendArrow} {_trendPercent}";
                using StringFormat statusFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                RectangleF trendRect = new RectangleF(statusRect.Left, statusRect.Top, statusRect.Width, statusRect.Height);
                g.DrawString(trendText, AppFonts.Caption, trendBrush, trendRect, statusFormat);
                
                float offset = g.MeasureString(trendText, AppFonts.Caption).Width + 6;
                RectangleF captionRect = new RectangleF(statusRect.Left + offset, statusRect.Top, Math.Max(0, statusRect.Width - offset), statusRect.Height);
                g.DrawString(_caption, AppFonts.Caption, mutedBrush, captionRect, statusFormat);
            }

            float[] barHeights = { 0.4f, 0.7f, 0.5f, 0.9f, 0.6f, 1.0f };
            int barWidth = 6;
            int barSpacing = 4;
            int barsWidth = 6 * barWidth + 5 * barSpacing;
            int startX = chartRect.Left + (chartRect.Width - barsWidth) / 2;
            int startY = chartRect.Top;

            for (int i = 0; i < 6; i++)
            {
                int h = (int)(barHeights[i] * chartHeight);
                // Clamp height to minimum 2 to ensure rounding doesn't fail
                if (h < 2) h = 2;
                
                int y = startY + (chartHeight - h);
                int x = startX + i * (barWidth + barSpacing);
                Rectangle barRect = new Rectangle(x, y, barWidth, h);

                Color bColor = (i == 5) ? AppColors.AccentBlue : AppColors.BarInactive;
                GraphicsHelpers.FillRoundedRect(g, barRect, 3, bColor);
            }
        }

        private static void DrawMetricIcon(Graphics g, Rectangle bounds, string iconName, Color color)
        {
            string key = (iconName ?? string.Empty).Trim().ToLowerInvariant();
            Rectangle r = new Rectangle(bounds.Left + 10, bounds.Top + 10, bounds.Width - 20, bounds.Height - 20);

            using Pen pen = new Pen(color, 1.8f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            using SolidBrush brush = new SolidBrush(color);

            switch (key)
            {
                case "course":
                case "book":
                    g.DrawRectangle(pen, r.Left + 2, r.Top + 2, r.Width - 4, r.Height - 4);
                    g.DrawLine(pen, r.Left + 7, r.Top + 2, r.Left + 7, r.Bottom - 2);
                    break;
                case "exam":
                case "edit":
                    g.DrawLine(pen, r.Left + 4, r.Bottom - 4, r.Right - 4, r.Top + 4);
                    g.DrawLine(pen, r.Right - 7, r.Top + 4, r.Right - 3, r.Top + 8);
                    g.FillEllipse(brush, r.Left + 2, r.Bottom - 5, 4, 4);
                    break;
                case "notice":
                case "bell":
                    g.DrawArc(pen, r.Left + 5, r.Top + 3, r.Width - 10, r.Height - 7, 200, 140);
                    g.DrawLine(pen, r.Left + 5, r.Top + 10, r.Left + 3, r.Bottom - 4);
                    g.DrawLine(pen, r.Right - 5, r.Top + 10, r.Right - 3, r.Bottom - 4);
                    g.DrawLine(pen, r.Left + 3, r.Bottom - 4, r.Right - 3, r.Bottom - 4);
                    g.FillEllipse(brush, r.Left + 9, r.Bottom - 2, 3, 3);
                    break;
                case "score":
                case "award":
                    Point[] star =
                    {
                        new Point(r.Left + 10, r.Top + 1), new Point(r.Left + 12, r.Top + 7),
                        new Point(r.Right - 2, r.Top + 7), new Point(r.Left + 14, r.Top + 11),
                        new Point(r.Left + 16, r.Bottom - 1), new Point(r.Left + 10, r.Top + 14),
                        new Point(r.Left + 4, r.Bottom - 1), new Point(r.Left + 6, r.Top + 11),
                        new Point(r.Left + 2, r.Top + 7), new Point(r.Left + 8, r.Top + 7),
                        new Point(r.Left + 10, r.Top + 1)
                    };
                    g.DrawLines(pen, star);
                    break;
                default:
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using Font iconFont = AppFonts.Semibold(12f);
                    g.DrawString(string.IsNullOrWhiteSpace(iconName) ? "•" : iconName[..Math.Min(iconName.Length, 2)].ToUpperInvariant(), iconFont, brush, bounds, sf);
                    break;
                }
            }
        }
    }
}
