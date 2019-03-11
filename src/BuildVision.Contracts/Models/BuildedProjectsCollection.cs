using System.Collections.Generic;
using System.Linq;

using BuildVision.Contracts;
using BuildVision.UI.Models;

namespace BuildVision.UI.Contracts
{
    public class BuildedProjectsCollection : List<IProjectItem>
    {
        public int BuildSuccessCount =>  this.Count(p => p.Success == true && p.State != ProjectState.BuildWarning && p.State != ProjectState.UpToDate);
        public int BuildErrorCount => this.Count(p => p.Success == false);
        public int BuildWarningsCount => this.Count(p => p.State == ProjectState.BuildWarning);

        public int BuildUpToDateCount => this.Count(p => p.State == ProjectState.UpToDate);

        public bool BuildWithoutErrors => this.All(p => p.Success == null || p.Success == true);

        public IProjectItem this[IProjectItem pi]
        {
            get
            {
                return Find(p => p.UniqueName == pi.UniqueName && p.Configuration == pi.Configuration && p.Platform == pi.Platform);
            }
        }
    }
}
