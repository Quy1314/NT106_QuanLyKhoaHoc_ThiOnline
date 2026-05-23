using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CourseGuard.Backend.Services.Monitoring
{
    public sealed class TcpScreenMonitorService : IDisposable
    {
        private TcpListener? _listener;

        public event EventHandler<ScreenFrameReceivedEventArgs>? FrameReceived;
        public event EventHandler<ScreenMonitorStatusEventArgs>? StatusChanged;

        public async Task StartAsync(int examId, int studentId, int attemptId, CancellationToken cancellationToken)
        {
            _listener = new TcpListener(IPAddress.Any, ScreenStreamProtocol.DefaultPort);
            _listener.Start();
            StatusChanged?.Invoke(this, new ScreenMonitorStatusEventArgs("Đang kết nối"));

            while (!cancellationToken.IsCancellationRequested)
            {
                using TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);
                StatusChanged?.Invoke(this, new ScreenMonitorStatusEventArgs("Đang nhận hình"));
                await ReadClientAsync(client, examId, studentId, attemptId, cancellationToken);
                StatusChanged?.Invoke(this, new ScreenMonitorStatusEventArgs("Mất kết nối"));
            }
        }

        private async Task ReadClientAsync(TcpClient client, int examId, int studentId, int attemptId, CancellationToken cancellationToken)
        {
            await using NetworkStream stream = client.GetStream();
            var header = new byte[ScreenStreamProtocol.HeaderLength];
            while (!cancellationToken.IsCancellationRequested)
            {
                await stream.ReadExactlyAsync(header, cancellationToken);
                if (!ScreenStreamProtocol.TryReadHeader(header, out int frameExamId, out int frameStudentId, out int frameAttemptId, out _, out int length))
                    return;

                var payload = new byte[length];
                await stream.ReadExactlyAsync(payload, cancellationToken);
                if (frameExamId == examId && frameStudentId == studentId && (attemptId <= 0 || frameAttemptId == attemptId))
                    FrameReceived?.Invoke(this, new ScreenFrameReceivedEventArgs(frameExamId, frameStudentId, frameAttemptId, payload));
            }
        }

        public void Dispose()
        {
            _listener?.Stop();
        }
    }
}
