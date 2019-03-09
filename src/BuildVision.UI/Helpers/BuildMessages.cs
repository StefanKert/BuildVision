using System;
using System.Linq;
using BuildVision.Contracts;
using BuildVision.UI.Models;
using BuildVision.UI.Settings.Models;
using BuildVision.Core;

namespace BuildVision.UI.Helpers
{
    public class BuildMessagesFactory
    {
        private readonly BuildMessagesSettings _labelSettings;

        public BuildMessagesFactory(BuildMessagesSettings labelsSettings)
        {
            _labelSettings = labelsSettings;
        }

        public string GetBuildBeginMajorMessage(VisualStudioSolution solutionItem)
        {
            var mainString = GetMainString(solutionItem);
            return string.Format(_labelSettings.BuildBeginMajorMessageStringFormat, mainString);
        }

        private string GetMainString(VisualStudioSolution solutionItem)
        {
            var buildAction = BuildActions.BuildActionBuild; //TOdo replace

            var unitName = GetUnitName(solutionItem);
            var actionName = GetActionName(solutionItem.BuildAction);
            var beginAtString = GetBeginAtString(solutionItem.BuildAction);
            var timeString = GetTimeString(solutionItem);
            string mainString;
            switch (_labelSettings.MajorMessageFormat)
            {
                case BuildMajorMessageFormat.Entire:
                    mainString = string.Format(Resources.BuildBeginStateLabelTemplate_Default, actionName, unitName, beginAtString, timeString);
                    break;

                case BuildMajorMessageFormat.Unnamed:
                    mainString = string.Format(Resources.BuildBeginStateLabelTemplate_ShortForm, actionName, beginAtString, timeString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_labelSettings.MajorMessageFormat));
            }

            return mainString;
        }

        private string GetTimeString(VisualStudioSolution solution)
        {
            string timeString = "";
            try
            {
                timeString = solution.BuildStartTime.Value.ToString(_labelSettings.DateTimeFormat);
            }
            catch (FormatException)
            {
                timeString = Resources.InvalidTimeStringFormat;
            }

            return timeString;
        }

        private string GetBeginAtString(BuildActions? buildAction)
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

        private string GetActionName(BuildActions buildAction)
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

        private string GetUnitName(VisualStudioSolution solutionItem)
        {
            string unitName = "";
            switch (solutionItem.BuildScope)
            {
                case BuildScopes.BuildScopeSolution:
                    unitName = Resources.BuildScopeSolution_UnitName;
                    //if (_labelSettings.ShowSolutionName)
                        //unitName += string.Format(Resources.BuildScopeSolution_SolutionNameTemplate, solutionItem.Name);
                    break;

                case BuildScopes.BuildScopeBatch:
                    unitName = Resources.BuildScopeBatch_UnitName;
                    break;

                case BuildScopes.BuildScopeProject:
                    unitName = Resources.BuildScopeProject_UnitName;
                    // TODO specify name for project?
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(solutionItem.BuildScope));
            }

