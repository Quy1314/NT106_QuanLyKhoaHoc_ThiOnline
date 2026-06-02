using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CourseGuard.Backend.Services.Realtime
{
    public class ClassStatusEventArgs : EventArgs
    {
        public int SessionId { get; }
        public bool IsOpened { get; }
        public ClassStatusEventArgs(int sessionId, bool isOpened)
        {
            SessionId = sessionId;
            IsOpened = isOpened;
        }
    }

    public sealed class TcpClassroomClient : IDisposable
    {
        private readonly string _host;
        private TcpClient? _client;
        private CancellationTokenSource? _cts;

        public event EventHandler<ClassStatusEventArgs>? ClassStatusChanged;

        public TcpClassroomClient(string host = "127.0.0.1")
        {
            _host = host;
        }

        public async Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    _client?.Dispose();
                    _client = new TcpClient();
                    await _client.ConnectAsync(_host, TcpClassroomService.DefaultPort, _cts.Token);
                    await using NetworkStream stream = _client.GetStream();
                    
                    var payload = new byte[5];
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        int read = await stream.ReadAsync(payload, _cts.Token);
                        if (read == 0) break;
                        if (read == 5)
                        {
                            bool isOpened = payload[0] == 1;
                            int sessionId = BitConverter.ToInt32(payload, 1);
                            ClassStatusChanged?.Invoke(this, new ClassStatusEventArgs(sessionId, isOpened));
                        }
                    }
                }
                catch
                {
                    if (_cts.Token.IsCancellationRequested) break;
                    try { await Task.Delay(2000, _cts.Token); } catch { }
                }
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _client?.Dispose();
        }
    }
}
