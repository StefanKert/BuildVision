using System;
using System.Linq;
using BuildVision.Contracts;
using BuildVision.UI.Models;
using BuildVision.UI.Settings.Models;
using BuildVision.Core;
using BuildVision.Exports.Providers;
using BuildVision.Contracts.Models;
using BuildVision.Exports.Factories;

namespace BuildVision.UI.Helpers
{
    public class BuildMessagesFactory : IBuildMessagesFactory
    {
        private readonly BuildMessagesSettings _labelSettings;
        private IBuildInformationProvider _buildInformationProvider;
        private IBuildingProjectsProvider _buildingProjectsProvider;
        private IBuildInformationModel _buildInformation;

        public BuildMessagesFactory(ControlSettings controlSettings,
            IBuildInformationProvider buildInformationProvider,
            IBuildingProjectsProvider buildingProjectsProvider)
        {
            _labelSettings = controlSettings.BuildMessagesSettings;
            _buildInformationProvider = buildInformationProvider;
            _buildingProjectsProvider = buildingProjectsProvider;
            _buildInformation = _buildInformationProvider.GetBuildInformationModel();
        }

        public string GetBuildBeginMajorMessage()
        {
            var mainString = GetMainString(_buildInformation.BuildAction);
            return string.Format(_labelSettings.BuildBeginMajorMessageStringFormat, mainString);
        }

        private string GetMainString(BuildActions buildAction)
        {
            var unitName = GetUnitName();
            var actionName = GetActionName(buildAction);
            var beginAtString = GetBeginAtString(buildAction);
            var timeString = GetTimeString();
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

        private string GetTimeString()
        {
            string timeString = "";
            try
            {
                timeString = _buildInformation.BuildStartTime.Value.ToString(_labelSettings.DateTimeFormat);
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

        private string GetUnitName()
        {
            string unitName = "";
            switch (_buildInformation.BuildScope)
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
                    throw new ArgumentOutOfRangeException(nameof(_buildInformation.BuildScope));
            }

            return unitName;
        }

        public string GetBuildBeginExtraMessage()
        {
            if (_buildInformation.BuildStartTime == null || !_labelSettings.ShowExtraMessage || _labelSettings.ExtraMessageDelay < 0)
            {
                return string.Empty;
            }

            TimeSpan timeSpan = DateTime.Now.Subtract(_buildInformation.BuildStartTime.Value);
            if (timeSpan.TotalSeconds > _labelSettings.ExtraMessageDelay)
            {
                return GetExtraTimePartString( timeSpan);
            }

            return string.Empty;
        }

        public string GetBuildDoneMessage()
        {
            return GetBuildDoneMajorMessage() + GetBuildDoneExtraMessage();
        }

        private string GetBuildDoneMajorMessage()
        {
            var buildAction = _buildInformation.BuildAction;
            var buildScope = _buildInformation.BuildScope;

            if (_buildInformation.BuildFinishTime == null)
                throw new InvalidOperationException();

            string timeString;
            try
            {
                timeString = _buildInformation.BuildFinishTime.Value.ToString(_labelSettings.DateTimeFormat);
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
                        //var uniqProjName = solutionItem.Projects.LastOrDefault(x => x.State == ProjectState.BuildDone)?.UniqueName;
                        //var projItem = solutionItem.Projects.FirstOrDefault(item => item.UniqueName == uniqProjName);
                        //unitName += string.Format(Resources.BuildScopeProject_ProjectNameTemplate, projItem.Name);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildScope));
            }

            var actionName = GetActionName(_buildInformation.BuildAction);
            var resultName = GetResultName(_buildInformation.ResultState);

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

        private string GetBuildDoneExtraMessage()
        {
            if (_buildInformation.BuildStartTime == null || _buildInformation.BuildFinishTime == null || !_labelSettings.ShowExtraMessage)
                return string.Empty;

            TimeSpan timeSpan = _buildInformation.BuildFinishTime.Value.Subtract(_buildInformation.BuildStartTime.Value);
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
