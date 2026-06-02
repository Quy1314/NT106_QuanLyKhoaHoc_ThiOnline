using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using CourseGuard.Backend.Models;

namespace CourseGuard.Backend.Services.Classroom
{
    public sealed class TcpClassroomServer : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, ClassroomConnectionState> _clients = new();
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private bool _isRunning;

        public event EventHandler<ClassroomSignalReceivedEventArgs>? SignalReceived;
        public event EventHandler<ClassroomClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<ClassroomClientConnectedEventArgs>? ClientDisconnected;
        public event EventHandler<ClassroomStatusEventArgs>? StatusChanged;

        public bool IsRunning => _isRunning;
        public int SessionId { get; private set; }
        public int Port { get; private set; } = ClassroomProtocol.DefaultPort;

        public IReadOnlyCollection<ClassroomConnectionState> Clients => _clients.Values.ToList().AsReadOnly();

        public Task StartAsync(int sessionId, int port = ClassroomProtocol.DefaultPort, CancellationToken cancellationToken = default)
        {
            if (_isRunning)
            {
                return Task.CompletedTask;
            }

            SessionId = sessionId;
            Port = port;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;

            StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Classroom server started on port {port}."));
            _ = Task.Run(() => AcceptLoopAsync(_cts.Token), _cts.Token);
            return Task.CompletedTask;
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                    client.NoDelay = true;

                    var state = new ClassroomConnectionState(client)
                    {
                        SessionId = SessionId
                    };
                    _clients[state.ConnectionId] = state;
                    ClientConnected?.Invoke(this, new ClassroomClientConnectedEventArgs(state));
                    StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Client connected: {client.Client.RemoteEndPoint}"));

                    _ = Task.Run(() => ReadClientLoopAsync(state, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Accept client failed: {ex.Message}"));
                }
            }
        }

        private async Task ReadClientLoopAsync(ClassroomConnectionState state, CancellationToken cancellationToken)
        {
            try
            {
                await using NetworkStream stream = state.Client.GetStream();
                while (!cancellationToken.IsCancellationRequested && state.Client.Connected)
                {
                    ClassroomSignalModel? signal = await ClassroomProtocol.ReadMessageAsync(stream, cancellationToken);
                    if (signal == null)
                    {
                        continue;
                    }

                    state.LastSeenAt = DateTime.UtcNow;
                    ApplySignalToState(state, signal);
                    SignalReceived?.Invoke(this, new ClassroomSignalReceivedEventArgs(signal));

                    if (signal.Type == ClassroomMessageType.Ping)
                    {
                        await SendToClientAsync(state, new ClassroomSignalModel
                        {
                            Type = ClassroomMessageType.Pong,
                            SessionId = signal.SessionId,
                            SenderId = 0,
                            SenderName = "CourseGuard Server",
                            SenderRole = "SERVER"
                        }, cancellationToken);
                    }
                    else
                    {
                        await BroadcastAsync(signal, cancellationToken: cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown.
            }
            catch (IOException ex)
            {
                StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Client stream closed: {ex.Message}"));
            }
            catch (SocketException ex)
            {
                StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Client socket closed: {ex.Message}"));
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Read client failed: {ex.Message}"));
            }
            finally
            {
                RemoveClient(state);
            }
        }

        private static void ApplySignalToState(ClassroomConnectionState state, ClassroomSignalModel signal)
        {
            if (signal.SessionId > 0)
            {
                state.SessionId = signal.SessionId;
            }

            if (signal.SenderId > 0)
            {
                state.UserId = signal.SenderId;
            }

            if (!string.IsNullOrWhiteSpace(signal.SenderName))
            {
                state.DisplayName = signal.SenderName;
            }

            if (!string.IsNullOrWhiteSpace(signal.SenderRole))
            {
                state.Role = signal.SenderRole;
            }

            state.IsHandRaised = signal.Type switch
            {
                ClassroomMessageType.RaiseHand => true,
                ClassroomMessageType.LowerHand => false,
                _ => state.IsHandRaised
            };

            state.IsMuted = signal.Type switch
            {
                ClassroomMessageType.MicOff => true,
                ClassroomMessageType.MicOn => false,
                _ => state.IsMuted
            };

            state.IsCameraOn = signal.Type switch
            {
                ClassroomMessageType.CamOn => true,
                ClassroomMessageType.CamOff => false,
                _ => state.IsCameraOn
            };
        }

        public async Task BroadcastAsync(ClassroomSignalModel signal, Guid? exceptConnectionId = null, CancellationToken cancellationToken = default)
        {
            List<Task> sendTasks = new();
            foreach (ClassroomConnectionState client in _clients.Values)
            {
                if (exceptConnectionId.HasValue && client.ConnectionId == exceptConnectionId.Value)
                {
                    continue;
                }

                sendTasks.Add(SendToClientSafeAsync(client, signal, cancellationToken));
            }

            await Task.WhenAll(sendTasks);
        }

        private async Task SendToClientSafeAsync(ClassroomConnectionState client, ClassroomSignalModel signal, CancellationToken cancellationToken)
        {
            try
            {
                await SendToClientAsync(client, signal, cancellationToken);
            }
            catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
            {
                StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Send failed, removing client: {ex.Message}"));
                RemoveClient(client);
            }
        }

        public static async Task SendToClientAsync(ClassroomConnectionState client, ClassroomSignalModel signal, CancellationToken cancellationToken = default)
        {
            await client.SendLock.WaitAsync(cancellationToken);
            try
            {
                NetworkStream stream = client.Client.GetStream();
                await ClassroomProtocol.WriteMessageAsync(stream, signal, cancellationToken);
            }
            finally
            {
                client.SendLock.Release();
            }
        }

        private void RemoveClient(ClassroomConnectionState state)
        {
            if (_clients.TryRemove(state.ConnectionId, out _))
            {
                try
                {
                    state.Client.Close();
                }
                catch
                {
                    // Ignore cleanup failures.
                }

                ClientDisconnected?.Invoke(this, new ClassroomClientConnectedEventArgs(state));
                StatusChanged?.Invoke(this, new ClassroomStatusEventArgs($"Client disconnected: {state.DisplayName}"));
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!_isRunning)
            {
                return;
            }

            _cts?.Cancel();
            _listener?.Stop();

            foreach (ClassroomConnectionState client in _clients.Values.ToList())
            {
                try
                {
                    await SendToClientSafeAsync(client, new ClassroomSignalModel
                    {
                        Type = ClassroomMessageType.ClassClosed,
                        SessionId = SessionId,
                        SenderId = 0,
                        SenderName = "CourseGuard Server",
                        SenderRole = "SERVER"
                    }, cancellationToken);
                }
                catch
                {
                    // Ignore shutdown send failures.
                }

                RemoveClient(client);
            }

            _clients.Clear();
            _isRunning = false;
            StatusChanged?.Invoke(this, new ClassroomStatusEventArgs("Classroom server stopped."));
        }

        public void Dispose()
        {
            _ = StopAsync();
            _cts?.Dispose();
        }
    }
}
