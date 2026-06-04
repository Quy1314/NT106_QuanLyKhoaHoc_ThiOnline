using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Services;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public class StudentAssignmentSubmitDialog : Form
    {
        private readonly CourseGuardDbContext _dbContext;
        private readonly StudentAssignmentRow _assignment;
        private readonly int _studentId;

        private Label lblTitle = null!;
        private Label lblCourse = null!;
        private Label lblDueDate = null!;
        private Label lblDescriptionTitle = null!;
        private TextBox txtDescription = null!;
        private Label lblDownloadTitle = null!;
        private Button btnDownloadTeacherFile = null!;

        private Label lblUploadTitle = null!;
        private TextBox txtSelectedFile = null!;
        private Button btnSelectFile = null!;
        private Button btnSubmit = null!;
        private Button btnCancel = null!;

        private string? _selectedFilePath;
        private byte[]? _selectedFileContent;

        public StudentAssignmentSubmitDialog(CourseGuardDbContext dbContext, StudentAssignmentRow assignment, int studentId)
        {
            _dbContext = dbContext;
            _assignment = assignment;
            _studentId = studentId;

            InitializeComponent();
            ApplyStyle();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Nộp bài tập";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label { Location = new Point(20, 20), AutoSize = true, Font = MetaTheme.Fonts.HeadingMd() };
            lblCourse = new Label { Location = new Point(20, 50), AutoSize = true, Font = MetaTheme.Fonts.BodyMd() };
            lblDueDate = new Label { Location = new Point(20, 75), AutoSize = true, Font = MetaTheme.Fonts.BodyMdBold(), ForeColor = Color.Red };

            lblDescriptionTitle = new Label { Location = new Point(20, 105), AutoSize = true, Font = MetaTheme.Fonts.BodyMdBold(), Text = "Mô tả đề bài:" };

            txtDescription = new TextBox
            {
                Location = new Point(20, 130),
                Size = new Size(540, 75),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = MetaTheme.Fonts.BodyMd()
            };

            lblDownloadTitle = new Label { Location = new Point(20, 215), AutoSize = true, Font = MetaTheme.Fonts.BodyMdBold(), Text = "Download bài tập:" };

            btnDownloadTeacherFile = new Button
            {
                Location = new Point(20, 240),
                Size = new Size(200, 35),
                Text = "Tải đề bài đính kèm",
                Font = MetaTheme.Fonts.BodyMd(),
                FlatStyle = FlatStyle.Flat
            };
            btnDownloadTeacherFile.Click += BtnDownloadTeacherFile_Click;

            lblUploadTitle = new Label { Location = new Point(20, 285), AutoSize = true, Font = MetaTheme.Fonts.BodyMdBold(), Text = "Bài làm của bạn (Tối đa 10MB):" };

            txtSelectedFile = new TextBox
            {
                Location = new Point(20, 315),
                Size = new Size(410, 30),
                ReadOnly = true,
                Font = MetaTheme.Fonts.BodyMd()
            };

            btnSelectFile = new Button
            {
                Location = new Point(440, 313),
                Size = new Size(120, 32),
                Text = "Chọn file...",
                Font = MetaTheme.Fonts.BodyMd(),
                FlatStyle = FlatStyle.Flat
            };
            btnSelectFile.Click += BtnSelectFile_Click;

            btnSubmit = new Button
            {
                Location = new Point(340, 395),
                Size = new Size(110, 40),
                Text = "Nộp bài",
                Font = MetaTheme.Fonts.ButtonMd(),
                FlatStyle = FlatStyle.Flat
            };
            btnSubmit.Click += async (s, e) => await BtnSubmit_Click();

            btnCancel = new Button
            {
                Location = new Point(460, 395),
                Size = new Size(100, 40),
                Text = "Đóng",
                Font = MetaTheme.Fonts.BodyMd(),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblCourse);
            this.Controls.Add(lblDueDate);
            this.Controls.Add(lblDescriptionTitle);
            this.Controls.Add(txtDescription);
            this.Controls.Add(lblDownloadTitle);
            this.Controls.Add(btnDownloadTeacherFile);
            this.Controls.Add(lblUploadTitle);
            this.Controls.Add(txtSelectedFile);
            this.Controls.Add(btnSelectFile);
            this.Controls.Add(btnSubmit);
            this.Controls.Add(btnCancel);
        }

        private void ApplyStyle()
        {
            this.BackColor = AppColors.BgBase;
            lblTitle.ForeColor = AppColors.TextPrimary;
            lblCourse.ForeColor = AppColors.TextSecondary;
            lblDescriptionTitle.ForeColor = AppColors.TextPrimary;
            lblDownloadTitle.ForeColor = AppColors.TextPrimary;
            lblUploadTitle.ForeColor = AppColors.TextPrimary;

            txtDescription.BackColor = AppColors.BgCard;
            txtDescription.ForeColor = AppColors.TextPrimary;
            txtDescription.BorderStyle = BorderStyle.FixedSingle;

            txtSelectedFile.BackColor = AppColors.BgCard;
            txtSelectedFile.ForeColor = AppColors.TextPrimary;

            btnDownloadTeacherFile.BackColor = AppColors.BgCard;
            btnDownloadTeacherFile.ForeColor = AppColors.TextPrimary;
            btnDownloadTeacherFile.FlatAppearance.BorderColor = AppColors.Border;

            btnSelectFile.BackColor = AppColors.BgCard;
            btnSelectFile.ForeColor = AppColors.TextPrimary;
            btnSelectFile.FlatAppearance.BorderColor = AppColors.Border;

            btnSubmit.BackColor = AppColors.AccentBlue;
            btnSubmit.ForeColor = Color.White;
            btnSubmit.FlatAppearance.BorderSize = 0;

            btnCancel.BackColor = AppColors.BgCard;
            btnCancel.ForeColor = AppColors.TextPrimary;
            btnCancel.FlatAppearance.BorderColor = AppColors.Border;            
            if (_assignment.Status != "OPEN")
            {
                btnSubmit.Enabled = false;
                btnSelectFile.Enabled = false;
                btnSubmit.BackColor = Color.Gray;
                btnSubmit.Text = "Đã đóng";
            }
        }

        private void LoadData()
        {
            lblTitle.Text = _assignment.Title;
            lblCourse.Text = $"Khóa học: {_assignment.CourseName}";
            lblDueDate.Text = $"Hạn nộp: {_assignment.DueDate:dd/MM/yyyy HH:mm}";
            
            int currentY = 105;

            if (string.IsNullOrWhiteSpace(_assignment.Description))
            {
                lblDescriptionTitle.Visible = false;
                txtDescription.Visible = false;
            }
            else
            {
                lblDescriptionTitle.Top = currentY;
                txtDescription.Top = currentY + 25;
                txtDescription.Text = _assignment.Description;
                currentY += 110;
            }

            if (_assignment.HasTeacherFile)
            {
                lblDownloadTitle.Visible = true;
                btnDownloadTeacherFile.Visible = true;
                btnDownloadTeacherFile.Enabled = true;
                lblDownloadTitle.Top = currentY;
                btnDownloadTeacherFile.Top = currentY + 25;
                btnDownloadTeacherFile.Text = $"Tải đính kèm ({_assignment.TeacherFileName})";
                currentY += 70;
            }
            else
            {
                lblDownloadTitle.Visible = false;
                btnDownloadTeacherFile.Visible = false;
            }

            lblUploadTitle.Top = currentY;
            txtSelectedFile.Top = currentY + 30;
            btnSelectFile.Top = currentY + 28;
            currentY += 80;

            btnSubmit.Top = currentY;
            btnCancel.Top = currentY;
            
            // Adjust the window height based on the final currentY + the button height (40) + padding (50)
            this.Height = currentY + 90;

            if (_assignment.IsSubmitted)
            {
                txtSelectedFile.Text = _assignment.StudentFileName;
                btnSubmit.Text = "Nộp lại";
                lblUploadTitle.Text = $"Đã nộp lúc: {_assignment.SubmittedAt:dd/MM/yyyy HH:mm}";
            }
        }

        private async void BtnDownloadTeacherFile_Click(object? sender, EventArgs e)
        {
            try
            {
                btnDownloadTeacherFile.Enabled = false;
                byte[]? content = await _dbContext.GetAssignmentContentAsync(_assignment.AssignmentId, _studentId);
                
                if (content == null || content.Length == 0)
                {
                    MetaTheme.ShowModernDialog("Không thể tải file đính kèm từ máy chủ.", "Lỗi");
                    return;
                }

                using var sfd = new SaveFileDialog
                {
                    FileName = _assignment.TeacherFileName,
                    Filter = "All Files (*.*)|*.*"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    await File.WriteAllBytesAsync(sfd.FileName, content);
                    MetaTheme.ShowModernDialog("Tải file thành công!", "Thông báo");
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        });
                    }
                    catch { /* Ignore if no default app */ }
                }
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog($"Lỗi khi tải file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDownloadTeacherFile.Enabled = true;
            }
        }

        private void BtnSelectFile_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn bài làm",
                Filter = "Tài liệu (*.pdf;*.doc;*.docx;*.txt)|*.pdf;*.doc;*.docx;*.txt|All files (*.*)|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var fileInfo = new FileInfo(ofd.FileName);
                if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    MetaTheme.ShowModernDialog("File bài làm không được vượt quá 10MB.", "Cảnh báo");
                    return;
                }

                _selectedFilePath = ofd.FileName;
                txtSelectedFile.Text = fileInfo.Name;
                _selectedFileContent = File.ReadAllBytes(ofd.FileName);
            }
        }

        private async Task BtnSubmit_Click()
        {
            if (_selectedFileContent == null && !_assignment.IsSubmitted)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn file bài làm trước khi nộp.", "Cảnh báo");
                return;
            }

            if (_selectedFileContent == null && _assignment.IsSubmitted)
            {
                // Already submitted, but no new file selected
                MetaTheme.ShowModernDialog("Bạn chưa chọn file mới để cập nhật bài làm.", "Thông báo");
                return;
            }

            try
            {
                btnSubmit.Enabled = false;
                btnSubmit.Text = "Đang nộp...";

                var submission = new AssignmentSubmissionModel
                {
                    AssignmentId = _assignment.AssignmentId,
                    StudentId = _studentId,
                    FileName = Path.GetFileName(_selectedFilePath) ?? "submission.txt",
                    ContentType = Path.GetExtension(_selectedFilePath) ?? "",
                    FileSize = _selectedFileContent!.Length,
                    FileContent = _selectedFileContent
                };

                bool success = await _dbContext.SubmitAssignmentAsync(submission);

                if (success)
                {
                    // Update in-app notification
                    var notifRepo = new NotificationRepository();
                    notifRepo.Create(
                        userId: _studentId,
                        title: "Nộp bài thành công",
                        content: $"Bạn đã nộp bài làm cho '{_assignment.Title}'.",
                        category: WorkflowConstants.NotificationCategory.Assignment,
                        notificationType: WorkflowConstants.NotificationType.Informational,
                        sourceType: "ASSIGNMENT",
                        sourceId: _assignment.AssignmentId
                    );

                    // Get student email via DB or session, fallback to generic
                    string studentEmail = UserSessionContext.CurrentUsername + "@student.edu.vn";
                    string assignmentTitle = _assignment.Title;

                    Task.Run(() =>
                    {
                        try
                        {
                            var emailService = new SmtpEmailService();
                            string emailBody = $"Chào bạn,\n\nBạn đã nộp bài làm thành công cho bài tập '{assignmentTitle}'.\n\nTrân trọng,\nCourseGuard";
                            emailService.SendEmail(studentEmail, "Xác nhận nộp bài tập: " + assignmentTitle, emailBody, out _);
                        }
                        catch
                        {
                            // Ignore SMTP errors so it doesn't crash the app
                        }
                    }).FireAndForgetSafe(this);

                    MetaTheme.ShowModernDialog("Nộp bài thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MetaTheme.ShowModernDialog("Nộp bài thất bại. Vui lòng thử lại sau.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnSubmit.Enabled = true;
                    btnSubmit.Text = "Nộp bài";
                }
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog($"Lỗi khi nộp bài: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSubmit.Enabled = true;
                btnSubmit.Text = "Nộp bài";
            }
        }
    }
}
