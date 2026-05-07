using System;
using System.IO;

namespace CourseGuard.Backend.Services
{
    public class ChatFileStorageService
    {
        private readonly string _storageRoot;

        public ChatFileStorageService(string? storageRoot = null)
        {
            _storageRoot = string.IsNullOrWhiteSpace(storageRoot)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage", "chat-files")
                : storageRoot;
        }

        public (string SavedPath, string SavedFileName, long FileSize) SaveChatFile(int courseId, string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("Khong tim thay file de tai len.", sourceFilePath);
            }

            string courseFolder = Path.Combine(_storageRoot, $"course-{courseId}");
            Directory.CreateDirectory(courseFolder);

            string originalName = Path.GetFileName(sourceFilePath);
            string fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}_{originalName}";
            string destination = Path.Combine(courseFolder, fileName);
            File.Copy(sourceFilePath, destination, true);

            var fileInfo = new FileInfo(destination);
            return (destination, originalName, fileInfo.Length);
        }
    }
}
