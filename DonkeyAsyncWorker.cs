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
    // DonkeyCar/WSL 관련 무거운 작업을 UI 스레드 밖에서 처리하는 공통 도우미입니다.
    // tub 파싱, 모델 database 읽기, WSL 명령 실행, Windows<->WSL 경로 변환,
    // AI 추론용 Python 스크립트 실행을 이 클래스에 모아 두었습니다.
    public static class DonkeyAsyncWorker
    {
        private const string FallbackWslDistroName = "Ubuntu-22.04";
        private const string FallbackCondaEnvName = "e2e_env";

        // =====================================================
        // WSL 설정과 기본 경로 탐색
        // =====================================================

        // 설정 데이터
        public class AppSettings
        {
            public string WslDistroName { get; set; } = string.Empty;
            public string CondaEnvName { get; set; } = FallbackCondaEnvName;
            public string MyCarPath { get; set; } = string.Empty;
        }

        public static async Task<string> GetPreferredWslDistroNameAsync(CancellationToken cancellationToken)
        {
            AppSettings settings = LoadSettings();
            if (!string.IsNullOrWhiteSpace(settings.WslDistroName)
                && IsUbuntu2204DistroName(settings.WslDistroName)
                && await WslDistroExistsAsync(settings.WslDistroName, cancellationToken))
            {
                return settings.WslDistroName;
            }

            List<string> distros = await GetInstalledWslDistrosAsync(cancellationToken);
            string? selected = distros.FirstOrDefault(IsUbuntu2204DistroName);
            selected ??= FallbackWslDistroName;

            settings.WslDistroName = selected;
            SaveSettings(settings);
            return selected;
        }

        public static async Task<OperationResult<string>> GetWslHomePathAsync(
            string distroName,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(distroName))
            {
                distroName = await GetPreferredWslDistroNameAsync(cancellationToken);
            }

            progress?.Report(new ProgressReport
            {
                Log = "WSL HOME 경로를 확인합니다."
            });

            OperationResult<string> result =
                await RunWslCommandAsync(
                    distroName,
                    "printf %s \"$HOME\"",
                    progress,
                    cancellationToken);

            if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
            {
                return new OperationResult<string>
                {
                    Success = false,
                    ErrorMessage = "WSL HOME 경로 확인 실패"
                };
            }

            string wslHome = result.Data.Trim();
            string windowsHome = ToWindowsPathFromWslPath(wslHome, distroName);

            return new OperationResult<string>
            {
                Success = true,
                Data = windowsHome
            };
        }

        public static async Task<OperationResult<List<PilotFrameData>>> ParseSingleTubFolderAsync(
            string tubPath,
            string distroName,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            // =====================================================
            // tub 폴더 파싱 진입점
            // =====================================================
            //
            // 하나의 tub 폴더 또는 tub들이 들어 있는 상위 폴더를 읽어 프레임 목록으로 변환합니다.
            // catalog_*.catalog, record_*.json, images-only 구조를 순서대로 시도합니다.
            if (string.IsNullOrWhiteSpace(distroName))
            {
                distroName = await GetPreferredWslDistroNameAsync(cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(tubPath))
            {
                return new OperationResult<List<PilotFrameData>>
                {
                    Success = false,
                    ErrorMessage = "tub 경로가 비어 있습니다.",
                    Data = new List<PilotFrameData>()
                };
            }

            string normalizedTubPath = tubPath.Trim();
            bool isWslPathInput = normalizedTubPath.StartsWith("/");

            string tubPathWsl = isWslPathInput
                ? normalizedTubPath
                : ToWslPathFromWindowsPath(normalizedTubPath);

            string tubPathWindows = isWslPathInput
                ? ToWindowsPathFromWslPath(normalizedTubPath, distroName)
                : normalizedTubPath;

            tubPathWindows = ResolveExistingWindowsPath(tubPathWindows);

            if (!Directory.Exists(tubPathWindows) && !isWslPathInput)
            {
                string normalizedWindowsPath = normalizedTubPath.Replace('/', '\\');
                normalizedWindowsPath = ResolveExistingWindowsPath(normalizedWindowsPath);
                if (Directory.Exists(normalizedWindowsPath))
                {
                    tubPathWindows = normalizedWindowsPath;
                    tubPathWsl = ToWslPathFromWindowsPath(normalizedWindowsPath);
                }
            }

            if (!Directory.Exists(tubPathWindows))
            {
                string resolvedWindowsPath = ResolveExistingWslWindowsPath(tubPathWindows, distroName);
                if (!string.Equals(resolvedWindowsPath, tubPathWindows, StringComparison.OrdinalIgnoreCase))
                {
                    progress?.Report(new ProgressReport
                    {
                        Log = $"WSL Windows 寃쎈줈 蹂댁젙: {tubPathWindows} -> {resolvedWindowsPath}"
                    });
                    tubPathWindows = resolvedWindowsPath;
                    tubPathWsl = ToWslPathFromWindowsPath(resolvedWindowsPath);
                }
            }

            if (Directory.Exists(tubPathWindows))
            {
                tubPathWsl = ToWslPathFromWindowsPath(tubPathWindows);
            }

            progress?.Report(new ProgressReport
            {
                Log = $"tub 경로 입력: {tubPath}"
            });
            progress?.Report(new ProgressReport
            {
                Log = $"WSL 경로: {tubPathWsl}"
            });
            progress?.Report(new ProgressReport
            {
                Log = $"Windows 경로: {tubPathWindows}"
            });

            if (!Directory.Exists(tubPathWindows))
            {
                return new OperationResult<List<PilotFrameData>>
                {
                    Success = false,
                    ErrorMessage = "tub 폴더가 존재하지 않습니다.",
                    Data = new List<PilotFrameData>()
                };
            }

            List<PilotFrameData> frames = new List<PilotFrameData>();
            List<string> tubRoots = FindTubRoots(tubPathWindows, progress);
            if (tubRoots.Count == 0)
            {
                tubRoots.Add(tubPathWindows);
                progress?.Report(new ProgressReport
                {
                    Log = "tub 루트를 찾지 못해 입력 경로를 직접 처리합니다."
                });
            }

            progress?.Report(new ProgressReport
            {
                Log = $"발견된 tub 루트 개수: {tubRoots.Count}"
            });

            foreach (string tubRoot in tubRoots)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string rootWsl = ToWslPathFromWindowsPath(tubRoot);
                progress?.Report(new ProgressReport
                {
                    Log = $"tub 루트 처리 중: {tubRoot}"
                });
                progress?.Report(new ProgressReport
                {
                    Log = $"tub 루트 WSL 경로: {rootWsl}"
                });

                string manifestPath = Path.Combine(tubRoot, "manifest.json");
                string catalogManifestPath = Path.Combine(tubRoot, "catalog_manifest.json");
                progress?.Report(new ProgressReport
                {
                    Log = $"manifest.json 존재: {File.Exists(manifestPath)}"
                });
                progress?.Report(new ProgressReport
                {
                    Log = $"catalog_manifest.json 존재: {File.Exists(catalogManifestPath)}"
                });

                string[] catalogFiles = Directory.GetFiles(tubRoot, "catalog_*.catalog", SearchOption.TopDirectoryOnly);
                Array.Sort(catalogFiles, StringComparer.OrdinalIgnoreCase);
                progress?.Report(new ProgressReport
                {
                    Log = $"catalog 파일 개수: {catalogFiles.Length}"
                });

                string imagesFolder = Path.Combine(tubRoot, "images");
                bool imagesFolderExists = Directory.Exists(imagesFolder);
                int imageCount = imagesFolderExists
                    ? Directory.GetFiles(imagesFolder, "*.*", SearchOption.TopDirectoryOnly)
                        .Count(path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                       || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                       || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    : 0;

                progress?.Report(new ProgressReport
                {
                    Log = $"images 폴더 존재: {imagesFolderExists}, 이미지 수: {imageCount}"
                });

                List<PilotFrameData> rootFrames = new List<PilotFrameData>();
                if (catalogFiles.Length > 0)
                {
                    rootFrames = await ParseCatalogFilesAsync(
                        rootWsl,
                        tubRoot,
                        catalogFiles,
                        progress,
                        cancellationToken);
                }

                if (rootFrames.Count == 0)
                {
                    rootFrames = await ParseRecordJsonFilesAsFramesAsync(
                        rootWsl,
                        tubRoot,
                        progress,
                        cancellationToken);
                }

                if (rootFrames.Count == 0)
                {
                    progress?.Report(new ProgressReport
                    {
                        Log = "catalog 데이터를 읽지 못해 images 폴더 기준으로 이미지만 로드했습니다."
                    });

                    rootFrames = BuildFramesFromImagesOnly(rootWsl, tubRoot);
                }

                HashSet<int> deletedIndexes = ReadDeletedIndexesFromTubRoot(tubRoot);
                if (deletedIndexes.Count > 0)
                {
                    int beforeCount = rootFrames.Count;
                    rootFrames = rootFrames
                        .Where(frame => !deletedIndexes.Contains(frame.Index))
                        .ToList();

                    progress?.Report(new ProgressReport
                    {
                        Log = $"manifest 제외 인덱스 적용: {beforeCount - rootFrames.Count}개 제외"
                    });
                }

                frames.AddRange(rootFrames);

                progress?.Report(new ProgressReport
                {
                    Log = $"현재 루트 frame 개수: {rootFrames.Count}, 누적 frame 개수: {frames.Count}"
                });
            }

            progress?.Report(new ProgressReport
            {
                Log = $"최종 frame 개수: {frames.Count}"
            });

            if (frames.Count > 0)
            {
                progress?.Report(new ProgressReport
                {
                    Log = $"첫 번째 frame 이미지: {frames[0].ImagePath}, UserAngle: {frames[0].UserAngle}, UserThrottle: {frames[0].UserThrottle}"
                });
            }

            return new OperationResult<List<PilotFrameData>>
            {
                Success = frames.Count > 0,
                ErrorMessage = frames.Count > 0 ? string.Empty : "tub 데이터를 찾지 못했습니다.",
                Data = frames
            };
        }

        public static async Task<List<PilotFrameData>> ParseCatalogFilesAsync(
            string tubPathWsl,
            string tubPathWindows,
            string[] catalogFiles,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            // DonkeyCar catalog 한 줄은 JSON 객체이며 이미지 파일명과 user angle/throttle을 담습니다.
            // catalog가 가리키는 이미지 경로를 실제 Windows/WSL 경로로 해결해 PilotFrameData로 만듭니다.
            var frames = new List<PilotFrameData>();
            int resolvedCount = 0;
            int unresolvedCount = 0;

            Array.Sort(catalogFiles, StringComparer.OrdinalIgnoreCase);
            int recordIndex = 0;

            foreach (string catalogFile in catalogFiles)
            {
                using var reader = new StreamReader(catalogFile);
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    JObject? obj;
                    try
                    {
                        obj = JObject.Parse(line);
                    }
                    catch (Exception ex)
                    {
                        progress?.Report(new ProgressReport
                        {
                            Log = $"catalog 라인 파싱 실패: {ex.Message}"
                        });
                        continue;
                    }

                    int index = SafeGetIntFromKeys(obj, "_index", "index", "record/index") ?? recordIndex;
                    string imageValue = SafeGetStringFromKeys(obj, "cam/image_array", "cam/image_array_path", "image", "image_path", "img") ?? string.Empty;
                    double? userAngle = SafeGetDoubleFromKeys(obj, "user/angle", "user/steering", "angle", "steering");
                    double? userThrottle = SafeGetDoubleFromKeys(obj, "user/throttle", "throttle");
                    string mode = SafeGetStringFromKeys(obj, "user/mode", "mode") ?? string.Empty;

                    string imagePath = ResolveTubImagePath(tubPathWsl, tubPathWindows, imageValue, index);
                    if (!string.IsNullOrWhiteSpace(imagePath))
                    {
                        resolvedCount++;
                    }
                    else
                    {
                        unresolvedCount++;
                    }

                    frames.Add(new PilotFrameData
                    {
                        Index = index,
                        TubPath = tubPathWsl,
                        ImagePath = imagePath,
                        UserAngle = userAngle,
                        UserThrottle = userThrottle,
                        Mode = mode
                    });

                    recordIndex++;
                    if (recordIndex % 100 == 0)
                    {
                        progress?.Report(new ProgressReport
                        {
                            Log = $"catalog {recordIndex}건 처리 중"
                        });
                    }
                }
            }

            progress?.Report(new ProgressReport
            {
                Log = $"catalog record 수: {frames.Count}, 이미지 연결 성공: {resolvedCount}, 실패: {unresolvedCount}"
            });

            return frames;
        }

        private static HashSet<int> ReadDeletedIndexesFromTubRoot(string tubRoot)
        {
            HashSet<int> deletedIndexes = new HashSet<int>();

            if (string.IsNullOrWhiteSpace(tubRoot) || !Directory.Exists(tubRoot))
            {
                return deletedIndexes;
            }

            string[] manifestCandidates =
            {
                Path.Combine(tubRoot, "manifest.json"),
                Path.Combine(tubRoot, "catalog_manifest.json")
            };

            foreach (string manifestPath in manifestCandidates.Where(File.Exists))
            {
                try
                {
                    foreach (string line in File.ReadLines(manifestPath))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        using JsonDocument document = JsonDocument.Parse(line);
                        if (document.RootElement.ValueKind != JsonValueKind.Object ||
                            !TryGetDeletedIndexesElement(document.RootElement, out JsonElement deletedElement) ||
                            deletedElement.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (JsonElement item in deletedElement.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out int number))
                            {
                                deletedIndexes.Add(number);
                            }
                            else if (int.TryParse(item.ToString(), out number))
                            {
                                deletedIndexes.Add(number);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return deletedIndexes;
        }

        private static bool TryGetDeletedIndexesElement(JsonElement root, out JsonElement deletedElement)
        {
            if (root.TryGetProperty("deleted_index", out deletedElement))
            {
                return true;
            }

            deletedElement = default;
            return false;
        }

        private static bool IsDeletedIndexesProperty(string propertyName)
        {
            return string.Equals(propertyName, "deleted_indexes", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(propertyName, "delete_index", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(propertyName, "delete_indexes", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(propertyName, "deleted_index", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<List<PilotFrameData>> ParseRecordJsonFilesAsFramesAsync(
            string tubPathWsl,
            string tubPathWindows,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            var frames = new List<PilotFrameData>();
            var recordFiles = new List<string>();

            recordFiles.AddRange(Directory.EnumerateFiles(tubPathWindows, "record_*.json", SearchOption.TopDirectoryOnly));

            string recordsFolder = Path.Combine(tubPathWindows, "records");
            if (Directory.Exists(recordsFolder))
            {
                recordFiles.AddRange(Directory.EnumerateFiles(recordsFolder, "record_*.json", SearchOption.AllDirectories));
            }

            recordFiles = recordFiles
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (recordFiles.Count == 0)
            {
                return frames;
            }

            progress?.Report(new ProgressReport
            {
                Log = $"record_*.json 파일 {recordFiles.Count}개를 파싱합니다."
            });

            int fallbackIndex = 0;
            foreach (string recordFile in recordFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                JObject? obj;
                try
                {
                    string json = await File.ReadAllTextAsync(recordFile, cancellationToken);
                    obj = JObject.Parse(json);
                }
                catch (Exception ex)
                {
                    progress?.Report(new ProgressReport
                    {
                        Log = $"record JSON 파싱 실패: {recordFile}, {ex.Message}"
                    });
                    continue;
                }

                int index = SafeGetIntFromKeys(obj, "_index", "index", "record/index") ?? fallbackIndex;
                string imageValue = SafeGetStringFromKeys(obj, "cam/image_array", "cam/image_array_path", "image", "image_path", "img") ?? string.Empty;
                double? userAngle = SafeGetDoubleFromKeys(obj, "user/angle", "user/steering", "angle", "steering");
                double? userThrottle = SafeGetDoubleFromKeys(obj, "user/throttle", "throttle");
                string mode = SafeGetStringFromKeys(obj, "user/mode", "mode") ?? string.Empty;
                string imagePath = ResolveTubImagePath(tubPathWsl, tubPathWindows, imageValue, index);

                frames.Add(new PilotFrameData
                {
                    Index = index,
                    TubPath = tubPathWsl,
                    ImagePath = imagePath,
                    UserAngle = userAngle,
                    UserThrottle = userThrottle,
                    Mode = mode
                });

                fallbackIndex++;
            }

            return frames;
        }

        private static List<string> FindTubRoots(
            string inputPathWindows,
            IProgress<ProgressReport>? progress)
        {
            var roots = new List<string>();
            if (string.IsNullOrWhiteSpace(inputPathWindows) || !Directory.Exists(inputPathWindows))
            {
                return roots;
            }

            bool IsTubRoot(string path)
            {
                if (!Directory.Exists(path))
                {
                    return false;
                }

                bool hasManifest = File.Exists(Path.Combine(path, "manifest.json"))
                    || File.Exists(Path.Combine(path, "catalog_manifest.json"));
                bool hasCatalog = Directory.GetFiles(path, "catalog_*.catalog", SearchOption.TopDirectoryOnly).Length > 0;
                bool hasRecords = Directory.GetFiles(path, "record_*.json", SearchOption.TopDirectoryOnly).Length > 0;
                bool hasImagesFolder = Directory.Exists(Path.Combine(path, "images"));

                return hasManifest || hasCatalog || hasRecords || hasImagesFolder;
            }

            if (IsTubRoot(inputPathWindows))
            {
                roots.Add(inputPathWindows);
                progress?.Report(new ProgressReport
                {
                    Log = $"입력 경로가 tub 루트로 확인되었습니다: {inputPathWindows}"
                });
            }

            foreach (string child in Directory.GetDirectories(inputPathWindows, "*", SearchOption.TopDirectoryOnly))
            {
                if (!IsTubRoot(child))
                {
                    continue;
                }

                roots.Add(child);
                progress?.Report(new ProgressReport
                {
                    Log = $"하위 tub 루트를 발견했습니다: {child}"
                });
            }

            return roots
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<PilotFrameData> BuildFramesFromImagesOnly(
            string tubPathWsl,
            string tubPathWindows)
        {
            var frames = new List<PilotFrameData>();
            string imagesFolder = Path.Combine(tubPathWindows, "images");
            string searchFolder = Directory.Exists(imagesFolder) ? imagesFolder : tubPathWindows;

            string[] images = Directory.GetFiles(searchFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                               || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                               || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            for (int i = 0; i < images.Length; i++)
            {
                frames.Add(new PilotFrameData
                {
                    Index = i,
                    TubPath = tubPathWsl,
                    ImagePath = ToWslPathFromWindowsPath(images[i])
                });
            }

            return frames;
        }

        private static string ResolveTubImagePath(
            string tubPathWsl,
            string tubPathWindows,
            string imageValue,
            int index)
        {
            // tub 버전과 편집 방식에 따라 이미지 경로가 절대/상대/images 폴더/파일명만 들어올 수 있습니다.
            // 가능한 후보를 만든 뒤 실제 존재하는 파일을 찾아 WSL 경로로 반환합니다.
            List<string> candidates = new List<string>();
            string imageValueTrimmed = imageValue?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(imageValueTrimmed))
            {
                if (imageValueTrimmed.StartsWith("/"))
                {
                    candidates.Add(imageValueTrimmed);
                }
                else if (Path.IsPathRooted(imageValueTrimmed))
                {
                    candidates.Add(ToWslPathFromWindowsPath(imageValueTrimmed));
                }
                else
                {
                    string relative = imageValueTrimmed.Replace('\\', '/');
                    if (relative.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
                    {
                        candidates.Add(CombineWslPath(tubPathWsl, relative));
                    }
                    else
                    {
                        string imagesRelativeCandidate = CombineWslPath(CombineWslPath(tubPathWsl, "images"), relative);
                        string tubRelativeCandidate = CombineWslPath(tubPathWsl, relative);

                        candidates.Add(imagesRelativeCandidate);
                        if (!string.Equals(imagesRelativeCandidate, tubRelativeCandidate, StringComparison.OrdinalIgnoreCase))
                        {
                            candidates.Add(tubRelativeCandidate);
                        }
                    }
                }
            }

            foreach (string candidate in candidates)
            {
                string windowsCandidate = ToWindowsPathFromWslPath(candidate, GetDistroNameFromWslPath(tubPathWsl));
                if (File.Exists(windowsCandidate))
                {
                    return candidate;
                }
            }

            string imagesFolder = Path.Combine(tubPathWindows, "images");
            if (Directory.Exists(imagesFolder))
            {
                string[] patterns =
                {
                    $"{index}_cam-image_array_.jpg",
                    $"{index}_cam-image_array_.jpeg",
                    $"{index}_cam-image_array_.png"
                };

                foreach (string pattern in patterns)
                {
                    string candidate = Path.Combine(imagesFolder, pattern);
                    if (File.Exists(candidate))
                    {
                        return ToWslPathFromWindowsPath(candidate);
                    }
                }

                string[] fallback = Directory.GetFiles(imagesFolder, $"{index}_*.*", SearchOption.TopDirectoryOnly)
                    .Where(path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                   || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                   || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (fallback.Length > 0)
                {
                    return ToWslPathFromWindowsPath(fallback[0]);
                }

                string[] allImages = Directory.GetFiles(imagesFolder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                   || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                   || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (index >= 0 && index < allImages.Length)
                {
                    return ToWslPathFromWindowsPath(allImages[index]);
                }
            }

            return string.Empty;
        }

        private static string? SafeGetStringFromKeys(JObject obj, params string[] keys)
        {
            foreach (string key in keys)
            {
                JToken? token = FindTokenByKey(obj, key);
                if (token == null || token.Type == JTokenType.Null)
                {
                    continue;
                }

                if (token.Type == JTokenType.String)
                {
                    string? value = token.Value<string>();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
                else
                {
                    string value = token.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            return null;
        }


        private static double? SafeGetDoubleFromKeys(JObject obj, params string[] keys)
        {
            foreach (string key in keys)
            {
                JToken? token = FindTokenByKey(obj, key);
                if (token == null || token.Type == JTokenType.Null)
                {
                    continue;
                }

                if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
                {
                    return token.Value<double>();
                }

                if (double.TryParse(token.ToString(), out double value))
                {
                    return value;
                }
            }

            return null;
        }

        private static int? SafeGetIntFromKeys(JObject obj, params string[] keys)
        {
            foreach (string key in keys)
            {
                JToken? token = FindTokenByKey(obj, key);
                if (token == null || token.Type == JTokenType.Null)
                {
                    continue;
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

                if (int.TryParse(token.ToString(), out int result))
                {
                    return result;
                }
            }

            return null;
        }

        private static JToken? FindTokenByKey(JObject obj, string key)
        {
            if (obj.TryGetValue(key, out JToken? directToken))
            {
                return directToken;
            }

            if (key.Contains('/'))
            {
                JToken? nestedToken = obj;
                foreach (string segment in key.Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (nestedToken is JObject nestedObject
                        && nestedObject.TryGetValue(segment, StringComparison.OrdinalIgnoreCase, out JToken? childToken))
                    {
                        nestedToken = childToken;
                        continue;
                    }

                    nestedToken = null;
                    break;
                }

                if (nestedToken != null)
                {
                    return nestedToken;
                }
            }

            foreach (JProperty property in obj.DescendantsAndSelf().OfType<JObject>().SelectMany(o => o.Properties()))
            {
                if (string.Equals(property.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value;
                }
            }

            return null;
        }

        private static string GetDistroNameFromWslPath(string wslPath)
        {
            _ = wslPath;
            AppSettings settings = LoadSettings();
            return IsUbuntu2204DistroName(settings.WslDistroName)
                ? settings.WslDistroName
                : FallbackWslDistroName;
        }

        // =====================================================
        // Pilot/Trainer가 공유하는 데이터 모델
        // =====================================================

        public class PilotCardState
        {
            // Pilot 화면이 모델 하나에 대해 기억해야 하는 상태 묶음입니다.
            // 모델 파일, WSL 환경, 연결 tub, 추론 결과 경로를 함께 저장합니다.
            public string ModelName { get; set; } = string.Empty;
            public string ModelPath { get; set; } = string.Empty;
            public string ModelType { get; set; } = string.Empty;
            public string DatabaseJsonPath { get; set; } = string.Empty;

            public string WslDistroName { get; set; } = string.Empty;
            public string CondaEnvName { get; set; } = FallbackCondaEnvName;
            public string MyCarPath { get; set; } = string.Empty;

            public List<string> TrainingTubPaths { get; set; } = new List<string>();
            public bool IsTubConnected { get; set; }

            public List<TubDrivingRecord> TubRecords { get; set; } = new List<TubDrivingRecord>();
            public string JudementJsonPath { get; set; } = string.Empty;
            public List<JudementRecord> JudementRecords { get; set; } = new List<JudementRecord>();
        }

        public class TubDrivingRecord
        {
            // tub에 저장된 사람 주행 데이터 한 프레임입니다.
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
            // Python 추론 결과 한 프레임입니다. user 값과 pilot 예측값, 오차를 함께 담습니다.
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

        public class PilotFrameData
        {
            // UI 표시용 통합 프레임입니다. tub 원본값과 AI 추론값을 한 행으로 합친 형태입니다.
            public int Index { get; set; }
            public string TubPath { get; set; } = string.Empty;
            public string ImagePath { get; set; } = string.Empty;

            public double? UserAngle { get; set; }
            public double? UserThrottle { get; set; }

            public double? PilotAngle { get; set; }
            public double? PilotThrottle { get; set; }

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

        // =====================================================
        // Pilot 모델 정보와 tub/AI 판단 데이터 로드
        // =====================================================

        public static async Task<OperationResult<string>> FindMyCarPathInWslAsync(
            string distroName,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(distroName))
            {
                distroName = await GetPreferredWslDistroNameAsync(cancellationToken);
            }

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
            await EnsurePilotRuntimePathsAsync(cardState, progress, cancellationToken);

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

        public static async Task EnsurePilotRuntimePathsAsync(
            PilotCardState cardState,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(cardState.WslDistroName))
            {
                cardState.WslDistroName = await GetPreferredWslDistroNameAsync(cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(cardState.CondaEnvName))
            {
                AppSettings settings = LoadSettings();
                cardState.CondaEnvName = string.IsNullOrWhiteSpace(settings.CondaEnvName)
                    ? FallbackCondaEnvName
                    : settings.CondaEnvName;
            }

            if (string.IsNullOrWhiteSpace(cardState.MyCarPath))
            {
                OperationResult<string> myCarResult = await FindMyCarPathInWslAsync(
                    cardState.WslDistroName,
                    progress,
                    cancellationToken);

                if (myCarResult.Success && !string.IsNullOrWhiteSpace(myCarResult.Data))
                {
                    cardState.MyCarPath = myCarResult.Data;
                }
            }

            if (string.IsNullOrWhiteSpace(cardState.MyCarPath))
            {
                throw new InvalidOperationException("WSL Ubuntu-22.04에서 mycar 폴더를 찾지 못했습니다.");
            }
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
            HashSet<int> deletedIndexes = GetDeletedIndexesForPilotCardState(cardState);
            if (deletedIndexes.Count > 0)
            {
                RemoveDeletedJudementRecordsFromFile(judementPath, deletedIndexes);
            }

            records = FilterJudementRecordsByDeletedIndexes(records, cardState);
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
            // 선택 모델과 tub를 Python 스크립트에 전달해 프레임별 AI 조향/스로틀 예측값을 생성합니다.
            // 결과 JSON은 모델별 output 폴더에 저장하고 다시 읽어 JudementRecord 목록으로 반환합니다.
            await EnsurePilotRuntimePathsAsync(cardState, progress, cancellationToken);

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

            // DB에는 Windows/WSL 경로가 섞여 저장될 수 있으므로 Python에 넘기기 전에 실제 경로로 정규화합니다.
            List<string> resolvedTubPaths = (cardState.TrainingTubPaths ?? new List<string>())
                .Select(path => ResolveTubPathForPython(path, cardState.WslDistroName))
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            string tubsArg = string.Join(";", resolvedTubPaths);
            string modelPathForWsl = ToWslPathFromWindowsPath(cardState.ModelPath);
            string outputPathForWsl = ToWslPathFromWindowsPath(judementPath);
            string pythonPath = await FindJudementPythonAsync(cardState, progress, cancellationToken);
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return new OperationResult<List<JudementRecord>>
                {
                    Success = false,
                    ErrorMessage = "AI 판단에 사용할 Python 환경을 찾지 못했습니다. tensorflow 또는 donkeycar, numpy, pillow가 필요합니다."
                };
            }

            string command = BuildPythonCommand(cardState, scriptPath, modelPathForWsl, outputPathForWsl, tubsArg, pythonPath);

            progress?.Report(new ProgressReport
            {
                Step = "WSL Python 실행 중...",
                Log = $"Python 판단 생성 실행: python={pythonPath}, model={modelPathForWsl}, output={outputPathForWsl}",
                IsIndeterminate = true
            });

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
            HashSet<int> generatedDeletedIndexes = GetDeletedIndexesForPilotCardState(cardState);
            if (generatedDeletedIndexes.Count > 0)
            {
                RemoveDeletedJudementRecordsFromFile(judementPath, generatedDeletedIndexes);
            }

            records = FilterJudementRecordsByDeletedIndexes(records, cardState);
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
            // =====================================================
            // WSL 명령 실행과 Windows/WSL 경로 변환
            // =====================================================
            //
            // wsl.exe -d <distro> bash -lc "<command>" 형태로 명령을 실행합니다.
            // 표준 출력/오류를 모아 실패 원인을 UI 로그에 전달할 수 있게 합니다.
            var psi = new ProcessStartInfo
            {
                FileName = "wsl.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("-d");
            psi.ArgumentList.Add(distroName);
            psi.ArgumentList.Add("bash");
            psi.ArgumentList.Add("-lc");
            psi.ArgumentList.Add(command);

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
            // /home/... 또는 /mnt/c/... 형태의 WSL 경로를 Windows에서 접근 가능한 경로로 바꿉니다.
            // WSL 내부 home 경로는 \\wsl.localhost\<distro>\... UNC 경로로 변환합니다.
            if (string.IsNullOrWhiteSpace(wslPath))
            {
                return string.Empty;
            }

            if (wslPath.StartsWith("\\\\wsl.localhost\\", StringComparison.OrdinalIgnoreCase)
                || wslPath.StartsWith("\\\\wsl$\\", StringComparison.OrdinalIgnoreCase))
            {
                return wslPath;
            }

            if (wslPath.Length >= 2 && wslPath[1] == ':')
            {
                return wslPath;
            }

            if (wslPath.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase))
            {
                string driveLetter = wslPath.Substring(5, 1).ToUpperInvariant();
                string rest = wslPath.Length > 6
                    ? wslPath.Substring(6).TrimStart('/').Replace('/', '\\')
                    : string.Empty;
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
            // C:\... 또는 \\wsl.localhost\... 경로를 bash 명령에서 쓸 수 있는 /mnt/c/... 경로로 바꿉니다.
            if (string.IsNullOrWhiteSpace(windowsPath))
            {
                return string.Empty;
            }

            windowsPath = windowsPath.Trim();

            if (windowsPath.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase)
                || windowsPath.StartsWith("/home/", StringComparison.OrdinalIgnoreCase))
            {
                return windowsPath;
            }

            if (windowsPath.StartsWith("\\\\wsl.localhost\\", StringComparison.OrdinalIgnoreCase)
                || windowsPath.StartsWith("\\\\wsl$\\", StringComparison.OrdinalIgnoreCase))
            {
                string prefix = windowsPath.StartsWith("\\\\wsl.localhost\\", StringComparison.OrdinalIgnoreCase)
                    ? "\\\\wsl.localhost\\"
                    : "\\\\wsl$\\";
                string trimmed = windowsPath.Substring(prefix.Length);
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
                string rest = windowsPath.Substring(2).TrimStart('\\', '/').Replace('\\', '/');
                return $"/mnt/{drive}/{rest}".TrimEnd('/');
            }

            return windowsPath.Replace('\\', '/');
        }

        private static string ResolveExistingWslWindowsPath(string windowsPath, string distroName)
        {
            if (string.IsNullOrWhiteSpace(windowsPath) || Directory.Exists(windowsPath))
            {
                return windowsPath;
            }

            string prefix = windowsPath.StartsWith("\\\\wsl.localhost\\", StringComparison.OrdinalIgnoreCase)
                ? "\\\\wsl.localhost\\"
                : windowsPath.StartsWith("\\\\wsl$\\", StringComparison.OrdinalIgnoreCase)
                    ? "\\\\wsl$\\"
                    : string.Empty;

            if (string.IsNullOrEmpty(prefix))
            {
                return windowsPath;
            }

            string trimmed = windowsPath.Substring(prefix.Length);
            int separatorIndex = trimmed.IndexOf('\\');
            if (separatorIndex < 0)
            {
                return windowsPath;
            }

            string rest = trimmed.Substring(separatorIndex + 1);
            foreach (string candidateDistro in GetWslDistroAliasCandidates(distroName))
            {
                string candidate = prefix + candidateDistro + "\\" + rest;
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            return windowsPath;
        }

        public static string ResolveExistingWindowsPath(string windowsPath)
        {
            if (string.IsNullOrWhiteSpace(windowsPath) || Directory.Exists(windowsPath))
            {
                return windowsPath;
            }

            string root = Path.GetPathRoot(windowsPath) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(root))
            {
                return windowsPath;
            }

            string relative = windowsPath.Substring(root.Length);
            string[] parts = relative.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            string current = root;

            foreach (string part in parts)
            {
                string direct = Path.Combine(current, part);
                if (Directory.Exists(direct) || File.Exists(direct))
                {
                    current = direct;
                    continue;
                }

                string? matched = FindMatchingPathSegment(current, part);
                if (string.IsNullOrWhiteSpace(matched))
                {
                    return windowsPath;
                }

                current = matched;
            }

            return Directory.Exists(current) || File.Exists(current)
                ? current
                : windowsPath;
        }

        private static string? FindMatchingPathSegment(string parentPath, string expectedName)
        {
            if (!Directory.Exists(parentPath))
            {
                return null;
            }

            string expectedKey = NormalizePathSegmentKey(expectedName);
            foreach (string candidate in Directory.EnumerateFileSystemEntries(parentPath))
            {
                string candidateName = Path.GetFileName(candidate);
                if (string.Equals(candidateName, expectedName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(candidateName, expectedName.Replace('_', ' '), StringComparison.OrdinalIgnoreCase)
                    || string.Equals(candidateName, expectedName.Replace(' ', '_'), StringComparison.OrdinalIgnoreCase)
                    || string.Equals(NormalizePathSegmentKey(candidateName), expectedKey, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string NormalizePathSegmentKey(string value)
        {
            return value
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal);
        }

        private static IEnumerable<string> GetWslDistroAliasCandidates(string distroName)
        {
            string[] candidates =
            {
                IsUbuntu2204DistroName(distroName) ? distroName : FallbackWslDistroName,
                "Ubuntu-22.04",
                "Ubuntu22.04",
                "Ubuntu_22.04",
                "Ubuntu-22",
                "ub22"
            };

            return candidates
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsUbuntu2204DistroName(string? distroName)
        {
            if (string.IsNullOrWhiteSpace(distroName))
            {
                return false;
            }

            string normalized = distroName
                .Trim()
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .ToLowerInvariant();

            return normalized is "ubuntu2204" or "ubuntu22";
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

        private static async Task<List<string>> GetInstalledWslDistrosAsync(CancellationToken cancellationToken)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "wsl.exe",
                Arguments = "-l -q",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = psi };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                await WaitForExitAsync(process, cancellationToken);

                return output
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Replace("\0", string.Empty, StringComparison.Ordinal).Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static async Task<bool> WslDistroExistsAsync(string distroName, CancellationToken cancellationToken)
        {
            List<string> distros = await GetInstalledWslDistrosAsync(cancellationToken);
            return distros.Any(name => string.Equals(name, distroName, StringComparison.OrdinalIgnoreCase));
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

            AddTubPathTokens(tubsToken, results);
            return results
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path => path.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void AddTubPathTokens(JToken token, List<string> results)
        {
            if (token.Type == JTokenType.String)
            {
                AddTubPathString(token.Value<string>(), results);
                return;
            }

            if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token)
                {
                    AddTubPathTokens(child, results);
                }

                return;
            }

            if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>())
                {
                    AddTubPathTokens(property.Value, results);
                }

                return;
            }

            return;
        }

        private static void AddTubPathString(string? value, List<string> results)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string trimmed = value.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal) || trimmed.StartsWith("[", StringComparison.Ordinal))
            {
                return;
            }

            foreach (string part in trimmed.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string path = part.Trim();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    results.Add(path);
                }
            }
        }

        private static string NormalizeTubPath(string tubPath, string? carPathFromConfig, string myCarPath)
        {
            if (string.IsNullOrWhiteSpace(tubPath))
            {
                return string.Empty;
            }

            string trimmed = tubPath.Trim().Trim('"');

            if (trimmed.StartsWith("\\\\wsl.localhost\\", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("\\\\wsl$\\", StringComparison.OrdinalIgnoreCase))
            {
                return ToWslPathFromWindowsPath(trimmed);
            }

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

            if (trimmed.StartsWith("\\", StringComparison.Ordinal) && !trimmed.StartsWith("\\\\", StringComparison.Ordinal))
            {
                string baseWindowsPath = basePath.Trim();
                if (baseWindowsPath.Length >= 2 && baseWindowsPath[1] == ':')
                {
                    return ToWslPathFromWindowsPath(baseWindowsPath.Substring(0, 2) + trimmed);
                }
            }

            basePath = NormalizeBaseTubPath(basePath, myCarPath);

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

        private static string NormalizeBaseTubPath(string basePath, string fallbackMyCarPath)
        {
            string normalizedBasePath = string.IsNullOrWhiteSpace(basePath)
                ? fallbackMyCarPath
                : basePath.Trim();

            if (string.IsNullOrWhiteSpace(normalizedBasePath))
            {
                return string.Empty;
            }

            if (normalizedBasePath.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase)
                || normalizedBasePath.StartsWith("/home/", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedBasePath;
            }

            if (normalizedBasePath.StartsWith("\\\\wsl.localhost\\", StringComparison.OrdinalIgnoreCase)
                || normalizedBasePath.StartsWith("\\\\wsl$\\", StringComparison.OrdinalIgnoreCase)
                || (normalizedBasePath.Length >= 2 && normalizedBasePath[1] == ':'))
            {
                return ToWslPathFromWindowsPath(normalizedBasePath);
            }

            return normalizedBasePath.Replace('\\', '/');
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
                        PilotAngle = ClampPilotValue(node["pilot_angle"]?.GetValue<double?>()),
                        PilotThrottle = ClampPilotValue(node["pilot_throttle"]?.GetValue<double?>()),
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

        private static double? ClampPilotValue(double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return Math.Max(-1.0, Math.Min(1.0, value.Value));
        }

        private static List<JudementRecord> FilterJudementRecordsByDeletedIndexes(
            List<JudementRecord> records,
            PilotCardState cardState)
        {
            if (records == null || records.Count == 0)
            {
                return new List<JudementRecord>();
            }

            HashSet<int> deletedIndexes = GetDeletedIndexesForPilotCardState(cardState);
            if (deletedIndexes.Count == 0)
            {
                return records;
            }

            return records
                .Where(record => !deletedIndexes.Contains(record.Index))
                .ToList();
        }

        private static void RemoveDeletedJudementRecordsFromFile(
            string judementPath,
            HashSet<int> deletedIndexes)
        {
            if (string.IsNullOrWhiteSpace(judementPath) ||
                deletedIndexes == null ||
                deletedIndexes.Count == 0 ||
                !File.Exists(judementPath))
            {
                return;
            }

            try
            {
                JsonNode? root = JsonNode.Parse(File.ReadAllText(judementPath));
                if (root is not JsonObject rootObject ||
                    rootObject["records"] is not JsonArray recordsArray)
                {
                    return;
                }

                for (int i = recordsArray.Count - 1; i >= 0; i--)
                {
                    JsonNode? node = recordsArray[i];
                    int? index = node?["index"]?.GetValue<int>();

                    if (index.HasValue && deletedIndexes.Contains(index.Value))
                    {
                        recordsArray.RemoveAt(i);
                    }
                }

                File.WriteAllText(
                    judementPath,
                    rootObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
            }
        }

        private static HashSet<int> GetDeletedIndexesForPilotCardState(PilotCardState cardState)
        {
            HashSet<int> deletedIndexes = new HashSet<int>();

            if (cardState?.TrainingTubPaths == null)
            {
                return deletedIndexes;
            }

            foreach (string tubPath in cardState.TrainingTubPaths)
            {
                if (string.IsNullOrWhiteSpace(tubPath))
                {
                    continue;
                }

                string windowsPath = tubPath.StartsWith("/", StringComparison.Ordinal)
                    ? ToWindowsPathFromWslPath(tubPath, cardState.WslDistroName)
                    : tubPath;

                windowsPath = ResolveExistingWindowsPath(windowsPath);

                if (!Directory.Exists(windowsPath))
                {
                    continue;
                }

                List<string> roots = FindTubRoots(windowsPath, null);
                if (roots.Count == 0)
                {
                    roots.Add(windowsPath);
                }

                foreach (string root in roots)
                {
                    deletedIndexes.UnionWith(ReadDeletedIndexesFromTubRoot(root));
                }
            }

            return deletedIndexes;
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

            string script = BuildPythonScript();
            await File.WriteAllTextAsync(windowsScriptPath, script, cancellationToken);

            return scriptPath;
        }

        private static async Task<string> FindJudementPythonAsync(
            PilotCardState cardState,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            string homePath = GetHomePathFromMyCarPath(cardState.MyCarPath);
            // 사용자별 shell 초기화에 의존하지 않도록 실제 python 실행 파일을 직접 검사합니다.
            List<string> candidates = BuildPythonCandidates(homePath, cardState);

            foreach (string pythonPath in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string windowsPythonPath = ToWindowsPathFromWslPath(pythonPath, cardState.WslDistroName);
                if (!File.Exists(windowsPythonPath))
                {
                    continue;
                }

                string probeCommand =
                    $"'{EscapeBash(pythonPath)}' -c 'import importlib.util as u; ok=(u.find_spec(\"tensorflow\") is not None or u.find_spec(\"donkeycar\") is not None) and u.find_spec(\"numpy\") is not None and u.find_spec(\"PIL\") is not None; raise SystemExit(0 if ok else 1)'";

                OperationResult<string> result = await RunWslCommandAsync(
                    cardState.WslDistroName,
                    probeCommand,
                    null,
                    cancellationToken);

                progress?.Report(new ProgressReport
                {
                    Log = result.Success
                        ? $"AI Python 환경 선택: {pythonPath}"
                        : $"AI Python 후보 제외: {pythonPath}"
                });

                if (result.Success)
                {
                    return pythonPath;
                }
            }

            return string.Empty;
        }

        private static string ResolveTubPathForPython(string tubPath, string distroName)
        {
            if (string.IsNullOrWhiteSpace(tubPath))
            {
                return string.Empty;
            }

            // UNC 경로와 Windows에서 보이는 실제 폴더명을 먼저 보정한 뒤 Python용 WSL 경로로 변환합니다.
            string trimmed = tubPath.Trim();
            string windowsPath = trimmed.StartsWith("/", StringComparison.Ordinal)
                ? ToWindowsPathFromWslPath(trimmed, distroName)
                : trimmed;

            string resolvedWindowsPath = ResolveExistingWindowsPath(windowsPath);
            if (Directory.Exists(resolvedWindowsPath))
            {
                return ToWslPathFromWindowsPath(resolvedWindowsPath);
            }

            return trimmed.StartsWith("/", StringComparison.Ordinal)
                ? trimmed
                : ToWslPathFromWindowsPath(trimmed);
        }

        private static List<string> BuildPythonCandidates(string homePath, PilotCardState cardState)
        {
            var candidates = new List<string>();
            string[] preferredEnvNames =
            {
                cardState.CondaEnvName,
                FallbackCondaEnvName,
                "donkey",
                "donkeycar",
                "base"
            };

            // 자주 쓰는 환경 이름을 먼저 확인하고, 실제 Conda env 폴더 목록을 뒤에 추가합니다.
            foreach (string envName in preferredEnvNames.Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                candidates.Add(CombineWslPath(CombineWslPath(CombineWslPath(homePath, "miniconda3/envs"), envName), "bin/python"));
                candidates.Add(CombineWslPath(CombineWslPath(CombineWslPath(homePath, "anaconda3/envs"), envName), "bin/python"));
            }

            candidates.Add(CombineWslPath(homePath, "miniconda3/bin/python"));
            candidates.Add(CombineWslPath(homePath, "anaconda3/bin/python"));
            AddPythonCandidatesFromEnvRoot(candidates, CombineWslPath(homePath, "miniconda3/envs"));
            AddPythonCandidatesFromEnvRoot(candidates, CombineWslPath(homePath, "anaconda3/envs"));

            return candidates
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void AddPythonCandidatesFromEnvRoot(List<string> candidates, string envRootWsl)
        {
            string envRootWindows = ToWindowsPathFromWslPath(envRootWsl, FallbackWslDistroName);
            if (!Directory.Exists(envRootWindows))
            {
                return;
            }

            foreach (string envFolder in Directory.EnumerateDirectories(envRootWindows))
            {
                string envName = Path.GetFileName(envFolder);
                candidates.Add(CombineWslPath(CombineWslPath(envRootWsl, envName), "bin/python"));
            }
        }

        private static string GetHomePathFromMyCarPath(string myCarPath)
        {
            string normalized = myCarPath.TrimEnd('/');
            if (normalized.EndsWith("/mycar", StringComparison.OrdinalIgnoreCase))
            {
                int index = normalized.LastIndexOf('/', normalized.Length - 1);
                if (index > 0)
                {
                    return normalized.Substring(0, index);
                }
            }

            return "/home";
        }

        private static string BuildPythonCommand(
            PilotCardState cardState,
            string scriptPath,
            string modelPath,
            string outputPath,
            string tubsArg,
            string pythonPath)
        {
            // C#에서 선택한 python을 직접 실행해 conda activate 방식의 환경별 차이를 피합니다.
            string command =
                $"cd '{EscapeBash(cardState.MyCarPath)}' || exit 12; " +
                $"'{EscapeBash(pythonPath)}' '{EscapeBash(scriptPath)}'" +
                $" --model '{EscapeBash(modelPath)}'" +
                $" --model-name '{EscapeBash(cardState.ModelName)}'" +
                $" --model-type '{EscapeBash(cardState.ModelType)}'" +
                $" --tubs '{EscapeBash(tubsArg)}'" +
                $" --output '{EscapeBash(outputPath)}'" +
                $" --car-path '{EscapeBash(cardState.MyCarPath)}'" +
                " --batch-size 6";

            return command;
        }

        private static string BuildPythonScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine("import argparse");
            sb.AppendLine("import json");
            sb.AppendLine("import os");
            sb.AppendLine("import sys");
            sb.AppendLine("import time");
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
            sb.AppendLine("    parser.add_argument('--batch-size', type=int, default=6)");
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
            sb.AppendLine("        from tensorflow.keras.models import load_model");
            sb.AppendLine("        return load_model(model_path, compile=False)");
            sb.AppendLine("    except Exception as tf_ex:");
            sb.AppendLine("        try:");
            sb.AppendLine("            import donkeycar.parts.keras as dk");
            sb.AppendLine("            candidate_names = ['KerasLinear', 'KerasCategorical', 'KerasIMU', 'KerasBehavioral', 'KerasPilot']");
            sb.AppendLine("            candidates = [getattr(dk, name) for name in candidate_names if hasattr(dk, name)]");
            sb.AppendLine("            errors = []");
            sb.AppendLine("            for cls in candidates:");
            sb.AppendLine("                try:");
            sb.AppendLine("                    pilot = cls()");
            sb.AppendLine("                    pilot.load(model_path)");
            sb.AppendLine("                    return pilot.model");
            sb.AppendLine("                except Exception as ex:");
            sb.AppendLine("                    errors.append(f'{cls.__name__}: {ex}')");
            sb.AppendLine("            raise RuntimeError('model load failed. tensorflow=' + str(tf_ex) + '; donkey=' + ' | '.join(errors))");
            sb.AppendLine("        except Exception as donkey_ex:");
            sb.AppendLine("            raise RuntimeError('model load failed. tensorflow=' + str(tf_ex) + '; donkey=' + str(donkey_ex))");
            sb.AppendLine("");
            sb.AppendLine("def collect_records(tub_path):");
            sb.AppendLine("    records = []");
            sb.AppendLine("    if not os.path.isdir(tub_path):");
            sb.AppendLine("        print(f'tub missing: {tub_path}', flush=True)");
            sb.AppendLine("        return records");
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
            sb.AppendLine("        if '/' in key:");
            sb.AppendLine("            current = obj");
            sb.AppendLine("            found = True");
            sb.AppendLine("            for part in key.split('/'):");
            sb.AppendLine("                if isinstance(current, dict) and part in current:");
            sb.AppendLine("                    current = current.get(part)");
            sb.AppendLine("                else:");
            sb.AppendLine("                    found = False");
            sb.AppendLine("                    break");
            sb.AppendLine("            if found and current is not None:");
            sb.AppendLine("                return current");
            sb.AppendLine("    return None");
            sb.AppendLine("");
            sb.AppendLine("def resolve_image_path(tub_path, value):");
            sb.AppendLine("    if not value:");
            sb.AppendLine("        return ''");
            sb.AppendLine("    if value.startswith('/'):");
            sb.AppendLine("        return value");
            sb.AppendLine("    candidates = []");
            sb.AppendLine("    normalized = value.replace('\\\\', '/')");
            sb.AppendLine("    if normalized.startswith('images/'):");
            sb.AppendLine("        candidates.append(os.path.join(tub_path, normalized))");
            sb.AppendLine("    else:");
            sb.AppendLine("        candidates.append(os.path.join(tub_path, 'images', normalized))");
            sb.AppendLine("        candidates.append(os.path.join(tub_path, normalized))");
            sb.AppendLine("    for candidate in candidates:");
            sb.AppendLine("        if os.path.exists(candidate):");
            sb.AppendLine("            return candidate");
            sb.AppendLine("    return candidates[0] if candidates else ''");
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
            sb.AppendLine("                    record_idx = obj.get('_index', obj.get('index', idx))");
            sb.AppendLine("                    records.append((record_idx, obj))");
            sb.AppendLine("    return records");
            sb.AppendLine("");
            sb.AppendLine("def load_deleted_index(tub_path):");
            sb.AppendLine("    deleted = set()");
            sb.AppendLine("    for name in ['manifest.json', 'catalog_manifest.json']:");
            sb.AppendLine("        path = os.path.join(tub_path, name)");
            sb.AppendLine("        if not os.path.exists(path):");
            sb.AppendLine("            continue");
            sb.AppendLine("        try:");
            sb.AppendLine("            with open(path, 'r', encoding='utf-8') as f:");
            sb.AppendLine("                for line in f:");
            sb.AppendLine("                    line = line.strip()");
            sb.AppendLine("                    if not line:");
            sb.AppendLine("                        continue");
            sb.AppendLine("                    obj = json.loads(line)");
            sb.AppendLine("                    values = obj.get('deleted_index')");
            sb.AppendLine("                    if isinstance(values, list):");
            sb.AppendLine("                        for value in values:");
            sb.AppendLine("                            try:");
            sb.AppendLine("                                deleted.add(int(value))");
            sb.AppendLine("                            except Exception:");
            sb.AppendLine("                                pass");
            sb.AppendLine("        except Exception:");
            sb.AppendLine("            pass");
            sb.AppendLine("    return deleted");
            sb.AppendLine("");
            sb.AppendLine("def is_deleted_record(idx, deleted_index):");
            sb.AppendLine("    try:");
            sb.AppendLine("        return int(idx) in deleted_index");
            sb.AppendLine("    except Exception:");
            sb.AppendLine("        return False");
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
            sb.AppendLine("        arr = np.asarray(img, dtype=np.float32)");
            sb.AppendLine("    return arr");
            sb.AppendLine("");
            sb.AppendLine("def clamp_pilot_value(value):");
            sb.AppendLine("    try:");
            sb.AppendLine("        value = float(value)");
            sb.AppendLine("    except Exception:");
            sb.AppendLine("        return 0.0");
            sb.AppendLine("    if not np.isfinite(value):");
            sb.AppendLine("        return 0.0");
            sb.AppendLine("    return max(-1.0, min(1.0, value))");
            sb.AppendLine("");
            sb.AppendLine("def linear_unbin(arr):");
            sb.AppendLine("    arr = np.asarray(arr).reshape(-1)");
            sb.AppendLine("    if len(arr) <= 1:");
            sb.AppendLine("        return clamp_pilot_value(arr[0] if len(arr) else 0.0)");
            sb.AppendLine("    idx = int(np.argmax(arr))");
            sb.AppendLine("    return clamp_pilot_value((idx * 2.0 / (len(arr) - 1)) - 1.0)");
            sb.AppendLine("");
            sb.AppendLine("def first_scalar(value, default=0.0):");
            sb.AppendLine("    arr = np.asarray(value).reshape(-1)");
            sb.AppendLine("    if len(arr) == 0:");
            sb.AppendLine("        return default");
            sb.AppendLine("    return arr[0]");
            sb.AppendLine("");
            sb.AppendLine("def model_has_internal_normalization(model):");
            sb.AppendLine("    try:");
            sb.AppendLine("        layers = getattr(model, 'layers', []) or []");
            sb.AppendLine("        for layer in layers[:3]:");
            sb.AppendLine("            name = (getattr(layer, 'name', '') or '').lower()");
            sb.AppendLine("            cls = layer.__class__.__name__.lower()");
            sb.AppendLine("            if 'lambda' in cls or 'rescaling' in cls or 'normal' in name or 'scale' in name:");
            sb.AppendLine("                return True");
            sb.AppendLine("    except Exception:");
            sb.AppendLine("        pass");
            sb.AppendLine("    return False");
            sb.AppendLine("");
            sb.AppendLine("def prepare_model_input(model, input_arr):");
            sb.AppendLine("    input_arr = np.asarray(input_arr, dtype=np.float32)");
            sb.AppendLine("    if model_has_internal_normalization(model):");
            sb.AppendLine("        return input_arr");
            sb.AppendLine("    try:");
            sb.AppendLine("        if np.nanmax(input_arr) > 2.0:");
            sb.AppendLine("            return input_arr / 255.0");
            sb.AppendLine("    except Exception:");
            sb.AppendLine("        pass");
            sb.AppendLine("    return input_arr");
            sb.AppendLine("");
            sb.AppendLine("def decode_prediction(pred, model_type):");
            sb.AppendLine("    model_type = (model_type or '').lower()");
            sb.AppendLine("    if isinstance(pred, list):");
            sb.AppendLine("        angle_raw = np.asarray(pred[0]).reshape(-1)");
            sb.AppendLine("        throttle_raw = np.asarray(pred[1]).reshape(-1) if len(pred) > 1 else np.asarray([0.0])");
            sb.AppendLine("        if 'categorical' in model_type or len(angle_raw) > 2:");
            sb.AppendLine("            angle = linear_unbin(angle_raw)");
            sb.AppendLine("        else:");
            sb.AppendLine("            angle = clamp_pilot_value(first_scalar(angle_raw))");
            sb.AppendLine("        throttle = clamp_pilot_value(first_scalar(throttle_raw))");
            sb.AppendLine("        return angle, throttle");
            sb.AppendLine("    row = np.asarray(pred).reshape(-1)");
            sb.AppendLine("    if len(row) > 2:");
            sb.AppendLine("        return linear_unbin(row), 0.0");
            sb.AppendLine("    if len(row) >= 2:");
            sb.AppendLine("        return clamp_pilot_value(row[0]), clamp_pilot_value(row[1])");
            sb.AppendLine("    return clamp_pilot_value(row[0] if len(row) else 0.0), 0.0");
            sb.AppendLine("");
            sb.AppendLine("def predict_model(model, image_arr, model_type):");
            sb.AppendLine("    input_arr = prepare_model_input(model, np.expand_dims(image_arr, axis=0))");
            sb.AppendLine("    pred = model.predict(input_arr, verbose=0)");
            sb.AppendLine("    if isinstance(pred, list):");
            sb.AppendLine("        pred = [np.asarray(item)[0] for item in pred]");
            sb.AppendLine("    else:");
            sb.AppendLine("        pred = np.asarray(pred)[0]");
            sb.AppendLine("    return decode_prediction(pred, model_type)");
            sb.AppendLine("");
            sb.AppendLine("def predict_batch(model, image_arrs, model_type):");
            sb.AppendLine("    if not image_arrs:");
            sb.AppendLine("        return []");
            sb.AppendLine("    input_arr = prepare_model_input(model, np.asarray(image_arrs))");
            sb.AppendLine("    pred = model.predict(input_arr, verbose=0)");
            sb.AppendLine("    results = []");
            sb.AppendLine("    if isinstance(pred, list):");
            sb.AppendLine("        for i in range(len(image_arrs)):");
            sb.AppendLine("            row = [np.asarray(item)[i] for item in pred]");
            sb.AppendLine("            results.append(decode_prediction(row, model_type))");
            sb.AppendLine("        return results");
            sb.AppendLine("    for row in pred:");
            sb.AppendLine("        results.append(decode_prediction(row, model_type))");
            sb.AppendLine("    return results");
            sb.AppendLine("");
            sb.AppendLine("def append_result(results, tub, idx, obj, image_path, angle, throttle):");
            sb.AppendLine("    user_angle = extract_value(obj, ['user/angle', 'angle', 'steering'])");
            sb.AppendLine("    user_throttle = extract_value(obj, ['user/throttle', 'throttle'])");
            sb.AppendLine("    mode = extract_value(obj, ['user/mode', 'mode'])");
            sb.AppendLine("    angle_error = None if user_angle is None else angle - float(user_angle)");
            sb.AppendLine("    throttle_error = None if user_throttle is None else throttle - float(user_throttle)");
            sb.AppendLine("    results.append({");
            sb.AppendLine("        'index': int(idx),");
            sb.AppendLine("        'tub_path': tub,");
            sb.AppendLine("        'image_path': image_path,");
            sb.AppendLine("        'user_angle': user_angle,");
            sb.AppendLine("        'user_throttle': user_throttle,");
            sb.AppendLine("        'pilot_angle': angle,");
            sb.AppendLine("        'pilot_throttle': throttle,");
            sb.AppendLine("        'angle_error': angle_error,");
            sb.AppendLine("        'throttle_error': throttle_error,");
            sb.AppendLine("        'mode': mode or ''");
            sb.AppendLine("    })");
            sb.AppendLine("");
            sb.AppendLine("def main():");
            sb.AppendLine("    args = parse_args()");
            sb.AppendLine("    tubs = parse_tubs(args.tubs)");
            sb.AppendLine("    if not tubs:");
            sb.AppendLine("        print('No tubs')");
            sb.AppendLine("        sys.exit(1)");
            sb.AppendLine("    model = load_model(args.model)");
            sb.AppendLine("    results = []");
            sb.AppendLine("    batch_size = max(1, min(6, int(args.batch_size)))");
            sb.AppendLine("    last_report_at = time.monotonic()");
            sb.AppendLine("    processed_count = 0");
            sb.AppendLine("    skipped_count = 0");
            sb.AppendLine("    total_count = 0");
            sb.AppendLine("    def report_progress(force=False):");
            sb.AppendLine("        nonlocal last_report_at");
            sb.AppendLine("        now = time.monotonic()");
            sb.AppendLine("        if force or now - last_report_at >= 5:");
            sb.AppendLine("            print(f'progress: completed={processed_count}, skipped={skipped_count}, total={total_count}', flush=True)");
            sb.AppendLine("            last_report_at = now");
            sb.AppendLine("    for tub in tubs:");
            sb.AppendLine("        deleted_index = load_deleted_index(tub)");
            sb.AppendLine("        record_files = collect_records(tub)");
            sb.AppendLine("        records = load_records_from_files(record_files)");
            sb.AppendLine("        if not records:");
            sb.AppendLine("            records = load_records_from_catalog(tub)");
            sb.AppendLine("        if deleted_index:");
            sb.AppendLine("            before = len(records)");
            sb.AppendLine("            records = [(idx, obj) for idx, obj in records if not is_deleted_record(idx, deleted_index)]");
            sb.AppendLine("            print(f'skip deleted indexes: {tub} -> {before - len(records)}', flush=True)");
            sb.AppendLine("        total_count += len(records)");
            sb.AppendLine("        print(f'tub records: {tub} -> {len(records)}', flush=True)");
            sb.AppendLine("        batch_meta = []");
            sb.AppendLine("        batch_images = []");
            sb.AppendLine("        for idx, obj in records:");
            sb.AppendLine("            image_value = extract_value(obj, ['cam/image_array', 'image', 'image_path', 'img', 'cam/image_array_path'])");
            sb.AppendLine("            image_path = resolve_image_path(tub, image_value)");
            sb.AppendLine("            if not image_path or not os.path.exists(image_path):");
            sb.AppendLine("                skipped_count += 1");
            sb.AppendLine("                report_progress()");
            sb.AppendLine("                continue");
            sb.AppendLine("            image_arr = image_to_array(image_path, args.image_w, args.image_h)");
            sb.AppendLine("            batch_images.append(image_arr)");
            sb.AppendLine("            batch_meta.append((tub, idx, obj, image_path))");
            sb.AppendLine("            if len(batch_images) >= batch_size:");
            sb.AppendLine("                for meta, pred in zip(batch_meta, predict_batch(model, batch_images, args.model_type)):");
            sb.AppendLine("                    append_result(results, meta[0], meta[1], meta[2], meta[3], pred[0], pred[1])");
            sb.AppendLine("                    processed_count += 1");
            sb.AppendLine("                report_progress()");
            sb.AppendLine("                batch_images = []");
            sb.AppendLine("                batch_meta = []");
            sb.AppendLine("        if batch_images:");
            sb.AppendLine("            for meta, pred in zip(batch_meta, predict_batch(model, batch_images, args.model_type)):");
            sb.AppendLine("                append_result(results, meta[0], meta[1], meta[2], meta[3], pred[0], pred[1])");
            sb.AppendLine("                processed_count += 1");
            sb.AppendLine("            report_progress()");
            sb.AppendLine("    report_progress(True)");
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
