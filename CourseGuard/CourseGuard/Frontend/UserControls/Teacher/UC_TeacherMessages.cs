using System.Data;
using System.Threading.Tasks;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public partial class UC_TeacherMessages : TeacherGridPageBase
    {
        private readonly ChatController _chat = new(new CourseGuardDbContext(""));
        public UC_TeacherMessages(int teacherId) : base(teacherId, "Tin nhắn", "Truy cập các phòng chat khóa học thuộc quyền.", "Phòng chat khóa học", showCrud: false) { }
        protected override Task<DataTable> CreateTableAsync() => Task.Run(() => TeacherTabChrome.ToTable(
            new[] { "CourseId", "Khóa học", "Lớp", "Vai trò" },
            _chat.GetMyCourses(TeacherId),
            c => new object?[] { c.CourseId, c.CourseName, c.ClassCode, c.IsTeacherCourse ? "Giảng viên" : "Thành viên" }));
    }
}
