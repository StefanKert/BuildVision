using System;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
{
    public class SuccessProjectsIndicator : ValueIndicator
    {
        protected override int? GetValue(DTE dte, BuildInfo buildContext)
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

        public override string Header
        {
            get { return Resources.SuccessProjectsIndicator_Header; }
        }

        public override string Description
        {
            get { return Resources.SuccessProjectsIndicator_Description; }
        }
    }
}