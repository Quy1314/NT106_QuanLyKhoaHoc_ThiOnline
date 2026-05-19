/*
 * RoundedPanel.cs
 *
 * Layer: Presentation (Theme)
 * Custom Panel with rounded corners, double buffering, and anti-aliasing.
 */
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    public class RoundedPanel : Panel
    {
        private int _cornerRadius = 12;
        private Color _borderColor = AppColors.Border;
        private Color _fillColor = AppColors.BgCard;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        // We use FillColor instead of BackColor for the rounded interior
        // because BackColor is set to Transparent to hide the rectangle corners.
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; Invalidate(); }
        }

        public RoundedPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);
                     
            this.BackColor = Color.Transparent;
            this.Tag = "custom"; // Prevents AppColors.ApplyTheme from overwriting BackColor
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Deflate bounds by 1 pixel to ensure the 1px border is fully visible inside the control bounds
            Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            
            // Fill the rounded background
            GraphicsHelpers.FillRoundedRect(e.Graphics, bounds, _cornerRadius, _fillColor);
            
            // Draw the border
            GraphicsHelpers.DrawRoundedBorder(e.Graphics, bounds, _cornerRadius, _borderColor, 1f);
        }
    }
}
