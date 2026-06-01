using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Data_Manager
{
    public partial class PliotModelList : Form
    {
        // 전체 모델 목록 및 필터 적용 후 표시 목록
        private readonly List<ModelListItem> allModels = new List<ModelListItem>();
        private readonly List<ModelListItem> visibleModels = new List<ModelListItem>();

        // 마지막으로 불러온 폴더 (필터 초기화 시 재로드용)
        private string? lastLoadedFolderPath;

        // 모델 이름과 파일 경로를 함께 전달합니다.
        public event Action<string, string>? ModelSelected;

        public PliotModelList()
        {
            InitializeComponent();
            btnModelFliter.Text = "검색";
            btnResetFilter.Text = "초기화";
            btnModelLoad.Text = "불러오기";
            btnModelFliter.Click += BtnModelFliter_Click;
            btnResetFilter.Click += BtnResetFilter_Click;
            btnModelLoad.Click += BtnModelLoad_Click;
            lstModelList.DoubleClick += LstModelList_DoubleClick;
            lstModelList.KeyDown += LstModelList_KeyDown;
            KeyPreview = true;
            KeyDown += PliotModelList_KeyDown;
        }

        // frmNewtrainer 리스트박스에서 모델을 가져오는 진입점
        public void LoadFromTrainerList()
        {
            allModels.Clear();

            // TODO: frmNewtrainer의 리스트박스에서 항목을 복사해 allModels에 채우기
            // 예: allModels.Add(new ModelListItem("모델명", "모델경로", 원본아이템));

            ApplyFilter(txtModelFilter.Text);
        }

        // 모델 폴더 선택 후 h5 파일 목록 로드
        private async void BtnModelLoad_Click(object? sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "모델 파일 선택";
                dialog.Filter = "Model Files (*.h5;*.keras;*.tflite)|*.h5;*.keras;*.tflite|All Files (*.*)|*.*";

                DonkeyAsyncWorker.OperationResult<string> homeResult =
                    await DonkeyAsyncWorker.GetWslHomePathAsync(
                        "Ubuntu-22.04",
                        null,
                        CancellationToken.None);

                if (homeResult.Success && !string.IsNullOrWhiteSpace(homeResult.Data))
                {
                    dialog.InitialDirectory = homeResult.Data;
                }

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string folderPath = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    LoadModelsFromFolder(folderPath);
                }
            }
        }

        // 필터 초기화 및 마지막 폴더 재로드 (파일 갱신 반영)
        private void BtnResetFilter_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lastLoadedFolderPath)
                || !Directory.Exists(lastLoadedFolderPath))
            {
                ApplyFilter(string.Empty);
                return;
            }

            LoadModelsFromFolder(lastLoadedFolderPath);
        }

        private void PliotModelList_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnModelFliter.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                btnResetFilter.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LstModelList_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Tab)
            {
                return;
            }

            SelectNextControl(
                lstModelList,
                !e.Shift,
                true,
                true,
                true);

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        // 현재 입력값으로 필터 적용
        private void BtnModelFliter_Click(object? sender, EventArgs e)
        {
            ApplyFilter(txtModelFilter.Text);
        }

        // 모델 선택 후 호출(더블클릭)
        private void LstModelList_DoubleClick(object? sender, EventArgs e)
        {
            if (lstModelList.SelectedIndex < 0 || lstModelList.SelectedIndex >= visibleModels.Count)
            {
                return;
            }

            ModelListItem model = visibleModels[lstModelList.SelectedIndex];

            ModelSelected?.Invoke(model.Name, model.Path);
        }

        // 필터 문자열을 기준으로 리스트 표시 갱신
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

        // 선택한 폴더 내 h5 파일을 목록으로 로드
        private void LoadModelsFromFolder(string folderPath)
        {
            allModels.Clear();
            lastLoadedFolderPath = folderPath;

            foreach (string file in Directory.EnumerateFiles(folderPath, "*.h5"))
            {
                string name = Path.GetFileNameWithoutExtension(file);

                // TODO: 다른 열/속성 파싱 구조 확정 후 ModelListItem에 추가
                allModels.Add(new ModelListItem(name, file, null));
            }

            ApplyFilter(txtModelFilter.Text);
        }

        // 현재는 이름 열만 필터링 대상으로 사용
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

            // 이름 열 (현재 필터 기준)
            public string Name { get; }

            // 모델 경로
            public string Path { get; }

            // 원본 항목 보관 (추후 매핑용)
            public object? SourceItem { get; }

            public string DisplayText => Name;
        }
    }
}
