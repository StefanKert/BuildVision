using System;
using System.Runtime.Serialization;

using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildMessages
{
    [DataContract]
    public class BuildMessagesSettings : BindableBase
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
                SetProperty(ref _majorMessageFormat, value);
            }
        }

        public bool ShowSolutionName
        {
            get { return _showSolutionName; }
            set 
            {
                SetProperty(ref _showSolutionName, value);
            }
        }

        public bool ShowProjectName
        {
            get { return _showProjectName; }
            set
            {
                SetProperty(ref _showProjectName, value);
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
                SetProperty(ref _showExtraMessage, value);
            }
        }

        public int ExtraMessageDelay
        {
            get { return _extraMessageDelay; }
            set 
            {
                SetProperty(ref _extraMessageDelay, value);
            }
        }

        public BuildExtraMessageFormat ExtraMessageFormat
        {
            get { return _extraMessageFormat; }
            set 
            {
                SetProperty(ref _extraMessageFormat, value);
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