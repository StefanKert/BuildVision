using System;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;
using BuildVision.UI;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
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