using System.Collections.Generic;
using CourseGuard.Backend.Models;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public partial class TeacherExamDialog : TeacherSimpleItemDialog
    {
        public TeacherExamDialog(IEnumerable<TeacherCourseModel> courses) : base("Bài kiểm tra", courses, status: WorkflowConstants.ExamStatus.Draft) { }
    }
}
