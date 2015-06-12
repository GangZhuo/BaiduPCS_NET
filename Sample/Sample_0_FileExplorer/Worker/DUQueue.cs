using System;
using System.Collections.Generic;

namespace FileExplorer
{
    /// <summary>
    /// 上传或下载队列
    /// </summary>
    public class DUQueue
    {
        private List<OperationInfo> pending { get; set; }
        private List<OperationInfo> processing { get; set; }
        private List<OperationInfo> completed { get; set; }

        public DUWorker worker { get; private set; }

        public event EventHandler<DUQueueEventArgs> OnAdd;
        public event EventHandler<DUQueueEventArgs> OnEnqueue;
        public event EventHandler<DUQueueEventArgs> OnDequeue;
        public event EventHandler<DUQueueEventArgs> OnRemove;
        public event EventHandler OnClear;
        public event EventHandler OnClearSuccessItems;

        private object locker = new object();

        public DUQueue(DUWorker worker)
        {
            this.worker = worker;
            pending = new List<OperationInfo>();
            processing = new List<OperationInfo>();
            completed = new List<OperationInfo>();
        }

        public void Add(OperationInfo op)
        {
            lock (locker)
            {
                switch (op.status)
                {
                    case OperationStatus.Pending:
                        pending.Add(op);
                        break;
                    case OperationStatus.Processing:
                        processing.Add(op);
                        break;
                    default:
                        completed.Add(op);
                        break;
                }
            }
            if (OnAdd != null)
                OnAdd(this, new DUQueueEventArgs(op));
        }

        public void Enqueue(OperationInfo op)
        {
            lock (locker)
            {
                pending.Add(op);
            }
            if (OnEnqueue != null)
                OnEnqueue(this, new DUQueueEventArgs(op));
        }

        public OperationInfo Dequeue()
        {
            OperationInfo op = null;
            lock (locker)
            {
                if (pending.Count > 0)
                {
                    op = (OperationInfo)pending[0];
                    pending.RemoveAt(0);
                }
            }
            if (op != null && OnDequeue != null)
                OnDequeue(this, new DUQueueEventArgs(op));
            return op;
        }

        public bool Contains(OperationInfo op)
        {
            bool contains = false;
            lock (locker)
            {
                contains = pending.Contains(op);
                if (!contains)
                    contains = processing.Contains(op);
                if (!contains)
                    contains = completed.Contains(op);
            }
            return contains;
        }

        public void Remove(OperationInfo op)
        {
            bool removed = false;
            lock (locker)
            {
                removed = pending.Remove(op);
                if (!removed)
                    removed = processing.Remove(op);
                if (!removed)
                    removed = completed.Remove(op);
            }
            if (removed && OnRemove != null)
                OnRemove(this, new DUQueueEventArgs(op));
            try
            {
                if (!string.IsNullOrEmpty(op.sliceFileName))
                {
                    //删除分片数据
                    if (System.IO.File.Exists(op.sliceFileName))
                        System.IO.File.Delete(op.sliceFileName);
                }
            }
            catch { }
        }

        public void Clear()
        {
            Clear(true);
        }

        public void Clear(bool fireOnClear)
        {
            lock (locker)
            {
                pending.Clear();
                processing.Clear();
                completed.Clear();
            }
            if (fireOnClear && OnClear != null)
                OnClear(this, new EventArgs());
        }

        public void ClearSuccessItems()
        {
            ClearSuccessItems(true);
        }

        public void ClearSuccessItems(bool fireOnClearSuccessItems)
        {
            lock (locker)
            {
                for (int i = 0; i < completed.Count; )
                {
                    if (completed[i].status == OperationStatus.Success)
                    {
                        completed.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            if (fireOnClearSuccessItems && OnClearSuccessItems != null)
                OnClearSuccessItems(this, new EventArgs());
        }

        public void place(OperationInfo op)
        {
            bool removed = false;
            lock (locker)
            {
                switch (op.status)
                {
                    case OperationStatus.Pending:
                        if (!pending.Contains(op))
                            pending.Insert(0, op);
                        removed = processing.Remove(op);
                        if (!removed)
                            removed = completed.Remove(op);
                        break;
                    case OperationStatus.Processing:
                        if (!processing.Contains(op))
                            processing.Insert(0, op);
                        removed = pending.Remove(op);
                        if (!removed)
                            removed = completed.Remove(op);
                        break;
                    default:
                        if (!completed.Contains(op))
                            completed.Insert(0, op);
                        removed = processing.Remove(op);
                        if (!removed)
                            removed = pending.Remove(op);
                        break;
                }
            }
        }

        public List<OperationInfo> list()
        {
            List<OperationInfo> list = new List<OperationInfo>();
            lock (locker)
            {
                list.AddRange(processing);
                list.AddRange(pending);
                list.AddRange(completed);
            }
            return list;
        }
    }

    public class DUQueueEventArgs : EventArgs
    {
        public OperationInfo op { get; private set; }

        public DUQueueEventArgs(OperationInfo op)
        {
            this.op = op;
        }
    }
}
