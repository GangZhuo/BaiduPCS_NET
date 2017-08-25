using System;
using System.IO;

using BaiduPCS_NET;

namespace FileExplorer
{
    public class Downloader : ICancellable, IProgressable
    {
        public BaiduPCS pcs { get; protected set; }
        public PcsFileInfo from { get; set; }
        public string to { get; set; }
        public long DoneSize { get; protected set; }
        public bool Success { get; protected set; }
        public bool IsCancelled { get; protected set; }
        public Exception Error { get; protected set; }
        public bool Downloading { get; protected set; }
        public object State { get; set; }

        public event EventHandler<CompletedEventArgs> Completed;
        public event EventHandler<ProgressEventArgs> Progress;
        public event EventHandler<StateFileNameDecideEventArgs> StateFileNameDecide;
        public event EventHandler<ThreadCountChangedEventArgs> ThreadChanged;

        public Downloader(BaiduPCS pcs, PcsFileInfo from, string to)
        {
            this.pcs = pcs;
            this.from = from;
            this.to = to;
        }

        public virtual void Download()
        {
            if (Downloading)
                throw new Exception("Can't download, since the previous download is not complete.");
            FileStream stream = null;
            DoneSize = 0;
            Success = false;
            IsCancelled = false;
            Error = null;
            Downloading = true;
            try
            {
                BaiduPCS pcs = this.pcs.clone();
                pcs.Write += onWrite;
                CreateDirectory(to);
                stream = new FileStream(to, FileMode.Create, FileAccess.Write);
                pcs.WriteUserData = stream;
                fireThreadChanged(new ThreadCountChangedEventArgs(1, 1));
                PcsRes rc = pcs.download(from.path, 0, 0, 0);
                if (rc == PcsRes.PCS_OK)
                {
                    Success = true;
                    IsCancelled = false;
                }
                else if (IsCancelled)
                {
                    Success = false;
                    IsCancelled = true;
                }
                else
                {
                    Success = false;
                    IsCancelled = false;
                    if (Error == null)
                        Error = new Exception(pcs.getError());
                }
            }
            catch (Exception ex)
            {
                Success = false;
                IsCancelled = false;
                Error = ex;
            }
            if (stream != null)
                stream.Close();
            Downloading = false;
            fireCompleted(new CompletedEventArgs(Success, IsCancelled, Error));
            fireThreadChanged(new ThreadCountChangedEventArgs(0, 1));
        }

        public virtual void Cancel()
        {
            IsCancelled = true;
        }

        private uint onWrite(BaiduPCS sender, byte[] data, uint contentlength, object userdata)
        {
            if (IsCancelled)
                return 0;
            try
            {
                if (data.Length > 0)
                {
                    Stream stream = (Stream)userdata;
                    stream.Write(data, 0, data.Length);
                }
                DoneSize += data.Length;
                ProgressEventArgs args = new ProgressEventArgs(DoneSize, from.size);
                fireProgress(args);
                if (args.Cancel)
                {
                    IsCancelled = true;
                    return 0;
                }
                return (uint)data.Length;
            }
            catch (Exception ex)
            {
                Error = ex;
            }
            return 0;
        }

        protected virtual void CreateDirectory(string filename)
        {
            string dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        protected virtual void fireProgress(ProgressEventArgs args)
        {
            if (Progress != null)
                Progress(this, args);
        }

        protected virtual void fireCompleted(CompletedEventArgs args)
        {
            if (Completed != null)
                Completed(this, args);
        }

        protected virtual void fireStateFileNameDecide(StateFileNameDecideEventArgs args)
        {
            if (StateFileNameDecide != null)
                StateFileNameDecide(this, args);
        }

        protected virtual void fireThreadChanged(ThreadCountChangedEventArgs args)
        {
            if (ThreadChanged != null)
                ThreadChanged(this, args);
        }
    }
}
