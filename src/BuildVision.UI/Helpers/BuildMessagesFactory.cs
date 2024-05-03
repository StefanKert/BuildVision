using System;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.Exports.Factories;
using BuildVision.UI.Models;
using BuildVision.Views.Settings;

namespace BuildVision.UI.Helpers
{
    public class BuildMessagesFactory : IBuildMessagesFactory
    {
        private readonly IPackageSettingsProvider _packageSettingsProvider;

        public BuildMessagesFactory(IPackageSettingsProvider packageSettingsProvider)
        {
            _packageSettingsProvider = packageSettingsProvider;
            
        }

        public string GetBuildBeginMajorMessage(IBuildInformationModel buildInformationModel)
        {
            var mainString = GetMainString(buildInformationModel);
            return string.Format(_packageSettingsProvider.Settings.BuildMessagesSettings.BuildBeginMajorMessageStringFormat, mainString);
        }

        private string GetMainString(IBuildInformationModel buildInformationModel)
        {
            var unitName = GetUnitName(buildInformationModel);
            var actionName = GetActionName(buildInformationModel.BuildAction);
            var beginAtString = GetBeginAtString(buildInformationModel.BuildAction);
            var timeString = GetTimeString(buildInformationModel.BuildStartTime);
            string mainString;
            switch (_packageSettingsProvider.Settings.BuildMessagesSettings.MajorMessageFormat)
            {
                case BuildMajorMessageFormat.Entire:
                    mainString = string.Format(Resources.BuildBeginStateLabelTemplate_Default, actionName, unitName, beginAtString, timeString);
                    break;

                case BuildMajorMessageFormat.Unnamed:
                    mainString = string.Format(Resources.BuildBeginStateLabelTemplate_ShortForm, actionName, beginAtString, timeString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_packageSettingsProvider.Settings.BuildMessagesSettings.MajorMessageFormat));
            }

            return mainString;
        }

        private string GetTimeString(DateTime? startTime)
        {
            try
            {
                return startTime.Value.ToString(_packageSettingsProvider.Settings.BuildMessagesSettings.DateTimeFormat);
            }
            catch (FormatException)
            {
                return Resources.InvalidTimeStringFormat;
            }
        }

