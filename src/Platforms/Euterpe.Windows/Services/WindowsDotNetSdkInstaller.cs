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
            var exitStatus = await Process.RunAsync(tempFilePath).ConfigureAwait(false);
            if (exitStatus.ExitCode is not 0)
            {
                throw new InvalidOperationException($".NET SDK installer exited with code {exitStatus.ExitCode}");
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
