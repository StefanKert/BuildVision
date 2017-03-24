using System.Linq;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
{
    public class WarningsIndicator : ValueIndicator
    {
        protected override int? GetValue(DTE dte, BuildInfo buildContext)
        {
            return buildContext.BuildedProjects.Sum(proj => proj.ErrorsBox.WarningsCount);
        }

        public override string Header
        {
            get { return Resources.WarningsIndicator_Header; }
        }

        public override string Description
        {
            get { return Resources.WarningsIndicator_Description; }
        }
    }
}