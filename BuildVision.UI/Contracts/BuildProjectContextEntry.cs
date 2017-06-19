using BuildVision.UI.Models;
using System.Collections.Generic;

namespace BuildVision.UI.Contracts
{
    public class BuildProjectContextEntry
    {
        public int InstanceId { get; set; }

        public int ContextId { get; set; }

        public string FileName { get; set; }

        public IDictionary<string, string> Properties { get; set; }

        public ProjectItem ProjectItem { get; set; }

        public bool IsInvalid { get; set; }

        public BuildProjectContextEntry(int instanceId, int contextId, string fileName, IDictionary<string, string> properties)
        {
            InstanceId = instanceId;
            ContextId = contextId;
            FileName = fileName;
            Properties = properties;
        }
    }
}