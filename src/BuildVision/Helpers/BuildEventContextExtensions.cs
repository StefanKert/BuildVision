using Microsoft.Build.Framework;

namespace BuildVision.Helpers
{
    public static class BuildEventContextExtensions
    {
        public static bool IsBuildEventContextInvalid(this BuildEventContext bec)
        {
            return (bec == null
                           || bec == BuildEventContext.Invalid
                           || bec.ProjectContextId == BuildEventContext.InvalidProjectContextId
                           || bec.ProjectInstanceId == BuildEventContext.InvalidProjectInstanceId);
        }
    }
}
