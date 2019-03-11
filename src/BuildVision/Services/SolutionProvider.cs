using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using BuildVision.Contracts.Models;
using BuildVision.Exports.Services;
using BuildVision.Helpers;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Models;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{
    [Export(typeof(IStatusBarNotificationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SolutionProvider : ISolutionProvider
    {
        private readonly IServiceProvider _serviceProvider;

        private Solution _solution;
        private SolutionModel _solutionModel;
        private DTE2 _dte;

        [ImportingConstructor]
        public SolutionProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ReloadSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _dte = _serviceProvider.GetService(typeof(DTE)) as DTE2;
            _solution = _dte.Solution;

            RefrehSolutionModel();
        }
        
        public ISolutionModel GetSolutionModel()
        {
            return _solutionModel;
        }

        private void RefrehSolutionModel()
        {
            if(_solutionModel == null)
                _solutionModel = new SolutionModel();

            try
            {
                if (_solution == null)
                {
                    _solutionModel.Name = Resources.GridCellNATextInBrackets;
                    _solutionModel.FullName = Resources.GridCellNATextInBrackets;
                    _solutionModel.IsEmpty = true;
                }
                else if (string.IsNullOrEmpty(_solution.FileName))
                {
                    if (_solution.Count != 0 /* projects count */)
                    {
                        var project = _solution.Item(1);
                        _solutionModel.Name = Path.GetFileNameWithoutExtension(project.FileName);
                        _solutionModel.FullName = project.FullName;
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
                    _solutionModel.Name = Path.GetFileNameWithoutExtension(_solution.FileName);
                    _solutionModel.FullName = _solution.FullName;
                    _solutionModel.IsEmpty = false;
                }
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                _solutionModel.Name = Resources.GridCellNATextInBrackets;
                _solutionModel.FullName = Resources.GridCellNATextInBrackets;
                _solutionModel.IsEmpty = true;
            }
        }

        public IEnumerable<IProjectItem> GetProjects()
        {
            if (_solution == null)
                ReloadSolution();
            return _solution.GetProjectItems();
        }
    }
}
