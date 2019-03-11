using System;
using BuildVision.Contracts;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;

namespace BuildVision.Exports
{
    public interface IBuildOutputLogger
    { 
        bool IsProjectUpToDate(IProjectItem projectItem);

        void Attach();

        event Action<BuildProjectContextEntry, object, ErrorLevel> OnErrorRaised;
    }
}
