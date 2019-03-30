using System.Collections.Generic;

namespace BuildVision.UI.Settings.Models.Columns
{
    public class GridColumnSettingsCollection : List<GridColumnSettings>
    {
        public GridColumnSettings this[string propertyName] => Find(s => s.PropertyNameId == propertyName);
    }
}
