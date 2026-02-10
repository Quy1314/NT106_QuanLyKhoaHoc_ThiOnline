using System.Drawing;

namespace CourseGuard.UI.Theme
{
    public static class ColorPalette
    {
        public struct LightMode
        {
            public static readonly Color Base = ColorTranslator.FromHtml("#F3F4F6"); // Gray-100
            public static readonly Color Secondary = ColorTranslator.FromHtml("#FFFFFF"); // White
            public static readonly Color Accent = ColorTranslator.FromHtml("#2563EB"); // Blue-600
            
            public static readonly Color TextPrimary = ColorTranslator.FromHtml("#111827"); // Gray-900
            public static readonly Color TextSecondary = ColorTranslator.FromHtml("#6B7280"); // Gray-500
            
            public static readonly Color Border = ColorTranslator.FromHtml("#E5E7EB"); // Gray-200
            
            public static readonly Color Hover = ColorTranslator.FromHtml("#1D4ED8"); // Blue-700
            public static readonly Color Active = ColorTranslator.FromHtml("#1E40AF"); // Blue-800
        }

        public struct DarkMode
        {
            public static readonly Color Base = ColorTranslator.FromHtml("#111827"); // Gray-900
            public static readonly Color Secondary = ColorTranslator.FromHtml("#1F2937"); // Gray-800
            public static readonly Color Accent = ColorTranslator.FromHtml("#3B82F6"); // Blue-500
            
            public static readonly Color TextPrimary = ColorTranslator.FromHtml("#F9FAFB"); // Gray-50
            public static readonly Color TextSecondary = ColorTranslator.FromHtml("#9CA3AF"); // Gray-400
            
            public static readonly Color Border = ColorTranslator.FromHtml("#374151"); // Gray-700
            
            public static readonly Color Hover = ColorTranslator.FromHtml("#2563EB"); // Blue-600
            public static readonly Color Active = ColorTranslator.FromHtml("#1D4ED8"); // Blue-700
        }

        public struct Status
        {
            // Light Mode Status
            public static readonly Color SuccessLight = ColorTranslator.FromHtml("#10B981");
            public static readonly Color WarningLight = ColorTranslator.FromHtml("#F59E0B");
            public static readonly Color ErrorLight = ColorTranslator.FromHtml("#EF4444");
            public static readonly Color InfoLight = ColorTranslator.FromHtml("#3B82F6");

            // Dark Mode Status
            public static readonly Color SuccessDark = ColorTranslator.FromHtml("#34D399");
            public static readonly Color WarningDark = ColorTranslator.FromHtml("#FBBF24");
            public static readonly Color ErrorDark = ColorTranslator.FromHtml("#F87171");
            public static readonly Color InfoDark = ColorTranslator.FromHtml("#60A5FA");
        }
    }
}
