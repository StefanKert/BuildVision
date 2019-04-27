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
                sut.BuildAction = BuildAction.Build;

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
        [InlineData(BuildAction.RebuildAll, "Rebuild")]
        [InlineData(BuildAction.Build, "Build")]
        [InlineData(BuildAction.Clean, "Clean")]
        [InlineData(BuildAction.Deploy, "StandBy")]
        public void StateIconKey_For_InProgressState_ShouldBe_RightStateDependingOnAction(BuildAction buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.InProgress;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }

        [Theory]
        [InlineData(BuildAction.RebuildAll, "RebuildDone")]
        [InlineData(BuildAction.Build, "BuildDone")]
        [InlineData(BuildAction.Clean, "CleanDone")]
        [InlineData(BuildAction.Deploy, "StandBy")]
        public void StateIconKey_For_InDoneState_ShouldBe_RightStateDependingOnAction(BuildAction buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.Done;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }

        [Theory]
        [InlineData(BuildAction.RebuildAll, "RebuildFailed")]
        [InlineData(BuildAction.Build, "BuildFailed")]
        [InlineData(BuildAction.Clean, "CleanFailed")]
        [InlineData(BuildAction.Deploy, "StandBy")]
        public void StateIconKey_For_InFailedState_ShouldBe_RightStateDependingOnAction(BuildAction buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.Failed;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }

        [Theory]
        [InlineData(BuildAction.RebuildAll, "RebuildErrorDone")]
        [InlineData(BuildAction.Build, "BuildErrorDone")]
        [InlineData(BuildAction.Clean, "CleanErrorDone")]
        [InlineData(BuildAction.Deploy, "StandBy")]
        public void StateIconKey_For_InErrorDoneState_ShouldBe_RightStateDependingOnAction(BuildAction buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.ErrorDone;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }

        [Theory]
        [InlineData(BuildAction.RebuildAll, "RebuildCancelled")]
        [InlineData(BuildAction.Build, "BuildCancelled")]
        [InlineData(BuildAction.Clean, "CleanCancelled")]
        [InlineData(BuildAction.Deploy, "StandBy")]
        public void StateIconKey_For_InCancelledState_ShouldBe_RightStateDependingOnAction(BuildAction buildAction, string expectedStateKey)
        {
            var buildInformationModel = new BuildInformationModel();
            buildInformationModel.CurrentBuildState = BuildState.Cancelled;
            buildInformationModel.BuildAction = buildAction;
            buildInformationModel.StateIconKey.Should().Be(expectedStateKey);
        }
    }
}
