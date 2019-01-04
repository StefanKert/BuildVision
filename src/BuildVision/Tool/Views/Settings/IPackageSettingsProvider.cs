using System.ComponentModel;
using BuildVision.UI.Settings.Models;

namespace BuildVision.Views.Settings
{
    public interface IPackageSettingsProvider : INotifyPropertyChanged
    {
        void Save();

        ControlSettings Settings { get; }
    }
}
