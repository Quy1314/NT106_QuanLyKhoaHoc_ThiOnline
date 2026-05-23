using System;

namespace CourseGuard.Backend.Services.Monitoring
{
    public class ScreenFrameReceivedEventArgs : EventArgs
    {
        public ScreenFrameReceivedEventArgs(int examId, int studentId, int attemptId, byte[] jpegBytes)
        {
            ExamId = examId;
            StudentId = studentId;
            AttemptId = attemptId;
            JpegBytes = jpegBytes;
        }

        public int ExamId { get; }
        public int StudentId { get; }
        public int AttemptId { get; }
        public byte[] JpegBytes { get; }
    }
}
