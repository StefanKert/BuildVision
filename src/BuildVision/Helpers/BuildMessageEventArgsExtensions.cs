using BuildVision.Tool.Building;
using Microsoft.Build.Framework;

namespace BuildVision.Helpers
{
    public static class BuildMessageEventArgsExtensions
    {
        public static bool IsUserMessage(this BuildMessageEventArgs message, BuildOutputLogger loggerSender)
        {
            return (message.Importance == MessageImportance.High && loggerSender.IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                    || (message.Importance == MessageImportance.Normal && loggerSender.IsVerbosityAtLeast(LoggerVerbosity.Normal))
                    || (message.Importance == MessageImportance.Low && loggerSender.IsVerbosityAtLeast(LoggerVerbosity.Detailed));
        }
    }
}
