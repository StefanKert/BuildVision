using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

using Microsoft.VisualStudio.Shell;

namespace BuildVision.UI.Common.Logging
{
    public static class TraceManager
    {
#if DEBUG
        static TraceManager()
        {
            string logDirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                Resources.ProductName, 
                "log");
            string logFilePath = Path.Combine(
                logDirectoryPath, 
                string.Format("log_{0}.svclog", Guid.NewGuid()));

            if (!Directory.Exists(logDirectoryPath))
                Directory.CreateDirectory(logDirectoryPath);

            var traceListener = new XmlWriterTraceListener(logFilePath)
            {
                TraceOutputOptions = TraceOptions.DateTime
            };

            System.Diagnostics.Trace.AutoFlush = true;
            System.Diagnostics.Trace.Listeners.Add(traceListener);

            // Tracing binding errors
            PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingErrorListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
        }
#endif

        public static void TraceUnknownException(this Exception ex)
        {
            Trace(ex, Resources.UnknownExceptionMsg);
        }

        public static void TraceError(string message)
        {
            Trace(message, EventLogEntryType.Error);
        }

        public static void Trace(this Exception ex, string message, EventLogEntryType type = EventLogEntryType.Error)
        {
            TraceAction(ex.ToLogString(message), type);
        }

        public static void Trace(string message, EventLogEntryType type)
        {
            TraceAction(message, type);
        }

        private static void TraceAction(string message, EventLogEntryType type)
        {
            // ActivityLog works if devenv.exe started with /log switch.
            // Read more https://msdn.microsoft.com/en-us/library/ms241272.aspx.
            switch (type)
            {
                case EventLogEntryType.Error:
                    ActivityLog.LogError(Resources.ProductName, message);
#if DEBUG
                    System.Diagnostics.Trace.TraceError(message);
                    MessageBox.Show(message, Resources.ProductName + " error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                    break;

                case EventLogEntryType.Warning:
                case EventLogEntryType.FailureAudit:
                    ActivityLog.LogWarning(Resources.ProductName, message);
#if DEBUG
                    System.Diagnostics.Trace.TraceWarning(message);
#endif
                    break;

                case EventLogEntryType.Information:
                case EventLogEntryType.SuccessAudit:
                    ActivityLog.LogInformation(Resources.ProductName, message);
#if DEBUG
                    System.Diagnostics.Trace.TraceInformation(message);
#endif
                    break;

                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        /// <summary>
        /// <para>Creates a log-string from the Exception.</para>
        /// <para>The result includes the stacktrace, innerexception et cetera, separated by <seealso cref="Environment.NewLine"/>.</para>
        /// </summary>
        /// <param name="ex">The exception to create the string from.</param>
        /// <param name="additionalMessage">Additional message to place at the top of the string, maybe be empty or null.</param>
        /// <returns></returns>
        public static string ToLogString(this Exception ex, string additionalMessage)
        {
            var msg = new StringBuilder();

            if (!string.IsNullOrEmpty(additionalMessage))
            {
                msg.Append(additionalMessage);
                msg.Append(Environment.NewLine);
            }

            if (ex != null)
            {
                Exception orgEx = ex;

                msg.Append("Exception:");
                msg.Append(Environment.NewLine);
                while (orgEx != null)
                {
                    msg.Append(orgEx.Message);
                    msg.Append(Environment.NewLine);
                    orgEx = orgEx.InnerException;
                }

                if (ex.Data != null && ex.Data.Count > 0)
                {
                    msg.AppendLine("Data:");
                    foreach (System.Collections.DictionaryEntry dataEntry in ex.Data)
                    {
                        msg.AppendFormat("Key='{0}';Value='{1}'", dataEntry.Key, dataEntry.Value);
                        msg.Append(Environment.NewLine);
                    }
                }

                if (ex.StackTrace != null)
                {
                    msg.Append("StackTrace:");
                    msg.Append(Environment.NewLine);
                    msg.Append(ex.StackTrace.ToString());
                    msg.Append(Environment.NewLine);
                }

                if (ex.Source != null)
                {
                    msg.Append("Source:");
                    msg.Append(Environment.NewLine);
                    msg.Append(ex.Source);
                    msg.Append(Environment.NewLine);
                }

                if (ex.TargetSite != null)
                {
                    msg.Append("TargetSite:");
                    msg.Append(Environment.NewLine);
                    msg.Append(ex.TargetSite.ToString());
                    msg.Append(Environment.NewLine);
                }

                Exception baseException = ex.GetBaseException();
                if (baseException != null)
                {
                    msg.Append("BaseException:");
                    msg.Append(Environment.NewLine);
                    msg.Append(ex.GetBaseException());
                }
            }

            return msg.ToString();
        }
    }
}