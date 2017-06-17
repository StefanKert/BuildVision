using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;
using BuildVision.UI;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
{
    public class SeparatorIndicator : ValueIndicator
    {
        public override string Header => Resources.SeparatorIndicator_Header;
        public override string Description => null;
        public override string StringValue => string.Empty;
        public override double Width => 20;

        protected override int? GetValue(IBuildInfo buildContext)
        {
            return null;
        }
    }
}