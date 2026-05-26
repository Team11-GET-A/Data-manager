using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using System.IO;

namespace Data_Manager
{
    public partial class Pliot : Form
    {
        private int pilotCardCount = 0;

        private const int CardGap = 5;

        private const int MaxCards = 4;

        private const int CardHeight = 552;

        private readonly Dictionary<Control, Rectangle>
            panel1BaseBounds =
            new Dictionary<Control, Rectangle>();

        private readonly Dictionary<Control, Rectangle>
            panel2BaseBounds =
            new Dictionary<Control, Rectangle>();

        private Size panel1BaseSize;

        private Size panel2BaseSize;

        // =====================================================
        // 데이터 변수
        // =====================================================

        private string selectedDataPath = "";

        private List<CatalogRecord>
            integratedCatalogList =
            new List<CatalogRecord>();

        // =====================================================
        // 데이터 구조
        // =====================================================

        public class CatalogRecord
        {
            public string OriginalLine { get; set; }

            public string SourceFilePath { get; set; }

            public int LineIndex { get; set; }

            public string ImageFileName { get; set; }

            public string Angle { get; set; }

            public string Throttle { get; set; }

            public string Index { get; set; }
        }

        // =====================================================
        // 생성자
        // =====================================================

        public Pliot()
        {
            InitializeComponent();

            flowLayoutPanel1.SizeChanged +=
                (s, e) => UpdatePilotCardLayout();

            flowLayoutPanel1.Layout +=
                (s, e) => UpdatePilotCardLayout();

            Shown += Form2_Shown;

            Resize += Form2_Resize;
        }

        // =====================================================
        // SHOWN
        // =====================================================

        private void Form2_Shown(
            object sender,
            EventArgs e)
        {
            CapturePanelLayout(
                panel1,
                panel1BaseBounds,
                ref panel1BaseSize);

            CapturePanelLayout(
                panel2,
                panel2BaseBounds,
                ref panel2BaseSize);

            UpdatePilotCardLayout();
        }

        // =====================================================
        // RESIZE
        // =====================================================

        private void Form2_Resize(
            object sender,
            EventArgs e)
        {
            ApplyPanelScale(
                panel1,
                panel1BaseBounds,
                panel1BaseSize);

            ApplyPanelScale(
                panel2,
                panel2BaseBounds,
                panel2BaseSize);

            UpdatePilotCardLayout();
        }

        // =====================================================
        // CARD ADD
        // =====================================================

        private void BtnCardAdder_Click(
            object sender,
            EventArgs e)
        {
            if (
                flowLayoutPanel1.Controls.Count
                >= MaxCards)
            {
                MessageBox.Show(
                    "파일럿 카드는 최대 4개까지만 추가할 수 있습니다.");

                return;
            }

            PilotCardControl card =
                new PilotCardControl();

            pilotCardCount++;

            card.SetModelName(
                $"테스트 모델 {pilotCardCount}");

            card.SetAngles(
                -15.2 + pilotCardCount * 2,
                -12.6 + pilotCardCount * 1.5);

            card.SetThrottles(
                0.72 - (pilotCardCount * 0.1),
                0.65 - (pilotCardCount * 0.05));

            card.RemoveRequested +=
                Card_RemoveRequested;

            card.ModelSelectRequested +=
                Card_ModelSelectRequested;

            flowLayoutPanel1.Controls.Add(card);

            BeginInvoke(
                new Action(UpdatePilotCardLayout));
        }

        // =====================================================
        // REMOVE
        // =====================================================

        private void Card_RemoveRequested(
            PilotCardControl card)
        {
            flowLayoutPanel1.Controls.Remove(card);

            card.Dispose();

            UpdatePilotCardLayout();
        }

        // =====================================================
        // MODEL SELECT
        // =====================================================

        private void Card_ModelSelectRequested(
            PilotCardControl card)
        {
            MessageBox.Show(
                "추후 모델 선택 로직이 연동될 예정입니다.");
        }

        // =====================================================
        // PANEL CAPTURE
        // =====================================================

        private void CapturePanelLayout(
            Panel panel,
            Dictionary<Control, Rectangle> bounds,
            ref Size baseSize)
        {
            bounds.Clear();

            baseSize = panel.ClientSize;

            foreach (Control control
                in panel.Controls)
            {
                bounds[control] =
                    control.Bounds;
            }
        }

        // =====================================================
        // PANEL SCALE
        // =====================================================

