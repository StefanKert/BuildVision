using System;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Contracts;

namespace BuildVision.UI.Models.Indicators
{
    public class UpToDateProjectsIndicator : ValueIndicator
    {
        public override string Header => Resources.UpToDateProjectsIndicator_Header;
        public override string Description => Resources.UpToDateProjectsIndicator_Description;

        protected override int? GetValue(IBuildInfo buildContext)
        {
            try
            {
                return buildContext.BuildedProjects.BuildUpToDateCount;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}
