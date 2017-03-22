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
        public string workfolder { get; set; }
        public BaiduPCS pcs { get; set; }
        public int logicProcessorCount { get; set; }

        public DUWorkerPersister persister { get; private set; }
        public DUQueue queue { get; private set; }

        public bool IsPause
        {
            get { return Interlocked.Read(ref pause) > 0; }
        }

        public event EventHandler OnStart;
        public event EventHandler OnStop;
        public event EventHandler<DUWorkerEventArgs> OnProgress;
        public event EventHandler<DUWorkerEventArgs> OnCompleted;
        public event EventHandler<DUWorkerEventArgs> OnThreadChanged;

        public bool IsStart { get { return Interlocked.Read(ref status) > 0; } }

        private long sid = 0; // 每次执行 Start() 时，都会增加此值，用于标记服务
        private long status = 0; // 标记当前是否处于启动状态
        private long dirty = 0; // 标记 queue 是否脏掉
        private long pause = 1; // 标记是否暂停

        public DUWorker()
        {
            workfolder = string.Empty;
            logicProcessorCount = Utils.GetLogicProcessorCount();
            persister = new DUWorkerPersister(this);
            queue = new DUQueue(this);
            queue.OnEnqueue += queue_OnEnqueue;
            queue.OnRemove += queue_OnRemove;
        }

        public void Start()
        {
            if (IsStart)
                return;
            Interlocked.Increment(ref sid);
            Interlocked.Exchange(ref status, 1);
            new Thread(new ThreadStart(execTask)).Start();
        }

        public void Stop()
        {
            if (!IsStart)
                return;
            Interlocked.Increment(ref sid);
        }

        public void Pause()
        {
            Interlocked.Exchange(ref pause, 1);
        }

        public void Resume()
        {
            Interlocked.Exchange(ref pause, 0);
        }

        private void execTask()
        {
            long csid = Interlocked.Read(ref sid);
            long tick = 0, ndirty;
            OperationInfo op = null;
            queue.Clear();
            persister.Restore();
            fireOnStart();
            while (csid == Interlocked.Read(ref sid))
            {
                #region 暂停
                if (IsPause)
                {
                    Thread.Sleep(100);
                    continue;
                }
                #endregion

                #region 每 5 秒保存一次
                ndirty = Interlocked.Read(ref dirty);
                if (tick > 50 && ndirty > 0)
                {
                    persister.Save();
                    Interlocked.Add(ref dirty, -ndirty);
                    tick = 0;
                }
                else
                {
                    tick++;
                }
                #endregion

                #region 获取待处理的 OperationInfo 对象

                op = queue.Dequeue();
                if (op == null)
                {
                    Thread.Sleep(100);
                    continue;
                }
                else if (op.status != OperationStatus.Pending
                    && op.status != OperationStatus.Processing) // 来自中断后还原
                {
                    queue.place(op);
                    continue;
                }

                #endregion

                #region 处理 OperationInfo 对象

                op.sid = csid;
                op.status = OperationStatus.Processing;
                queue.place(op);
                //如果在进度处理程序中，改变了状态，则跳过继续处理
                if (op.operation == Operation.Download)
                {
                    download(op);
                    //如果 download() 方法中未设置状态，则设置状态为失败
                    if (op.status == OperationStatus.Processing)
                    {
                        op.errmsg = "Unknow error";
                        op.status = OperationStatus.Fail;
                    }
                }
                else if (op.operation == Operation.Upload)
                {
                    upload(op);
                    //如果 upload() 方法中未设置状态，则设置状态为失败
                    if (op.status == OperationStatus.Processing)
                    {
                        op.errmsg = "Unknow error";
                        op.status = OperationStatus.Fail;
                    }
                }
                else
                {
                    //未知的操作类型，直接设置状态为失败
                    op.errmsg = "Unknow operation";
                    op.status = OperationStatus.Fail;
                }
                queue.place(op);
                Interlocked.Increment(ref dirty);

                #endregion

                fireOnCompleted(op);
            }
            Interlocked.Exchange(ref status, 0);
            persister.Save();
            Interlocked.Exchange(ref dirty, 0);
            queue.Clear();
            fireOnStop();
        }

        private void upload(OperationInfo op)
        {
            Uploader u = null;
            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(op.from);
                op.totalSize = fi.Length;
                fireOnProgress(op); // 触发一次进度改变
                //如果在进度处理程序中，改变了状态，则跳过继续处理
                if (op.status == OperationStatus.Processing)
                {
                    if (fi.Length > MultiThreadUploader.MIN_SLICE_SIZE)
                    {
                        u = new RapidUploader(pcs, op.from, op.to);
                        u.Upload();
                        if (u.Success)
                            op.status = OperationStatus.Success;
                        else if (u.IsCancelled)
                            op.status = OperationStatus.Cancel;
                        else if (op.status == OperationStatus.Processing)
                            u = new MultiThreadUploader(pcs, op.from, op.to, workfolder, getUploadMaxThreadCount());
                        else
                            u = null; // op 的状态已经被改变，且不是 OperationStatus.Processing
                    }
                    else if (op.status == OperationStatus.Processing)
                        u = new Uploader(pcs, op.from, op.to);
                    else
                        u = null; // op 的状态已经被改变，且不是 OperationStatus.Processing
                    if (u != null && op.status == OperationStatus.Processing)
                    {
                        u.IsOverWrite = AppSettings.OverWriteWhenUploadFile;
                        u.Progress += du_onProgress;
                        u.Completed += du_onCompleted;
                        u.StateFileNameDecide += du_onStateFileNameDecide;
                        u.ThreadChanged += du_onThreadChanged;
                        u.State = op;
                        u.Upload();
                    }
                }
            }
            catch (Exception ex)
            {
                op.status = OperationStatus.Fail;
                op.errmsg = ex.Message;
            }
        }

        private void download(OperationInfo op)
        {
            Downloader d = null;
            PcsFileInfo from;
            try
            {
                fireOnProgress(op); // 触发一次进度改变
                from = pcs.meta(op.from);
                if (from.IsEmpty)
                {
                    op.status = OperationStatus.Fail;
                    op.errmsg = "The remote file not exists (" + from.path + ").";
                }
                else if (from.isdir)
                {
                    op.status = OperationStatus.Fail;
                    op.errmsg = "Can't download directory (" + from.path + ").";
                }
                else if (op.status == OperationStatus.Processing)
                {
                    op.totalSize = from.size;
                    fireOnProgress(op); // 触发一次进度改变
                    //如果在进度处理程序中，改变了状态，则跳过继续处理
                    if (op.status == OperationStatus.Processing)
                    {
                        if (from.size > MultiThreadDownloader.MIN_SLICE_SIZE)
                            d = new MultiThreadDownloader(pcs, from, op.to, workfolder, getDownloadMaxThreadCount(), getMinDownloadSliceSize());
                        else
                            d = new Downloader(pcs, from, op.to);
                        d.Completed += du_onCompleted;
                        d.Progress += du_onProgress;
                        d.StateFileNameDecide += du_onStateFileNameDecide;
                        d.ThreadChanged += du_onThreadChanged;
                        d.State = op;
                        d.Download();
                    }
                }
            }
            catch (Exception ex)
            {
                op.status = OperationStatus.Fail;
                op.errmsg = ex.Message;
            }
        }

        private void du_onStateFileNameDecide(object sender, StateFileNameDecideEventArgs e)
        {
            IProgressable d = (IProgressable)sender;
            OperationInfo op = (OperationInfo)d.State;
            op.sliceFileName = e.SliceFileName;
        }

        private void du_onProgress(object sender, ProgressEventArgs e)
        {
            IProgressable d = (IProgressable)sender;
            OperationInfo op = (OperationInfo)d.State;
            op.totalSize = e.totalSize;
            op.doneSize = e.doneSize;
            fireOnProgress(op);
            if (op.status != OperationStatus.Processing)
                e.Cancel = true;
            else if (IsPause)
            {
                e.Cancel = true;
                op.status = OperationStatus.Pending;
            }
            else if (op.sid != Interlocked.Read(ref sid))
            {
                e.Cancel = true;
                op.status = OperationStatus.Pending;
            }
        }

        private void du_onCompleted(object sender, CompletedEventArgs e)
        {
            IProgressable d = (IProgressable)sender;
            OperationInfo op = (OperationInfo)d.State;
            if (e.Success)
                op.status = OperationStatus.Success;
            else if (op.status == OperationStatus.Processing)
            {
                if (e.Cancel)
                    op.status = OperationStatus.Cancel;
                else
                {
                    op.status = OperationStatus.Fail;
                    op.errmsg = e.Exception == null ? string.Empty : e.Exception.Message;
                }
            }
        }

        private void du_onThreadChanged(object sender, ThreadCountChangedEventArgs e)
        {
            IProgressable d = (IProgressable)sender;
            OperationInfo op = (OperationInfo)d.State;
            op.runningThreadCount = e.RunningThreadCount;
            op.totalThreadCount = e.TotalThreadCount;
            fireOnThreadChanged(op);
        }

        private void fireOnStart()
        {
            if (OnStart != null)
                OnStart(this, new EventArgs());
        }

        private void fireOnStop()
        {
            if (OnStop != null)
                OnStop(this, new EventArgs());
        }

        private void fireOnProgress(OperationInfo op)
        {
            if (OnProgress != null)
                OnProgress(this, new DUWorkerEventArgs(op));
        }

        private void fireOnCompleted(OperationInfo op)
        {
            if (OnCompleted != null)
                OnCompleted(this, new DUWorkerEventArgs(op));
        }

        private void fireOnThreadChanged(OperationInfo op)
        {
            if (OnThreadChanged != null)
                OnThreadChanged(this, new DUWorkerEventArgs(op));
        }

        private void queue_OnRemove(object sender, DUQueueEventArgs e)
        {
            Interlocked.Increment(ref dirty);
        }

        private void queue_OnEnqueue(object sender, DUQueueEventArgs e)
        {
            Interlocked.Increment(ref dirty);
        }

        private int getDownloadMaxThreadCount()
        {
            int co = 0;
            if (AppSettings.AutomaticDownloadMaxThreadCount || AppSettings.DownloadMaxThreadCount <= 0)
                co = logicProcessorCount - 2;
            else
                co = AppSettings.DownloadMaxThreadCount;
            if (co < 1) co = 1;
            return co;
        }

        private int getUploadMaxThreadCount()
        {
            int co = 0;
            if (AppSettings.AutomaticUploadMaxThreadCount || AppSettings.UploadMaxThreadCount <= 0)
                co = logicProcessorCount - 2;
            else
                co = AppSettings.UploadMaxThreadCount;
            if (co < 1) co = 1;
            return co;
        }

        private int getMinDownloadSliceSize()
        {
            if (AppSettings.MinDownloasSliceSize >= MultiThreadDownloader.MIN_SLICE_SIZE / Utils.KB && AppSettings.MinDownloasSliceSize <= MultiThreadDownloader.MAX_SLICE_SIZE / Utils.KB)
                return AppSettings.MinDownloasSliceSize * Utils.KB;
            return MultiThreadDownloader.MIN_SLICE_SIZE;
        }
    }

    public class DUWorkerEventArgs : EventArgs
    {
        public OperationInfo op { get; private set; }

        public DUWorkerEventArgs(OperationInfo op)
        {
            this.op = op;
        }
    }
}
