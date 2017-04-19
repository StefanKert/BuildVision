using System.Collections.ObjectModel;
using System.Windows.Controls;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    public class ControlModel
    {
        public ProjectItem CurrentProject { get; set; }

        public ControlTemplate ImageCurrentState { get; set; }

        public ControlTemplate ImageCurrentStateResult { get; set; }

        public string TextCurrentState { get; set; }

        public SolutionItem SolutionItem { get; set; }

        public ObservableCollection<ValueIndicator> ValueIndicators { get; set; }

        public ControlModel()
        {
            ValueIndicators = ValueIndicatorsFactory.CreateCollection();
            SolutionItem = new SolutionItem();
            TextCurrentState = BuildMessages.GetBuildDoneMessage(null, null, null);
            ImageCurrentState = BuildImages.GetBuildDoneImage(null, null, out ControlTemplate stateImage);
            ImageCurrentStateResult = stateImage;
        }
    }
}
