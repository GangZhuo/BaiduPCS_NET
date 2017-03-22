using System;

namespace FileExplorer
{
    public class ThreadCountChangedEventArgs : EventArgs
    {
        public int RunningThreadCount { get; private set; }
        public int TotalThreadCount { get; private set; }

        public ThreadCountChangedEventArgs(int running, int total)
        {
            RunningThreadCount = running;
            TotalThreadCount = total;
        }
    }
}
