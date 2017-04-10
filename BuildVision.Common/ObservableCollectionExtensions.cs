using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
        {
            foreach (T item in range)
            {
                collection.Add(item);
            }
        }
    }
}