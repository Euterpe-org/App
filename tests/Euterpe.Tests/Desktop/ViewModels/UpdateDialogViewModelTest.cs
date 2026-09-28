using Euterpe.Features.Update;

namespace Euterpe.Tests.Desktop.ViewModels;

[Category("UpdateDialogViewModelTests")]
[TestSubject(typeof(UpdateDialogViewModel))]
public sealed class UpdateDialogViewModelTest
{
    [Test]
    public async Task Constructor_Version_SetsInitialDisplayState()
    {
        var vm = new UpdateDialogViewModel("2.1.0-beta.1");

        using var assertions = Assert.Multiple();
        await Assert.That(vm.VersionDisplay).IsEqualTo("v2.1.0-beta.1");
        await Assert.That(vm.Progress).IsEqualTo(0);
        await Assert.That(vm.ProgressText).IsEqualTo("0%");
    }

    [Test]
    public async Task Report_Progress_UpdatesPercentage()
    {
        var vm = new UpdateDialogViewModel("2.1.0");

        vm.Report(42);

        using var assertions = Assert.Multiple();
        await Assert.That(vm.Progress).IsEqualTo(42);
        await Assert.That(vm.ProgressText).IsEqualTo("42%");
    }
}
