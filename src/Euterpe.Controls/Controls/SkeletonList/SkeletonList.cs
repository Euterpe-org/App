namespace Euterpe.Controls;

public sealed partial class SkeletonList : ItemsControl
{
    [GeneratedStyledProperty]
    public partial int Count { get; set; }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CountProperty)
        {
            ItemsSource = new int[Count];
        }
    }
}
