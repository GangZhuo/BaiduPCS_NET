using System;
using System.Collections;
using System.Collections.Generic;

namespace FileExplorer
{
    /// <summary>
    /// 上传或下载队列
    /// </summary>
    public class DUQueue : IEnumerable
    {
        public Queue queue { get; private set; }

        public int Count
        {
            get { return queue.Count; }
        }

        public event OnDUQueueEnqueue OnEnqueue;

        public event OnDUQueueDequeue OnDequeue;

        public event OnDUQueueClear OnClear;

        public DUQueue()
        {
            queue = Queue.Synchronized(new Queue());
        }

        public void Enqueue(OperationInfo op)
        {
            queue.Enqueue(op);
            if (OnEnqueue != null)
                OnEnqueue(this, op);
        }

        public OperationInfo Dequeue()
        {
            OperationInfo op = (OperationInfo)queue.Dequeue();
            if (op != null && OnDequeue != null)
                OnDequeue(this, op);
            return op;
        }

        public bool Contains(OperationInfo op)
        {
            return queue.Contains(op);
        }

        public void Clear()
        {
            queue.Clear();
            if (OnClear != null)
                OnClear(this);
        }

        public IEnumerator GetEnumerator()
        {
            return queue.GetEnumerator();
        }
    }

    public delegate void OnDUQueueEnqueue(object sender, OperationInfo op);

    public delegate void OnDUQueueDequeue(object sender, OperationInfo op);

    public delegate void OnDUQueueClear(object sender);

}
