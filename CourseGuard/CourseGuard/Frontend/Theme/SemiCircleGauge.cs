/*
 * SemiCircleGauge.cs
 *
 * Layer: Presentation (Theme)
 * Semicircle progress gauge for tracking goals.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    public class SemiCircleGauge : UserControl
    {
        private float _percent = 0.65f;
        private string _targetLabel = "Target: $250,000";
        private string _achievedLabel = "Achieved: $155,000";

        [Category("Data")]
        [DefaultValue(0.65f)]
        public float Percent 
        { 
            get => _percent; 
            set { _percent = Math.Max(0f, Math.Min(1f, value)); Invalidate(); } 
        }

        [Category("Data")]
        [DefaultValue("Target: $250,000")]
        public string TargetLabel { get => _targetLabel; set { _targetLabel = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue("Achieved: $155,000")]
        public string AchievedLabel { get => _achievedLabel; set { _achievedLabel = value; Invalidate(); } }

        public SemiCircleGauge()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Size = new Size(260, 160);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // Optional card background
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            GraphicsHelpers.FillRoundedRect(g, bounds, 12, AppColors.BgCard);
            GraphicsHelpers.DrawRoundedBorder(g, bounds, 12, AppColors.Border, 1f);

            // Title
            using (SolidBrush titleBrush = new SolidBrush(AppColors.TextSecondary))
            {
                g.DrawString("Monthly Goals", AppFonts.CardTitle, titleBrush, new PointF(15, 15));
            }

            // Draw arc in center
            float arcSize = 140;
            float arcX = (Width - arcSize) / 2f;
            float arcY = 40; // Push down for title
            RectangleF arcRect = new RectangleF(arcX, arcY, arcSize, arcSize);

            // 1. Draw semicircle track arc (180 to 360)
            using (Pen trackPen = new Pen(AppColors.BgCardHover, 16))
            {
                trackPen.StartCap = LineCap.Round;
                trackPen.EndCap = LineCap.Round;
                g.DrawArc(trackPen, arcRect, 180, 180);
            }

            // 2. Draw progress arc
            using (Pen progressPen = new Pen(AppColors.AccentBlue, 16))
            {
                progressPen.StartCap = LineCap.Round;
                progressPen.EndCap = LineCap.Round;
                float sweepAngle = _percent * 180f;
                if (sweepAngle > 0)
                {
                    g.DrawArc(progressPen, arcRect, 180, sweepAngle);
                }
            }

            // 3. Draw percent text centered inside the arc
            string percentText = $"{Math.Round(_percent * 100)}%";
            using (SolidBrush textBrush = new SolidBrush(AppColors.TextPrimary))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                // Center vertically in the top half of the bounding box
                RectangleF textRect = new RectangleF(arcX, arcY + arcSize/4, arcSize, arcSize/2);
                g.DrawString(percentText, AppFonts.Title, textBrush, textRect, sf);
            }

            // 4. Draw TargetLabel and AchievedLabel below arc
            using (SolidBrush labelBrush = new SolidBrush(AppColors.TextSecondary))
            {
                float labelY = arcY + arcSize/2 + 20; // 20px padding below arc
                g.DrawString(_targetLabel, AppFonts.Caption, labelBrush, new PointF(20, labelY));
                
                float achievedX = Width - g.MeasureString(_achievedLabel, AppFonts.Caption).Width - 20;
                g.DrawString(_achievedLabel, AppFonts.Caption, labelBrush, new PointF(achievedX, labelY));
            }
        }
    }
}
