using System.Runtime.Serialization;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.ToolWindow;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings
{
    [DataContract]
    public class WindowSettings
    {
        [DataMember]
        public WindowStateAction WindowActionOnBuildBegin { get; set; }

        [DataMember]
        public WindowStateAction WindowActionOnBuildSuccess { get; set; }

        [DataMember]
        public WindowStateAction WindowActionOnBuildError { get; set; }

        public WindowSettings()
        {
            WindowActionOnBuildBegin = new WindowStateAction(WindowState.Show);
            WindowActionOnBuildSuccess = new WindowStateAction(WindowState.Nothing);
            WindowActionOnBuildError = new WindowStateAction(WindowState.Show);
        }
    }
}