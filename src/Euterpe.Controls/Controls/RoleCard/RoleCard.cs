namespace Euterpe.Controls;

public sealed class RoleCard : TemplatedControl
{
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<RoleCard, Geometry?>(nameof(Icon));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<RoleCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<RoleCard, string?>(nameof(Description));

    public static readonly StyledProperty<IBrush?> AccentColorProperty =
        AvaloniaProperty.Register<RoleCard, IBrush?>(nameof(AccentColor));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<RoleCard, bool>(nameof(IsSelected));

    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IBrush? AccentColor
    {
        get => GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
}
