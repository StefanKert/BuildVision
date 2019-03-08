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
using BuildVision.Helpers;
using BuildVision.UI.ViewModels;
using BuildVision.Core;

namespace BuildVision.Tool.Building
{
    public class BuildOutputLogger : Logger
    {
  
        private List<BuildProjectContextEntry> _projects;
        private readonly Guid _id;
        private readonly IVsItemLocatorService _locatorService;
        private readonly BuildVisionPaneViewModel _viewModel;

        private BuildOutputLogger(Guid id, IVsItemLocatorService locatorService = null, BuildVisionPaneViewModel  viewModel = null)
        {
            _id = id;
            _locatorService = locatorService;
            _viewModel = viewModel;
        }

        public List<BuildProjectContextEntry> Projects => _projects;

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

        public static RegisterLoggerResult Register(Guid loggerId, LoggerVerbosity loggerVerbosity, out BuildOutputLogger buildLogger)
        {
            try
            {
                const BindingFlags InterfacePropertyFlags = BindingFlags.GetProperty
                                                            | BindingFlags.Public
                                                            | BindingFlags.Instance;

                var buildManager = Microsoft.Build.Execution.BuildManager.DefaultBuildManager;
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

        private void EventSource_ErrorRaised(object sender, BuildEventArgs e, ErrorLevel errorLevel)
        {
            try
            {
                bool verified = VerifyLoggerBuildEvent(e, errorLevel);
                if (!verified)
                    return;

                int projectInstanceId = e.BuildEventContext.ProjectInstanceId;
                int projectContextId = e.BuildEventContext.ProjectContextId;

                var projectEntry = Projects.Find(t => t.InstanceId == projectInstanceId && t.ContextId == projectContextId);
                if (projectEntry == null)
                {
                    //TraceManager.Trace(string.Format("Project entry not found by ProjectInstanceId='{0}' and ProjectContextId='{1}'.", projectInstanceId, projectContextId), EventLogEntryType.Warning);
                    return;
                }
                if (projectEntry.IsInvalid)
                    return;

                //if (!_locatorService.GetProjectItem(_viewModel, BuildScope, projectEntry, out var projectItem))
                //{
                //    projectEntry.IsInvalid = true;
                //    return;
                //}

                //BuildedProject buildedProject = BuildedProjects[projectItem];
                //var errorItem = new ErrorItem(errorLevel, (eI) =>
                //{
                //    Services.Dte.Solution.GetProject(x => x.UniqueName == projectItem.UniqueName).NavigateToErrorItem(eI);
                //});

                //switch (errorLevel)
                //{
                //    case ErrorLevel.Message:
                //        errorItem.Init((BuildMessageEventArgs) e);
                //        break;

                //    case ErrorLevel.Warning:
                //        errorItem.Init((BuildWarningEventArgs) e);
                //        throw new ArgumentOutOfRangeException("errorLevel");
                //}
                //errorItem.VerifyValues();
                //        break;

                //    case ErrorLevel.Error:
                //        errorItem.Init((BuildErrorEventArgs) e);
                //        break;

                //    default:
                //buildedProject.ErrorsBox.Add(errorItem);
                //OnErrorRaised(this, new BuildErrorRaisedEventArgs(errorLevel, buildedProject));
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
    }
}
