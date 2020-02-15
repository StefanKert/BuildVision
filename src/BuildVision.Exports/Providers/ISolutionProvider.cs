using System.Collections.Generic;
using BuildVision.Contracts.Models;
using BuildVision.UI.Models;

namespace BuildVision.Exports.Providers
{
    public interface ISolutionProvider
    {
        ISolutionModel GetSolutionModel();
        void ReloadSolution();
        IEnumerable<IProjectItem> GetProjects();
    }
}
