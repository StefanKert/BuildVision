using BuildVision.Common;

namespace BuildVision.Core
{
    public class SolutionModel : BindableBase
    {
        public string State { get; set; } = "StandBy";

        private int _errorCount = -1;
        public int ErrorCount
        {
            get => _errorCount;
            set => SetProperty(ref _errorCount, value);
        }

        private int _warningsCount = -1;
        public int WarningsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _warningsCount, value);
        }

        private int _messagesCount = -1;
        public int MessagesCount
        {
            get => _messagesCount;
            set => SetProperty(ref _messagesCount, value);
        }

        private int _succeededProjectsCount = -1;
        public int SucceededProjectsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _succeededProjectsCount, value);
        }

        private int _upToDateProjectsCount = -1;
        public int UpToDateProjectsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _upToDateProjectsCount, value);
        }

        private int _failedProjectsCount = -1;
        public int FailedProjectsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _failedProjectsCount, value);
        }

        private int _warnedProjectsCount = -1;
        public int WarnedProjectsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _warnedProjectsCount, value);
        }

        private string _stateMessage = "";
        public string StateMessage
        {
            get => _stateMessage;
            set => SetProperty(ref _stateMessage, value);
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

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
    }
}
