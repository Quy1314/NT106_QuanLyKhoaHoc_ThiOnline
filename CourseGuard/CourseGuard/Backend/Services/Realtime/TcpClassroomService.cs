using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CourseGuard.Backend.Services.Realtime
{
    public sealed class TcpClassroomService : IDisposable
    {
        public static readonly TcpClassroomService Instance = new();
        public const int DefaultPort = 5056;

        private TcpListener? _listener;
        private bool _isRunning;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentBag<TcpClient> _clients = new();

        private TcpClassroomService() { }

        public void StartListening()
        {
            if (_isRunning) return;
            _isRunning = true;
            _cts = new CancellationTokenSource();

            _listener = new TcpListener(IPAddress.Any, DefaultPort);
            _listener.Start();

            _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                    _clients.Add(client);
                    _ = Task.Run(() => ReadClientLoop(client, cancellationToken));
                }
                catch (OperationCanceledException) { break; }
                catch (Exception) { /* log or ignore */ }
            }
        }

        private async Task ReadClientLoop(TcpClient client, CancellationToken token)
        {
            using (client)
            {
                try
                {
                    await using NetworkStream stream = client.GetStream();
                    var buffer = new byte[1];
                    while (!token.IsCancellationRequested)
                    {
                        int read = await stream.ReadAsync(buffer, token);
                        if (read == 0) break; // Client disconnected
                    }
                }
                catch { }
            }
        }

        public async Task BroadcastClassOpened(int sessionId)
        {
            var payload = new byte[5];
            payload[0] = 1; // 1 = OPEN
            BitConverter.GetBytes(sessionId).CopyTo(payload, 1);
            
            foreach (var client in _clients)
            {
                if (client.Connected)
                {
                    try
                    {
                        var stream = client.GetStream();
                        await stream.WriteAsync(payload);
                        await stream.FlushAsync();
                    }
                    catch { }
                }
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _isRunning = false;
        }
    }
}
