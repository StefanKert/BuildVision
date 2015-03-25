using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
{
    public class SeparatorIndicator : ValueIndicator
    {
        protected override int? GetValue(DTE dte, BuildInfo buildContext)
        {
            return null;
        }

        public override string Header
        {
            get { return Resources.SeparatorIndicator_Header; }
        }

        public override string Description
        {
            get { return null; }
        }

        public override string StringValue
        {
            get { return string.Empty; }
        }

        public override double Width
        {
            get { return 20; }
        }
    }
}