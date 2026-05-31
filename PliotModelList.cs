using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Data_Manager
{
    public partial class PliotModelList : Form
    {
        private readonly List<ModelListItem> allModels = new List<ModelListItem>();
        private readonly List<ModelListItem> visibleModels = new List<ModelListItem>();

        public event Action<string>? ModelSelected;

        public PliotModelList()
        {
            InitializeComponent();
            btnModelFliter.Text = "검색";
            btnModelLoad.Text = "불러오기";
            btnModelFliter.Click += BtnModelFliter_Click;
            btnModelLoad.Click += BtnModelLoad_Click;
            lstModelList.DoubleClick += LstModelList_DoubleClick;
        }

        public void LoadFromTrainerList()
        {
            allModels.Clear();

            // TODO: frmNewtrainer의 리스트박스에서 항목을 복사해 allModels에 채우기
            // 예: allModels.Add(new ModelListItem("모델명", "모델경로", 원본아이템));

            ApplyFilter(txtModelFilter.Text);
        }

        private void BtnModelLoad_Click(object? sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "모델 폴더 선택";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                LoadModelsFromFolder(dialog.SelectedPath);
            }
        }

        private void BtnModelFliter_Click(object? sender, EventArgs e)
        {
            ApplyFilter(txtModelFilter.Text);
        }

        private void LstModelList_DoubleClick(object? sender, EventArgs e)
        {
            if (lstModelList.SelectedIndex < 0 || lstModelList.SelectedIndex >= visibleModels.Count)
            {
                return;
            }

            ModelListItem model = visibleModels[lstModelList.SelectedIndex];

            ModelSelected?.Invoke(model.Path);
        }

        private void ApplyFilter(string? filterText)
        {
            string normalizedFilter = (filterText ?? string.Empty).Trim();

            lstModelList.Items.Clear();
            visibleModels.Clear();

            foreach (ModelListItem model in allModels)
            {
                if (!IsMatch(model, normalizedFilter))
                {
                    continue;
                }

                visibleModels.Add(model);
                lstModelList.Items.Add(new MaterialSkin.MaterialListBoxItem(model.DisplayText));
            }
        }

        private void LoadModelsFromFolder(string folderPath)
        {
            allModels.Clear();

            foreach (string file in Directory.EnumerateFiles(folderPath, "*.h5"))
            {
                string name = Path.GetFileNameWithoutExtension(file);

                // TODO: 다른 열/속성 파싱 구조 확정 후 ModelListItem에 추가
                allModels.Add(new ModelListItem(name, file, null));
            }

            ApplyFilter(txtModelFilter.Text);
        }

        private static bool IsMatch(ModelListItem model, string normalizedFilter)
        {
            if (string.IsNullOrWhiteSpace(normalizedFilter))
            {
                return true;
            }

            return model.Name.Contains(normalizedFilter, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class ModelListItem
        {
            public ModelListItem(string name, string path, object? source)
            {
                Name = name;
                Path = path;
                SourceItem = source;
            }

            public string Name { get; }

            public string Path { get; }

            public object? SourceItem { get; }

            public string DisplayText => Name;
        }
    }
}
