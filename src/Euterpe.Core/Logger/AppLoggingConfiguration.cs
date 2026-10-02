using NLog.Config;
using NLog.Targets;

namespace Euterpe.Core.Logger;

internal static class AppLoggingConfiguration
{
#if DEBUG
    internal const LogLevel MinimumMicrosoftLogLevel = LogLevel.Debug;
    private static readonly NLog.LogLevel MinimumNLogLevel = NLog.LogLevel.Debug;
#else
    internal const LogLevel MinimumMicrosoftLogLevel = LogLevel.Information;
    private static readonly NLog.LogLevel MinimumNLogLevel = NLog.LogLevel.Info;
#endif

    internal static LoggingConfiguration Create(LiveLogTarget liveLogTarget)
    {
        var configuration = new LoggingConfiguration();
#if DEBUG
        var consoleTarget = new ConsoleTarget("console")
        {
            Layout = "[${level:format=TriLetter}](${logger}) ${message}${onexception:inner=${newline}${exception:format=tostring}}"
        };
        configuration.AddRule(MinimumNLogLevel, NLog.LogLevel.Fatal, consoleTarget);
#endif

        var fileTarget = new FileTarget("file")
        {
            FileName = LogFilePath,
            KeepFileOpen = false,
            MaxArchiveFiles = 30,
            Layout = "[${date:format=HH\\:mm\\:ss.fff zzz}] [${level}] (${logger})${newline}${message}${onexception:inner=${newline}${exception:format=tostring}}"
        };

        configuration.AddRule(MinimumNLogLevel, NLog.LogLevel.Fatal, fileTarget);
        configuration.AddRule(MinimumNLogLevel, NLog.LogLevel.Fatal, liveLogTarget);
        return configuration;
    }
}
