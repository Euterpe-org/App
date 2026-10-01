namespace Euterpe.Headless.Tests.Services;

[TestSubject(typeof(ResourceService))]
public sealed class ResourceServiceTest : HeadlessTest
{
    private static ResourceService NewService() => new();

    [Test]
    public Task GetAssetAsStream_ExistingAsset_ReturnsReadableStream() => RunOnUI(async () =>
    {
        var service = NewService();

        await using var stream = service.GetAssetAsStream("Icon.png");

        using var _ = Assert.Multiple();
        await Assert.That(stream).IsNotNull();
        await Assert.That(stream.CanRead).IsTrue();
        await Assert.That(stream.Length).IsGreaterThan(0);
    });
}
