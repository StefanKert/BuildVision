using System;
using BuildVision.Common;
using BuildVision.UI.Models;

namespace BuildVision.UI.Settings.Models
{
    public class BuildMessagesSettings : SettingsBase
    {
        private BuildMajorMessageFormat _majorMessageFormat = BuildMajorMessageFormat.Entire;
        private string _buildBeginMajorMessageStringFormat = "{0} ...";
        private string _buildDoneMajorMessageStringFormat = "{0}";
        private bool _showSolutionName = true;
        private bool _showProjectName = true;
        private string _dateTimeFormat = "HH:mm:ss";
        private string _timeSpanFormat = @"mm\:ss";
        private bool _showExtraMessage = true;
        private BuildExtraMessageFormat _extraMessageFormat = BuildExtraMessageFormat.Custom;
        private string _extraMessageStringFormat = " ({0})";
        private int _extraMessageDelay = 5;

        public BuildMajorMessageFormat MajorMessageFormat
        {
            get => _majorMessageFormat;
            set => SetProperty(ref _majorMessageFormat, value);
        }

        public BuildExtraMessageFormat ExtraMessageFormat
        {
            get => _extraMessageFormat;
            set => SetProperty(ref _extraMessageFormat, value);
        }

        public bool ShowSolutionName
        {
            get => _showSolutionName;
            set => SetProperty(ref _showSolutionName, value);
        }

        public bool ShowProjectName
        {
            get => _showProjectName;
            set => SetProperty(ref _showProjectName, value);
        }

        public bool ShowExtraMessage
        {
            get => _showExtraMessage;
            set => SetProperty(ref _showExtraMessage, value);
        }

        public int ExtraMessageDelay
        {
            get => _extraMessageDelay;
            set => SetProperty(ref _extraMessageDelay, value);
        }

        public string BuildBeginMajorMessageStringFormat
        {
            get => _buildBeginMajorMessageStringFormat;
            set
            {
                if (_buildBeginMajorMessageStringFormat != value)
                {
                    _buildBeginMajorMessageStringFormat = value;
                    OnPropertyChanged(nameof(BuildBeginMajorMessageStringFormat));

                    if (!value.Contains("{0}"))
                        throw new FormatException("Format must contain '{0}' argument.");

                    string tmp = string.Format(value, "test");
                }
            }
        }

        public string BuildDoneMajorMessageStringFormat
        {
            get => _buildDoneMajorMessageStringFormat;
            set
            {
                if (_buildDoneMajorMessageStringFormat != value)
                {
                    _buildDoneMajorMessageStringFormat = value;
                    OnPropertyChanged(nameof(BuildDoneMajorMessageStringFormat));

                    if (!value.Contains("{0}"))
                        throw new FormatException("Format must contain '{0}' argument.");

                    string tmp = string.Format(value, "test");
                }
            }
        }

        public string DateTimeFormat
        {
            get => _dateTimeFormat;
            set
            {
                if (_dateTimeFormat != value)
                {
                    _dateTimeFormat = value;
                    OnPropertyChanged(nameof(DateTimeFormat));

                    var tmp = DateTime.Now.ToString(value);
                }
            }
        }

        public string TimeSpanFormat
        {
            get => _timeSpanFormat;
            set
            {
                if (_timeSpanFormat != value)
                {
                    _timeSpanFormat = value;
                    OnPropertyChanged(nameof(TimeSpanFormat));

                    var tmp = TimeSpan.MaxValue.ToString(value);
                }
            }
        }

        public string ExtraMessageStringFormat
        {
            get => _extraMessageStringFormat;
            set
            {
                if (_extraMessageStringFormat != value)
                {
                    _extraMessageStringFormat = value;
                    OnPropertyChanged(nameof(ExtraMessageStringFormat));

                    if (!value.Contains("{0}"))
                        throw new FormatException("Format must contain '{0}' argument.");

                    var tmp = string.Format(value, "test");
                }
            }
        }
    }
}
