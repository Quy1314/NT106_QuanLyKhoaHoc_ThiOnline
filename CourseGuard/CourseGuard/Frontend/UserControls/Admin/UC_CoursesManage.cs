/*
 * UC_CoursesManage.cs
 * 
 * Layer: Presentation (UserControls)
 * Vai trò: Màn hình quản lý khóa học. Hiển thị danh sách khóa học (kèm tên giáo viên), thêm/xóa/sửa khóa học.
 * Phụ thuộc: CourseService, UserService (để lấy danh sách giáo viên).
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public partial class UC_CoursesManage : UserControl
    {
        private readonly CourseGuard.Backend.Controllers.UserController _userService;
        private readonly CourseGuard.Backend.Controllers.CourseController _courseService;
        private int _selectedCourseId = -1;
        private bool _isBusy;

        public UC_CoursesManage()
        {
            InitializeComponent();
            
            _userService = new CourseGuard.Backend.Controllers.UserController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));
            _courseService = new CourseGuard.Backend.Controllers.CourseController(new CourseGuard.Backend.Data.CourseGuardDbContext(""));

            WireEvents();
            // Initial load
            _ = RefreshDataAsync();
        }

        private void WireEvents()
        {
            this.VisibleChanged += UC_CoursesManage_VisibleChanged;
            btnAddCourse.Click += btnAddCourse_Click;
            btnUpdateCourse.Click += btnUpdateCourse_Click;
            btnDeleteCourse.Click += btnDeleteCourse_Click;
            dgvCourses.CellClick += dgvCourses_CellClick;
            btnAddStudent.Click += btnAddStudent_Click;
        }

        private async void UC_CoursesManage_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                await RefreshDataAsync();
            }
        }

        private async Task RefreshDataAsync()
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                // Avoid parallel loading on shared services to prevent race/dispose issues.
                var teachers = await Task.Run(() => _userService.GetByRole("TEACHER"));
                var students = await Task.Run(() => _userService.GetByRole("STUDENT"));
                var courses = await Task.Run(() => _courseService.GetAllCourses());

                if (IsDisposed || Disposing) return;

                BindTeachers(teachers);
                BindStudents(students);
                BindCourses(courses);
                ClearInputs();
            }
            catch (ObjectDisposedException)
            {
                // Ignore when control is closed while background load is running.
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void BindTeachers(List<CourseGuard.Backend.Models.UserModel> teachers)
        {
            cboTeacher.DataSource = teachers;
            cboTeacher.DisplayMember = "FullName";
            cboTeacher.ValueMember = "Id";
            cboTeacher.SelectedIndex = -1;
        }
        
        private void BindStudents(List<CourseGuard.Backend.Models.UserModel> students)
        {
            cboStudent.DataSource = students;
            cboStudent.DisplayMember = "FullName";
            cboStudent.ValueMember = "Id";
            cboStudent.SelectedIndex = -1;
        }

        private void BindCourses(List<CourseGuard.Backend.Models.CourseModel> courses)
        {
            dgvCourses.DataSource = courses;

            // Adjust columns if needed
            if (dgvCourses.Columns["TeacherId"] != null) dgvCourses.Columns["TeacherId"].Visible = false;
            if (dgvCourses.Columns["CreatedAt"] != null) dgvCourses.Columns["CreatedAt"].Visible = false;

            // Headers (Optional customization)
            if (dgvCourses.Columns["Id"] != null) dgvCourses.Columns["Id"].HeaderText = "ID";
            if (dgvCourses.Columns["Name"] != null) dgvCourses.Columns["Name"].HeaderText = "Tên Khóa Học";
            if (dgvCourses.Columns["Description"] != null) dgvCourses.Columns["Description"].HeaderText = "Mô Tả";
            if (dgvCourses.Columns["TeacherName"] != null) dgvCourses.Columns["TeacherName"].HeaderText = "Giáo Viên";
            if (dgvCourses.Columns["Status"] != null) dgvCourses.Columns["Status"].HeaderText = "Trạng Thái";
            if (dgvCourses.Columns["StartDate"] != null) dgvCourses.Columns["StartDate"].HeaderText = "Ngày Bắt Đầu";
            if (dgvCourses.Columns["EndDate"] != null) dgvCourses.Columns["EndDate"].HeaderText = "Ngày Kết Thúc";

            // Reuse same source to avoid duplicate query
            cboSelectCourse.DataSource = courses.ToList();
            cboSelectCourse.DisplayMember = "Name";
            cboSelectCourse.ValueMember = "Id";
            cboSelectCourse.SelectedIndex = -1;
        }

        private void dgvCourses_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRow = dgvCourses.Rows[e.RowIndex];
                var course = selectedRow.DataBoundItem as CourseGuard.Backend.Models.CourseModel;

                if (course != null)
                {
                    _selectedCourseId = course.Id;
                    txtCourseName.Text = course.Name;
                    txtDescription.Text = course.Description;
                    try 
                    { 
                        cboTeacher.SelectedValue = course.TeacherId; 
                    } 
                    catch { /* If teacher deleted/inactive, handle gracefully */ }
                    
                    cboStatus.Text = course.Status;
                    dtpStartDate.Value = course.StartDate != DateTime.MinValue ? course.StartDate : DateTime.Now;
                    dtpEndDate.Value = course.EndDate != DateTime.MinValue ? course.EndDate : DateTime.Now;
                }
            }
        }

        private async void btnAddCourse_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            var course = new CourseGuard.Backend.Models.CourseModel
            {
                Name = txtCourseName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                TeacherId = (int)cboTeacher.SelectedValue,
                Status = cboStatus.Text, // "Active" or "Closed"
                StartDate = dtpStartDate.Value,
                EndDate = dtpEndDate.Value
            };

            try
            {
                await Task.Run(() => _courseService.AddCourse(course));
                MessageBox.Show("Thêm khóa học thành công!");
                await RefreshDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm khóa học: " + ex.Message);
            }
        }

        private async void btnUpdateCourse_Click(object sender, EventArgs e)
        {
            if (_selectedCourseId == -1)
            {
                MessageBox.Show("Vui lòng chọn khóa học để cập nhật.");
                return;
            }
            if (!ValidateInputs()) return;

            var course = new CourseGuard.Backend.Models.CourseModel
            {
                Id = _selectedCourseId,
                Name = txtCourseName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                TeacherId = (int)cboTeacher.SelectedValue,
                Status = cboStatus.Text,
                StartDate = dtpStartDate.Value,
                EndDate = dtpEndDate.Value
            };

            try
            {
                bool success = await Task.Run(() => _courseService.UpdateCourse(course));
                if (success)
                {
                    MessageBox.Show("Cập nhật khóa học thành công!");
                    await RefreshDataAsync();
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật khóa học: " + ex.Message);
            }
        }

        private async void btnDeleteCourse_Click(object sender, EventArgs e)
        {
            if (_selectedCourseId == -1)
            {
                MessageBox.Show("Vui lòng chọn khóa học để xóa.");
                return;
            }

            var confirmResult = MessageBox.Show("Bạn có chắc chắn muốn xóa khóa học này không?",
                                     "Xác nhận xóa",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    bool success = await Task.Run(() => _courseService.DeleteCourse(_selectedCourseId));
                    if (success)
                    {
                        MessageBox.Show("Xóa khóa học thành công!");
                        await RefreshDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa khóa học: " + ex.Message);
                }
            }
        }
        
        private async void btnAddStudent_Click(object sender, EventArgs e)
        {
            if (cboSelectCourse.SelectedValue == null)
            {
                 MessageBox.Show("Vui lòng chọn khóa học.");
                 return;
            }
            if (cboStudent.SelectedValue == null)
            {
                 MessageBox.Show("Vui lòng chọn học viên.");
                 return;
            }

            int courseId = (int)cboSelectCourse.SelectedValue;
            int studentId = (int)cboStudent.SelectedValue;

            try
            {
                bool success = await Task.Run(() => _courseService.EnrollStudent(courseId, studentId));
                 if (success)
                {
                    MessageBox.Show("Thêm học viên vào khóa học thành công!");
                    // potentially reload list of students in course if there was a grid for that
                }
                else
                {
                    MessageBox.Show("Học viên đã tham gia khóa học này hoặc có lỗi xảy ra.");
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show("Lỗi thêm học viên: " + ex.Message);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtCourseName.Text))
            {
                MessageBox.Show("Tên khóa học không được để trống.");
                return false;
            }
            if (cboTeacher.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn giáo viên.");
                return false;
            }
            if (dtpStartDate.Value > dtpEndDate.Value)
            {
                MessageBox.Show("Ngày bắt đầu phải trước ngày kết thúc.");
                return false;
            }
            return true;
        }

        private void ClearInputs()
        {
            _selectedCourseId = -1;
            txtCourseName.Clear();
            txtDescription.Clear();
            cboTeacher.SelectedIndex = -1;
            cboStatus.SelectedIndex = -1;
            dtpStartDate.Value = DateTime.Now;
            dtpEndDate.Value = DateTime.Now.AddMonths(1);
        }
    }
}
