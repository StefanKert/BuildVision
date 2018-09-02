using System.Linq;

using BuildVision.UI;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Contracts;

namespace BuildVision.UI.Models.Indicators
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