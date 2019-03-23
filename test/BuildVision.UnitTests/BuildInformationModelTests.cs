using BuildVision.Contracts;
using BuildVision.UI.Models;
using FluentAssertions;
using Xunit;

namespace BuildVision.UnitTests
{
    public class BuildInformationModelTests
    {
        [Fact]
        public void SetBuildState_ShouldTrigger_PropertyChanged_ForIsFinished()
        {
            var sut = new BuildInformationModel();
            using (var monitoredSut = sut.Monitor())
            {
                sut.CurrentBuildState = BuildState.ErrorDone;

                monitoredSut.Should().RaisePropertyChangeFor(x => x.CurrentBuildState);
                monitoredSut.Should().RaisePropertyChangeFor(x => x.IsFinished);
            }
        }

        [Fact]
        public void SetBuildAction_ShouldTrigger_PropertyChanged_ForStateIconKey()
        {
            var sut = new BuildInformationModel();
            using (var monitoredSut = sut.Monitor())
            {
                sut.BuildAction = BuildActions.BuildActionBuild;

                monitoredSut.Should().RaisePropertyChangeFor(x => x.BuildAction);
                monitoredSut.Should().RaisePropertyChangeFor(x => x.StateIconKey);
            }
        }

        [Fact]
        public void SetCurrentBuildState_ShouldTrigger_PropertyChanged_ForStateIconKey()
        {
            var sut = new BuildInformationModel();
            using (var monitoredSut = sut.Monitor())
            {
                sut.CurrentBuildState = BuildState.ErrorDone;

                monitoredSut.Should().RaisePropertyChangeFor(x => x.CurrentBuildState);
                monitoredSut.Should().RaisePropertyChangeFor(x => x.StateIconKey);
            }
        }

        [Fact]
        public void StateIconKey_ForNonStartedProject_ShouldBe_StandBy()
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.StateIconKey.Should().Be("StandBy");
        }

        [Theory]
        [InlineData(BuildActions.BuildActionRebuildAll, "Rebuild")]
        [InlineData(BuildActions.BuildActionBuild, "Build")]
        [InlineData(BuildActions.BuildActionClean, "Clean")]
        [InlineData(BuildActions.BuildActionDeploy, "StandBy")]
        public void StateIconKey_For_InProgressState_ShouldBe_RightStateDependingOnAction(BuildActions buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.InProgress;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }

        [Theory]
        [InlineData(BuildActions.BuildActionRebuildAll, "RebuildDone")]
        [InlineData(BuildActions.BuildActionBuild, "BuildDone")]
        [InlineData(BuildActions.BuildActionClean, "CleanDone")]
        [InlineData(BuildActions.BuildActionDeploy, "StandBy")]
        public void StateIconKey_For_InDoneState_ShouldBe_RightStateDependingOnAction(BuildActions buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.Done;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }

        [Theory]
        [InlineData(BuildActions.BuildActionRebuildAll, "RebuildFailed")]
        [InlineData(BuildActions.BuildActionBuild, "BuildFailed")]
        [InlineData(BuildActions.BuildActionClean, "CleanFailed")]
        [InlineData(BuildActions.BuildActionDeploy, "StandBy")]
        public void StateIconKey_For_InFailedState_ShouldBe_RightStateDependingOnAction(BuildActions buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.Failed;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }

        [Theory]
        [InlineData(BuildActions.BuildActionRebuildAll, "RebuildErrorDone")]
        [InlineData(BuildActions.BuildActionBuild, "BuildErrorDone")]
        [InlineData(BuildActions.BuildActionClean, "CleanErrorDone")]
        [InlineData(BuildActions.BuildActionDeploy, "StandBy")]
        public void StateIconKey_For_InErrorDoneState_ShouldBe_RightStateDependingOnAction(BuildActions buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.ErrorDone;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }

        [Theory]
        [InlineData(BuildActions.BuildActionRebuildAll, "RebuildCancelled")]
        [InlineData(BuildActions.BuildActionBuild, "BuildCancelled")]
        [InlineData(BuildActions.BuildActionClean, "CleanCancelled")]
        [InlineData(BuildActions.BuildActionDeploy, "StandBy")]
        public void StateIconKey_For_InCancelledState_ShouldBe_RightStateDependingOnAction(BuildActions buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.Cancelled;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }
    }
}
