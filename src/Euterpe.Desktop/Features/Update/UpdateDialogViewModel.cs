using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Features.Update;

public sealed partial class UpdateDialogViewModel(string version) : ObservableObject, IDialogContext
{
    public string VersionDisplay { get; } = $"v{version}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    public partial int Progress { get; set; }

    public string ProgressText => $"{Progress}%";

    public event EventHandler<object?>? RequestClose;

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public void Report(int progress) => Progress = progress;
}
