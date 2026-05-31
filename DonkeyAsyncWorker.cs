using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Data_Manager
{
    // 무거운 작업은 이 클래스에서 비동기로 처리합니다.
    public static class DonkeyAsyncWorker
    {
        // 설정 데이터
        public class AppSettings
        {
            public string WslDistroName { get; set; } = "Ubuntu-22.04";
            public string CondaEnvName { get; set; } = "e2e_env";
            public string MyCarPath { get; set; } = string.Empty;
        }

        public class PilotCardState
        {
            public string ModelName { get; set; } = string.Empty;
            public string ModelPath { get; set; } = string.Empty;
            public string ModelType { get; set; } = string.Empty;
            public string DatabaseJsonPath { get; set; } = string.Empty;

            public string WslDistroName { get; set; } = "Ubuntu-22.04";
            public string CondaEnvName { get; set; } = "e2e_env";
            public string MyCarPath { get; set; } = string.Empty;

            public List<string> TrainingTubPaths { get; set; } = new List<string>();
            public bool IsTubConnected { get; set; }

            public List<TubDrivingRecord> TubRecords { get; set; } = new List<TubDrivingRecord>();
            public string JudementJsonPath { get; set; } = string.Empty;
            public List<JudementRecord> JudementRecords { get; set; } = new List<JudementRecord>();
        }

        public class TubDrivingRecord
        {
            public int Index { get; set; }
            public string TubPath { get; set; } = string.Empty;
            public string ImagePath { get; set; } = string.Empty;
            public double? UserAngle { get; set; }
            public double? UserThrottle { get; set; }
            public string Mode { get; set; } = string.Empty;
            public string RawJsonPath { get; set; } = string.Empty;
        }

        public class JudementRecord
        {
            public int Index { get; set; }
            public string TubPath { get; set; } = string.Empty;
            public string ImagePath { get; set; } = string.Empty;

            public double? UserAngle { get; set; }
            public double? UserThrottle { get; set; }

            public double? PilotAngle { get; set; }
            public double? PilotThrottle { get; set; }

            public double? AngleError { get; set; }
            public double? ThrottleError { get; set; }

            public string Mode { get; set; } = string.Empty;
        }

        public class ProgressReport
        {
            public string Title { get; set; } = string.Empty;
            public string Step { get; set; } = string.Empty;
            public string Log { get; set; } = string.Empty;
            public int? Percent { get; set; }
            public bool IsIndeterminate { get; set; }
        }

        public class OperationResult<T>
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
            public T? Data { get; set; }
        }

        private const string SettingsFileName = "Data_Manager_settings.json";

        public static async Task<OperationResult<string>> FindMyCarPathInWslAsync(
            string distroName,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            AppSettings settings = LoadSettings();
            if (!string.IsNullOrWhiteSpace(settings.MyCarPath))
            {
                progress?.Report(new ProgressReport
                {
                    Step = "저장된 mycar 경로 확인 중...",
                    Log = "설정 파일에 저장된 mycar 경로를 확인합니다.",
                    IsIndeterminate = true
                });

                bool exists = await WslDirectoryExistsAsync(distroName, settings.MyCarPath, cancellationToken);
                if (exists)
                {
                    return new OperationResult<string> { Success = true, Data = settings.MyCarPath };
                }
            }

            progress?.Report(new ProgressReport
            {
                Step = "WSL에서 mycar 경로 탐색 중...",
                Log = "WSL에서 mycar 경로를 찾고 있습니다.",
                IsIndeterminate = true
            });

            string? myCarPath = await TryFindMyCarPathAsync(distroName, cancellationToken);
            if (string.IsNullOrWhiteSpace(myCarPath))
            {
                return new OperationResult<string>
                {
                    Success = false,
                    ErrorMessage = "WSL에서 mycar 경로를 찾지 못했습니다."
                };
            }

            settings.WslDistroName = distroName;
            settings.MyCarPath = myCarPath;
            SaveSettings(settings);

            return new OperationResult<string> { Success = true, Data = myCarPath };
        }

        public static async Task<OperationResult<PilotCardState>> LoadModelInfoFromDatabaseAsync(
            PilotCardState cardState,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            progress?.Report(new ProgressReport
            {
                Step = "database.json 읽는 중...",
                Log = "모델 database.json을 확인합니다.",
                IsIndeterminate = true
            });

            if (string.IsNullOrWhiteSpace(cardState.ModelName) || string.IsNullOrWhiteSpace(cardState.ModelPath))
            {
                return new OperationResult<PilotCardState>
                {
                    Success = false,
                    ErrorMessage = "모델 이름 또는 경로가 비어 있습니다."
                };
            }

            string modelFolder = Path.GetDirectoryName(cardState.ModelPath) ?? string.Empty;
            string primaryDatabase = Path.Combine(modelFolder, "database.json");
            string fallbackDatabase = string.IsNullOrWhiteSpace(cardState.MyCarPath)
                ? string.Empty
                : Path.Combine(ToWindowsPathFromWslPath(cardState.MyCarPath, cardState.WslDistroName), "models", "database.json");

            string databasePath = File.Exists(primaryDatabase)
                ? primaryDatabase
                : (File.Exists(fallbackDatabase) ? fallbackDatabase : string.Empty);

            if (string.IsNullOrWhiteSpace(databasePath))
            {
                return new OperationResult<PilotCardState>
                {
                    Success = false,
                    ErrorMessage = "database.json 파일을 찾지 못했습니다."
                };
            }

            string json = await File.ReadAllTextAsync(databasePath, cancellationToken);
            JToken root;

            try
            {
                root = JToken.Parse(json);
            }
            catch (Exception ex)
            {
                progress?.Report(new ProgressReport
                {
                    Log = $"database.json 파싱 중 숫자 타입 변환 오류 발생: {ex.Message}"
                });

                return new OperationResult<PilotCardState>
                {
                    Success = false,
                    ErrorMessage = $"database.json 파싱 중 숫자 타입 변환 오류 발생: {ex.Message}"
                };
            }

            if (root.Type != JTokenType.Array)
            {
                progress?.Report(new ProgressReport
                {
                    Log = "database.json 최상위 구조가 배열이 아닙니다."
                });

                return new OperationResult<PilotCardState>
                {
                    Success = false,
                    ErrorMessage = "database.json 최상위 구조가 배열이 아닙니다."
                };
            }

            JArray modelArray = (JArray)root;
            List<string> nameSamples = new List<string>();
            JObject? latest = null;
            double latestTime = double.MinValue;

            foreach (JToken token in modelArray)
            {
                if (token is not JObject obj)
                {
                    continue;
                }

                string? name = SafeGetString(obj, "Name");
                if (!string.IsNullOrWhiteSpace(name) && nameSamples.Count < 5)
                {
                    nameSamples.Add(name);
                }

                if (!string.Equals(name, cardState.ModelName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                double timeValue = SafeGetDouble(obj, "Time") ?? 0;
                if (latest == null || timeValue >= latestTime)
                {
                    latest = obj;
                    latestTime = timeValue;
                }
            }

            if (latest == null)
            {
                string sampleList = string.Join(", ", nameSamples);
                progress?.Report(new ProgressReport
                {
                    Log = $"선택한 모델명을 찾지 못했습니다. database.json 모델 목록: {sampleList}"
                });

                return new OperationResult<PilotCardState>
                {
                    Success = false,
                    ErrorMessage = $"선택한 모델명을 찾지 못했습니다. database.json 모델 목록: {sampleList}"
                };
            }

            cardState.DatabaseJsonPath = databasePath;
            cardState.ModelType = SafeGetString(latest, "Type") ?? string.Empty;

            JObject? config = SafeGetJObject(latest, "Config");
            string? configCarPath = config == null ? null : SafeGetString(config, "CAR_PATH");
            int? imageW = config == null ? null : SafeGetInt(config, "IMAGE_W");
            int? imageH = config == null ? null : SafeGetInt(config, "IMAGE_H");
            int? imageDepth = config == null ? null : SafeGetInt(config, "IMAGE_DEPTH");
            string baseCarPath = string.IsNullOrWhiteSpace(configCarPath) ? cardState.MyCarPath : configCarPath;

            _ = imageW;
            _ = imageH;
            _ = imageDepth;

            cardState.TrainingTubPaths = new List<string>();
            foreach (string tub in ExtractTubPaths(latest["Tubs"]))
            {
                string normalized = NormalizeTubPath(tub, baseCarPath, cardState.MyCarPath);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    cardState.TrainingTubPaths.Add(normalized);
                }
            }

            if (cardState.TrainingTubPaths.Count == 0)
            {
                progress?.Report(new ProgressReport
                {
                    Log = "Tubs 값이 없습니다."
                });
            }

            return new OperationResult<PilotCardState> { Success = true, Data = cardState };
        }

        public static async Task<OperationResult<List<TubDrivingRecord>>> LoadTubDrivingRecordsAsync(
            List<string> tubPaths,
            string distroName,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            var records = new List<TubDrivingRecord>();
            if (tubPaths == null || tubPaths.Count == 0)
            {
                return new OperationResult<List<TubDrivingRecord>>
                {
                    Success = false,
                    ErrorMessage = "tub 경로가 없습니다.",
                    Data = records
                };
            }

            int processed = 0;
            foreach (string tubPath in tubPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string windowsTubPath = ToWindowsPathFromWslPath(tubPath, distroName);
                if (!Directory.Exists(windowsTubPath))
                {
                    continue;
                }

                bool loaded = TryLoadFromRecordJson(windowsTubPath, tubPath, records, cancellationToken);
                if (!loaded)
                {
                    loaded = TryLoadFromCatalog(windowsTubPath, tubPath, records, cancellationToken);
                }

                if (!loaded)
                {
                    LoadFromImages(windowsTubPath, tubPath, records);
                }

                processed++;
                progress?.Report(new ProgressReport
                {
                    Step = "tub 데이터 파싱 중...",
                    Log = $"{processed}개 tub 경로를 확인했습니다.",
                    IsIndeterminate = true
                });
            }

            return new OperationResult<List<TubDrivingRecord>>
            {
                Success = records.Count > 0,
                ErrorMessage = records.Count > 0 ? string.Empty : "tub 데이터를 찾지 못했습니다.",
                Data = records
            };
        }

        public static async Task<OperationResult<List<JudementRecord>>> CheckOrLoadJudementAsync(
            PilotCardState cardState,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            progress?.Report(new ProgressReport
            {
                Step = "기존 AI 판단 데이터 확인 중...",
                Log = "judement JSON 존재 여부를 확인합니다.",
                IsIndeterminate = true
            });

            string modelFolder = Path.GetDirectoryName(cardState.ModelPath) ?? string.Empty;
            string judementPath = Path.Combine(modelFolder, $"{cardState.ModelName}_judement.json");
            cardState.JudementJsonPath = judementPath;

            if (!File.Exists(judementPath))
            {
                return new OperationResult<List<JudementRecord>>
                {
                    Success = false,
                    ErrorMessage = "AI 판단 데이터가 아직 없습니다. 생성 버튼을 눌러 생성하세요.",
                    Data = new List<JudementRecord>()
                };
            }

            string json = await File.ReadAllTextAsync(judementPath, cancellationToken);
            List<JudementRecord> records = ParseJudementRecords(json);
            return new OperationResult<List<JudementRecord>>
            {
                Success = true,
                Data = records
            };
        }

        public static async Task<OperationResult<List<JudementRecord>>> GenerateJudementAsync(
            PilotCardState cardState,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            string scriptPath = await EnsurePythonScriptAsync(cardState, cancellationToken);
            if (string.IsNullOrWhiteSpace(scriptPath))
            {
                return new OperationResult<List<JudementRecord>>
                {
                    Success = false,
                    ErrorMessage = "Python 스크립트 생성 실패"
                };
            }

            string modelFolder = Path.GetDirectoryName(cardState.ModelPath) ?? string.Empty;
            string judementPath = Path.Combine(modelFolder, $"{cardState.ModelName}_judement.json");
            cardState.JudementJsonPath = judementPath;

            progress?.Report(new ProgressReport
            {
                Step = "WSL Python 실행 중...",
                Log = "Python 추론을 실행합니다.",
                IsIndeterminate = true
            });

            string tubsArg = string.Join(";", cardState.TrainingTubPaths ?? new List<string>());
            string command = BuildPythonCommand(cardState, scriptPath, judementPath, tubsArg);

            OperationResult<string> runResult = await RunWslCommandAsync(cardState.WslDistroName, command, progress, cancellationToken);
            if (!runResult.Success)
            {
                return new OperationResult<List<JudementRecord>>
                {
                    Success = false,
                    ErrorMessage = runResult.ErrorMessage
                };
            }

            if (!File.Exists(judementPath))
            {
                return new OperationResult<List<JudementRecord>>
                {
                    Success = false,
                    ErrorMessage = "judement JSON 파일이 생성되지 않았습니다."
                };
            }

            string json = await File.ReadAllTextAsync(judementPath, cancellationToken);
            List<JudementRecord> records = ParseJudementRecords(json);
            return new OperationResult<List<JudementRecord>>
            {
                Success = true,
                Data = records
            };
        }

        public static async Task<OperationResult<string>> RunWslCommandAsync(
            string distroName,
            string command,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "wsl.exe",
                Arguments = $"-d {distroName} bash -lc \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                    progress?.Report(new ProgressReport { Log = e.Data });
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                    progress?.Report(new ProgressReport { Log = e.Data });
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await WaitForExitAsync(process, cancellationToken);

            if (process.ExitCode != 0)
            {
                return new OperationResult<string>
                {
                    Success = false,
                    ErrorMessage = errorBuilder.Length > 0 ? errorBuilder.ToString() : "WSL 실행 실패"
                };
            }

            return new OperationResult<string> { Success = true, Data = outputBuilder.ToString() };
        }

        public static string ToWindowsPathFromWslPath(string wslPath, string distroName)
        {
            if (string.IsNullOrWhiteSpace(wslPath))
            {
                return string.Empty;
            }

            if (wslPath.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase))
            {
                string driveLetter = wslPath.Substring(5, 1).ToUpperInvariant();
                string rest = wslPath.Substring(6).Replace('/', '\\');
                return $"{driveLetter}:\\{rest}".TrimEnd('\\');
            }

            if (wslPath.StartsWith("/home/", StringComparison.OrdinalIgnoreCase))
            {
                string rest = wslPath.TrimStart('/').Replace('/', '\\');
                return $"\\\\wsl.localhost\\{distroName}\\{rest}";
            }

            return wslPath.Replace('/', '\\');
        }

        public static string ToWslPathFromWindowsPath(string windowsPath)
        {
            if (string.IsNullOrWhiteSpace(windowsPath))
            {
                return string.Empty;
            }

            if (windowsPath.StartsWith("\\\\wsl.localhost\\", StringComparison.OrdinalIgnoreCase))
            {
                string trimmed = windowsPath.Substring("\\\\wsl.localhost\\".Length);
                int separatorIndex = trimmed.IndexOf('\\');
                if (separatorIndex < 0)
                {
                    return string.Empty;
                }

                string rest = trimmed.Substring(separatorIndex + 1).Replace('\\', '/');
                return $"/{rest}";
            }

            if (windowsPath.Length >= 2 && windowsPath[1] == ':')
            {
                string drive = char.ToLowerInvariant(windowsPath[0]).ToString();
                string rest = windowsPath.Substring(2).TrimStart('\\').Replace('\\', '/');
                return $"/mnt/{drive}/{rest}".TrimEnd('/');
            }

            return windowsPath.Replace('\\', '/');
        }

        public static string CombineWslPath(string basePath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                return relativePath.Replace('\\', '/');
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return basePath.Replace('\\', '/');
            }

            string normalizedBase = basePath.TrimEnd('/');
            string normalizedRelative = relativePath.TrimStart('/');
            return $"{normalizedBase}/{normalizedRelative}";
        }

        private static async Task WaitForExitAsync(Process process, CancellationToken cancellationToken)
        {
            Task waitTask = process.WaitForExitAsync(cancellationToken);
            try
            {
                await waitTask;
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                    }
                }
                catch
                {
                }

                throw;
            }
        }

        private static AppSettings LoadSettings()
        {
            if (!File.Exists(SettingsFileName))
            {
                return new AppSettings();
            }

            try
            {
                string json = File.ReadAllText(SettingsFileName);
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        private static void SaveSettings(AppSettings settings)
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsFileName, json);
        }

        private static async Task<string?> TryFindMyCarPathAsync(string distroName, CancellationToken cancellationToken)
        {
            string homeProbe = "if [ -d \"$HOME/mycar\" ]; then echo \"$HOME/mycar\"; fi";
            string? path = await RunProbeAsync(distroName, homeProbe, cancellationToken);
            if (!string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            string usersProbe = "for d in /home/*/mycar; do if [ -d \"$d\" ]; then echo \"$d\"; break; fi; done";
            path = await RunProbeAsync(distroName, usersProbe, cancellationToken);
            if (!string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            string findProbe = "find /home -maxdepth 3 -type d -name mycar 2>/dev/null | head -n 1";
            return await RunProbeAsync(distroName, findProbe, cancellationToken);
        }

        private static async Task<string?> RunProbeAsync(string distroName, string probeCommand, CancellationToken cancellationToken)
        {
            OperationResult<string> result = await RunWslCommandAsync(distroName, probeCommand, null, cancellationToken);
            string output = result.Data?.Trim() ?? string.Empty;
            return string.IsNullOrWhiteSpace(output) ? null : output;
        }

        private static async Task<bool> WslDirectoryExistsAsync(string distroName, string wslPath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(wslPath))
            {
                return false;
            }

            string command = $"if [ -d '{EscapeBash(wslPath)}' ]; then echo OK; fi";
            OperationResult<string> result = await RunWslCommandAsync(distroName, command, null, cancellationToken);
            return (result.Data ?? string.Empty).Trim().Equals("OK", StringComparison.OrdinalIgnoreCase);
        }

        private static string? SafeGetString(JObject obj, string propertyName)
        {
            if (!obj.TryGetValue(propertyName, out JToken? token) || token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            return token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
        }

        private static double? SafeGetDouble(JObject obj, string propertyName)
        {
            if (!obj.TryGetValue(propertyName, out JToken? token) || token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                return token.Value<double>();
            }

            if (double.TryParse(token.ToString(), out double number))
            {
                return number;
            }

            return null;
        }

        private static int? SafeGetInt(JObject obj, string propertyName)
        {
            if (!obj.TryGetValue(propertyName, out JToken? token) || token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token.Type == JTokenType.Integer)
            {
                return token.Value<int>();
            }

            if (token.Type == JTokenType.Float)
            {
                double value = token.Value<double>();
                if (value >= int.MinValue && value <= int.MaxValue && Math.Abs(value % 1) < double.Epsilon)
                {
                    return Convert.ToInt32(value);
                }
            }

            if (int.TryParse(token.ToString(), out int number))
            {
                return number;
            }

            return null;
        }

        private static bool? SafeGetBool(JObject obj, string propertyName)
        {
            if (!obj.TryGetValue(propertyName, out JToken? token) || token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token.Type == JTokenType.Boolean)
            {
                return token.Value<bool>();
            }

            if (bool.TryParse(token.ToString(), out bool value))
            {
                return value;
            }

            return null;
        }

        private static JObject? SafeGetJObject(JObject obj, string propertyName)
        {
            if (!obj.TryGetValue(propertyName, out JToken? token) || token == null)
            {
                return null;
            }

            return token as JObject;
        }

        private static List<string> ExtractTubPaths(JToken? tubsToken)
        {
            var results = new List<string>();

            if (tubsToken == null || tubsToken.Type == JTokenType.Null)
            {
                return results;
            }

            if (tubsToken.Type == JTokenType.Array)
            {
                foreach (JToken token in tubsToken)
                {
                    string value = token.Type == JTokenType.String ? token.Value<string>() ?? string.Empty : token.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        results.Add(value.Trim());
                    }
                }

                return results;
            }

            if (tubsToken.Type == JTokenType.Object)
            {
                return results;
            }

            string text = tubsToken.Type == JTokenType.String ? tubsToken.Value<string>() ?? string.Empty : tubsToken.ToString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                results.Add(text.Trim());
            }

            return results;
        }

        private static string NormalizeTubPath(string tubPath, string? carPathFromConfig, string myCarPath)
        {
            if (string.IsNullOrWhiteSpace(tubPath))
            {
                return string.Empty;
            }

            string trimmed = tubPath.Trim();

            if (trimmed.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("/home/", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            if (trimmed.Length >= 2 && trimmed[1] == ':')
            {
                return ToWslPathFromWindowsPath(trimmed);
            }

            string basePath = string.IsNullOrWhiteSpace(carPathFromConfig) ? myCarPath : carPathFromConfig;

            if (trimmed.StartsWith("./"))
            {
                string relative = trimmed.Substring(2);
                return CombineWslPath(basePath, relative);
            }

            if (!trimmed.StartsWith("/"))
            {
                return CombineWslPath(basePath, trimmed);
            }

            return trimmed;
        }

        private static bool TryLoadFromRecordJson(
            string windowsTubPath,
            string wslTubPath,
            List<TubDrivingRecord> records,
            CancellationToken cancellationToken)
        {
            var recordFiles = Directory.EnumerateFiles(windowsTubPath, "record_*.json", SearchOption.TopDirectoryOnly)
                .ToList();

            string recordsFolder = Path.Combine(windowsTubPath, "records");
            if (Directory.Exists(recordsFolder))
            {
                recordFiles.AddRange(Directory.EnumerateFiles(recordsFolder, "record_*.json", SearchOption.AllDirectories));
            }

            if (recordFiles.Count == 0)
            {
                return false;
            }

            foreach (string recordFile in recordFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    JsonNode? node = JsonNode.Parse(File.ReadAllText(recordFile));
                    if (node == null)
                    {
                        continue;
                    }

                    records.Add(CreateRecordFromJsonNode(node, wslTubPath, recordFile));
                }
                catch
                {
                }
            }

            return records.Count > 0;
        }

        private static bool TryLoadFromCatalog(
            string windowsTubPath,
            string wslTubPath,
            List<TubDrivingRecord> records,
            CancellationToken cancellationToken)
        {
            string[] catalogs = Directory.GetFiles(windowsTubPath, "catalog_*.catalog", SearchOption.TopDirectoryOnly);
            if (catalogs.Length == 0)
            {
                return false;
            }

            foreach (string catalog in catalogs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    string[] lines = File.ReadAllLines(catalog);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        JsonNode? node = JsonNode.Parse(line);
                        if (node == null)
                        {
                            continue;
                        }

                        TubDrivingRecord record = CreateRecordFromJsonNode(node, wslTubPath, catalog);
                        record.Index = i;
                        records.Add(record);
                    }
                }
                catch
                {
                }
            }

            return records.Count > 0;
        }

        private static void LoadFromImages(string windowsTubPath, string wslTubPath, List<TubDrivingRecord> records)
        {
            string[] images = Directory.GetFiles(windowsTubPath, "*.*", SearchOption.AllDirectories)
                .Where(path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                               || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            int index = records.Count;
            foreach (string image in images)
            {
                string relative = Path.GetRelativePath(windowsTubPath, image).Replace('\\', '/');
                string wslImagePath = CombineWslPath(wslTubPath, relative);

                records.Add(new TubDrivingRecord
                {
                    Index = index++,
                    TubPath = wslTubPath,
                    ImagePath = wslImagePath,
                    RawJsonPath = string.Empty
                });
            }
        }

        private static TubDrivingRecord CreateRecordFromJsonNode(JsonNode node, string wslTubPath, string rawJsonPath)
        {
            string imageValue = GetStringValue(node, "cam/image_array", "image", "image_path", "img", "cam/image_array_path");
            string wslImagePath = ResolveImagePath(wslTubPath, imageValue);

            return new TubDrivingRecord
            {
                Index = GetIntValue(node, "_index", "index"),
                TubPath = wslTubPath,
                ImagePath = wslImagePath,
                UserAngle = GetDoubleValue(node, "user/angle", "angle", "steering"),
                UserThrottle = GetDoubleValue(node, "user/throttle", "throttle"),
                Mode = GetStringValue(node, "user/mode", "mode"),
                RawJsonPath = rawJsonPath
            };
        }

        private static string ResolveImagePath(string wslTubPath, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (value.StartsWith("/"))
            {
                return value;
            }

            return CombineWslPath(wslTubPath, value);
        }

        private static string GetStringValue(JsonNode node, params string[] keys)
        {
            foreach (string key in keys)
            {
                string? value = node[key]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static double? GetDoubleValue(JsonNode node, params string[] keys)
        {
            foreach (string key in keys)
            {
                string? value = node[key]?.ToString();
                if (double.TryParse(value, out double number))
                {
                    return number;
                }
            }

            return null;
        }

        private static int GetIntValue(JsonNode node, params string[] keys)
        {
            foreach (string key in keys)
            {
                string? value = node[key]?.ToString();
                if (int.TryParse(value, out int number))
                {
                    return number;
                }
            }

            return 0;
        }

        private static List<JudementRecord> ParseJudementRecords(string json)
        {
            var records = new List<JudementRecord>();
            try
            {
                JsonNode? root = JsonNode.Parse(json);
                JsonArray? array = root?["records"] as JsonArray;
                if (array == null)
                {
                    return records;
                }

                foreach (JsonNode? node in array)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    records.Add(new JudementRecord
                    {
                        Index = node["index"]?.GetValue<int>() ?? 0,
                        TubPath = node["tub_path"]?.ToString() ?? string.Empty,
                        ImagePath = node["image_path"]?.ToString() ?? string.Empty,
                        UserAngle = node["user_angle"]?.GetValue<double?>(),
                        UserThrottle = node["user_throttle"]?.GetValue<double?>(),
                        PilotAngle = node["pilot_angle"]?.GetValue<double?>(),
                        PilotThrottle = node["pilot_throttle"]?.GetValue<double?>(),
                        AngleError = node["angle_error"]?.GetValue<double?>(),
                        ThrottleError = node["throttle_error"]?.GetValue<double?>(),
                        Mode = node["mode"]?.ToString() ?? string.Empty
                    });
                }
            }
            catch
            {
            }

            return records;
        }

        private static async Task<string> EnsurePythonScriptAsync(
            PilotCardState cardState,
            CancellationToken cancellationToken)
        {
            string logFolder = CombineWslPath(cardState.MyCarPath, "log");
            string scriptPath = CombineWslPath(logFolder, "extract_model_judement.py");
            string windowsLogFolder = ToWindowsPathFromWslPath(logFolder, cardState.WslDistroName);
            string windowsScriptPath = ToWindowsPathFromWslPath(scriptPath, cardState.WslDistroName);

            if (!Directory.Exists(windowsLogFolder))
            {
                Directory.CreateDirectory(windowsLogFolder);
            }

            if (!File.Exists(windowsScriptPath))
            {
                string script = BuildPythonScript();
                await File.WriteAllTextAsync(windowsScriptPath, script, cancellationToken);
            }

            return scriptPath;
        }

        private static string BuildPythonCommand(
            PilotCardState cardState,
            string scriptPath,
            string outputPath,
            string tubsArg)
        {
            string command =
                "if [ -f \"$HOME/miniconda3/etc/profile.d/conda.sh\" ]; then " +
                "source \"$HOME/miniconda3/etc/profile.d/conda.sh\"; " +
                "elif [ -f \"$HOME/anaconda3/etc/profile.d/conda.sh\" ]; then " +
                "source \"$HOME/anaconda3/etc/profile.d/conda.sh\"; " +
                "else echo 'conda.sh를 찾을 수 없습니다.' >&2; exit 10; fi; " +
                $"conda activate {cardState.CondaEnvName} || exit 11; " +
                $"cd '{EscapeBash(cardState.MyCarPath)}' || exit 12; " +
                $"python '{EscapeBash(scriptPath)}'" +
                $" --model '{EscapeBash(cardState.ModelPath)}'" +
                $" --model-name '{EscapeBash(cardState.ModelName)}'" +
                $" --model-type '{EscapeBash(cardState.ModelType)}'" +
                $" --tubs '{EscapeBash(tubsArg)}'" +
                $" --output '{EscapeBash(outputPath)}'" +
                $" --car-path '{EscapeBash(cardState.MyCarPath)}'";

            return command;
        }

        private static string BuildPythonScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine("import argparse");
            sb.AppendLine("import json");
            sb.AppendLine("import os");
            sb.AppendLine("import sys");
            sb.AppendLine("from datetime import datetime");
            sb.AppendLine("import numpy as np");
            sb.AppendLine("from PIL import Image");
            sb.AppendLine("");
            sb.AppendLine("def parse_args():");
            sb.AppendLine("    parser = argparse.ArgumentParser()");
            sb.AppendLine("    parser.add_argument('--model', required=True)");
            sb.AppendLine("    parser.add_argument('--model-name', required=True)");
            sb.AppendLine("    parser.add_argument('--model-type', default='linear')");
            sb.AppendLine("    parser.add_argument('--tubs', required=True)");
            sb.AppendLine("    parser.add_argument('--output', required=True)");
            sb.AppendLine("    parser.add_argument('--car-path', required=True)");
            sb.AppendLine("    parser.add_argument('--image-w', type=int, default=160)");
            sb.AppendLine("    parser.add_argument('--image-h', type=int, default=120)");
            sb.AppendLine("    parser.add_argument('--image-depth', type=int, default=3)");
            sb.AppendLine("    return parser.parse_args()");
            sb.AppendLine("");
            sb.AppendLine("def parse_tubs(tubs_arg):");
            sb.AppendLine("    tubs_arg = tubs_arg.strip()");
            sb.AppendLine("    if not tubs_arg:");
            sb.AppendLine("        return []");
            sb.AppendLine("    if tubs_arg.startswith('['):");
            sb.AppendLine("        try:");
            sb.AppendLine("            return json.loads(tubs_arg)");
            sb.AppendLine("        except Exception:");
            sb.AppendLine("            return []");
            sb.AppendLine("    return [t.strip() for t in tubs_arg.split(';') if t.strip()]");
            sb.AppendLine("");
            sb.AppendLine("def load_model(model_path):");
            sb.AppendLine("    try:");
            sb.AppendLine("        from donkeycar.parts.keras import KerasPilot");
            sb.AppendLine("        pilot = KerasPilot()");
            sb.AppendLine("        pilot.load(model_path)");
            sb.AppendLine("        return pilot.model");
            sb.AppendLine("    except Exception:");
            sb.AppendLine("        try:");
            sb.AppendLine("            from tensorflow.keras.models import load_model" );
            sb.AppendLine("            return load_model(model_path)");
            sb.AppendLine("        except Exception as ex:");
            sb.AppendLine("            raise ex");
            sb.AppendLine("");
            sb.AppendLine("def collect_records(tub_path):");
            sb.AppendLine("    records = []");
            sb.AppendLine("    records_folder = os.path.join(tub_path, 'records')");
            sb.AppendLine("    if os.path.isdir(records_folder):");
            sb.AppendLine("        for root, _, files in os.walk(records_folder):");
            sb.AppendLine("            for file in files:");
            sb.AppendLine("                if file.startswith('record_') and file.endswith('.json'):");
            sb.AppendLine("                    records.append(os.path.join(root, file))");
            sb.AppendLine("    for file in os.listdir(tub_path):");
            sb.AppendLine("        if file.startswith('record_') and file.endswith('.json'):");
            sb.AppendLine("            records.append(os.path.join(tub_path, file))");
            sb.AppendLine("    return records");
            sb.AppendLine("");
            sb.AppendLine("def parse_json_line(line):");
            sb.AppendLine("    try:");
            sb.AppendLine("        return json.loads(line)");
            sb.AppendLine("    except Exception:");
            sb.AppendLine("        return None");
            sb.AppendLine("");
            sb.AppendLine("def extract_value(obj, keys):");
            sb.AppendLine("    for key in keys:");
            sb.AppendLine("        if key in obj:");
            sb.AppendLine("            value = obj.get(key)");
            sb.AppendLine("            if value is not None:");
            sb.AppendLine("                return value");
            sb.AppendLine("    return None");
            sb.AppendLine("");
            sb.AppendLine("def resolve_image_path(tub_path, value):");
            sb.AppendLine("    if not value:");
            sb.AppendLine("        return ''");
            sb.AppendLine("    if value.startswith('/'):");
            sb.AppendLine("        return value");
            sb.AppendLine("    return os.path.join(tub_path, value)");
            sb.AppendLine("");
            sb.AppendLine("def load_records_from_catalog(tub_path):");
            sb.AppendLine("    records = []");
            sb.AppendLine("    for file in os.listdir(tub_path):");
            sb.AppendLine("        if file.startswith('catalog_') and file.endswith('.catalog'):");
            sb.AppendLine("            with open(os.path.join(tub_path, file), 'r', encoding='utf-8') as f:");
            sb.AppendLine("                for idx, line in enumerate(f):");
            sb.AppendLine("                    line = line.strip()");
            sb.AppendLine("                    if not line:");
            sb.AppendLine("                        continue");
            sb.AppendLine("                    obj = parse_json_line(line)");
            sb.AppendLine("                    if not obj:");
            sb.AppendLine("                        continue");
            sb.AppendLine("                    records.append((idx, obj))");
            sb.AppendLine("    return records");
            sb.AppendLine("");
            sb.AppendLine("def load_records_from_files(record_files):");
            sb.AppendLine("    records = []");
            sb.AppendLine("    for file in record_files:");
            sb.AppendLine("        try:");
            sb.AppendLine("            with open(file, 'r', encoding='utf-8') as f:");
            sb.AppendLine("                obj = json.load(f)");
            sb.AppendLine("                records.append((obj.get('_index', 0), obj))");
            sb.AppendLine("        except Exception:");
            sb.AppendLine("            pass");
            sb.AppendLine("    return records");
            sb.AppendLine("");
            sb.AppendLine("def image_to_array(image_path, w, h):");
            sb.AppendLine("    with Image.open(image_path) as img:");
            sb.AppendLine("        img = img.convert('RGB')");
            sb.AppendLine("        img = img.resize((w, h))");
            sb.AppendLine("        arr = np.asarray(img)");
            sb.AppendLine("    return arr");
            sb.AppendLine("");
            sb.AppendLine("def predict_model(model, image_arr):");
            sb.AppendLine("    input_arr = np.expand_dims(image_arr, axis=0)");
            sb.AppendLine("    pred = model.predict(input_arr)");
            sb.AppendLine("    if isinstance(pred, list):");
            sb.AppendLine("        angle = float(pred[0][0])");
            sb.AppendLine("        throttle = float(pred[1][0]) if len(pred) > 1 else 0.0");
            sb.AppendLine("        return angle, throttle");
            sb.AppendLine("    pred = pred[0]");
            sb.AppendLine("    if len(pred) >= 2:");
            sb.AppendLine("        return float(pred[0]), float(pred[1])");
            sb.AppendLine("    return float(pred[0]), 0.0");
            sb.AppendLine("");
            sb.AppendLine("def main():");
            sb.AppendLine("    args = parse_args()");
            sb.AppendLine("    tubs = parse_tubs(args.tubs)");
            sb.AppendLine("    if not tubs:");
            sb.AppendLine("        print('No tubs')");
            sb.AppendLine("        sys.exit(1)");
            sb.AppendLine("    model = load_model(args.model)");
            sb.AppendLine("    results = []");
            sb.AppendLine("    for tub in tubs:");
            sb.AppendLine("        record_files = collect_records(tub)");
            sb.AppendLine("        records = load_records_from_files(record_files)");
            sb.AppendLine("        if not records:");
            sb.AppendLine("            records = load_records_from_catalog(tub)");
            sb.AppendLine("        for idx, obj in records:");
            sb.AppendLine("            image_value = extract_value(obj, ['cam/image_array', 'image', 'image_path', 'img', 'cam/image_array_path'])");
            sb.AppendLine("            image_path = resolve_image_path(tub, image_value)");
            sb.AppendLine("            if not image_path or not os.path.exists(image_path):");
            sb.AppendLine("                continue");
            sb.AppendLine("            image_arr = image_to_array(image_path, args.image_w, args.image_h)");
            sb.AppendLine("            angle, throttle = predict_model(model, image_arr)");
            sb.AppendLine("            user_angle = extract_value(obj, ['user/angle', 'angle', 'steering'])");
            sb.AppendLine("            user_throttle = extract_value(obj, ['user/throttle', 'throttle'])");
            sb.AppendLine("            mode = extract_value(obj, ['user/mode', 'mode'])");
            sb.AppendLine("            angle_error = None if user_angle is None else angle - float(user_angle)");
            sb.AppendLine("            throttle_error = None if user_throttle is None else throttle - float(user_throttle)");
            sb.AppendLine("            results.append({");
            sb.AppendLine("                'index': int(idx),");
            sb.AppendLine("                'tub_path': tub,");
            sb.AppendLine("                'image_path': image_path,");
            sb.AppendLine("                'user_angle': user_angle,");
            sb.AppendLine("                'user_throttle': user_throttle,");
            sb.AppendLine("                'pilot_angle': angle,");
            sb.AppendLine("                'pilot_throttle': throttle,");
            sb.AppendLine("                'angle_error': angle_error,");
            sb.AppendLine("                'throttle_error': throttle_error,");
            sb.AppendLine("                'mode': mode or ''");
            sb.AppendLine("            })");
            sb.AppendLine("    output = {");
            sb.AppendLine("        'model_name': args.model_name,");
            sb.AppendLine("        'model_path': args.model,");
            sb.AppendLine("        'model_type': args.model_type,");
            sb.AppendLine("        'created_at': datetime.utcnow().isoformat(),");
            sb.AppendLine("        'tubs': tubs,");
            sb.AppendLine("        'records': results");
            sb.AppendLine("    }");
            sb.AppendLine("    with open(args.output, 'w', encoding='utf-8') as f:");
            sb.AppendLine("        json.dump(output, f, ensure_ascii=False, indent=2)");
            sb.AppendLine("    print('done')");
            sb.AppendLine("");
            sb.AppendLine("if __name__ == '__main__':");
            sb.AppendLine("    main()");
            return sb.ToString();
        }

        private static string EscapeBash(string value)
        {
            return value.Replace("'", "'\"'\"'");
        }
    }
}
