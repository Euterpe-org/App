namespace Euterpe.Controls;

public sealed partial class LabeledSection : TemplatedControl
{
    [GeneratedStyledProperty]
    public partial string Title { get; set; }

    [GeneratedStyledProperty]
    public partial string Description { get; set; }

    [Content]
    [GeneratedStyledProperty]
    public partial object Content { get; set; }
}
