using System;

namespace CourseGuard.Backend.Models
{
    public class ChatMessageModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderAvatar { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "TEXT";
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int? PollId { get; set; }
        public string DeliveryStatus { get; set; } = "SENT";
        public string DeliveryError { get; set; } = string.Empty;
        public PollModel? Poll { get; set; }
        public DateTime CreatedAt
        {
            get => SentAt;
            set => SentAt = value;
        }
    }
}
