using System;
using Microsoft.VisualStudio.Shell;
using BuildVision.UI.Settings.Models;

using Task = System.Threading.Tasks.Task;

namespace BuildVision.Core
{
    public interface IPackageContext
    {
        event Action<ControlSettings> ControlSettingsChanged;
        void NotifyControlSettingsChanged();
        void ShowOptionPage(Type optionsPageType);
        Task ExecuteCommandAsync(string command);
    }
}
