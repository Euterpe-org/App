using Avalonia.Input;
using Avalonia.Interactivity;

namespace Euterpe.Features.Home;

public sealed partial class HomePage : UserControl
{
    private const int EasterEggClickThreshold = 10;
    private int _titleClickCount;

    public HomePage()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        SupportCard.IsVisible = false;
    }

    private void TitleButton_Click(object? sender, RoutedEventArgs e)
    {
        _titleClickCount++;

        if (_titleClickCount < EasterEggClickThreshold)
        {
            return;
        }

        _titleClickCount = 0;
        MadelineOverlay.IsVisible = true;
    }

    private void MadelineOverlay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MadelineOverlay.IsVisible = false;
    }
}
