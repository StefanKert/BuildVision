using System;

namespace AlekseyNagovitsyn.BuildVision.Tool.ViewModels
{
    public class ViewCommandEventArgs : EventArgs
    {
        private readonly string _command;
        public string Command
        {
            get { return _command; }
        }

        public object Response { get; set; }

        public ViewCommandEventArgs(string command)
        {
            _command = command;
        }
    }
}