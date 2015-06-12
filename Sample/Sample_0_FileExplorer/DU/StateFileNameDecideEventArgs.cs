using System;

namespace FileExplorer
{
    public class StateFileNameDecideEventArgs : EventArgs
    {
        public string SliceFileName { get; set; }
    }
}
