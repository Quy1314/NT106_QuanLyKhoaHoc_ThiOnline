using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CourseGuard.Frontend.Helpers
{
    public static class ChatImageHelper
    {
        public const long MaxImageSizeBytes = 5 * 1024 * 1024;
        public const int MaxImageDimension = 1600;
        public const long TargetCompressedBytes = 950 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public static bool IsSupportedImage(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public static bool IsImageMessage(string? messageType, string? mimeType, string? fileName, string? fileUrl)
        {
            return string.Equals(messageType, "IMAGE", StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(mimeType) && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                || IsSupportedImage(fileName)
                || IsSupportedImage(fileUrl);
        }

        public static string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".png" ? "image/png" : "image/jpeg";
        }

        public static void ValidateImageForUpload(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("Không tìm thấy ảnh cần gửi.", filePath);
            }

            if (!IsSupportedImage(filePath))
            {
                throw new InvalidOperationException("Chỉ hỗ trợ gửi ảnh JPG, JPEG hoặc PNG.");
            }

            long length = new FileInfo(filePath).Length;
            if (length > MaxImageSizeBytes)
            {
                throw new InvalidOperationException("Ảnh gốc vượt quá giới hạn 5MB. Vui lòng chọn ảnh nhỏ hơn.");
            }
        }

        public static Task<CompressedChatImageResult> CompressImageAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => CompressImage(filePath, cancellationToken), cancellationToken);
        }

        private static CompressedChatImageResult CompressImage(string filePath, CancellationToken cancellationToken)
        {
            ValidateImageForUpload(filePath);
            cancellationToken.ThrowIfCancellationRequested();

            string tempDirectory = Path.Combine(Path.GetTempPath(), "CourseGuard", "chat-images");
            Directory.CreateDirectory(tempDirectory);

            string outputPath = Path.Combine(tempDirectory, $"chat_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}.jpg");
            long originalSize = new FileInfo(filePath).Length;

            using (Image original = Image.FromFile(filePath))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Size targetSize = CalculateTargetSize(original.Width, original.Height);

                using (var resized = new Bitmap(targetSize.Width, targetSize.Height))
                {
                    resized.SetResolution(original.HorizontalResolution, original.VerticalResolution);
                    using (Graphics graphics = Graphics.FromImage(resized))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(original, 0, 0, targetSize.Width, targetSize.Height);
                    }

                    SaveJpegWithQuality(resized, outputPath, originalSize > TargetCompressedBytes ? 78L : 86L);
                }
            }

            long compressedSize = new FileInfo(outputPath).Length;
            return new CompressedChatImageResult(outputPath, Path.GetFileName(filePath), originalSize, compressedSize, isTemporaryFile: true);
        }

        private static Size CalculateTargetSize(int width, int height)
        {
            int longest = Math.Max(width, height);
            if (longest <= MaxImageDimension)
            {
                return new Size(width, height);
            }

            double ratio = MaxImageDimension / (double)longest;
            return new Size(Math.Max(1, (int)Math.Round(width * ratio)), Math.Max(1, (int)Math.Round(height * ratio)));
        }

        private static void SaveJpegWithQuality(Image image, string outputPath, long quality)
        {
            ImageCodecInfo? jpegCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.MimeType == "image/jpeg");
            if (jpegCodec == null)
            {
                image.Save(outputPath, ImageFormat.Jpeg);
                return;
            }

            using (var parameters = new EncoderParameters(1))
            {
                parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                image.Save(outputPath, jpegCodec, parameters);
            }
        }
    }
}
