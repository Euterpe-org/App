using System.Globalization;
using Euterpe.Localization;
using Euterpe.Models.Progress;

namespace Euterpe.Tests.Desktop.ViewModels;

public sealed partial class ShareImportDialogViewModelTest
{
    [Test]
    public async Task ImportCommand_ValidPackage_DelegatesAndSummarizesTheOutcome()
    {
        var package = CreatePackage(GameId.MuseDash);
        BulkItemResult[] result = [new("13", BulkItemOutcome.Added)];
        var shareService = IGameShareService.Mock();
        shareService.TryParseShareLink("valid").Returns(package);
        shareService.ImportAsync(package, Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).Returns(result);
        var viewModel = CreateViewModel(shareService);
        await viewModel.PrepareAsync("valid");

        await viewModel.ImportCommand.ExecuteAsync(null);

        using var _ = Assert.Multiple();
        shareService.ImportAsync(package, Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).WasCalled(Times.Once);
        await Assert.That(viewModel.ImportCommand.IsRunning).IsFalse();
        await Assert.That(viewModel.IsStatusWarning).IsFalse();
        await Assert.That(viewModel.CanImport).IsFalse();
        await Assert.That(viewModel.StatusMessage)
            .IsEqualTo(string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_Result_Added, 1));
    }

    [Test]
    public async Task ImportCommand_MixedOutcome_DropsTheZeroCounts()
    {
        var package = CreatePackage(GameId.MuseDash);
        BulkItemResult[] result =
        [
            new("13", BulkItemOutcome.AlreadyPresent),
            new("14", BulkItemOutcome.AlreadyPresent),
            new("15", BulkItemOutcome.Added)
        ];
        var shareService = IGameShareService.Mock();
        shareService.TryParseShareLink("valid").Returns(package);
        shareService.ImportAsync(package, Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).Returns(result);
        var viewModel = CreateViewModel(shareService);
        await viewModel.PrepareAsync("valid");

        await viewModel.ImportCommand.ExecuteAsync(null);

        var expected = string.Join(" · ",
            string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_Result_Added, 1),
            string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_Result_Present, 2));

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.StatusMessage).IsEqualTo(expected);
        await Assert.That(viewModel.IsStatusWarning).IsFalse();
    }

    [Test]
    public async Task ImportCommand_SomeChartsFail_FlagsTheStatusAsAWarning()
    {
        var package = CreatePackage(GameId.MuseDash);
        BulkItemResult[] result = [new("13", BulkItemOutcome.Added), new("14", BulkItemOutcome.Failed)];
        var shareService = IGameShareService.Mock();
        shareService.TryParseShareLink("valid").Returns(package);
        shareService.ImportAsync(package, Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).Returns(result);
        var viewModel = CreateViewModel(shareService);
        await viewModel.PrepareAsync("valid");

        await viewModel.ImportCommand.ExecuteAsync(null);

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.IsStatusWarning).IsTrue();
        await Assert.That(viewModel.StatusMessage)
            .Contains(string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_Result_Failed, 1));
    }

    [Test]
    public async Task ImportCommand_Canceled_StopsImportAndWarns()
    {
        var package = CreatePackage(GameId.MuseDash);
        var viewModel = CreateViewModel(new CancelableGameShareService(package));
        await viewModel.PrepareAsync("valid");

        var importTask = viewModel.ImportCommand.ExecuteAsync(null);
        viewModel.CancelImport();
        await importTask;

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.ImportCommand.IsRunning).IsFalse();
        await Assert.That(viewModel.IsStatusWarning).IsTrue();
        await Assert.That(viewModel.StatusMessage).IsEqualTo(XAML.Share_Import_Canceled);
    }

    private sealed class CancelableGameShareService(GameSharePackage package) : IGameShareService
    {
        public string CreateChartShareLink(IReadOnlyCollection<int> chartIds) => throw new NotSupportedException();

        public GameSharePackage? TryParseShareLink(string text) => package;

        public async Task<IReadOnlyList<BulkItemResult>> ImportAsync(GameSharePackage value, IProgress<BatchProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
            return null!;
        }
    }
}
