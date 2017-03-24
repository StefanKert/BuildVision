using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using AlekseyNagovitsyn.BuildVision.Tool.DataGrid;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Sorting;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings
{
    [DataContract]
    public class GridSettings
    {
        [DataMember(Name = "Columns")]
        private GridColumnSettingsCollection _columns;

        [DataMember(Name = "GroupName")]
        private string _groupPropertyName;

        [DataMember(Name = "GroupHeaderFormat")]
        private string _groupHeaderFormat;

        [DataMember(Name = "ShowColumnsHeader")]
        private bool _showColumnsHeader = true;

        [DataMember(Name = "Sort")]
        private SortDescription _sortDescription;

        [DataMember(Name = "CollapsedGroups")]
        private List<string> _collapsedGroups;

        private static string[] _groupHeaderFormatArgs;

        public GridColumnSettingsCollection Columns
        {
            get { return _columns ?? (_columns = new GridColumnSettingsCollection()); }
        }

        public IEnumerable<GridColumnSettings> SortableColumnsUIList
        {
            get 
            {
                yield return GridColumnSettings.Empty;
                foreach (GridColumnSettings column in Columns.Where(ColumnsManager.ColumnIsSortable))
                    yield return column;
            }
        }

        public IEnumerable<GridColumnSettings> GroupableColumnsUIList
        {
            get 
            {
                yield return GridColumnSettings.Empty;
                foreach (GridColumnSettings column in Columns.Where(ColumnsManager.ColumnIsGroupable))
                    yield return column;
            }
        }
        
        public string GroupPropertyName
        {
            get { return _groupPropertyName ?? (_groupPropertyName = string.Empty); }
            set { _groupPropertyName = value; }
        }

        /// <summary>
        /// User-friendly header format for groups.
        /// For example, "{title}: {value} - {count} items".
        /// Available arguments see in <see cref="GroupHeaderFormatArgs"/>.
        /// </summary>
        public string GroupHeaderFormat
        {
            get { return _groupHeaderFormat ?? (_groupHeaderFormat = "{title}: {value}"); }
            set { _groupHeaderFormat = value; }
        }

        public string GroupHeaderRawFormat
        {
            get { return ConvertGroupHeaderToRawFormat(GroupHeaderFormat); }
        }

        public static string[] GroupHeaderFormatArgs
        {
            get { return _groupHeaderFormatArgs ?? (_groupHeaderFormatArgs = new[] {"title", "value", "count"}); }
        }

        public bool ShowColumnsHeader
        {
            get { return _showColumnsHeader; }
            set { _showColumnsHeader = value; }
        }

        public SortDescription SortDescription
        {
            get { return _sortDescription ?? (_sortDescription = new SortDescription(SortOrder.Ascending, "BuildOrder")); }
            set { _sortDescription = value; }
        }

        public List<string> CollapsedGroups
        {
            get { return _collapsedGroups ?? (_collapsedGroups = new List<string>()); }
        }

        /// <summary>
        /// Converts user-friendly string format with <see cref="GroupHeaderFormatArgs"/> arguments
        /// into system format string (with {0},{1},... arguments).
        /// </summary>
        private static string ConvertGroupHeaderToRawFormat(string userFriendlyFormatString)
        {
            if (string.IsNullOrEmpty(userFriendlyFormatString))
                return string.Empty;

            string rawFormat = userFriendlyFormatString;
            for (int i = 0; i < GroupHeaderFormatArgs.Length; i++)
                rawFormat = rawFormat.Replace("{" + GroupHeaderFormatArgs[i], "{" + i);

            return rawFormat;
        }
    }
}