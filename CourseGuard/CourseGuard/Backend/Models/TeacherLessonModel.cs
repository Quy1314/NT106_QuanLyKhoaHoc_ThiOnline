using System;

namespace CourseGuard.Backend.Models
{
    public class TeacherLessonModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? PublishAt { get; set; }
        public string Status { get; set; } = "DRAFT";
    }
}
