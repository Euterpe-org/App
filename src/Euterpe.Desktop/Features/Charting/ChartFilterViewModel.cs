namespace Euterpe.Features.Charting;

public sealed partial class ChartFilterViewModel : ObservableObject
{
    private const int RatingLowerBound = 1;
    private const int RatingUpperBound = 12;

    private bool _resetting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOnlineSource))]
    public partial ChartSource Source { get; set; } = ChartSource.Online;

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty] public partial bool ShowEasy { get; set; } = true;
    [ObservableProperty] public partial bool ShowHard { get; set; } = true;
    [ObservableProperty] public partial bool ShowMaster { get; set; } = true;
    [ObservableProperty] public partial bool ShowHidden { get; set; } = true;

    [ObservableProperty] public partial int? RatingMin { get; set; } = RatingLowerBound;
    [ObservableProperty] public partial int? RatingMax { get; set; } = RatingUpperBound;

    [ObservableProperty] public partial int? BpmMin { get; set; }
    [ObservableProperty] public partial int? BpmMax { get; set; }

    [ObservableProperty] public partial bool StreamerSafeOnly { get; set; }
    [ObservableProperty] public partial bool HasVideoOnly { get; set; }

    public Observable<Unit> Changed => field ??= this.ObservePropertyChanges()
        .Where(this, static (name, vm) => !vm._resetting && name is not (nameof(Source) or nameof(IsOnlineSource)))
        .DebounceSearch(nameof(SearchText));

    public bool IsOnlineSource => Source is ChartSource.Online;

    public void Reset()
    {
        _resetting = true;
        SearchText = null;
        ShowEasy = ShowHard = ShowMaster = ShowHidden = true;
        RatingMin = RatingLowerBound;
        RatingMax = RatingUpperBound;
        BpmMin = null;
        BpmMax = null;
        StreamerSafeOnly = false;
        HasVideoOnly = false;
        _resetting = false;
        OnPropertyChanged(string.Empty);
    }

    public bool Matches(ChartDto chart) =>
        MatchesSearch(chart)
        && MatchesDifficulty(chart)
        && MatchesRating(chart)
        && MatchesBpm(chart)
        && MatchesStreamerSafe(chart)
        && MatchesVideo(chart);

    private bool MatchesSearch(ChartDto chart)
    {
        if (SearchText.IsNullOrEmpty())
        {
            return true;
        }

        var meta = chart.Manifest.Meta;

        return ContainsSearchText(meta.Name)
               || ContainsSearchText(meta.NameRomanized)
               || ContainsSearchText(meta.Author)
               || (meta.SearchKeywords?.Any(ContainsSearchText) ?? false)
               || meta.Maps.Values.Any(map => map.Charters.Any(ContainsSearchText));

        bool ContainsSearchText(string? value)
        {
            return value?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }

    private bool MatchesDifficulty(ChartDto chart)
    {
        if (ShowEasy && ShowHard && ShowMaster && ShowHidden)
        {
            return true;
        }

        return (ShowEasy && chart.HasDifficulty(ChartDifficulty.Easy))
               || (ShowHard && chart.HasDifficulty(ChartDifficulty.Hard))
               || (ShowMaster && chart.HasDifficulty(ChartDifficulty.Master))
               || (ShowHidden && chart.HasDifficulty(ChartDifficulty.Hidden));
    }

    private bool MatchesRating(ChartDto chart)
    {
        var min = RatingMin ?? RatingLowerBound;
        var max = RatingMax ?? RatingUpperBound;
        if (min <= RatingLowerBound && max >= RatingUpperBound)
        {
            return true;
        }

        return chart.Manifest.Meta.Maps.Values.Any(m => (int)m.RatingValue >= min && (int)m.RatingValue <= max);
    }

    private bool MatchesBpm(ChartDto chart)
    {
        var bpm = chart.Manifest.Meta.Bpm;
        return (BpmMin is not { } min || bpm >= min) && (BpmMax is not { } max || bpm <= max);
    }

    private bool MatchesStreamerSafe(ChartDto chart) =>
        !StreamerSafeOnly || chart.Manifest.Meta.SafeForStreamer;

    private bool MatchesVideo(ChartDto chart) =>
        !HasVideoOnly || chart.HasVideo;
}
