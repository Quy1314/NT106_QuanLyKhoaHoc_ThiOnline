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
using System.Text;
using System.Windows.Forms;

namespace CourseGuard.Presentation.UserControls.Admin
{
    public partial class UC_CoursesManage : UserControl
    {
        private readonly CourseGuard.Application.Interfaces.IUserService _userService;
        private readonly CourseGuard.Application.Interfaces.ICourseService _courseService;
        private int _selectedCourseId = -1;

        public UC_CoursesManage()
        {
            InitializeComponent();
            
            // Manual injection
            var userRepository = new CourseGuard.Infrastructure.Data.Repositories.UserRepository();
            var courseRepository = new CourseGuard.Infrastructure.Data.Repositories.CourseRepository();

            _userService = new CourseGuard.Application.Services.UserService(userRepository);
            _courseService = new CourseGuard.Application.Services.CourseService(courseRepository);

            WireEvents();
            // Initial load
            RefreshData();
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

        private void UC_CoursesManage_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                RefreshData();
            }
        }

        private void RefreshData()
        {
            LoadTeachers();
            LoadCourses();
            LoadStudents();
            // LoadStudentCourses is called inside LoadCourses
        }

        private void LoadTeachers()
        {
            try
            {
                var teachers = _userService.GetByRole("TEACHER"); // Changed from GetTeachers() if it didn't exist, assume GetByRole works
                cboTeacher.DataSource = teachers;
                cboTeacher.DisplayMember = "FullName";
                cboTeacher.ValueMember = "Id";
                cboTeacher.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách giáo viên: " + ex.Message);
            }
        }
        
        private void LoadStudents()
        {
             try
            {
                var students = _userService.GetByRole("STUDENT");
                cboStudent.DataSource = students;
                cboStudent.DisplayMember = "FullName";
                cboStudent.ValueMember = "Id";
                cboStudent.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách học viên: " + ex.Message);
            }
        }

        private void LoadStudentCourses()
        {
            try
            {
                // Re-fetch to ensure fresh data for dropdown
                var courses = _courseService.GetAllCourses();
                cboSelectCourse.DataSource = courses;
                cboSelectCourse.DisplayMember = "Name";
                cboSelectCourse.ValueMember = "Id";
                cboSelectCourse.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                 MessageBox.Show("Lỗi tải danh sách khóa học (Dropdown): " + ex.Message);
            }
        }

        private void LoadCourses()
        {
            try
            {
                var courses = _courseService.GetAllCourses();
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

                ClearInputs();
                
                // Sync Dropdown
                LoadStudentCourses(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách khóa học: " + ex.Message);
            }
        }

        private void dgvCourses_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRow = dgvCourses.Rows[e.RowIndex];
                var course = selectedRow.DataBoundItem as CourseGuard.Core.Models.CourseModel;

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

        private void btnAddCourse_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            var course = new CourseGuard.Core.Models.CourseModel
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
                _courseService.AddCourse(course);
                MessageBox.Show("Thêm khóa học thành công!");
                LoadCourses(); // This will also reload cboSelectCourse via LoadStudentCourses
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm khóa học: " + ex.Message);
            }
        }

        private void btnUpdateCourse_Click(object sender, EventArgs e)
        {
            if (_selectedCourseId == -1)
            {
                MessageBox.Show("Vui lòng chọn khóa học để cập nhật.");
                return;
            }
            if (!ValidateInputs()) return;

            var course = new CourseGuard.Core.Models.CourseModel
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
                bool success = _courseService.UpdateCourse(course);
                if (success)
                {
                    MessageBox.Show("Cập nhật khóa học thành công!");
                    LoadCourses();
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

        private void btnDeleteCourse_Click(object sender, EventArgs e)
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
                    bool success = _courseService.DeleteCourse(_selectedCourseId);
                    if (success)
                    {
                        MessageBox.Show("Xóa khóa học thành công!");
                        LoadCourses();
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
        
        private void btnAddStudent_Click(object sender, EventArgs e)
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
                bool success = _courseService.EnrollStudent(courseId, studentId);
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
