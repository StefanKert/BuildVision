using System;
using System.Collections.Generic;

using EnvDTE;

using ProjectItem = AlekseyNagovitsyn.BuildVision.Tool.Models.ProjectItem;
using BuildVision.Contracts;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public interface IBuildInfo
    {
        BuildActions? BuildAction { get; }

        BuildScopes? BuildScope { get; }

        BuildState CurrentBuildState { get; }

        bool BuildIsCancelled { get; }

        DateTime? BuildStartTime { get; }

        DateTime? BuildFinishTime { get; }

        BuildedProjectsCollection BuildedProjects { get; }

        IList<ProjectItem> BuildingProjects { get; }

        BuildedSolution BuildedSolution { get; }

        void OverrideBuildProperties(BuildActions? buildAction = null, BuildScopes? buildScope = null);

        Project BuildScopeProject { get; }
    }
}