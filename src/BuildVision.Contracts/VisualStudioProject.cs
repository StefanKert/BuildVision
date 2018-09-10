using System;

namespace BuildVision.Contracts
{
    public class VisualStudioProject
    {
        public bool IsBatchBuildProject { get; set; }
        public string UniqueName { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string FullPath { get; set; }
        public string Language { get; set; }
        public string CommonType { get; set; }
        public string Configuration { get; set; }
        public string Platform { get; set; }
        public string Framework { get; set; }
        public string FlavourType { get; set; }
        public string MainFlavourType { get; set; }
        public string OutputType { get; set; }
        public string ExtenderNames { get; set; }
        public int? BuildOrder { get; set; }
        public string RootNamespace { get; set; }
        public string SolutionFolder { get; set; }
        public ProjectState State { get; set; }
        public DateTime? BuildStartTime { get; set; }
        public DateTime? BuildFinishTime { get; set; }
        public TimeSpan? BuildElapsedTime { get; set; }
    }
}