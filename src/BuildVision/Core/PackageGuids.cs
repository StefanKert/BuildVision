// Guids.cs
// MUST match guids.h
using System;

namespace BuildVision.Core
{
    public static class PackageGuids
    {
        public const string GuidBuildVisionPackageString = "837c3c3b-8382-4839-9c9a-807b758a929f";
        public const string GuidBuildVisionCmdSetString = "caca30de-6571-483c-b8a1-b6a067964af1";
        public const string GuidBuildVisionToolWindowString = "e1d4b2b5-934e-4b0e-96cc-1c0449764501";

        public static readonly Guid GuidBuildVisionPackage = new Guid(GuidBuildVisionPackageString);
        public static readonly Guid GuidBuildVisionCmdSet = new Guid(GuidBuildVisionCmdSetString);
        public static readonly Guid GuidBuildVisionToolWindow = new Guid(GuidBuildVisionToolWindowString);
    }
}
