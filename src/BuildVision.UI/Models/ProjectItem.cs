using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using BuildVision.Common;
using BuildVision.Common.Extensions;
using BuildVision.Contracts;
using BuildVision.UI.Extensions;
using BuildVision.UI.Modelss;

namespace BuildVision.UI.Models
{
    public class ProjectItem : BindableBase, IProjectItem
    {
        private const string ResourcesUri = @"Resources/ProjectItem.Resources.xaml";

        public bool IsBatchBuildProject { get; set; }
        public bool Success { get; set; }

        [GridColumn("ProjectItemHeader_UniqueName", ColumnsOrder.UniqueName, false, ExampleValue = @"ConsoleApplication1\ConsoleApplication1.csproj")]
        public string UniqueName { get; set; }
        [GridColumn("ProjectItemHeader_Name", ColumnsOrder.Name, true, ExampleValue = @"ConsoleApplication1")]
        public string Name { get; set; }
        [GridColumn("ProjectItemHeader_FullName", ColumnsOrder.FullName, false, ExampleValue = @"D:\Projects\ConsoleApplication1\ConsoleApplication1.csproj")]
        public string FullName { get; set; }
        [GridColumn("ProjectItemHeader_FullPath", ColumnsOrder.FullPath, false, ExampleValue = @"D:\Projects\ConsoleApplication1")]
        public string FullPath { get; set; }
        [GridColumn("ProjectItemHeader_Language", ColumnsOrder.Language, true, ExampleValue = @"C#")]
        public string Language { get; set; }
        [GridColumn("ProjectItemHeader_CommonType", ColumnsOrder.CommonType, false, ExampleValue = @"Windows")]
        public string CommonType { get; set; }
        [GridColumn("ProjectItemHeader_Configuration", ColumnsOrder.Configuration, true, ExampleValue = @"Debug")]
        public string Configuration { get; set; }
        [GridColumn("ProjectItemHeader_Platform", ColumnsOrder.Platform, true, ExampleValue = @"x86")]
        public string Platform { get; set; }
        [GridColumn("ProjectItemHeader_Framework", ColumnsOrder.Framework, false, ExampleValue = @"3.5")]
        public string Framework { get; set; }
        [GridColumn("ProjectItemHeader_FlavourType", ColumnsOrder.FlavourType, true, ExampleValue = @"Windows; VSTA")]
        public string FlavourType { get; set; }
        [GridColumn("ProjectItemHeader_MainFlavourType", ColumnsOrder.MainFlavourType, false, ExampleValue = @"VSTA")]
        public string MainFlavourType { get; set; }
        [GridColumn("ProjectItemHeader_OutputType", ColumnsOrder.OutputType, false, ExampleValue = @"Library")]
        public string OutputType { get; set; }
        [GridColumn("ProjectItemHeader_ExtenderNames", ColumnsOrder.ExtenderNames, false, ExampleValue = @"VST")]
        public string ExtenderNames { get; set; }
        [GridColumn("ProjectItemHeader_RootNamespace", ColumnsOrder.RootNamespace, false, ExampleValue = @"MyApplication")]
        public string RootNamespace { get; set; }
        [GridColumn("ProjectItemHeader_SolutionFolder", ColumnsOrder.SolutionFolder, false, ExampleValue = @"SolutionFolder1\SolutionFolder2")]
        public string SolutionFolder { get; set; }

        [GridColumn("ProjectItemHeader_StateBitmap", ColumnsOrder.StateBitmap, true, ImageKey = GridColumnAttribute.EmptyHeaderImageKey)]
        public ControlTemplate StateBitmap => _state.GetAssociatedContent();
        [GridColumn("ProjectItemHeader_BuildElapsedTime", ColumnsOrder.BuildElapsedTime, true, ValueStringFormat = @"mm\:ss", TimeSpanExampleValue = @"00:09:21.60")]
        public TimeSpan? BuildElapsedTime
        {
            get
            {
                if (_buildStartTime == null)
                {
                    return null;
                }
                if (_buildFinishTime == null)
                {
                    return DateTime.Now.Subtract(_buildStartTime.Value);
                }
                return _buildFinishTime.Value.Truncate(TimeSpan.FromSeconds(1)).Subtract(_buildStartTime.Value.Truncate(TimeSpan.FromSeconds(1)));
            }
        }

