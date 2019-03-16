using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Controls.Indicators
{
    public class ErrorProjectsIndicator : ValueIndicator
    {
        static ErrorProjectsIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ErrorProjectsIndicator), new FrameworkPropertyMetadata(typeof(ErrorProjectsIndicator)));
        }
    }
}
