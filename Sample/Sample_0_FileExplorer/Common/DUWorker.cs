using System;
using System.Collections.Generic;
using System.Threading;

using BaiduPCS_NET;

namespace FileExplorer
{
    /// <summary>
    /// 上传器或下载器
    /// </summary>
    public class DUWorker
    {
        public BaiduPCS pcs { get; set; }

        public int threadCount { get; set; }

        public DUQueue queue { get; private set; }
        public IList<OperationInfo> completedDownload { get; private set; }
        public IList<OperationInfo> completedUpload { get; private set; }

        public event OnDUWorkerReset OnReset;
        public event OnDUWorkerProgressChange OnProgressChange;
        public event OnDUWorkerUploaded OnUploaded;
        public event OnDUWorkerDownloaded OnDownloaded;

        private long sid = 0;

        public DUWorker()
        {
            threadCount = Utils.GetLogicProcessorCount();
            queue = new DUQueue();
            completedDownload = new List<OperationInfo>();
            completedUpload = new List<OperationInfo>();
        }

        public void Start()
        {
            Interlocked.Increment(ref sid);
            for (int i = 0; i < threadCount; i++)
            {
                new Thread(new ThreadStart(execTask)).Start();
            }

        }

        public void Stop()
        {
            Interlocked.Increment(ref sid);
        }

        public void Reset()
        {
            queue.Clear();
            completedDownload.Clear();
            completedUpload.Clear();
            if (OnReset != null)
                OnReset(this);
        }

        private void execTask()
        {
            long csid = Interlocked.Read(ref sid);
            BaiduPCS pcs = this.pcs.clone();
            OperationInfo op = null;
            while(csid == Interlocked.Read(ref sid))
            {
                op = queue.Dequeue();
                if(op == null)
                {
                    Thread.Sleep(100);
                    continue;
                }
                if(op.operation == Operation.Download)
                {
                    download(op);
                }
                else if(op.operation == Operation.Upload)
                {
                    upload(op);
                }
            }
        }

        private bool upload(OperationInfo op)
        {
            return false;
        }

        public bool download(OperationInfo op)
        {
            return false;
        }

    }

    public delegate void OnDUWorkerReset(object sender);

    public delegate void OnDUWorkerProgressChange(object sender, OperationInfo op);

    public delegate void OnDUWorkerUploaded(object sender, OperationInfo op);

    public delegate void OnDUWorkerDownloaded(object sender, OperationInfo op);
}
