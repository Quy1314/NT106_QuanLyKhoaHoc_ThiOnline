using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CourseGuard.Backend.Services.Realtime
{
    public sealed class TcpClassroomService : IClassroomSignalService, IDisposable
    {
        public static readonly TcpClassroomService Instance = new();
        public const int DefaultPort = 5056;

        private TcpListener? _listener;
        private bool _isRunning;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentBag<TcpClient> _clients = new();
        private readonly object _startLock = new();

        private TcpClassroomService() { }

        public void StartListening()
        {
            lock (_startLock)
            {
                if (_isRunning) return;

                var cts = new CancellationTokenSource();
                var listener = new TcpListener(IPAddress.Any, DefaultPort);
                try
                {
                    listener.Start();
                }
                catch
                {
                    listener.Stop();
                    cts.Dispose();
                    throw;
                }

                _cts = cts;
                _listener = listener;
                _isRunning = true;

                _ = Task.Run(() => AcceptLoopAsync(cts.Token));
            }
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
            payload[0] = 1;
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

        public async Task BroadcastClassClosed(int sessionId)
        {
            var payload = new byte[5];
            payload[0] = 0;
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
