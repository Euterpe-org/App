using Euterpe.Platform;

namespace Euterpe.Windows;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => Bootstrapper.Run<PlatformServices>(args);
}
