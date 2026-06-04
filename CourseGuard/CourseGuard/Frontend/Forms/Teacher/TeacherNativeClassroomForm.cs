using CourseGuard.Backend.Models;
using CourseGuard.Backend.Services.Classroom;
using CourseGuard.Frontend.Helpers;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.Forms.Teacher
{
    public sealed class TeacherNativeClassroomForm : Form
    {
        private readonly int _sessionId;
        private readonly TcpClassroomServer _server = new();
        private Label _statusLabel = null!;
        private ListBox _eventsList = null!;
        private PictureBox _teacherPreview = null!;
        private FlowLayoutPanel _studentVideoGrid = null!;
        private Button _btnCamera = null!;
        private Button _btnMic = null!;
        private Button _btnShareScreen = null!;
        private Button _btnEndClass = null!;
        private Panel _teacherCameraPip = null!;
        private PictureBox _teacherCameraPipPreview = null!;
        private ListBox _chatList = null!;
        private TextBox _chatInput = null!;
        private Button _btnSendChat = null!;
        private readonly Dictionary<int, PictureBox> _studentVideoBoxes = new();
        private readonly Dictionary<int, Label> _studentVideoLabels = new();

        private ClassroomCameraManager _cameraManager = null!;
        private ClassroomScreenShareManager _screenShareManager = null!;
        private bool _isMicOn = true;

        public TeacherNativeClassroomForm(int sessionId)
        {
            _sessionId = sessionId;
            Text = "CourseGuard Classroom - Giáo viên";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(980, 640);
            Size = new Size(1180, 720);
            BackColor = AppColors.BgBase;
            FormClosing += TeacherNativeClassroomForm_FormClosing;

            BuildLayout();
            InitializeMediaManagers();
            WireServerEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await _server.StartAsync(_sessionId);
            _statusLabel.Text = $"Lớp #{_sessionId} đã mở - bấm 'Bật Camera' để bắt đầu video call native.";
            SafeAddEvent("Classroom socket server đã sẵn sàng.");
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
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
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

            _teacherPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 10, 18),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            stage.Controls.Add(_teacherPreview);

            var placeholder = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Camera giáo viên đang tắt\nBấm 'Bật Camera' để bắt đầu video call native",
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(AppFonts.Body.FontFamily, 16F, FontStyle.Regular),
                BackColor = Color.Transparent
            };
            stage.Controls.Add(placeholder);
            placeholder.BringToFront();
            _teacherPreview.Tag = placeholder;

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
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            side.Controls.Add(sideLayout);

            var studentVideoTitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Camera học sinh",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = MetaTheme.Fonts.SubtitleLg()
            };
            sideLayout.Controls.Add(studentVideoTitle, 0, 0);

            _studentVideoGrid = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = AppColors.BgInput,
                Padding = new Padding(8),
                Margin = new Padding(0, 0, 0, 8)
            };
            sideLayout.Controls.Add(_studentVideoGrid, 0, 1);

            var participantTitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Hoạt động lớp học",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = MetaTheme.Fonts.BodyMdBold()
            };
            sideLayout.Controls.Add(participantTitle, 0, 2);

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

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            controlBar.Controls.Add(buttons);

            _btnCamera = CreateActionButton("Bật Camera", AppColors.AccentBlue);
            _btnMic = CreateActionButton("Tắt Mic", AppColors.Warning);
            _btnShareScreen = CreateActionButton("Share màn hình", Color.FromArgb(139, 92, 246));
            _btnEndClass = CreateActionButton("Kết thúc lớp", AppColors.Danger);

            _btnCamera.Click += async (_, _) => await ToggleCameraAsync();
            _btnMic.Click += async (_, _) => await ToggleMicAsync();
            _btnShareScreen.Click += async (_, _) => await ToggleScreenShareAsync();
            _btnEndClass.Click += (_, _) => Close();

            buttons.Controls.Add(_btnCamera);
            buttons.Controls.Add(_btnMic);
            buttons.Controls.Add(_btnShareScreen);
            buttons.Controls.Add(_btnEndClass);

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Đang khởi động socket classroom...",
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = MetaTheme.Fonts.SubtitleLg(),
                Padding = new Padding(14),
                BackColor = AppColors.BgCard
            };
            root.SetColumnSpan(_statusLabel, 2);
            root.Controls.Add(_statusLabel, 0, 2);
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

        private void WireServerEvents()
        {
            _server.StatusChanged += (_, e) => SafeAddEvent(e.Status);
            _server.SignalReceived += (_, e) => HandleIncomingSignal(e.Signal);
        }

        private void InitializeMediaManagers()
        {
            _screenShareManager = new ClassroomScreenShareManager(
                timerIntervalMilliseconds: 220,
                throttleMilliseconds: 240,
                maxWidth: 1280,
                maxHeight: 720,
                jpegQuality: 46L,
                canCaptureFrame: () => !IsDisposed,
                previewFrame: ShowTeacherScreenSharePreview,
                sendFrameAsync: BroadcastScreenFrameAsync);

            _cameraManager = new ClassroomCameraManager(
                previewTargetProvider: () => _screenShareManager.IsSharing ? _teacherCameraPipPreview : _teacherPreview,
                maxWidth: 640,
                maxHeight: 360,
                jpegQuality: 45L,
                throttleMilliseconds: 160,
                canSendFrame: () => _server.Clients.Count > 0,
                sendFrameAsync: BroadcastCameraFrameAsync,
                afterPreviewUpdated: AfterTeacherCameraPreviewUpdated);
        }

        private async Task BroadcastScreenFrameAsync(ClassroomScreenShareFrame frame)
        {
            if (_server.Clients.Count == 0) return;

            await _server.BroadcastAsync(new ClassroomSignalModel
            {
                Type = ClassroomMessageType.ScreenShareFrame,
                SessionId = _sessionId,
                SenderId = 0,
                SenderName = "Giáo viên",
                SenderRole = "TEACHER",
                Payload =
                {
                    ["imageBase64"] = frame.ImageBase64,
                    ["width"] = frame.Width.ToString(),
                    ["height"] = frame.Height.ToString(),
                    ["sourceTitle"] = frame.SourceTitle
                }
            });
        }

        private async Task BroadcastCameraFrameAsync(ClassroomCameraFrame frame)
        {
            await _server.BroadcastAsync(new ClassroomSignalModel
            {
                Type = ClassroomMessageType.VideoFrame,
                SessionId = _sessionId,
                SenderId = 0,
                SenderName = "Giáo viên",
                SenderRole = "TEACHER",
                Payload =
                {
                    ["imageBase64"] = frame.ImageBase64,
                    ["width"] = frame.Width.ToString(),
                    ["height"] = frame.Height.ToString()
                }
            });
        }

        private void ShowTeacherScreenSharePreview(Bitmap frame)
        {
            if (IsDisposed || !_teacherPreview.IsHandleCreated)
            {
                frame.Dispose();
                return;
            }

            try
            {
                _teacherPreview.BeginInvoke(() =>
                {
                    if (IsDisposed || _teacherPreview.IsDisposed)
                    {
                        frame.Dispose();
                        return;
                    }

                    Image? old = _teacherPreview.Image;
                    _teacherPreview.Image = frame;
                    old?.Dispose();
                    HideCameraPlaceholder();
                    if (_cameraManager.IsRunning)
                    {
                        _teacherCameraPip.Visible = true;
                        _teacherCameraPip.BringToFront();
                    }
                });
            }
            catch
            {
                frame.Dispose();
            }
        }

        private void AfterTeacherCameraPreviewUpdated()
        {
            if (_screenShareManager.IsSharing)
            {
                _teacherCameraPip.Visible = true;
                _teacherCameraPip.BringToFront();
                return;
            }

            HideCameraPlaceholder();
        }

        private void HandleIncomingSignal(ClassroomSignalModel signal)
        {
            if (signal.Type == ClassroomMessageType.VideoFrame && signal.SenderRole == "STUDENT")
            {
                RenderStudentVideoFrame(signal);
                return;
            }

            if (signal.Type == ClassroomMessageType.ScreenShareFrame && signal.SenderRole == "STUDENT")
            {
                RenderIncomingScreenShare(signal);
                return;
            }

            if (signal.Type == ClassroomMessageType.Chat)
            {
                RenderChatMessage(signal);
                return;
            }

            if (signal.SenderRole == "STUDENT" && signal.Type == ClassroomMessageType.CamOff)
            {
                SafeRemoveStudentVideo(signal.SenderId);
            }
            else if (signal.SenderRole == "STUDENT" && signal.Type == ClassroomMessageType.ScreenShareOn)
            {
                SafeAddEvent($"{signal.SenderName} bắt đầu share màn hình.");
            }
            else if (signal.SenderRole == "STUDENT" && signal.Type == ClassroomMessageType.ScreenShareOff)
            {
                SafeSetStagePlaceholder($"{signal.SenderName} đã dừng share màn hình.");
            }
        }

        private async Task SendChatMessageAsync()
        {
            string message = _chatInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;

            _chatInput.Clear();
            AppendChatMessage("Giáo viên", message, true);

            await _server.BroadcastAsync(new ClassroomSignalModel
            {
                Type = ClassroomMessageType.Chat,
                SessionId = _sessionId,
                SenderId = 0,
                SenderName = "Giáo viên",
                SenderRole = "TEACHER",
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
            if (IsDisposed || !IsHandleCreated) return;
            string label = isTeacher ? "GV" : "HS";
            BeginInvoke(() =>
            {
                _chatList.Items.Add($"[{DateTime.Now:HH:mm}] {label} {senderName}: {message}");
                _chatList.TopIndex = Math.Max(0, _chatList.Items.Count - 1);
            });
        }

        private async Task ToggleScreenShareAsync()
        {
            if (_screenShareManager.IsSharing)
            {
                StopScreenShare();
                await BroadcastScreenShareStateAsync(ClassroomMessageType.ScreenShareOff);
                return;
            }

            using var picker = new ScreenSharePickerDialog("Giáo viên");
            if (picker.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _btnShareScreen.Text = "Dừng trình bày";
            _btnShareScreen.BackColor = AppColors.Danger;
            _statusLabel.Text = $"Bạn đang trình bày: {picker.SelectedTitle}";
            HideCameraPlaceholder();
            SafeAddEvent($"Đang trình bày {picker.SelectedTitle}.");
            await BroadcastScreenShareStateAsync(ClassroomMessageType.ScreenShareOn);

            _screenShareManager.Start(picker.SelectedBounds, picker.SelectedTitle);
        }

        private async Task BroadcastScreenShareStateAsync(string type)
        {
            await _server.BroadcastAsync(new ClassroomSignalModel
            {
                Type = type,
                SessionId = _sessionId,
                SenderId = 0,
                SenderName = "Giáo viên",
                SenderRole = "TEACHER"
            });
        }

        private async Task ToggleCameraAsync()
        {
            if (_cameraManager.IsRunning)
            {
                StopCamera();
                _btnCamera.Text = "Bật Camera";
                _btnCamera.BackColor = AppColors.AccentBlue;
                _statusLabel.Text = "Camera giáo viên đã tắt.";
                await _server.BroadcastAsync(new Backend.Models.ClassroomSignalModel
                {
                    Type = ClassroomMessageType.CamOff,
                    SessionId = _sessionId,
                    SenderId = 0,
                    SenderName = "Giáo viên",
                    SenderRole = "TEACHER"
                });
                return;
            }

            try
            {
                if (!_cameraManager.Start())
                {
                    MetaTheme.ShowModernDialog("Không tìm thấy webcam trên máy giáo viên.", "Camera");
                    return;
                }

                _btnCamera.Text = "Tắt Camera";
                _btnCamera.BackColor = AppColors.Danger;
                _statusLabel.Text = "Camera giáo viên đang bật - video call native đã bắt đầu.";
                HideCameraPlaceholder();

                await _server.BroadcastAsync(new Backend.Models.ClassroomSignalModel
                {
                    Type = ClassroomMessageType.CamOn,
                    SessionId = _sessionId,
                    SenderId = 0,
                    SenderName = "Giáo viên",
                    SenderRole = "TEACHER"
                });
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể bật camera: " + ex.Message, "Camera");
            }
        }

        private async Task ToggleMicAsync()
        {
            _isMicOn = !_isMicOn;
            _btnMic.Text = _isMicOn ? "Tắt Mic" : "Bật Mic";
            _btnMic.BackColor = _isMicOn ? AppColors.Warning : AppColors.AccentBlue;
            _statusLabel.Text = _isMicOn ? "Micro đang bật." : "Micro đang tắt.";

            await _server.BroadcastAsync(new Backend.Models.ClassroomSignalModel
            {
                Type = _isMicOn ? ClassroomMessageType.MicOn : ClassroomMessageType.MicOff,
                SessionId = _sessionId,
                SenderId = 0,
                SenderName = "Giáo viên",
                SenderRole = "TEACHER"
            });
        }

        private void HideCameraPlaceholder()
        {
            if (_teacherPreview.Tag is Label placeholder)
            {
                placeholder.Visible = false;
            }
        }

        private void ShowCameraPlaceholder()
        {
            if (_teacherPreview.Tag is Label placeholder)
            {
                placeholder.Visible = true;
                placeholder.BringToFront();
            }
        }

        private void StopCamera()
        {
            _cameraManager.Stop();

            if (!_screenShareManager.IsSharing)
            {
                Image? old = _teacherPreview.Image;
                _teacherPreview.Image = null;
                old?.Dispose();
                ShowCameraPlaceholder();
            }

            Image? oldPip = _teacherCameraPipPreview.Image;
            _teacherCameraPipPreview.Image = null;
            oldPip?.Dispose();
            _teacherCameraPip.Visible = false;
        }

        private void RenderStudentVideoFrame(ClassroomSignalModel signal)
        {
            try
            {
                if (signal.SenderId <= 0)
                {
                    return;
                }

                if (!signal.Payload.TryGetValue("imageBase64", out string? base64) || string.IsNullOrWhiteSpace(base64))
                {
                    return;
                }

                Image frame = ClassroomFrameHelper.DecodeFrame(base64);

                ClassroomFrameHelper.TryBeginReplaceImage(
                    this,
                    frame,
                    () =>
                    {
                        if (IsDisposed || _studentVideoGrid.IsDisposed || !_studentVideoGrid.IsHandleCreated)
                        {
                            throw new ObjectDisposedException(nameof(TeacherNativeClassroomForm));
                        }

                        return GetOrCreateStudentVideoBox(signal.SenderId, signal.SenderName);
                    });
            }
            catch
            {
                // Drop corrupt student frames silently to keep teacher UI stable.
            }
        }

        private void RenderIncomingScreenShare(ClassroomSignalModel signal)
        {
            try
            {
                if (!signal.Payload.TryGetValue("imageBase64", out string? base64) || string.IsNullOrWhiteSpace(base64)) return;
                Image frame = ClassroomFrameHelper.DecodeFrame(base64);

                ClassroomFrameHelper.TryBeginReplaceImage(
                    this,
                    _teacherPreview,
                    frame,
                    () =>
                    {
                        if (IsDisposed || _teacherPreview.IsDisposed || _statusLabel.IsDisposed)
                        {
                            return;
                        }

                        HideCameraPlaceholder();
                        string sourceTitle = signal.Payload.TryGetValue("sourceTitle", out string? title) ? title : "màn hình";
                        _statusLabel.Text = $"Đang xem {signal.SenderName} trình bày: {sourceTitle} - {DateTime.Now:HH:mm:ss}";
                    });
            }
            catch
            {
                // Drop corrupt screen-share frames silently.
            }
        }

        private void SafeSetStagePlaceholder(string text)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(() =>
            {
                Image? old = _teacherPreview.Image;
                _teacherPreview.Image = null;
                old?.Dispose();
                if (_teacherPreview.Tag is Label placeholder)
                {
                    placeholder.Text = text;
                    placeholder.Visible = true;
                    placeholder.BringToFront();
                }
                _statusLabel.Text = text;
            });
        }

        private PictureBox GetOrCreateStudentVideoBox(int studentId, string studentName)
        {
            if (_studentVideoBoxes.TryGetValue(studentId, out PictureBox? existing))
            {
                if (_studentVideoLabels.TryGetValue(studentId, out Label? existingLabel))
                {
                    existingLabel.Text = string.IsNullOrWhiteSpace(studentName) ? $"Student #{studentId}" : studentName;
                }
                return existing;
            }

            var tile = new Panel
            {
                Width = 128,
                Height = 104,
                Margin = new Padding(0, 0, 8, 8),
                BackColor = Color.FromArgb(13, 17, 27)
            };

            var box = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            tile.Controls.Add(box);

            var label = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                Text = string.IsNullOrWhiteSpace(studentName) ? $"Student #{studentId}" : studentName,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = MetaTheme.Fonts.CaptionBold()
            };
            tile.Controls.Add(label);
            label.BringToFront();

            _studentVideoBoxes[studentId] = box;
            _studentVideoLabels[studentId] = label;
            _studentVideoGrid.Controls.Add(tile);
            return box;
        }

        private void SafeRemoveStudentVideo(int studentId)
        {
            if (studentId <= 0 || IsDisposed || !IsHandleCreated) return;
            BeginInvoke(() =>
            {
                if (!_studentVideoBoxes.TryGetValue(studentId, out PictureBox? box))
                {
                    return;
                }

                Control? tile = box.Parent;
                Image? old = box.Image;
                box.Image = null;
                old?.Dispose();
                _studentVideoBoxes.Remove(studentId);
                _studentVideoLabels.Remove(studentId);
                if (tile != null)
                {
                    _studentVideoGrid.Controls.Remove(tile);
                    tile.Dispose();
                }
            });
        }

        private void SafeAddEvent(string text)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(() => _eventsList.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} - {text}"));
        }

        private void StopScreenShare()
        {
            _screenShareManager.Stop();
            _btnShareScreen.Text = "Share màn hình";
            _btnShareScreen.BackColor = Color.FromArgb(139, 92, 246);
            _teacherCameraPip.Visible = false;
            Image? oldPip = _teacherCameraPipPreview.Image;
            _teacherCameraPipPreview.Image = null;
            oldPip?.Dispose();
            _statusLabel.Text = $"Đã dừng trình bày {_screenShareManager.SelectedTitle}.";
        }

        private async void TeacherNativeClassroomForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopScreenShare();
            StopCamera();
            foreach (PictureBox box in _studentVideoBoxes.Values)
            {
                Image? old = box.Image;
                box.Image = null;
                old?.Dispose();
            }
            _studentVideoBoxes.Clear();
            _studentVideoLabels.Clear();
            _screenShareManager.Dispose();
            _cameraManager.Dispose();
            await _server.StopAsync();
        }
        private sealed class ScreenSharePickerDialog : Form
        {
            public Rectangle SelectedBounds { get; private set; }
            public string SelectedTitle { get; private set; } = "Màn hình";

            public ScreenSharePickerDialog(string roleName)
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
                    Text = "Chọn nội dung muốn trình bày\nGiống Google Meet: bạn chọn màn hình trước, rồi mới bắt đầu share.",
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
