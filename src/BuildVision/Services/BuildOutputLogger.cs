using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using BuildVision.Contracts;
using BuildVision.UI.Contracts;
using BuildVision.UI.Common.Logging;
using BuildVision.Helpers;
using System.Diagnostics;
using BuildVision.Core;
using BuildVision.Exports.Providers;
using BuildVision.UI.Models;
using BuildVision.Exports;

namespace BuildVision.Tool.Building
{
    public class BuildOutputLogger : Logger, IBuildOutputLogger
    {
        private readonly Guid _loggerId;

        public RegisterLoggerResult LoggerState { get; set; }

        private List<BuildProjectContextEntry> _projects = new List<BuildProjectContextEntry>();

        public BuildOutputLogger(Guid loggerId, LoggerVerbosity loggerVerbosity)
        {
            _loggerId = loggerId;
            Verbosity = loggerVerbosity;
        }

        public override void Initialize(IEventSource eventSource)
        {
            _projects = new List<BuildProjectContextEntry>();
            eventSource.ProjectStarted += OnProjectStarted;
            eventSource.MessageRaised += (s, e) => EventSource_ErrorRaised(s, e, ErrorLevel.Message);
            eventSource.WarningRaised += (s, e) => EventSource_ErrorRaised(s, e, ErrorLevel.Warning);
            eventSource.ErrorRaised += (s, e) => EventSource_ErrorRaised(s, e, ErrorLevel.Error);
        }

        private void OnProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            _projects.Add(new BuildProjectContextEntry(
                e.BuildEventContext.ProjectInstanceId,
                e.BuildEventContext.ProjectContextId,
                e.ProjectFile,
                e.GlobalProperties));
        }

        public void Attach()
        {
            try
            {
                _projects.Clear();

                const BindingFlags InterfacePropertyFlags = BindingFlags.GetProperty
                                                            | BindingFlags.Public
                                                            | BindingFlags.Instance;
                var buildManager = Microsoft.Build.Execution.BuildManager.DefaultBuildManager;
                var buildHostType = buildManager.GetType().Assembly.GetType("Microsoft.Build.BackEnd.IBuildComponentHost");
                var loggingSeviceProperty = buildHostType.GetProperty("LoggingService", InterfacePropertyFlags);

                object loggingServiceObj;
                try
                {
                    // Microsoft.Build.BackEnd.ILoggingService
                    loggingServiceObj = loggingSeviceProperty.GetValue(buildManager, null);
                }
                catch (TargetInvocationException ex)
                {
                    ex.Trace("Microsoft.Build.BackEnd.ILoggingService is not available.");
                    LoggerState = RegisterLoggerResult.FatalError;
                    return;
                }

                var loggersProperty = loggingServiceObj.GetType().GetProperty("Loggers", InterfacePropertyFlags);
                var loggers = (ICollection<ILogger>) loggersProperty.GetValue(loggingServiceObj, null);

                var logger = loggers.FirstOrDefault(x => x is BuildOutputLogger && ((BuildOutputLogger) x)._loggerId.Equals(_loggerId));
                if (logger != null)
                {
                    LoggerState = RegisterLoggerResult.AlreadyExists;
                    return;
                }

                var registerLoggerMethod = loggingServiceObj.GetType().GetMethod("RegisterLogger");
                var registerResult = (bool) registerLoggerMethod.Invoke(loggingServiceObj, new object[] { this });
                LoggerState = registerResult ? RegisterLoggerResult.RegisterSuccess : RegisterLoggerResult.RegisterFailed;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                LoggerState = RegisterLoggerResult.FatalError;
            }
        }

        public bool IsProjectUpToDate(IProjectItem projectItem)
        {
            return !_projects.Exists(t => t.FileName == projectItem.FullName);
        }

        private void EventSource_ErrorRaised(object sender, BuildEventArgs e, ErrorLevel errorLevel)
        {
            try
            {
                bool verified = VerifyLoggerBuildEvent(e, errorLevel);
                if (!verified)
                    return;

                int projectInstanceId = e.BuildEventContext.ProjectInstanceId;
                int projectContextId = e.BuildEventContext.ProjectContextId;

                var projectEntry = _projects.Find(t => t.InstanceId == projectInstanceId && t.ContextId == projectContextId);
                if (projectEntry == null)
                {
                    TraceManager.Trace(string.Format("Project entry not found by ProjectInstanceId='{0}' and ProjectContextId='{1}'.", projectInstanceId, projectContextId), EventLogEntryType.Warning);
                    return;
                }
                if (projectEntry.IsInvalid)
                    return;

                OnErrorRaised(projectEntry, e, errorLevel);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private bool VerifyLoggerBuildEvent(BuildEventArgs eventArgs, ErrorLevel errorLevel)
        {
            if (eventArgs.BuildEventContext.IsBuildEventContextInvalid())
                return false;

            if (errorLevel == ErrorLevel.Message)
            {
                var messageEventArgs = (BuildMessageEventArgs) eventArgs;
                if (!messageEventArgs.IsUserMessage(this))
                    return false;
            }

            return true;
        }

        public void Reset()
        {
            _projects.Clear();
        }

        public event Action<BuildProjectContextEntry, object, ErrorLevel> OnErrorRaised;
    }
}
