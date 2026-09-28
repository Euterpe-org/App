using Avalonia.Styling;
using Euterpe.Shared.Collections;

namespace Euterpe;

public static class AvaloniaResources
{
    public static readonly FrozenBiDictionary<string, ThemeVariant> ThemeVariants = new BiDictionary<string, ThemeVariant>
    {
        ["Light"] = ThemeVariant.Light,
        ["Dark"] = ThemeVariant.Dark
    }.ToFrozenBiDictionary();
}
