namespace Euterpe.Tests.Desktop;

[Category("LanguageCodeMappingsTests")]
[TestSubject(typeof(LanguageCodeMappings))]
public sealed class LanguageCodeMappingsTest
{
    private static readonly (string LanguageCode, string CreditsLanguageCode)[] CreditsLanguageCodes =
    [
        ("en", "en"),
        ("de", "de"),
        ("es", "es"),
        ("fr", "fr"),
        ("hr", "hr"),
        ("hu", "hu"),
        ("id", "id"),
        ("ja", "ja"),
        ("ko", "ko"),
        ("nl", "nl"),
        ("pt", "pt-BR"),
        ("ru", "ru"),
        ("zh-Hans", "zh-CN"),
        ("zh-Hant", "zh-TW")
    ];

    [Test]
    public async Task ToCreditsLanguageCode_SupportedLanguage_ReturnsCreditsLanguageCode()
    {
        using var assertions = Assert.Multiple();
        foreach (var (languageCode, creditsLanguageCode) in CreditsLanguageCodes)
        {
            await Assert.That(LanguageCodeMappings.ToCreditsLanguageCode(languageCode))
                .IsEqualTo(creditsLanguageCode);
        }
    }

    [Test]
    public async Task ToSemiLanguageCode_SupportedLanguage_ReturnsSemiLanguageCode() =>
        await Assert.That(LanguageCodeMappings.ToSemiLanguageCode("zh-Hans")).IsEqualTo("zh-cn");
}
