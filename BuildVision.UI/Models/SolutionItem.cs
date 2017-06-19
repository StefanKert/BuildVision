using BuildVision.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace BuildVision.UI.Models
{
    public class SolutionItem : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private bool _isEmpty = true;
        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        public ObservableCollection<ProjectItem> Projects { get; }

        public List<ProjectItem> AllProjects { get; }

        public SolutionItem()
        {
            Projects = new ObservableRangeCollection<ProjectItem>();
            AllProjects = new List<ProjectItem>();
        }
    }
}