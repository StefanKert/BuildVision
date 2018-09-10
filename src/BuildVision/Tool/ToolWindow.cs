using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

using Microsoft.VisualStudio.Shell;
using BuildVision.UI;
using BuildVision.Core;
using BuildVision.UI.ViewModels;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models;
using System;
using System.Linq;
using System.Windows.Media;
using System.Text;
using System.Collections;
using System.IO;

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
    public sealed class ToolWindow : ToolWindowPane
    {
        private bool _controlCreatedSuccessfully;

        public ToolWindow()
            :base(null)
        {
            Caption = Resources.ToolWindowTitle;

            // Set the image that will appear on the tab of the window frame
            // when docked with an other window.
            // The resource ID correspond to the one defined in the VSPackage.resx file
            // while the Index is the offset in the bitmap strip. 
            // Each image in the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;
            
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement), 
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            // Content has been set in Initialize(), because Package == null now.
        }

        protected override void Initialize()
        {
            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            Content = CreateControlView();
            _controlCreatedSuccessfully = true;

            base.Initialize();
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
            var viewModel = new ControlViewModel(new ControlModel(), packageContext.ControlSettings);
            packageContext.ControlSettingsChanged += (settings) =>
            {
                viewModel.OnControlSettingsChanged(settings, buildInfo => BuildMessages.GetBuildDoneMessage(viewModel.SolutionItem, buildInfo, viewModel.ControlSettings.BuildMessagesSettings));
            };
            var view = new ControlView { DataContext = viewModel };
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
            packageContext.SaveSettings();
        }

        public static ControlViewModel GetViewModel(ToolWindowPane toolWindow)
        {
            var controlView = (ControlView)toolWindow.Content;
            return (ControlViewModel)controlView.DataContext;
        }
    }
}
