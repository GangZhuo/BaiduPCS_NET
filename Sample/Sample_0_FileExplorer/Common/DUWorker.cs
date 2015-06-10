using System;
using System.Collections.Generic;

using BaiduPCS_NET;

namespace FileExplorer
{
    /// <summary>
    /// 上传器或下载器
    /// </summary>
    public class DUWorker
    {
        public BaiduPCS pcs { get; set; }

        public int ThreadCount { get; set; }

        public DUQueue queue { get; private set; }
        public IList<OperationInfo> completedDownload { get; private set; }
        public IList<OperationInfo> completedUpload { get; private set; }

        public event OnDUWorkerReset OnReset;
        public event OnDUWorkerProgressChange OnProgressChange;
        public event OnDUWorkerUploaded OnUploaded;
        public event OnDUWorkerDownloaded OnDownloaded;

        public DUWorker()
        {
            ThreadCount = Utils.GetLogicProcessorCount();
            queue = new DUQueue();
            completedDownload = new List<OperationInfo>();
            completedUpload = new List<OperationInfo>();
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public void Reset()
        {
            queue.Clear();
            completedDownload.Clear();
            completedUpload.Clear();
            if (OnReset != null)
                OnReset(this);
        }

    }

    public delegate void OnDUWorkerReset(object sender);

    public delegate void OnDUWorkerProgressChange(object sender, OperationInfo op);

    public delegate void OnDUWorkerUploaded(object sender, OperationInfo op);

    public delegate void OnDUWorkerDownloaded(object sender, OperationInfo op);
}
