using System.Text.Json;
using Euterpe.Contracts.Distribution;
using Euterpe.Core.JsonContexts;

namespace Euterpe.Tests.Core.JsonContexts;

[Category("SnakeCaseJsonContextTests")]
[TestSubject(typeof(SnakeCaseJsonContext))]
public sealed class SnakeCaseJsonContextTest
{
    [Test]
    public async Task DependencyMetadata_RoundTrip_UsesExactDotnetRuntimeVersionKey()
    {
        Dependency[] dependencies =
        [
            new()
            {
                Slug = "MelonLoader",
                Versions = new Dictionary<string, DistributionVersion<DependencyMetadata>>
                {
                    ["0.7.3"] = new()
                    {
                        Metadata = new DependencyMetadata { DotNetRuntimeVersion = "6.0" }
                    }
                }
            }
        ];

        var json = JsonSerializer.Serialize(dependencies, SnakeCaseJsonContext.Default.DependencyArray);
        var roundTripped = JsonSerializer.Deserialize(json, SnakeCaseJsonContext.Default.DependencyArray)!;

        using var _ = Assert.Multiple();
        await Assert.That(json).Contains("\"dotnet_runtime_version\":\"6.0\"");
        await Assert.That(json.Contains("\"dot_net_runtime_version\"", StringComparison.Ordinal)).IsFalse();
        await Assert.That(roundTripped.Single().Versions.Single().Value.Metadata.DotNetRuntimeVersion).IsEqualTo("6.0");
    }
}
