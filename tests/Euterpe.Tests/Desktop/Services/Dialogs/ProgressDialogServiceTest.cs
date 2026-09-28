using Euterpe.Features.Charting;
using Ursa.Controls;

namespace Euterpe.Tests.Desktop.Services;

[Category("ProgressDialogServiceTests")]
[TestSubject(typeof(ProgressDialogService))]
public sealed class ProgressDialogServiceTest
{
    [Test]
    public async Task ExecuteAsync_ResultOperation_ReturnsResultAndClosesDialog()
    {
        var dialog = IDialogService.Mock();
        var closed = false;
        OverlayDialogOptions? capturedOptions = null;
        dialog.ShowOverlayAsync<ProgressDialog, ProgressDialogViewModel>(
                Any<ProgressDialogViewModel>(), Any<OverlayDialogOptions?>(), Any<string?>(), Any<CancellationToken?>())
            .Callback((viewModel, options, _, _) =>
            {
                capturedOptions = options;
                viewModel.RequestClose += (_, _) => closed = true;
            });
        var service = CreateService(dialog);

        var result = await service.ExecuteAsync("Title", "Hint", true, _ => Task.FromResult(42));

        using var assertions = Assert.Multiple();
        await Assert.That(result).IsEqualTo(42);
        await Assert.That(capturedOptions).IsNotNull();
        await Assert.That(capturedOptions!.IsCloseButtonVisible).IsFalse();
        await Assert.That(service.ProgressDialogViewModel.IsIndeterminate).IsTrue();
        await Assert.That(closed).IsTrue();
    }

    [Test]
    public async Task ExecuteAsync_TaskOperation_ExecutesOperation()
    {
        var executed = false;
        var service = CreateService();

        await service.ExecuteAsync("Title", "Hint", false, _ =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        await Assert.That(executed).IsTrue();
    }

    private static ProgressDialogService CreateService(IDialogService? dialog = null) =>
        new()
        {
            DialogService = dialog ?? IDialogService.Mock(),
            ProgressDialogViewModel = new ProgressDialogViewModel { Launcher = IPlatformLauncher.Mock() }
        };
}
