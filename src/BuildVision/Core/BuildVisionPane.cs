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
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Controls;
using BuildVision.UI.Settings.Models;
using BuildVision.UI.Settings.Models.Columns;

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

        JoinableTask<BuildVisionPaneViewModel> viewModelTask;
        private ContentPresenter _contentPresenter;
        private IPackageSettingsProvider _packageSettingsProvider;

        public JoinableTaskFactory JoinableTaskFactory { get; private set; }
        public ControlView View
        {
            get => _contentPresenter.Content as ControlView;
            set => _contentPresenter.Content = value;
        }

        public BuildVisionPane()
            :base(null)
        {
            Caption = Resources.ToolWindowTitle;
            BitmapResourceID = 301;
            BitmapIndex = 1;

            Content = _contentPresenter = new ContentPresenter();
            //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        protected override void Initialize()
        {
            // Using JoinableTaskFactory from parent AsyncPackage. That way if VS shuts down before this
            // work is done, we won't risk crashing due to arbitrary work going on in background threads.
            var asyncPackage = (AsyncPackage) Package;
            JoinableTaskFactory = asyncPackage.JoinableTaskFactory;

            viewModelTask = JoinableTaskFactory.RunAsync(() => InitializeAsync(asyncPackage));

            base.Initialize();
        }

        public Task<BuildVisionPaneViewModel> GetViewModelAsync() => viewModelTask.JoinAsync();

        async Task<BuildVisionPaneViewModel> InitializeAsync(AsyncPackage asyncPackage)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _packageSettingsProvider = await asyncPackage.GetServiceAsync(typeof(IPackageSettingsProvider)) as IPackageSettingsProvider;
            Assumes.Present(_packageSettingsProvider);
            var solutionProvider = await asyncPackage.GetServiceAsync(typeof(ISolutionProvider)) as ISolutionProvider;
            Assumes.Present(solutionProvider);
            var buildInformationProvider = await asyncPackage.GetServiceAsync(typeof(IBuildInformationProvider)) as IBuildInformationProvider;
            Assumes.Present(buildInformationProvider);
            var buildService = await asyncPackage.GetServiceAsync(typeof(IBuildService)) as IBuildService;
            Assumes.Present(buildService);
            var errorNavigationService = await asyncPackage.GetServiceAsync(typeof(IErrorNavigationService)) as IErrorNavigationService;
            Assumes.Present(errorNavigationService);
            var taskBarInfoService = await asyncPackage.GetServiceAsync(typeof(ITaskBarInfoService)) as ITaskBarInfoService;
            Assumes.Present(taskBarInfoService);

            var viewModel = new BuildVisionPaneViewModel(buildInformationProvider, _packageSettingsProvider, solutionProvider, buildService, errorNavigationService, taskBarInfoService);

            View = CreateControlView();
            View.DataContext = viewModel;
            viewModel.ShowOptionPage += ViewModel_ShowOptionPage;
            _controlCreatedSuccessfully = true;
            return viewModel;
        }

        private void ViewModel_ShowOptionPage(Type obj)
        {
            var asyncPackage = (AsyncPackage)Package;
            if (obj == typeof(GeneralSettings))
                asyncPackage.ShowOptionPage(typeof(GeneralSettingsDialogPage));
            if(obj == typeof(GridColumnSettings))
                asyncPackage.ShowOptionPage(typeof(GridSettingsDialogPage));
        }

        protected override void OnClose()
        {
            if (_controlCreatedSuccessfully)
                _packageSettingsProvider.Save();

            base.OnClose();
        }

        private ControlView CreateControlView()
        {
            var view = new ControlView();
            view.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/BuildVision.UI;component/Styles/ExtensionStyle.xaml")
            });
            return view;
        }
    }
}
