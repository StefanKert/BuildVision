using System.Collections.Generic;
using System.Linq;

using AlekseyNagovitsyn.BuildVision.Tool.Models;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    // TODO: thread-safety.
    public class BuildedProjectsCollection : List<BuildedProject>
    {
        public int BuildSuccessCount
        {
            get { return this.Count(p => p.Success == true); }
        }

        public int BuildErrorCount
        {
            get { return this.Count(p => p.Success == false); }
        }

        public bool BuildWithoutErrors
        {
            get { return this.All(p => p.Success == null || p.Success == true); }
        }

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