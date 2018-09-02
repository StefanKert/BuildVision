using System;

using BuildVision.UI;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Contracts;

namespace BuildVision.UI.Models.Indicators
{
    public class ErrorProjectsIndicator : ValueIndicator
    {
        public override string Header => Resources.ErrorProjectsIndicator_Header;
        public override string Description => Resources.ErrorProjectsIndicator_Description;

        protected override int? GetValue(IBuildInfo buildContext)
        {
            try
            {
                return buildContext.BuildedProjects.BuildErrorCount;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}
