using System.Runtime.InteropServices;
using Avalonia.Input.Platform;

namespace Euterpe.Shell;

[Register]
[AppSingleton]
public sealed partial class CrashWindowViewModel : ViewModelBase, IDialog<bool>
{
    public string EnvironmentInfo { get; } =
        $"Euterpe {DisplayVersion} · {RuntimeInformation.OSDescription.Trim()} ({RuntimeInformation.OSArchitecture}) · {RuntimeInformation.FrameworkDescription}";

    public string CrashTime { get; private set; } = string.Empty;

    public string ExceptionType { get; private set; } = string.Empty;

    public string ExceptionMessage { get; private set; } = string.Empty;

    public string ExceptionDetails { get; private set; } = string.Empty;

    public event EventHandler<bool>? RequestClose;
    public void Close(bool result) => RequestClose?.Invoke(this, result);

    public void SetException(Exception ex)
    {
        ExceptionType = ex.GetType().FullName ?? nameof(Exception);
        ExceptionMessage = ex.Message;
        ExceptionDetails = ex.ToString();
        CrashTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    [RelayCommand]
    private async Task CopyAsync()
    {
        try
        {
            var copyText = $"{EnvironmentInfo}{Environment.NewLine}{CrashTime}{Environment.NewLine}{Environment.NewLine}{ExceptionDetails}";
            await TopLevel.Clipboard.SetTextAsync(copyText).ConfigureAwait(true);
            await MessageBoxService.SuccessAsync(MessageBox_Content_CrashDialog_CopySuccess).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Crash dialog clipboard copy failed");
            await MessageBoxService.ErrorAsync(MessageBox_Content_CrashDialog_CopyFailed).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private void Continue() => Close(true);

    [RelayCommand]
    private void Exit() => Close(false);

    #region Injections

    public required TopLevelProxy TopLevel { get; init; }
    public required ILogger<CrashWindowViewModel> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
