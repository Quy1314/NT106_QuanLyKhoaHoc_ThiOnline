using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    internal static class TeacherTabChrome
    {
        private const int HeaderHeight = 112;
        private const int ButtonHeight = 36;
        private const int ButtonRadius = 10;

        public static TableLayoutPanel CreateRoot(UserControl page)
        {
            page.SuspendLayout();
            page.Controls.Clear();
            page.BackColor = AppColors.BgBase;
            page.Padding = Padding.Empty;
            page.TabStop = false;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgBase,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(24)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, HeaderHeight));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            page.Controls.Add(root);
            EnableNaturalFocusClear(page);
            page.ResumeLayout(true);
            return root;
        }

        public static RoundedPanel CreateHeader(string title, string subtitle, params Control[] actions)
        {
            var card = CreateCard();
            card.Padding = new Padding(20, 16, 20, 16);
            card.Margin = new Padding(0, 0, 0, 16);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var textStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2
            };
            textStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            textStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            textStack.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = AppFonts.Semibold(16f),
                ForeColor = AppColors.TextPrimary,
                Padding = new Padding(0, 0, 0, 3),
                Text = title,
                TextAlign = ContentAlignment.MiddleLeft,
                UseCompatibleTextRendering = false
            }, 0, 0);
            textStack.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = AppFonts.Body,
                ForeColor = AppColors.TextSecondary,
                Text = subtitle,
                TextAlign = ContentAlignment.MiddleLeft,
                UseCompatibleTextRendering = false
            }, 0, 1);

            var actionPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = new Padding(0, 9, 0, 0)
            };
            foreach (Control action in actions)
            {
                action.Margin = action is Label ? new Padding(8, 7, 0, 0) : new Padding(8, 0, 0, 0);
                actionPanel.Controls.Add(action);
            }

            grid.Controls.Add(textStack, 0, 0);
            grid.Controls.Add(actionPanel, 1, 0);
            card.Controls.Add(grid);
            return card;
        }

        public static RoundedPanel CreateDataCard(string title, Control content)
        {
            var card = CreateCard();
            card.Padding = new Padding(18);
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2
            };
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            grid.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = AppFonts.Semibold(12f),
                ForeColor = AppColors.TextPrimary,
                Padding = new Padding(0, 0, 0, 3),
                Text = title,
                TextAlign = ContentAlignment.MiddleLeft,
                UseCompatibleTextRendering = false
            }, 0, 0);
            content.Dock = DockStyle.Fill;
            content.Margin = Padding.Empty;
            grid.Controls.Add(content, 0, 1);
            card.Controls.Add(grid);
            return card;
        }

        public static RoundedPanel CreateTableBody(DataGridView grid, out Label emptyLabel)
        {
            var body = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                FillColor = AppColors.BgCard,
                BorderColor = Color.Transparent,
                CornerRadius = 12,
                Padding = Padding.Empty
            };

            emptyLabel = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = Color.Transparent,
                ForeColor = AppColors.TextMuted,
                Font = AppFonts.Body,
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = false,
                Visible = false
            };

            grid.Dock = DockStyle.Fill;
            grid.Margin = Padding.Empty;
            body.Controls.Add(grid);
            body.Controls.Add(emptyLabel);
            return body;
        }

        public static void SetTableState(RoundedPanel body, DataGridView grid, Label emptyLabel, bool showTable, string emptyMessage)
        {
            Color emptyFill = AppColors.IsDarkMode ? AppColors.BgCardHover : ColorTranslator.FromHtml("#F8FAFC");
            body.Padding = showTable ? Padding.Empty : new Padding(18);
            body.FillColor = showTable ? AppColors.BgCard : emptyFill;
            body.BorderColor = showTable ? Color.Transparent : AppColors.Border;
            body.BackColor = AppColors.BgCard;
            grid.Visible = showTable;
            emptyLabel.Text = emptyMessage;
            emptyLabel.BackColor = emptyFill;
            emptyLabel.ForeColor = AppColors.TextMuted;
            emptyLabel.Visible = !showTable;
            if (!showTable)
                emptyLabel.BringToFront();
            body.Invalidate();
        }

        public static RoundedPanel CreateCard()
        {
            return new RoundedPanel
            {
                Dock = DockStyle.Fill,
                FillColor = AppColors.BgCard,
                BorderColor = AppColors.Border,
                CornerRadius = 16,
                Margin = Padding.Empty
            };
        }

        public static Button PrimaryButton(string text)
        {
            var button = new Button { Text = text };
            StylePrimaryButton(button);
            return button;
        }

        public static Button SecondaryButton(string text)
        {
            var button = new Button { Text = text };
            StyleSecondaryButton(button);
            return button;
        }

        public static Button DangerButton(string text)
        {
            var button = new Button { Text = text };
            StyleDangerButton(button);
            return button;
        }

        public static void StylePrimaryButton(Button button)
        {
            MetaTheme.StylePrimaryButton(button);
            PrepareButton(button);
            button.Tag = "primary";
            button.UseVisualStyleBackColor = false;
            button.BackColor = AppColors.AccentBlue;
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = AppColors.AccentHover;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = AppColors.AccentHover;
            button.FlatAppearance.MouseDownBackColor = AppColors.AccentPressed;
            RoundedButtonHelper.Apply(button, ButtonRadius);
        }

        public static void StyleSecondaryButton(Button button)
        {
            MetaTheme.StyleSecondaryButton(button);
            PrepareButton(button);
            button.Tag = "secondary";
            button.UseVisualStyleBackColor = false;
            button.BackColor = AppColors.IsDarkMode ? AppColors.BgCardHover : ColorTranslator.FromHtml("#F8FAFC");
            button.ForeColor = AppColors.TextPrimary;
            button.FlatAppearance.BorderColor = AppColors.BorderStrong;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = AppColors.IsDarkMode ? AppColors.BgElevated : ColorTranslator.FromHtml("#EEF2F7");
            button.FlatAppearance.MouseDownBackColor = AppColors.BgElevated;
            RoundedButtonHelper.Apply(button, ButtonRadius);
        }

        public static void StyleDangerButton(Button button)
        {
            PrepareButton(button);
            button.Tag = "danger";
            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = AppColors.Danger;
            button.ForeColor = Color.White;
            button.Font = AppFonts.Button;
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(185, 28, 28);
            RoundedButtonHelper.Apply(button, ButtonRadius);
        }

        public static void FitButtonToText(Button button)
        {
            int desiredWidth = TextRenderer.MeasureText(button.Text, button.Font).Width + 30;
            button.Width = Math.Max(button.MinimumSize.Width, desiredWidth);
        }

        public static void EnableNaturalFocusClear(UserControl page, params DataGridView[] gridsToClear)
        {
            void Clear()
            {
                page.ActiveControl = null;
                SearchFocusManager.BlurFocusedSearchInput(page.FindForm());

                foreach (DataGridView grid in gridsToClear)
                {
                    if (grid.IsDisposed)
                        continue;

                    grid.ClearSelection();
                    grid.CurrentCell = null;
                }
            }

            void Attach(Control control)
            {
                if (control is TextBox || control is ComboBox || control is Button || control is DataGridView || control is ListBox)
                    return;

                control.MouseDown += (_, _) => Clear();

                foreach (Control child in control.Controls)
                    Attach(child);
            }

            Attach(page);
        }

        private static void PrepareButton(Button button)
        {
            button.AutoSize = false;
            button.Height = ButtonHeight;
            button.MinimumSize = new Size(84, ButtonHeight);
            button.Padding = new Padding(14, 0, 14, 1);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = AppFonts.Button;
            FitButtonToText(button);
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = AppColors.BgCard;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.AllowUserToResizeRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            MetaTheme.StyleGrid(grid);
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 42;
            grid.RowTemplate.Height = 40;
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = AppColors.IsDarkMode ? AppColors.BgCard : ColorTranslator.FromHtml("#F8FAFC");
            grid.GridColor = AppColors.Border;
        }

        public static DataTable ToTable<T>(string[] columns, System.Collections.Generic.IEnumerable<T> rows, Func<T, object?[]> map)
        {
            var table = new DataTable();
            foreach (string column in columns)
                table.Columns.Add(column, typeof(object));
            foreach (T row in rows)
                table.Rows.Add(map(row));
            return table;
        }
    }
}
