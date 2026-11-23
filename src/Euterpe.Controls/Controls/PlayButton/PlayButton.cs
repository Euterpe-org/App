using System.Windows.Input;

namespace Euterpe.Controls;

public sealed partial class PlayButton : SelectingItemsControl
{
    [GeneratedStyledProperty]
    public partial ICommand Command { get; set; }

    [Content]
    [GeneratedStyledProperty]
    public partial string Content { get; set; }
}
