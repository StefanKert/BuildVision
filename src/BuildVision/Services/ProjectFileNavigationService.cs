using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildVision.Contracts;
using BuildVision.Exports.Providers;
using BuildVision.Helpers;
using BuildVision.UI.Common.Logging;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace BuildVision.Services
{
    public class ProjectFileNavigationService : ErrorNavigationService, IProjectFileNavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBuildingProjectsProvider _buildingProjectsProvider;

        [ImportingConstructor]
        public ProjectFileNavigationService(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import(typeof(IBuildingProjectsProvider))] IBuildingProjectsProvider buildingProjectsProvider) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _buildingProjectsProvider = buildingProjectsProvider;
        }

        public void NavigateToFirstError()
        {
            var projects = _buildingProjectsProvider.GetBuildingProjects();
            foreach(var project in projects){
                foreach(var error in project.ErrorsBox)
                {
                    NavigateToErrorItem(error);
                    if (BuildErrorNavigated)
                        return;
                }
            }
        }
    }
}
