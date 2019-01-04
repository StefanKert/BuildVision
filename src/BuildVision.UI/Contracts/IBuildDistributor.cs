using BuildVision.Contracts;
using System;
using System.Threading.Tasks;

namespace BuildVision.UI.Contracts
{
    public interface IBuildDistributor
    {
        Task CancelBuildAsync();
    }
}
