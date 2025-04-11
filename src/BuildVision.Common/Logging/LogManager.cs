﻿using System;
using System.IO;
using BuildVision.Common.Diagnostics;
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

            try
            {
                return new LoggerConfiguration()
                    .Enrich.WithProcessId()
                    .Enrich.WithThreadId()
                    .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                    .WriteTo.File(logPath,
                        outputTemplate: outputTemplate,
                        shared: true,
                        fileSizeLimitBytes: 10 * 1024 * 1024,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 10,
                        rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }
            catch(Exception ex)
            {
                DiagnosticsClient.TrackException(ex);
                // In case of a failure we just don´t log to a file to make sure we at least can start BuildVision
                return new LoggerConfiguration()
                    .Enrich.WithProcessId()
                    .Enrich.WithThreadId()
                    .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                    .CreateLogger();
            }
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
