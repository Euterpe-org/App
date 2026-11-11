using static Euterpe.Models.Charts.ChartFiles;

namespace Euterpe.Models.Charts;

public sealed class ChartDto
{
    private double? _maxRating;
    private long? _sizeBytes;

    // Chart Properties
    public required string FolderPath { get; init; }
    public required string FolderName { get; init; }
    public required Manifest Manifest { get; init; }
    public ChartSource Source { get; init; } = ChartSource.Offline;

    public bool IsOnline => Source is ChartSource.Online;

    // Web Urls
    public string? DetailUrl =>
        Manifest.Cid is { } cid ? $"{EuterpeWeb.BaseUrl}/charts/{cid}" : null;

    public string? UploaderPageUrl =>
        Manifest.Meta.Uploader is { } uploader ? $"{EuterpeWeb.BaseUrl}/users/{uploader.Uid}?tab=charts" : null;

    // Asset Paths
    public string? CoverPath => Manifest.Files.FindCoverPath(FolderPath);

    public string? AudioPath => AssetPath(DemoFileName) ?? AssetPath(MusicFileName);
    public string? VideoPath => AssetPath(VideoFileName);
    public bool HasVideo => VideoPath is not null;

    // Difficulties
    public IReadOnlyList<ChartDifficulty> Difficulties =>
        field ??= Manifest.Files.ExistingDifficulties();

    public IReadOnlyList<DifficultyBadge> DifficultyBadges =>
        field ??= [.. Difficulties.Select(difficulty => new DifficultyBadge(difficulty, Manifest.Meta.Maps[MapName(difficulty)].Rating))];

    // Sort Values
    public double MaxRating =>
        _maxRating ??= Manifest.Meta.Maps.Values.Max(static map => map.RatingValue);

    public long SizeBytes => _sizeBytes ??= Manifest.Files.Values.Sum(static file => file.Size);

    // Display Strings
    public LocalizedString? UploaderDisplay =>
        Manifest.Meta.Uploader is { } uploader
            ? XAML.ChartManage_UploadedBy(uploader.Nickname)
            : null;

    public string SizeDisplay => SizeBytes.ToReadableSize();

    public string BpmDisplay =>
        Manifest.Meta is { BpmMin: { } min, BpmMax: { } max }
            ? $"{min}–{max}"
            : Manifest.Meta.Bpm.ToString();

    public string CharterDisplay =>
        string.Join(", ", Manifest.Meta.Maps.Values
            .SelectMany(map => map.Charters)
            .Distinct(StringComparer.OrdinalIgnoreCase));

    // Chart Files
    public bool HasDifficulty(ChartDifficulty difficulty) => Difficulties.Contains(difficulty);

    private string? AssetPath(string fileName) =>
        Manifest.Files.ContainsKey(fileName) ? Path.Combine(FolderPath, fileName) : null;
}
