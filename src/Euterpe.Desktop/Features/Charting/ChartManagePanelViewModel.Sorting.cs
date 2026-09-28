namespace Euterpe.Features.Charting;

public sealed partial class ChartManagePanelViewModel
{
    public static IReadOnlyList<EnumOption<ChartSortField>> SortFields { get; } =
    [
        .. ChartSortFieldExtensions.GetValues().Select(static field =>
            new EnumOption<ChartSortField>(field, $"{nameof(ChartSortField)}_{field.ToStringFast()}"))
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortComparer))]
    public partial ChartSortField SortField { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortComparer))]
    public partial bool SortDescending { get; set; }

    public Comparer<ChartManageItemViewModel> SortComparer => BuildComparer();

    private Comparer<ChartManageItemViewModel> BuildComparer()
    {
        var comparison = SortField switch
        {
            ChartSortField.Author => ByText(x => x.Chart.Manifest.Meta.Author),
            ChartSortField.Bpm => By(x => x.Chart.Manifest.Meta.Bpm),
            ChartSortField.Rating => By(x => x.Chart.MaxRating),
            ChartSortField.DateAdded => By(x => x.Chart.Manifest.Meta.CreatedAt ?? 0),
            ChartSortField.DateUpdated => By(x => x.Chart.Manifest.Meta.UpdatedAt ?? 0),
            ChartSortField.MapCount => By(x => x.Chart.Difficulties.Count),
            ChartSortField.Size => By(x => x.Chart.SizeBytes),
            _ => ByText(x => x.Chart.Manifest.Meta.Name)
        };

        return Comparer<ChartManageItemViewModel>.Create(SortDescending ? (a, b) => comparison(b, a) : comparison);

        static Comparison<ChartManageItemViewModel> By<TKey>(Func<ChartManageItemViewModel, TKey> key) where TKey : IComparable<TKey>
        {
            return (a, b) => key(a).CompareTo(key(b));
        }

        static Comparison<ChartManageItemViewModel> ByText(Func<ChartManageItemViewModel, string> key)
        {
            return (a, b) => string.Compare(key(a), key(b), StringComparison.OrdinalIgnoreCase);
        }
    }
}
