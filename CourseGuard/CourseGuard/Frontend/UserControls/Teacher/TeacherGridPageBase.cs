using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using CourseGuard.Backend.Controllers;
using CourseGuard.Backend.Data;
using CourseGuard.Frontend.Theme;

namespace CourseGuard.Frontend.UserControls.Teacher
{
    public abstract class TeacherGridPageBase : UserControl
    {
        protected readonly int TeacherId;
        protected readonly TeacherController Controller = new(new CourseGuardDbContext(""));
        protected readonly DataGridView Grid = new();
        protected readonly BindingSource BindingSource = new();
        protected readonly Button RefreshButton = TeacherTabChrome.SecondaryButton("Tải lại");
        protected readonly Button AddButton = TeacherTabChrome.PrimaryButton("Thêm");
        protected readonly Button EditButton = TeacherTabChrome.SecondaryButton("Sửa");
        protected readonly Button DeleteButton = TeacherTabChrome.DangerButton("Xóa");

        protected TeacherGridPageBase(int teacherId, string title, string subtitle, string cardTitle, bool showCrud = true)
        {
            TeacherId = teacherId;
            InitializeComponent(title, subtitle, cardTitle, showCrud);
            RefreshButton.Click += async (_, _) => await LoadDataAsync();
            AddButton.Click += async (_, _) => await AddAsync();
            EditButton.Click += async (_, _) => await EditAsync();
            DeleteButton.Click += async (_, _) => await DeleteAsync();
            _ = LoadDataAsync();
        }

        protected virtual Task AddAsync() => Task.CompletedTask;
        protected virtual Task EditAsync() => Task.CompletedTask;
        protected virtual Task DeleteAsync() => Task.CompletedTask;
        protected abstract Task<DataTable> CreateTableAsync();

        protected void AddHeaderAction(Control action)
        {
            FlowLayoutPanel? panel = FindActionPanel(this);
            if (panel == null)
                return;

            action.Margin = new Padding(8, 0, 0, 0);
            panel.Controls.Add(action);
        }

        protected int CurrentInt(string columnName)
        {
            if (Grid.CurrentRow == null || Grid.CurrentRow.IsNewRow)
                return 0;
            object? value = Grid.CurrentRow.Cells[columnName].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        protected string CurrentString(string columnName)
        {
            if (Grid.CurrentRow == null || Grid.CurrentRow.IsNewRow)
                return string.Empty;
            return Grid.CurrentRow.Cells[columnName].Value?.ToString() ?? string.Empty;
        }

        protected async Task LoadDataAsync()
        {
            this.ShowSkeleton(SkeletonType.TableWithToolbar);
            try
            {
                BindingSource.DataSource = await CreateTableAsync();
                Grid.DataSource = BindingSource;
                foreach (DataGridViewColumn column in Grid.Columns)
                {
                    if (column.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || column.Name == "ID")
                        column.Visible = false;
                }
                Grid.ClearSelection();
                Grid.CurrentCell = null;
            }
            catch (Exception ex)
            {
                MetaTheme.ShowModernDialog("Không thể tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.HideSkeleton();
            }
        }

        private void InitializeComponent(string title, string subtitle, string cardTitle, bool showCrud)
        {
            var root = TeacherTabChrome.CreateRoot(this);
            Control[] actions = showCrud
                ? new Control[] { AddButton, EditButton, DeleteButton, RefreshButton }
                : new Control[] { RefreshButton };
            root.Controls.Add(TeacherTabChrome.CreateHeader(title, subtitle, actions), 0, 0);
            TeacherTabChrome.StyleGrid(Grid);
            root.Controls.Add(TeacherTabChrome.CreateDataCard(cardTitle, Grid), 0, 1);
        }

        private static FlowLayoutPanel? FindActionPanel(Control root)
        {
            foreach (Control child in root.Controls)
            {
                if (child is FlowLayoutPanel panel)
                    return panel;

                FlowLayoutPanel? nested = FindActionPanel(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }
    }
}
