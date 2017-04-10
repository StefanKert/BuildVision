using System;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
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