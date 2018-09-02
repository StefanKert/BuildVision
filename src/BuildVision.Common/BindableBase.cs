using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BuildVision.Common
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        public virtual bool SetProperty<T>(Func<T> storage, Action<T> set, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage(), value))
                return false;

            set(value);
            OnPropertyChanged(propertyName);

            return true;
        }

        public virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
