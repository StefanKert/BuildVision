using System;
using System.Collections.Generic;
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

        private Dictionary<string ,BuildProjectContextEntry> _projectsLookup = new Dictionary<string, BuildProjectContextEntry>();

        public BuildOutputLogger(Guid loggerId, LoggerVerbosity loggerVerbosity)
        {
            _loggerId = loggerId;
            Verbosity = loggerVerbosity;
        }

        public override void Initialize(IEventSource eventSource)
        {
            _projectsLookup = new Dictionary<string, BuildProjectContextEntry>();
            eventSource.ProjectStarted += OnProjectStarted;
            eventSource.MessageRaised += (sender, e) => EventSource_Event(e.ProjectFile, e, ErrorLevel.Message); 
            eventSource.WarningRaised += (sender, e) => EventSource_Event(e.ProjectFile, e, ErrorLevel.Warning);
            eventSource.ErrorRaised += (sender, e) => EventSource_Event(e.ProjectFile, e, ErrorLevel.Error);
        }

        private void OnProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            var entry = new BuildProjectContextEntry(e.ProjectFile, e.GlobalProperties);
            if (!_projectsLookup.ContainsKey(entry.ProjectFile))
            {
                _projectsLookup.Add(entry.ProjectFile, entry);
            }

            _logger.Information("Currently there are {Count} projects listed.", _projectsLookup.Count);
        }

        public void Attach()
        {
            try
            {
                _projectsLookup.Clear();

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

        public bool IsProjectUpToDate(IProjectItem projectItem) => !_projectsLookup.ContainsKey(projectItem.FullName);

        private void EventSource_Event(string projectFile, BuildEventArgs e, ErrorLevel errorLevel)
        {
            try
            {
                if (e.BuildEventContext.IsBuildEventContextInvalid())
                {
                    return;
                }
                if(projectFile == null)
                {
                    return;
                }

                if (!_projectsLookup.TryGetValue(projectFile, out var projectEntry))
                {
                    _logger.Warning("Project entry not found by ProjectFile='{ProjectFile}'.", projectFile);
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
