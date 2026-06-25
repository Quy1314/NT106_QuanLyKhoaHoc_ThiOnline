using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CourseGuard.Backend.Config;

namespace CourseGuard.Backend.Services.Realtime
{
    public sealed class TcpClassroomService : IClassroomSignalService, IDisposable
    {
        public static readonly TcpClassroomService Instance = new();

        private ClientWebSocket? _ws;
        private bool _isRunning;
        private CancellationTokenSource? _cts;
        private readonly object _startLock = new();

        private TcpClassroomService() { }

        public void StartListening()
        {
            lock (_startLock)
            {
                if (_isRunning) return;

                _cts = new CancellationTokenSource();
                
                string? rawRelay = AppEnvironment.GetOptional("RELAY_WS_URL");
                string wsBase = "ws://localhost:8080";
                if (!string.IsNullOrWhiteSpace(rawRelay))
                {
                    wsBase = rawRelay;
                    if (wsBase.EndsWith("/relay", StringComparison.OrdinalIgnoreCase))
                    {
                        wsBase = wsBase.Substring(0, wsBase.Length - 6);
                    }
                }

                string connectionUrl = $"{wsBase}/classroom-status-relay?role=teacher";

                _ws = new ClientWebSocket();
                try
                {
                    _ws.ConnectAsync(new Uri(connectionUrl), _cts.Token).GetAwaiter().GetResult();
                    _isRunning = true;
                }
                catch (Exception ex)
                {
                    _ws.Dispose();
                    _cts.Dispose();
                    throw new InvalidOperationException($"Could not connect to Cloud Status Relay: {ex.Message}", ex);
                }
            }
        }

        public async Task BroadcastClassOpened(int sessionId)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                return;
            }

            var payload = new byte[5];
            payload[0] = 1;
            BitConverter.GetBytes(sessionId).CopyTo(payload, 1);

            try
            {
                await _ws.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch { }
        }

        public async Task BroadcastClassClosed(int sessionId)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                return;
            }

            var payload = new byte[5];
            payload[0] = 0;
            BitConverter.GetBytes(sessionId).CopyTo(payload, 1);

            try
            {
                await _ws.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch { }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                try
                {
                    _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).GetAwaiter().GetResult();
                }
                catch { }
            }
            _ws?.Dispose();
            _isRunning = false;
        }
    }
}
