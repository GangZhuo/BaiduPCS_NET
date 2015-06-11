using System;

namespace FileExplorer
{
    public class ProgressEventArgs : EventArgs
    {
        public long doneSize { get; private set; }

        public long totalSize { get; private set; }

        public bool Cancel { get; set; }

        public ProgressEventArgs(long doneSize, long totalSize)
        {
            this.doneSize = doneSize;
            this.totalSize = totalSize;
        }
    }
}
