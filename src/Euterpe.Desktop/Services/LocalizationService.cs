using SemiTheme = Semi.Avalonia.SemiTheme;
using UrsaSemiTheme = Ursa.Themes.Semi.UrsaSemiTheme;

namespace Euterpe.Services;

public sealed class LocalizationService
{
    public Language[] AvailableLanguages { get; } =
    [
        "en",
        "de",
        "es",
        "fr",
        "hr",
        "hu",
        "id",
        "ja",
        "ko",
        "nl",
        "pt",
        "ru",
        "zh-Hans",
        "zh-Hant"
    ];

    public Language GetCurrentLanguage()
    {
        CultureInfo currentCulture;

        try
        {
            currentCulture = CultureInfo.GetCultureInfo(Config.LanguageCode);
        }
        catch (CultureNotFoundException ex)
        {
            currentCulture = CultureInfo.CurrentUICulture;
            Logger.LogError(ex, "Invalid language code {LanguageCode} from config, falling back to {CultureName}", Config.LanguageCode, currentCulture.EnglishName);
        }

        foreach (var cultureName in CreateCultureFallbackChain(currentCulture).Select(x => x.Name))
        {
            var language = AvailableLanguages.FirstOrDefault(x => x.Name == cultureName);
            if (language is null)
            {
                continue;
            }

            Config.LanguageCode = cultureName;
            return language;
        }

        Logger.LogError("No matching language found for {CultureName}, falling back to {Language}", currentCulture.Name, AvailableLanguages[0].Name);
        Config.LanguageCode = AvailableLanguages[0].Name;
        return AvailableLanguages[0];
    }

    public void SetLanguage(string language)
    {
        if (CultureInfo.CurrentUICulture.Name == language)
        {
            return;
        }

        var culture = CultureInfo.GetCultureInfo(language);
        LocalizationManager.Culture = culture;

        var semiCulture = CultureInfo.GetCultureInfo(LanguageCodeMappings.ToSemiLanguageCode(culture.Name));
        SemiTheme.OverrideLocaleResources(GetCurrentApplication(), semiCulture);
        UrsaSemiTheme.OverrideLocaleResources(GetCurrentApplication(), semiCulture);

        Config.LanguageCode = language;
        Logger.LogInformation("Language set to {Language}", language);
    }

    private static IEnumerable<CultureInfo> CreateCultureFallbackChain(CultureInfo startingCulture)
    {
        var current = startingCulture;
        while (current.Name != CultureInfo.InvariantCulture.Name)
        {
            yield return current;
            current = current.Parent;
        }

        yield return CultureInfo.InvariantCulture;
    }

    #region Injections

    public required Config Config { get; init; }
    public required ILogger<LocalizationService> Logger { get; init; }

    #endregion Injections
}
