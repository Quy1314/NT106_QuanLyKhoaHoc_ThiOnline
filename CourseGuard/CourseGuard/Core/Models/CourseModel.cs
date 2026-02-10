/*
 * CourseModel.cs
 * 
 * Layer: Core
 * Vai trò: Định nghĩa đối tượng Khóa học (Course) với các thuộc tính như Name, Description, TeacherId, v.v.
 * Sử dụng: Truyền dữ liệu khóa học giữa Database, Service và UI.
 */
using System;

namespace CourseGuard.Core.Models
{
    public class CourseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty; // For display
        public string Status { get; set; } = "Active";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
