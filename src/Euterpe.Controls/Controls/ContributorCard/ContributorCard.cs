using System.Windows.Input;
using Euterpe.Controls.Models;

namespace Euterpe.Controls;

public sealed partial class ContributorCard : TemplatedControl
{
    [GeneratedStyledProperty]
    public partial string AvatarUrl { get; set; }

    [GeneratedStyledProperty]
    public partial string ContributorName { get; set; }

    [GeneratedStyledProperty]
    public partial string? ContributorDescription { get; set; }

    [GeneratedStyledProperty]
    public partial IEnumerable<ContributorLink>? Links { get; set; }

    [GeneratedStyledProperty]
    public partial ICommand ButtonCommand { get; set; }
}
