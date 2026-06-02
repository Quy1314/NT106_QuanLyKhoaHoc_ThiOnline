using System;

namespace CourseGuard.Backend.Models
{
    public class AttendanceLogModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SessionId { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsValid { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
