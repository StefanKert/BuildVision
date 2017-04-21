using System.Collections.Generic;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns
{
    public class GridColumnSettingsCollection : List<GridColumnSettings>
    {
        public GridColumnSettings this[string propertyName]
        {
            get { return Find(s => s.PropertyNameId == propertyName); }
        }
    }
}