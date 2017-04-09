using System.Linq;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
{
    public class ErrorsIndicator : ValueIndicator
    {
        protected override int? GetValue( BuildInfo buildContext)
        {
            return buildContext.BuildedProjects.Sum(proj => proj.ErrorsBox.ErrorsCount);
        }

        public override string Header
        {
            get { return Resources.ErrorsIndicator_Header; }
        }

        public override string Description
        {
            get { return Resources.ErrorsIndicator_Description; }
        }
    }
}