using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

using Microsoft.VisualStudio.Shell;
using BuildVision.UI;
using BuildVision.Core;
using BuildVision.UI.ViewModels;
using System;
using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;
using Microsoft;
using BuildVision.Exports.Providers;
using BuildVision.Tool.Building;
using BuildVision.Exports.Services;
using BuildVision.Views.Settings;

namespace BuildVision.Tool
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the <see cref="ToolWindowPane"/> class provided from the MPF 
    /// in order to use its implementation of the <c>IVsUIElementPane</c> interface.
    /// </summary>
    [Guid(PackageGuids.GuidBuildVisionToolWindowString)]
    public sealed class BuildVisionPane : ToolWindowPane
    {
        private bool _controlCreatedSuccessfully;
        private readonly BuildVisionPaneViewModel _viewModel;

        JoinableTask<BuildVisionPaneViewModel> viewModelTask;
        public JoinableTaskFactory JoinableTaskFactory { get; private set; }
        public ControlView View { get; set; }

        public BuildVisionPane()
            :base(null)
        {
            Caption = Resources.ToolWindowTitle;
            BitmapResourceID = 301;
            BitmapIndex = 1;
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        protected override void Initialize()
        {
            // Using JoinableTaskFactory from parent AsyncPackage. That way if VS shuts down before this
            // work is done, we won't risk crashing due to arbitrary work going on in background threads.
            var asyncPackage = (AsyncPackage) Package;
            JoinableTaskFactory = asyncPackage.JoinableTaskFactory;

            viewModelTask = JoinableTaskFactory.RunAsync(() => InitializeAsync(asyncPackage));

        }

        public Task<BuildVisionPaneViewModel> GetViewModelAsync() => viewModelTask.JoinAsync();

        async Task<BuildVisionPaneViewModel> InitializeAsync(AsyncPackage asyncPackage)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                var sp = (AsyncPackage) Package;
                Assumes.Present(sp);
                var solutionProvider = await sp.GetServiceAsync(typeof(ISolutionProvider)) as ISolutionProvider;
                Assumes.Present(solutionProvider);
                var buildInformationProvider = await sp.GetServiceAsync(typeof(IBuildInformationProvider)) as IBuildInformationProvider;
                Assumes.Present(buildInformationProvider);
                var buildingProjectsProvider = await sp.GetServiceAsync(typeof(IBuildingProjectsProvider)) as IBuildingProjectsProvider;
                Assumes.Present(buildingProjectsProvider);
                var buildService = await sp.GetServiceAsync(typeof(IBuildService)) as IBuildService;
                Assumes.Present(buildService);
                var packageSettingsProvider = await sp.GetServiceAsync(typeof(IPackageSettingsProvider)) as IPackageSettingsProvider;
                Assumes.Present(packageSettingsProvider);

                var viewModel = new BuildVisionPaneViewModel(buildService, buildingProjectsProvider, buildInformationProvider, packageSettingsProvider, solutionProvider);
         
                View = CreateControlView();
                View.DataContext = viewModel;
                _controlCreatedSuccessfully = true;
                base.Initialize();
                return viewModel;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        protected override void OnClose()
        {
            if (_controlCreatedSuccessfully)
                SaveControlSettings();

            base.OnClose();
        }

        private ControlView CreateControlView()
        {
            var packageContext = (IPackageContext)Package;
            packageContext.ControlSettingsChanged += (settings) =>
            {
                _viewModel.OnControlSettingsChanged(settings); //, buildInfo => BuildMessages.GetBuildDoneMessage(viewModel.SolutionItem, buildInfo, viewModel.ControlSettings.BuildMessagesSettings));
            };
            var view = new ControlView();
            view.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/BuildVision.UI;component/Styles/ExtensionStyle.xaml")
            });
            return view;
        }

        private void SaveControlSettings()
        {
            var viewModel = GetViewModel(this);
            viewModel.SyncColumnSettings();

            var packageContext = (IPackageContext)Package;
            //packageContext.SaveSettings();
        }

        public static BuildVisionPaneViewModel GetViewModel(ToolWindowPane toolWindow)
        {
            var controlView = (ControlView)toolWindow.Content;
            return (BuildVisionPaneViewModel)controlView.DataContext;
        }
    }
}
