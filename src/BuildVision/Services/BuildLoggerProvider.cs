using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using BuildVision.Contracts;
using BuildVision.UI.Contracts;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell.BuildLogging;

namespace BuildVision.Tool.Building
{
    [Export(typeof(IVsBuildLoggerProvider))]
    public class BuildLoggerProvider : IVsBuildLoggerProvider
    {
        public LoggerVerbosity Verbosity => LoggerVerbosity.Diagnostic;

        public BuildLoggerEvents Events =>
            BuildLoggerEvents.BuildStartedEvent |
            BuildLoggerEvents.BuildFinishedEvent |
            BuildLoggerEvents.ErrorEvent |
            BuildLoggerEvents.WarningEvent |
            BuildLoggerEvents.HighMessageEvent |
            BuildLoggerEvents.NormalMessageEvent |
            BuildLoggerEvents.ProjectStartedEvent |
            BuildLoggerEvents.ProjectFinishedEvent |
            BuildLoggerEvents.TargetStartedEvent |
            BuildLoggerEvents.TargetFinishedEvent |
            BuildLoggerEvents.CommandLine |
            BuildLoggerEvents.TaskStartedEvent |
            BuildLoggerEvents.TaskFinishedEvent |
            BuildLoggerEvents.LowMessageEvent |
            BuildLoggerEvents.ProjectEvaluationStartedEvent |
            BuildLoggerEvents.ProjectEvaluationFinishedEvent |
            BuildLoggerEvents.CustomEvent;

        public ILogger GetLogger(string projectPath, IEnumerable<string> targets, IDictionary<string, string> properties, bool isDesignTimeBuild)
        {
            if (BuildOutputLogger == null)
            {
                BuildOutputLogger = new BuildOutputLogger(Guid.NewGuid(), Verbosity);
                BuildOutputLogger.OnErrorRaised += OnErrorRaised;
            }
            BuildOutputLogger.Clear();
            return BuildOutputLogger;
        }

        public static BuildOutputLogger BuildOutputLogger { get; set; }

        public static event Action<BuildProjectContextEntry, object, ErrorLevel> OnErrorRaised;
    }
}
