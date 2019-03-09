namespace BuildVision.Contracts
{
    public enum BuildResultState
    {
        Unknown,

        RebuildSucceeded,
        RebuildSucceededWithErrors,
        RebuildFailed,
        RebuildCancelled,

        BuildSucceeded,
        BuildSucceededWithErrors,
        BuildFailed,
        BuildCancelled,

        CleanSucceeded,
        CleanSucceededWithErrors,
        CleanFailed,
        CleanCancelled
    }
}
