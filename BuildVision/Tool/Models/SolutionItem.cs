using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using AlekseyNagovitsyn.BuildVision.Core.Logging;
using AlekseyNagovitsyn.BuildVision.Helpers;
using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    public class SolutionItem : NotifyPropertyChangedBase
    {
        private Solution _storageSolution;
        private readonly ObservableRangeCollection<ProjectItem> _projects = new ObservableRangeCollection<ProjectItem>();
        private string _name;
        private string _fullName;
        private bool _isEmpty = true;

        public Solution StorageSolution
        {
            get { return _storageSolution; }
        }

        public ObservableCollection<ProjectItem> Projects
        {
            get { return _projects; }
        }

        public void UpdateSolution(Solution solution)
        {
            _storageSolution = solution;
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            try
            {
                var solution = _storageSolution;
                if (solution == null)
                {
                    Name = Resources.GridCellNATextInBrackets;
                    FullName = Resources.GridCellNATextInBrackets;
                    IsEmpty = true;
                } 
                else if (string.IsNullOrEmpty(solution.FileName))
                {
                    if (solution.Count != 0 /* projects count */)
                    {
                        var project = solution.Item(1);
                        Name = Path.GetFileNameWithoutExtension(project.FileName);
                        FullName = project.FullName;
                        IsEmpty = false;
                    }
                    else
                    {
                        Name = Resources.GridCellNATextInBrackets;
                        FullName = Resources.GridCellNATextInBrackets;
                        IsEmpty = true;
                    }
                }
                else
                {
                    Name = Path.GetFileNameWithoutExtension(solution.FileName);
                    FullName = solution.FullName;
                    IsEmpty = false;
                }
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();

                Name = Resources.GridCellNATextInBrackets;
                FullName = Resources.GridCellNATextInBrackets;
                IsEmpty = true;
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged("FullName");
                }
            }
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
            set
            {
                if (_isEmpty != value)
                {
                    _isEmpty = value;
                    OnPropertyChanged("IsEmpty");
                }
            }
        }

        public void UpdateProjects()
        {
            _projects.Clear();

            Solution solution = _storageSolution;
            if (solution == null)
                return;

            IList<Project> dteProjects;
            try
            {
                dteProjects = solution.GetProjects();
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                return;
            }

            var projectItems = new List<ProjectItem>(dteProjects.Count);
            foreach (Project project in dteProjects)
            {
                try
                {
                    var projectItem = new ProjectItem();
                    ViewModelHelper.UpdateProperties(project, projectItem);
                    projectItems.Add(projectItem);
                }
                catch (Exception ex)
                {
                    ex.TraceUnknownException();
                }
            }

            _projects.AddRange(projectItems);
        }
    }
}