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
        public int threadCount { get; set; }

        public DUWorkerPersister persister { get; private set; }
        public DUQueue queue { get; private set; }

        public event EventHandler OnStart;
        public event EventHandler OnStop;
        public event EventHandler<DUWorkerEventArgs> OnProgress;
        public event EventHandler<DUWorkerEventArgs> OnCompleted;

        private long sid = 0;
        private long status = 0;
        private long dirty = 0;

        public bool IsStart { get { return Interlocked.Read(ref status) > 0; } }

        public DUWorker()
        {
            threadCount = Utils.GetLogicProcessorCount();
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

                op.status = OperationStatus.Processing;
                queue.place(op);
                fireOnProgress(op);
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
            Console.WriteLine(op.ToString());
        }

        private void download(OperationInfo op)
        {
            Console.WriteLine(op.ToString());
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

        private void queue_OnRemove(object sender, DUQueueEventArgs e)
        {
            Interlocked.Increment(ref dirty);
        }

        private void queue_OnEnqueue(object sender, DUQueueEventArgs e)
        {
            Interlocked.Increment(ref dirty);
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
