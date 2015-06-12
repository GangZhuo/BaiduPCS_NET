using System;

namespace FileExplorer
{
    public class SliceFileNameCreatedEventArgs : EventArgs
    {
        public string SliceFileName { get; set; }
    }
}
