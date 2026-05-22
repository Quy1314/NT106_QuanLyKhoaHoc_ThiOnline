using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    internal static class FuturisticLoginKit
    {
        public static Font CreateUiFont(float size, FontStyle style = FontStyle.Regular)
        {
            string[] preferred = { "Inter", "Poppins", "Segoe UI Variable", "Segoe UI" };
            foreach (var name in preferred)
            {
                try
                {
                    using var test = new Font(name, size, style);
                    if (string.Equals(test.Name, name, StringComparison.OrdinalIgnoreCase))
                        return new Font(name, size, style);
                }
                catch { /* ignore */ }
            }
            var fallbackFont = SystemFonts.MessageBoxFont;
            return new Font(fallbackFont?.FontFamily ?? FontFamily.GenericSansSerif, size, style);
        }

        public static GraphicsPath CreateRoundedRect(RectangleF rect, float radius)
        {
            float d = radius * 2f;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static Color WithAlpha(Color c, int alpha) => Color.FromArgb(Math.Clamp(alpha, 0, 255), c);
    }

    internal sealed class AnimatedFuturisticBackgroundPanel : Panel
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly List<Particle> _particles = new();
        private readonly Random _rng = new();
        private float _t;
        private Point _mouse;

        public AnimatedFuturisticBackgroundPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            Dock = DockStyle.Fill;

            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += (_, _) =>
            {
                _t += 0.012f;
                StepParticles();
                Invalidate();
            };
            _timer.Start();

            MouseMove += (_, e) => _mouse = e.Location;
            Resize += (_, _) => EnsureParticles();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer.Dispose();
            base.Dispose(disposing);
        }

        private void EnsureParticles()
        {
            int target = Math.Clamp((Width * Height) / 35000, 45, 120);
            while (_particles.Count < target) _particles.Add(Particle.Create(_rng, Width, Height));
            while (_particles.Count > target && _particles.Count > 0) _particles.RemoveAt(_particles.Count - 1);
        }

        private void StepParticles()
        {
            if (Width <= 0 || Height <= 0) return;
            EnsureParticles();

            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                p.X += p.Vx;
                p.Y += p.Vy;
                if (p.X < -50) p.X = Width + 50;
                if (p.X > Width + 50) p.X = -50;
                if (p.Y < -50) p.Y = Height + 50;
                if (p.Y > Height + 50) p.Y = -50;
                _particles[i] = p;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Base: dark cinematic background
            g.Clear(Color.FromArgb(8, 8, 12));

            // Animated gradient wash
            using (var brush = new LinearGradientBrush(ClientRectangle,
                       Color.FromArgb(255, 18, 10, 32),
                       Color.FromArgb(255, 6, 18, 24),
                       (float)(45 + 35 * Math.Sin(_t * 0.7f))))
            {
                var blend = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.FromArgb(255, 10, 10, 14),
                        Color.FromArgb(255, 35, 12, 65),   // neon purple tint
                        Color.FromArgb(255, 0, 60, 75),    // cyan tint
                        Color.FromArgb(255, 10, 10, 14),
                    },
                    Positions = new[] { 0f, 0.38f, 0.72f, 1f }
                };
                brush.InterpolationColors = blend;
                g.FillRectangle(brush, ClientRectangle);
            }

            // Big glow blobs (depth)
            DrawGlowBlob(g, new PointF(Width * 0.25f, Height * 0.30f), 520, Color.FromArgb(180, 160, 60, 255));
            DrawGlowBlob(g, new PointF(Width * 0.78f, Height * 0.55f), 640, Color.FromArgb(150, 30, 220, 255));
            DrawGlowBlob(g, new PointF(Width * 0.55f, Height * 0.20f), 420, Color.FromArgb(120, 210, 120, 255));

            // Floating particles
            using var particleBrush = new SolidBrush(Color.FromArgb(115, 210, 240, 255));
            using var particleBrush2 = new SolidBrush(Color.FromArgb(95, 210, 120, 255));
            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                var b = (i % 3 == 0) ? particleBrush2 : particleBrush;
                float a = p.Size;
                g.FillEllipse(b, p.X - a / 2f, p.Y - a / 2f, a, a);
            }

            // Mouse-follow glow (subtle, blended)
            DrawMouseGlow(g);

            // Soft vignette
            using var vignette = new PathGradientBrush(new[]
            {
                new Point(0,0), new Point(Width,0), new Point(Width,Height), new Point(0,Height)
            })
            {
                CenterColor = Color.FromArgb(0, 0, 0, 0),
                SurroundColors = new[] { Color.FromArgb(210, 0, 0, 0) }
            };
            g.FillRectangle(vignette, ClientRectangle);
        }

        private void DrawGlowBlob(Graphics g, PointF center, float radius, Color color)
        {
            RectangleF r = new(center.X - radius / 2f, center.Y - radius / 2f, radius, radius);
            using var pgb = new PathGradientBrush(new[] { new PointF(r.Left, r.Top), new PointF(r.Right, r.Top), new PointF(r.Right, r.Bottom), new PointF(r.Left, r.Bottom) })
            {
                CenterPoint = center,
                CenterColor = color,
                SurroundColors = new[] { Color.FromArgb(0, color) }
            };
            g.FillEllipse(pgb, r);
        }

        private void DrawMouseGlow(Graphics g)
        {
            if (Width <= 0 || Height <= 0) return;
            var center = new PointF(_mouse.X, _mouse.Y);
            float radius = Math.Min(Math.Min(Width, Height) * 0.55f, 520);
            RectangleF r = new(center.X - radius / 2f, center.Y - radius / 2f, radius, radius);
            using var pgb = new PathGradientBrush(new[] { new PointF(r.Left, r.Top), new PointF(r.Right, r.Top), new PointF(r.Right, r.Bottom), new PointF(r.Left, r.Bottom) })
            {
                CenterPoint = center,
                CenterColor = Color.FromArgb(65, 180, 80, 255),
                SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) }
            };
            g.FillEllipse(pgb, r);
        }

        private struct Particle
        {
            public float X;
            public float Y;
            public float Vx;
            public float Vy;
            public float Size;

            public static Particle Create(Random rng, int w, int h)
            {
                float x = (float)(rng.NextDouble() * Math.Max(1, w));
                float y = (float)(rng.NextDouble() * Math.Max(1, h));
                float vx = (float)((rng.NextDouble() - 0.5) * 0.55);
                float vy = (float)((rng.NextDouble() - 0.5) * 0.35);
                float size = (float)(1.6 + rng.NextDouble() * 2.6);
                return new Particle { X = x, Y = y, Vx = vx, Vy = vy, Size = size };
            }
        }
    }

    internal sealed class GlassCardHostPanel : Panel
    {
        private readonly System.Windows.Forms.Timer _timer;
        private float _hover;
        private float _sweep;
        private bool _isHover;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float CornerRadius { get; set; } = 26f;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GlassFill { get; set; } = Color.FromArgb(36, 255, 255, 255);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(110, 255, 255, 255);

        public GlassCardHostPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;

            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += (_, _) =>
            {
                float target = _isHover ? 1f : 0f;
                _hover = _hover + (target - _hover) * 0.10f;
                _sweep = (_sweep + 0.02f) % 1.2f;
                if (_hover > 0.001f || _isHover) Invalidate();
            };
            _timer.Start();

            MouseEnter += (_, _) => _isHover = true;
            MouseLeave += (_, _) => _isHover = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // avoid default background to keep it glassy over animated backdrop
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            using var path = FuturisticLoginKit.CreateRoundedRect(rect, CornerRadius);

            // Subtle neon shadow glow
            DrawSoftGlow(g, rect, CornerRadius, Color.FromArgb((int)(60 + 80 * _hover), 170, 80, 255), 18);
            DrawSoftGlow(g, rect, CornerRadius, Color.FromArgb((int)(40 + 70 * _hover), 40, 220, 255), 14);

            // Glass fill
            using (var fill = new SolidBrush(GlassFill))
                g.FillPath(fill, path);

            // Light sweep on hover
            if (_hover > 0.05f)
            {
                float sweepW = Width * 0.55f;
                float x = (Width + sweepW) * (_sweep - 0.2f) - sweepW;
                using var sweepBrush = new LinearGradientBrush(
                    new RectangleF(x, 0, sweepW, Height),
                    Color.FromArgb(0, 255, 255, 255),
                    Color.FromArgb((int)(55 * _hover), 255, 255, 255),
                    20f);
                var cb = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.FromArgb(0, 255,255,255),
                        Color.FromArgb((int)(70 * _hover), 255,255,255),
                        Color.FromArgb(0, 255,255,255)
                    },
                    Positions = new[] { 0f, 0.5f, 1f }
                };
                sweepBrush.InterpolationColors = cb;
                var state = g.Save();
                g.SetClip(path);
                g.FillRectangle(sweepBrush, x, 0, sweepW, Height);
                g.Restore(state);
            }

            // Border
            using var pen = new Pen(BorderColor, 1.1f);
            g.DrawPath(pen, path);
        }

        private static void DrawSoftGlow(Graphics g, RectangleF rect, float radius, Color glowColor, int spread)
        {
            for (int i = spread; i >= 1; i -= 2)
            {
                float alphaScale = (float)i / spread;
                int a = (int)(glowColor.A * (1f - alphaScale) * 0.55f);
                if (a <= 0) continue;
                using var p = FuturisticLoginKit.CreateRoundedRect(new RectangleF(rect.X - i, rect.Y - i, rect.Width + i * 2, rect.Height + i * 2), radius + i);
                using var b = new SolidBrush(Color.FromArgb(a, glowColor));
                g.FillPath(b, p);
            }
        }
    }

    internal sealed class NeonGradientButton : Button
    {
        private readonly System.Windows.Forms.Timer _timer;
        private bool _hover;
        private float _shimmer;
        private Point _lastClick;
        private float _ripple;

        public NeonGradientButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;

            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += (_, _) =>
            {
                _shimmer = (_shimmer + 0.025f) % 1.2f;
                if (_ripple > 0) _ripple *= 0.86f;
                Invalidate();
            };
            _timer.Start();

            MouseEnter += (_, _) => { _hover = true; Invalidate(); };
            MouseLeave += (_, _) => { _hover = false; Invalidate(); };
            MouseDown += (_, e) => { _lastClick = e.Location; _ripple = 1f; Invalidate(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            var rect = new RectangleF(0, 0, Width - 1f, Height - 1f);
            float r = Math.Min(Height, 56) / 2f;
            using var path = FuturisticLoginKit.CreateRoundedRect(rect, r);

            // Glow
            int glowA = _hover ? 120 : 70;
            DrawGlow(g, rect, r, Color.FromArgb(glowA, 160, 70, 255), 14);
            DrawGlow(g, rect, r, Color.FromArgb(glowA, 40, 220, 255), 12);

            // Gradient fill (purple -> cyan)
            using (var bg = new LinearGradientBrush(rect, Color.FromArgb(255, 140, 70, 255), Color.FromArgb(255, 20, 220, 255), 0f))
            {
                g.FillPath(bg, path);
            }

            // Shimmer band
            float bandW = Width * 0.45f;
            float x = (Width + bandW) * (_shimmer - 0.2f) - bandW;
            using (var shimmer = new LinearGradientBrush(new RectangleF(x, 0, bandW, Height),
                       Color.FromArgb(0, 255, 255, 255),
                       Color.FromArgb(_hover ? 90 : 55, 255, 255, 255),
                       20f))
            {
                var cb = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.FromArgb(0, 255,255,255),
                        Color.FromArgb(_hover ? 130 : 90, 255,255,255),
                        Color.FromArgb(0, 255,255,255),
                    },
                    Positions = new[] { 0f, 0.5f, 1f }
                };
                shimmer.InterpolationColors = cb;
                var state = g.Save();
                g.SetClip(path);
                g.FillRectangle(shimmer, x, 0, bandW, Height);
                g.Restore(state);
            }

            // Ripple effect (subtle)
            if (_ripple > 0.02f)
            {
                float maxR = Math.Max(Width, Height) * 1.15f;
                float rr = maxR * (1f - _ripple);
                int a = (int)(55 * _ripple);
                using var rippleBrush = new SolidBrush(Color.FromArgb(a, 255, 255, 255));
                var state = g.Save();
                g.SetClip(path);
                g.FillEllipse(rippleBrush, _lastClick.X - rr / 2f, _lastClick.Y - rr / 2f, rr, rr);
                g.Restore(state);
            }

            // Text
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                Rectangle.Round(rect),
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static void DrawGlow(Graphics g, RectangleF rect, float radius, Color color, int spread)
        {
            for (int i = spread; i >= 1; i -= 2)
            {
                float alphaScale = (float)i / spread;
                int a = (int)(color.A * (1f - alphaScale) * 0.6f);
                if (a <= 0) continue;
                using var p = FuturisticLoginKit.CreateRoundedRect(new RectangleF(rect.X - i, rect.Y - i, rect.Width + i * 2, rect.Height + i * 2), radius + i);
                using var b = new SolidBrush(Color.FromArgb(a, color));
                g.FillPath(b, p);
            }
        }
    }
}

