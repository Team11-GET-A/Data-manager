using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Data_Manager
{
    // Trainer와 Pilot 화면이 함께 사용하는 모델 등록 정보입니다.
    // 학습된 .h5 파일 경로와 원본 tub 경로를 저장해 화면 간 모델 목록을 동기화합니다.
    public sealed class SharedModelRegistryEntry
    {
        public string Name { get; set; } = "";

        public string WindowsPath { get; set; } = "";

        public string WslPath { get; set; } = "";

        public string SourceTubWindowsPath { get; set; } = "";

        public string SourceTubWslPath { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }

    public static class SharedModelRegistry
    {
        // 사용자 AppData에 저장하여 프로그램 실행 위치가 바뀌어도 모델 목록을 유지합니다.
        private static readonly string RegistryPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "DonkeyDataManager",
                "models.json");

        public static event EventHandler? ModelsChanged;

        public static List<SharedModelRegistryEntry> Load()
        {
            // 실제 파일이 남아 있는 .h5 모델만 유효한 항목으로 반환합니다.
            try
            {
                if (!File.Exists(RegistryPath))
                {
                    return new List<SharedModelRegistryEntry>();
                }

                string json =
                    File.ReadAllText(RegistryPath);

                return
                    (JsonSerializer.Deserialize<List<SharedModelRegistryEntry>>(json) ??
                    new List<SharedModelRegistryEntry>())
                    .Where(IsValidModel)
                    .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return new List<SharedModelRegistryEntry>();
            }
        }

        public static void Upsert(
            SharedModelRegistryEntry entry)
        {
            if (!IsValidModel(entry))
            {
                return;
            }

            List<SharedModelRegistryEntry> entries =
                Load();

            entries.RemoveAll(
                item =>
                    string.Equals(
                        item.WindowsPath,
                        entry.WindowsPath,
                        StringComparison.OrdinalIgnoreCase));

            entries.Add(entry);

            Save(entries);
        }

        public static void NotifyChanged()
        {
            ModelsChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void Save(
            List<SharedModelRegistryEntry> entries)
        {
            string? directory =
                Path.GetDirectoryName(RegistryPath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            JsonSerializerOptions options =
                new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

            File.WriteAllText(
                RegistryPath,
                JsonSerializer.Serialize(entries, options));

            NotifyChanged();
        }

        private static bool IsValidModel(
            SharedModelRegistryEntry entry)
        {
            return
                entry != null &&
                !string.IsNullOrWhiteSpace(entry.WindowsPath) &&
                File.Exists(entry.WindowsPath) &&
                string.Equals(
                    Path.GetExtension(entry.WindowsPath),
                    ".h5",
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}
