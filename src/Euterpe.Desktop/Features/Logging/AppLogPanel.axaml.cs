namespace Euterpe.Features.Logging;

public sealed partial class AppLogPanel : UserControl
{
    public AppLogPanel()
    {
        InitializeComponent();
        LogListBox.Loaded += (_, _) => LogListBox.ScrollIntoView(LogListBox.ItemCount - 1);
    }
}
