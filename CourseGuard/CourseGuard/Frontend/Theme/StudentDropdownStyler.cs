using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    public static class StudentDropdownStyler
    {
        private const int MenuRadius = 8;
        private const int MenuMinWidth = 230;
        private const int MenuItemHeight = 36;
        private const int ComboItemHeight = 34;
        private const int ComboRadius = 10;
        private const int ComboHeight = 36;
        private static readonly ConditionalWeakTable<ComboBox, ComboPopupState> ComboStates = new();

        public static void Apply(ContextMenuStrip menu, int minimumWidth = MenuMinWidth)
        {
            if (menu.IsDisposed)
                return;

            menu.ShowImageMargin = false;
            menu.ShowCheckMargin = false;
            menu.Font = AppFonts.Body;
            menu.Padding = new Padding(5);
            menu.MinimumSize = new Size(Math.Max(MenuMinWidth, minimumWidth), 0);
            menu.BackColor = MenuBackColor;
            menu.ForeColor = AppColors.TextPrimary;
            menu.Cursor = Cursors.Hand;
            menu.Renderer = StudentDropdownRenderer.Instance;
            menu.Opened -= Menu_Opened;
            menu.Opened += Menu_Opened;

            int itemMinWidth = Math.Max(MenuMinWidth, minimumWidth);
            foreach (ToolStripItem item in menu.Items)
                StyleItem(item, itemMinWidth);
        }

        public static void StyleComboBox(ComboBox combo, bool? useCustomPopup = null)
        {
            ComboPopupState state = ComboStates.GetOrCreateValue(combo);
            if (useCustomPopup.HasValue)
                state.UseCustomPopup = useCustomPopup.Value;

            combo.FlatStyle = FlatStyle.Flat;
            combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.IntegralHeight = false;
            combo.ItemHeight = ComboItemHeight;
            combo.Height = Math.Max(combo.Height, ComboHeight);
            combo.MinimumSize = new Size(Math.Max(combo.MinimumSize.Width, 160), ComboHeight);
            combo.DropDownHeight = state.UseCustomPopup ? 1 : 250;
            combo.Font = AppFonts.Body;
            combo.BackColor = AppColors.BgInput;
            combo.ForeColor = AppColors.TextPrimary;
            combo.Cursor = Cursors.Hand;

            combo.DrawItem -= Combo_DrawItem;
            combo.DrawItem += Combo_DrawItem;
            combo.DropDown -= Combo_DropDown;
            combo.DropDown += Combo_DropDown;
            combo.KeyDown -= Combo_KeyDown;
            combo.MouseDown -= Combo_MouseDown;
            combo.SizeChanged -= Combo_SizeChanged;
            combo.SizeChanged += Combo_SizeChanged;

            if (state.UseCustomPopup)
            {
                combo.KeyDown += Combo_KeyDown;
                combo.MouseDown += Combo_MouseDown;
                state.Attach(combo);
            }

            ApplyComboRegion(combo);
        }

        private static Color MenuBackColor => AppColors.IsDarkMode
            ? ColorTranslator.FromHtml("#1B1B1F")
            : AppColors.BgCard;

        private static Color HoverBackColor => AppColors.AccentBlue;

        private static void StyleItem(ToolStripItem item, int minimumWidth)
        {
            if (item is ToolStripSeparator separator)
            {
                separator.AutoSize = false;
                separator.Height = 12;
                return;
            }

            item.BackColor = MenuBackColor;
            item.ForeColor = AppColors.TextPrimary;
            item.AutoSize = false;
            item.Padding = new Padding(18, 0, 18, 0);
            item.Margin = new Padding(0, 2, 0, 2);
            item.Size = new Size(Math.Max(minimumWidth, TextRenderer.MeasureText(item.Text, AppFonts.Body).Width + 56), MenuItemHeight);

            if (item is ToolStripMenuItem menuItem)
            {
                menuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
                menuItem.DropDown.BackColor = MenuBackColor;
                menuItem.DropDown.ForeColor = AppColors.TextPrimary;
                if (menuItem.DropDown is ContextMenuStrip childMenu)
                    Apply(childMenu);
            }
        }

        private static void Menu_Opened(object? sender, System.EventArgs e)
        {
            if (sender is not ContextMenuStrip menu || menu.IsDisposed || menu.Width <= 0 || menu.Height <= 0)
                return;

            menu.BackColor = MenuBackColor;
            menu.ForeColor = AppColors.TextPrimary;
            menu.Cursor = Cursors.Hand;

            menu.Region?.Dispose();
            using var path = GraphicsHelpers.RoundedRect(new Rectangle(0, 0, menu.Width, menu.Height), MenuRadius);
            menu.Region = new Region(path);
        }

        private static void Combo_DropDown(object? sender, System.EventArgs e)
        {
            if (sender is not ComboBox combo)
                return;

            ComboPopupState state = ComboStates.GetOrCreateValue(combo);
            if (!state.UseCustomPopup)
            {
                StyleComboBox(combo);
                return;
            }

            combo.BeginInvoke(new MethodInvoker(() =>
            {
                combo.DroppedDown = false;
                ShowComboPopup(combo);
            }));
        }

        private static void Combo_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is ComboBox combo && e.Button == MouseButtons.Left)
                ShowComboPopup(combo);
        }

        private static void Combo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not ComboBox combo)
                return;

            bool shouldOpen = e.KeyCode is Keys.Enter or Keys.Space or Keys.Down
                || (e.Alt && e.KeyCode == Keys.Down)
                || e.KeyCode == Keys.F4;

            if (!shouldOpen)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            ShowComboPopup(combo);
        }

        private static void Combo_SizeChanged(object? sender, System.EventArgs e)
        {
            if (sender is ComboBox combo)
                ApplyComboRegion(combo);
        }

        private static void ApplyComboRegion(ComboBox combo)
        {
            if (combo.Width <= 0 || combo.Height <= 0)
                return;

            combo.Region?.Dispose();
            using var path = GraphicsHelpers.RoundedRect(new Rectangle(0, 0, combo.Width, combo.Height), ComboRadius);
            combo.Region = new Region(path);
            combo.Invalidate();
        }

        private static void ShowComboPopup(ComboBox combo)
        {
            ComboPopupState state = ComboStates.GetOrCreateValue(combo);
            if (!state.UseCustomPopup || state.IsOpen || combo.IsDisposed)
                return;

            state.IsOpen = true;
            var menu = new ContextMenuStrip();

            if (combo.Items.Count == 0)
            {
                var emptyItem = new ToolStripMenuItem("Không có lựa chọn")
                {
                    Enabled = false
                };
                menu.Items.Add(emptyItem);
            }
            else
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    int selectedIndex = i;
                    string text = combo.GetItemText(combo.Items[i]) ?? string.Empty;
                    var item = new ToolStripMenuItem(text)
                    {
                        Tag = selectedIndex
                    };
                    item.Click += (_, _) =>
                    {
                        combo.SelectedIndex = selectedIndex;
                        combo.Focus();
                        combo.Invalidate();
                    };
                    menu.Items.Add(item);
                }
            }

            Apply(menu, combo.Width);
            menu.Closed += (_, _) =>
            {
                state.IsOpen = false;
            };

            if (!combo.IsDisposed && combo.IsHandleCreated && !menu.IsDisposed)
                menu.Show(combo, new Point(0, combo.Height + 4));
            else
                state.IsOpen = false;
        }

        private static void Combo_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not ComboBox combo)
                return;

            bool isEditArea = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
            bool selected = !isEditArea && (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = isEditArea ? AppColors.BgInput : selected ? HoverBackColor : MenuBackColor;
            Color fg = selected ? Color.White : AppColors.TextPrimary;

            using (SolidBrush bgBrush = new SolidBrush(bg))
                e.Graphics.FillRectangle(bgBrush, e.Bounds);

            if (e.Index >= 0)
            {
                string text = combo.GetItemText(combo.Items[e.Index]) ?? string.Empty;
                TextRenderer.DrawText(
                    e.Graphics,
                    text,
                    AppFonts.Body,
                    new Rectangle(e.Bounds.Left + 14, e.Bounds.Top, e.Bounds.Width - 24, e.Bounds.Height),
                    fg,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private sealed class ComboPopupState
        {
            private ComboBoxPainter? _painter;

            public bool UseCustomPopup { get; set; }
            public bool IsOpen { get; set; }

            public void Attach(ComboBox combo)
            {
                _painter ??= new ComboBoxPainter();
                _painter.Attach(combo);
            }
        }

        private sealed class ComboBoxPainter : NativeWindow
        {
            private const int WmPaint = 0x000F;
            private const int WmNcPaint = 0x0085;
            private ComboBox? _combo;

            public void Attach(ComboBox combo)
            {
                if (_combo == combo && Handle == combo.Handle)
                    return;

                ReleaseHandle();
                _combo = combo;

                if (combo.IsHandleCreated)
                    AssignHandle(combo.Handle);

                combo.HandleCreated -= Combo_HandleCreated;
                combo.HandleDestroyed -= Combo_HandleDestroyed;
                combo.HandleCreated += Combo_HandleCreated;
                combo.HandleDestroyed += Combo_HandleDestroyed;
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if ((m.Msg == WmPaint || m.Msg == WmNcPaint) && _combo != null && !_combo.IsDisposed)
                    PaintComboChrome(_combo);
            }

            private void Combo_HandleCreated(object? sender, System.EventArgs e)
            {
                if (sender is ComboBox combo)
                    AssignHandle(combo.Handle);
            }

            private void Combo_HandleDestroyed(object? sender, System.EventArgs e)
            {
                ReleaseHandle();
            }

            private static void PaintComboChrome(ComboBox combo)
            {
                using Graphics g = Graphics.FromHwnd(combo.Handle);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle fullRect = new Rectangle(0, 0, combo.Width - 1, combo.Height - 1);
                Rectangle fillRect = new Rectangle(1, 1, Math.Max(1, combo.Width - 2), Math.Max(1, combo.Height - 2));
                GraphicsHelpers.FillRoundedRect(g, fillRect, ComboRadius - 1, AppColors.BgInput);

                string text = combo.SelectedIndex >= 0
                    ? combo.GetItemText(combo.SelectedItem) ?? string.Empty
                    : combo.Text;
                TextRenderer.DrawText(
                    g,
                    text,
                    AppFonts.Body,
                    new Rectangle(14, 1, Math.Max(1, combo.Width - 48), Math.Max(1, combo.Height - 2)),
                    AppColors.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                Point center = new Point(combo.Width - 15, combo.Height / 2);
                using Pen pen = new Pen(AppColors.TextSecondary, 1.8f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };
                g.DrawLine(pen, center.X - 4, center.Y - 2, center.X, center.Y + 2);
                g.DrawLine(pen, center.X, center.Y + 2, center.X + 4, center.Y - 2);

                GraphicsHelpers.DrawRoundedBorder(g, fullRect, ComboRadius, AppColors.BorderStrong, 1f);
            }
        }

        private sealed class StudentDropdownRenderer : ToolStripProfessionalRenderer
        {
            public static readonly StudentDropdownRenderer Instance = new();

            private StudentDropdownRenderer() : base(new StudentDropdownColorTable())
            {
                RoundedEdges = true;
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using Pen pen = new Pen(AppColors.BorderStrong);
                Rectangle rect = new Rectangle(Point.Empty, new Size(e.ToolStrip.Width - 1, e.ToolStrip.Height - 1));
                GraphicsHelpers.DrawRoundedBorder(e.Graphics, rect, MenuRadius, AppColors.BorderStrong, 1f);
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item == null)
                    return;

                Rectangle rect = new Rectangle(5, 2, e.Item.Width - 10, e.Item.Height - 4);
                Color bg = e.Item.Selected ? HoverBackColor : MenuBackColor;
                GraphicsHelpers.FillRoundedRect(e.Graphics, rect, 6, bg);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                using Pen pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 8, e.Item.Height / 2, e.Item.Width - 8, e.Item.Height / 2);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = e.Item?.Selected == true ? Color.White : AppColors.TextSecondary;
                base.OnRenderArrow(e);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (e.Item == null)
                    return;

                Color textColor = e.Item.Selected ? Color.White : AppColors.TextPrimary;
                Rectangle textRect = new Rectangle(20, 0, e.Item.Width - 40, e.Item.Height);
                TextRenderer.DrawText(
                    e.Graphics,
                    e.Text,
                    AppFonts.Body,
                    textRect,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private sealed class StudentDropdownColorTable : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground => MenuBackColor;
            public override Color ImageMarginGradientBegin => MenuBackColor;
            public override Color ImageMarginGradientMiddle => MenuBackColor;
            public override Color ImageMarginGradientEnd => MenuBackColor;
            public override Color MenuBorder => AppColors.BorderStrong;
            public override Color MenuItemBorder => HoverBackColor;
            public override Color MenuItemSelected => HoverBackColor;
            public override Color MenuItemSelectedGradientBegin => HoverBackColor;
            public override Color MenuItemSelectedGradientEnd => HoverBackColor;
            public override Color MenuItemPressedGradientBegin => HoverBackColor;
            public override Color MenuItemPressedGradientEnd => HoverBackColor;
            public override Color SeparatorDark => AppColors.Border;
            public override Color SeparatorLight => AppColors.Border;
        }
    }
}
