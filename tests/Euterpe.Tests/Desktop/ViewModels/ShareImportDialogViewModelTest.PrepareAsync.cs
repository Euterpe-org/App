using System.Globalization;
using Euterpe.Localization;

namespace Euterpe.Tests.Desktop.ViewModels;

public sealed partial class ShareImportDialogViewModelTest
{
    [Test]
    public async Task PrepareAsync_ValidPackage_EnablesImportAndCountsTheCharts()
    {
        var package = CreatePackage(GameId.MuseDash);
        var shareService = IGameShareService.Mock();
        shareService.TryParseShareLink("valid").Returns(package);
        var viewModel = CreateViewModel(shareService);

        await viewModel.PrepareAsync("valid");

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.CanImport).IsTrue();
        await Assert.That(viewModel.StatusMessage).Contains(package.ChartIds.Length.ToString(CultureInfo.CurrentCulture));
    }

    [Test]
    public async Task PrepareAsync_PackageForAnotherGame_DisablesImport()
    {
        var package = CreatePackage(GameId.MuseDash2);
        var shareService = IGameShareService.Mock();
        shareService.TryParseShareLink("wrong-game").Returns(package);
        var viewModel = CreateViewModel(shareService);

        await viewModel.PrepareAsync("wrong-game");

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.CanImport).IsFalse();
        await Assert.That(viewModel.StatusMessage)
            .Contains(viewModel.Config.Games.First(game => game.Id == GameId.MuseDash2).DisplayName);
    }

    [Test]
    public async Task PrepareAsync_UnparsableText_DisablesImportAndSaysSo()
    {
        var shareService = IGameShareService.Mock();
        shareService.TryParseShareLink(Any<string>()).Returns((GameSharePackage?)null);
        var viewModel = CreateViewModel(shareService);

        await viewModel.PrepareAsync("not-a-share-code");

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.CanImport).IsFalse();
        await Assert.That(viewModel.StatusMessage).IsEqualTo(XAML.Share_Import_Invalid);
    }

    [Test]
    public async Task PrepareAsync_BlankText_DisablesImportAndReportsAnEmptyClipboard()
    {
        var shareService = IGameShareService.Mock();
        var viewModel = CreateViewModel(shareService);

        await viewModel.PrepareAsync("   ");

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.CanImport).IsFalse();
        await Assert.That(viewModel.StatusMessage).IsEqualTo(XAML.Share_Import_Empty);
    }
}
