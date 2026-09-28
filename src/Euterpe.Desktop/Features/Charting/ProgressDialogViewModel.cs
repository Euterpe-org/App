using Euterpe.Models.Progress;
using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Features.Charting;

[Register]
public sealed partial class ProgressDialogViewModel : ViewModelBase, IDialogContext
{
    [ObservableProperty]
    public partial LocalizedString? Hint { get; set; }

    [ObservableProperty]
    public partial bool IsIndeterminate { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressLabel { get; set; } = string.Empty;

    public event EventHandler<object?>? RequestClose;

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public void Reset()
    {
        Hint = null;
        IsIndeterminate = false;
        Progress = 0;
        ProgressLabel = string.Empty;
    }

    public void Report(BatchProgress progress)
    {
        IsIndeterminate = false;
        Progress = progress.Percentage;
        ProgressLabel = progress.CountDisplay;
    }
}
