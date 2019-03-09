using System.Collections.Generic;
using BuildVision.Contracts.Models;
using BuildVision.UI.Models;

namespace BuildVision.Core
{
    public interface ISolutionProvider
    {
        ISolutionModel GetSolutionModel();
        void ReloadSolution();
        IEnumerable<ProjectItem> GetProjects();
    }
}
