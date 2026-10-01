using System.Collections;
using Euterpe.Core.Logger;
using NLog;
using NLog.Config;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Euterpe.Tests.Desktop.Services;

[Category("LiveLogServiceTests")]
[TestSubject(typeof(LiveLogService))]
public sealed class LiveLogServiceTest
{
    private static List<LogMessage> MaterializeView(IEnumerable view)
    {
        var list = new List<LogMessage>();
        foreach (LogMessage item in view)
        {
            list.Add(item);
        }

        return list;
    }

    private static (LiveLogService Service, LogFactory Factory) CreateWiredService()
    {
        var target = new LiveLogTarget();
        var service = new LiveLogService(target);
        var logFactory = new LogFactory();
        var configuration = new LoggingConfiguration(logFactory);
        configuration.AddRule(LogLevel.Trace, LogLevel.Fatal, target);
        logFactory.Configuration = configuration;
        return (service, logFactory);
    }

    [Test]
    public async Task LogMessagesView_MessageReceived_AppendsMessage()
    {
        var (service, factory) = CreateWiredService();
        using (factory)
        {
            factory.GetLogger("Euterpe.Tests").Info("hello world");
        }

        var view = MaterializeView(service.LogMessagesView);
        using var _ = Assert.Multiple();
        await Assert.That(view).HasSingleItem();
        await Assert.That(view[0].Message).IsEqualTo("hello world");
        await Assert.That(view[0].LogLevel).IsEqualTo(MicrosoftLogLevel.Information);
    }

    [Test]
    public async Task TargetEvent_FilteredCategory_DoesNotAppendToView()
    {
        var (service, factory) = CreateWiredService();
        using (factory)
        {
            factory.GetLogger("Euterpe.Services.NavigationService").Info("routed");
        }

        await Assert.That(service.LogMessagesView).IsEmpty();
    }
}
