using Data_Manager;

string modelPath = args.Length > 0
    ? args[0]
    : @"\\wsl.localhost\Ubuntu-22.04\home\cheon\mycar\models\model_20260531_001.h5";

var progress = new Progress<DonkeyAsyncWorker.ProgressReport>(report =>
{
    if (!string.IsNullOrWhiteSpace(report.Step))
    {
        Console.WriteLine($"STEP {report.Step}");
    }

    if (!string.IsNullOrWhiteSpace(report.Log))
    {
        Console.WriteLine($"LOG {report.Log}");
    }
});

var state = new DonkeyAsyncWorker.PilotCardState
{
    ModelName = Path.GetFileNameWithoutExtension(modelPath),
    ModelPath = modelPath,
    MyCarPath = "/home/cheon/mycar",
    WslDistroName = "Ubuntu-22.04"
};

Console.WriteLine($"MODEL {state.ModelPath}");
Console.WriteLine($"MODEL_EXISTS {File.Exists(state.ModelPath)}");

DonkeyAsyncWorker.OperationResult<DonkeyAsyncWorker.PilotCardState> modelResult =
    await DonkeyAsyncWorker.LoadModelInfoFromDatabaseAsync(state, progress, CancellationToken.None);

Console.WriteLine($"MODEL_INFO_SUCCESS {modelResult.Success}");
if (!modelResult.Success || modelResult.Data == null)
{
    Console.WriteLine($"MODEL_INFO_ERROR {modelResult.ErrorMessage}");
    return 1;
}

state = modelResult.Data;
Console.WriteLine($"DATABASE {state.DatabaseJsonPath}");
Console.WriteLine($"MODEL_TYPE {state.ModelType}");
Console.WriteLine($"TUB_COUNT {state.TrainingTubPaths.Count}");

foreach (string tubPath in state.TrainingTubPaths)
{
    string windowsPath = DonkeyAsyncWorker.ToWindowsPathFromWslPath(tubPath, state.WslDistroName);
    string resolvedPath = DonkeyAsyncWorker.ResolveExistingWindowsPath(windowsPath);
    Console.WriteLine($"TUB_WSL {tubPath}");
    Console.WriteLine($"TUB_WINDOWS {windowsPath}");
    Console.WriteLine($"TUB_RESOLVED {resolvedPath}");
    Console.WriteLine($"TUB_WINDOWS_EXISTS {Directory.Exists(windowsPath)}");
    Console.WriteLine($"TUB_RESOLVED_EXISTS {Directory.Exists(resolvedPath)}");

    DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.PilotFrameData>> parseResult =
        await DonkeyAsyncWorker.ParseSingleTubFolderAsync(tubPath, state.WslDistroName, progress, CancellationToken.None);

    Console.WriteLine($"PARSE_SUCCESS {parseResult.Success}");
    Console.WriteLine($"FRAME_COUNT {parseResult.Data?.Count ?? 0}");
    Console.WriteLine($"PARSE_ERROR {parseResult.ErrorMessage}");
}

return 0;
