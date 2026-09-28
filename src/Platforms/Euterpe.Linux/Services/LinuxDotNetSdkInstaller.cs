using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxDotNetSdkInstaller : IDotNetSdkInstaller
{
    private const string DotNetInstallScriptUrl = "https://dot.net/v1/dotnet-install.sh";

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
            await AppDownloadManager.DownloadFileAsync(DotNetInstallScriptUrl, tempFilePath).ConfigureAwait(false);
            Logger.LogInformation("Downloaded .NET install script to {TempFilePath}", tempFilePath);

            var chmodResult = await Cli.Wrap("chmod")
                .WithArguments(["+x", tempFilePath])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (chmodResult.ExitCode is not 0)
            {
                throw new InvalidOperationException($"chmod +x dotnet-install.sh failed with exit code {chmodResult.ExitCode}: {chmodResult.StandardError}");
            }

            var installResult = await Cli.Wrap("bash")
                .WithArguments([tempFilePath, "--version", "latest"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (installResult.ExitCode is not 0)
            {
                throw new InvalidOperationException($"dotnet-install.sh failed with exit code {installResult.ExitCode}: {installResult.StandardError}");
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
    public required ILogger<LinuxDotNetSdkInstaller> Logger { get; init; }

    #endregion Injections
}
