using System;

namespace FileExplorer
{
    public interface IProgressable
    {
        object State { get; set; }
        event EventHandler<StateFileNameDecideEventArgs> StateFileNameDecide;
        event EventHandler<ProgressEventArgs> Progress;
        event EventHandler<CompletedEventArgs> Completed;
    }
}
