using Microsoft.Win32;

namespace Euterpe.Windows;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsSystemAssociationSetup : ISystemAssociationSetup
{
    #region Injections

    public required ILogger<WindowsSystemAssociationSetup> Logger { get; init; }

    #endregion Injections

    public Task RegisterAsync(string processPath)
    {
        try
        {
            RegisterDeepLinkScheme(processPath);
            RegisterEpkFileAssociation(processPath);

            Logger.LogInformation("Registered deep link protocol and file association on Windows with process path: {ProcessPath}", processPath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to register deep link protocol and file association on Windows");
        }

        return Task.CompletedTask;
    }

    private static void RegisterDeepLinkScheme(string processPath)
    {
        using var schemeKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ISystemAssociationSetup.DeepLinkScheme}");
        schemeKey.SetValue(string.Empty, $"URL:{AppName} Protocol", RegistryValueKind.String);
        schemeKey.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);

        using var commandKey = schemeKey.CreateSubKey(@"shell\open\command");
        commandKey.SetValue(string.Empty, $"\"{processPath}\" \"%1\"", RegistryValueKind.String);

        using var iconKey = schemeKey.CreateSubKey("DefaultIcon");
        iconKey.SetValue(string.Empty, $"\"{processPath}\",0", RegistryValueKind.String);
    }

    private static void RegisterEpkFileAssociation(string processPath)
    {
        const string progId = $"{AppName}{ChartFiles.ManifestExtension}";

        using var extensionKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ChartFiles.ManifestExtension}");
        extensionKey.SetValue(string.Empty, progId, RegistryValueKind.String);

        using var progIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}");
        progIdKey.SetValue(string.Empty, $"{AppName} Manifest Pack", RegistryValueKind.String);

        using var iconKey = progIdKey.CreateSubKey("DefaultIcon");
        iconKey.SetValue(string.Empty, $"\"{processPath}\",0", RegistryValueKind.String);

        using var commandKey = progIdKey.CreateSubKey(@"shell\open\command");
        commandKey.SetValue(string.Empty, $"\"{processPath}\" \"%1\"", RegistryValueKind.String);
    }
}
