/*
 * MetaTheme.cs
 *
 * Layer: Presentation (Theme)
 * Dark modern design system inspired by todesktop.com.
 * Deep blue-black backgrounds, indigo accent, clean white text.
 *
 * Every public constant maps to the user-defined token palette.
 * Usage: MetaTheme.Colors.FormBg, MetaTheme.Fonts.BodyMd(), MetaTheme.Radius.Full, etc.
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    /// <summary>
    /// Centralized design tokens — dark modern (todesktop.com style).
    /// </summary>
    public static class MetaTheme
    {
        // ═══════════════════════════════════════════════════════════════
        //  COLORS
        // ═══════════════════════════════════════════════════════════════

        public static class Colors
        {
            // ── Core surfaces ───────────────────────────────────────
            // MUST use => (not readonly) so values update when AppColors.IsDarkMode toggles
            public static Color FormBg          => AppColors.BgBase;
            public static Color CardBg          => AppColors.BgCard;
            public static Color SidebarBg       => AppColors.BgSidebar;
            public static Color ElevatedBg      => AppColors.BgCard;
            public static Color InputBg         => AppColors.BgInput;

            // ── Accent / Highlight ──────────────────────────────────
            public static Color Accent          => AppColors.AccentBlue;
            public static Color AccentHover     => AppColors.AccentHover;
            public static Color AccentPressed   => AppColors.AccentPressed;
            public static Color AccentSoft      => AppColors.AccentSoft;
            public static Color AccentMuted     => AppColors.IsDarkMode
                ? Color.FromArgb(30, 58, 138)
                : Color.FromArgb(219, 234, 254);

            // ── Text ────────────────────────────────────────────────
            public static Color TextPrimary     => AppColors.TextPrimary;
            public static Color TextSecondary   => AppColors.TextSecondary;
            public static Color TextMuted       => AppColors.TextMuted;
            public static Color TextDisabled    => AppColors.IsDarkMode
                ? Color.FromArgb(71, 85, 105)
                : Color.FromArgb(148, 163, 184);

            // ── Borders ─────────────────────────────────────────────
            public static Color Border          => AppColors.Border;
            public static Color BorderSoft      => AppColors.IsDarkMode
                ? Color.FromArgb(30, 30, 36)
                : Color.FromArgb(226, 232, 240);
            public static Color BorderFocus     => Accent;

            // ── Semantic ────────────────────────────────────────────
            public static readonly Color Success         = Color.FromArgb(34, 197, 94);
            public static readonly Color SuccessBg       = Color.FromArgb(20, 34, 197, 94);
            public static readonly Color Warning         = Color.FromArgb(251, 191, 36);
            public static readonly Color WarningBg       = Color.FromArgb(20, 251, 191, 36);
            public static readonly Color Critical        = Color.FromArgb(239, 68, 68);
            public static readonly Color CriticalBg      = Color.FromArgb(20, 239, 68, 68);
            public static readonly Color Info            = Color.FromArgb(56, 189, 248);
            public static readonly Color InfoBg          = Color.FromArgb(20, 56, 189, 248);

            // ── Button-specific ─────────────────────────────────────
            public static Color ButtonPrimary       => Accent;
            public static Color ButtonPrimaryHover  => AccentHover;
            public static Color ButtonSecondaryBg   => AppColors.IsDarkMode
                ? Color.FromArgb(30, 30, 50)
                : Color.FromArgb(241, 245, 249);
            public static Color ButtonSecondaryHover => AppColors.IsDarkMode
                ? Color.FromArgb(40, 40, 65)
                : Color.FromArgb(226, 232, 240);
            public static Color ButtonGhostHover    => AppColors.IsDarkMode
                ? Color.FromArgb(25, 255, 255, 255)
                : Color.FromArgb(25, 0, 0, 0);

            // ── Logout ──────────────────────────────────────────────
            public static readonly Color LogoutRed       = Color.FromArgb(220, 38, 38);
            public static readonly Color LogoutRedHover  = Color.FromArgb(239, 68, 68);

            // ── Sidebar ─────────────────────────────────────────────
            public static Color SidebarHover    => AppColors.IsDarkMode
                ? Color.FromArgb(30, 30, 50)
                : Color.FromArgb(226, 232, 240);
            public static Color SidebarActive   => Accent;

            // ── Backward-compat aliases ─────────────────────────────
            public static Color Primary      => Accent;
            public static Color PrimaryDeep  => AccentPressed;
            public static Color PrimarySoft  => AccentSoft;
            public static Color Canvas       => FormBg;
            public static Color SurfaceSoft  => CardBg;
            public static Color HairlineSoft => Border;
            public static Color Hairline     => BorderSoft;
            public static Color InkDeep      => TextPrimary;
            public static Color Ink          => TextPrimary;
            public static Color Charcoal     => TextSecondary;
            public static Color Slate        => TextMuted;
            public static Color Steel        => TextMuted;
            public static Color Stone        => TextDisabled;
            public static Color InkButton    => Accent;
            public static Color OnInkButton  => TextPrimary;
            public static Color OnPrimary    => TextPrimary;
            public static Color FbBlue       => Accent;
            public static Color ActivePill   => SidebarActive;
        }

        // ═══════════════════════════════════════════════════════════════
        //  TYPOGRAPHY
        // ═══════════════════════════════════════════════════════════════

        public static class Fonts
        {
            private static readonly string[] FamilyChain = { "Inter", "Segoe UI", "Helvetica", "Arial", "Noto Sans" };

            private static string ResolvedFamily
            {
                get
                {
                    foreach (var name in FamilyChain)
                    {
                        try
                        {
                            using var test = new Font(name, 10f);
                            if (string.Equals(test.Name, name, StringComparison.OrdinalIgnoreCase))
                                return name;
                        }
                        catch { /* skip */ }
                    }
                    return SystemFonts.MessageBoxFont.FontFamily.Name;
                }
            }

            private static string? _cached;
            private static string Family => _cached ??= ResolvedFamily;
            private static string EmphasisFamily => string.Equals(Family, "Segoe UI", StringComparison.OrdinalIgnoreCase)
                ? "Segoe UI Semibold"
                : Family;

            // ── Heading hierarchy ───────────────────────────────────
            public static Font HeroDisplay()  => new Font(EmphasisFamily, 28f, FontStyle.Regular);
            public static Font DisplayLg()    => new Font(EmphasisFamily, 22f, FontStyle.Regular);
            public static Font HeadingLg()    => new Font(EmphasisFamily, 18f, FontStyle.Regular);
            public static Font HeadingMd()    => new Font(EmphasisFamily, 14f, FontStyle.Regular);
            public static Font HeadingSm()    => new Font(EmphasisFamily, 12f, FontStyle.Regular);

            // ── Body + Subtitle ─────────────────────────────────────
            public static Font SubtitleLg()   => new Font(EmphasisFamily, 11f, FontStyle.Regular);
            public static Font SubtitleMd()   => new Font(Family, 11f, FontStyle.Regular);
            public static Font BodyMd()       => new Font(Family, 10f, FontStyle.Regular);
            public static Font BodyMdBold()   => new Font(EmphasisFamily, 10f, FontStyle.Regular);
            public static Font BodySm()       => new Font(Family, 9f,  FontStyle.Regular);
            public static Font BodySmBold()   => new Font(EmphasisFamily, 9f,  FontStyle.Regular);

            // ── Micro ───────────────────────────────────────────────
            public static Font CaptionBold()  => new Font(EmphasisFamily, 8f,  FontStyle.Regular);
            public static Font Caption()      => new Font(Family, 8f,  FontStyle.Regular);
            public static Font ButtonMd()     => new Font(EmphasisFamily, 10f, FontStyle.Regular);
        }

        // ═══════════════════════════════════════════════════════════════
        //  SPACING (px)
        // ═══════════════════════════════════════════════════════════════

        public static class Spacing
        {
            public const int XXS       = 4;
            public const int XS        = 8;
            public const int SM        = 10;
            public const int MD        = 12;
            public const int Base      = 16;
            public const int LG        = 20;
            public const int XL        = 24;
            public const int XXL       = 32;
            public const int XXXL      = 40;
            public const int SectionSm = 48;
            public const int Section   = 64;
            public const int SectionLg = 80;
            public const int Hero      = 120;
        }

        // ═══════════════════════════════════════════════════════════════
        //  BORDER RADIUS
        // ═══════════════════════════════════════════════════════════════

        public static class Radius
        {
            public const int XS      = 2;
            public const int SM      = 4;
            public const int MD      = 6;
            public const int LG      = 8;
            public const int XL      = 12;
            public const int XXL     = 16;
            public const int XXXL    = 20;
            public const int Feature = 24;
            public const int Full    = 100;   // Pill buttons
        }

        // ═══════════════════════════════════════════════════════════════
        //  COMPONENT HELPERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Style a DataGridView — dark header, alternating dark rows, no harsh borders.
        /// </summary>
        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Colors.CardBg;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Colors.BorderSoft;
            grid.EnableHeadersVisualStyles = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AllowUserToAddRows = false;

            // Header — deep dark with indigo tint
            grid.ColumnHeadersHeight = 44;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(18, 18, 32);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Colors.TextSecondary;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(18, 18, 32);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Colors.TextSecondary;
            grid.ColumnHeadersDefaultCellStyle.Font = Fonts.BodySmBold();
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(Spacing.SM, 0, Spacing.SM, 0);

            // Rows
            grid.DefaultCellStyle.Font = Fonts.BodyMd();
            grid.DefaultCellStyle.ForeColor = Colors.TextPrimary;
            grid.DefaultCellStyle.BackColor = Colors.CardBg;
            grid.DefaultCellStyle.SelectionBackColor = Colors.AccentMuted;
            grid.DefaultCellStyle.SelectionForeColor = Colors.TextPrimary;
            grid.DefaultCellStyle.Padding = new Padding(Spacing.SM, Spacing.XXS, Spacing.SM, Spacing.XXS);
            grid.RowTemplate.Height = 40;

            // Alternating
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(16, 16, 28);
        }

        /// <summary>
        /// Style a panel as a dark card with rounded corners and subtle border.
        /// </summary>
        public static void StyleCard(Panel panel, int radius = 12)
        {
            panel.BackColor = Colors.CardBg;
            panel.Paint -= PanelCardPaint;
            panel.Paint += PanelCardPaint;
            ApplyRoundedRegion(panel, radius);
            panel.Tag = radius;
            panel.Resize -= PanelResizeHandler;
            panel.Resize += PanelResizeHandler;
        }

        /// <summary>
        /// Style a sidebar button — dark inactive state with muted text.
        /// </summary>
        public static void StyleSidebarButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = Colors.TextSecondary;
            btn.Font = Fonts.BodyMd();
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(Spacing.Base, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Colors.SidebarHover;
        }

        /// <summary>
        /// Set sidebar button active — indigo background pill.
        /// </summary>
        public static void SetSidebarActive(Button btn)
        {
            btn.BackColor = Colors.SidebarActive;
            btn.ForeColor = Colors.TextPrimary;
            btn.Font = Fonts.BodyMdBold();
        }

        /// <summary>
        /// Reset sidebar button to inactive.
        /// </summary>
        public static void SetSidebarInactive(Button btn)
        {
            btn.BackColor = Color.Transparent;
            btn.ForeColor = Colors.TextSecondary;
            btn.Font = Fonts.BodyMd();
        }

        /// <summary>
        /// Style logout button — red pill.
        /// </summary>
        public static void StyleLogoutButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Colors.LogoutRed;
            btn.ForeColor = Colors.TextPrimary;
            btn.Font = Fonts.ButtonMd();
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Colors.LogoutRedHover;
        }

        /// <summary>
        /// Style a primary indigo CTA button.
        /// </summary>
        public static void StylePrimaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Colors.ButtonPrimary;
            btn.ForeColor = Colors.TextPrimary;
            btn.Font = Fonts.ButtonMd();
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Colors.ButtonPrimaryHover;
        }

        /// <summary>
        /// Style a dark marketing CTA button (same as primary in dark mode).
        /// </summary>
        public static void StyleDarkButton(Button btn)
        {
            StylePrimaryButton(btn);
        }

        /// <summary>
        /// Style a secondary (ghost/outlined) button — subtle border on dark bg.
        /// </summary>
        public static void StyleSecondaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Colors.Border;
            btn.BackColor = Colors.ButtonSecondaryBg;
            btn.ForeColor = Colors.TextPrimary;
            btn.Font = Fonts.ButtonMd();
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Colors.ButtonSecondaryHover;
        }

        /// <summary>
        /// Style a ghost button — transparent with subtle border.
        /// </summary>
        public static void StyleGhostButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Colors.Border;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = Colors.TextSecondary;
            btn.Font = Fonts.ButtonMd();
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Colors.ButtonGhostHover;
        }

        /// <summary>
        /// Style a Form with the dark background.
        /// </summary>
        public static void StyleForm(Form form)
        {
            form.BackColor = Colors.FormBg;
            form.ForeColor = Colors.TextPrimary;
            form.Font = Fonts.BodyMd();
        }

        /// <summary>
        /// Style a TextBox — dark background, white text, subtle border.
        /// </summary>
        public static void StyleTextInput(TextBox textBox)
        {
            textBox.Font = Fonts.BodyMd();
            textBox.ForeColor = Colors.TextPrimary;
            textBox.BackColor = Colors.InputBg;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        /// <summary>
        /// Style a ComboBox — dark background, white text.
        /// </summary>
        public static void StyleComboBox(ComboBox combo)
        {
            combo.Font = Fonts.BodyMd();
            combo.ForeColor = Colors.TextPrimary;
            combo.BackColor = Colors.InputBg;
            combo.FlatStyle = FlatStyle.Flat;

            // Fix WinForms DropDownList white background issue
            combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.DrawItem -= Combo_DrawItem;
            combo.DrawItem += Combo_DrawItem;
        }

        private static void Combo_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not ComboBox cb) return;

            e.DrawBackground();

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = isSelected ? Colors.AccentMuted : Colors.InputBg;
            Color fg = Colors.TextPrimary;

            using (SolidBrush brush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            if (e.Index >= 0)
            {
                string text = cb.Items[e.Index].ToString() ?? "";
                
                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near
                };

                using (SolidBrush textBrush = new SolidBrush(fg))
                {
                    Rectangle textRect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height);
                    e.Graphics.DrawString(text, e.Font ?? cb.Font, textBrush, textRect, sf);
                }
            }

            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Recursively apply dark theme to all controls in a container.
        /// Useful for UserControls that don't manually style each child.
        /// </summary>
        public static void ApplyDarkRecursive(Control root)
        {
            if (root is UserControl || root is Panel)
            {
                if (root.BackColor == SystemColors.Control || root.BackColor.GetBrightness() > 0.8f)
                {
                    root.BackColor = root is UserControl ? Colors.FormBg : Colors.CardBg;
                }
            }

            foreach (Control c in root.Controls)
            {
                if (c is Label lbl)
                {
                    if (lbl.Font.Size >= 12f)
                        lbl.ForeColor = Colors.TextPrimary;
                    else
                        lbl.ForeColor = lbl.Font.Bold ? Colors.TextPrimary : Colors.TextSecondary;
                }
                else if (c is TextBox tb)
                {
                    StyleTextInput(tb);
                }
                else if (c is ComboBox cb)
                {
                    StyleComboBox(cb);
                }
                else if (c is DataGridView dgv)
                {
                    StyleGrid(dgv);
                }
                else if (c is ListBox lb)
                {
                    lb.BackColor = Colors.InputBg;
                    lb.ForeColor = Colors.TextPrimary;
                    lb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is RichTextBox rtb)
                {
                    rtb.BackColor = Colors.InputBg;
                    rtb.ForeColor = Colors.TextPrimary;
                    rtb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is NumericUpDown nud)
                {
                    nud.BackColor = Colors.InputBg;
                    nud.ForeColor = Colors.TextPrimary;
                }
                else if (c is DateTimePicker dtp)
                {
                    // Basic styling for dtp calendar
                    dtp.CalendarMonthBackground = Colors.InputBg;
                    dtp.CalendarForeColor = Colors.TextPrimary;
                    dtp.CalendarTitleBackColor = Colors.SidebarBg;
                    dtp.CalendarTitleForeColor = Colors.TextPrimary;
                    dtp.CalendarTrailingForeColor = Colors.TextMuted;
                }
                else if (c is CheckBox chk)
                {
                    chk.ForeColor = Colors.TextPrimary;
                }
                else if (c is GroupBox gb)
                {
                    gb.BackColor = Colors.CardBg;
                    gb.ForeColor = Colors.TextPrimary;
                }
                else if (c is Panel pnl && !(c is FlowLayoutPanel) && !(c is TableLayoutPanel))
                {
                    if (pnl.BackColor == SystemColors.Control || pnl.BackColor.GetBrightness() > 0.8f)
                    {
                        pnl.BackColor = Colors.CardBg;
                    }
                }

                if (c.HasChildren)
                {
                    ApplyDarkRecursive(c);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  INTERNAL HELPERS
        // ═══════════════════════════════════════════════════════════════

        private static void PanelResizeHandler(object? sender, EventArgs e)
        {
            if (sender is Panel panel)
            {
                int r = panel.Tag is int rad ? rad : Radius.XL;
                ApplyRoundedRegion(panel, r);
            }
        }

        private static void ApplyRoundedRegion(Panel panel, int radius)
        {
            if (panel.Width <= 0 || panel.Height <= 0) return;
            using GraphicsPath path = CreateRoundedRect(new Rectangle(0, 0, panel.Width, panel.Height), radius);
            panel.Region = new Region(path);
        }

        private static void PanelCardPaint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;
            int r = panel.Tag is int rad ? rad : Radius.XL;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen p = new Pen(Colors.Border, 1f);
            using GraphicsPath path = CreateRoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), r);
            e.Graphics.DrawPath(p, path);
        }

        public static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
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
        /// <summary>
        /// Shows a modern dark dialog to replace standard MessageBox.
        /// </summary>
        public static void ShowModernDialog(string content, string title = "Thông báo")
        {
            Form dialog = new Form
            {
                Text = title,
                Size = new Size(450, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Colors.FormBg,
                ForeColor = Colors.TextPrimary,
                Font = Fonts.BodyMd()
            };

            Panel header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Colors.SidebarBg };
            Label lblTitle = new Label 
            { 
                Text = title, 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleLeft, 
                Padding = new Padding(20, 0, 0, 0),
                Font = Fonts.HeadingMd(),
                ForeColor = Colors.TextPrimary
            };
            header.Controls.Add(lblTitle);

            Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            Label lblContent = new Label 
            { 
                Text = content, 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.TopLeft,
                Font = Fonts.BodyMd(),
                ForeColor = Colors.TextSecondary
            };
            body.Controls.Add(lblContent);

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
            Button btnOk = new Button 
            { 
                Text = "Đồng ý", 
                DialogResult = DialogResult.OK, 
                Dock = DockStyle.Right, 
                Width = 100
            };
            StylePrimaryButton(btnOk);
            footer.Controls.Add(btnOk);

            dialog.Controls.Add(body);
            dialog.Controls.Add(header);
            dialog.Controls.Add(footer);

            dialog.ShowDialog();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  BACKWARD-COMPAT REDIRECTS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Thin redirect layer — existing code referencing AcademicTheme now gets
    /// dark modern tokens from MetaTheme.
    /// </summary>
    internal static class AcademicTheme
    {
        public static Color AppBackground  => MetaTheme.Colors.FormBg;
        public static Color Surface        => MetaTheme.Colors.CardBg;
        public static Color SurfaceLow     => MetaTheme.Colors.ElevatedBg;
        public static Color Primary        => MetaTheme.Colors.Accent;
        public static Color PrimaryStrong  => MetaTheme.Colors.AccentPressed;
        public static Color TextPrimary    => MetaTheme.Colors.TextPrimary;
        public static Color TextSecondary  => MetaTheme.Colors.TextSecondary;
        public static Color BorderSoft     => MetaTheme.Colors.Border;

        public static void StyleCard(Panel panel, int radius = 12) => MetaTheme.StyleCard(panel, radius);
        public static void StyleGrid(DataGridView grid) => MetaTheme.StyleGrid(grid);
        public static void ShowModernDialog(string content, string title = "Thông báo") => MetaTheme.ShowModernDialog(content, title);
    }
}
