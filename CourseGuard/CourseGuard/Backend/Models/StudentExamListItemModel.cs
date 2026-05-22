using System;

namespace CourseGuard.Backend.Models
{
    public class StudentExamListItemModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime? OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
        public int DurationMinutes { get; set; }
        public int MaxAttempts { get; set; }
        public int AttemptCount { get; set; }
        public int QuestionCount { get; set; }

        public bool IsOpen
        {
            get
            {
                DateTime now = DateTime.Now;
                return RemainingAttempts > 0
                    && (!OpenTime.HasValue || OpenTime.Value <= now)
                    && (!CloseTime.HasValue || CloseTime.Value >= now);
            }
        }

        public int RemainingAttempts
        {
            get
            {
                if (MaxAttempts <= 0)
                    return int.MaxValue;

                return Math.Max(0, MaxAttempts - AttemptCount);
            }
        }

        public string StatusText
        {
            get
            {
                DateTime now = DateTime.Now;
                if (RemainingAttempts <= 0)
                    return "Hết lượt";
                if (OpenTime.HasValue && OpenTime.Value > now)
                    return "Sắp mở";
                if (CloseTime.HasValue && CloseTime.Value < now)
                    return "Đã đóng";

                return "Đang mở";
            }
        }
    }
}
