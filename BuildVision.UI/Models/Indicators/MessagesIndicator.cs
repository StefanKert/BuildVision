using System.Linq;

using BuildVision.UI;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Contracts;

namespace BuildVision.UI.Models.Indicators
{
    public class MessagesIndicator : ValueIndicator
    {
        public override string Header => Resources.MessagesIndicator_Header;
        public override string Description => Resources.MessagesIndicator_Description;

        protected override int? GetValue(IBuildInfo buildContext)
        {
            return buildContext.BuildedProjects.Sum(proj => proj.ErrorsBox.MessagesCount);
        }
    }
}