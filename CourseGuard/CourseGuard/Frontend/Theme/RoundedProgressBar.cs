/*
 * RoundedProgressBar.cs
 *
 * Layer: Presentation (Theme)
 * Rounded progress bar for budget usage or general progress tracking.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    public class RoundedProgressBar : Control
    {
        private int _value = 0;
        private int _maximum = 100;
        private Color _barColor = AppColors.AccentBlue;
        private string _displayText = "";

        [Category("Data")]
        [DefaultValue(0)]
        public int Value 
        { 
            get => _value; 
            set 
            { 
                _value = Math.Max(0, Math.Min(_maximum, value)); 
                Invalidate(); 
            } 
        }

        [Category("Data")]
        [DefaultValue(100)]
        public int Maximum 
        { 
            get => _maximum; 
            set 
            { 
                _maximum = Math.Max(1, value); 
                if (_value > _maximum) _value = _maximum;
                Invalidate(); 
            } 
        }

        [Category("Appearance")]
        public Color BarColor 
        { 
            get => _barColor; 
            set { _barColor = value; Invalidate(); } 
        }

        private bool ShouldSerializeBarColor() => _barColor != AppColors.AccentBlue;
        private void ResetBarColor() => _barColor = AppColors.AccentBlue;

        [Category("Data")]
        [DefaultValue("")]
        public string DisplayText 
        { 
            get => _displayText; 
            set { _displayText = value; Invalidate(); } 
        }

        public RoundedProgressBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);
            
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Height = 28;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Force height to 28
            base.SetBoundsCore(x, y, width, 28, specified);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            int radius = Height / 2;
            Rectangle trackRect = new Rectangle(0, 0, Width - 1, Height - 1);

            // 1. Fill rounded track
            GraphicsHelpers.FillRoundedRect(g, trackRect, radius, AppColors.ProgressTrack);

            // 2. Fill rounded progress
            if (_maximum > 0 && _value > 0)
            {
                int fillWidth = (int)((_value / (float)_maximum) * (Width - 1));
                
                // Ensure minimum width looks like a pill if > 0
                if (fillWidth < radius * 2) 
                    fillWidth = radius * 2;
                if (fillWidth > Width - 1)
                    fillWidth = Width - 1;

                Rectangle fillRect = new Rectangle(0, 0, fillWidth, Height - 1);
                GraphicsHelpers.FillRoundedRect(g, fillRect, radius, _barColor);
            }

            // 3. Draw value text
            if (!string.IsNullOrEmpty(_displayText))
            {
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    };
                    
                    // Left align with 15px padding
                    RectangleF textRect = new RectangleF(15, 0, Width - 15, Height);
                    g.DrawString(_displayText, AppFonts.Body, textBrush, textRect, sf);
                }
            }
        }
    }
}
