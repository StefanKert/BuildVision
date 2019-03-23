namespace BuildVision.Contracts
{
    public enum BuildResultState
    {
        Unknown,

        RebuildDone,
        RebuildErrorDone,
        RebuildFailed,
        RebuildCancelled,

        BuildDone,
        BuildErrorDone,
        BuildFailed,
        BuildCancelled,

        CleanDone,
        CleanErrorDone,
        CleanFailed,
        CleanCancelled
    }
}
