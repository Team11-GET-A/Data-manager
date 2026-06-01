using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data_Manager
{
    public partial class PliotModelList : Form
    {
        private readonly List<ModelListItem> _allModels = new List<ModelListItem>();
        private readonly List<ModelListItem> _visibleModels = new List<ModelListItem>();

        public event Action<string, string>? ModelSelected;

        public PliotModelList()
        {
            InitializeComponent();
            btnModelFliter.Click += BtnModelFliter_Click;
            btnResetFilter.Click += BtnResetFilter_Click;
            btnModelLoad.Click += BtnModelLoad_Click;
            lvModelList.DoubleClick += LvModelList_DoubleClick;
            lvModelList.KeyDown += LvModelList_KeyDown;
            lvModelList.SizeChanged += (s, e) => ResizeColumns();
            KeyPreview = true;
            KeyDown += PliotModelList_KeyDown;
            ResizeColumns();
        }

        public void LoadFromTrainerList()
        {
            _allModels.Clear();
            ApplyFilter(txtModelFilter.Text);
        }

        private async void BtnModelLoad_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "모델 파일 선택";
            dialog.Filter = "Model Files (*.h5;*.keras;*.tflite)|*.h5;*.keras;*.tflite|All Files (*.*)|*.*";
            dialog.Multiselect = false;

            string initialDirectory = await GetWslHomeInitialDirectoryAsync();
            if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
            {
                dialog.InitialDirectory = initialDirectory;
            }
            else
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            AddModelFile(dialog.FileName);
            ApplyFilter(txtModelFilter.Text);
            SelectModelByPath(dialog.FileName);
        }

        private static async Task<string> GetWslHomeInitialDirectoryAsync()
        {
            DonkeyAsyncWorker.OperationResult<string> homeResult =
                await DonkeyAsyncWorker.GetWslHomePathAsync(
                    await DonkeyAsyncWorker.GetPreferredWslDistroNameAsync(CancellationToken.None),
                    null,
                    CancellationToken.None);

            return homeResult.Success ? homeResult.Data ?? string.Empty : string.Empty;
        }

        private void AddModelFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            string extension = Path.GetExtension(filePath);
            if (!IsSupportedModelExtension(extension))
            {
                return;
            }

            if (_allModels.Any(model =>
                string.Equals(model.Path, filePath, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            string name = Path.GetFileNameWithoutExtension(filePath);
            _allModels.Add(new ModelListItem(name, filePath));
        }

        private static bool IsSupportedModelExtension(string extension)
        {
            return string.Equals(extension, ".h5", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".keras", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".tflite", StringComparison.OrdinalIgnoreCase);
        }

        private void BtnResetFilter_Click(object? sender, EventArgs e)
        {
            txtModelFilter.Clear();
            ApplyFilter(string.Empty);
        }

        private void BtnModelFliter_Click(object? sender, EventArgs e)
        {
            ApplyFilter(txtModelFilter.Text);
        }

        private void PliotModelList_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (lvModelList.Focused && lvModelList.SelectedItems.Count > 0)
                {
                    RaiseSelectedModel();
                }
                else
                {
                    ApplyFilter(txtModelFilter.Text);
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                BtnResetFilter_Click(sender, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LvModelList_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                RaiseSelectedModel();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode != Keys.Tab)
            {
                return;
            }

            SelectNextControl(lvModelList, !e.Shift, true, true, true);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void LvModelList_DoubleClick(object? sender, EventArgs e)
        {
            RaiseSelectedModel();
        }

        private void RaiseSelectedModel()
        {
            if (lvModelList.SelectedItems.Count == 0)
            {
                return;
            }

            if (lvModelList.SelectedItems[0].Tag is not ModelListItem model)
            {
                return;
            }

            ModelSelected?.Invoke(model.Name, model.Path);
        }

        private void ApplyFilter(string? filterText)
        {
            string normalizedFilter = (filterText ?? string.Empty).Trim();
            _visibleModels.Clear();
            lvModelList.Items.Clear();

            foreach (ModelListItem model in _allModels)
            {
                if (!IsMatch(model, normalizedFilter))
                {
                    continue;
                }

                _visibleModels.Add(model);
                ListViewItem item = new ListViewItem(_visibleModels.Count.ToString());
                item.SubItems.Add(model.Name);
                item.SubItems.Add(model.Path);
                item.Tag = model;
                lvModelList.Items.Add(item);
            }

            ResizeColumns();
        }

        private void SelectModelByPath(string filePath)
        {
            foreach (ListViewItem item in lvModelList.Items)
            {
                if (item.Tag is ModelListItem model
                    && string.Equals(model.Path, filePath, StringComparison.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    break;
                }
            }
        }

        private static bool IsMatch(ModelListItem model, string normalizedFilter)
        {
            if (string.IsNullOrWhiteSpace(normalizedFilter))
            {
                return true;
            }

            return model.Name.Contains(normalizedFilter, StringComparison.OrdinalIgnoreCase)
                || model.Path.Contains(normalizedFilter, StringComparison.OrdinalIgnoreCase);
        }

        private void ResizeColumns()
        {
            int width = Math.Max(600, lvModelList.ClientSize.Width);
            colNo.Width = Math.Max(50, width / 8);
            colName.Width = Math.Max(160, width * 3 / 8);
            colPath.Width = Math.Max(240, width - colNo.Width - colName.Width - 8);
        }

        private sealed class ModelListItem
        {
            public ModelListItem(string name, string path)
            {
                Name = name;
                Path = path;
            }

            public string Name { get; }
            public string Path { get; }
        }
    }
}
