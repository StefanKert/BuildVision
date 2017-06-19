using System.Linq;

using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Contracts;

namespace BuildVision.UI.Models.Indicators
{
    public class WarningsIndicator : ValueIndicator
    {
        public override string Header => Resources.WarningsIndicator_Header;
        public override string Description => Resources.WarningsIndicator_Description;

        protected override int? GetValue(IBuildInfo buildContext)
        {
            return buildContext.BuildedProjects.Sum(proj => proj.ErrorsBox.WarningsCount);
        }
    }
}