using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CourseGuard.Frontend.UserControls.Shared.Chat
{
    public sealed class ChatImageLoader : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(4, 4);
        private bool _disposed;

        public async Task<Image?> LoadThumbnailAsync(string imagePath, Size targetSize, CancellationToken cancellationToken = default)
        {
            string resolvedPath = Helpers.ChatImageHelper.ResolveLocalFilePath(imagePath);
            if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
            {
                return null;
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await Task.Run(() => CreateThumbnail(resolvedPath, targetSize, cancellationToken), cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static Image CreateThumbnail(string imagePath, Size targetSize, CancellationToken cancellationToken)
        {
            using (Image source = Image.FromFile(imagePath))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Size size = FitInside(source.Size, targetSize);
                var thumbnail = new Bitmap(size.Width, size.Height);
                using (Graphics graphics = Graphics.FromImage(thumbnail))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(source, 0, 0, size.Width, size.Height);
                }

                return thumbnail;
            }
        }

        private static Size FitInside(Size source, Size bounds)
        {
            if (source.Width <= 0 || source.Height <= 0)
            {
                return new Size(180, 120);
            }

            double ratio = Math.Min(bounds.Width / (double)source.Width, bounds.Height / (double)source.Height);
            ratio = Math.Min(1d, ratio);
            return new Size(Math.Max(1, (int)Math.Round(source.Width * ratio)), Math.Max(1, (int)Math.Round(source.Height * ratio)));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _semaphore.Dispose();
        }
    }
}
