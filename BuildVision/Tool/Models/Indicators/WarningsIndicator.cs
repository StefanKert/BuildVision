using System.Linq;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
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