namespace Euterpe.Linux;

internal static class LinuxPaths
{
    internal static readonly string[] SteamSearch = new[]
        {
            ".local/share/Steam",
            ".steam/steam",
            ".steam/root",
            ".var/app/com.valvesoftware.Steam/data/Steam"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToArray();
}
