using System.Collections.Generic;
using System.Linq;

namespace CourseGuard.Frontend.Helpers
{
    public static class OverviewActionBuilder
    {
        public static OverviewActionItem[] BuildStudentActions(
            bool hasOpenExam,
            bool hasDeadlineSoon,
            bool hasClassToday,
            bool hasUnreadChat,
            bool hasUnreadNotification)
        {
            var actions = new List<OverviewActionItem>();

            if (hasOpenExam)
                actions.Add(new OverviewActionItem { Priority = 10, Title = "Làm bài thi đang mở", ActionText = "Làm bài", PageName = "Bài kiểm tra", Tone = "Warning" });
            if (hasDeadlineSoon)
                actions.Add(new OverviewActionItem { Priority = 20, Title = "Nộp bài tập sắp hết hạn", ActionText = "Nộp bài", PageName = "Bài tập", Tone = "Warning" });
            if (hasClassToday)
                actions.Add(new OverviewActionItem { Priority = 30, Title = "Vào lớp học hôm nay", ActionText = "Xem lịch", PageName = "Lịch học", Tone = "Neutral" });
            if (hasUnreadChat)
                actions.Add(new OverviewActionItem { Priority = 40, Title = "Đọc tin nhắn mới", ActionText = "Mở tin nhắn", PageName = "Tin nhắn", Tone = "Neutral" });
            if (hasUnreadNotification)
                actions.Add(new OverviewActionItem { Priority = 41, Title = "Xem thông báo mới", ActionText = "Mở thông báo", PageName = "Thông báo", Tone = "Neutral" });

            return actions.OrderBy(a => a.Priority).ToArray();
        }

        public static OverviewActionItem[] BuildTeacherActions(
            bool hasActiveExam,
            bool hasRequiredTask,
            bool hasUpcomingClass,
            bool hasUnreadChat,
            bool hasUnreadNotification)
        {
            var actions = new List<OverviewActionItem>();

            if (hasActiveExam)
                actions.Add(new OverviewActionItem { Priority = 10, Title = "Giám sát kỳ thi đang diễn ra", ActionText = "Giám sát", PageName = "Giám sát thi", Tone = "Warning" });
            if (hasRequiredTask)
                actions.Add(new OverviewActionItem { Priority = 20, Title = "Xử lý việc cần chú ý", ActionText = "Xử lý", PageName = "Bài tập", Tone = "Warning" });
            if (hasUpcomingClass)
                actions.Add(new OverviewActionItem { Priority = 30, Title = "Mở buổi dạy sắp tới", ActionText = "Xem lịch", PageName = "Lịch dạy", Tone = "Neutral" });
            if (hasUnreadChat)
                actions.Add(new OverviewActionItem { Priority = 40, Title = "Trả lời tin nhắn mới", ActionText = "Mở tin nhắn", PageName = "Tin nhắn", Tone = "Neutral" });
            if (hasUnreadNotification)
                actions.Add(new OverviewActionItem { Priority = 41, Title = "Xem thông báo mới", ActionText = "Mở thông báo", PageName = "Thông báo", Tone = "Neutral" });

            return actions.OrderBy(a => a.Priority).ToArray();
        }
    }
}
