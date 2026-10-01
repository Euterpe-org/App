using System.Runtime.CompilerServices;
using DotNext.Threading;
using Euterpe.Features.Update;
using Euterpe.Proxies;
using Euterpe.Shell;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.Desktop.ViewModels;

[Category("MainSplashWindowViewModelTests")]
[TestSubject(typeof(MainSplashWindowViewModel))]
public sealed class MainSplashWindowViewModelTest
{
    [Test]
    public async Task OnInitializeAsync_RestoreSessionSucceeds_DoesNotCallLogin()
    {
        var auth = NewAuthService(true, out var loginCount);
        var vm = NewViewModel(auth);

        await vm.InitializeAsync();

        await Assert.That(loginCount.Value).IsEqualTo(0);
    }

    [Test]
    public async Task OnInitializeAsync_RestoreSessionFails_CallsLogin()
    {
        var auth = NewAuthService(false, out var loginCount);
        var vm = NewViewModel(auth);

        await vm.InitializeAsync();

        await Assert.That(loginCount.Value).IsEqualTo(1);
    }

    [Test]
    public async Task OnInitializeAsync_LoginFails_ClearsCopyFeedbackBeforeRetry()
    {
        var ready = new AsyncManualResetEvent(false);
        var auth = IAuthService.Mock();
        auth.Ready.Returns(ready);
        auth.RestoreSessionAsync().Returns(false);
        auth.IsServerHealthyAsync().Returns(true);
        MainSplashWindowViewModel? vm = null;
        var attempts = 0;
        auth.LoginAsync().Callback(() =>
        {
            vm!.CopyLoginLinkStatus = "Copied";
            if (++attempts is 2)
            {
                ready.Set();
            }
        });
        string? feedbackAtRetryPrompt = "not-cleared";
        var messageBox = IMessageBoxService.Mock();
        messageBox.WarningConfirmAsync(Any<string>())
            .Callback(_ => feedbackAtRetryPrompt = vm!.CopyLoginLinkStatus)
            .Returns(true);
        vm = NewViewModel(auth, messageBox);

        await vm.InitializeAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(feedbackAtRetryPrompt).IsNull();
        await Assert.That(attempts).IsEqualTo(2);
        await Assert.That(vm.CopyLoginLinkStatus).IsNull();
    }

    [Test]
    public async Task OnInitializeAsync_LoginFailsThenRetrySucceeds_PromptsAndRetries()
    {
        var ready = new AsyncManualResetEvent(false);
        var loginCount = new StrongBox<int>(0);
        var auth = IAuthService.Mock();
        auth.Ready.Returns(ready);
        auth.RestoreSessionAsync().Returns(false);
        auth.IsServerHealthyAsync().Returns(true);
        auth.LoginAsync().Callback(() =>
        {
            loginCount.Value++;
            if (loginCount.Value is 2)
            {
                ready.Set();
            }
        });

        var messageBox = IMessageBoxService.Mock();
        messageBox.WarningConfirmAsync(Any<string>()).Returns(true);
        var vm = NewViewModel(auth, messageBox);

        await vm.InitializeAsync();

        await Assert.That(loginCount.Value).IsEqualTo(2);
        messageBox.WarningConfirmAsync(Any<string>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task OnInitializeAsync_SignalsReady_AndRaisesRequestClose()
    {
        var auth = NewAuthService(true, out _);
        var vm = NewViewModel(auth);
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        await vm.InitializeAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(vm.Ready.IsSet).IsTrue();
        await Assert.That(closed).IsTrue();
    }

    [Test]
    public async Task Close_SubscriberRegistered_RaisesRequestClose()
    {
        var vm = NewViewModel(NewAuthService(true, out _));
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.Close();

        await Assert.That(closed).IsTrue();
    }

    private static IAuthService NewAuthService(bool restoreSucceeds, out StrongBox<int> loginCount)
    {
        var counter = new StrongBox<int>(0);
        loginCount = counter;
        var auth = IAuthService.Mock();
        auth.Ready.Returns(new AsyncManualResetEvent(true));
        auth.RestoreSessionAsync().Returns(restoreSucceeds);
        auth.IsServerHealthyAsync().Returns(true);
        auth.LoginAsync().Callback(() => counter.Value++);
        return auth;
    }

    private static MainSplashWindowViewModel NewViewModel(IAuthService authService, IMessageBoxService? messageBoxService = null)
    {
        var updateService = IUpdateService.Mock();
        updateService.CheckForUpdatesAsync().Returns((string?)null);

        return new MainSplashWindowViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            Logger = NullLogger<MainSplashWindowViewModel>.Instance,
            AuthService = authService,
            AuthState = new AuthState(),
            MessageBoxService = messageBoxService ?? IMessageBoxService.Mock(),
            UpdateDialogService = new UpdateDialogService
            {
                DialogService = IDialogService.Mock(),
                Logger = NullLogger<UpdateDialogService>.Instance,
                MessageBoxService = messageBoxService ?? IMessageBoxService.Mock(),
                UpdateService = updateService,
                UpdateDialogViewModelFactory = static version => new UpdateDialogViewModel(version)
            },
            UpdateService = updateService,
            TopLevel = new TopLevelProxy { Logger = NullLogger<TopLevelProxy>.Instance }
        };
    }
}
