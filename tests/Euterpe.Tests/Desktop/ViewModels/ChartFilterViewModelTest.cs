using Euterpe.Features.Charting;

namespace Euterpe.Tests.Desktop.ViewModels;

[Category("ChartFilterViewModelTests")]
[TestSubject(typeof(ChartFilterViewModel))]
public sealed class ChartFilterViewModelTest
{
    [Test]
    public async Task Matches_SearchKeywordContainsSearchText_ReturnsTrue()
    {
        var viewModel = new ChartFilterViewModel { SearchText = "summer" };
        var chart = CreateChart(["Summer Event"]);

        await Assert.That(viewModel.Matches(chart)).IsTrue();
    }

    private static ChartDto CreateChart(string[]? searchKeywords) =>
        new()
        {
            FolderPath = "/charts/folder",
            FolderName = "folder",
            Source = ChartSource.Online,
            Manifest = new Manifest
            {
                Schema = Manifest.CurrentSchema,
                Meta = new ManifestMeta
                {
                    Name = "song",
                    Author = "author",
                    Scene = "scene",
                    Bpm = 120,
                    SearchKeywords = searchKeywords,
                    Maps = new Dictionary<string, ManifestMap>()
                },
                Files = new Dictionary<string, ManifestFileEntry>()
            }
        };
}
