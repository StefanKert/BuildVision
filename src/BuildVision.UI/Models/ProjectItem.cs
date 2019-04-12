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

        public ProjectItem()
        {
            State = ProjectState.Pending;
        }

        public bool IsBatchBuildProject { get; set; }

        private string _uniqueName;

        [GridColumn("ProjectItemHeader_UniqueName", ColumnsOrder.UniqueName, false, ExampleValue = @"ConsoleApplication1\ConsoleApplication1.csproj")]
        public string UniqueName
        {
            get => _uniqueName;
            set => SetProperty(ref _uniqueName, value);
        }

        private string _name;

        [GridColumn("ProjectItemHeader_Name", ColumnsOrder.Name, true, ExampleValue = @"ConsoleApplication1")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _fullName;

        [GridColumn("ProjectItemHeader_FullName", ColumnsOrder.FullName, false, ExampleValue = @"D:\Projects\ConsoleApplication1\ConsoleApplication1.csproj")]
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string _fullPath;

        [GridColumn("ProjectItemHeader_FullPath", ColumnsOrder.FullPath, false, ExampleValue = @"D:\Projects\ConsoleApplication1")]
        public string FullPath
        {
            get => _fullPath;
            set => SetProperty(ref _fullPath, value);
        }

        private string _language;

        [GridColumn("ProjectItemHeader_Language", ColumnsOrder.Language, true, ExampleValue = @"C#")]
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        private string _commonType;

        /// <remarks>
        /// See registered types in HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\[Version "11.0"]\Projects.
        /// </remarks>
        [GridColumn("ProjectItemHeader_CommonType", ColumnsOrder.CommonType, false, ExampleValue = @"Windows")]
        public string CommonType
        {
            get => _commonType;
            set => SetProperty(ref _commonType, value);
        }

        private string _configuration;

        [GridColumn("ProjectItemHeader_Configuration", ColumnsOrder.Configuration, true, ExampleValue = @"Debug")]
        public string Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }

        private string _platform;

        [GridColumn("ProjectItemHeader_Platform", ColumnsOrder.Platform, true, ExampleValue = @"x86")]
        public string Platform
        {
            get => _platform;
            set => SetProperty(ref _platform, value);
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

        [GridColumn("ProjectItemHeader_StateBitmap", ColumnsOrder.StateBitmap, true, ImageKey = GridColumnAttribute.EmptyHeaderImageKey)]
        public ControlTemplate StateBitmap => _state.GetAssociatedContent();

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

        [GridColumn("ProjectItemHeader_BuildElapsedTime", ColumnsOrder.BuildElapsedTime, true, ValueStringFormat = @"mm\:ss", TimeSpanExampleValue = @"00:09:21.60")]
        public TimeSpan? BuildElapsedTime
        {
            get
            {
                if (_buildStartTime == null)
                    return null;

                if (_buildFinishTime == null)
                    return DateTime.Now.Subtract(_buildStartTime.Value);

                return _buildFinishTime.Value.Truncate(TimeSpan.FromSeconds(1))
                  .Subtract(_buildStartTime.Value.Truncate(TimeSpan.FromSeconds(1)));
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

        private string _framework;

        [GridColumn("ProjectItemHeader_Framework", ColumnsOrder.Framework, false, ExampleValue = @"3.5")]
        public string Framework
        {
            get => _framework;
            set => SetProperty(ref _framework, value);
        }

        private string _flavourType;

        [GridColumn("ProjectItemHeader_FlavourType", ColumnsOrder.FlavourType, true, ExampleValue = @"Windows; VSTA")]
        public string FlavourType
        {
            get => _flavourType;
            set => SetProperty(ref _flavourType, value);
        }

        private string _mainFlavourType;

        [GridColumn("ProjectItemHeader_MainFlavourType", ColumnsOrder.MainFlavourType, false, ExampleValue = @"VSTA")]
        public string MainFlavourType
        {
            get => _mainFlavourType;
            set => SetProperty(ref _mainFlavourType, value);
        }

        private string _outputType;

        [GridColumn("ProjectItemHeader_OutputType", ColumnsOrder.OutputType, false, ExampleValue = @"Library")]
        public string OutputType
        {
            get => _outputType;
            set => SetProperty(ref _outputType, value);
        }

        private string _extenderNames;

        [GridColumn("ProjectItemHeader_ExtenderNames", ColumnsOrder.ExtenderNames, false, ExampleValue = @"VST")]
        public string ExtenderNames
        {
            get => _extenderNames;
            set => SetProperty(ref _extenderNames, value);
        }

        private int? _buildOrder;

        [GridColumn("ProjectItemHeader_BuildOrder", ColumnsOrder.BuildOrder, false, ImageDictionaryUri = ResourcesUri, ImageKey = "BuildOrder", Width = 23, ExampleValue = 4)]
        public int? BuildOrder
        {
            get => _buildOrder;
            set => SetProperty(ref _buildOrder, value);
        }

        private string _rootNamespace;

        [GridColumn("ProjectItemHeader_RootNamespace", ColumnsOrder.RootNamespace, false, ExampleValue = @"MyApplication")]
        public string RootNamespace
        {
            get => _rootNamespace;
            set => SetProperty(ref _rootNamespace, value);
        }

        private string _solutionFolder;

        [GridColumn("ProjectItemHeader_SolutionFolder", ColumnsOrder.SolutionFolder, false, ExampleValue = @"SolutionFolder1\SolutionFolder2")]
        public string SolutionFolder
        {
            get => _solutionFolder;
            set => SetProperty(ref _solutionFolder, value);
        }
        public bool Success { get; set; }

        public void RaiseBuildElapsedTimeChanged()
        {
            OnPropertyChanged(nameof(BuildElapsedTime));
        }
    }
}
