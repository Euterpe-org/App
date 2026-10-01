using Euterpe.Contracts.Charts;
using Euterpe.Core.Http.Clients;
using Euterpe.Tests.TestSupport;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("IEuterpeChartClientTests")]
[TestSubject(typeof(IEuterpeChartClient))]
public sealed class IEuterpeChartClientTest
{
    [Test]
    public async Task CheckChartUpdatesAsync_GoldenServerJson_PreservesCidDictionaryKeysVerbatim()
    {
        using var http = Mock.HttpHandler();
        http.OnPost("/api/charts/check-updates")
            .RespondWithJson("""{"charts":{"AbCdEf123":{"changed":["map1.bms"],"deleted":["video.mp4"]}}}""");
        var api = http.CreateEuterpeClient<IEuterpeChartClient>(Charts.BasePath);

        var response = await api.CheckChartUpdatesAsync(new CheckChartUpdatesRequest
        {
            Charts =
            {
                ["AbCdEf123"] = new Dictionary<string, ChartFileEntry>
                {
                    ["map1.bms"] = new()
                        { Version = 3 }
                }
            }
        });

        var delta = response.Charts["AbCdEf123"];
        using var assertions = Assert.Multiple();
        await Assert.That(delta.Changed)
            .IsEquivalentTo(["map1.bms"], StringComparer.Ordinal, CollectionOrdering.Matching);
        await Assert.That(delta.Deleted)
            .IsEquivalentTo(["video.mp4"], StringComparer.Ordinal, CollectionOrdering.Matching);
        await Assert.That(http.Requests[0].Body).Contains("\"AbCdEf123\"");
        await Assert.That(http.Requests[0].Body).Contains("\"version\":3");
    }
}
