using BuildVision.Contracts;
using BuildVision.UI;
using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns
{
    public class GridColumnSettings : BaseGridColumnSettings
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