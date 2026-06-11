namespace CourseGuard.Backend.Models
{
    public static class TeacherQuickSearchKinds
    {
        public const string Course = "Course";
        public const string Student = "Student";
        public const string Material = "Material";
    }

    public sealed class TeacherQuickSearchResultModel
    {
        public string Kind { get; set; } = string.Empty;
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PageName { get; set; } = string.Empty;
        public string Keyword { get; set; } = string.Empty;
    }
}
