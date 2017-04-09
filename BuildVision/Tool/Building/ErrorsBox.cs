using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Build.Framework;
using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public class ErrorsBox : IEnumerable<ErrorItem>
    {
        const bool KeepErrorsOnly = true;

        private readonly List<ErrorItem> _errors = new List<ErrorItem>();

        private readonly List<ErrorItem> _warnings = new List<ErrorItem>();

        private readonly List<ErrorItem> _messages = new List<ErrorItem>();

        public int ErrorsCount { get; private set; }
        public int WarningsCount { get; private set; }
        public int MessagesCount { get; private set; }

        public bool IsEmpty
        {
            get { return _errors.Count == 0 && _warnings.Count == 0 && _messages.Count == 0; }
        }

        public IList<ErrorItem> Errors
        {
            get { return _errors; }
        }

        public void Keep(ErrorLevel errorLevel, BuildEventArgs e, Project project)
        {
            switch (errorLevel)
            {
                case ErrorLevel.Message:
                    MessagesCount++;
                    break;
                case ErrorLevel.Warning:
                    WarningsCount++;
                    break;
                case ErrorLevel.Error:
                    ErrorsCount++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("errorLevel");
            }

            if (KeepErrorsOnly && errorLevel != ErrorLevel.Error)
                return;

            int errorNumber = _errors.Count + _warnings.Count + _messages.Count + 1;
            var errorItem = new ErrorItem(errorNumber, errorLevel, e, project);

            switch (errorLevel)
            {
                case ErrorLevel.Message:
                    _messages.Add(errorItem);
                    break;

                case ErrorLevel.Warning:
                    _warnings.Add(errorItem);
                    break;

                case ErrorLevel.Error:
                    _errors.Add(errorItem);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("errorLevel");
            }
        }

        public IEnumerator<ErrorItem> GetEnumerator()
        {
            foreach (var error in _errors)
                yield return error;

            foreach (var warning in _warnings)
                yield return warning;

            foreach (var message in _messages)
                yield return message;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}