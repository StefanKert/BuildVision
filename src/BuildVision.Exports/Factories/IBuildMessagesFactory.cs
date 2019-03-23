using BuildVision.Contracts.Models;

namespace BuildVision.Exports.Factories
{
    public interface IBuildMessagesFactory
    {
        string GetBuildBeginExtraMessage(IBuildInformationModel buildInformationModel);
        string GetBuildBeginMajorMessage(IBuildInformationModel buildInformationModel);
        string GetBuildDoneMessage(IBuildInformationModel buildInformationModel);
    }
}
