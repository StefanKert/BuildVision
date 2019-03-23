using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace BuildVision.Contracts.Models
{
    public interface IBuildInformationModel : INotifyPropertyChanged
    {
        BuildActions BuildAction { get; set; }
        DateTime? BuildFinishTime { get; set; }
        BuildScopes BuildScope { get; set; }
        DateTime? BuildStartTime { get; set; }
        BuildState CurrentBuildState { get; set; }
        int ErrorCount { get; set; }
        int FailedProjectsCount { get; set; }
        int MessagesCount { get; set; }
        string StateMessage { get; set; }
        int SucceededProjectsCount { get; set; }
        int UpToDateProjectsCount { get; set; }
        int WarnedProjectsCount { get; set; }
        int WarningsCount { get; set; }

        BuildResultState ResultState { get; }
        string StateIconKey { get; }
    }
}
