using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourseGuard.Backend.Services.Monitoring
{
    public sealed class StudentScreenStreamClient : IDisposable
    {
        private readonly int _examId;
        private readonly int _studentId;
        private readonly int _attemptId;
        private readonly string _host;
        private readonly ScreenMonitorConnectionLossTracker _connectionLossTracker;
        private TcpClient? _client;

        private volatile bool _pendingWarning;

        public event EventHandler<ScreenMonitorConnectionLostEventArgs>? ConnectionLostThresholdReached;

        public StudentScreenStreamClient(
            int examId,
            int studentId,
            int attemptId = 0,
            string host = "127.0.0.1",
            TimeSpan? connectionLossThreshold = null)
        {
            _examId = examId;
            _studentId = studentId;
            _attemptId = attemptId;
            _host = host;
            _connectionLossTracker = new ScreenMonitorConnectionLossTracker(
                connectionLossThreshold ?? ScreenMonitorConnectionLossTracker.DefaultThreshold,
                () => DateTimeOffset.UtcNow);
        }

        public void SendWarning()
        {
            _pendingWarning = true;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _client?.Dispose();
                    _client = new TcpClient();
                    await _client.ConnectAsync(_host, ScreenStreamProtocol.DefaultPort, cancellationToken);
                    _connectionLossTracker.ObserveConnected();
                    await using NetworkStream stream = _client.GetStream();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        byte frameType = 0;
                        if (_pendingWarning)
                        {
                            frameType = 1;
                            _pendingWarning = false;
                        }

                        byte[] jpeg = CaptureJpeg();
                        byte[] header = ScreenStreamProtocol.BuildHeader(frameType, _examId, _studentId, _attemptId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), jpeg.Length);
                        await stream.WriteAsync(header, cancellationToken);
                        await stream.WriteAsync(jpeg, cancellationToken);
                        await stream.FlushAsync(cancellationToken);
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch
                {
                    // Monitoring must never interrupt the student's exam flow.
                    if (cancellationToken.IsCancellationRequested) break;
                    if (_connectionLossTracker.ObserveDisconnected())
                    {
                        ConnectionLostThresholdReached?.Invoke(
                            this,
                            new ScreenMonitorConnectionLostEventArgs(
                                _examId,
                                _studentId,
                                _attemptId,
                                _connectionLossTracker.CurrentDisconnectedFor));
                    }
                    try { await Task.Delay(2000, cancellationToken); } catch { }
                }
            }
        }

        private static byte[] CaptureJpeg()
        {
            Rectangle bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1280, 720);
            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
                graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            using var output = new MemoryStream();
            ImageCodecInfo? jpgEncoder = Array.Find(ImageCodecInfo.GetImageEncoders(), e => e.MimeType == "image/jpeg");
            if (jpgEncoder == null)
            {
                bitmap.Save(output, ImageFormat.Jpeg);
            }
            else
            {
                using var parameters = new EncoderParameters(1);
                parameters.Param[0] = new EncoderParameter(Encoder.Quality, 55L);
                bitmap.Save(output, jpgEncoder, parameters);
            }
            return output.ToArray();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
