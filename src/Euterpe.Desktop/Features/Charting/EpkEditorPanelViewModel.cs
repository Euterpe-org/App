using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Euterpe.Features.Charting;

[Register]
[AppSingleton]
public sealed partial class EpkEditorPanelViewModel : ViewModelBase
{
    private Dictionary<string, ManifestFileEntry> _files = null!;
    private Dictionary<string, ManifestMap> _originalMaps = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? NameRomanized { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    public partial string Author { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial bool SafeForStreamer { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    public partial int? Bpm { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyPropertyChangedFor(nameof(BpmRangeInvalid))]
    public partial bool IsBpmRange { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyPropertyChangedFor(nameof(BpmRangeInvalid))]
    public partial int? BpmMin { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyPropertyChangedFor(nameof(BpmRangeInvalid))]
    public partial int? BpmMax { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    public partial string Scene { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? SceneEgg { get; set; }

    [ObservableProperty]
    public partial float? BackgroundVideoOpacity { get; set; }

    [ObservableProperty]
    public partial bool HasHiddenDifficulty { get; set; }

    [ObservableProperty]
    public partial string? HideMode { get; set; }

    [ObservableProperty]
    public partial string? HideRatingOverride { get; set; }

    [ObservableProperty]
    public partial string? HideMessage { get; set; }

    [ObservableProperty]
    public partial string FilePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FolderPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? CoverPath { get; set; }

    [ObservableProperty]
    public partial bool HasVideo { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMissingFiles))]
    [NotifyPropertyChangedFor(nameof(MissingFilesMessage))]
    public partial int MissingFileCount { get; set; }

    [ObservableProperty]
    public partial string FilesSummary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsDirty { get; set; }

    public ObservableCollection<string> SearchKeywords { get; } = [];

    public ObservableCollection<DifficultyEditViewModel> Maps { get; } = [];

    public ObservableCollection<EpkFileEntry> Files { get; } = [];

    public bool CanSave =>
        !Name.IsNullOrWhiteSpace()
        && !Author.IsNullOrWhiteSpace()
        && !Scene.IsNullOrWhiteSpace()
        && Bpm is not null
        && !BpmRangeInvalid
        && Maps.Any(static map => map.Difficulty is ChartDifficulty.Hard)
        && Maps.All(static map => !map.Rating.IsNullOrWhiteSpace() && map.Charters.Any(static charter => !charter.IsNullOrWhiteSpace()));

    public bool BpmRangeInvalid =>
        IsBpmRange && (BpmMin is not { } min || BpmMax is not { } max || min > max);

    public bool HasMissingFiles => MissingFileCount > 0;

    public string MissingFilesMessage => string.Format(XAML.EpkEditor_MissingFiles, MissingFileCount);

    public EpkEditorPanelViewModel() => SearchKeywords.CollectionChanged += OnChildEdited;

    public event Action? CloseRequested;
    public event Action<string>? Saved;

    public void Open(string filePath, Manifest manifest)
    {
        _originalMaps = manifest.Meta.Maps;
        _files = new Dictionary<string, ManifestFileEntry>(manifest.Files, StringComparer.OrdinalIgnoreCase);
        var meta = manifest.Meta;

        Name = meta.Name;
        NameRomanized = meta.NameRomanized;
        Author = meta.Author;
        Description = meta.Description;
        SafeForStreamer = meta.SafeForStreamer;
        Bpm = meta.Bpm;
        IsBpmRange = meta is { BpmMin: not null, BpmMax: not null };
        BpmMin = meta.BpmMin;
        BpmMax = meta.BpmMax;
        Scene = meta.Scene;
        SceneEgg = meta.SceneEgg;
        BackgroundVideoOpacity = meta.BackgroundVideoOpacity;
        HideMode = meta.HideMode;
        HideRatingOverride = meta.HideRatingOverride;
        HideMessage = meta.HideMessage;

        FilePath = filePath;
        FolderPath = Path.GetDirectoryName(filePath)!;
        CoverPath = _files.FindCoverPath(FolderPath);

        SearchKeywords.Clear();
        foreach (var keyword in meta.SearchKeywords ?? [])
        {
            SearchKeywords.Add(keyword);
        }

        ReconcileMaps(false);
        RefreshFilesDisplay();

        IsDirty = false;
    }

    public void CreateNew(string folder) =>
        Open(Path.Combine(folder, ChartFiles.ManifestFileName), new Manifest
        {
            Schema = Manifest.CurrentSchema,
            Meta = new ManifestMeta
            {
                Name = string.Empty,
                Author = string.Empty,
                Scene = "scene_01",
                Maps = new Dictionary<string, ManifestMap>(StringComparer.OrdinalIgnoreCase)
            },
            Files = ScanFiles(folder, null)
        });

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var bytes = MessagePackSerialization.SerializeManifest(BuildManifest());
        if (await FileSystemService.TryWriteFileAtomicAsync(FilePath, bytes).ConfigureAwait(true))
        {
            IsDirty = false;
            NotificationService.SuccessLight(Notification_Content_Epk_Save_Success, Name);
            Saved?.Invoke(FolderPath);
        }
        else
        {
            NotificationService.ErrorLight(Notification_Content_Epk_Save_Failed, Name);
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        if (IsDirty &&
            !await MessageBoxService.WarningConfirmAsync(MessageBox_Content_Epk_Discard_Confirm).ConfigureAwait(true))
        {
            return;
        }

        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void RefreshFiles()
    {
        _files = ScanFiles(FolderPath, _files);
        ReconcileMaps(true);
        RefreshFilesDisplay();
        IsDirty = true;
    }

    // Online charts are blocked on open, so server-stamped fields (Cid, Uploader, timestamps, PredictedRating) stay null.
    private Manifest BuildManifest()
    {
        var maps = new Dictionary<string, ManifestMap>(Maps.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var row in Maps)
        {
            maps[ChartFiles.MapName(row.Difficulty)] = new ManifestMap
            {
                Rating = row.Rating,
                Charters = CleanTags(row.Charters)
            };
        }

        var keywords = CleanTags(SearchKeywords);

        return new Manifest
        {
            Schema = Manifest.CurrentSchema,
            Meta = new ManifestMeta
            {
                Name = Name,
                NameRomanized = NullIfBlank(NameRomanized),
                Author = Author,
                Description = NullIfBlank(Description),
                SafeForStreamer = SafeForStreamer,
                Bpm = Bpm!.Value,
                BpmMin = IsBpmRange ? BpmMin : null,
                BpmMax = IsBpmRange ? BpmMax : null,
                Scene = Scene,
                SceneEgg = NullIfBlank(SceneEgg),
                BackgroundVideoOpacity = BackgroundVideoOpacity,
                SearchKeywords = keywords is [] ? null : keywords,
                Maps = maps,
                HideMode = NullIfBlank(HideMode),
                HideRatingOverride = NullIfBlank(HideRatingOverride),
                HideMessage = NullIfBlank(HideMessage)
            },
            Files = _files
        };
    }

    // Rows track the .bms files on disk; the manifest only prefills each row's rating and charters.
    private void ReconcileMaps(bool preserveEdits)
    {
        var kept = preserveEdits
            ? Maps.ToDictionary(static row => row.Difficulty)
            : new Dictionary<ChartDifficulty, DifficultyEditViewModel>();

        Maps.Clear();
        foreach (var difficulty in _files.ExistingDifficulties())
        {
            if (!kept.TryGetValue(difficulty, out var row))
            {
                var manifestMap = _originalMaps.GetValueOrDefault(ChartFiles.MapName(difficulty));
                row = new DifficultyEditViewModel(difficulty, manifestMap?.Rating ?? string.Empty, manifestMap?.Charters ?? []);
                row.PropertyChanged += OnChildEdited;
                row.Charters.CollectionChanged += OnChildEdited;
            }

            Maps.Add(row);
        }

        HasHiddenDifficulty = Maps.Any(static row => row.Difficulty is ChartDifficulty.Hidden);
        NotifySaveCanExecuteChanged();
    }

    private Dictionary<string, ManifestFileEntry> ScanFiles(string folder, IReadOnlyDictionary<string, ManifestFileEntry>? carryVersionsFrom)
    {
        var scanned = new Dictionary<string, ManifestFileEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, size) in FileSystemService.GetFileSizes(folder))
        {
            if (string.Equals(name, ChartFiles.ManifestFileName, StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var version = carryVersionsFrom?.GetValueOrDefault(name)?.Version ?? 1;
            scanned[name] = new ManifestFileEntry { Version = version, Size = size };
        }

        return scanned;
    }

    private void RefreshFilesDisplay()
    {
        Files.Clear();
        foreach (var (name, entry) in _files.OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            Files.Add(new EpkFileEntry(name, entry.Size));
        }

        HasVideo = _files.ContainsKey(ChartFiles.VideoFileName);
        var totalSize = _files.Values.Sum(static entry => entry.Size);
        FilesSummary = string.Format(XAML.EpkEditor_Files_Summary, Files.Count, totalSize.ToReadableSize());

        var present = FileSystemService.GetFileSizes(FolderPath);
        MissingFileCount = _files.Keys.Count(name => !present.ContainsKey(name));
    }

    private void OnChildEdited(object? sender, EventArgs e)
    {
        IsDirty = true;
        NotifySaveCanExecuteChanged();
    }

    private void NotifySaveCanExecuteChanged()
    {
        OnPropertyChanged(nameof(CanSave));
        SaveCommand.NotifyCanExecuteChanged();
    }

    // Any property set marks the editor dirty; Open resets IsDirty after it finishes loading.
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName != nameof(IsDirty))
        {
            IsDirty = true;
        }
    }

    private static string? NullIfBlank(string? value) =>
        value.IsNullOrWhiteSpace() ? null : value.Trim();

    private static string[] CleanTags(IEnumerable<string> tags) =>
        [.. tags.Select(static tag => tag.Trim()).Where(static tag => tag.Length > 0)];

    #region Injections

    public required IFileSystemService FileSystemService { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
