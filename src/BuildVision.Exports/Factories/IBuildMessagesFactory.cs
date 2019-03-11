namespace BuildVision.Exports.Factories
{
    public interface IBuildMessagesFactory
    {
        string GetBuildBeginExtraMessage();
        string GetBuildBeginMajorMessage();
        string GetBuildDoneMessage();
    }
}
