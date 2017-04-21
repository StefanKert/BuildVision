using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AlekseyNagovitsyn.BuildVision.Tool.Views.Extensions;

using ProjectItem = AlekseyNagovitsyn.BuildVision.Tool.Models.ProjectItem;
using UIElement = System.Windows.UIElement;
using BuildVision.Contracts;
using AlekseyNagovitsyn.BuildVision.Core.Logging;

namespace BuildVision.UI
{
    public partial class ProjectItemDetailsView : UserControl
    {

        public static readonly DependencyProperty ProjectItemProperty = 
            DependencyProperty.Register("ProjectItem", typeof(ProjectItem), typeof(ProjectItemDetailsView));

        public ProjectItem ProjectItem
        {
            get { return (ProjectItem)GetValue(ProjectItemProperty); }
            set { SetValue(ProjectItemProperty, value); }
        }

        public ProjectItemDetailsView()
        {
            InitializeComponent();
        }
    }
}
