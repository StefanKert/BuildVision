using System.Collections.Generic;
using System.Linq;

using BuildVision.Contracts;
using BuildVision.UI.Models;

namespace BuildVision.UI.Contracts
{
    // TODO: thread-safety.
    public class BuildedProjectsCollection : List<BuildedProject>
    {
        public int BuildSuccessCount =>  this.Count(p => p.Success == true && p.ProjectState != ProjectState.BuildWarning && p.ProjectState != ProjectState.UpToDate);
        public int BuildErrorCount => this.Count(p => p.Success == false);
        public int BuildWarningsCount => this.Count(p => p.ProjectState == ProjectState.BuildWarning);

        public int BuildUpToDateCount => this.Count(p => p.ProjectState == ProjectState.UpToDate);

        public bool BuildWithoutErrors => this.All(p => p.Success == null || p.Success == true);

        /// <summary>
        /// Get <see cref="BuildedProject"/> by <see cref="ProjectItem.UniqueName"/>. 
        /// If not exists, it has been created and added to the collection.
        /// </summary>
        public BuildedProject this[ProjectItem pi]
        {
            get
            {
                var proj = Find(p => p.UniqueName == pi.UniqueName && p.Configuration == pi.Configuration && p.Platform == pi.Platform);
                if (proj == null)
                {
                    proj = new BuildedProject(pi.UniqueName, pi.FullName, pi.Configuration, pi.Platform);
                    Add(proj);
                }

                return proj;
            }
        }
    }
}
