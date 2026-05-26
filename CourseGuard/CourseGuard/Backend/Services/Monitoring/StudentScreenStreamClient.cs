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
        private TcpClient? _client;

        public StudentScreenStreamClient(int examId, int studentId, int attemptId = 0, string host = "127.0.0.1")
        {
            _examId = examId;
            _studentId = studentId;
            _attemptId = attemptId;
            _host = host;
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
                    await using NetworkStream stream = _client.GetStream();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        byte[] jpeg = CaptureJpeg();
                        byte[] header = ScreenStreamProtocol.BuildHeader(_examId, _studentId, _attemptId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), jpeg.Length);
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
