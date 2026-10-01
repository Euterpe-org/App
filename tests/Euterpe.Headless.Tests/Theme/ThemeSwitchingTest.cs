using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace Euterpe.Headless.Tests.Theme;

[NotInParallel("ApplicationThemeVariant")]
[Category("ThemeSwitchingTests")]
public sealed class ThemeSwitchingTest : HeadlessTest
{
    [Test]
    public Task FindResource_DarkVsLight_HomeBackgroundIsOneBitmapDressedPerTheme() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var bitmap = window.FindResource(ThemeVariant.Dark, "BackgroundImage");

            using var _ = Assert.Multiple();
            await Assert.That(bitmap).IsTypeOf<Bitmap>();
            await Assert.That(window.FindResource(ThemeVariant.Light, "BackgroundImage")).IsSameReferenceAs(bitmap);
            await Assert.That(window.FindResource(ThemeVariant.Dark, "HomeBackgroundOpacity"))
                .IsNotEqualTo(window.FindResource(ThemeVariant.Light, "HomeBackgroundOpacity"));
        }
        finally
        {
            window.Close();
        }
    });

    [Test]
    public Task FindResource_HomeBackgroundMask_FadesOutAtBothEnds() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var mask = (LinearGradientBrush)window.FindResource(ThemeVariant.Dark, "HomeBackgroundMask")!;

            using var _ = Assert.Multiple();
            await Assert.That(mask.GradientStops[0].Color.A).IsLessThan<byte>(32);
            await Assert.That(mask.GradientStops[^1].Color.A).IsLessThan<byte>(32);
            await Assert.That(mask.GradientStops.Select(stop => stop.Color.A).Max()).IsEqualTo<byte>(255);
        }
        finally
        {
            window.Close();
        }
    });

    [Test]
    public Task FindResource_DarkVsLight_TitleEdgePaintsOnLightOnly() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var dark = (DropShadowEffect)window.FindResource(ThemeVariant.Dark, "HomeTitleEffect")!;
            var light = (DropShadowEffect)window.FindResource(ThemeVariant.Light, "HomeTitleEffect")!;

            using var _ = Assert.Multiple();
            await Assert.That(dark.Color.A).IsEqualTo<byte>(0);
            await Assert.That(light.Color.A).IsEqualTo<byte>(255);
        }
        finally
        {
            window.Close();
        }
    });

    [Test]
    public Task FindResource_DarkVsLight_PrimaryTitleColorsDiffer() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var darkColor = window.FindResource(ThemeVariant.Dark, "PrimaryTitleColor");
            var lightColor = window.FindResource(ThemeVariant.Light, "PrimaryTitleColor");

            using var _ = Assert.Multiple();
            await Assert.That(darkColor).IsTypeOf<Color>();
            await Assert.That(lightColor).IsTypeOf<Color>();
            await Assert.That(darkColor).IsNotEqualTo(lightColor);
        }
        finally
        {
            window.Close();
        }
    });

    private static Window NewWindow()
    {
        var window = new Window { Width = 100, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
