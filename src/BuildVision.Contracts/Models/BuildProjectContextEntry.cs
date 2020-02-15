using BuildVision.UI.Models;
using System.Collections.Generic;

namespace BuildVision.UI.Contracts
{
    public class BuildProjectContextEntry
    {
        public string ProjectFile { get; set; }

        public IDictionary<string, string> Properties { get; }

        public IProjectItem ProjectItem { get; set; }

        public bool IsInvalid { get; set; }

        public BuildProjectContextEntry(string fileName, IDictionary<string, string> properties)
        {
            ProjectFile = fileName;
            Properties = properties;
        }
    }
}
