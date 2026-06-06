using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services;

namespace CourseGuard.Backend.Controllers
{
    public class ChatController
    {
        private readonly CourseGuardDbContext _dbContext;
        private readonly ChatFileStorageService _fileStorageService;
        public string LastErrorMessage { get; private set; } = string.Empty;

        public ChatController(CourseGuardDbContext dbContext)
        {
            _dbContext = dbContext;
            _fileStorageService = new ChatFileStorageService();
        }

        public List<ChatCourseModel> GetMyCourses(int userId)
        {
            if (userId <= 0)
            {
                return new List<ChatCourseModel>();
            }

            return _dbContext.GetChatCoursesForUser(userId);
        }

        public Task<List<ChatCourseModel>> GetMyCoursesAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
            {
                return Task.FromResult(new List<ChatCourseModel>());
            }

            return _dbContext.GetChatCoursesForUserAsync(userId, cancellationToken);
        }

        public int GetUnreadCount(int userId)
        {
            LastErrorMessage = string.Empty;
            return userId <= 0 ? 0 : _dbContext.GetUnreadChatCount(userId);
        }

        public void MarkAllRead(int userId)
        {
            LastErrorMessage = string.Empty;
            if (userId > 0)
            {
                _dbContext.MarkAllChatRead(userId);
            }
        }

        public void MarkCourseRead(int userId, int courseId)
        {
            LastErrorMessage = string.Empty;
            if (userId > 0 && courseId > 0)
            {
                _dbContext.MarkCourseChatRead(userId, courseId);
            }
        }

        public List<ChatMessageModel> GetMessages(int userId, int courseId, int limit = 100)
        {
            LastErrorMessage = string.Empty;
            if (!_dbContext.CanAccessCourseChat(userId, courseId))
            {
                LastErrorMessage = "Bạn không có quyền truy cập phòng chat này.";
                return new List<ChatMessageModel>();
            }

            return _dbContext.GetChatMessages(courseId, limit);
        }

        public async Task<List<ChatMessageModel>> GetMessagesAsync(int userId, int courseId, int limit = 100, CancellationToken cancellationToken = default)
        {
            LastErrorMessage = string.Empty;
            if (!_dbContext.CanAccessCourseChat(userId, courseId))
            {
                LastErrorMessage = "Bạn không có quyền truy cập phòng chat này.";
                return new List<ChatMessageModel>();
            }

            return await _dbContext.GetChatMessagesAsync(courseId, limit, cancellationToken);
        }

        public bool SendMessage(int userId, int courseId, string content)
        {
            LastErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(content))
            {
                LastErrorMessage = "Nội dung tin nhắn không được để trống.";
                return false;
            }

            if (!_dbContext.CanAccessCourseChat(userId, courseId))
            {
                LastErrorMessage = "Bạn không có quyền gửi tin nhắn trong phòng chat này.";
                return false;
            }

            int messageId = _dbContext.SendChatMessage(courseId, userId, content.Trim());
            if (messageId <= 0)
            {
                LastErrorMessage = "Gửi tin nhắn thất bại.";
                return false;
            }

            _dbContext.LogUserActivity(userId, "CHAT_USE", $"Gửi tin nhắn chat trong khóa học ID={courseId}", string.Empty);
            return true;
        }

        public async Task<bool> SendMessageAsync(int userId, int courseId, string content, CancellationToken cancellationToken = default)
        {
            LastErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(content))
            {
                LastErrorMessage = "Nội dung tin nhắn không được để trống.";
                return false;
            }

            if (!_dbContext.CanAccessCourseChat(userId, courseId))
            {
                LastErrorMessage = "Bạn không có quyền gửi tin nhắn trong phòng chat này.";
                return false;
            }

            int messageId = await _dbContext.SendChatMessageAsync(courseId, userId, content.Trim(), cancellationToken);
            if (messageId <= 0)
            {
                LastErrorMessage = "Gửi tin nhắn thất bại.";
                return false;
            }

            await _dbContext.LogUserActivityAsync(userId, "CHAT_USE", $"Gửi tin nhắn chat trong khóa học ID={courseId}", string.Empty, cancellationToken);
            return true;
        }

        public bool SendFileMessage(int userId, int courseId, string sourceFilePath, string caption = "")
        {
            LastErrorMessage = string.Empty;
            if (!_dbContext.CanAccessCourseChat(userId, courseId))
            {
                LastErrorMessage = "Bạn không có quyền gửi file trong phòng chat này.";
                return false;
            }

            if (!ValidateFile(sourceFilePath, out string validationError))
            {
                LastErrorMessage = validationError;
                return false;
            }

            var saved = _fileStorageService.SaveChatFile(courseId, sourceFilePath);
            string mimeType = ResolveMimeType(Path.GetExtension(saved.SavedFileName));
            int messageId = _dbContext.SendChatFileMessage(
                courseId,
                userId,
                caption,
                saved.SavedPath,
                saved.SavedFileName,
                saved.FileSize,
                mimeType);

            if (messageId <= 0)
            {
                LastErrorMessage = "Gửi file thất bại.";
                return false;
            }

            _dbContext.LogUserActivity(userId, "CHAT_USE", $"Gửi file chat trong khóa học ID={courseId}: {saved.SavedFileName}", string.Empty);
            return true;
        }

        public Task<bool> SendFileMessageAsync(int userId, int courseId, string sourceFilePath, string caption = "", CancellationToken cancellationToken = default)
        {
            // Keep implementation simple and stable for WinForms flow.
            return Task.Run(() => SendFileMessage(userId, courseId, sourceFilePath, caption), cancellationToken);
        }

        private static bool ValidateFile(string sourceFilePath, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                error = "Không tìm thấy file để gửi.";
                return false;
            }

            var fileInfo = new FileInfo(sourceFilePath);
            const long maxSize = 20 * 1024 * 1024; // 20MB
            if (fileInfo.Length <= 0)
            {
                error = "File rỗng, không thể gửi.";
                return false;
            }

            if (fileInfo.Length > maxSize)
            {
                error = "File vuot qua gioi han 20MB.";
                return false;
            }

            string ext = Path.GetExtension(fileInfo.Name).ToLowerInvariant();
            string[] allowed = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".png", ".jpg", ".jpeg", ".txt", ".zip", ".rar" };
            if (Array.IndexOf(allowed, ext) < 0)
            {
                error = "Dinh dang file chua duoc ho tro.";
                return false;
            }

            return true;
        }

        private static string ResolveMimeType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                _ => "application/octet-stream"
            };
        }
    }
}
