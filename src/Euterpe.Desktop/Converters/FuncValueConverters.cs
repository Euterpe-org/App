using Avalonia.Layout;
using Avalonia.Media;

namespace Euterpe.Converters;

public static class FuncValueConverters
{
    public static FuncValueConverter<string, StreamGeometry?> SemiIconConverter { get; } =
        new(key => GetCurrentApplication().TryGetResource($"SemiIcon{key}", out var result) ? result as StreamGeometry : null);

    public static FuncValueConverter<int, HorizontalAlignment> HorizontalAlignmentConverter { get; } =
        new(count => count is 1 ? HorizontalAlignment.Center : HorizontalAlignment.Left);

    public static FuncValueConverter<string?, IBrush?> HexColorToBrushConverter { get; } =
        new(hex => Color.TryParse(hex, out var color) ? new SolidColorBrush(color) : null);

    // [PlaybackState.PlayingKey, PlaybackState.Status, ChartDto.FolderPath]
    public static FuncMultiValueConverter<object?, bool> TileIsPlaying { get; } =
        new(static values => values.ToArray() is [string playingKey, PlaybackStatus.Playing, string folderPath]
                             && playingKey == folderPath);
}
