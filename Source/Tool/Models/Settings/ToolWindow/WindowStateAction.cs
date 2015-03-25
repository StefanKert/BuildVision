using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.ToolWindow
{
    [DataContract]
    public class WindowStateAction
    {
        [DataMember]
        public WindowState State { get; set; }

        public WindowStateAction(WindowState state)
        {
            State = state;
        }
    }
}