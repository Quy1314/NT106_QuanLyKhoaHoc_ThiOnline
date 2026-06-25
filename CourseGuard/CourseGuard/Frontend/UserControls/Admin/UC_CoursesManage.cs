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
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public partial class UC_CoursesManage : UserControl
    {
        private readonly CourseGuard.Backend.Controllers.UserController _userService;
        private readonly CourseGuard.Backend.Controllers.CourseController _courseService;
        private int _selectedCourseId = -1;
        private bool _isBusy;
        private List<CourseGuard.Backend.Models.UserModel> _allStudents = new List<CourseGuard.Backend.Models.UserModel>();
        private readonly Button _btnApproveCourse = new Button();
        private readonly Button _btnRejectCourse = new Button();

        public UC_CoursesManage()
        {
            InitializeComponent();
            ApplyAcademicStyle();

            // Bo góc + cursor tay cho tất cả buttons
            CourseGuard.Frontend.Theme.RoundedButtonHelper.Apply(10,
                btnAddCourse, btnUpdateCourse, btnDeleteCourse,
                btnApproveStudent, btnRemoveStudent);

            var dbContext = new CourseGuard.Backend.Data.CourseGuardDbContext("");
            _userService = new CourseGuard.Backend.Controllers.UserController(dbContext);
            _courseService = new CourseGuard.Backend.Controllers.CourseController(dbContext);

            // Set up cboRegStatus items
            cboRegStatus.Items.Clear();
            cboRegStatus.Items.AddRange(new object[] { "Chờ duyệt (Pending)", "Đã tham gia (Approved)", "Tất cả học viên (All)" });
            cboRegStatus.SelectedIndex = 0; // Default to Pending
            cboStatus.Items.Clear();
            cboStatus.Items.AddRange(new object[] { WorkflowConstants.CourseStatus.Draft, WorkflowConstants.CourseStatus.Pending, WorkflowConstants.CourseStatus.Active, WorkflowConstants.CourseStatus.Rejected, WorkflowConstants.CourseStatus.Closed });
            SetupCourseApprovalButtons();

            WireEvents();
            // Initial load
            RefreshDataAsync().FireAndForgetSafe(this);
        }

        private void ApplyAcademicStyle()
        {
            BackColor = AcademicTheme.AppBackground;
            btnAddCourse.BackColor = AcademicTheme.Primary;
            btnAddCourse.ForeColor = Color.White;
            btnUpdateCourse.BackColor = AcademicTheme.Primary;
            btnUpdateCourse.ForeColor = Color.White;
            btnDeleteCourse.BackColor = Color.FromArgb(220, 38, 38);
            btnDeleteCourse.ForeColor = Color.White;
            
            btnApproveStudent.BackColor = AcademicTheme.Primary;
            btnApproveStudent.ForeColor = Color.White;
            
            btnRemoveStudent.BackColor = AcademicTheme.Surface;
            btnRemoveStudent.ForeColor = AcademicTheme.TextSecondary;
            btnRemoveStudent.FlatAppearance.BorderColor = AcademicTheme.BorderSoft;
            btnRemoveStudent.FlatAppearance.BorderSize = 1;
            
            lblSelectCourse.ForeColor = AcademicTheme.TextSecondary;
            lblStudent.ForeColor = AcademicTheme.TextSecondary;
            lblRegStatus.ForeColor = AcademicTheme.TextSecondary;
            
            AcademicTheme.StyleGrid(dgvCourses);
        }

        private void SetupCourseApprovalButtons()
        {
            grpCourseInfo.Height = 392;
            _btnApproveCourse.Text = "Duyệt";
            _btnApproveCourse.Location = new Point(20, 342);
            _btnApproveCourse.Size = new Size(125, 35);
            _btnApproveCourse.FlatStyle = FlatStyle.Flat;
            _btnApproveCourse.FlatAppearance.BorderSize = 0;
            _btnApproveCourse.Font = MetaTheme.Fonts.ButtonMd();
            _btnApproveCourse.BackColor = AcademicTheme.Primary;
            _btnApproveCourse.ForeColor = Color.White;

            _btnRejectCourse.Text = "Từ chối";
            _btnRejectCourse.Location = new Point(155, 342);
            _btnRejectCourse.Size = new Size(125, 35);
            _btnRejectCourse.FlatStyle = FlatStyle.Flat;
            _btnRejectCourse.FlatAppearance.BorderSize = 0;
            _btnRejectCourse.Font = MetaTheme.Fonts.ButtonMd();
            _btnRejectCourse.BackColor = Color.FromArgb(220, 38, 38);
            _btnRejectCourse.ForeColor = Color.White;

            RoundedButtonHelper.Apply(10, _btnApproveCourse, _btnRejectCourse);
            grpCourseInfo.Controls.Add(_btnApproveCourse);
            grpCourseInfo.Controls.Add(_btnRejectCourse);
        }

        private void WireEvents()
        {
            this.VisibleChanged += UC_CoursesManage_VisibleChanged;
            btnAddCourse.Click += btnAddCourse_Click;
            btnUpdateCourse.Click += btnUpdateCourse_Click;
            btnDeleteCourse.Click += btnDeleteCourse_Click;
            _btnApproveCourse.Click += btnApproveCourse_Click;
            _btnRejectCourse.Click += btnRejectCourse_Click;
            dgvCourses.CellClick += dgvCourses_CellClick;
            WireStudentApprovalEvents();
        }

        private async void UC_CoursesManage_VisibleChanged(object? sender, EventArgs e)
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
            this.ShowSkeleton(SkeletonType.FormWithTable);
            try
            {
                // Parallelize data loading since DbContext uses a separate connection per method call
                var teachersTask = Task.Run(() => _userService.GetByRole("TEACHER"));
                var studentsTask = Task.Run(() => _userService.GetByRole("STUDENT"));
                var coursesTask = Task.Run(() => _courseService.GetAllCourses());

                await Task.WhenAll(teachersTask, studentsTask, coursesTask);

                if (IsDisposed || Disposing) return;

                var teachers = await teachersTask;
                var students = await studentsTask;
                var courses = await coursesTask;

                _allStudents = students;
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
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isBusy = false;
                this.HideSkeleton();
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
            if (dgvCourses.Columns["TeacherId"] is DataGridViewColumn teacherIdColumn) teacherIdColumn.Visible = false;
            if (dgvCourses.Columns["CreatedAt"] is DataGridViewColumn createdAtColumn) createdAtColumn.Visible = false;

            // Headers (Optional customization)
#pragma warning disable CS8602
            if (dgvCourses.Columns["Id"] is DataGridViewColumn idColumn) idColumn.HeaderText = "ID";
            if (dgvCourses.Columns["Name"] != null) dgvCourses.Columns["Name"].HeaderText = "Tên Khóa Học";
            if (dgvCourses.Columns["Description"] != null) dgvCourses.Columns["Description"].HeaderText = "Mô Tả";
            if (dgvCourses.Columns["TeacherName"] != null) dgvCourses.Columns["TeacherName"].HeaderText = "Giáo Viên";
            if (dgvCourses.Columns["Status"] != null) dgvCourses.Columns["Status"].HeaderText = "Trạng Thái";
            if (dgvCourses.Columns["RejectionReason"] != null) dgvCourses.Columns["RejectionReason"].HeaderText = "Lý Do Từ Chối";
            if (dgvCourses.Columns["StartDate"] != null) dgvCourses.Columns["StartDate"].HeaderText = "Ngày Bắt Đầu";
            if (dgvCourses.Columns["EndDate"] != null) dgvCourses.Columns["EndDate"].HeaderText = "Ngày Kết Thúc";

#pragma warning restore CS8602

            // Reuse same source to avoid duplicate query
            cboSelectCourse.DataSource = courses.ToList();
            cboSelectCourse.DisplayMember = "Name";
            cboSelectCourse.ValueMember = "Id";
            cboSelectCourse.SelectedIndex = -1;
        }

        private void dgvCourses_CellClick(object? sender, DataGridViewCellEventArgs e)
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

        private async void btnAddCourse_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            var course = new CourseGuard.Backend.Models.CourseModel
            {
                Name = txtCourseName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                TeacherId = Convert.ToInt32(cboTeacher.SelectedValue),
                Status = cboStatus.Text, // "Active" or "Closed"
                StartDate = dtpStartDate.Value,
                EndDate = dtpEndDate.Value
            };

            try
            {
                string result = await Task.Run(() => _courseService.AddCourse(course));
                if (result == "Success")
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Thêm khóa học thành công!");
                    await RefreshDataAsync();
                }
                else if (result == "Forbidden")
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Bạn không có quyền thực hiện thao tác này.");
                }
                else
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi thêm khóa học: " + result);
                }
            }
            catch (Exception ex)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi thêm khóa học: " + ex.Message);
            }
        }

        private async void btnUpdateCourse_Click(object? sender, EventArgs e)
        {
            if (_selectedCourseId == -1)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn khóa học để cập nhật.");
                return;
            }
            if (!ValidateInputs()) return;

            var course = new CourseGuard.Backend.Models.CourseModel
            {
                Id = _selectedCourseId,
                Name = txtCourseName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                TeacherId = Convert.ToInt32(cboTeacher.SelectedValue),
                Status = cboStatus.Text,
                StartDate = dtpStartDate.Value,
                EndDate = dtpEndDate.Value
            };

            try
            {
                bool success = await Task.Run(() => _courseService.UpdateCourse(course));
                if (success)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Cập nhật khóa học thành công!");
                    await RefreshDataAsync();
                }
                else
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Cập nhật thất bại hoặc bạn không có quyền.");
                }
            }
            catch (Exception ex)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi cập nhật khóa học: " + ex.Message);
            }
        }

        private async void btnDeleteCourse_Click(object? sender, EventArgs e)
        {
            if (_selectedCourseId == -1)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn khóa học để xóa.");
                return;
            }

            var confirmResult = CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Bạn có chắc chắn muốn xóa khóa học này không?",
                                     "Xác nhận xóa",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    bool success = await Task.Run(() => _courseService.DeleteCourse(_selectedCourseId));
                    if (success)
                    {
                        CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Xóa khóa học thành công!");
                        await RefreshDataAsync();
                    }
                    else
                    {
                        CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Xóa thất bại hoặc bạn không có quyền.");
                    }
                }
                catch (Exception ex)
                {
                    CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi xóa khóa học: " + ex.Message);
                }
            }
        }

        private async void btnApproveCourse_Click(object? sender, EventArgs e)
        {
            if (_selectedCourseId <= 0)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn khóa học cần duyệt.");
                return;
            }

            bool ok = await Task.Run(() => _courseService.ApproveCourse(_selectedCourseId));
            CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(ok ? "Đã duyệt khóa học." : "Chỉ khóa học đang chờ duyệt mới có thể được duyệt.");
            await RefreshDataAsync();
        }

        private async void btnRejectCourse_Click(object? sender, EventArgs e)
        {
            if (_selectedCourseId <= 0)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn khóa học cần từ chối.");
                return;
            }

            string reason = PromptRejectReason();
            bool ok = await Task.Run(() => _courseService.RejectCourse(_selectedCourseId, reason));
            CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog(ok ? "Đã từ chối khóa học." : "Chỉ khóa học đang chờ duyệt mới có thể bị từ chối.");
            await RefreshDataAsync();
        }

        private static string PromptRejectReason()
        {
            using var form = new Form
            {
                Text = "Từ chối khóa học",
                Width = 420,
                Height = 180,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var input = new TextBox { Left = 16, Top = 42, Width = 370 };
            var label = new Label { Left = 16, Top = 16, Width = 370, Text = "Nhập lý do từ chối:" };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 214, Top = 82, Width = 80 };
            var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Left = 304, Top = 82, Width = 80 };
            form.Controls.Add(label);
            form.Controls.Add(input);
            form.Controls.Add(ok);
            form.Controls.Add(cancel);
            form.AcceptButton = ok;
            form.CancelButton = cancel;
            return form.ShowDialog() == DialogResult.OK ? input.Text.Trim() : string.Empty;
        }
        
        private void WireStudentApprovalEvents()
        {
            cboSelectCourse.SelectedIndexChanged += async (s, e) =>
            {
                if (cboSelectCourse.SelectedValue is int courseId)
                {
                    await RefreshStudentDropdown(courseId);
                }
            };

            cboRegStatus.SelectedIndexChanged += async (s, e) =>
            {
                if (cboSelectCourse.SelectedValue is int courseId)
                {
                    await RefreshStudentDropdown(courseId);
                }
            };

            btnApproveStudent.Click += async (s, e) =>
            {
                if (cboSelectCourse.SelectedValue is int courseId && cboStudent.SelectedValue is int studentId)
                {
                    if (studentId <= 0)
                    {
                        CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Không có học viên hợp lệ để thao tác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if there is an enrollment status for this student
                    string? status = await Task.Run(() => _courseService.GetEnrollmentStatus(courseId, studentId));
                    if (status == "PENDING")
                    {
                        bool ok = await Task.Run(() => _courseService.ApproveEnrollment(courseId, studentId));
                        if (ok)
                        {
                            CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Đã duyệt yêu cầu tham gia khóa học của học viên!");
                            await RefreshStudentDropdown(courseId);
                        }
                        else CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi duyệt học viên.");
                    }
                    else if (status == "ACTIVE" || status == "APPROVED")
                    {
                        CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Học viên này đã tham gia khóa học rồi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Directly enroll
                        bool ok = await Task.Run(() => _courseService.EnrollStudent(courseId, studentId));
                        if (ok)
                        {
                            CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Thêm học viên vào khóa học thành công!");
                            await RefreshStudentDropdown(courseId);
                        }
                        else CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi thêm học viên vào khóa học.");
                    }
                }
                else CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn khóa học và học viên.");
            };

            btnRemoveStudent.Click += async (s, e) =>
            {
                if (cboSelectCourse.SelectedValue is int courseId && cboStudent.SelectedValue is int studentId)
                {
                    if (studentId <= 0)
                    {
                        CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Không có học viên hợp lệ để thao tác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    bool ok = await Task.Run(() => _courseService.RejectEnrollment(courseId, studentId));
                    if (ok)
                    {
                        CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Đã xóa học viên khỏi khóa học / từ chối yêu cầu tham gia!");
                        await RefreshStudentDropdown(courseId);
                    }
                    else CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Lỗi xóa/từ chối học viên.");
                }
                else CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn khóa học và học viên.");
            };
        }

        private async Task RefreshStudentDropdown(int courseId)
        {
            if (cboRegStatus.SelectedIndex == -1)
            {
                cboRegStatus.SelectedIndex = 0;
                return;
            }

            string selectedStatus = cboRegStatus.SelectedItem?.ToString() ?? "Pending";
            
            try
            {
                if (selectedStatus.Contains("Pending") || selectedStatus.Contains("Chờ duyệt"))
                {
                    var pendings = await Task.Run(() => _courseService.GetPendingEnrollments(courseId));
                    
                    cboStudent.DataSource = null;
                    if (pendings.Count == 0)
                    {
                        var dummyList = new List<CourseGuard.Backend.Models.EnrollmentModel>
                        {
                            new CourseGuard.Backend.Models.EnrollmentModel { StudentId = -1, StudentName = "Không có yêu cầu chờ duyệt" }
                        };
                        cboStudent.DataSource = dummyList;
                        cboStudent.DisplayMember = "StudentName";
                        cboStudent.ValueMember = "StudentId";
                        cboStudent.SelectedIndex = 0;
                    }
                    else
                    {
                        cboStudent.DataSource = pendings;
                        cboStudent.DisplayMember = "StudentName";
                        cboStudent.ValueMember = "StudentId";
                        cboStudent.SelectedIndex = -1;
                    }
                }
                else if (selectedStatus.Contains("Approved") || selectedStatus.Contains("Đã tham gia"))
                {
                    var approveds = await Task.Run(() => _courseService.GetEnrollmentsByStatus(courseId, "ACTIVE"));
                    
                    cboStudent.DataSource = null;
                    if (approveds.Count == 0)
                    {
                        var dummyList = new List<CourseGuard.Backend.Models.EnrollmentModel>
                        {
                            new CourseGuard.Backend.Models.EnrollmentModel { StudentId = -1, StudentName = "Không có học viên đã tham gia" }
                        };
                        cboStudent.DataSource = dummyList;
                        cboStudent.DisplayMember = "StudentName";
                        cboStudent.ValueMember = "StudentId";
                        cboStudent.SelectedIndex = 0;
                    }
                    else
                    {
                        cboStudent.DataSource = approveds;
                        cboStudent.DisplayMember = "StudentName";
                        cboStudent.ValueMember = "StudentId";
                        cboStudent.SelectedIndex = -1;
                    }
                }
                else // "Tất cả học viên"
                {
                    cboStudent.DataSource = null;
                    if (_allStudents == null || _allStudents.Count == 0)
                    {
                        var dummyList = new List<CourseGuard.Backend.Models.UserModel>
                        {
                            new CourseGuard.Backend.Models.UserModel { Id = -1, FullName = "Không có học viên nào" }
                        };
                        cboStudent.DataSource = dummyList;
                        cboStudent.DisplayMember = "FullName";
                        cboStudent.ValueMember = "Id";
                        cboStudent.SelectedIndex = 0;
                    }
                    else
                    {
                        cboStudent.DataSource = _allStudents;
                        cboStudent.DisplayMember = "FullName";
                        cboStudent.ValueMember = "Id";
                        cboStudent.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtCourseName.Text))
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Tên khóa học không được để trống.");
                return false;
            }
            if (cboTeacher.SelectedValue == null)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Vui lòng chọn giáo viên.");
                return false;
            }
            if (dtpStartDate.Value > dtpEndDate.Value)
            {
                CourseGuard.Frontend.Theme.MetaTheme.ShowModernDialog("Ngày bắt đầu phải trước ngày kết thúc.");
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
