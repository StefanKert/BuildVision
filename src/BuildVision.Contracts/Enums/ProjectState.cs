namespace BuildVision.Contracts
{
    public enum ProjectState
    {
        None,
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
