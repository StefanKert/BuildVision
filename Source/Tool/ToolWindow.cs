using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

using AlekseyNagovitsyn.BuildVision.Core.Common;
using AlekseyNagovitsyn.BuildVision.Tool.Models;
using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;
using AlekseyNagovitsyn.BuildVision.Tool.Views;

using Microsoft.VisualStudio.Shell;

namespace AlekseyNagovitsyn.BuildVision.Tool
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
    [Guid("e1d4b2b5-934e-4b0e-96cc-1c0449764501")]
    public sealed class ToolWindow : ToolWindowPane
    {
        private bool _controlCreatedSuccessfully;
        
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
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
            Content = CreateMyControl();
            _controlCreatedSuccessfully = true;

            base.Initialize();
        }

        protected override void OnClose()
        {
            if (_controlCreatedSuccessfully)
                SaveControlSettings();

            base.OnClose();
        }

        private ControlView CreateMyControl()
        {
            var packageContext = (IPackageContext)Package;
            var viewMode = new ControlViewModel(new ControlModel(), packageContext);
            var view = new ControlView { DataContext = viewMode };
            return view;
        }

        private void SaveControlSettings()
        {
            ControlViewModel viewModel = GetViewModel(this);
            viewModel.SyncColumnSettings();

            var packageContext = (IPackageContext)Package;
            packageContext.SaveSettings();
        }

        public static ControlViewModel GetViewModel(ToolWindowPane toolWindow)
        {
            var myControlView = (ControlView)toolWindow.Content;
            return (ControlViewModel)myControlView.DataContext;
        }
    }
}
