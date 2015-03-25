using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace AlekseyNagovitsyn.BuildVision.Helpers
{
    /// <summary>
    /// Extensions for <see cref="Window"/>.
    /// </summary>
    public static class WindowExtensions
    {
        /// <summary>
        /// Set the window as modal relative to the main window of the process.
        /// </summary>
        public static void SetMainWindowOwner(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            var procs = Process.GetCurrentProcess();
            IntPtr hwnd = procs.MainWindowHandle;
            helper.Owner = hwnd;
        }
    }
}