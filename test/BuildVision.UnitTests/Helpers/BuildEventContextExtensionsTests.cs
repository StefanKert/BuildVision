using BuildVision.Helpers;
using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace BuildVision.UnitTests
{
    public class BuildEventContextExtensionsTests
    {
        [Fact]
        public void IsBuildEventContextInvalid_ShouldReturn_False_IfValid()
        {
            var buildEventContext = new BuildEventContext(0, projectInstanceId: 0, projectContextId: 0, targetId: 0, taskId: 0);

            var result = BuildEventContextExtensions.IsBuildEventContextInvalid(buildEventContext);
            result.Should().BeFalse();
        }

        [Fact]
        public void IsBuildEventContextInvalid_ShouldReturn_True_IfNull()
        {
            var result = BuildEventContextExtensions.IsBuildEventContextInvalid(null);
            result.Should().BeTrue();
        }

        [Fact]
        public void IsBuildEventContextInvalid_ShouldReturn_True_If_BuildEventContextInvalid()
        {
            var result = BuildEventContextExtensions.IsBuildEventContextInvalid(BuildEventContext.Invalid);
            result.Should().BeTrue();
        }

        [Fact]
        public void IsBuildEventContextInvalid_ShouldReturn_True_If_ProjectContextId_MatchesBuildEventContext_InvalidProjectContextId()
        {
            var buildEventContext = new BuildEventContext(0, projectInstanceId: 0, projectContextId: BuildEventContext.InvalidProjectContextId, targetId: 0, taskId: 0);

            var result = BuildEventContextExtensions.IsBuildEventContextInvalid(buildEventContext);
            result.Should().BeTrue();
        }

        [Fact]
        public void IsBuildEventContextInvalid_ShouldReturn_True_If_ProjectInstanceId_MatchesBuildEventContext_InvalidProjectInstanceId()
        {
            var buildEventContext = new BuildEventContext(0, projectInstanceId: BuildEventContext.InvalidProjectInstanceId, projectContextId: 0, targetId: 0, taskId: 0);

            var result = BuildEventContextExtensions.IsBuildEventContextInvalid(buildEventContext);
            result.Should().BeTrue();
        }
    }
}
