namespace BuildVision.Contracts.Models
{
    public interface ISolutionModel
    {
        string FileName { get; set; }
        string FullName { get; set; }
        bool IsEmpty { get; set; }
        string Name { get; set; }
    }
}
