using System;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class ProjectStateAttribute : Attribute { }

    public class ProjectStateSuccessAttribute : ProjectStateAttribute { }
    public class ProjectStateErrorAttribute : ProjectStateAttribute { }
    public class ProjectStateProgressAttribute : ProjectStateAttribute { }
    public class ProjectStateStandByAttribute : ProjectStateAttribute { }
}