using System;
using System.Diagnostics;
using System.Linq;
using BuildVision.Contracts;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;
using BuildVision.UI.Extensions;
using BuildVision.UI.Settings.Models;

namespace BuildVision.UI.Helpers
{
    public static class BuildMessages
    {
        public static string GetBuildBeginMajorMessage(SolutionItem solutionItem,  IBuildInfo buildInfo, BuildMessagesSettings labelsSettings)
        {
            if (buildInfo.BuildAction == null || buildInfo.BuildScope == null || buildInfo.BuildAction.Value == BuildActions.BuildActionDeploy)
                return Resources.UnknownBuildActionOrScope_BuildBeginText;

            if (buildInfo.BuildStartTime == null)
                throw new InvalidOperationException();

            var mainString = GetMainString(solutionItem, buildInfo, labelsSettings);
            return string.Format(labelsSettings.BuildBeginMajorMessageStringFormat, mainString);
        }

        private static string GetMainString(SolutionItem solutionItem, IBuildInfo buildInfo, BuildMessagesSettings labelsSettings)
        {
            var unitName = GetUnitName(solutionItem, buildInfo, labelsSettings);
            var actionName = GetActionName(buildInfo.BuildAction.Value);
            var beginAtString = GetBeginAtString(buildInfo.BuildAction.Value);
            var timeString = GetTimeString(buildInfo, labelsSettings);
            string mainString;
            switch (labelsSettings.MajorMessageFormat)
            {
                case BuildMajorMessageFormat.Entire:
                    mainString = string.Format(Resources.BuildBeginStateLabelTemplate_Default, actionName, unitName, beginAtString, timeString);
                    break;

                case BuildMajorMessageFormat.Unnamed:
                    mainString = string.Format(Resources.BuildBeginStateLabelTemplate_ShortForm, actionName, beginAtString, timeString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(labelsSettings.MajorMessageFormat));
            }

            return mainString;
        }

        private static string GetTimeString(IBuildInfo buildInfo, BuildMessagesSettings labelsSettings)
        {
            string timeString;
            try
            {
                timeString = buildInfo.BuildStartTime.Value.ToString(labelsSettings.DateTimeFormat);
            }
            catch (FormatException)
            {
                timeString = Resources.InvalidTimeStringFormat;
            }

            return timeString;
        }

