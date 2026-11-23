using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Desktop.Services;

[Category("LocalizationServiceTests")]
[TestSubject(typeof(LocalizationService))]
public sealed class LocalizationServiceTest
{
    [Test]
    [Arguments("zh-CN", "zh-Hans")]
    [Arguments("zh-TW", "zh-Hant")]
    [Arguments("en-US", "en")]
    public async Task GetCurrentLanguage_RegionalLanguageCode_ReturnsAndStoresSupportedLanguage(string languageCode, string supportedLanguageCode)
    {
        var config = new Config { LanguageCode = languageCode };
        var service = new LocalizationService { Config = config, Logger = Mock.Logger<LocalizationService>() };

        var language = service.GetCurrentLanguage();

        using var assertions = Assert.Multiple();
        await Assert.That(language.Name).IsEqualTo(supportedLanguageCode);
        await Assert.That(config.LanguageCode).IsEqualTo(supportedLanguageCode);
    }
}
