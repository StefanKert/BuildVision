using System;
using Serilog;

namespace BuildVision.Common.Logging
{
    public static class Log
    {
        private static Lazy<ILogger> Logger { get; } = new Lazy<ILogger>(() => LogManager.ForContext(typeof(Log)));

        public static void Assert(bool condition, string messageTemplate)
            => Logger.Value.Assert(condition, messageTemplate);
    }
}
