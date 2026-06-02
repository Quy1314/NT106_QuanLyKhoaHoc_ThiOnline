using System.Net.Sockets;
using CourseGuard.Backend.Models;

namespace CourseGuard.Backend.Services.Classroom
{
    public sealed class TcpClassroomClient : IDisposable
    {
        private TcpClient? _client;
        private CancellationTokenSource? _cts;
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private bool _isConnected;

        public event EventHandler<ClassroomSignalReceivedEventArgs>? SignalReceived;
        public event EventHandler<ClassroomStatusEventArgs>? StatusChanged;

        public bool IsConnected => _isConnected;
        public string Host { get; private set; } = string.Empty;
        public int Port { get; private set; } = ClassroomProtocol.DefaultPort;

        public async Task ConnectAsync(string host, int port = ClassroomProtocol.DefaultPort, CancellationToken cancellationToken = default)
        {
            if (_isConnected)
            {
                return;
            }

            Host = host;
            Port = port;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _client = new TcpClient
            {
                NoDelay = true
            };

            await _client.ConnectAsync(host, port, _cts.Token);
            _isConnected = true;
            StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Connected to classroom server {host}:{port}."));

            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token), _cts.Token);
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_client == null)
                {
                    return;
                }

                await using NetworkStream stream = _client.GetStream();
                while (!cancellationToken.IsCancellationRequested && _client.Connected)
                {
                    ClassroomSignalModel? signal = await ClassroomProtocol.ReadMessageAsync(stream, cancellationToken);
                    if (signal != null)
                    {
                        SignalReceived?.Invoke(this, new ClassroomSignalReceivedEventArgs(signal));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown.
            }
            catch (IOException ex)
            {
                StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Classroom stream closed: {ex.Message}"));
            }
            catch (SocketException ex)
            {
                StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Classroom socket closed: {ex.Message}"));
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Receive classroom signal failed: {ex.Message}"));
            }
            finally
            {
                _isConnected = false;
                StatusChanged?.Invoke(this, new ClassroomStatusEventArgs("Disconnected from classroom server."));
            }
        }

        public async Task SendAsync(ClassroomSignalModel signal, CancellationToken cancellationToken = default)
        {
            if (_client == null || !_isConnected)
            {
                throw new InvalidOperationException("Classroom client is not connected.");
            }

            await _sendLock.WaitAsync(cancellationToken);
            try
            {
                NetworkStream stream = _client.GetStream();
                await ClassroomProtocol.WriteMessageAsync(stream, signal, cancellationToken);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public async Task JoinRoomAsync(int sessionId, int userId, string displayName, string role, CancellationToken cancellationToken = default)
        {
            await SendAsync(new ClassroomSignalModel
            {
                Type = ClassroomMessageType.JoinRoom,
                SessionId = sessionId,
                SenderId = userId,
                SenderName = displayName,
                SenderRole = role
            }, cancellationToken);
        }

        public async Task LeaveRoomAsync(int sessionId, int userId, string displayName, string role, CancellationToken cancellationToken = default)
        {
            await SendAsync(new ClassroomSignalModel
            {
                Type = ClassroomMessageType.LeaveRoom,
                SessionId = sessionId,
                SenderId = userId,
                SenderName = displayName,
                SenderRole = role
            }, cancellationToken);
        }

        public async Task DisconnectAsync()
        {
            _cts?.Cancel();
            _isConnected = false;

            try
            {
                _client?.Close();
            }
            catch
            {
                // Ignore cleanup failures.
            }

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _ = DisconnectAsync();
            _cts?.Dispose();
            _sendLock.Dispose();
        }
    }
}
