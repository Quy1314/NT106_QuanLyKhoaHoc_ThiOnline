/*
 * RoundedButtonHelper.cs
 * 
 * Layer: Presentation (Theme)
 * Vai trò: Helper class để bo góc (rounded corners) cho Button trong WinForms.
 * WinForms mặc định không hỗ trợ border-radius. Dùng GraphicsPath trong sự kiện Paint 
 * để vẽ button chống răng cưa (Anti-alias) thay cho Region.
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CourseGuard.Presentation.Theme
{
    public static class RoundedButtonHelper
    {
        /// <summary>
        /// Áp dụng bo góc cho một Button.
        /// Gọi trong constructor hoặc Load event của Form/UserControl.
        /// </summary>
        public static void Apply(Button btn, int radius = 15)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            
            // Bỏ Region cũ nếu có
            btn.Region = null;

            btn.Paint += (s, e) => 
            {
                Button b = s as Button;
                if (b == null) return;

                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // ── BƯỚC 1: Xóa nền bằng màu cha (ẩn đi nút hình chữ nhật gốc)
                Color parentColor = b.Parent?.BackColor ?? Color.White;
                g.Clear(parentColor);

                // ── BƯỚC 2: Vẽ shape bo góc lên trên
                Rectangle rect = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
                using (GraphicsPath path = GetRoundedRect(rect, radius))
                {
                    // Lấy màu nền, nếu Transparent thì dùng nguyên màu cha để giả vờ
                    Color bgColor = b.BackColor == Color.Transparent ? parentColor : b.BackColor;

                    using (SolidBrush brush = new SolidBrush(bgColor))
                    {
                        g.FillPath(brush, path);
                    }

                    // ── BƯỚC 3: Vẽ viền ngoài cùng màu cha để làm mịn phần pixels bao ngoài
                    using (Pen pen = new Pen(parentColor, 1.5f))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // ── BƯỚC 4: Vẽ lại định dạng chữ và ảnh
                if (!string.IsNullOrEmpty(b.Text))
                {
                    TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
                    TextRenderer.DrawText(g, b.Text, b.Font, rect, b.ForeColor, flags);
                }

                if (b.Image != null)
                {
                    int imgX = (b.Width - b.Image.Width) / 2;
                    int imgY = (b.Height - b.Image.Height) / 2;
                    g.DrawImage(b.Image, imgX, imgY);
                }
            };
        }

        /// <summary>
        /// Áp dụng bo góc cho nhiều buttons cùng lúc.
        /// </summary>
        public static void Apply(int radius, params Button[] buttons)
        {
            foreach (var btn in buttons)
            {
                Apply(btn, radius);
            }
        }

        private static GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

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
