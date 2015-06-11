using System;

namespace FileExplorer
{
    public class CompletedEventArgs : EventArgs
    {
        public bool Success { get; private set; }
        public bool Cancel { get; private set; }
        public Exception Exception { get; private set; }

        public CompletedEventArgs(bool success, bool cancel, Exception ex)
        {
            Success = success;
            Cancel = cancel;
            Exception = ex;
        }
    }
}
