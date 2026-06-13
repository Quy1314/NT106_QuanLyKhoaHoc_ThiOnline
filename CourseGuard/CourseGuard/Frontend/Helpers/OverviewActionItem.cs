namespace CourseGuard.Frontend.Helpers
{
    public sealed class OverviewActionItem
    {
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public string PageName { get; init; } = string.Empty;
        public string ActionText { get; init; } = string.Empty;
        public int Priority { get; init; }
        public string Tone { get; init; } = "Neutral";
    }
}
