using System;

namespace FileBackup
{
    public delegate void OnWorkDone(IWorker sender);

    public interface IWorker
    {
        event OnWorkDone Done;
        void Run();
    }
}
