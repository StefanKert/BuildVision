using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns
{
    [DataContract]
    public class GridColumnSettings : BaseGridColumnSettings
    {
        private static readonly GridColumnSettings _empty = new GridColumnSettings
                                                                {
                                                                    PropertyNameId = string.Empty,
                                                                    Header = Resources.NoneMenuItem
                                                                };

        public static GridColumnSettings Empty
        {
            get { return _empty; }
        }

        [DataMember]
        public string PropertyNameId { get; set; }

        private GridColumnSettings()
        {
        }

        public GridColumnSettings(
            string propertyNameId, 
            string header, 
            bool visible, 
            int displayIndex, 
            double width, 
            string valueStringFormat)
        {
            PropertyNameId = propertyNameId;
            Header = header;
            Visible = visible;
            DisplayIndex = displayIndex;
            Width = width;
            ValueStringFormat = valueStringFormat;
        }
    }
}