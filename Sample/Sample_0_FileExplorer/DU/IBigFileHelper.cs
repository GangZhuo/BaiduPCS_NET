using System;

namespace FileExplorer
{
    public interface IBigFileHelper : IDisposable
    {
        string FileName { get; }

        void Update(long position, byte[] array, int offset, int count);
    }
}
