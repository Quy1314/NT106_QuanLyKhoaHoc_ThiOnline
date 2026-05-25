using System;
using CourseGuard.Backend.Models;

namespace CourseGuard.Backend.Services
{
    public static class StudentExamAvailabilityService
    {
        public static bool CanStart(StudentExamListItemModel exam)
        {
            if (exam.InProgressAttemptCount > 0)
                return true;

            if (exam.QuestionCount <= 0)
                return false;

            DateTime now = DateTime.Now;
            if (exam.OpenTime.HasValue && exam.OpenTime.Value > now)
                return false;

            if (exam.CloseTime.HasValue && exam.CloseTime.Value < now)
                return false;

            return exam.RemainingAttempts > 0;
        }

        public static string GetStatusText(StudentExamListItemModel exam)
        {
            if (exam.InProgressAttemptCount > 0)
                return "Đang làm dở";

            if (exam.QuestionCount <= 0)
                return "Chưa có câu hỏi";

            DateTime now = DateTime.Now;
            if (exam.OpenTime.HasValue && exam.OpenTime.Value > now)
                return "Chưa đến giờ";

            if (exam.CloseTime.HasValue && exam.CloseTime.Value < now)
                return "Đã hết hạn";

            if (exam.RemainingAttempts <= 0)
                return "Hết lượt";

            return "Có thể làm";
        }

        public static string GetStartBlockedMessage(StudentExamListItemModel exam)
        {
            string status = GetStatusText(exam);
            return status switch
            {
                "Chưa có câu hỏi" => "Bài kiểm tra này chưa có câu hỏi. Vui lòng chờ giáo viên soạn nội dung.",
                "Chưa đến giờ" => "Bài kiểm tra này chưa đến giờ mở.",
                "Đã hết hạn" => "Bài kiểm tra này đã hết thời gian làm bài.",
                "Hết lượt" => "Bạn đã sử dụng hết lượt làm bài kiểm tra này.",
                _ => "Bài kiểm tra này chưa thể làm ở thời điểm hiện tại."
            };
        }
    }
}
