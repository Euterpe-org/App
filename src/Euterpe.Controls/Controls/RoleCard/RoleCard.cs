namespace Euterpe.Controls;

public sealed partial class RoleCard : TemplatedControl
{
    [GeneratedStyledProperty]
    public partial Geometry? Icon { get; set; }

    [GeneratedStyledProperty]
    public partial string? Title { get; set; }

    [GeneratedStyledProperty]
    public partial string? Description { get; set; }

    [GeneratedStyledProperty]
    public partial IBrush? AccentColor { get; set; }

    [GeneratedStyledProperty]
    public partial bool IsSelected { get; set; }
}
