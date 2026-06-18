using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Backend.Security;
using CourseGuard.Backend.Services.Classroom;
using CourseGuard.Frontend.Extensions;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Student
{
    public sealed class StudentNativeClassroomForm : Form
    {
        private readonly int _sessionId;
        private readonly string _sessionName;
        private readonly TcpClassroomClient _client = new();
        private readonly CourseGuardDbContext _dbContext = new("");
        private int _attendanceLogId;
        private Task? _attendanceInTask;
        private Label _statusLabel = null!;
        private Label _mediaStateLabel = null!;
        private ListBox _eventsList = null!;
        private PictureBox _teacherVideo = null!;
        private PictureBox _studentPreview = null!;
        private Label _videoPlaceholder = null!;
        private Button _btnCamera = null!;
        private Button _btnMic = null!;
        private Button _btnShareScreen = null!;
        private Panel _teacherCameraPip = null!;
        private PictureBox _teacherCameraPipPreview = null!;
        private ListBox _chatList = null!;
        private TextBox _chatInput = null!;
        private Button _btnSendChat = null!;

        private ClassroomCameraManager _cameraManager = null!;
        private ClassroomScreenShareManager _screenShareManager = null!;
        private bool _isConnectedToClassroom;
        private bool _isCameraOn;
        private bool _isMicOn = true;
        private bool _isSharingScreen;
        private bool _hasActiveLiveStatus;
        private bool _isClosing;
        private bool _cleanupComplete;

        public StudentNativeClassroomForm(int sessionId, string sessionName)
        {
            _sessionId = sessionId;
            _sessionName = sessionName;
            Text = "CourseGuard Classroom - Học sinh";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(940, 600);
            Size = new Size(1080, 680);
            BackColor = AppColors.BgBase;
            FormClosing += StudentNativeClassroomForm_FormClosing;

            BuildLayout();
            InitializeMediaManagers();
            WireClientEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                if (_isClosing)
                    return;

                await _client.ConnectAsync("127.0.0.1", ClassroomProtocol.DefaultPort);
                if (_isClosing)
                    return;

                await _client.JoinRoomAsync(
                    _sessionId,
                    UserSessionContext.CurrentUserId ?? 0,
                    UserSessionContext.CurrentUsername ?? "Student",
                    "STUDENT");
                if (_isClosing)
                    return;

                _isConnectedToClassroom = true;
                UpdateClassroomPresentation();

                _attendanceInTask = LogAttendanceInIfPossibleAsync();
                await _attendanceInTask;
                if (_isClosing)
                    return;
            }
            catch (Exception ex)
            {
                _isConnectedToClassroom = false;
                UpdateClassroomPresentation(forceStatus: true);
                MetaTheme.ShowModernDialog("Không kết nối được lớp học native: " + ex.Message, "Thông báo");
            }
        }

        private async Task LogAttendanceInIfPossibleAsync()
        {
            int userId = UserSessionContext.CurrentUserId ?? 0;
            if (userId <= 0 || _sessionId <= 0 || _attendanceLogId > 0)
                return;

            try
            {
                _attendanceLogId = await _dbContext.LogAttendanceInAsync(userId, _sessionId);
                if (_attendanceLogId <= 0)
                    SafeAddEvent("Không ghi nhận điểm danh vì lớp chưa mở hoặc bạn chưa ghi danh.");
            }
            catch (Exception ex)
            {
                SafeAddEvent("Lỗi ghi nhận điểm danh vào: " + ex.Message);
            }
        }

        private async Task LogAttendanceOutIfPossibleAsync()
        {
            if (_attendanceLogId <= 0)
                return;

            int logId = _attendanceLogId;
            _attendanceLogId = 0;

            try
            {
                await _dbContext.LogAttendanceOutAsync(logId);
            }
            catch
            {
            }
        }

        private async Task WaitForAttendanceInIfPossibleAsync()
        {
            if (_attendanceInTask == null)
                return;

            try
            {
                await _attendanceInTask;
            }
            catch
            {
            }
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(18),
                BackColor = AppColors.BgBase
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            Controls.Add(root);

            var stage = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 14, 14)
            };
            root.Controls.Add(stage, 0, 0);

            _teacherVideo = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 10, 18),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            stage.Controls.Add(_teacherVideo);

            _videoPlaceholder = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Đang chờ video từ giáo viên...\nKhi giáo viên bật camera, hình ảnh sẽ hiện tại đây.",
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(AppFonts.Body.FontFamily, 16F, FontStyle.Regular),
                BackColor = Color.Transparent
            };
            stage.Controls.Add(_videoPlaceholder);
            _videoPlaceholder.BringToFront();

            _teacherCameraPip = new Panel
            {
                Width = 220,
                Height = 128,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Left = Math.Max(18, stage.Width - 242),
                Top = Math.Max(18, stage.Height - 150),
                BackColor = Color.FromArgb(230, 8, 10, 18),
                Padding = new Padding(4),
                Visible = false
            };
            stage.Controls.Add(_teacherCameraPip);
            _teacherCameraPip.BringToFront();
            stage.Resize += (_, _) =>
            {
                _teacherCameraPip.Left = Math.Max(18, stage.Width - _teacherCameraPip.Width - 22);
                _teacherCameraPip.Top = Math.Max(18, stage.Height - _teacherCameraPip.Height - 22);
            };

            _teacherCameraPipPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            _teacherCameraPip.Controls.Add(_teacherCameraPipPreview);

            var side = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgCard,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 0, 14)
            };
            root.Controls.Add(side, 1, 0);

            var sideLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 6,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
            side.Controls.Add(sideLayout);

            sideLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Camera của bạn",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = MetaTheme.Fonts.BodyMdBold()
            }, 0, 0);

            _studentPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 10, 18),
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0, 0, 0, 8)
            };
            sideLayout.Controls.Add(_studentPreview, 0, 1);

            sideLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Hoạt động lớp học",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = MetaTheme.Fonts.BodyMdBold()
            }, 0, 2);

            _eventsList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgInput,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.None,
                Font = MetaTheme.Fonts.BodySm()
            };
            sideLayout.Controls.Add(_eventsList, 0, 3);

            sideLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Chat lớp học",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = MetaTheme.Fonts.BodyMdBold()
            }, 0, 4);

            var chatPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 2,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(10)
            };
            chatPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            chatPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            chatPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            chatPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74));
            sideLayout.Controls.Add(chatPanel, 0, 5);

            _chatList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(226, 232, 240),
                BorderStyle = BorderStyle.None,
                Font = MetaTheme.Fonts.BodySmBold()
            };
            chatPanel.SetColumnSpan(_chatList, 2);
            chatPanel.Controls.Add(_chatList, 0, 0);

            _chatInput = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.FixedSingle,
                Font = MetaTheme.Fonts.BodyMdBold(),
                PlaceholderText = "Nhập tin nhắn..."
            };
            _chatInput.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    await SendChatMessageAsync();
                }
            };
            chatPanel.Controls.Add(_chatInput, 0, 1);

            _btnSendChat = new Button
            {
                Dock = DockStyle.Fill,
                Text = "Gửi",
                BackColor = AppColors.AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = MetaTheme.Fonts.BodySmBold(),
                Cursor = Cursors.Hand
            };
            _btnSendChat.FlatAppearance.BorderSize = 0;
            _btnSendChat.Click += async (_, _) => await SendChatMessageAsync();
            chatPanel.Controls.Add(_btnSendChat, 1, 1);

            var controlBar = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BgCard,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 14, 14)
            };
            root.Controls.Add(controlBar, 0, 1);

            var controlLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };
            controlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            controlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            controlBar.Controls.Add(controlLayout);

            _mediaStateLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = MetaTheme.Fonts.BodySmBold(),
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                Margin = Padding.Empty
            };
            controlLayout.Controls.Add(_mediaStateLabel, 0, 0);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            controlLayout.Controls.Add(buttons, 0, 1);

            _btnCamera = CreateActionButton("Bật Camera", AppColors.AccentBlue);
            _btnMic = CreateActionButton("Tắt Mic", AppColors.Warning);
            _btnShareScreen = CreateActionButton("Share màn hình", Color.FromArgb(139, 92, 246));
            _btnCamera.Click += async (_, _) => await ToggleCameraAsync();
            _btnMic.Click += async (_, _) => await ToggleMicAsync();
            _btnShareScreen.Click += async (_, _) => await ToggleScreenShareAsync();
            buttons.Controls.Add(_btnCamera);
            buttons.Controls.Add(_btnMic);
            buttons.Controls.Add(_btnShareScreen);

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Đang kết nối classroom socket...",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = MetaTheme.Fonts.SubtitleLg(),
                Padding = new Padding(14),
                BackColor = AppColors.BgCard
            };
            root.SetColumnSpan(_statusLabel, 2);
            root.Controls.Add(_statusLabel, 0, 2);

            UpdateClassroomPresentation();
        }

        private static Button CreateActionButton(string text, Color color)
        {
            var button = new Button
            {
                Text = text,
                Width = 150,
                Height = 46,
                Margin = new Padding(0, 0, 12, 0),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = MetaTheme.Fonts.ButtonMd(),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            RoundedButtonHelper.Apply(button, 12);
            return button;
        }

        private void WireClientEvents()
        {
            _client.StatusChanged += (_, e) => HandleClientStatusChanged(e.Status);
            _client.SignalReceived += (_, e) => HandleSignal(e.Signal);
        }

        private void HandleClientStatusChanged(string status)
        {
            if (string.Equals(status, "Disconnected from classroom server.", StringComparison.Ordinal))
            {
                _isConnectedToClassroom = false;
                SafeUpdateClassroomPresentation(forceStatus: true);
            }

            SafeAddEvent(status);
        }

        private void InitializeMediaManagers()
        {
            _screenShareManager = new ClassroomScreenShareManager(
                timerIntervalMilliseconds: 240,
                throttleMilliseconds: 260,
                maxWidth: 1280,
                maxHeight: 720,
                jpegQuality: 44L,
                canCaptureFrame: () => _client.IsConnected,
                sendFrameAsync: SendScreenFrameAsync);

            _cameraManager = new ClassroomCameraManager(
                previewTargetProvider: () => _studentPreview,
                maxWidth: 360,
                maxHeight: 240,
                jpegQuality: 40L,
                throttleMilliseconds: 220,
                canSendFrame: () => _client.IsConnected,
                sendFrameAsync: SendCameraFrameAsync);
        }

        private void UpdateClassroomPresentation(bool forceStatus = false)
        {
            if (IsDisposed ||
                _statusLabel == null ||
                _mediaStateLabel == null ||
                _btnCamera == null ||
                _btnMic == null ||
                _btnShareScreen == null ||
                _statusLabel.IsDisposed ||
                _mediaStateLabel.IsDisposed ||
                _btnCamera.IsDisposed ||
                _btnMic.IsDisposed ||
                _btnShareScreen.IsDisposed)
            {
                return;
            }

            ClassroomUxPresentation view = ClassroomUxPresenter.Present(
                isTeacher: false,
                isConnected: _isConnectedToClassroom,
                isCameraOn: _isCameraOn,
                isMicOn: _isMicOn,
                isSharingScreen: _isSharingScreen,
                participantCount: 0);

            if (forceStatus)
            {
                ClearLiveStatus();
            }

            if (forceStatus || !_hasActiveLiveStatus)
            {
                _statusLabel.Text = view.StatusText;
            }

            _mediaStateLabel.Text = view.DetailText;
            _btnCamera.Text = view.CameraActionText;
            _btnMic.Text = view.MicActionText;
            _btnShareScreen.Text = view.ShareActionText;
        }

        private async Task SendScreenFrameAsync(ClassroomScreenShareFrame frame)
        {
            await _client.SendAsync(new ClassroomSignalModel
            {
                Type = ClassroomMessageType.ScreenShareFrame,
                SessionId = _sessionId,
                SenderId = UserSessionContext.CurrentUserId ?? 0,
                SenderName = UserSessionContext.CurrentUsername ?? "Student",
                SenderRole = "STUDENT",
                Payload =
                {
                    ["imageBase64"] = frame.ImageBase64,
                    ["width"] = frame.Width.ToString(),
                    ["height"] = frame.Height.ToString(),
                    ["sourceTitle"] = frame.SourceTitle
                }
            });
        }

        private async Task SendCameraFrameAsync(ClassroomCameraFrame frame)
        {
            await _client.SendAsync(new ClassroomSignalModel
            {
                Type = ClassroomMessageType.VideoFrame,
                SessionId = _sessionId,
                SenderId = UserSessionContext.CurrentUserId ?? 0,
                SenderName = UserSessionContext.CurrentUsername ?? "Student",
                SenderRole = "STUDENT",
                Payload =
                {
                    ["imageBase64"] = frame.ImageBase64,
                    ["width"] = frame.Width.ToString(),
                    ["height"] = frame.Height.ToString()
                }
            });
        }

        private void HandleSignal(ClassroomSignalModel signal)
        {
            if (signal.Type == ClassroomMessageType.VideoFrame && signal.SenderRole == "TEACHER")
            {
                RenderTeacherVideoFrame(signal);
                return;
            }

            if (signal.Type == ClassroomMessageType.ScreenShareFrame && signal.SenderRole == "TEACHER")
            {
                RenderTeacherScreenShareFrame(signal);
                return;
            }

            if (signal.Type == ClassroomMessageType.Chat)
            {
                RenderChatMessage(signal);
                return;
            }

            SafeAddEvent($"{signal.Type} từ {signal.SenderName}");
            if (signal.Type == ClassroomMessageType.CamOff && signal.SenderRole == "TEACHER")
            {
                SafeShowTeacherPlaceholder("Camera giáo viên đã tắt.");
            }
            else if (signal.Type == ClassroomMessageType.CamOn && signal.SenderRole == "TEACHER")
            {
                SafeSetStatus("Giáo viên đã bật camera. Đang nhận video...");
            }
            else if (signal.Type == ClassroomMessageType.ScreenShareOn && signal.SenderRole == "TEACHER")
            {
                SafeSetStatus("Giáo viên đang chia sẻ màn hình...");
            }
            else if (signal.Type == ClassroomMessageType.ScreenShareOff && signal.SenderRole == "TEACHER")
            {
                SafeShowTeacherPlaceholder("Giáo viên đã dừng chia sẻ màn hình.");
            }
        }

        private async Task SendChatMessageAsync()
        {
            string message = _chatInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;
            if (!_client.IsConnected)
            {
                SafeSetStatus("Chưa kết nối classroom socket nên chưa gửi được chat.");
                return;
            }

            _chatInput.Clear();
            string senderName = UserSessionContext.CurrentUsername ?? "Student";

            await _client.SendAsync(new ClassroomSignalModel
            {
                Type = ClassroomMessageType.Chat,
                SessionId = _sessionId,
                SenderId = UserSessionContext.CurrentUserId ?? 0,
                SenderName = senderName,
                SenderRole = "STUDENT",
                Payload =
                {
                    ["message"] = message,
                    ["sentAt"] = DateTime.Now.ToString("HH:mm")
                }
            });
        }

        private void RenderChatMessage(ClassroomSignalModel signal)
        {
            if (!signal.Payload.TryGetValue("message", out string? message) || string.IsNullOrWhiteSpace(message)) return;
            AppendChatMessage(signal.SenderName, message, signal.SenderRole == "TEACHER");
        }

        private void AppendChatMessage(string senderName, string message, bool isTeacher)
        {
            string label = isTeacher ? "GV" : "HS";
            this.InvokeIfRequired(() =>
            {
                _chatList.Items.Add($"[{DateTime.Now:HH:mm}] {label} {senderName}: {message}");
                _chatList.TopIndex = Math.Max(0, _chatList.Items.Count - 1);
            });
        }

        private async Task ToggleCameraAsync()
        {
            if (_cameraManager.IsRunning)
            {
                StopCamera();
                _btnCamera.BackColor = AppColors.AccentBlue;
                _isCameraOn = false;
                UpdateClassroomPresentation();
                await SendStateAsync(ClassroomMessageType.CamOff);
                return;
            }

            try
            {
                if (!_cameraManager.Start())
                {
                    MetaTheme.ShowModernDialog("Không tìm thấy webcam trên máy học sinh.", "Camera");
                    return;
                }

                _btnCamera.BackColor = AppColors.Danger;
                _isCameraOn = true;
                UpdateClassroomPresentation();
                await SendStateAsync(ClassroomMessageType.CamOn);
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể bật camera: " + ex.Message, "Camera");
            }
        }

        private async Task ToggleMicAsync()
        {
            _isMicOn = !_isMicOn;
            _btnMic.BackColor = _isMicOn ? AppColors.Warning : AppColors.AccentBlue;
            UpdateClassroomPresentation();
            await SendStateAsync(_isMicOn ? ClassroomMessageType.MicOn : ClassroomMessageType.MicOff);
        }

        private async Task SendStateAsync(string type)
        {
            if (!_client.IsConnected) return;
            await _client.SendAsync(new ClassroomSignalModel
            {
                Type = type,
                SessionId = _sessionId,
                SenderId = UserSessionContext.CurrentUserId ?? 0,
                SenderName = UserSessionContext.CurrentUsername ?? "Student",
                SenderRole = "STUDENT"
            });
        }

        private async Task ToggleScreenShareAsync()
        {
            if (_screenShareManager.IsSharing)
            {
                StopScreenShare();
                await SendStateAsync(ClassroomMessageType.ScreenShareOff);
                return;
            }

            using var picker = new ScreenSharePickerDialog();
            if (picker.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _btnShareScreen.BackColor = AppColors.Danger;
            _isSharingScreen = true;
            UpdateClassroomPresentation();
            SafeAddEvent($"Đang trình bày {picker.SelectedTitle} cho giáo viên.");
            await SendStateAsync(ClassroomMessageType.ScreenShareOn);

            _screenShareManager.Start(picker.SelectedBounds, picker.SelectedTitle);
        }

        private void RenderTeacherVideoFrame(ClassroomSignalModel signal)
        {
            try
            {
                if (!signal.Payload.TryGetValue("imageBase64", out string? base64) || string.IsNullOrWhiteSpace(base64)) return;
                Image frame = ClassroomFrameHelper.DecodeFrame(base64);

                ClassroomFrameHelper.TryBeginReplaceImage(
                    this,
                    frame,
                    () =>
                    {
                        if (IsDisposed ||
                            _teacherVideo.IsDisposed ||
                            !_teacherVideo.IsHandleCreated ||
                            _teacherCameraPip.IsDisposed ||
                            _teacherCameraPipPreview.IsDisposed)
                        {
                            throw new ObjectDisposedException(nameof(StudentNativeClassroomForm));
                        }

                        return _teacherCameraPip.Visible ? _teacherCameraPipPreview : _teacherVideo;
                    },
                    () =>
                    {
                        if (IsDisposed ||
                            _videoPlaceholder.IsDisposed ||
                            _teacherCameraPip.IsDisposed ||
                            _statusLabel.IsDisposed)
                        {
                            return;
                        }

                        _videoPlaceholder.Visible = false;
                        if (_teacherCameraPip.Visible)
                        {
                            _teacherCameraPip.BringToFront();
                            SetLiveStatus($"\u0110ang xem gi\u00e1o vi\u00ean tr\u00ecnh b\u00e0y, camera thu nh\u1ecf - {DateTime.Now:HH:mm:ss}");
                        }
                        else
                        {
                            SetLiveStatus($"\u0110ang xem video gi\u00e1o vi\u00ean - {DateTime.Now:HH:mm:ss}");
                        }
                    });
            }
            catch
            {
                // Drop corrupt frames silently.
            }
        }

        private void RenderTeacherScreenShareFrame(ClassroomSignalModel signal)
        {
            try
            {
                if (!signal.Payload.TryGetValue("imageBase64", out string? base64) || string.IsNullOrWhiteSpace(base64)) return;
                Image frame = ClassroomFrameHelper.DecodeFrame(base64);

                ClassroomFrameHelper.TryBeginReplaceImage(
                    this,
                    _teacherVideo,
                    frame,
                    () =>
                    {
                        if (IsDisposed ||
                            _teacherVideo.IsDisposed ||
                            _videoPlaceholder.IsDisposed ||
                            _teacherCameraPip.IsDisposed ||
                            _statusLabel.IsDisposed)
                        {
                            return;
                        }

                        _videoPlaceholder.Visible = false;
                        _teacherCameraPip.Visible = true;
                        _teacherCameraPip.BringToFront();
                        string sourceTitle = signal.Payload.TryGetValue("sourceTitle", out string? title) ? title : "màn hình";
                        SetLiveStatus($"\u0110ang xem gi\u00e1o vi\u00ean tr\u00ecnh b\u00e0y: {sourceTitle} - {DateTime.Now:HH:mm:ss}");
                    });
            }
            catch
            {
                // Drop corrupt screen-share frames silently.
            }
        }

        private void SafeShowTeacherPlaceholder(string text)
        {
            this.InvokeIfRequired(() =>
            {
                _videoPlaceholder.Text = text;
                _videoPlaceholder.Visible = true;
                _videoPlaceholder.BringToFront();
                Image? old = _teacherVideo.Image;
                _teacherVideo.Image = null;
                old?.Dispose();
                _teacherCameraPip.Visible = false;
                Image? oldPip = _teacherCameraPipPreview.Image;
                _teacherCameraPipPreview.Image = null;
                oldPip?.Dispose();
                SetLiveStatus(text);
            });
        }

        private void SetLiveStatus(string text)
        {
            _hasActiveLiveStatus = true;
            _statusLabel.Text = text;
        }

        private void ClearLiveStatus()
        {
            _hasActiveLiveStatus = false;
        }

        private void SafeSetStatus(string text)
        {
            _statusLabel.InvokeIfRequired(() => SetLiveStatus(text));
        }

        private void SafeAddEvent(string text)
        {
            _eventsList.InvokeIfRequired(() => _eventsList.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} - {text}"));
        }

        private void SafeUpdateClassroomPresentation(bool forceStatus = false)
        {
            this.InvokeIfRequired(() => UpdateClassroomPresentation(forceStatus));
        }

        private void StopCamera()
        {
            _cameraManager.Stop();
            _isCameraOn = false;

            Image? old = _studentPreview.Image;
            _studentPreview.Image = null;
            old?.Dispose();
        }

        private void StopScreenShare()
        {
            _screenShareManager.Stop();
            _isSharingScreen = false;
            _btnShareScreen.BackColor = Color.FromArgb(139, 92, 246);
            UpdateClassroomPresentation();
        }

        private async void StudentNativeClassroomForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_cleanupComplete)
                return;

            e.Cancel = true;
            if (_isClosing)
                return;

            _isClosing = true;

            try
            {
                await CloseClassroomAsync();
            }
            finally
            {
                _cleanupComplete = true;
                Close();
            }
        }

        private async Task CloseClassroomAsync()
        {
            StopScreenShare();
            StopCamera();
            try
            {
                if (_client.IsConnected)
                {
                    await _client.LeaveRoomAsync(
                        _sessionId,
                        UserSessionContext.CurrentUserId ?? 0,
                        UserSessionContext.CurrentUsername ?? "Student",
                        "STUDENT");
                }
            }
            catch
            {
            }

            await WaitForAttendanceInIfPossibleAsync();
            await LogAttendanceOutIfPossibleAsync();

            Image? oldTeacher = _teacherVideo.Image;
            _teacherVideo.Image = null;
            oldTeacher?.Dispose();
            _screenShareManager.Dispose();
            _cameraManager.Dispose();
            try
            {
                await _client.DisconnectAsync();
            }
            catch
            {
            }

            _isConnectedToClassroom = false;
        }
        private sealed class ScreenSharePickerDialog : Form
        {
            public Rectangle SelectedBounds { get; private set; }
            public string SelectedTitle { get; private set; } = "Màn hình";

            public ScreenSharePickerDialog()
            {
                Text = "Trình bày ngay";
                StartPosition = FormStartPosition.CenterParent;
                Size = new Size(620, 430);
                MinimumSize = new Size(620, 430);
                BackColor = AppColors.BgBase;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;

                var root = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 3,
                    Padding = new Padding(22),
                    BackColor = AppColors.BgBase
                };
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
                Controls.Add(root);

                root.Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "Chọn nội dung muốn trình bày\nBạn chọn màn hình trước, sau đó lớp học mới nhận được màn hình.",
                    ForeColor = AppColors.TextPrimary,
                    Font = AppFonts.Semibold(13F),
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, 0);

                var cards = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    WrapContents = true,
                    BackColor = AppColors.BgCard,
                    Padding = new Padding(12)
                };
                root.Controls.Add(cards, 0, 1);

                Screen[] screens = Screen.AllScreens;
                for (int i = 0; i < screens.Length; i++)
                {
                    Screen screen = screens[i];
                    string title = screens.Length == 1 ? "Toàn bộ màn hình" : $"Màn hình {i + 1}";
                    cards.Controls.Add(CreateScreenCard(title, screen.Bounds, screen.Primary));
                }

                var actions = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    BackColor = Color.Transparent
                };
                root.Controls.Add(actions, 0, 2);

                var cancel = new Button
                {
                    Text = "Hủy",
                    Width = 110,
                    Height = 42,
                    BackColor = AppColors.BgInput,
                    ForeColor = AppColors.TextPrimary,
                    FlatStyle = FlatStyle.Flat,
                    Font = MetaTheme.Fonts.ButtonMd(),
                    DialogResult = DialogResult.Cancel
                };
                cancel.FlatAppearance.BorderSize = 0;
                RoundedButtonHelper.Apply(cancel, 10);
                actions.Controls.Add(cancel);

                SelectedBounds = Screen.PrimaryScreen?.Bounds ?? Screen.AllScreens[0].Bounds;
                SelectedTitle = "Toàn bộ màn hình";
            }

            private Control CreateScreenCard(string title, Rectangle bounds, bool primary)
            {
                var card = new Panel
                {
                    Width = 260,
                    Height = 150,
                    Margin = new Padding(0, 0, 12, 12),
                    BackColor = Color.FromArgb(20, 25, 38),
                    Cursor = Cursors.Hand,
                    Tag = bounds
                };

                var preview = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 92,
                    BackColor = Color.FromArgb(8, 10, 18)
                };
                card.Controls.Add(preview);

                var icon = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "▣",
                    ForeColor = Color.FromArgb(139, 92, 246),
                    Font = AppFonts.Semibold(34F),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                preview.Controls.Add(icon);

                var name = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = primary ? $"{title} · Chính" : title,
                    ForeColor = AppColors.TextPrimary,
                    Font = MetaTheme.Fonts.BodyMdBold(),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(name);

                void Choose(object? _, EventArgs __)
                {
                    SelectedBounds = bounds;
                    SelectedTitle = title;
                    DialogResult = DialogResult.OK;
                    Close();
                }

                card.Click += Choose;
                preview.Click += Choose;
                icon.Click += Choose;
                name.Click += Choose;
                return card;
            }
        }
    }
}
