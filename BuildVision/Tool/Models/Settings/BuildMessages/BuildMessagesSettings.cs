using System;
using System.Runtime.Serialization;

using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildMessages
{
    [DataContract]
    public class BuildMessagesSettings : NotifyPropertyChangedBase
    {
        [DataMember(Name = "MajorMessageFormat")]
        private BuildMajorMessageFormat _majorMessageFormat = BuildMajorMessageFormat.Entire;

        [DataMember(Name = "BuildBeginMajorMessageStringFormat")]
        private string _buildBeginMajorMessageStringFormat = "{0} ...";

        [DataMember(Name = "BuildDoneMajorMessageStringFormat")]
        private string _buildDoneMajorMessageStringFormat = "{0}";

        [DataMember(Name = "ShowSolutionName")]
        private bool _showSolutionName = true;

        [DataMember(Name = "ShowProjectName")]
        private bool _showProjectName = true;

        [DataMember(Name = "DateTimeFormat")]
        private string _dateTimeFormat = "HH:mm:ss";

        [DataMember(Name = "TimeSpanFormat")]
        private string _timeSpanFormat = @"mm\:ss";

        [DataMember(Name = "ShowExtraMessage")]
        private bool _showExtraMessage = true;

        [DataMember(Name = "ExtraMessageFormat")]
        private BuildExtraMessageFormat _extraMessageFormat = BuildExtraMessageFormat.Custom;

        [DataMember(Name = "ExtraMessageStringFormat")]
        private string _extraMessageStringFormat = " ({0})";

        [DataMember(Name = "ExtraMessageDelay")]
        private int _extraMessageDelay = 5;

        public BuildMajorMessageFormat MajorMessageFormat
        {
            get { return _majorMessageFormat; }
            set 
            {
                if (_majorMessageFormat != value)
                {
                    _majorMessageFormat = value;
                    OnPropertyChanged("MajorMessageFormat");
                }
            }
        }

        public bool ShowSolutionName
        {
            get { return _showSolutionName; }
            set 
            {
                if (_showSolutionName != value)
                {
                    _showSolutionName = value;
                    OnPropertyChanged("ShowSolutionName");
                }
            }
        }

        public bool ShowProjectName
        {
            get { return _showProjectName; }
            set
            {
                if (_showProjectName != value)
                {
                    _showProjectName = value;
                    OnPropertyChanged("ShowProjectName");
                }
            }
        }

        public string DateTimeFormat
        {
            get { return _dateTimeFormat; }
            set 
            {
                if (_dateTimeFormat != value)
                {
                    _dateTimeFormat = value;
                    OnPropertyChanged("DateTimeFormat");

                    var tmp = DateTime.Now.ToString(value);
                }
            }
        }

        public string BuildBeginMajorMessageStringFormat
        {
            get { return _buildBeginMajorMessageStringFormat; }
            set
            {
                if (_buildBeginMajorMessageStringFormat != value)
                {
                    _buildBeginMajorMessageStringFormat = value;
                    OnPropertyChanged("BuildBeginMajorMessageStringFormat");

                    if (!value.Contains("{0}"))
                        throw new FormatException("Format must contain '{0}' argument.");

                    string tmp = string.Format(value, "test");
                }
            }
        }

        public string BuildDoneMajorMessageStringFormat
        {
            get { return _buildDoneMajorMessageStringFormat; }
            set
            {
                if (_buildDoneMajorMessageStringFormat != value)
                {
                    _buildDoneMajorMessageStringFormat = value;
                    OnPropertyChanged("BuildDoneMajorMessageStringFormat");

                    if (!value.Contains("{0}"))
                        throw new FormatException("Format must contain '{0}' argument.");

                    string tmp = string.Format(value, "test");
                }
            }
        }

        public bool ShowExtraMessage
        {
            get { return _showExtraMessage; }
            set 
            {
                if (_showExtraMessage != value)
                {
                    _showExtraMessage = value;
                    OnPropertyChanged("ShowExtraMessage");
                }
            }
        }

        public int ExtraMessageDelay
        {
            get { return _extraMessageDelay; }
            set 
            {
                if (_extraMessageDelay != value)
                {
                    _extraMessageDelay = value;
                    OnPropertyChanged("ExtraMessageDelay");
                }
            }
        }

        public BuildExtraMessageFormat ExtraMessageFormat
        {
            get { return _extraMessageFormat; }
            set 
            {
                if (_extraMessageFormat != value)
                {
                    _extraMessageFormat = value;
                    OnPropertyChanged("ExtraMessageFormat");
                }
            }
        }

        public string TimeSpanFormat
        {
            get { return _timeSpanFormat; }
            set
            {
                if (_timeSpanFormat != value)
                {
                    _timeSpanFormat = value;
                    OnPropertyChanged("TimeSpanFormat");

                    var tmp = TimeSpan.MaxValue.ToString(value);
                }
            }
        }

        public string ExtraMessageStringFormat
        {
            get { return _extraMessageStringFormat; }
            set
            {
                if (_extraMessageStringFormat != value)
                {
                    _extraMessageStringFormat = value;
                    OnPropertyChanged("ExtraMessageStringFormat");

                    if (!value.Contains("{0}"))
                        throw new FormatException("Format must contain '{0}' argument.");

                    var tmp = string.Format(value, "test");
                }
            }
        }
    }
}