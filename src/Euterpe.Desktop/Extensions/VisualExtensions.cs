namespace Euterpe.Extensions;

public static class VisualExtensions
{
    public static TopLevel GetTopLevel(this Visual visual) =>
        TopLevel.GetTopLevel(visual) ?? throw new InvalidOperationException("TopLevel not found.");
}
