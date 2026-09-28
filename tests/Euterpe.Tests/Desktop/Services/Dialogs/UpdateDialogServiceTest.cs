using Euterpe.Features.Update;
using Ursa.Controls;

namespace Euterpe.Tests.Desktop.Services;

[Category("UpdateDialogServiceTests")]
[TestSubject(typeof(UpdateDialogService))]
public sealed class UpdateDialogServiceTest
{
    [Test]
    public async Task ShowAsync_Version_ShowsUncloseableDialogAndStartsUpdate()
    {
        var dialog = IDialogService.Mock();
        OverlayDialogOptions? capturedOptions = null;
        UpdateDialogViewModel? capturedViewModel = null;
        var closed = false;
        dialog.ShowOverlayAsync<UpdateDialog, UpdateDialogViewModel>(
                Any<UpdateDialogViewModel>(), Any<OverlayDialogOptions?>(), Any<string?>(), Any<CancellationToken?>())
            .Callback((vm, options, _, _) =>
            {
                capturedViewModel = vm;
                capturedOptions = options;
                vm.RequestClose += (_, _) => closed = true;
            });
        var update = IUpdateService.Mock();
        IProgress<int>? capturedProgress = null;
        update.UpdateAsync(Any<IProgress<int>>()).Callback(progress => capturedProgress = progress);
        var service = new UpdateDialogService
        {
            DialogService = dialog,
            Logger = Mock.Logger<UpdateDialogService>(),
            MessageBoxService = IMessageBoxService.Mock(),
            UpdateService = update,
            UpdateDialogViewModelFactory = static version => new UpdateDialogViewModel(version)
        };

        var result = await service.ShowAsync("2.1.0", "host");

        using var assertions = Assert.Multiple();
        await Assert.That(result).IsTrue();
        await Assert.That(capturedViewModel).IsNotNull();
        await Assert.That(capturedViewModel!.VersionDisplay).IsEqualTo("v2.1.0");
        await Assert.That(closed).IsTrue();
        await Assert.That(capturedProgress).IsNotNull();
        await Assert.That(capturedOptions).IsNotNull();
        await Assert.That(capturedOptions!.CanDragMove).IsFalse();
        await Assert.That(capturedOptions.CanLightDismiss).IsFalse();
        await Assert.That(capturedOptions.CanResize).IsFalse();
        await Assert.That(capturedOptions.IsCloseButtonVisible).IsFalse();
        update.UpdateAsync(Any<IProgress<int>>()).WasCalled(Times.Once);
        dialog.ShowOverlayAsync<UpdateDialog, UpdateDialogViewModel>(
                capturedViewModel, Any<OverlayDialogOptions?>(), "host", Any<CancellationToken?>())
            .WasCalled(Times.Once);
    }

    [Test]
    public async Task ShowAsync_UpdateFails_ClosesDialogAndReturnsFalse()
    {
        var dialog = IDialogService.Mock();
        var closed = false;
        dialog.ShowOverlayAsync<UpdateDialog, UpdateDialogViewModel>(
                Any<UpdateDialogViewModel>(), Any<OverlayDialogOptions?>(), Any<string?>(), Any<CancellationToken?>())
            .Callback((vm, _, _, _) => vm.RequestClose += (_, _) => closed = true);
        var update = IUpdateService.Mock();
        update.UpdateAsync(Any<IProgress<int>>()).Throws<InvalidOperationException>();
        var messageBox = IMessageBoxService.Mock();
        var service = new UpdateDialogService
        {
            DialogService = dialog,
            Logger = Mock.Logger<UpdateDialogService>(),
            MessageBoxService = messageBox,
            UpdateService = update,
            UpdateDialogViewModelFactory = static version => new UpdateDialogViewModel(version)
        };

        var result = await service.ShowAsync("2.1.0", "host");

        await Assert.That(result).IsFalse();
        await Assert.That(closed).IsTrue();
        messageBox.ErrorAsync(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
    }
}
