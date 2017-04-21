using System;
using System.Collections.Generic;
using BuildVision.Contracts;
using AlekseyNagovitsyn.BuildVision.Tool.Models;

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

        ProjectItem BuildScopeProject { get; }
    }
}