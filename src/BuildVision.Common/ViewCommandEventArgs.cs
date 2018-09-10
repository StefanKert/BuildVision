using System;

namespace BuildVision.Common
{
    public class ViewCommandEventArgs : EventArgs
    {
        public string Command { get; }

        public object Response { get; set; }

        public ViewCommandEventArgs(string command)
        {
            Command = command;
        }
    }
}
