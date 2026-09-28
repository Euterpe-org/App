namespace Euterpe.Windows;

internal static class WindowsPaths
{
    internal static readonly string[] SteamSearch = new[]
        {
            @"Program Files\Steam",
            @"Program Files (x86)\Steam",
            @"Program Files\SteamLibrary",
            @"Program Files (x86)\SteamLibrary",
            @"Steam",
            @"SteamLibrary"
        }
        .SelectMany(path => Environment.GetLogicalDrives().Select(drive => Path.Combine(drive, path))).ToArray();
}
