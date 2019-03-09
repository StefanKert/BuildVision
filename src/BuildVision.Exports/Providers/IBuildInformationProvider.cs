using System.Collections.Generic;
using BuildVision.Contracts.Models;

namespace BuildVision.Exports.Providers
{
    public interface IBuildInformationProvider
    {
        void BuildFinished(bool success, bool modified, bool canceled);
        void BuildStarted(uint dwAction);
        void BuildUpdate();
        IBuildInformationModel GetBuildInformationModel();
    }
}