        private static string GetBeginAtString(BuildAction? buildAction)
        {
            switch (buildAction.Value)
            {
                case BuildAction.RebuildAll:
                    return Resources.BuildActionRebuildAll_BeginAtString;

                case BuildAction.Build:
                    return Resources.BuildActionBuild_BeginAtString;

                case BuildAction.Clean:
                    return Resources.BuildActionClean_BeginAtString;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
        }

        private static string GetActionName(BuildAction buildAction)
        {
            switch (buildAction)
            {
                case BuildAction.RebuildAll:
                    return Resources.BuildActionRebuildAll;

                case BuildAction.Build:
                    return Resources.BuildActionBuild;

                case BuildAction.Clean:
                    return Resources.BuildActionClean;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction), $"Actual: {buildAction}");
            }
        }

        private string GetUnitName(IBuildInformationModel buildInformationModel)
        {
            string unitName = "";
            switch (buildInformationModel.BuildScope)
            {
                case BuildScope.Solution:
                    unitName = Resources.BuildScopeSolution_UnitName;
                    if (_packageSettingsProvider.Settings.BuildMessagesSettings.ShowSolutionName)
                    {
                        unitName += string.Format(Resources.BuildScopeSolution_SolutionNameTemplate, buildInformationModel.SolutionName);
                    }

                    break;

                case BuildScope.Batch:
                    unitName = Resources.BuildScopeBatch_UnitName;
                    break;

                case BuildScope.Project:
                    unitName = Resources.BuildScopeProject_UnitName;
                    // TODO specify name for project?
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildInformationModel.BuildScope));
            }

            return unitName;
        }

        public string GetBuildBeginExtraMessage(IBuildInformationModel buildInformationModel)
        {
            if (buildInformationModel.BuildStartTime == null || !_packageSettingsProvider.Settings.BuildMessagesSettings.ShowExtraMessage || _packageSettingsProvider.Settings.BuildMessagesSettings.ExtraMessageDelay < 0)
            {
                return string.Empty;
            }

            var timeSpan = DateTime.Now.Subtract(buildInformationModel.BuildStartTime.Value);
            if (timeSpan.TotalSeconds > _packageSettingsProvider.Settings.BuildMessagesSettings.ExtraMessageDelay)
            {
                return GetExtraTimePartString(timeSpan);
            }

            return string.Empty;
        }

        public string GetBuildDoneMessage(IBuildInformationModel buildInformationModel)
        {
            return GetBuildDoneMajorMessage(buildInformationModel) + GetBuildDoneExtraMessage(buildInformationModel);
        }

        private string GetBuildDoneMajorMessage(IBuildInformationModel buildInformationModel)
        {
            var buildAction = buildInformationModel.BuildAction;
            var buildScope = buildInformationModel.BuildScope;

            if (buildInformationModel.BuildFinishTime == null)
            {
                throw new InvalidOperationException();
            }

            string timeString;
            try
            {
                timeString = buildInformationModel.BuildFinishTime.Value.ToString(_packageSettingsProvider.Settings.BuildMessagesSettings.DateTimeFormat);
            }
            catch (FormatException)
            {
                timeString = Resources.InvalidTimeStringFormat;
            }

            string unitName;
            switch (buildScope)
            {
                case BuildScope.Solution:
                    unitName = Resources.BuildScopeSolution_UnitName;
                    if (_packageSettingsProvider.Settings.BuildMessagesSettings.ShowSolutionName)
                    {
                        unitName += string.Format(Resources.BuildScopeSolution_SolutionNameTemplate, buildInformationModel.SolutionName);
                    }

                    break;

                case BuildScope.Batch:
                    unitName = Resources.BuildScopeBatch_UnitName;
                    break;

                case BuildScope.Project:
                    unitName = Resources.BuildScopeProject_UnitName;
                    if (_packageSettingsProvider.Settings.BuildMessagesSettings.ShowProjectName)
                    {
                        // Todo this is probably wrong. maybe we should go the extra mile and check which projects are selected?
                        //var uniqProjName = solutionItem.Projects.LastOrDefault(x => x.State == ProjectState.BuildDone)?.UniqueName;
                        //var projItem = solutionItem.Projects.FirstOrDefault(item => item.UniqueName == uniqProjName);
                        //unitName += string.Format(Resources.BuildScopeProject_ProjectNameTemplate, projItem.Name);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildScope));
            }

            var actionName = GetActionName(buildInformationModel.BuildAction);
            var resultName = GetResultName(buildInformationModel.ResultState);

            string mainString;
            switch (_packageSettingsProvider.Settings.BuildMessagesSettings.MajorMessageFormat)
            {
                case BuildMajorMessageFormat.Entire:
                    mainString = string.Format(Resources.BuildDoneStateLabelTemplate_Default, actionName, unitName, resultName, timeString);
                    break;

                case BuildMajorMessageFormat.Unnamed:
                    mainString = string.Format(Resources.BuildDoneStateLabelTemplate_ShortForm, actionName, resultName, timeString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_packageSettingsProvider.Settings.BuildMessagesSettings.MajorMessageFormat));
            }

            return string.Format(_packageSettingsProvider.Settings.BuildMessagesSettings.BuildDoneMajorMessageStringFormat, mainString);
        }

        private static string GetResultName(BuildResultState resultState)
        {
            switch (resultState)
            {
                case BuildResultState.BuildCancelled:
                case BuildResultState.RebuildCancelled:
                    return Resources.BuildActionCancelled;
                case BuildResultState.BuildFailed:
                case BuildResultState.RebuildFailed:
                    return Resources.BuildActionFailed;
                case BuildResultState.BuildDone:
                case BuildResultState.RebuildDone:
                    return Resources.BuildActionFinishedSuccessfully;
                case BuildResultState.CleanCancelled:
                    return Resources.BuildActionCancelled_Clean;
                case BuildResultState.CleanFailed:
                    return Resources.BuildActionFailed_Clean;
                case BuildResultState.CleanDone:
                    return Resources.BuildActionFinishedSuccessfully_Clean;
                case BuildResultState.Unknown: // Check if this is right
                    return Resources.BuildActionFinished_Clean;
                default:
                    return Resources.BuildActionFinished;
            }
        }

        private string GetBuildDoneExtraMessage(IBuildInformationModel buildInformationModel)
        {
            if (buildInformationModel.BuildStartTime == null || buildInformationModel.BuildFinishTime == null || !_packageSettingsProvider.Settings.BuildMessagesSettings.ShowExtraMessage)
            {
                return string.Empty;
            }

            var timeSpan = buildInformationModel.BuildFinishTime.Value.Subtract(buildInformationModel.BuildStartTime.Value);
            return GetExtraTimePartString(timeSpan);
        }

        private string GetExtraTimePartString(TimeSpan timeSpan)
        {
            string extraTimePartString;
            switch (_packageSettingsProvider.Settings.BuildMessagesSettings.ExtraMessageFormat)
            {
                case BuildExtraMessageFormat.Custom:
                    try
                    {
                        extraTimePartString = timeSpan.ToString(_packageSettingsProvider.Settings.BuildMessagesSettings.TimeSpanFormat);
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
                    throw new ArgumentOutOfRangeException(nameof(_packageSettingsProvider.Settings.BuildMessagesSettings.ExtraMessageFormat));
            }

            return string.Format(_packageSettingsProvider.Settings.BuildMessagesSettings.ExtraMessageStringFormat, extraTimePartString);
        }
    }
}
