using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Windows;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsDotNetSdkInstaller : IDotNetSdkInstaller
{
    private const string DotNetSdkUrl = "https://aka.ms/dotnet/10.0/dotnet-sdk-win-x64.exe";

    public async Task<bool> CheckInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-sdks")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            return result.IsSuccess && !result.StandardOutput.IsNullOrEmpty();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check .NET SDK installation");
            return false;
        }
    }

    public async Task InstallAsync()
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Logger.LogInformation("Downloading .NET SDK from {DotNetSdkUrl} to {TempFilePath}", DotNetSdkUrl, tempFilePath);
            await AppDownloadManager.DownloadFileAsync(DotNetSdkUrl, tempFilePath).ConfigureAwait(false);

            Logger.LogInformation("Launching .NET SDK installer: {TempFilePath}", tempFilePath);
            using var process = Process.Start(
                new ProcessStartInfo(tempFilePath)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

            if (process is null)
            {
                throw new InvalidOperationException($"Failed to start .NET SDK installer process at {tempFilePath}");
            }

            await process.WaitForExitAsync().ConfigureAwait(false);

            if (process.ExitCode is not 0)
            {
                throw new InvalidOperationException($".NET SDK installer exited with code {process.ExitCode}");
            }

            Logger.LogInformation(".NET SDK installation completed successfully");
        }
        finally
        {
            FileSystemService.TryDeleteFile(tempFilePath);
        }
    }

    #region Injections

    public required IAppDownloadManager AppDownloadManager { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required ILogger<WindowsDotNetSdkInstaller> Logger { get; init; }

    #endregion Injections
}
