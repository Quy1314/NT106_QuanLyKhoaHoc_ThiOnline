using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Extensions;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Shared.Chat
{
    public class ChatBubbleControl : UserControl
    {
        private const int AvatarSize = 36;
        private const int MaxBubbleWidth = 520;
        private const int MinBubbleWidth = 96;

        private readonly ChatMessageModel _message;
        private readonly bool _isMine;
        private readonly ToolTip _timeToolTip = new();

        public int MessageId => _message.Id;
        public DateTime SentAt => _message.SentAt;

        public ChatBubbleControl(ChatMessageModel message, int currentUserId)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _isMine = message.SenderId == currentUserId;

            DoubleBuffered = true;
            AutoSize = false;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = AppColors.BgCard;
            Margin = new Padding(0, 4, 0, 4);
            MinimumSize = new Size(220, 52);
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            BuildLayout();
        }

        public void UpdateContainerWidth(int containerWidth)
        {
            int safeWidth = Math.Max(260, containerWidth - 28);
            Width = safeWidth;
            Height = CalculatePreferredHeight(safeWidth);
            Invalidate(true);
        }

        private void BuildLayout()
        {
            Controls.Clear();

            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgCard,
                ColumnCount = 3,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AvatarSize + 8));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AvatarSize + 8));

            Control content = CreateContentStack();
            if (_isMine)
            {
                row.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = AppColors.BgCard }, 0, 0);
                row.Controls.Add(CreateRightHost(content), 1, 0);
                row.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = AppColors.BgCard }, 2, 0);
            }
            else
            {
                row.Controls.Add(CreateAvatar(), 0, 0);
                row.Controls.Add(CreateLeftHost(content), 1, 0);
                row.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = AppColors.BgCard }, 2, 0);
            }

            Controls.Add(row);
        }

        private Control CreateContentStack()
        {
            string senderName = _isMine ? "Bạn" : _message.SenderName.GetShortName();
            string body = BuildBodyText(_message);
            Size textSize = TextRenderer.MeasureText(body, AppFonts.Body, new Size(MaxBubbleWidth - 28, int.MaxValue), TextFormatFlags.WordBreak);
            int bubbleWidth = Math.Min(MaxBubbleWidth, Math.Max(MinBubbleWidth, textSize.Width + 30));
            int bubbleHeight = Math.Max(38, textSize.Height + 22);

            var stack = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = AppColors.BgCard,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            var nameLabel = new Label
            {
                AutoSize = false,
                Width = bubbleWidth,
                Height = 18,
                Text = senderName,
                TextAlign = _isMine ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                Font = AppFonts.Caption,
                ForeColor = AppColors.TextSecondary,
                BackColor = AppColors.BgCard,
                Margin = new Padding(0, 0, 0, 2)
            };

            var bubble = new RoundedBubblePanel(_isMine, body)
            {
                Width = bubbleWidth,
                Height = bubbleHeight,
                Margin = Padding.Empty,
                Padding = new Padding(14, 10, 14, 10),
                Font = AppFonts.Body
            };

            string tooltip = _message.SentAt.ToString("HH:mm dd/MM/yyyy");
            _timeToolTip.SetToolTip(this, tooltip);
            _timeToolTip.SetToolTip(stack, tooltip);
            _timeToolTip.SetToolTip(nameLabel, tooltip);
            _timeToolTip.SetToolTip(bubble, tooltip);

            stack.Controls.Add(nameLabel);
            stack.Controls.Add(bubble);
            return stack;
        }

        private static string BuildBodyText(ChatMessageModel message)
        {
            if (string.Equals(message.MessageType, "FILE", StringComparison.OrdinalIgnoreCase))
            {
                string sizeText = message.FileSize <= 0 ? "-" : $"{Math.Round(message.FileSize / 1024.0, 1)} KB";
                string fileName = string.IsNullOrWhiteSpace(message.FileName) ? "Tệp đính kèm" : message.FileName;
                string caption = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : $"\n{message.Content}";
                return $"📎 {fileName} ({sizeText}){caption}";
            }

            return string.IsNullOrWhiteSpace(message.Content) ? "(Tin nhắn trống)" : message.Content;
        }

        private Control CreateLeftHost(Control content)
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgCard,
                Padding = new Padding(0, 0, 80, 0),
                Controls = { content }
            };
        }

        private Control CreateRightHost(Control content)
        {
            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgCard,
                Padding = new Padding(80, 0, 0, 0)
            };
            content.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            host.Controls.Add(content);
            host.Resize += (_, _) => content.Left = Math.Max(0, host.ClientSize.Width - host.Padding.Right - content.Width);
            return host;
        }

        private Control CreateAvatar()
        {
            var avatar = new InitialsAvatarControl(_message.SenderName)
            {
                Width = AvatarSize,
                Height = AvatarSize,
                Margin = new Padding(0, 20, 8, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            return avatar;
        }

        private int CalculatePreferredHeight(int safeWidth)
        {
            int available = Math.Min(MaxBubbleWidth - 28, Math.Max(120, safeWidth - 160));
            Size textSize = TextRenderer.MeasureText(BuildBodyText(_message), AppFonts.Body, new Size(available, int.MaxValue), TextFormatFlags.WordBreak);
            return Math.Max(58, textSize.Height + 48);
        }

        private sealed class RoundedBubblePanel : Panel
        {
            private readonly bool _isMine;
            private readonly string _body;

            public RoundedBubblePanel(bool isMine, string body)
            {
                _isMine = isMine;
                _body = body;
                DoubleBuffered = true;
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
                BackColor = AppColors.BgCard;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new(0, 0, Width - 1, Height - 1);
                using GraphicsPath path = CreateRoundRect(rect, 18);
                using SolidBrush brush = new(_isMine ? AppColors.AccentPressed : AppColors.BgElevated);
                using Pen border = new(_isMine ? AppColors.AccentHover : AppColors.BorderStrong);
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);

                Rectangle textRect = new(Padding.Left, Padding.Top, Width - Padding.Horizontal, Height - Padding.Vertical);
                Color textColor = _isMine ? Color.White : AppColors.TextPrimary;
                TextRenderer.DrawText(
                    e.Graphics,
                    _body,
                    Font,
                    textRect,
                    textColor,
                    TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding);
            }
        }

        private sealed class InitialsAvatarControl : Control
        {
            private readonly string _initials;

            public InitialsAvatarControl(string name)
            {
                _initials = name.GetInitials();
                DoubleBuffered = true;
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                BackColor = AppColors.BgCard;
                Font = AppFonts.Semibold(9f);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new(0, 0, Width - 1, Height - 1);
                using SolidBrush fill = new(AppColors.AccentSoft);
                using Pen border = new(AppColors.AccentBlue);
                e.Graphics.FillEllipse(fill, rect);
                e.Graphics.DrawEllipse(border, rect);
                TextRenderer.DrawText(e.Graphics, _initials, Font, rect, AppColors.AccentBlue, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private static GraphicsPath CreateRoundRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
