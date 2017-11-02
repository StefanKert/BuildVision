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
            System.Threading.Tasks.Task.Run(() =>
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
                        throw new ArgumentOutOfRangeException(nameof(type));
                }
            });
        }

        public static string ToLogString(this Exception ex, string additionalMessage)
        {
            var msg = new StringBuilder();
            if (!string.IsNullOrEmpty(additionalMessage))
                msg.AppendLine(additionalMessage);
    
            if (ex == null)
                return msg.ToString();

            Exception orgEx = ex;
            msg.AppendLine("Exception:");
            while (orgEx != null)
            {
                msg.AppendLine(orgEx.Message);
                orgEx = orgEx.InnerException;
            }

            AddDataEntries(ex, msg);
            AddStackTrace(ex, msg);
            AddSource(ex, msg);
            AddTargetSite(ex, msg);
            AddBaseExcception(ex, msg);
            return msg.ToString();
        }

        private static void AddBaseExcception(Exception ex, StringBuilder msg)
        {
            Exception baseException = ex.GetBaseException();
            if (baseException != null)
            {
                msg.AppendLine("BaseException:");
                msg.Append(ex.GetBaseException());
            }
        }

        private static void AddTargetSite(Exception ex, StringBuilder msg)
        {
            if (ex.TargetSite != null)
            {
                msg.AppendLine("TargetSite:");
                msg.AppendLine(ex.TargetSite.ToString());
            }
        }

        private static void AddSource(Exception ex, StringBuilder msg)
        {
            if (ex.Source != null)
            {
                msg.AppendLine("Source:");
                msg.AppendLine(ex.Source.ToString());
            }
        }

        private static void AddStackTrace(Exception ex, StringBuilder msg)
        {
            if (ex.StackTrace != null)
            {
                msg.AppendLine("StackTrace:");
                msg.AppendLine(ex.StackTrace);
            }
        }

        private static void AddDataEntries(Exception ex, StringBuilder msg)
        {
            if (ex.Data != null && ex.Data.Count > 0)
            {
                msg.AppendLine("Data:");
                foreach (System.Collections.DictionaryEntry dataEntry in ex.Data)
                {
                    msg.AppendLine(string.Format("Key='{0}';Value='{1}'", dataEntry.Key, dataEntry.Value));
                }
            }
        }
    }
}
