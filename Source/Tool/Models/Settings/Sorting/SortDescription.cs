using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Sorting
{
    [DataContract]
    public class SortDescription
    {
        [DataMember(Name = "Order")]
        public SortOrder SortOrder { get; set; }

        [DataMember(Name = "Property")]
        public string SortPropertyName { get; set; }

        public SortDescription(SortOrder sortOrder, string sortPropertyName)
        {
            SortOrder = sortOrder;
            SortPropertyName = sortPropertyName;
        }
    }
}