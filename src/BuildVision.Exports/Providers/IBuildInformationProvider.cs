using System.Collections.Generic;
using BuildVision.Contracts.Models;
using BuildVision.UI.Models;

namespace BuildVision.Exports.Providers
{
    public interface IBuildInformationProvider
    {
        void BuildFinished(IEnumerable<IProjectItem> projects, bool success, bool canceled);
        void BuildStarted(uint dwAction);
        void BuildUpdate();
        void ResetBuildInformationModel();
        IBuildInformationModel GetBuildInformationModel();
    }
}
