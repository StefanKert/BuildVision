using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using BuildVision.Contracts;
using BuildVision.UI.Contracts;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell.BuildLogging;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem;

namespace BuildVision.Tool.Building
{
    [AppliesTo(ProjectCapabilities.AlwaysApplicable)]
    [Export(typeof(IVsBuildLoggerProvider))]
    [Export(typeof(IBuildLoggerProviderAsync))]
    public class BuildLoggerProvider : IVsBuildLoggerProvider, IBuildLoggerProviderAsync
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

        public Task<IImmutableSet<ILogger>> GetLoggersAsync(IReadOnlyList<string> targets, IImmutableDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            if (BuildOutputLogger == null)
            {
                BuildOutputLogger = new BuildOutputLogger(Guid.NewGuid(), Verbosity);
                BuildOutputLogger.OnErrorRaised += OnErrorRaised;
            }
            BuildOutputLogger.Clear();

            var loggers = ImmutableHashSet<ILogger>.Empty.Add(BuildOutputLogger);
            return Task.FromResult<IImmutableSet<ILogger>>(loggers);
        }

        public static BuildOutputLogger BuildOutputLogger { get; set; }

        public static event Action<BuildProjectContextEntry, object, ErrorLevel> OnErrorRaised;
    }
}
