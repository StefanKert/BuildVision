using System.Linq;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;
using BuildVision.UI;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
{
    public class ErrorsIndicator : ValueIndicator
    {
        public override string Header => Resources.ErrorsIndicator_Header;
        public override string Description => Resources.ErrorsIndicator_Description;

        protected override int? GetValue( IBuildInfo buildContext)
        {
            return buildContext.BuildedProjects.Sum(proj => proj.ErrorsBox.ErrorsCount);
        }
    }
}