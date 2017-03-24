using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildProgress
{
    [DataContract]
    public class BuildProgressSettings
    {
        private bool _taskBarProgressEnabled = true;
        private ResetTaskBarItemInfoCondition _resetTaskBarProgressAfterBuildDone = ResetTaskBarItemInfoCondition.ByMouseClick;
        private int _resetTaskBarProgressDelay = 5000;

        [DataMember]
        public bool TaskBarProgressEnabled
        {
            get { return _taskBarProgressEnabled; }
            set { _taskBarProgressEnabled = value; }
        }

        [DataMember]
        public ResetTaskBarItemInfoCondition ResetTaskBarProgressAfterBuildDone
        {
            get { return _resetTaskBarProgressAfterBuildDone; }
            set { _resetTaskBarProgressAfterBuildDone = value; }
        }

        [DataMember]
        public int ResetTaskBarProgressDelay
        {
            get { return _resetTaskBarProgressDelay; }
            set { _resetTaskBarProgressDelay = value; }
        }
    }
}