            return unitName;
        }

        public string GetBuildBeginExtraMessage(VisualStudioSolution solutionItem)
        {
            if (solutionItem.BuildStartTime == null || !_labelSettings.ShowExtraMessage || _labelSettings.ExtraMessageDelay < 0)
            {
                return string.Empty;
            }

            TimeSpan timeSpan = DateTime.Now.Subtract(solutionItem.BuildStartTime.Value);
            if (timeSpan.TotalSeconds > _labelSettings.ExtraMessageDelay)
            {
                return GetExtraTimePartString( timeSpan);
            }

            return string.Empty;
        }

        public string GetBuildDoneMessage(VisualStudioSolution solutionItem)
        {
            return GetBuildDoneMajorMessage(solutionItem) + GetBuildDoneExtraMessage(solutionItem);
        }

        private string GetBuildDoneMajorMessage(VisualStudioSolution solutionItem)
        {
            var buildAction = solutionItem.BuildAction;
            var buildScope = solutionItem.BuildScope;

            if (solutionItem.BuildFinishTime == null)
                throw new InvalidOperationException();

            string timeString;
            try
            {
                timeString = solutionItem.BuildFinishTime.Value.ToString(_labelSettings.DateTimeFormat);
            }
            catch (FormatException)
            {
                timeString = Resources.InvalidTimeStringFormat;
            }

            string unitName;
            switch (buildScope)
            {
                case BuildScopes.BuildScopeSolution:
                    unitName = Resources.BuildScopeSolution_UnitName;
                    //if (_labelSettings.ShowSolutionName)
                        //unitName += string.Format(Resources.BuildScopeSolution_SolutionNameTemplate, solutionItem.Name);
                    break;

                case BuildScopes.BuildScopeBatch:
                    unitName = Resources.BuildScopeBatch_UnitName;
                    break;

                case BuildScopes.BuildScopeProject:
                    unitName = Resources.BuildScopeProject_UnitName;
                    if (_labelSettings.ShowProjectName)
                    {
                        // Todo this is probably wrong. maybe we should go the extra mile and check which projects are selected?
                        var uniqProjName = solutionItem.Projects.LastOrDefault(x => x.State == ProjectState.BuildDone)?.UniqueName;
                        var projItem = solutionItem.Projects.FirstOrDefault(item => item.UniqueName == uniqProjName);
                        unitName += string.Format(Resources.BuildScopeProject_ProjectNameTemplate, projItem.Name);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildScope));
            }

            var actionName = GetActionName(solutionItem.BuildAction);
            var resultName = GetResultName(solutionItem.ResultState);

            string mainString;
            switch (_labelSettings.MajorMessageFormat)
            {
                case BuildMajorMessageFormat.Entire:
                    mainString = string.Format(Resources.BuildDoneStateLabelTemplate_Default, actionName, unitName, resultName, timeString);
                    break;

                case BuildMajorMessageFormat.Unnamed:
                    mainString = string.Format(Resources.BuildDoneStateLabelTemplate_ShortForm, actionName, resultName, timeString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_labelSettings.MajorMessageFormat));
            }

            string resultMainString = string.Format(_labelSettings.BuildDoneMajorMessageStringFormat, mainString);
            return resultMainString;
        }

        private string GetResultName(BuildResultState resultState)
        {
            switch (resultState)
            {
                case BuildResultState.BuildCancelled:
                case BuildResultState.RebuildCancelled:
                    return Resources.BuildActionCancelled;
                case BuildResultState.BuildFailed:
                case BuildResultState.RebuildFailed:
                    return Resources.BuildActionFailed;
                case BuildResultState.BuildSucceeded:
                case BuildResultState.RebuildSucceeded:
                    return Resources.BuildActionFinishedSuccessfully;
                case BuildResultState.CleanCancelled:
                    return Resources.BuildActionCancelled_Clean;
                case BuildResultState.CleanFailed:
                    return Resources.BuildActionFailed_Clean;
                case BuildResultState.CleanSucceeded:
                    return Resources.BuildActionFinishedSuccessfully_Clean;
                case BuildResultState.Unknown: // Check if this is right
                    return Resources.BuildActionFinished_Clean;
                default:
                    return Resources.BuildActionFinished; 
            }
        }

        private string GetBuildDoneExtraMessage(VisualStudioSolution solutionItem)
        {
            if (solutionItem.BuildStartTime == null || solutionItem.BuildFinishTime == null || !_labelSettings.ShowExtraMessage)
                return string.Empty;

            TimeSpan timeSpan = solutionItem.BuildFinishTime.Value.Subtract(solutionItem.BuildStartTime.Value);
            string extraTimePartString = GetExtraTimePartString(timeSpan);
            return string.Format(_labelSettings.ExtraMessageStringFormat, extraTimePartString);
        }

        private string GetExtraTimePartString(TimeSpan timeSpan)
        {
            string extraTimePartString;
            switch (_labelSettings.ExtraMessageFormat)
            {
                case BuildExtraMessageFormat.Custom:
                    try
                    {
                        extraTimePartString = timeSpan.ToString(_labelSettings.TimeSpanFormat);
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
                    throw new ArgumentOutOfRangeException(nameof(_labelSettings.ExtraMessageFormat));
            }

            return string.Format(_labelSettings.ExtraMessageStringFormat, extraTimePartString);
        }
    }
}
