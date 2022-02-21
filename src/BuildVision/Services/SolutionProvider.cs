using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using BuildVision.Common.Logging;
using BuildVision.Contracts.Models;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Extensions;
using BuildVision.Helpers;
using BuildVision.UI;
using BuildVision.UI.Models;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Serilog;

namespace BuildVision.Core
{
    [Export(typeof(IStatusBarNotificationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SolutionProvider : ISolutionProvider
    {
        private ILogger _logger = LogManager.ForContext<SolutionProvider>();
        private readonly IServiceProvider _serviceProvider;
        private IVsSolution _vsSolution;
        private SolutionModel _solutionModel;

        [ImportingConstructor]
        public SolutionProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ReloadSolution()
        {
            _vsSolution = _serviceProvider.GetService<SVsSolution>() as IVsSolution;
            RefrehSolutionModel();
        }

        public ISolutionModel GetSolutionModel()
        {
            return _solutionModel;
        }

        private void RefrehSolutionModel()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_solutionModel == null)
            {
                _solutionModel = new SolutionModel();
            }

            try
            {
                if (_vsSolution == null)
                {
                    _solutionModel.Name = Resources.GridCellNATextInBrackets;
                    _solutionModel.FullName = Resources.GridCellNATextInBrackets;
                    _solutionModel.IsEmpty = true;
                    return;
                }

                _vsSolution.GetProperty((int)__VSPROPID.VSPROPID_SolutionFileName, out object fileName);
                _vsSolution.GetProperty((int)__VSPROPID.VSPROPID_SolutionBaseName, out object fullName);
                if (string.IsNullOrEmpty((string)fileName))
                {
                    var solutionItems = _vsSolution.ToSolutionItemAsync().Result;
                    if (solutionItems.Children.Any())
                    {
                        var project = solutionItems.Children.First();
                        _solutionModel.Name = Path.GetFileNameWithoutExtension(project.FullPath);
                        _solutionModel.FullName = project.Name;
                        _solutionModel.IsEmpty = false;
                    }
                    else
                    {
                        _solutionModel.Name = Resources.GridCellNATextInBrackets;
                        _solutionModel.FullName = Resources.GridCellNATextInBrackets;
                        _solutionModel.IsEmpty = true;
                    }
                }
                else
                {
                    _solutionModel.Name = Path.GetFileNameWithoutExtension((string)fileName);
                    _solutionModel.FullName = (string)fullName;
                    _solutionModel.IsEmpty = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to refresh solutionmodel.");
                _solutionModel.Name = Resources.GridCellNATextInBrackets;
                _solutionModel.FullName = Resources.GridCellNATextInBrackets;
                _solutionModel.IsEmpty = true;
            }
        }

        public IEnumerable<IProjectItem> GetProjects()
        {
            if (_vsSolution == null)
            {
                ReloadSolution();
            }

            return _vsSolution.GetProjectItems();
        }
    }
}
