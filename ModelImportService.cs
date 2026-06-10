using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Data_Manager
{
    public sealed class ModelImportResult
    {
        public string DestinationModelPath { get; set; } = "";

        public string ModelFileName { get; set; } = "";
    }

    public static class ModelImportService
    {
        public static ModelImportResult ImportModelToFolder(
            string sourceModelPath,
            string destinationModelsPath)
        {
            sourceModelPath = Path.GetFullPath(sourceModelPath);
            destinationModelsPath = Path.GetFullPath(destinationModelsPath);

            if (!File.Exists(sourceModelPath))
            {
                throw new FileNotFoundException("선택한 모델 파일을 찾지 못했습니다.", sourceModelPath);
            }

            if (!string.Equals(
                Path.GetExtension(sourceModelPath),
                ".h5",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(".h5 모델 파일만 가져올 수 있습니다.");
            }

            if (!HasDonkeyModelDatabaseEntry(sourceModelPath, out string databaseErrorMessage))
            {
                throw new InvalidOperationException(databaseErrorMessage);
            }

            Directory.CreateDirectory(destinationModelsPath);

            string modelFileName = Path.GetFileName(sourceModelPath);
            string destinationModelPath = Path.Combine(destinationModelsPath, modelFileName);
            List<string> relatedFiles = GetRelatedModelFilesFromPath(sourceModelPath);

            if (relatedFiles.Count == 0)
            {
                relatedFiles.Add(sourceModelPath);
            }

            foreach (string sourceFile in relatedFiles)
            {
                string destinationFile =
                    Path.Combine(
                        destinationModelsPath,
                        Path.GetFileName(sourceFile));

                if (!string.Equals(sourceFile, destinationFile, StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(destinationFile))
                {
                    throw new IOException(
                        "같은 이름의 모델 또는 관련 파일이 이미 존재합니다.\n" +
                        destinationFile);
                }
            }

            foreach (string sourceFile in relatedFiles)
            {
                string destinationFile =
                    Path.Combine(
                        destinationModelsPath,
                        Path.GetFileName(sourceFile));

                if (!string.Equals(sourceFile, destinationFile, StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(sourceFile, destinationFile, overwrite: false);
                }
            }

            ImportDonkeyModelDatabaseEntry(sourceModelPath, destinationModelPath);

            return new ModelImportResult
            {
                DestinationModelPath = destinationModelPath,
                ModelFileName = modelFileName
            };
        }

        public static bool IsPathInsideDirectory(
            string path,
            string directory)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                string.IsNullOrWhiteSpace(directory))
            {
                return false;
            }

            string fullPath =
                Path.GetFullPath(path)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string fullDirectory =
                Path.GetFullPath(directory)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return
                string.Equals(fullPath, fullDirectory, StringComparison.OrdinalIgnoreCase) ||
                fullPath.StartsWith(
                    fullDirectory + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase) ||
                fullPath.StartsWith(
                    fullDirectory + Path.AltDirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase);
        }

        public static List<string> GetRelatedModelFilesFromPath(string modelPath)
        {
            List<string> files = new List<string>();

            if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
            {
                return files;
            }

            string modelFolder = Path.GetDirectoryName(modelPath) ?? "";
            string modelFileName = Path.GetFileName(modelPath);
            string modelBaseName = Path.GetFileNameWithoutExtension(modelPath);

            if (Directory.Exists(modelFolder))
            {
                foreach (string file in Directory.GetFiles(modelFolder))
                {
                    string fileName = Path.GetFileName(file);

                    if (string.Equals(fileName, "database.json", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (IsRelatedModelFileName(fileName, modelFileName, modelBaseName))
                    {
                        files.Add(file);
                    }
                }
            }

            if (File.Exists(modelPath))
            {
                files.Add(modelPath);
            }

            return files
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static bool HasDonkeyModelDatabaseEntry(
            string sourceModelPath,
            out string errorMessage)
        {
            errorMessage = "";

            string sourceDatabasePath =
                Path.Combine(
                    Path.GetDirectoryName(sourceModelPath) ?? "",
                    "database.json");

            if (!File.Exists(sourceDatabasePath))
            {
                errorMessage =
                    "원본 모델 폴더에서 database.json 파일을 찾지 못했습니다.\n" +
                    sourceDatabasePath;

                return false;
            }

            JsonNode? sourceRoot;

            try
            {
                sourceRoot = JsonNode.Parse(File.ReadAllText(sourceDatabasePath));
            }
            catch (Exception ex)
            {
                errorMessage =
                    "database.json 파일을 읽는 중 오류가 발생했습니다.\n" +
                    ex.Message;

                return false;
            }

            if (sourceRoot is not JsonArray sourceArray)
            {
                errorMessage = "database.json 파일 구조가 올바르지 않습니다.";
                return false;
            }

            string sourceModelName = Path.GetFileName(sourceModelPath);
            string sourceModelBaseName = Path.GetFileNameWithoutExtension(sourceModelPath);

            bool hasEntry =
                sourceArray
                    .OfType<JsonObject>()
                    .Any(
                        modelObject =>
                            IsDonkeyModelDatabaseEntry(
                                modelObject,
                                sourceModelBaseName,
                                sourceModelName));

            if (!hasEntry)
            {
                errorMessage =
                    "database.json 파일에서 선택한 모델 데이터를 찾지 못했습니다.\n" +
                    sourceModelName;
            }

            return hasEntry;
        }

        public static void ImportDonkeyModelDatabaseEntry(
            string sourceModelPath,
            string destinationModelPath)
        {
            string sourceDatabasePath =
                Path.Combine(
                    Path.GetDirectoryName(sourceModelPath) ?? "",
                    "database.json");

            string destinationModelsPath =
                Path.GetDirectoryName(destinationModelPath) ?? "";

            if (string.IsNullOrWhiteSpace(destinationModelsPath))
            {
                return;
            }

            Directory.CreateDirectory(destinationModelsPath);

            string destinationDatabasePath =
                Path.Combine(
                    destinationModelsPath,
                    "database.json");

            JsonNode? sourceRoot =
                JsonNode.Parse(File.ReadAllText(sourceDatabasePath));

            if (sourceRoot is not JsonArray sourceArray)
            {
                throw new InvalidOperationException("database.json 파일 구조가 올바르지 않습니다.");
            }

            string sourceModelName = Path.GetFileName(sourceModelPath);
            string sourceModelBaseName = Path.GetFileNameWithoutExtension(sourceModelPath);

            JsonObject? sourceEntry =
                sourceArray
                    .OfType<JsonObject>()
                    .FirstOrDefault(
                        modelObject =>
                            IsDonkeyModelDatabaseEntry(
                                modelObject,
                                sourceModelBaseName,
                                sourceModelName));

            if (sourceEntry == null)
            {
                throw new InvalidOperationException(
                    "database.json 파일에서 선택한 모델 데이터를 찾지 못했습니다.\n" +
                    sourceModelName);
            }

            string destinationModelName = Path.GetFileName(destinationModelPath);
            string destinationModelBaseName = Path.GetFileNameWithoutExtension(destinationModelPath);

            JsonObject? importedEntry =
                JsonNode.Parse(sourceEntry.ToJsonString()) as JsonObject;

            if (importedEntry == null)
            {
                throw new InvalidOperationException("모델 database 항목을 복사하지 못했습니다.");
            }

            ReplaceModelNameInJson(
                importedEntry,
                sourceModelBaseName,
                destinationModelBaseName,
                sourceModelName,
                destinationModelName);

            JsonArray destinationArray = LoadDestinationDatabaseArray(destinationDatabasePath);

            for (int i = destinationArray.Count - 1; i >= 0; i--)
            {
                if (destinationArray[i] is JsonObject modelObject &&
                    IsDonkeyModelDatabaseEntry(
                        modelObject,
                        destinationModelBaseName,
                        destinationModelName))
                {
                    destinationArray.RemoveAt(i);
                }
            }

            destinationArray.Add(importedEntry);
            WriteJsonNode(destinationDatabasePath, destinationArray);
        }

        private static JsonArray LoadDestinationDatabaseArray(string destinationDatabasePath)
        {
            if (!File.Exists(destinationDatabasePath))
            {
                return new JsonArray();
            }

            JsonNode? destinationRoot =
                JsonNode.Parse(File.ReadAllText(destinationDatabasePath));

            return destinationRoot as JsonArray ?? new JsonArray();
        }

        private static bool IsRelatedModelFileName(
            string fileName,
            string modelFileName,
            string modelBaseName)
        {
            return
                string.Equals(fileName, modelFileName, StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith(modelFileName + ".", StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith(modelBaseName + ".", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDonkeyModelDatabaseEntry(
            JsonObject modelObject,
            string modelBaseName,
            string modelFileName)
        {
            if (modelObject.TryGetPropertyValue("Name", out JsonNode? nameNode))
            {
                string name =
                    nameNode == null
                        ? ""
                        : nameNode.GetValue<string>();

                return
                    string.Equals(name, modelBaseName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, modelFileName, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static void ReplaceModelNameInJson(
            JsonNode node,
            string oldBaseName,
            string newBaseName,
            string oldFileName,
            string newFileName)
        {
            if (node is JsonObject obj)
            {
                foreach (string key in obj.Select(pair => pair.Key).ToList())
                {
                    JsonNode? child = obj[key];

                    if (child is JsonValue value)
                    {
                        string text = TryGetJsonString(value);

                        if (string.Equals(text, oldBaseName, StringComparison.OrdinalIgnoreCase))
                        {
                            obj[key] = newBaseName;
                        }
                        else if (string.Equals(text, oldFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            obj[key] = newFileName;
                        }
                    }
                    else if (child != null)
                    {
                        ReplaceModelNameInJson(
                            child,
                            oldBaseName,
                            newBaseName,
                            oldFileName,
                            newFileName);
                    }
                }
            }
            else if (node is JsonArray array)
            {
                foreach (JsonNode? child in array)
                {
                    if (child != null)
                    {
                        ReplaceModelNameInJson(
                            child,
                            oldBaseName,
                            newBaseName,
                            oldFileName,
                            newFileName);
                    }
                }
            }
        }

        private static string TryGetJsonString(JsonValue value)
        {
            try
            {
                return value.GetValue<string>();
            }
            catch
            {
                return "";
            }
        }

        private static void WriteJsonNode(
            string path,
            JsonNode node)
        {
            JsonSerializerOptions options =
                new JsonSerializerOptions
                {
                    WriteIndented = true
                };

            File.WriteAllText(path, node.ToJsonString(options));
        }
    }
}
