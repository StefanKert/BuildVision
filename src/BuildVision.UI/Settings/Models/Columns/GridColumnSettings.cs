using BuildVision.Contracts;

namespace BuildVision.UI.Settings.Models.Columns
{
    public class GridColumnSettings : BaseGridColumnSettingsAttribute
    {
        public static GridColumnSettings Empty { get; } = new GridColumnSettings
        {
            PropertyNameId = string.Empty,
            Header = Resources.NoneMenuItem
        };
    
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