        private void ApplyPanelScale(
            Panel panel,
            Dictionary<Control, Rectangle> bounds,
            Size baseSize)
        {
            if (
                baseSize.Width == 0 ||
                baseSize.Height == 0)
            {
                return;
            }

            float scaleX =
                panel.ClientSize.Width /
                (float)baseSize.Width;

            float scaleY =
                panel.ClientSize.Height /
                (float)baseSize.Height;

            foreach (var entry in bounds)
            {
                Rectangle original =
                    entry.Value;

                entry.Key.Bounds =
                    new Rectangle(
                        (int)Math.Round(
                            original.X * scaleX),

                        (int)Math.Round(
                            original.Y * scaleY),

                        (int)Math.Round(
                            original.Width * scaleX),

                        (int)Math.Round(
                            original.Height * scaleY));
            }
        }

        // =====================================================
        // CARD LAYOUT
        // =====================================================

        private void UpdatePilotCardLayout()
        {
            int count =
                flowLayoutPanel1.Controls.Count;

            if (count == 0)
            {
                return;
            }

            int availableWidth =
                flowLayoutPanel1.ClientSize.Width -
                flowLayoutPanel1.Padding.Left -
                flowLayoutPanel1.Padding.Right;

            int totalGap =
                CardGap * (count - 1);

            int cardWidth =
                (availableWidth - totalGap) / count;

            int availableHeight =
                flowLayoutPanel1.ClientSize.Height -
                flowLayoutPanel1.Padding.Top -
                flowLayoutPanel1.Padding.Bottom;

            int cardHeight =
                availableHeight;

            foreach (Control control
                in flowLayoutPanel1.Controls)
            {
                control.Margin =
                    new Padding(0, 0, CardGap, 0);

                control.Size =
                    new Size(cardWidth, cardHeight);
            }

            if (
                flowLayoutPanel1.Controls.Count > 0)
            {
                flowLayoutPanel1.Controls[^1]
                    .Margin = new Padding(0);
            }
        }

        // =====================================================
        // LOAD DATA BUTTON
        // =====================================================

        private void btnLoadData1_Click(
            object sender,
            EventArgs e)
        {
            using (FolderBrowserDialog fbd =
                new FolderBrowserDialog())
            {
                fbd.Description =
                    "mycar/data 폴더 선택";

                if (
                    fbd.ShowDialog() ==
                    DialogResult.OK)
                {
                    selectedDataPath =
                        fbd.SelectedPath;

                    integratedCatalogList.Clear();

                    string[] catalogFiles =
                        Directory.GetFiles(
                            selectedDataPath,
                            "catalog_*.catalog");

                    Array.Sort(catalogFiles);

                    foreach (
                        string catalogPath
                        in catalogFiles)
                    {
                        string[] lines =
                            File.ReadAllLines(
                                catalogPath);

                        for (
                            int i = 0;
                            i < lines.Length;
                            i++)
                        {
                            string line =
                                lines[i];

                            if (
                                string.IsNullOrWhiteSpace(
                                    line))
                            {
                                continue;
                            }

                            CatalogRecord record =
                                new CatalogRecord()
                                {
                                    OriginalLine =
                                        line,

                                    SourceFilePath =
                                        catalogPath,

                                    LineIndex =
                                        i,

                                    ImageFileName =
                                        ExtractJsonValue(
                                            line,
                                            "cam/image_array"),

                                    Angle =
                                        ExtractJsonValue(
                                            line,
                                            "user/angle"),

                                    Throttle =
                                        ExtractJsonValue(
                                            line,
                                            "user/throttle"),

                                    Index =
                                        ExtractJsonValue(
                                            line,
                                            "_index")
                                };

                            integratedCatalogList
                                .Add(record);
                        }
                    }

                    MessageBox.Show(
                        $"총 {integratedCatalogList.Count}개 프레임 로드 완료");
                }
            }
        }

        // =====================================================
        // JSON VALUE
        // =====================================================

        private string ExtractJsonValue(
            string json,
            string key)
        {
            try
            {
                string searchKey =
                    $"\"{key}\":";

                int startIdx =
                    json.IndexOf(searchKey);

                if (startIdx == -1)
                    return "";

                startIdx +=
                    searchKey.Length;

                while (
                    startIdx < json.Length &&
                    json[startIdx] == ' ')
                {
                    startIdx++;
                }

                if (json[startIdx] == '"')
                {
                    startIdx++;

                    int endIdx =
                        json.IndexOf(
                            '"',
                            startIdx);

                    return json.Substring(
                        startIdx,
                        endIdx - startIdx);
                }
                else
                {
                    int endIdx =
                        json.IndexOfAny(
                            new char[]
                            {
                                ',',
                                '}'
                            },
                            startIdx);

                    return json.Substring(
                        startIdx,
                        endIdx - startIdx)
                        .Trim();
                }
            }
            catch
            {
                return "";
            }
        }
    }
}