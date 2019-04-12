using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace BuildVision.Common.Logging
{
    public static class LogManager
    {
#if DEBUG
        private static LogEventLevel DefaultLoggingLevel = LogEventLevel.Debug;
#else
        private static LogEventLevel DefaultLoggingLevel = LogEventLevel.Information;
#endif

        private static LoggingLevelSwitch LoggingLevelSwitch = new LoggingLevelSwitch(DefaultLoggingLevel);

        static Logger CreateLogger()
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ApplicationInfo.ApplicationName,
                "extension.log");

            const string outputTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{ProcessId:00000}] {Level:u4} [{ThreadId:00}] {ShortSourceContext,-25} {Message:lj}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                .WriteTo.File(logPath,
                    fileSizeLimitBytes: null,
                    outputTemplate: outputTemplate,
                    shared: true)
                .CreateLogger();
        }

        public static void EnableTraceLogging(bool enable)
        {
            var logEventLevel = enable ? LogEventLevel.Verbose : DefaultLoggingLevel;
            if (LoggingLevelSwitch.MinimumLevel != logEventLevel)
            {
                ForContext(typeof(LogManager)).Information("Set Logging Level: {LogEventLevel}", logEventLevel);
                LoggingLevelSwitch.MinimumLevel = logEventLevel;
            }
        }

        static Lazy<Logger> Logger { get; } = new Lazy<Logger>(CreateLogger);

        public static ILogger ForContext<T>() => ForContext(typeof(T));

        public static ILogger ForContext(Type type) => Logger.Value.ForContext(type).ForContext("ShortSourceContext", type.Name);
    }
}
