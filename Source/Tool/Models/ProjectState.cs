namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    public enum ProjectState
    {
        [ProjectStateStandBy]
        [AssociatedProjectStateVectorIcon("Pending")]
        Pending,

        [ProjectStateStandBy]
        [AssociatedProjectStateVectorIcon("Skipped")]
        Skipped,

        [ProjectStateStandBy]
        [AssociatedProjectStateVectorIcon("Skipped")]
        BuildCancelled,

        [ProjectStateProgress]
        [AssociatedProjectStateVectorIcon("Building")]
        Building,

        [ProjectStateProgress]
        [AssociatedProjectStateVectorIcon("Building")]
        Cleaning,

        [ProjectStateSuccess]
        [AssociatedProjectStateVectorIcon("BuildDone")]
        BuildDone,

        [ProjectStateSuccess]
        [AssociatedProjectStateVectorIcon("BuildDone")]
        UpToDate,

        [ProjectStateSuccess]
        [AssociatedProjectStateVectorIcon("BuildDone")]
        CleanDone,

        [ProjectStateError]
        [AssociatedProjectStateVectorIcon("BuildError")]
        BuildError,

        [ProjectStateError]
        [AssociatedProjectStateVectorIcon("BuildError")]
        CleanError
    }
}