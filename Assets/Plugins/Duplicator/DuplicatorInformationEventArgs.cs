using System;

namespace Duplicator
{
    public class DuplicatorInformationEventArgs : EventArgs
    {
        internal DuplicatorInformationEventArgs(string information)
        {
            Information = information;
        }

        public string Information { get; }
    }
}
