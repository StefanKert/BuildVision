using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace BuildVision.UI.Converters
{
    public class LogEntryTypeToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (EventLogEntryType)value;
            string uriString;
            switch (val)
            {
                case EventLogEntryType.Error:
                    uriString = "/BuildVision.UI;component/Images/exclamation.png";
                    break;

                case EventLogEntryType.Warning:
                case EventLogEntryType.FailureAudit:
                    uriString = "/BuildVision.UI;component/Images/warning.png";
                    break;

                case EventLogEntryType.Information:
                case EventLogEntryType.SuccessAudit:
                    uriString = "/BuildVision.UI;component/Images/information.png";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return uriString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}