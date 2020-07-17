using System.ComponentModel;
using System.Diagnostics;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models;
using SortDescription = BuildVision.UI.Settings.Models.Sorting.SortDescription;

namespace BuildVision.UI.ViewModels
{
    public class SortOrderFactory
    {
        public static ProjectItemColumnSorter GetProjectItemSorter(SortDescription sortDescription)
        {
            var sortOrder = sortDescription.Order;
            var sortPropertyName = sortDescription.Property;
            if (sortOrder != SortOrder.None && !string.IsNullOrEmpty(sortPropertyName))
            {
                ListSortDirection? sortDirection = sortOrder.ToSystem();
                Debug.Assert(sortDirection != null);

                switch (sortPropertyName)
                {
                    case nameof(ProjectItem.BuildElapsedTime):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.BuildElapsedTime);
                    case nameof(ProjectItem.BuildFinishTime):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.BuildFinishTime);
                    case nameof(ProjectItem.BuildOrder):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.BuildOrder);
                    case nameof(ProjectItem.CommonType):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.CommonType);
                    case nameof(ProjectItem.Configuration):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.Configuration);
                    case nameof(ProjectItem.ErrorsCount):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.ErrorsCount);
                    case nameof(ProjectItem.ExtenderNames):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.ExtenderNames);
                    case nameof(ProjectItem.FlavourType):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.FlavourType);
                    case nameof(ProjectItem.Framework):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.Framework);
                    case nameof(ProjectItem.FullName):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.FullName);
                    case nameof(ProjectItem.FullPath):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.FullPath);
                    case nameof(ProjectItem.IsBatchBuildProject):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.IsBatchBuildProject);
                    case nameof(ProjectItem.Language):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.Language);
                    case nameof(ProjectItem.MainFlavourType):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.MainFlavourType);
                    case nameof(ProjectItem.MessagesCount):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.MessagesCount);
                    case nameof(ProjectItem.Name):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.Name);
                    case nameof(ProjectItem.OutputType):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.OutputType);
                    case nameof(ProjectItem.Platform):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.Platform);
                    case nameof(ProjectItem.RootNamespace):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.RootNamespace);
                    case nameof(ProjectItem.SolutionFolder):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.SolutionFolder);
                    case nameof(ProjectItem.State):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.State);
                    case nameof(ProjectItem.Success):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.Success);
                    case nameof(ProjectItem.UniqueName):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.UniqueName);
                    case nameof(ProjectItem.WarningsCount):
                        return new ProjectItemColumnSorter(sortDirection.Value, prop => prop.WarningsCount);
                }
            }
            return new ProjectItemColumnSorter(ListSortDirection.Ascending, prop => prop.BuildOrder);
        }
    }
}
