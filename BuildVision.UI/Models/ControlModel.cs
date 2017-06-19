using BuildVision.UI.Extensions;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models.Indicators.Core;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace BuildVision.UI.Models
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
            TextCurrentState = Resources.BuildDoneText_BuildNotStarted;
            ImageCurrentState = VectorResources.TryGet(BuildImages.BuildActionResourcesUri, "StandBy");
            ImageCurrentStateResult = null;
        }
    }
}
