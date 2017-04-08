using System;
using System.Runtime.Serialization;
using System.Windows.Controls;
using AlekseyNagovitsyn.BuildVision.Helpers;
using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns;
using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{

    [DataContract]
    public class ProjectItem : NotifyPropertyChangedBase
    {
        private const string ResourcesUri = @"Tool/Views/Resources/ProjectItem.Resources.xaml";

        public ProjectItem()
        {
            State = ProjectState.Pending;
        }

        public bool IsBatchBuildProject { get; set; }

        #region Properties

        [DataMember(Name = "UniqueName")]
        private string _uniqueName;

        [GridColumn("ProjectItemHeader_UniqueName", ColumnsOrder.UniqueName, false, ExampleValue = @"ConsoleApplication1\ConsoleApplication1.csproj")]
        public string UniqueName
        {
            get { return _uniqueName; }
            set 
            {
                if (_uniqueName != value)
                {
                    _uniqueName = value;
                    OnPropertyChanged("UniqueName");
                }
            }
        }

        [DataMember(Name = "Name")]
        private string _name;

        [GridColumn("ProjectItemHeader_Name", ColumnsOrder.Name, true, ExampleValue = @"ConsoleApplication1")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        [DataMember(Name = "FullName")]
        private string _fullName;

        [GridColumn("ProjectItemHeader_FullName", ColumnsOrder.FullName, false, ExampleValue = @"D:\Projects\ConsoleApplication1\ConsoleApplication1.csproj")]
        public string FullName
        {
            get { return _fullName; }
            set 
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged("FullName");
                }
            }
        }

        [DataMember(Name = "FullPath")]
        private string _fullPath;

        [GridColumn("ProjectItemHeader_FullPath", ColumnsOrder.FullPath, false, ExampleValue = @"D:\Projects\ConsoleApplication1")]
        public string FullPath
        {
            get { return _fullPath; }
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                    OnPropertyChanged("FullPath");
                }
            }
        }

        [DataMember(Name = "Language")]
        private string _language;

        [GridColumn("ProjectItemHeader_Language", ColumnsOrder.Language, true, ExampleValue = @"C#")]
        public string Language
        {
            get { return _language; }
            set
            {
                if (_language != value)
                {
                    _language = value;
                    OnPropertyChanged("Language");
                }
            }
        }

        [DataMember(Name = "CommonType")]
        private string _commonType;

        /// <remarks>
        /// See registered types in HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\[Version "11.0"]\Projects.
        /// </remarks>
        [GridColumn("ProjectItemHeader_CommonType", ColumnsOrder.CommonType, false, ExampleValue = @"Windows")]
        public string CommonType
        {
            get { return _commonType; }
            set
            {
                if (_commonType != value)
                {
                    _commonType = value;
                    OnPropertyChanged("CommonType");
                }
            }
        }

        [DataMember(Name = "Configuration")]
        private string _configuration;

        [GridColumn("ProjectItemHeader_Configuration", ColumnsOrder.Configuration, true, ExampleValue = @"Debug")]
        public string Configuration
        {
            get { return _configuration; }
            set 
            {
                if (_configuration != value)
                {
                    _configuration = value;
                    OnPropertyChanged("Configuration");
                }
            }
        }

        [DataMember(Name = "Platform")]
        private string _platform;

        [GridColumn("ProjectItemHeader_Platform", ColumnsOrder.Platform, true, ExampleValue = @"x86")]
        public string Platform
        {
            get { return _platform; }
            set
            {
                _platform = value;
                OnPropertyChanged("Platform");
            }
        }

        [DataMember(Name = "State")]
        private ProjectState _state;

        [GridColumn("ProjectItemHeader_State", ColumnsOrder.State, true, ExampleValue = @"BuildDone")]
        public ProjectState State
        {
            get { return _state; }
            set 
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                    OnPropertyChanged("StateBitmap");
                }
            }
        }

        [GridColumn("ProjectItemHeader_StateBitmap", ColumnsOrder.StateBitmap, true, ImageKey = GridColumnAttribute.EmptyHeaderImageKey)]
        [NonSortable, NonGroupable]
        public ControlTemplate StateBitmap
        {
            get { return _state.GetAssociatedContent(); }
        }

        [DataMember(Name = "BuildStartTime")]
        private DateTime? _buildStartTime;

        [GridColumn("ProjectItemHeader_BuildStartTime", ColumnsOrder.BuildStartTime, true, ValueStringFormat = @"HH:mm:ss", DateTimeExampleValue = @"2012-07-27T20:06:12.3691406+06:00")]
        [NonGroupable]
        public DateTime? BuildStartTime
        {
            get { return _buildStartTime; }
            set
            {
                if (_buildStartTime != value)
                {
                    _buildStartTime = value;
                    OnPropertyChanged("BuildStartTime");
                    OnPropertyChanged("BuildElapsedTime");
                }
            }
        }

        [DataMember(Name = "BuildFinishTime")]
        private DateTime? _buildFinishTime;

        [GridColumn("ProjectItemHeader_BuildFinishTime", ColumnsOrder.BuildFinishTime, true, ValueStringFormat = @"HH:mm:ss", DateTimeExampleValue = @"2012-07-27T20:06:12.3691406+06:00")]
        [NonGroupable]
        public DateTime? BuildFinishTime
        {
            get { return _buildFinishTime; }
            set 
            {
                if (_buildFinishTime != value)
                {
                    _buildFinishTime = value;
                    OnPropertyChanged("BuildFinishTime");
                    OnPropertyChanged("BuildElapsedTime");
                }
            }
        }

        [GridColumn("ProjectItemHeader_BuildElapsedTime", ColumnsOrder.BuildElapsedTime, true, ValueStringFormat = @"mm\:ss", TimeSpanExampleValue = @"00:09:21.60")]
        [NonGroupable]
        public TimeSpan? BuildElapsedTime
        {
            get
            {
                if (_buildStartTime == null)
                    return null;

                if (_buildFinishTime == null)
                    return DateTime.Now.Subtract(_buildStartTime.Value);

                return _buildFinishTime.Value.Subtract(_buildStartTime.Value);
            }
        }

        private ErrorsBox _errorsBox;

        public ErrorsBox ErrorsBox
        {
            get { return _errorsBox ?? (_errorsBox = new ErrorsBox()); }
            set
            {
                if (_errorsBox != value)
                {
                    _errorsBox = value;
                    OnPropertyChanged("ErrorsBox");
                    OnPropertyChanged("ErrorsCount");
                    OnPropertyChanged("WarningsCount");
                    OnPropertyChanged("MessagesCount");
                }
            }
        }

        [GridColumn("ProjectItemHeader_ErrorsCount", ColumnsOrder.ErrorsCount, true, ImageDictionaryUri = ValueIndicator.ResourcesUri, ImageKey = "ErrorsIndicator", ExampleValue = 4)]
        public int ErrorsCount
        {
            get { return ErrorsBox.ErrorsCount; }
        }

        [GridColumn("ProjectItemHeader_WarningsCount", ColumnsOrder.WarningsCount, true, ImageDictionaryUri = ValueIndicator.ResourcesUri, ImageKey = "WarningsIndicator", ExampleValue = 1253)]
        public int WarningsCount
        {
            get { return ErrorsBox.WarningsCount; }
        }

        [GridColumn("ProjectItemHeader_MessagesCount", ColumnsOrder.MessagesCount, false, ImageDictionaryUri = ValueIndicator.ResourcesUri, ImageKey = "MessagesIndicator", ExampleValue = 2)]
        public int MessagesCount
        {
            get { return ErrorsBox.MessagesCount; }
        }

        [DataMember(Name = "Framework")]
        private string _framework;

        [GridColumn("ProjectItemHeader_Framework", ColumnsOrder.Framework, false, ExampleValue = @"3.5")]
        public string Framework
        {
            get { return _framework; }
            set
            {
                if (_framework != value)
                {
                    _framework = value;
                    OnPropertyChanged("Framework");
                }
            }
        }

        [DataMember(Name = "FlavourType")]
        private string _flavourType;

        [GridColumn("ProjectItemHeader_FlavourType", ColumnsOrder.FlavourType, true, ExampleValue = @"Windows; VSTA")]
        public string FlavourType
        {
            get { return _flavourType; }
            set
            {
                if (_flavourType != value)
                {
                    _flavourType = value;
                    OnPropertyChanged("FlavourType");
                }
            }
        }

        [DataMember(Name = "MainFlavourType")]
        private string _mainFlavourType;

        [GridColumn("ProjectItemHeader_MainFlavourType", ColumnsOrder.MainFlavourType, false, ExampleValue = @"VSTA")]
        public string MainFlavourType
        {
            get { return _mainFlavourType; }
            set
            {
                if (_mainFlavourType != value)
                {
                    _mainFlavourType = value;
                    OnPropertyChanged("MainFlavourType");
                }
            }
        }

        [DataMember(Name = "OutputType")]
        private string _outputType;

        [GridColumn("ProjectItemHeader_OutputType", ColumnsOrder.OutputType, false, ExampleValue = @"Library")]
        public string OutputType
        {
            get { return _outputType; }
            set
            {
                if (_outputType != value)
                {
                    _outputType = value;
                    OnPropertyChanged("OutputType");
                }
            }
        }

        [DataMember(Name = "ExtenderNames")]
        private string _extenderNames;

        [GridColumn("ProjectItemHeader_ExtenderNames", ColumnsOrder.ExtenderNames, false, ExampleValue = @"VST")]
        public string ExtenderNames
        {
            get { return _extenderNames; }
            set
            {
                if (_extenderNames != value)
                {
                    _extenderNames = value;
                    OnPropertyChanged("ExtenderNames");
                }
            }
        }

        [DataMember(Name = "BuildOrder")]
        private int? _buildOrder;

        [GridColumn("ProjectItemHeader_BuildOrder", ColumnsOrder.BuildOrder, false, ImageDictionaryUri = ResourcesUri, ImageKey = "BuildOrder", Width = 23, ExampleValue = 4)]
        [NonGroupable]
        public int? BuildOrder
        {
            get { return _buildOrder; }
            set
            {
                if (_buildOrder != value)
                {
                    _buildOrder = value;
                    OnPropertyChanged("BuildOrder");
                }
            }
        }

        [DataMember(Name = "RootNamespace")]
        private string _rootNamespace;

        [GridColumn("ProjectItemHeader_RootNamespace", ColumnsOrder.RootNamespace, false, ExampleValue = @"MyApplication")]
        public string RootNamespace
        {
            get { return _rootNamespace; }
            set
            {
                if (_rootNamespace != value)
                {
                    _rootNamespace = value;
                    OnPropertyChanged("RootNamespace");
                }
            }
        }

        [DataMember(Name = "SolutionFolder")]
        private string _solutionFolder;

        [GridColumn("ProjectItemHeader_SolutionFolder", ColumnsOrder.SolutionFolder, false, ExampleValue = @"SolutionFolder1\SolutionFolder2")]
        public string SolutionFolder
        {
            get { return _solutionFolder; }
            set
            {
                if (_solutionFolder != value)
                {
                    _solutionFolder = value;
                    OnPropertyChanged("SolutionFolder");
                }
            }
        }

        #endregion

        public ProjectItem GetBatchBuildCopy(string configuration, string platform)
        {
            var pi = this.Clone();
            pi.Configuration = configuration;
            pi.Platform = platform;
            pi.ErrorsBox = new ErrorsBox();
            pi.IsBatchBuildProject = true;
            return pi;
        }

        public void UpdatePostBuildProperties(BuildedProject buildedProjectInfo)
        {
            if (buildedProjectInfo != null)
                ErrorsBox = buildedProjectInfo.ErrorsBox;
        }

        public void RaiseBuildElapsedTimeChanged()
        {
            OnPropertyChanged("BuildElapsedTime");
        }
    }
}