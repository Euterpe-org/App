using System.Collections.Frozen;

namespace Euterpe;

public static class LanguageCodeMappings
{
    private static readonly FrozenDictionary<string, string> CreditsLanguageCodes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["en"] = "en",
            ["de"] = "de",
            ["es"] = "es",
            ["fr"] = "fr",
            ["hr"] = "hr",
            ["hu"] = "hu",
            ["id"] = "id",
            ["ja"] = "ja",
            ["ko"] = "ko",
            ["nl"] = "nl",
            ["pt"] = "pt-BR",
            ["ru"] = "ru",
            ["zh-Hans"] = "zh-CN",
            ["zh-Hant"] = "zh-TW"
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, string> SemiLanguageCodes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["en"] = "en-us",
            ["de"] = "de-de",
            ["es"] = "es-es",
            ["fr"] = "fr-fr",
            ["hr"] = "hr",
            ["hu"] = "hu",
            ["id"] = "en-us",
            ["ja"] = "ja-jp",
            ["ko"] = "ko-kr",
            ["nl"] = "nl-nl",
            ["pt"] = "en-us",
            ["ru"] = "ru-ru",
            ["zh-Hans"] = "zh-cn",
            ["zh-Hant"] = "zh-tw"
        }.ToFrozenDictionary();

    public static string ToCreditsLanguageCode(string languageCode) => CreditsLanguageCodes[languageCode];

    public static string ToSemiLanguageCode(string languageCode) => SemiLanguageCodes[languageCode];
}
