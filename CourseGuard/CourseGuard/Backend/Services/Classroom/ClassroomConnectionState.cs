using System.Net.Sockets;

namespace CourseGuard.Backend.Services.Classroom
{
    public sealed class ClassroomConnectionState
    {
        public Guid ConnectionId { get; } = Guid.NewGuid();
        public TcpClient Client { get; }
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsMuted { get; set; }
        public bool IsCameraOn { get; set; }
        public bool IsHandRaised { get; set; }
        public SemaphoreSlim SendLock { get; } = new(1, 1);
        public DateTime ConnectedAt { get; } = DateTime.UtcNow;
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

        public ClassroomConnectionState(TcpClient client)
        {
            Client = client;
        }
    }
}
