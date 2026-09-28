namespace Euterpe.Features.Charting;

public sealed partial class ChartManageItemViewModel(ChartDto chart) : ObservableObject
{
    [ObservableProperty]
    public partial ChartDto Chart { get; set; } = chart;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
