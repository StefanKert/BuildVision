using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildVision.UI.Models;

namespace BuildVision.UI
{
    public interface IBuildVisionPaneViewModel
    {
        SolutionItem SolutionItem { get; }

        ObservableCollection<ProjectItem> ProjectsList { get;}
    }
}
