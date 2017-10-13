using System;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Contracts;

namespace BuildVision.UI.Models.Indicators
{
    public class WarningProjectsIndicator : ValueIndicator
    {
        public override string Header => Resources.WarningProjectsIndicator_Header;
        public override string Description => Resources.WarningProjectsIndicator_Description;

        protected override int? GetValue(IBuildInfo buildContext)
        {
            try
            {
                return buildContext.BuildedProjects.BuildWarningsCount;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}
