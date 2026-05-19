using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    public sealed class SearchBoxPanel : RoundedPanel
    {
        private readonly TextBox _input;

        public SearchBoxPanel(TextBox input, int width = 330)
        {
            _input = input;

            Width = width;
            Height = 36;
            MinimumSize = new Size(180, 36);
            Margin = Padding.Empty;
            CornerRadius = 12;
            Cursor = Cursors.IBeam;

            SearchFocusManager.MarkSearchInput(_input);
            _input.BorderStyle = BorderStyle.None;
            _input.Font = AppFonts.Body;
            _input.TabStop = false;
            _input.Margin = Padding.Empty;
            _input.Cursor = Cursors.IBeam;

            Controls.Add(_input);
            ApplyTheme();
            LayoutInput();

            MouseDown += (_, _) => _input.Focus();
        }

        public void ApplyTheme()
        {
            FillColor = AppColors.IsDarkMode ? AppColors.BgInput : ColorTranslator.FromHtml("#F8FAFC");
            BorderColor = AppColors.Border;
            _input.BackColor = FillColor;
            _input.ForeColor = AppColors.TextPrimary;
            Invalidate();
        }

        protected override void OnResize(System.EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Height = 36;
            LayoutInput();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            base.OnPaint(e);
            DrawSearchIcon(e.Graphics, new Rectangle(11, 9, 17, 17), AppColors.TextMuted);
        }

        private void LayoutInput()
        {
            if (_input.IsDisposed)
                return;

            _input.SetBounds(36, 9, Math.Max(40, Width - 48), 18);
        }

        private static void DrawSearchIcon(Graphics g, Rectangle bounds, Color color)
        {
            using Pen pen = new Pen(color, 1.7f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            g.DrawEllipse(pen, bounds.Left + 1, bounds.Top + 1, 9, 9);
            g.DrawLine(pen, bounds.Left + 10, bounds.Top + 10, bounds.Right - 1, bounds.Bottom - 1);
        }
    }
}
