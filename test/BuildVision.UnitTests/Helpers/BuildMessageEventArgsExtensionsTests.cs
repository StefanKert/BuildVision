using System;
using BuildVision.Helpers;
using BuildVision.Tool.Building;
using FluentAssertions;
using Microsoft.Build.Framework;
using Xunit;

namespace BuildVision.UnitTests
{
    public class BuildMessageEventArgsExtensionsTests
    {
        [Theory]
        [InlineData(LoggerVerbosity.Diagnostic, MessageImportance.High)]
        [InlineData(LoggerVerbosity.Diagnostic, MessageImportance.Normal)]
        [InlineData(LoggerVerbosity.Diagnostic, MessageImportance.Low)]
        [InlineData(LoggerVerbosity.Detailed, MessageImportance.High)]
        [InlineData(LoggerVerbosity.Detailed, MessageImportance.Normal)]
        [InlineData(LoggerVerbosity.Detailed, MessageImportance.Low)]
        [InlineData(LoggerVerbosity.Normal, MessageImportance.High)]
        [InlineData(LoggerVerbosity.Normal, MessageImportance.Normal)]
        [InlineData(LoggerVerbosity.Minimal, MessageImportance.High)]
        public void IsUserMessage_ShouldReturn_True_ForCombinations(LoggerVerbosity loggerVerbosity, MessageImportance messageImportance)
        {
            var buildMessageEventArgs = new BuildMessageEventArgs("", "", "", messageImportance);
            var buildOutputLogger = new BuildOutputLogger(Guid.NewGuid(), loggerVerbosity);

            var isUserMessage = BuildMessageEventArgsExtensions.IsUserMessage(buildMessageEventArgs, buildOutputLogger);
            isUserMessage.Should().BeTrue();
        }

        [Theory]
        [InlineData(LoggerVerbosity.Normal, MessageImportance.Low)]
        [InlineData(LoggerVerbosity.Minimal, MessageImportance.Normal)]
        [InlineData(LoggerVerbosity.Minimal, MessageImportance.Low)]
        [InlineData(LoggerVerbosity.Quiet, MessageImportance.High)]
        [InlineData(LoggerVerbosity.Quiet, MessageImportance.Normal)]
        [InlineData(LoggerVerbosity.Quiet, MessageImportance.Low)]
        public void IsUserMessage_ShouldReturn_False_If_ForCombinations(LoggerVerbosity loggerVerbosity, MessageImportance messageImportance)
        {
            var buildMessageEventArgs = new BuildMessageEventArgs("", "", "", messageImportance);
            var buildOutputLogger = new BuildOutputLogger(Guid.NewGuid(), loggerVerbosity);

            var isUserMessage = BuildMessageEventArgsExtensions.IsUserMessage(buildMessageEventArgs, buildOutputLogger);
            isUserMessage.Should().BeFalse();
        }
    }
}

