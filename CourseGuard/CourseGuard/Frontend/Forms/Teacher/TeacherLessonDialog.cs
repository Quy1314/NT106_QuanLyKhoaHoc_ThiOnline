using System.Collections.Generic;
using CourseGuard.Backend.Models;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class TeacherLessonDialog : TeacherSimpleItemDialog
    {
        public TeacherLessonDialog(IEnumerable<TeacherCourseModel> courses) : base("Bài học", courses, status: "DRAFT") { }
    }
}
