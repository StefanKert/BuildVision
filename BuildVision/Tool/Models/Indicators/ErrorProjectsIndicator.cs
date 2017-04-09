using System;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
{
    public class ErrorProjectsIndicator : ValueIndicator
    {
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

        public override string Header
        {
            get { return Resources.ErrorProjectsIndicator_Header; }
        }

        public override string Description
        {
            get { return Resources.ErrorProjectsIndicator_Description; }
        }
    }
}