using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Euterpe.Abstractions;
using Euterpe.Features.Update;
using Euterpe.Localization;
using Euterpe.Models.Auth;
using Euterpe.Proxies;
using Euterpe.Shell;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Headless.Tests.Views;

[Category("MainSplashWindowTests")]
[TestSubject(typeof(MainSplashWindow))]
public sealed class MainSplashWindowTest : HeadlessTest
{
    [Test]
    public Task AuthState_BackgroundChange_UpdatesLoginVisibility() => RunOnUI(async () =>
    {
        var vm = NewViewModel();
        var window = new MainSplashWindow { DataContext = vm, MainWindowFactory = () => new MainWindow() };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var button = window.GetVisualDescendants().OfType<Button>().Single(b => b.Command == vm.CopyLoginLinkCommand);

            await Task.Run(() => vm.AuthState.AuthorizeUrl = "https://euterpe-org.com/auth/app?state=background")
                .WaitAsync(TimeSpan.FromSeconds(5));
            Dispatcher.UIThread.RunJobs();
            await Assert.That(button.IsEffectivelyVisible).IsTrue();

            await Task.Run(() => vm.AuthState.AuthorizeUrl = null).WaitAsync(TimeSpan.FromSeconds(5));
            Dispatcher.UIThread.RunJobs();
            await Assert.That(button.IsEffectivelyVisible).IsFalse();
        }
        finally
        {
            window.Hide();
        }
    });

    [Test]
    public Task CopyLoginLink_WaitingForAuthorization_CopiesCurrentLinkAndShowsInlineFeedback() => RunOnUI(async () =>
    {
        const string url = "https://euterpe-org.com/auth/app?state=current-attempt&code_challenge=challenge";
        var vm = NewViewModel();
        var window = new MainSplashWindow { DataContext = vm, MainWindowFactory = () => new MainWindow() };
        var desktop = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        var previousMainWindow = desktop.MainWindow;
        desktop.MainWindow = window;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var button = window.GetVisualDescendants().OfType<Button>().Single(b => b.Command == vm.CopyLoginLinkCommand);
            await Assert.That(button.IsEffectivelyVisible).IsFalse();

            vm.AuthState.AuthorizeUrl = url;
            Dispatcher.UIThread.RunJobs();
            await Assert.That(button.IsEffectivelyVisible).IsTrue();
            var position = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
            window.MouseMove(position);
            window.MouseDown(position, MouseButton.Left);
            window.MouseUp(position, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            await vm.CopyLoginLinkCommand.ExecutionTask!;

            using var assertions = Assert.Multiple();
            await Assert.That(await window.Clipboard!.TryGetTextAsync()).IsEqualTo(url);
            await Assert.That(vm.CopyLoginLinkStatus).IsEqualTo(XAML.Splash_CopyLoginLink_Success);
            await Assert.That(window.GetVisualDescendants().OfType<TextBlock>()
                .Any(t => t.IsEffectivelyVisible && t.Text == XAML.Splash_CopyLoginLink_Success)).IsTrue();

            vm.AuthState.AuthorizeUrl = null;
            Dispatcher.UIThread.RunJobs();
            await Assert.That(button.IsEffectivelyVisible).IsFalse();
        }
        finally
        {
            window.Hide();
            desktop.MainWindow = previousMainWindow;
        }
    });

    private static MainSplashWindowViewModel NewViewModel()
    {
        var messageBox = IMessageBoxService.Mock();
        var updateService = IUpdateService.Mock();
        return new MainSplashWindowViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            AuthService = IAuthService.Mock(),
            AuthState = new AuthState(),
            Logger = NullLogger<MainSplashWindowViewModel>.Instance,
            MessageBoxService = messageBox,
            TopLevel = new TopLevelProxy { Logger = NullLogger<TopLevelProxy>.Instance },
            UpdateService = updateService,
            UpdateDialogService = new UpdateDialogService
            {
                DialogService = IDialogService.Mock(),
                Logger = NullLogger<UpdateDialogService>.Instance,
                MessageBoxService = messageBox,
                UpdateService = updateService,
                UpdateDialogViewModelFactory = static version => new UpdateDialogViewModel(version)
            }
        };
    }
}
