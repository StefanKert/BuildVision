using System;
using System.Collections.Generic;

using EnvDTE;

using ProjectItem = AlekseyNagovitsyn.BuildVision.Tool.Models.ProjectItem;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public abstract class BuildInfo
    {
        public abstract vsBuildAction? BuildAction { get; }

        public abstract vsBuildScope? BuildScope { get; }

        public abstract bool BuildIsCancelled { get; }

        public abstract BuildState CurrentBuildState { get; }

        public abstract DateTime? BuildStartTime { get; }

        public abstract DateTime? BuildFinishTime { get; }

        public abstract BuildedProjectsCollection BuildedProjects { get; }

        public abstract IEnumerable<ProjectItem> BuildingProjects { get; }

        public abstract BuildedSolution BuildedSolution { get; }

        public abstract void OverrideBuildProperties(vsBuildAction? buildAction = null, vsBuildScope? buildScope = null);

        public abstract Project BuildScopeProject { get; }
    }
}