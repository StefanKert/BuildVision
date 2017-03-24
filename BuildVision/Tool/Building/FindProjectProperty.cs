namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    /// <summary>
    /// Defines how to find the project.
    /// </summary>
    public enum FindProjectProperty
    {
        /// <summary>
        /// By unique name.
        /// </summary>
        UniqueName,

        /// <summary>
        /// By full name.
        /// </summary>
        FullName,

        /// <summary>
        /// By <see cref="EnvDTE.Project"/> instance reference.
        /// </summary>
        ProjectObject,

        /// <summary>
        /// By UniqueName, Configuration, Platform properties (<see cref="Building.UniqueNameProjectDefinition"/>).
        /// </summary>
        UniqueNameProjectDefinition,

        /// <summary>
        /// By FullName, Configuration, Platform properties (<see cref="Building.FullNameProjectDefinition"/>).
        /// </summary>
        FullNameProjectDefinition
    }
}