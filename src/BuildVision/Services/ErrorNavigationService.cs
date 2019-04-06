using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using BuildVision.Contracts;
using BuildVision.Exports.Services;
using BuildVision.Helpers;
using BuildVision.UI.Common.Logging;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace BuildVision.Services
{
    public class ErrorNavigationService : IErrorNavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public static bool BuildErrorNavigated { get; set; }

        [ImportingConstructor]
        public ErrorNavigationService(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateToErrorItem(ErrorItem errorItem)
        {
            try
            {
                var dte = _serviceProvider.GetService(typeof(DTE)) as DTE;
                var project = dte.Solution.GetProject(x => x.FileName == errorItem.ProjectFile);

                if (project == null)
                    throw new ArgumentNullException("project");

                if (errorItem == null)
                    throw new ArgumentNullException("errorItem");

                if (!errorItem.CanNavigateTo)
                    return;

                if (string.IsNullOrEmpty(errorItem.File))
                    return;

                string fullPath;

                // Check if path is absolute.
                if (Path.IsPathRooted(errorItem.File))
                {
                    // Foo VC++ projects errorItem.File contains full path.
                    fullPath = errorItem.File;
                }
                else
                {
                    var projectItemFile = project.FindProjectItem(errorItem.File);
                    if (projectItemFile == null)
                        return;

                    fullPath = projectItemFile.Properties.GetPropertyOrDefault<string>("FullPath");
                    if (fullPath == null)
                        throw new KeyNotFoundException("FullPath property not found.");
                }

                try
                {
                    var window = project.DTE.ItemOperations.OpenFile(fullPath, EnvDTE.Constants.vsViewKindAny);
                    if (window == null)
                        throw new NullReferenceException("Associated window is null reference.");

                    window.Activate();

                    var selection = (TextSelection)window.Selection;
                    selection.MoveToLineAndOffset(errorItem.LineNumber, errorItem.ColumnNumber);
                    selection.MoveToLineAndOffset(errorItem.EndLineNumber, errorItem.EndColumnNumber, true /*select text*/);
                }
                catch (Exception ex)
                {
                    var msg = string.Format("Navigate to error item exception (fullPath='{0}').", fullPath);
                    ex.Trace(msg);
                }
            }
            catch (Exception ex)
            {
                ex.Trace("Navigate to error item exception.");
            }

            BuildErrorNavigated = true;
        }
    }
}
