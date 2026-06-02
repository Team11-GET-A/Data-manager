using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Data_Manager
{
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
        private static readonly string RegistryPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "DonkeyDataManager",
                "models.json");

        public static event EventHandler? ModelsChanged;

        public static List<SharedModelRegistryEntry> Load()
        {
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
