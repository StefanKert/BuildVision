using System.Linq;

using BuildVision.UI;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Contracts;
using System;

namespace BuildVision.UI.Models.Indicators
{
    public class SuccessProjectsIndicator : ValueIndicator
    {
        public override string Header => Resources.SuccessProjectsIndicator_Header;
        public override string Description => Resources.SuccessProjectsIndicator_Description;

        protected override int? GetValue(IBuildInfo buildContext)
        {
            try
            {
                return buildContext.BuildedProjects.BuildSuccessCount;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}