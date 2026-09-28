namespace Euterpe.Mvvm;

public abstract partial class ViewModelBase : ObservableObject, IAsyncInitializable
{
    private readonly Lazy<Task> _initialization;

    #region Injections

    public required IPlatformLauncher Launcher { get; init; }

    #endregion Injections

    protected ViewModelBase() => _initialization = new Lazy<Task>(OnInitializeAsync);

    public Task InitializeAsync() => _initialization.Value;

    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    [RelayCommand]
    private Task OpenFileAsync(string filePath) => Launcher.OpenFileAsync(filePath);

    [RelayCommand]
    private Task OpenFolderAsync(string folderPath) => Launcher.OpenFolderAsync(folderPath);

    [RelayCommand]
    private Task OpenUrlAsync(string url) => Launcher.OpenUriAsync(url);

    [RelayCommand]
    private Task RevealFileAsync(string filePath) => Launcher.RevealFileAsync(filePath);
}
