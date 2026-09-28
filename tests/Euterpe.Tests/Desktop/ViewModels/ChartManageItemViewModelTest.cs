using Euterpe.Features.Charting;

namespace Euterpe.Tests.Desktop.ViewModels;

[Category("ChartManageItemViewModelTests")]
[TestSubject(typeof(ChartManageItemViewModel))]
public sealed class ChartManageItemViewModelTest
{
    [Test]
    public async Task IsSelected_Default_IsFalse()
    {
        var item = new ChartManageItemViewModel(CreateChart());

        await Assert.That(item.IsSelected).IsFalse();
    }

    private static ChartDto CreateChart() =>
        new()
        {
            FolderPath = "/charts/chart",
            FolderName = "chart",
            Source = ChartSource.Online,
            Manifest = new Manifest
            {
                Schema = Manifest.CurrentSchema,
                Cid = 13,
                Meta = new ManifestMeta
                {
                    Name = "Chart",
                    Author = "Author",
                    Scene = "Scene",
                    Maps = []
                },
                Files = []
            }
        };
}
