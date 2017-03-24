using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public class ErrorItem
    {
        public const string DefaultSortPropertyName = "Number";
        private readonly List<string> _invalidFileNames = new List<string> { "CSC", "MSBUILD", "LINK" };

        private bool _canNavigateTo;
        private int? _number;
        private ErrorLevel _level;
        private string _code;
        private string _file;
        private string _projectFile;
        private int _lineNumber;
        private int _columnNumber;
        private int _endLineNumber;
        private int _endColumnNumber;
        private string _subcategory;
        private string _message;

        public bool CanNavigateTo
        {
            get { return _canNavigateTo; }
        }

        public int? Number
        {
            get { return _number; }
        }

        public ErrorLevel Level
        {
            get { return _level; }
        }

        public string Code
        {
            get { return _code; }
        }

        public string File
        {
            get { return _file; }
        }

        public string FileName
        {
            get
            {
                try
                {
                    return Path.GetFileName(_file);
                }
                catch (ArgumentException)
                {
                    return _file;
                }
            }
        }

        public string ProjectFile
        {
            get { return _projectFile; }
        }

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        public int ColumnNumber
        {
            get { return _columnNumber; }
        }

        public int EndLineNumber
        {
            get { return _endLineNumber; }
        }

        public int EndColumnNumber
        {
            get { return _endColumnNumber; }
        }

        public string Subcategory
        {
            get { return _subcategory; }
        }

        public string Message
        {
            get { return _message; }
        }

        public ErrorItem(int? errorNumber, ErrorLevel errorLevel, BuildEventArgs args)
        {
            _number = errorNumber;
            _level = errorLevel;
            switch (errorLevel)
            {
                case ErrorLevel.Message:
                    Init((BuildMessageEventArgs)args);
                    break;

                case ErrorLevel.Warning:
                    Init((BuildWarningEventArgs)args);
                    break;

                case ErrorLevel.Error:
                    Init((BuildErrorEventArgs)args);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("errorLevel");
            }

            VerifyValues();
        }

        private void Init(BuildErrorEventArgs e)
        {
            _code = e.Code;
            _file = e.File;
            _projectFile = e.ProjectFile;
            _lineNumber = e.LineNumber;
            _columnNumber = e.ColumnNumber;
            _endLineNumber = e.EndLineNumber;
            _endColumnNumber = e.EndColumnNumber;
            _subcategory = e.Subcategory;
            _message = e.Message;
        }

        private void Init(BuildWarningEventArgs e)
        {
            _code = e.Code;
            _file = e.File;
            _projectFile = e.ProjectFile;
            _lineNumber = e.LineNumber;
            _columnNumber = e.ColumnNumber;
            _endLineNumber = e.EndLineNumber;
            _endColumnNumber = e.EndColumnNumber;
            _subcategory = e.Subcategory;
            _message = e.Message;
        }

        private void Init(BuildMessageEventArgs e)
        {
            _code = e.Code;
            _file = e.File;
            _projectFile = e.ProjectFile;
            _lineNumber = e.LineNumber;
            _columnNumber = e.ColumnNumber;
            _endLineNumber = e.EndLineNumber;
            _endColumnNumber = e.EndColumnNumber;
            _subcategory = e.Subcategory;
            _message = e.Message;
        }

        // 1. EnvDTE.TextSelection.MoveToLineAndOffset requires line and offset numbers beginning at one.
        // BuildErrorEventArgs.LineNumber and BuildErrorEventArgs.ColumnNumber may be uninitialized.
        // 2. BuildErrorEventArgs.EndLineNumber and BuildErrorEventArgs.EndColumnNumber may be uninitialized,
        // regardless of BuildErrorEventArgs.LineNumber and BuildErrorEventArgs.ColumnNumber.
        private void VerifyValues()
        {
            if (_invalidFileNames.Contains(_file) && _lineNumber == 0 && _columnNumber == 0)
            {
                _canNavigateTo = false;
                return;
            }

            if (_lineNumber < 1)
                _lineNumber = 1;

            if (_columnNumber < 1)
                _columnNumber = 1;

            if (_endLineNumber == 0 && _endColumnNumber == 0)
            {
                _endLineNumber = _lineNumber;
                _endColumnNumber = _columnNumber;
            }

            _canNavigateTo = true;
        }
    }
}