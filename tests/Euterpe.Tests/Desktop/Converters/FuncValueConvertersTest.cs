using System.Globalization;
using Avalonia.Data.Converters;
using Euterpe.Converters;

namespace Euterpe.Tests.Desktop.Converters;

[Category("FuncValueConvertersTests")]
[TestSubject(typeof(FuncValueConverters))]
public sealed class FuncValueConvertersTest
{
    private static bool ConvertTileIsPlaying(object? playingKey, object? status, object? folderPath) =>
        (bool)((IMultiValueConverter)FuncValueConverters.TileIsPlaying)
        .Convert([playingKey, status, folderPath], typeof(bool), null, CultureInfo.InvariantCulture)!;

    [Test]
    public async Task TileIsPlaying_PlayingMatchingKey_ReturnsTrue() =>
        await Assert.That(ConvertTileIsPlaying("song", PlaybackStatus.Playing, "song")).IsTrue();

    [Test]
    public async Task TileIsPlaying_PausedMatchingKey_ReturnsFalse() =>
        await Assert.That(ConvertTileIsPlaying("song", PlaybackStatus.Paused, "song")).IsFalse();

    [Test]
    public async Task TileIsPlaying_PlayingDifferentKey_ReturnsFalse() =>
        await Assert.That(ConvertTileIsPlaying("song", PlaybackStatus.Playing, "other")).IsFalse();

    [Test]
    public async Task TileIsPlaying_NullKey_ReturnsFalse() =>
        await Assert.That(ConvertTileIsPlaying(null, PlaybackStatus.Idle, "song")).IsFalse();
}
