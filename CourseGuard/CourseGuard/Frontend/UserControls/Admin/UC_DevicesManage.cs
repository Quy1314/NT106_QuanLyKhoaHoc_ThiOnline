using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Data;
using CourseGuard.Backend.Models;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Admin
{
    public class UC_DevicesManage : UserControl
    {
        private readonly CourseGuardDbContext _dbContext;
        private DataGridView _dgvDevices = new();
        private TextBox _txtSearch = new();
        private Button _btnBlock = new();
        private Button _btnUnblock = new();
        private Button _btnForceLogout = new();
        private Button _btnRefresh = new();
        private List<DeviceModel> _allDevices = new();

        public UC_DevicesManage()
        {
            _dbContext = new CourseGuardDbContext(string.Empty);
            BuildLayout();
            LoadDataAsync();
        }

        private void BuildLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = MetaTheme.Colors.FormBg;
            this.Padding = new Padding(20);

            // Container Panel
            var rootPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = MetaTheme.Colors.FormBg
            };

            // Title Label
            var lblTitle = new Label
            {
                Text = "Giám sát & Quản lý thiết bị truy cập",
                Font = MetaTheme.Fonts.HeadingLg(),
                ForeColor = MetaTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Top Search Bar Panel
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10)
            };

            _txtSearch.Width = 250;
            _txtSearch.Height = 35;
            _txtSearch.Font = MetaTheme.Fonts.BodyMd();
            _txtSearch.ForeColor = MetaTheme.Colors.TextPrimary;
            _txtSearch.BackColor = MetaTheme.Colors.InputBg;
            _txtSearch.BorderStyle = BorderStyle.FixedSingle;
            _txtSearch.Location = new Point(0, 12);
            _txtSearch.PlaceholderText = "Tìm theo tên máy, tài khoản...";
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) FilterDevices(); };

            var btnSearch = new Button
            {
                Text = "Tìm kiếm",
                Width = 100,
                Height = 32,
                Location = new Point(265, 11),
                Cursor = Cursors.Hand
            };
            MetaTheme.StylePrimaryButton(btnSearch);
            btnSearch.Click += (s, e) => FilterDevices();

            _btnRefresh.Text = "Tải lại";
            _btnRefresh.Width = 100;
            _btnRefresh.Height = 32;
            _btnRefresh.Location = new Point(375, 11);
            _btnRefresh.Cursor = Cursors.Hand;
            MetaTheme.StyleGhostButton(_btnRefresh);
            _btnRefresh.Click += (s, e) => LoadDataAsync();

            pnlSearch.Controls.Add(_txtSearch);
            pnlSearch.Controls.Add(btnSearch);
            pnlSearch.Controls.Add(_btnRefresh);

            // Middle DataGridView Panel
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 10)
            };

            _dgvDevices.Dock = DockStyle.Fill;
            _dgvDevices.AllowUserToAddRows = false;
            _dgvDevices.AllowUserToDeleteRows = false;
            _dgvDevices.ReadOnly = true;
            _dgvDevices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvDevices.MultiSelect = false;
            _dgvDevices.RowHeadersVisible = false;
            _dgvDevices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            _dgvDevices.Columns.Add("Id", "Mã TB");
            _dgvDevices.Columns.Add("Username", "Tài khoản");
            _dgvDevices.Columns.Add("UserFullName", "Họ tên");
            _dgvDevices.Columns.Add("DeviceName", "Tên thiết bị");
            _dgvDevices.Columns.Add("IpAddress", "Địa chỉ IP");
            _dgvDevices.Columns.Add("Status", "Trạng thái");
            _dgvDevices.Columns.Add("LastActive", "Hoạt động cuối");

            _dgvDevices.Columns["Id"].Width = 60;
            _dgvDevices.Columns["Id"].Visible = false;

            MetaTheme.StyleGrid(_dgvDevices);
            pnlGrid.Controls.Add(_dgvDevices);

            // Bottom Action Bar Panel
            var pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(0, 10, 0, 0)
            };

            _btnBlock.Text = "Khóa thiết bị";
            _btnBlock.Width = 140;
            _btnBlock.Height = 40;
            _btnBlock.Location = new Point(0, 10);
            _btnBlock.BackColor = MetaTheme.Colors.LogoutRed;
            _btnBlock.ForeColor = MetaTheme.Colors.TextPrimary;
            _btnBlock.FlatStyle = FlatStyle.Flat;
            _btnBlock.FlatAppearance.BorderSize = 0;
            _btnBlock.Cursor = Cursors.Hand;
            _btnBlock.Click += BtnBlock_Click;

            _btnUnblock.Text = "Mở khóa thiết bị";
            _btnUnblock.Width = 140;
            _btnUnblock.Height = 40;
            _btnUnblock.Location = new Point(155, 10);
            _btnUnblock.Cursor = Cursors.Hand;
            MetaTheme.StylePrimaryButton(_btnUnblock);
            _btnUnblock.Click += BtnUnblock_Click;

            _btnForceLogout.Text = "Đăng xuất từ xa";
            _btnForceLogout.Width = 140;
            _btnForceLogout.Height = 40;
            _btnForceLogout.Location = new Point(310, 10);
            _btnForceLogout.Cursor = Cursors.Hand;
            MetaTheme.StyleGhostButton(_btnForceLogout);
            _btnForceLogout.Click += BtnForceLogout_Click;

            RoundedButtonHelper.Apply(8, _btnBlock, _btnUnblock, _btnForceLogout, btnSearch, _btnRefresh);

            pnlActions.Controls.Add(_btnBlock);
            pnlActions.Controls.Add(_btnUnblock);
            pnlActions.Controls.Add(_btnForceLogout);

            // Assemble
            rootPanel.Controls.Add(pnlGrid);
            rootPanel.Controls.Add(pnlSearch);
            rootPanel.Controls.Add(pnlActions);
            rootPanel.Controls.Add(lblTitle);
            this.Controls.Add(rootPanel);
        }

        private async void LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.FormWithTable);
            try
            {
                _allDevices = await Task.Run(() => _dbContext.GetActiveDevices());
                if (IsDisposed || Disposing) return;
                BindDevices(_allDevices);
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể tải danh sách thiết bị: " + ex.Message, "Lỗi");
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void BindDevices(List<DeviceModel> devices)
        {
            _dgvDevices.Rows.Clear();
            foreach (var d in devices)
            {
                _dgvDevices.Rows.Add(
                    d.Id,
                    d.Username,
                    d.UserFullName,
                    d.DeviceName,
                    d.IpAddress,
                    d.Status,
                    d.LastActive.ToString("dd/MM/yyyy HH:mm:ss")
                );
            }
        }

        private void FilterDevices()
        {
            string keyword = _txtSearch.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                BindDevices(_allDevices);
                return;
            }

            var filtered = _allDevices.FindAll(d =>
                d.DeviceName.ToLowerInvariant().Contains(keyword) ||
                d.Username.ToLowerInvariant().Contains(keyword) ||
                d.UserFullName.ToLowerInvariant().Contains(keyword) ||
                d.IpAddress.ToLowerInvariant().Contains(keyword)
            );
            BindDevices(filtered);
        }

        private async void BtnBlock_Click(object? sender, EventArgs e)
        {
            if (_dgvDevices.SelectedRows.Count == 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một thiết bị để khóa.", "Thông báo");
                return;
            }

            int deviceId = Convert.ToInt32(_dgvDevices.SelectedRows[0].Cells["Id"].Value);
            string deviceName = _dgvDevices.SelectedRows[0].Cells["DeviceName"].Value.ToString() ?? "";
            string username = _dgvDevices.SelectedRows[0].Cells["Username"].Value.ToString() ?? "";

            if (MetaTheme.ShowModernDialog($"Bạn có chắc chắn muốn KHÓA thiết bị '{deviceName}' của tài khoản '{username}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                this.ShowSkeleton(SkeletonType.FormWithTable);
                try
                {
                    bool success = await Task.Run(() => _dbContext.UpdateDeviceStatus(deviceId, "BLOCKED"));
                    if (success)
                    {
                        MetaTheme.ShowModernDialog("Khóa thiết bị thành công!", "Thành công");
                        LoadDataAsync();
                    }
                    else
                    {
                        MetaTheme.ShowModernDialog("Không thể cập nhật trạng thái thiết bị.", "Thất bại");
                    }
                }
                finally
                {
                    this.HideSkeleton();
                }
            }
        }

        private async void BtnUnblock_Click(object? sender, EventArgs e)
        {
            if (_dgvDevices.SelectedRows.Count == 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một thiết bị để mở khóa.", "Thông báo");
                return;
            }

            int deviceId = Convert.ToInt32(_dgvDevices.SelectedRows[0].Cells["Id"].Value);
            string deviceName = _dgvDevices.SelectedRows[0].Cells["DeviceName"].Value.ToString() ?? "";
            string username = _dgvDevices.SelectedRows[0].Cells["Username"].Value.ToString() ?? "";

            if (MetaTheme.ShowModernDialog($"Bạn có chắc chắn muốn MỞ KHÓA thiết bị '{deviceName}' của tài khoản '{username}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.ShowSkeleton(SkeletonType.FormWithTable);
                try
                {
                    bool success = await Task.Run(() => _dbContext.UpdateDeviceStatus(deviceId, "ACTIVE"));
                    if (success)
                    {
                        MetaTheme.ShowModernDialog("Mở khóa thiết bị thành công!", "Thành công");
                        LoadDataAsync();
                    }
                    else
                    {
                        MetaTheme.ShowModernDialog("Không thể cập nhật trạng thái thiết bị.", "Thất bại");
                    }
                }
                finally
                {
                    this.HideSkeleton();
                }
            }
        }

        private async void BtnForceLogout_Click(object? sender, EventArgs e)
        {
            if (_dgvDevices.SelectedRows.Count == 0)
            {
                MetaTheme.ShowModernDialog("Vui lòng chọn một thiết bị để đăng xuất cưỡng bức.", "Thông báo");
                return;
            }

            int deviceId = Convert.ToInt32(_dgvDevices.SelectedRows[0].Cells["Id"].Value);
            string deviceName = _dgvDevices.SelectedRows[0].Cells["DeviceName"].Value.ToString() ?? "";
            string username = _dgvDevices.SelectedRows[0].Cells["Username"].Value.ToString() ?? "";

            if (MetaTheme.ShowModernDialog($"Bạn có chắc chắn muốn ĐĂNG XUẤT CƯỠNG BỨC thiết bị '{deviceName}' của tài khoản '{username}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                this.ShowSkeleton(SkeletonType.FormWithTable);
                try
                {
                    bool success = await Task.Run(() => _dbContext.DeleteDevice(deviceId));
                    if (success)
                    {
                        MetaTheme.ShowModernDialog("Đăng xuất cưỡng bức thành công!", "Thành công");
                        LoadDataAsync();
                    }
                    else
                    {
                        MetaTheme.ShowModernDialog("Không thể xóa phiên thiết bị.", "Thất bại");
                    }
                }
                finally
                {
                    this.HideSkeleton();
                }
            }
        }
    }
}
