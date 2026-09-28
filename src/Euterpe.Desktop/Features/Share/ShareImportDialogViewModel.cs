using Euterpe.Models.Progress;
using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Features.Share;

[Register]
public sealed partial class ShareImportDialogViewModel : ViewModelBase, IDialogContext
{
    private GameSharePackage? _pendingPackage;

    [ObservableProperty]
    public partial string StatusMessage { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsStatusWarning { get; private set; }

    [ObservableProperty]
    public partial bool CanImport { get; private set; }

    [ObservableProperty]
    public partial double Progress { get; private set; }

    [ObservableProperty]
    public partial string ProgressLabel { get; private set; } = string.Empty;

    public event EventHandler<object?>? RequestClose;

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public async Task PrepareAsync(string? shareText = null)
    {
        shareText ??= await TopLevel.TryGetClipboardTextAsync().ConfigureAwait(true) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(shareText))
        {
            SetStatus(XAML.Share_Import_Empty, true);
            return;
        }

        if (GameShareService.TryParseShareLink(shareText) is not { } package)
        {
            SetStatus(XAML.Share_Import_Invalid, true);
            return;
        }

        if (package.GameId != Config.ActiveGame)
        {
            var gameName = Config.Games.FirstOrDefault(game => game.Id == package.GameId)?.DisplayName ?? package.GameId.ToString();
            SetStatus(string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_WrongGame, gameName), true);
            return;
        }

        SetStatus(string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_Preview, package.ChartIds.Length), false,
            package);
    }

    [RelayCommand]
    private Task RefreshAsync() => PrepareAsync();

    [RelayCommand]
    private async Task ImportAsync(CancellationToken cancellationToken)
    {
        if (_pendingPackage is not { } package)
        {
            return;
        }

        Progress = 0;
        ProgressLabel = string.Empty;
        try
        {
            var progress = new Progress<BatchProgress>(report =>
            {
                Progress = report.Percentage;
                ProgressLabel = report.CountDisplay;
            });
            var result = await GameShareService.ImportAsync(package, progress, cancellationToken).ConfigureAwait(true);
            SetStatus(FormatResult(result), result.Any(static item => item.Outcome is BulkItemOutcome.Failed));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            SetStatus(XAML.Share_Import_Canceled, true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to import game share package");
            SetStatus(XAML.Share_Import_Failed, true);
        }
    }

    [RelayCommand]
    public void CancelImport() => ImportCommand.Cancel();

    private void SetStatus(string message, bool warning, GameSharePackage? package = null)
    {
        _pendingPackage = package;
        CanImport = package is not null;
        IsStatusWarning = warning;
        StatusMessage = message;
    }

    private static string FormatResult(IReadOnlyList<BulkItemResult> result)
    {
        string[] phrases =
        [
            Phrase(BulkItemOutcome.Added, XAML.Share_Import_Result_Added),
            Phrase(BulkItemOutcome.AlreadyPresent, XAML.Share_Import_Result_Present),
            Phrase(BulkItemOutcome.Failed, XAML.Share_Import_Result_Failed)
        ];

        return string.Join(" · ", phrases.Where(static phrase => phrase.Length > 0));

        string Phrase(BulkItemOutcome outcome, string format)
        {
            var count = result.Count(item => item.Outcome == outcome);
            return count is 0 ? string.Empty : string.Format(CultureInfo.CurrentCulture, format, count);
        }
    }

    #region Injections

    public required IGameShareService GameShareService { get; init; }
    public required Config Config { get; init; }
    public required ILogger<ShareImportDialogViewModel> Logger { get; init; }
    public required TopLevelProxy TopLevel { get; init; }

    #endregion Injections
}
