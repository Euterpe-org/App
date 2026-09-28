using Euterpe.Platform;

namespace Euterpe.Linux;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => Bootstrapper.Run<PlatformServices>(args);
}
