// Guids.cs
// MUST match guids.h
using System;

namespace BuildVision.Core
{
    static class GuidList
    {
        public const string guidBuildVisionPkgString = "837c3c3b-8382-4839-9c9a-807b758a929f";
        public const string guidBuildVisionCmdSetString = "caca30de-6571-483c-b8a1-b6a067964af1";
        public const string guidToolWindowPersistanceString = "e1d4b2b5-934e-4b0e-96cc-1c0449764501";

        public static readonly Guid guidBuildVisionCmdSet = new Guid(guidBuildVisionCmdSetString);
    };
}