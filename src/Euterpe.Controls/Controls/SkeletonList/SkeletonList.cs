namespace Euterpe.Controls;

public sealed class SkeletonList : ItemsControl
{
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<SkeletonList, int>(nameof(Count));

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CountProperty)
        {
            ItemsSource = new int[Count];
        }
    }
}