        private static string GetBeginAtString(BuildActions? buildAction)
        {
            switch (buildAction.Value)
            {
                case BuildActions.BuildActionRebuildAll:
                    return Resources.BuildActionRebuildAll_BeginAtString;

                case BuildActions.BuildActionBuild:
                    return Resources.BuildActionBuild_BeginAtString;
  
                case BuildActions.BuildActionClean:
                    return Resources.BuildActionClean_BeginAtString;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
        }

        private static string GetActionName(BuildActions buildAction)
        {
            switch (buildAction)
            {
                case BuildActions.BuildActionRebuildAll:
                    return Resources.BuildActionRebuildAll;

                case BuildActions.BuildActionBuild:
                    return Resources.BuildActionBuild;

                case BuildActions.BuildActionClean:
                    return Resources.BuildActionClean;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
        }

        private static string GetUnitName(SolutionItem solutionItem, IBuildInfo buildInfo, BuildMessagesSettings labelsSettings)
        {
            string unitName;
            switch (buildInfo.BuildScope.Value)
            {
                case BuildScopes.BuildScopeSolution:
                    unitName = Resources.BuildScopeSolution_UnitName;
                    if (labelsSettings.ShowSolutionName)
                        unitName += string.Format(Resources.BuildScopeSolution_SolutionNameTemplate, solutionItem.Name);
                    break;

                case BuildScopes.BuildScopeBatch:
                    unitName = Resources.BuildScopeBatch_UnitName;
                    break;

                case BuildScopes.BuildScopeProject:
                    unitName = Resources.BuildScopeProject_UnitName;
                    if (labelsSettings.ShowProjectName)
                    {
                        var proj = buildInfo.BuildScopeProject;
                        if (proj != null)
                        {
                            unitName += string.Format(Resources.BuildScopeProject_ProjectNameTemplate, proj.Name);
                        }
                        else
                        {
                            unitName = Resources.BuildScopeBatch_UnitName;
                            buildInfo.OverrideBuildProperties(buildScope: BuildScopes.BuildScopeBatch);
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildInfo.BuildScope));
            }

            return unitName;
        }

        public static string GetBuildBeginExtraMessage(IBuildInfo buildInfo, BuildMessagesSettings labelsSettings)
        {
            if (buildInfo == null || buildInfo.BuildStartTime == null || !labelsSettings.ShowExtraMessage || labelsSettings.ExtraMessageDelay < 0)
            {
                return string.Empty;
            }

            TimeSpan timeSpan = DateTime.Now.Subtract(buildInfo.BuildStartTime.Value);
            if (timeSpan.TotalSeconds > labelsSettings.ExtraMessageDelay)
            {
                return GetExtraTimePartString(labelsSettings, timeSpan);
            }

            return string.Empty;
        }

        public static string GetBuildDoneMessage(SolutionItem solutionItem, IBuildInfo buildInfo, BuildMessagesSettings labelsSettings)
        {
            return GetBuildDoneMajorMessage(solutionItem, buildInfo, labelsSettings) + GetBuildDoneExtraMessage(buildInfo, labelsSettings);
        }

        private static string GetBuildDoneMajorMessage(SolutionItem solutionItem, IBuildInfo buildInfo, BuildMessagesSettings labelsSettings)
        {
            if (buildInfo == null)
                return Resources.BuildDoneText_BuildNotStarted;

            var buildAction = buildInfo.BuildAction;
            var buildScope = buildInfo.BuildScope;

            if (buildInfo.BuildFinishTime == null)
                throw new InvalidOperationException();

            string timeString;
            try
            {
                timeString = buildInfo.BuildFinishTime.Value.ToString(labelsSettings.DateTimeFormat);
            }
            catch (FormatException)
            {
                timeString = Resources.InvalidTimeStringFormat;
            }

            if (buildAction == null || buildScope == null)
                return string.Format(Resources.BuildDoneText_NotSupported_BuildActionOrScopeIsNull_CompletedAtTemplate, timeString); //? WTF???

            string unitName;
            switch (buildScope.Value)
            {
                case BuildScopes.BuildScopeSolution:
                    unitName = Resources.BuildScopeSolution_UnitName;
                    if (labelsSettings.ShowSolutionName)
                        unitName += string.Format(Resources.BuildScopeSolution_SolutionNameTemplate, solutionItem.Name);
                    break;

                case BuildScopes.BuildScopeBatch:
                    unitName = Resources.BuildScopeBatch_UnitName;
                    break;

                case BuildScopes.BuildScopeProject:
                    unitName = Resources.BuildScopeProject_UnitName;
                    if (labelsSettings.ShowProjectName)
                    {
                        // Skip dependent projects. The last project in the list is the target project.
                        string uniqProjName = buildInfo.BuildedProjects[buildInfo.BuildedProjects.Count - 1].UniqueName;
                        ProjectItem projItem = solutionItem.AllProjects.FirstOrDefault(item => item.UniqueName == uniqProjName);
                        Debug.Assert(projItem != null);

                        unitName += string.Format(Resources.BuildScopeProject_ProjectNameTemplate, projItem.Name);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildScope));
            }

            var actionName = GetActionName(buildInfo);
            var resultName = GetResultName(solutionItem, buildInfo);

            string mainString;
            switch (labelsSettings.MajorMessageFormat)
            {
                case BuildMajorMessageFormat.Entire:
                    mainString = string.Format(Resources.BuildDoneStateLabelTemplate_Default, actionName, unitName, resultName, timeString);
                    break;

                case BuildMajorMessageFormat.Unnamed:
                    mainString = string.Format(Resources.BuildDoneStateLabelTemplate_ShortForm, actionName, resultName, timeString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(labelsSettings.MajorMessageFormat));
            }

            string resultMainString = string.Format(labelsSettings.BuildDoneMajorMessageStringFormat, mainString);
            return resultMainString;
        }

        private static string GetActionName(IBuildInfo buildInfo)
        {
            if (buildInfo.BuildAction == null)
                throw new InvalidOperationException();

            switch (buildInfo.BuildAction.Value)
            {
                case BuildActions.BuildActionBuild:
                    return Resources.BuildActionBuild;

                case BuildActions.BuildActionRebuildAll:
                    return Resources.BuildActionRebuildAll;

                case BuildActions.BuildActionClean:
                    return Resources.BuildActionClean;

                case BuildActions.BuildActionDeploy:
                    throw new InvalidOperationException();

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildInfo.BuildAction));
            }
        }

