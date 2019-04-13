using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BuildVision.Common.Logging;
using BuildVision.Contracts;
using BuildVision.Exports;
using BuildVision.Helpers;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace BuildVision.Tool.Building
{
    public class BuildOutputLogger : Logger, IBuildOutputLogger
    {
        private readonly Guid _loggerId;
        private Serilog.ILogger _logger = LogManager.ForContext<BuildOutputLogger>();
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
            eventSource.MessageRaised += (s, e) => EventSource_ErrorRaised(e, ErrorLevel.Message);
            eventSource.WarningRaised += (s, e) => EventSource_ErrorRaised(e, ErrorLevel.Warning);
            eventSource.ErrorRaised += (s, e) => EventSource_ErrorRaised(e, ErrorLevel.Error);
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
                    _logger.Error(ex, "Microsoft.Build.BackEnd.ILoggingService is not available.");
                    LoggerState = RegisterLoggerResult.FatalError;
                    return;
                }

                var loggersProperty = loggingServiceObj.GetType().GetProperty("Loggers", InterfacePropertyFlags);
                var loggers = (ICollection<ILogger>)loggersProperty.GetValue(loggingServiceObj, null);

                var logger = loggers.FirstOrDefault(x => x is BuildOutputLogger && ((BuildOutputLogger)x)._loggerId.Equals(_loggerId));
                if (logger != null)
                {
                    LoggerState = RegisterLoggerResult.AlreadyExists;
                    return;
                }

                var registerLoggerMethod = loggingServiceObj.GetType().GetMethod("RegisterLogger");
                var registerResult = (bool)registerLoggerMethod.Invoke(loggingServiceObj, new object[] { this });
                LoggerState = registerResult ? RegisterLoggerResult.RegisterSuccess : RegisterLoggerResult.RegisterFailed;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error registering MSBuild Logger");
                LoggerState = RegisterLoggerResult.FatalError;
            }
        }

        public bool IsProjectUpToDate(IProjectItem projectItem)
        {
            return !_projects.Exists(t => t.FileName == projectItem.FullName);
        }

        private void EventSource_ErrorRaised(BuildEventArgs e, ErrorLevel errorLevel)
        {
            try
            {
                if (e.BuildEventContext.IsBuildEventContextInvalid())
                    return;

                int projectInstanceId = e.BuildEventContext.ProjectInstanceId;
                int projectContextId = e.BuildEventContext.ProjectContextId;

                var projectEntry = _projects.Find(t => t.InstanceId == projectInstanceId && t.ContextId == projectContextId);
                if (projectEntry == null)
                {
                    _logger.Warning("Project entry not found by ProjectInstanceId='{ProjectInstanceId}' and ProjectContextId='{ProjectContextId}'.", projectInstanceId, projectContextId);
                    return;
                }
                if (projectEntry.IsInvalid)
                {
                    return;
                }

                OnErrorRaised(projectEntry, e, errorLevel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during eventsource_raised.");
            }
        }

        public event Action<BuildProjectContextEntry, object, ErrorLevel> OnErrorRaised;
    }
}
