/*
 * TopbarPanel.cs
 *
 * Layer: Presentation (Theme)
 * Top navigation bar with logo, page title, and user profile actions.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CourseGuard.Frontend.Theme
{
    public class TopbarPanel : UserControl
    {
        private string _pageTitle = "Tổng quan";
        private string _subtitle = "";
        private string _userName = "User";
        private bool _hasNotification = true;
        private bool _useStudentTopbar;
        private string _quickSearchPlaceholder = "Tìm khóa học, bài kiểm tra, tài liệu...";
        private int _openExamCount = 0;
        private bool _isOnline = true;
        private int _notificationCount = 0;
        
        private Rectangle _themeRect;
        private Rectangle _notificationRect;
        private Rectangle _accountRect;
        private Rectangle _searchRect;
        private bool _hoverTheme;
        private bool _hoverNotification;
        private bool _hoverAccount;
        private readonly TextBox _quickSearchBox;
        private ContextMenuStrip _notificationMenu;
        private ContextMenuStrip _accountMenu;
        private FloatingTopbarLabel? _floatingLabel;
        private string _floatingLabelText = string.Empty;

        public event EventHandler? LogoutRequested;

        public event EventHandler? ThemeToggled;

        [Category("Data")]
        [DefaultValue("Store Overview")]
        public string PageTitle { get => _pageTitle; set { _pageTitle = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue("Here's how your store is performing today.")]
        public string Subtitle { get => _subtitle; set { _subtitle = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue("Yanuar Arifin")]
        public string UserName { get => _userName; set { _userName = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue(true)]
        public bool HasNotification { get => _hasNotification; set { _hasNotification = value; Invalidate(); } }

        [Category("Layout")]
        [DefaultValue(false)]
        public bool UseStudentTopbar
        {
            get => _useStudentTopbar;
            set
            {
                _useStudentTopbar = value;
                Height = value ? 68 : 60;
                _quickSearchBox.Visible = value;
                LayoutSearchBox();
                Invalidate();
            }
        }

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string QuickSearchPlaceholder
        {
            get => _quickSearchPlaceholder;
            set { _quickSearchPlaceholder = value; _quickSearchBox.PlaceholderText = value; Invalidate(); }
        }

        [Category("Data")]
        [DefaultValue(0)]
        public int OpenExamCount { get => _openExamCount; set { _openExamCount = Math.Max(0, value); Invalidate(); } }

        [Category("Data")]
        [DefaultValue(true)]
        public bool IsOnline { get => _isOnline; set { _isOnline = value; Invalidate(); } }

        [Category("Data")]
        [DefaultValue(0)]
        public int NotificationCount { get => _notificationCount; set { _notificationCount = Math.Max(0, value); Invalidate(); } }

        public TopbarPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);
            
            DoubleBuffered = true;
            BackColor = AppColors.BgCard;
            Dock = DockStyle.Top;
            Height = 60;

            _quickSearchBox = new TextBox
            {
                Name = "txtTopbarSearch",
                BorderStyle = BorderStyle.None,
                PlaceholderText = _quickSearchPlaceholder,
                Visible = false,
                Font = AppFonts.Body,
                TabStop = false
            };
            SearchFocusManager.MarkSearchInput(_quickSearchBox);
            Controls.Add(_quickSearchBox);

            _notificationMenu = new ContextMenuStrip();
            _accountMenu = new ContextMenuStrip();
            BuildMenus();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? AppColors.BgBase);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.Clear(Parent?.BackColor ?? AppColors.BgBase);

            Rectangle moduleRect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
            using (GraphicsPath topbarPath = GraphicsHelpers.BottomRoundedRect(moduleRect, 16))
            {
                GraphicsHelpers.FillPath(g, topbarPath, AppColors.BgCard);
                GraphicsHelpers.DrawPathBorder(g, topbarPath, AppColors.Border, 1f);
            }

            LayoutSearchBox();

            if (_useStudentTopbar)
            {
                PaintStudentTopbar(g);
                return;
            }

            using (SolidBrush textPrimary = new SolidBrush(AppColors.TextPrimary))
            using (SolidBrush textSecondary = new SolidBrush(AppColors.TextSecondary))
            using (SolidBrush textMuted = new SolidBrush(AppColors.TextMuted))
            using (SolidBrush accentBrush = new SolidBrush(AppColors.AccentBlue))
            {
                StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                StringFormat sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

                // 2. Left side: Logo + Brand
                Rectangle logoRect = new Rectangle(20, 10, 40, 40);
                GraphicsHelpers.FillRoundedRect(g, logoRect, 8, AppColors.AccentBlue);
                using (Font logoFont = AppFonts.Semibold(16f))
                using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                {
                    g.DrawString("C", logoFont, whiteBrush, logoRect, sfCenter);
                }
                
                // Brand Name
                float brandW = g.MeasureString("CourseGuard", AppFonts.Button).Width;
                g.DrawString("CourseGuard", AppFonts.Button, textPrimary, new RectangleF(70, 10, brandW, 40), sfLeft);

                // 3. Center section: PageTitle + optional Subtitle
                using (Font titleFont = AppFonts.Semibold(13f))
                {
                    if (string.IsNullOrWhiteSpace(_subtitle))
                    {
                        g.DrawString(_pageTitle, titleFont, textPrimary, new RectangleF(180, 0, 320, Height), sfLeft);
                    }
                    else
                    {
                        g.DrawString(_pageTitle, titleFont, textPrimary, new PointF(180, 8));
                    }
                }
                if (!string.IsNullOrWhiteSpace(_subtitle))
                    g.DrawString(_subtitle, AppFonts.Caption, textSecondary, new PointF(184, 32));

                // 4. Right side: Avatar + Username + Settings + Bell
                int rx = Width - 20;

                // Avatar (32x32)
                rx -= 32;
                Rectangle avatarRect = new Rectangle(rx, 14, 32, 32);
                g.FillEllipse(accentBrush, avatarRect);
                
                // Initials
                string initials = "YA";
                if (!string.IsNullOrEmpty(_userName) && _userName.Contains(" "))
                {
                    var parts = _userName.Split(' ');
                    initials = $"{parts[0][0]}{parts[1][0]}";
                }
                else if (!string.IsNullOrEmpty(_userName) && _userName.Length >= 2) 
                {
                    initials = _userName.Substring(0, 2);
                }
                
                using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                using (Font initialFont = AppFonts.Semibold(9f))
                {
                    g.DrawString(initials.ToUpper(), initialFont, whiteBrush, avatarRect, sfCenter);
                }

                // Username
                rx -= 10; // padding
                float userW = g.MeasureString(_userName, AppFonts.Body).Width;
                rx -= (int)userW;
                g.DrawString(_userName, AppFonts.Body, textPrimary, new RectangleF(rx, 14, userW, 32), sfCenter);

                // Settings Icon
                rx -= 20; // padding
                rx -= 32; // settings width
                Rectangle settingsRect = new Rectangle(rx, 14, 32, 32);
                DrawGearIcon(g, settingsRect, AppColors.TextSecondary);

                // Bell Icon
                rx -= 10;
                rx -= 32;
                Rectangle bellRect = new Rectangle(rx, 14, 32, 32);
                DrawBellIcon(g, bellRect, AppColors.TextSecondary);

                // Notification Badge
                if (_hasNotification)
                {
                    using (SolidBrush dangerBrush = new SolidBrush(AppColors.Danger))
                    {
                        Rectangle badgeRect = new Rectangle(bellRect.Right - 10, bellRect.Top + 4, 8, 8);
                        g.FillEllipse(dangerBrush, badgeRect);
                    }
                }

                // Dark/Light Toggle Icon
                rx -= 10;
                rx -= 32;
                Rectangle themeRect = new Rectangle(rx, 14, 32, 32);
                _themeRect = themeRect;
                if (_hoverTheme)
                    GraphicsHelpers.FillRoundedRect(g, themeRect, 8, AppColors.BgCardHover);
                DrawThemeIcon(g, themeRect, AppColors.TextSecondary);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool nextHoverTheme = _themeRect.Contains(e.Location);
            bool nextHoverNotification = _notificationRect.Contains(e.Location);
            bool nextHoverAccount = _accountRect.Contains(e.Location);
            bool nextHoverSearch = _useStudentTopbar && _searchRect.Contains(e.Location);
            this.Cursor = (nextHoverTheme || nextHoverNotification || nextHoverAccount || nextHoverSearch) ? Cursors.Hand : Cursors.Default;

            if (nextHoverTheme != _hoverTheme || nextHoverNotification != _hoverNotification || nextHoverAccount != _hoverAccount)
            {
                _hoverTheme = nextHoverTheme;
                _hoverNotification = nextHoverNotification;
                _hoverAccount = nextHoverAccount;
                Invalidate();
            }

            UpdateHoverLabel(e.Location, nextHoverTheme, nextHoverNotification, nextHoverAccount, nextHoverSearch);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverTheme = false;
            _hoverNotification = false;
            _hoverAccount = false;
            this.Cursor = Cursors.Default;
            HideFloatingLabel();
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (_themeRect.Contains(e.Location))
            {
                HideFloatingLabel();
                AppColors.IsDarkMode = !AppColors.IsDarkMode;

                Form? parentForm = this.FindForm();
                if (parentForm != null)
                {
                    // Suppress flicker during bulk color update
                    parentForm.SuspendLayout();

                    parentForm.BackColor = AppColors.BgBase;
                    parentForm.ForeColor = AppColors.TextPrimary;

                    // Comprehensive recursive repaint
                    AppColors.ApplyTheme(parentForm);

                    parentForm.ResumeLayout(true);
                    parentForm.Refresh();   // immediate
                }

                // Repaint self
                BackColor = AppColors.BgCard;
                ApplyChildTheme();
                Invalidate();
                ThemeToggled?.Invoke(this, EventArgs.Empty);
            }
            else if (_useStudentTopbar && _notificationRect.Contains(e.Location))
            {
                HideFloatingLabel();
                BuildMenus();
                _notificationMenu.Show(this, _notificationRect.Left, _notificationRect.Bottom + 4);
            }
            else if (_useStudentTopbar && _searchRect.Contains(e.Location))
            {
                HideFloatingLabel();
                _quickSearchBox.Focus();
            }
            else if (_useStudentTopbar && _accountRect.Contains(e.Location))
            {
                HideFloatingLabel();
                BuildMenus();
                _accountMenu.Show(this, _accountRect.Left, _accountRect.Bottom + 4);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!_useStudentTopbar || !_searchRect.Contains(e.Location))
                SearchFocusManager.BlurFocusedSearchInput(FindForm());
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutSearchBox();
        }

        private void PaintStudentTopbar(Graphics g)
        {
            ApplyChildTheme();

            int y = (Height - 36) / 2;
            using SolidBrush textPrimary = new SolidBrush(AppColors.TextPrimary);
            using SolidBrush textSecondary = new SolidBrush(AppColors.TextSecondary);
            using SolidBrush accentBrush = new SolidBrush(AppColors.AccentBlue);
            using SolidBrush whiteBrush = new SolidBrush(Color.White);
            using Font logoFont = AppFonts.Semibold(15f);
            using Font titleFont = AppFonts.Semibold(9f);

            StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            StringFormat sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

            Rectangle logoRect = new Rectangle(18, y, 36, 36);
            GraphicsHelpers.FillRoundedRect(g, logoRect, 10, AppColors.AccentBlue);
            g.DrawString("C", logoFont, whiteBrush, logoRect, sfCenter);

            bool compact = Width < 900;
            bool ultraCompact = Width < 720;
            int x = logoRect.Right + 8;
            int brandWidth = ultraCompact ? 0 : compact ? 90 : 112;
            if (brandWidth > 0)
            {
                g.DrawString("CourseGuard", AppFonts.Button, textPrimary, new RectangleF(x, y, brandWidth, 36), sfLeft);
                x += brandWidth + 8;
            }

            int pageWidth = compact
                ? Math.Max(70, Math.Min(96, TextRenderer.MeasureText(_pageTitle, AppFonts.Body).Width + 20))
                : Math.Max(92, Math.Min(150, TextRenderer.MeasureText(_pageTitle, AppFonts.Body).Width + 28));
            Rectangle pageRect = new Rectangle(x, y + 3, pageWidth, 30);
            GraphicsHelpers.FillRoundedRect(g, pageRect, 10, AppColors.IsDarkMode ? AppColors.BgCardHover : ColorTranslator.FromHtml("#EFF6FF"));
            using SolidBrush pageBrush = new SolidBrush(AppColors.AccentBlue);
            g.DrawString(_pageTitle, titleFont, pageBrush, pageRect, sfCenter);

            DrawStudentRightActions(g, y, pageRect.Right + 10, sfCenter, sfLeft);
            DrawSearchShell(g);
        }

        private void DrawStudentRightActions(Graphics g, int y, int leftSearch, StringFormat sfCenter, StringFormat sfLeft)
        {
            bool compact = Width < 900;
            bool ultraCompact = Width < 720;
            int rx = Width - 18;

            int accountWidth = compact ? 96 : Math.Max(112, Math.Min(160, TextRenderer.MeasureText(_userName, AppFonts.Body).Width + 58));
            rx -= accountWidth;
            _accountRect = new Rectangle(rx, y, accountWidth, 36);
            GraphicsHelpers.FillRoundedRect(g, _accountRect, 12, _hoverAccount ? AppColors.BgCardHover : Color.Transparent);
            Rectangle avatarRect = new Rectangle(_accountRect.Left + 8, y + 4, 28, 28);
            using SolidBrush accentBrush = new SolidBrush(AppColors.AccentBlue);
            using SolidBrush whiteBrush = new SolidBrush(Color.White);
            using Font initialFont = AppFonts.Semibold(8f);
            g.FillEllipse(accentBrush, avatarRect);
            g.DrawString(GetInitials(), initialFont, whiteBrush, avatarRect, sfCenter);
            using SolidBrush textPrimary = new SolidBrush(AppColors.TextPrimary);
            string displayName = compact && _userName.Length > 7 ? _userName.Substring(0, 7) : _userName;
            g.DrawString(displayName, AppFonts.Body, textPrimary, new RectangleF(avatarRect.Right + 6, y, accountWidth - 46, 36), sfLeft);
            rx -= compact ? 6 : 10;

            int iconSize = compact ? 34 : 36;
            rx -= iconSize;
            _notificationRect = new Rectangle(rx, y, iconSize, 36);
            if (_hoverNotification)
                GraphicsHelpers.FillRoundedRect(g, _notificationRect, 10, AppColors.BgCardHover);
            DrawBellIcon(g, _notificationRect, AppColors.TextSecondary);
            if (_notificationCount > 0)
                DrawCountBadge(g, new Rectangle(_notificationRect.Right - 16, _notificationRect.Top + 3, 18, 18), _notificationCount);
            rx -= compact ? 4 : 8;

            rx -= iconSize;
            _themeRect = new Rectangle(rx, y, iconSize, 36);
            if (_hoverTheme)
                GraphicsHelpers.FillRoundedRect(g, _themeRect, 10, AppColors.BgCardHover);
            DrawThemeIcon(g, _themeRect, AppColors.TextSecondary);
            rx -= compact ? 6 : 10;

            int connectionWidth = ultraCompact ? 0 : compact ? 66 : 92;
            if (connectionWidth > 0)
            {
                rx -= connectionWidth;
                DrawConnectionChip(g, new Rectangle(rx, y + 4, connectionWidth, 28));
                rx -= compact ? 6 : 8;
            }

            int examWidth = ultraCompact ? 92 : compact ? 100 : 158;
            if (examWidth > 0)
            {
                rx -= examWidth;
                DrawExamChip(g, new Rectangle(rx, y + 4, examWidth, 28));
                rx -= compact ? 8 : 12;
            }

            int searchWidth = Math.Max(0, rx - leftSearch);
            int minSearchWidth = compact ? 140 : 220;
            _searchRect = searchWidth >= minSearchWidth
                ? new Rectangle(leftSearch, y + 2, Math.Min(searchWidth, compact ? 240 : 360), 32)
                : Rectangle.Empty;
            _quickSearchBox.Visible = !_searchRect.IsEmpty;
        }

        private void DrawSearchShell(Graphics g)
        {
            if (_searchRect.IsEmpty)
                return;

            Color bg = AppColors.IsDarkMode ? AppColors.BgInput : ColorTranslator.FromHtml("#F8FAFC");
            GraphicsHelpers.FillRoundedRect(g, _searchRect, 12, bg);
            GraphicsHelpers.DrawRoundedBorder(g, _searchRect, 12, AppColors.Border, 1f);
            DrawSearchIcon(g, new Rectangle(_searchRect.Left + 8, _searchRect.Top + 6, 20, 20), AppColors.TextMuted);
            LayoutSearchBox();
        }

        private void DrawExamChip(Graphics g, Rectangle rect)
        {
            bool hasExam = _openExamCount > 0;
            Color bg = hasExam ? AppColors.WarningSoft : (AppColors.IsDarkMode ? AppColors.BgCardHover : ColorTranslator.FromHtml("#F1F5F9"));
            Color fg = hasExam ? AppColors.Warning : AppColors.TextSecondary;
            string text = rect.Width < 130
                ? (hasExam ? $"Bài thi: {_openExamCount}" : "Không có bài")
                : (hasExam ? $"Bài thi đang mở: {_openExamCount}" : "Không có bài thi đang mở");
            GraphicsHelpers.FillRoundedRect(g, rect, 10, bg);
            using SolidBrush brush = new SolidBrush(fg);
            using Font font = AppFonts.Semibold(8.5f);
            g.DrawString(text, font, brush, rect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DrawConnectionChip(Graphics g, Rectangle rect)
        {
            Color dot = _isOnline ? AppColors.Success : AppColors.Warning;
            string text = rect.Width < 80
                ? (_isOnline ? "Online" : "Offline")
                : (_isOnline ? "Online" : "Mất kết nối");
            GraphicsHelpers.FillRoundedRect(g, rect, 10, AppColors.IsDarkMode ? AppColors.BgCardHover : ColorTranslator.FromHtml("#F8FAFC"));
            using SolidBrush dotBrush = new SolidBrush(dot);
            using SolidBrush textBrush = new SolidBrush(AppColors.TextPrimary);
            g.FillEllipse(dotBrush, rect.Left + 10, rect.Top + 10, 8, 8);
            g.DrawString(text, AppFonts.Caption, textBrush, new RectangleF(rect.Left + 22, rect.Top, rect.Width - 24, rect.Height),
                new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
        }

        private void DrawCountBadge(Graphics g, Rectangle rect, int count)
        {
            GraphicsHelpers.FillRoundedRect(g, rect, 9, AppColors.Danger);
            using SolidBrush brush = new SolidBrush(Color.White);
            using Font font = AppFonts.Semibold(7f);
            string text = count > 9 ? "9+" : count.ToString();
            g.DrawString(text, font, brush, rect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DrawSearchIcon(Graphics g, Rectangle bounds, Color color)
        {
            using Pen pen = CreateIconPen(color);
            g.DrawEllipse(pen, bounds.Left + 3, bounds.Top + 3, 10, 10);
            g.DrawLine(pen, bounds.Left + 12, bounds.Top + 12, bounds.Right - 3, bounds.Bottom - 3);
        }

        private void LayoutSearchBox()
        {
            if (_quickSearchBox == null)
                return;

            if (_searchRect.IsEmpty || !_useStudentTopbar)
            {
                _quickSearchBox.Visible = false;
                return;
            }

            _quickSearchBox.SetBounds(_searchRect.Left + 34, _searchRect.Top + 7, _searchRect.Width - 44, 20);
        }

        private void ApplyChildTheme()
        {
            if (_quickSearchBox == null)
                return;

            _quickSearchBox.BackColor = AppColors.IsDarkMode ? AppColors.BgInput : ColorTranslator.FromHtml("#F8FAFC");
            _quickSearchBox.ForeColor = AppColors.TextPrimary;
        }

        private string GetInitials()
        {
            if (string.IsNullOrWhiteSpace(_userName))
                return "ST";

            string[] parts = _userName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();

            return _userName.Length >= 2
                ? _userName.Substring(0, 2).ToUpperInvariant()
                : _userName.Substring(0, 1).ToUpperInvariant();
        }

        private void BuildMenus()
        {
            _notificationMenu = new ContextMenuStrip();
            _accountMenu = new ContextMenuStrip();

            _notificationMenu.Items.Clear();
            _notificationMenu.Items.Add("Bài tập mới: OOP Cơ bản");
            _notificationMenu.Items.Add("Nhắc nhở: Lịch thi giữa kỳ");
            _notificationMenu.Items.Add("Tài liệu mới: C#");
            _notificationMenu.Items.Add(new ToolStripSeparator());
            _notificationMenu.Items.Add("Xem tất cả thông báo");

            StudentDropdownStyler.Apply(_notificationMenu);

            _accountMenu.Items.Clear();
            _accountMenu.Items.Add("Hồ sơ cá nhân");
            _accountMenu.Items.Add("Đổi mật khẩu");
            _accountMenu.Items.Add("Lịch sử đăng nhập");
            _accountMenu.Items.Add("Cài đặt");
            _accountMenu.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem logoutItem = new ToolStripMenuItem("Đăng xuất");
            logoutItem.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
            _accountMenu.Items.Add(logoutItem);
            StudentDropdownStyler.Apply(_accountMenu);
        }

        private void UpdateHoverLabel(Point mouseLocation, bool hoverTheme, bool hoverNotification, bool hoverAccount, bool hoverSearch)
        {
            string label = string.Empty;
            Rectangle anchor = Rectangle.Empty;

            if (hoverTheme)
                label = AppColors.IsDarkMode ? "Chuyển sang Light" : "Chuyển sang Dark";
            else if (_useStudentTopbar && hoverNotification)
                label = "Thông báo";
            else if (_useStudentTopbar && hoverAccount)
                label = "Tài khoản";
            else if (_useStudentTopbar && hoverSearch)
                label = "Tìm kiếm nhanh";

            if (hoverTheme)
                anchor = _themeRect;
            else if (_useStudentTopbar && hoverNotification)
                anchor = _notificationRect;
            else if (_useStudentTopbar && hoverAccount)
                anchor = _accountRect;
            else if (_useStudentTopbar && hoverSearch)
                anchor = _searchRect;

            if (string.IsNullOrWhiteSpace(label) || anchor.IsEmpty)
            {
                HideFloatingLabel();
                return;
            }

            ShowFloatingLabel(label, anchor);
        }

        private void ShowFloatingLabel(string text, Rectangle anchor)
        {
            _floatingLabel ??= new FloatingTopbarLabel();

            if (_floatingLabelText != text)
            {
                _floatingLabelText = text;
                _floatingLabel.SetText(text);
            }

            _floatingLabel.Location = PointToScreen(new Point(
                anchor.Left + (anchor.Width - _floatingLabel.Width) / 2,
                anchor.Bottom + 8));
            if (!_floatingLabel.Visible)
                _floatingLabel.Show(this);
        }

        private void HideFloatingLabel()
        {
            _floatingLabelText = string.Empty;
            if (_floatingLabel != null && !_floatingLabel.IsDisposed && _floatingLabel.Visible)
                _floatingLabel.Hide();
        }

        private void EnsureMenus()
        {
            if (_notificationMenu == null || _notificationMenu.IsDisposed)
                _notificationMenu = new ContextMenuStrip();

            if (_accountMenu == null || _accountMenu.IsDisposed)
                _accountMenu = new ContextMenuStrip();
        }

        private static void DrawGearIcon(Graphics g, Rectangle bounds, Color color)
        {
            Rectangle r = CenterSquare(bounds, 18);
            using Pen pen = CreateIconPen(color);

            g.DrawEllipse(pen, r.Left + 5, r.Top + 5, 8, 8);
            g.DrawEllipse(pen, r.Left + 7, r.Top + 7, 4, 4);

            Point center = new Point(r.Left + 9, r.Top + 9);
            for (int i = 0; i < 8; i++)
            {
                double angle = Math.PI * 2 * i / 8;
                Point p1 = new Point(center.X + (int)(Math.Cos(angle) * 7), center.Y + (int)(Math.Sin(angle) * 7));
                Point p2 = new Point(center.X + (int)(Math.Cos(angle) * 9), center.Y + (int)(Math.Sin(angle) * 9));
                g.DrawLine(pen, p1, p2);
            }
        }

        private static void DrawBellIcon(Graphics g, Rectangle bounds, Color color)
        {
            Rectangle r = CenterSquare(bounds, 18);
            using Pen pen = CreateIconPen(color);
            using SolidBrush brush = new SolidBrush(color);

            g.DrawArc(pen, r.Left + 4, r.Top + 3, 10, 12, 200, 140);
            g.DrawLine(pen, r.Left + 5, r.Top + 9, r.Left + 3, r.Bottom - 4);
            g.DrawLine(pen, r.Right - 5, r.Top + 9, r.Right - 3, r.Bottom - 4);
            g.DrawLine(pen, r.Left + 3, r.Bottom - 4, r.Right - 3, r.Bottom - 4);
            g.FillEllipse(brush, r.Left + 8, r.Bottom - 2, 3, 3);
        }

        private static void DrawThemeIcon(Graphics g, Rectangle bounds, Color color)
        {
            Rectangle r = CenterSquare(bounds, 18);
            using Pen pen = CreateIconPen(color);
            using SolidBrush brush = new SolidBrush(color);

            if (AppColors.IsDarkMode)
            {
                g.DrawEllipse(pen, r.Left + 5, r.Top + 5, 8, 8);
                Point center = new Point(r.Left + 9, r.Top + 9);
                for (int i = 0; i < 8; i++)
                {
                    double angle = Math.PI * 2 * i / 8;
                    Point p1 = new Point(center.X + (int)(Math.Cos(angle) * 7), center.Y + (int)(Math.Sin(angle) * 7));
                    Point p2 = new Point(center.X + (int)(Math.Cos(angle) * 9), center.Y + (int)(Math.Sin(angle) * 9));
                    g.DrawLine(pen, p1, p2);
                }
            }
            else
            {
                g.FillEllipse(brush, r.Left + 4, r.Top + 3, 10, 12);
                using SolidBrush cutout = new SolidBrush(AppColors.BgCard);
                g.FillEllipse(cutout, r.Left + 8, r.Top + 1, 10, 13);
            }
        }

        private static Pen CreateIconPen(Color color)
        {
            return new Pen(color, 1.8f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
        }

        private static Rectangle CenterSquare(Rectangle bounds, int size)
        {
            return new Rectangle(
                bounds.Left + (bounds.Width - size) / 2,
                bounds.Top + (bounds.Height - size) / 2,
                size,
                size);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_floatingLabel != null)
                {
                    _floatingLabel.Dispose();
                    _floatingLabel = null;
                }

                _notificationMenu.Dispose();
                _accountMenu.Dispose();
            }

            base.Dispose(disposing);
        }

        private sealed class FloatingTopbarLabel : Form
        {
            private readonly Label _label;

            public FloatingTopbarLabel()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                BackColor = AppColors.BgElevated;
                Padding = new Padding(10, 6, 10, 6);
                DoubleBuffered = true;

                _label = new Label
                {
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    ForeColor = AppColors.TextPrimary,
                    Font = AppFonts.Button,
                    Location = new Point(10, 6)
                };
                Controls.Add(_label);
            }

            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    const int WS_EX_NOACTIVATE = 0x08000000;
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            public void SetText(string text)
            {
                _label.Text = text;
                Size = new Size(_label.Width + Padding.Horizontal, _label.Height + Padding.Vertical);
                BackColor = AppColors.BgElevated;
                _label.ForeColor = AppColors.TextPrimary;
                Region?.Dispose();
                using var path = GraphicsHelpers.RoundedRect(new Rectangle(0, 0, Width, Height), 8);
                Region = new Region(path);
            }
        }
    }
}
