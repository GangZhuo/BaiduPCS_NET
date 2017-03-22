using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace FileExplorer
{
    public class BigFileHelper : IDisposable, IBigFileHelper
    {
        public string FileName { get; private set; }
        private MemoryMappedFile mmf;
        private long fileSize;

        public BigFileHelper(string filename, long filesize)
        {
            FileName = filename;
            this.fileSize = filesize;
            mmf = MemoryMappedFile.CreateFromFile(FileName, FileMode.Open, Utils.md5(FileName), filesize, MemoryMappedFileAccess.ReadWrite); //映射文件到内存
        }

        ~BigFileHelper()
        {
            Disposing(false);
        }

        public void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        protected void Disposing(bool disposing)
        {
            if(disposing)
            {
                if (mmf != null)
                {
                    mmf.Dispose();
                    mmf = null;
                }
            }
        }

        public void Update(long position, byte[] array, int offset, int count)
        {
            using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(position, count))
            {
                accessor.WriteArray<byte>(0, array, offset, count);
            }
        }

    }
}
