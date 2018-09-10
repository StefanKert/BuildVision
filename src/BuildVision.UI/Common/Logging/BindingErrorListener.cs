using System.Diagnostics;

namespace BuildVision.UI.Common.Logging
{
    public class BindingErrorListener : TraceListener
    {
        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
            TraceManager.Trace("Binding error: " + message, EventLogEntryType.Warning);
        }
    }
}