namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Sorting
{
  public class SortDescription
  {
    public SortOrder Order { get; set; }
    
    public string Property { get; set; }

    public SortDescription(SortOrder order, string property)
    {
      Order = order;
      Property = property;
    }

    public SortDescription() { }
  }
}