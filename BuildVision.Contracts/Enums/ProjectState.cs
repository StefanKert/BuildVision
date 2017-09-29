namespace BuildVision.Contracts
{
    public enum ProjectState
    {
        Pending,
        Skipped,
        BuildCancelled,
        Building,
        Cleaning,
        BuildDone,
        UpToDate,
        CleanDone,
        BuildError,
        CleanError,
        BuildWarning
    }
}