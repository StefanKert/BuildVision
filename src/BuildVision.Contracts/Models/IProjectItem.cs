using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using BuildVision.Contracts;

namespace BuildVision.UI.Models
{
    public interface IProjectItem
    {
        TimeSpan? BuildElapsedTime { get; }
        DateTime? BuildFinishTime { get; set; }
        int? BuildOrder { get; set; }
        DateTime? BuildStartTime { get; set; }
        string CommonType { get; set; }
        string Configuration { get; set; }
        string ExtenderNames { get; set; }
        string FlavourType { get; set; }
        string Framework { get; set; }
        string FullName { get; set; }
        string FullPath { get; set; }
        bool IsBatchBuildProject { get; set; }
        string Language { get; set; }
        string MainFlavourType { get; set; }
        string Name { get; set; }
        string OutputType { get; set; }
        string Platform { get; set; }
        string RootNamespace { get; set; }
        string SolutionFolder { get; set; }
        ProjectState State { get; set; }
        ControlTemplate StateBitmap { get; }
        bool Success { get; set; }
        string UniqueName { get; set; }
        int MessagesCount { get; set; }
        int ErrorsCount { get; set; }
        int WarningsCount { get; set; }
        ObservableCollection<ErrorItem> Errors { get; }
        ObservableCollection<ErrorItem> Warnings { get; }
        ObservableCollection<ErrorItem> Messages { get; }

        void RaiseBuildElapsedTimeChanged();
        void AddErrorItem(ErrorItem errorItem);
    }
}
