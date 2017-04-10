using System.Linq;

using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators
{
    public class MessagesIndicator : ValueIndicator
    {
        public override string Header => Resources.MessagesIndicator_Header;
        public override string Description => Resources.MessagesIndicator_Description;

        protected override int? GetValue(IBuildInfo buildContext)
        {
            return buildContext.BuildedProjects.Sum(proj => proj.ErrorsBox.MessagesCount);
        }
    }
}