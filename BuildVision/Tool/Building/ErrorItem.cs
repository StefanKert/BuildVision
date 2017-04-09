using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;
using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public class ErrorItem
    {
        public const string DefaultSortPropertyName = "Number";
        private readonly List<string> _invalidFileNames = new List<string> { "CSC", "MSBUILD", "LINK" };


        public bool CanNavigateTo { get; private set; }
        public int? Number { get; private set; }

        public ErrorLevel Level { get; private set; }

        public string Code { get; private set; }

        public string File { get; private set; }

        public string FileName
        {
            get
            {
                try
                {
                    return Path.GetFileName(File);
                }
                catch (ArgumentException)
                {
                    return File;
                }
            }
        }

        public string ProjectFile { get; private set; }

        public int LineNumber { get; private set; }
        public int ColumnNumber { get; private set; }

        public int EndLineNumber { get; private set; }

        public int EndColumnNumber { get; private set; }

        public string Subcategory { get; private set; }

        public string Message { get; private set; }

        public Project Project { get; }

        public ErrorItem(int? errorNumber, ErrorLevel errorLevel, BuildEventArgs args, Project project)
        {
            Project = project;
            Number = errorNumber;
            Level = errorLevel;
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
            Code = e.Code;
            File = e.File;
            ProjectFile = e.ProjectFile;
            LineNumber = e.LineNumber;
            ColumnNumber = e.ColumnNumber;
            EndLineNumber = e.EndLineNumber;
            EndColumnNumber = e.EndColumnNumber;
            Subcategory = e.Subcategory;
            Message = e.Message;
        }

        private void Init(BuildWarningEventArgs e)
        {
            Code = e.Code;
            File = e.File;
            ProjectFile = e.ProjectFile;
            LineNumber = e.LineNumber;
            ColumnNumber = e.ColumnNumber;
            EndLineNumber = e.EndLineNumber;
            EndColumnNumber = e.EndColumnNumber;
            Subcategory = e.Subcategory;
            Message = e.Message;
        }

        private void Init(BuildMessageEventArgs e)
        {
            Code = e.Code;
            File = e.File;
            ProjectFile = e.ProjectFile;
            LineNumber = e.LineNumber;
            ColumnNumber = e.ColumnNumber;
            EndLineNumber = e.EndLineNumber;
            EndColumnNumber = e.EndColumnNumber;
            Subcategory = e.Subcategory;
            Message = e.Message;
        }

        // 1. EnvDTE.TextSelection.MoveToLineAndOffset requires line and offset numbers beginning at one.
        // BuildErrorEventArgs.LineNumber and BuildErrorEventArgs.ColumnNumber may be uninitialized.
        // 2. BuildErrorEventArgs.EndLineNumber and BuildErrorEventArgs.EndColumnNumber may be uninitialized,
        // regardless of BuildErrorEventArgs.LineNumber and BuildErrorEventArgs.ColumnNumber.
        private void VerifyValues()
        {
            if (_invalidFileNames.Contains(File) && LineNumber == 0 & ColumnNumber == 0)
            {
                CanNavigateTo = false;
                return;
            }

            if (LineNumber < 1)
                LineNumber = 1;

            if (ColumnNumber < 1)
                ColumnNumber = 1;

            if (EndLineNumber == 0 && EndColumnNumber == 0)
            {
                EndLineNumber = LineNumber;
                EndColumnNumber = ColumnNumber;
            }

            CanNavigateTo = true;
        }
    }
}