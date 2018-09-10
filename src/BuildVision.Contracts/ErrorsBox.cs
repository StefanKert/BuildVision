using System;
using System.Collections;
using System.Collections.Generic;

namespace BuildVision.Contracts
{
    public class ErrorsBox : IEnumerable<ErrorItem>
    {
        private readonly List<ErrorItem> _errors = new List<ErrorItem>();

        private readonly List<ErrorItem> _warnings = new List<ErrorItem>();

        private readonly List<ErrorItem> _messages = new List<ErrorItem>();

        public int ErrorsCount { get; private set; }
        public int WarningsCount { get; private set; }
        public int MessagesCount { get; private set; }

        public bool IsEmpty => _errors.Count == 0 && _warnings.Count == 0 && _messages.Count == 0;
        public IList<ErrorItem> Errors => _errors;

        public void Add(ErrorItem errorItem)
        {
            switch (errorItem.Level)
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

            if (errorItem.Level != ErrorLevel.Error)
                return;

            int errorNumber = _errors.Count + _warnings.Count + _messages.Count + 1;
            errorItem.Number = errorNumber;
            switch (errorItem.Level)
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
