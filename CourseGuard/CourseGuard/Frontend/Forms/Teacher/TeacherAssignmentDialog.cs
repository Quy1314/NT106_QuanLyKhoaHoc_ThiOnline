using System.Collections.Generic;
using CourseGuard.Backend.Models;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class TeacherAssignmentDialog : TeacherSimpleItemDialog
    {
        public TeacherAssignmentDialog(IEnumerable<TeacherCourseModel> courses) : base("Bài tập", courses, status: "OPEN") { }
    }
}