        private static string GetResultName(SolutionItem solutionItem, IBuildInfo buildInfo)
        {
            var buildAction = buildInfo.BuildAction;
            int errorStateProjectsCount = solutionItem.AllProjects.Count(item => item.State.IsErrorState());

            if (buildInfo.BuildIsCancelled)
                return buildAction.Value == BuildActions.BuildActionClean ? Resources.BuildActionCancelled_Clean : Resources.BuildActionCancelled;
            else if (!buildInfo.BuildedProjects.BuildWithoutErrors)
                return buildAction.Value == BuildActions.BuildActionClean ? Resources.BuildActionFailed_Clean : Resources.BuildActionFailed;
            else if (errorStateProjectsCount == 0)
                return buildAction.Value == BuildActions.BuildActionClean ? Resources.BuildActionFinishedSuccessfully_Clean : Resources.BuildActionFinishedSuccessfully;
            else
                return buildAction.Value == BuildActions.BuildActionClean ? Resources.BuildActionFinished_Clean : Resources.BuildActionFinished;
        }

        private static string GetBuildDoneExtraMessage(IBuildInfo buildInfo, BuildMessagesSettings labelsSettings)
        {
            if (buildInfo?.BuildStartTime == null || buildInfo?.BuildFinishTime == null || !labelsSettings.ShowExtraMessage)
                return string.Empty;

            TimeSpan timeSpan = buildInfo.BuildFinishTime.Value.Subtract(buildInfo.BuildStartTime.Value);
            string extraTimePartString = GetExtraTimePartString(labelsSettings, timeSpan);
            return string.Format(labelsSettings.ExtraMessageStringFormat, extraTimePartString);
        }

        private static string GetExtraTimePartString(BuildMessagesSettings labelsSettings, TimeSpan timeSpan)
        {
            string extraTimePartString;
            switch (labelsSettings.ExtraMessageFormat)
            {
                case BuildExtraMessageFormat.Custom:
                    try
                    {
                        extraTimePartString = timeSpan.ToString(labelsSettings.TimeSpanFormat);
                    }
                    catch (FormatException)
                    {
                        extraTimePartString = Resources.InvalidTimeStringFormat;
                    }
                    break;

                case BuildExtraMessageFormat.TotalSeconds:
                    extraTimePartString = string.Format("{0}", Math.Truncate(timeSpan.TotalSeconds));
                    break;

                case BuildExtraMessageFormat.TotalMinutes:
                    extraTimePartString = string.Format("{0}", Math.Truncate(timeSpan.TotalMinutes));
                    break;

                case BuildExtraMessageFormat.TotalMinutesWithSeconds:
                    extraTimePartString = string.Format("{0:00}:{1:00}", Math.Truncate(timeSpan.TotalMinutes), timeSpan.Seconds);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(labelsSettings.ExtraMessageFormat));
            }

            return string.Format(labelsSettings.ExtraMessageStringFormat, extraTimePartString);
        }
    }
}
