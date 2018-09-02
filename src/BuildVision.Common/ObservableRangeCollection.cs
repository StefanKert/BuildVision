using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BuildVision.Common
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        public ObservableRangeCollection()
            : base()
        {
        }

        public ObservableRangeCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public ObservableRangeCollection(List<T> list)
            : base(list)
        {
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (T item in range)
            {
                Items.Add(item);
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Items)));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
