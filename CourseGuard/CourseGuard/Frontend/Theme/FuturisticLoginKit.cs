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

        public void DrawBackgroundGraphics(Graphics g, Rectangle clipRect)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Base background: #F1F5F9 (Slate 100)
            g.Clear(ColorTranslator.FromHtml("#F1F5F9"));

            // Radial glows - Soft pastel colors with high transparency
            float maxDim = Math.Max(Width, Height);
            
            // Layer 1: Soft Sky Blue glow behind the card (left-center)
            DrawGlowBlob(g, new PointF(Width * 0.30f, Height * 0.40f), maxDim * 0.7f, Color.FromArgb(35, 14, 165, 233));
            
            // Layer 2: Soft Indigo glow on the left side
            DrawGlowBlob(g, new PointF(Width * 0.15f, Height * 0.55f), maxDim * 0.8f, Color.FromArgb(30, 99, 102, 241));
            
            // Layer 3: Soft Pink/Lavender glow on the right side
            DrawGlowBlob(g, new PointF(Width * 0.80f, Height * 0.50f), maxDim * 0.9f, Color.FromArgb(30, 236, 72, 153));
            
            // Layer 4: Soft Purple glow on the top-right side
            DrawGlowBlob(g, new PointF(Width * 0.65f, Height * 0.30f), maxDim * 0.8f, Color.FromArgb(30, 168, 85, 247));

            // Floating particles: Soft Indigo and Pink (25% opacity)
            using var particleBrush = new SolidBrush(Color.FromArgb(50, 99, 102, 241));
            using var particleBrush2 = new SolidBrush(Color.FromArgb(50, 236, 72, 153));
            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                var b = (i % 2 == 0) ? particleBrush : particleBrush2;
                float a = p.Size;
                g.FillEllipse(b, p.X - a / 2f, p.Y - a / 2f, a, a);
            }

            // Mouse-follow glow: Soft Indigo (subtle but visible)
            DrawMouseGlow(g);

            // Soft white vignette to fade edges nicely
            using var vignette = new PathGradientBrush(new[]
            {
                new Point(0,0), new Point(Width,0), new Point(Width,Height), new Point(0,Height)
            })
            {
                CenterColor = Color.FromArgb(0, 255, 255, 255),
                SurroundColors = new[] { Color.FromArgb(140, 255, 255, 255) }
            };
            g.FillRectangle(vignette, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawBackgroundGraphics(e.Graphics, e.ClipRectangle);
        }

        private void DrawGlowBlob(Graphics g, PointF center, float radius, Color color)
        {
            RectangleF r = new(center.X - radius / 2f, center.Y - radius / 2f, radius, radius);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(r);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterPoint = center;
                    pgb.CenterColor = color;
                    pgb.SurroundColors = new[] { Color.FromArgb(0, color.R, color.G, color.B) };
                    g.FillPath(pgb, path);
                }
            }
        }

        private void DrawMouseGlow(Graphics g)
        {
            if (Width <= 0 || Height <= 0) return;
            var center = new PointF(_mouse.X, _mouse.Y);
            float radius = Math.Min(Math.Min(Width, Height) * 0.55f, 520);
            RectangleF r = new(center.X - radius / 2f, center.Y - radius / 2f, radius, radius);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(r);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterPoint = center;
                    pgb.CenterColor = Color.FromArgb(25, 99, 102, 241);
                    pgb.SurroundColors = new[] { Color.FromArgb(0, 99, 102, 241) };
                    g.FillPath(pgb, path);
                }
            }
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
        private float _scanProgress;
        private bool _isHover;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float CornerRadius { get; set; } = 32f; // 2rem / 32px matching mockup

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GlassFill { get; set; } = Color.FromArgb(215, 255, 255, 255); // beautiful frosted white glassmorphism

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(200, 226, 232, 240); // Slate 200 border

        public GlassCardHostPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Tag = "custom";

            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += (_, _) =>
            {
                float target = _isHover ? 1f : 0f;
                _hover = _hover + (target - _hover) * 0.10f;
                _sweep = (_sweep + 0.02f) % 1.2f;
                _scanProgress = (_scanProgress + 0.003f) % 1.0f; // travel speed of scanning line
                Invalidate(); // redraw to animate the scanning line
            };
            _timer.Start();

            MouseEnter += (_, _) => _isHover = true;
            MouseLeave += (_, _) => _isHover = false;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent is AnimatedFuturisticBackgroundPanel bg)
            {
                var g = e.Graphics;
                var state = g.Save();
                g.TranslateTransform(-Left, -Top);
                bg.DrawBackgroundGraphics(g, new Rectangle(Left, Top, Width, Height));
                g.Restore(state);
            }
            else if (Parent != null)
            {
                var g = e.Graphics;
                var state = g.Save();
                g.TranslateTransform(-Left, -Top);
                using (var pe = new PaintEventArgs(g, new Rectangle(Left, Top, Width, Height)))
                {
                    InvokePaintBackground(Parent, pe);
                    InvokePaint(Parent, pe);
                }
                g.Restore(state);
            }
            else
            {
                base.OnPaintBackground(e);
            }
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

            float margin = 24f;
            var rect = new RectangleF(margin, margin, Width - margin * 2f, Height - margin * 2f);
            using var path = FuturisticLoginKit.CreateRoundedRect(rect, CornerRadius);

            // Draw soft drop shadow (elevation 3D) outside the card bounds
            var stateGlow = g.Save();
            using (var reg = new Region(path))
            {
                g.ExcludeClip(reg);
            }
            // Deep soft shadow matching Slate 900 base shadow
            DrawSoftGlow(g, rect, CornerRadius, Color.FromArgb((int)(12 + 12 * _hover), 15, 23, 42), 24); // spread = 24
            g.Restore(stateGlow);

            // Glass fill
            using (var fill = new SolidBrush(GlassFill))
                g.FillPath(fill, path);

            // Light sweep on hover (refined white shimmer)
            if (_hover > 0.05f)
            {
                float sweepW = rect.Width * 0.55f;
                float x = rect.X + (rect.Width + sweepW) * (_sweep - 0.2f) - sweepW;
                using var sweepBrush = new LinearGradientBrush(
                    new RectangleF(x, rect.Y, sweepW, rect.Height),
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
                g.FillRectangle(sweepBrush, x, rect.Y, sweepW, rect.Height);
                g.Restore(state);
            }

            // Decorative Scan Line (Indigo 500 scan line sweeping down)
            float scanY = rect.Y + rect.Height * _scanProgress;
            float opacity = (float)Math.Sin(_scanProgress * Math.PI); // peak opacity in middle
            if (opacity > 0.01f)
            {
                var state = g.Save();
                g.SetClip(path);

                using (var scanBrush = new LinearGradientBrush(
                    new RectangleF(rect.X, scanY - 2, rect.Width, 4),
                    Color.FromArgb(0, 99, 102, 241),
                    Color.FromArgb((int)(160 * opacity), 99, 102, 241),
                    0f))
                {
                    var cb = new ColorBlend
                    {
                        Colors = new[]
                        {
                            Color.FromArgb(0, 99, 102, 241),
                            Color.FromArgb((int)(160 * opacity), 99, 102, 241),
                            Color.FromArgb(0, 99, 102, 241)
                        },
                        Positions = new[] { 0f, 0.5f, 1f }
                    };
                    scanBrush.InterpolationColors = cb;
                    g.FillRectangle(scanBrush, rect.X, scanY - 2, rect.Width, 4);
                }

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
            ForeColor = Color.White; // Clean white text
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

            // Glow - Soft Indigo glow
            int glowA = _hover ? 110 : 60;
            DrawGlow(g, rect, r, Color.FromArgb(glowA, 99, 102, 241), 14);

            // Gradient fill (Indigo 500 to Violet 600)
            var c1 = _hover ? Color.FromArgb(255, 79, 70, 229) : Color.FromArgb(255, 99, 102, 241);
            var c2 = _hover ? Color.FromArgb(255, 55, 48, 163) : Color.FromArgb(255, 124, 58, 237);
            using (var bg = new LinearGradientBrush(rect, c1, c2, 25f))
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

    internal class TransparentPanel : Panel
    {
        public TransparentPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Tag = "custom"; // Prevents AppColors.ApplyTheme from overwriting BackColor
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent != null)
            {
                var g = e.Graphics;
                var state = g.Save();
                g.TranslateTransform(-Left, -Top);
                using (var pe = new PaintEventArgs(g, new Rectangle(Left, Top, Width, Height)))
                {
                    InvokePaintBackground(Parent, pe);
                    // InvokePaint(Parent, pe); // Removed to fix border drawing artifacts
                }
                g.Restore(state);
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }
    }
}

