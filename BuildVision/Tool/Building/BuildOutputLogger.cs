using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using BuildVision.Contracts;
using BuildVision.UI.Contracts;
using BuildVision.UI.Common.Logging;

namespace BuildVision.Tool.Building
{
    public class BuildOutputLogger : Logger
    {
        private IEventSource _eventSource;
        private List<BuildProjectContextEntry> _projects;
        private readonly Guid _id;

        private BuildOutputLogger(Guid id)
        {
            _id = id;
        }

        public IEventSource EventSource => _eventSource;
        public List<BuildProjectContextEntry> Projects => _projects;

        public override void Initialize(IEventSource eventSource)
        {
            _eventSource = eventSource;
            if (_eventSource == null)
            {
                TraceManager.TraceError("Unexpected value of BuildOutputLogger.EventSource: null");
                return;
            }

            _projects = new List<BuildProjectContextEntry>();
            _eventSource.ProjectStarted += OnProjectStarted;
        }

        private void OnProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            _projects.Add(new BuildProjectContextEntry(
                e.BuildEventContext.ProjectInstanceId,
                e.BuildEventContext.ProjectContextId,
                e.ProjectFile,
                e.GlobalProperties));
        }

        public static RegisterLoggerResult Register(Guid loggerId, LoggerVerbosity loggerVerbosity, out BuildOutputLogger buildLogger)
        {
            try
            {
                const BindingFlags InterfacePropertyFlags = BindingFlags.GetProperty
                                                            | BindingFlags.Public
                                                            | BindingFlags.Instance;

                BuildManager buildManager = BuildManager.DefaultBuildManager;
                Type buildHostType = buildManager.GetType().Assembly.GetType("Microsoft.Build.BackEnd.IBuildComponentHost");
                PropertyInfo loggingSeviceProperty = buildHostType.GetProperty("LoggingService", InterfacePropertyFlags);

                object loggingServiceObj;
                try
                {
                    // Microsoft.Build.BackEnd.ILoggingService
                    loggingServiceObj = loggingSeviceProperty.GetValue(buildManager, null);
                }
                catch (TargetInvocationException ex)
                {
                    ex.Trace("Microsoft.Build.BackEnd.ILoggingService is not available.");
                    buildLogger = null;
                    return RegisterLoggerResult.FatalError;
                }

                PropertyInfo loggersProperty = loggingServiceObj.GetType().GetProperty("Loggers", InterfacePropertyFlags);
                ICollection<ILogger> loggers = (ICollection<ILogger>)loggersProperty.GetValue(loggingServiceObj, null);

                ILogger logger = loggers.FirstOrDefault(x => x is BuildOutputLogger && ((BuildOutputLogger)x)._id.Equals(loggerId));
                if (logger != null)
                {
                    buildLogger = (BuildOutputLogger)logger;
                    buildLogger.Verbosity = loggerVerbosity;
                    return RegisterLoggerResult.AlreadyExists;
                }

                MethodInfo registerLoggerMethod = loggingServiceObj.GetType().GetMethod("RegisterLogger");
                buildLogger = new BuildOutputLogger(loggerId) { Verbosity = loggerVerbosity };
                bool registerResult = (bool)registerLoggerMethod.Invoke(loggingServiceObj, new object[] { buildLogger });

                return registerResult ? RegisterLoggerResult.RegisterSuccess : RegisterLoggerResult.RegisterFailed;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                buildLogger = null;
                return RegisterLoggerResult.FatalError;
            }
        }
    }
}