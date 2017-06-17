using System.Collections.Generic;
using System.Linq;
using BuildVision.Common;
using BuildVision.UI.DataGrid;
using BuildVision.UI.Settings.Models.Columns;
using BuildVision.UI.Models;
using BuildVision.UI.Settings.Models.Sorting;

namespace BuildVision.UI.Settings.Models
{
  public class GridSettings : SettingsBase
  {
    private GridColumnSettingsCollection _columns;
    
    private string _groupName;
    
    private string _groupHeaderFormat;

    private SortDescription _sort;

    private List<string> _collapsedGroups;

    private static string[] _groupHeaderFormatArgs;

    public GridColumnSettingsCollection Columns => _columns ?? (_columns = new GridColumnSettingsCollection());

    public IEnumerable<GridColumnSettings> SortableColumnsUIList
    {
      get
      {
        yield return GridColumnSettings.Empty;
        foreach (GridColumnSettings column in Columns.Where(x => ColumnsManager.ColumnIsSortable(x.PropertyNameId)))
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

    public string GroupName
    {
      get => _groupName ?? (_groupName = string.Empty);
      set => _groupName = value;
    }

    /// <summary>
    /// User-friendly header format for groups.
    /// For example, "{title}: {value} - {count} items".
    /// Available arguments see in <see cref="GroupHeaderFormatArgs"/>.
    /// </summary>
    public string GroupHeaderFormat
    {
      get => _groupHeaderFormat ?? (_groupHeaderFormat = "{title}: {value}");
      set => _groupHeaderFormat = value;
    }

    public string GroupHeaderRawFormat => ConvertGroupHeaderToRawFormat(GroupHeaderFormat);

    public static string[] GroupHeaderFormatArgs => _groupHeaderFormatArgs ?? (_groupHeaderFormatArgs = new[] { "title", "value", "count" });

    public bool ShowColumnsHeader { get; set; } = true;

    public SortDescription Sort
    {
      get => _sort ?? (_sort = new SortDescription(SortOrder.Ascending, "BuildOrder"));
      set => _sort = value;
    }

    public List<string> CollapsedGroups => _collapsedGroups ?? (_collapsedGroups = new List<string>());

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