        private ProjectState _state;
        [GridColumn("ProjectItemHeader_State", ColumnsOrder.State, true, ExampleValue = @"BuildDone")]
        public ProjectState State
        {
            get => _state;
            set
            {
                SetProperty(ref _state, value);
                OnPropertyChanged(nameof(StateBitmap));
            }
        }
        private DateTime? _buildStartTime;
        [GridColumn("ProjectItemHeader_BuildStartTime", ColumnsOrder.BuildStartTime, true, ValueStringFormat = @"HH:mm:ss", DateTimeExampleValue = @"2012-07-27T20:06:12.3691406+06:00")]
        public DateTime? BuildStartTime
        {
            get => _buildStartTime;
            set
            {
                SetProperty(ref _buildStartTime, value);
                OnPropertyChanged(nameof(BuildStartTime));
                OnPropertyChanged(nameof(BuildElapsedTime));
            }
        }
        private DateTime? _buildFinishTime;
        [GridColumn("ProjectItemHeader_BuildFinishTime", ColumnsOrder.BuildFinishTime, true, ValueStringFormat = @"HH:mm:ss", DateTimeExampleValue = @"2012-07-27T20:06:12.3691406+06:00")]
        public DateTime? BuildFinishTime
        {
            get => _buildFinishTime;
            set
            {
                SetProperty(ref _buildFinishTime, value);
                OnPropertyChanged(nameof(BuildFinishTime));
                OnPropertyChanged(nameof(BuildElapsedTime));
            }
        }

        public ObservableCollection<ErrorItem> Errors { get; set; } = new ObservableCollection<ErrorItem>();

        public ObservableCollection<ErrorItem> Warnings { get; set; } = new ObservableCollection<ErrorItem>();

        public ObservableCollection<ErrorItem> Messages { get; set; } = new ObservableCollection<ErrorItem>();

        private int _errorsCount;
        [GridColumn("ProjectItemHeader_ErrorsCount", ColumnsOrder.ErrorsCount, true, ImageDictionaryUri = "Resources/ValueIndicator.Resources.xaml", ImageKey = "ErrorsIndicatorIcon", ExampleValue = 4)]
        public int ErrorsCount
        {
            get => _errorsCount;
            set => SetProperty(ref _errorsCount, value);
        }

        private int _warningsCount;
        [GridColumn("ProjectItemHeader_WarningsCount", ColumnsOrder.WarningsCount, true, ImageDictionaryUri = "Resources/ValueIndicator.Resources.xaml", ImageKey = "WarningsIndicatorIcon", ExampleValue = 1253)]
        public int WarningsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _warningsCount, value);
        }

        private int _messagesCount;
        [GridColumn("ProjectItemHeader_MessagesCount", ColumnsOrder.MessagesCount, false, ImageDictionaryUri = "Resources/ValueIndicator.Resources.xaml", ImageKey = "MessagesIndicatorIcon", ExampleValue = 2)]
        public int MessagesCount
        {
            get => _messagesCount;
            set => SetProperty(ref _messagesCount, value);
        }

        private int? _buildOrder;
        [GridColumn("ProjectItemHeader_BuildOrder", ColumnsOrder.BuildOrder, false, ImageDictionaryUri = ResourcesUri, ImageKey = "BuildOrder", Width = 23, ExampleValue = 4)]
        public int? BuildOrder
        {
            get => _buildOrder;
            set => SetProperty(ref _buildOrder, value);
        }

        public ProjectItem() => State = ProjectState.Pending;

        public void RaiseBuildElapsedTimeChanged() => OnPropertyChanged(nameof(BuildElapsedTime));

        public void AddErrorItem(ErrorItem errorItem)
        {
            switch (errorItem.Level)
            {
                case ErrorLevel.Message:
                    MessagesCount++;
                    break;
                case ErrorLevel.Warning:
                    WarningsCount++;
                    break;
                case ErrorLevel.Error:
                    ErrorsCount++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(errorItem));
            }
            if (errorItem.Level != ErrorLevel.Error)
            {
                return;
            }

            int errorNumber = Errors.Count + Warnings.Count + Messages.Count + 1;
            errorItem.Number = errorNumber;
            switch (errorItem.Level)
            {
                case ErrorLevel.Message:
                    Messages.Add(errorItem);
                    break;

                case ErrorLevel.Warning:
                    Warnings.Add(errorItem);
                    break;

                case ErrorLevel.Error:
                    Errors.Add(errorItem);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(errorItem));
            }
        }
    }
}
