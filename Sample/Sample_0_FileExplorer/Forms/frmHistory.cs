using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FileExplorer
{
    public partial class frmHistory : Form
    {
        private DUWorker worker;
        private Hashtable updatedOp;

        public frmHistory(DUWorker worker)
        {
            InitializeComponent();
            this.worker = worker;
            worker.queue.OnEnqueue += queue_OnEnqueue;
            worker.queue.OnRemove += queue_OnRemove;
            worker.queue.OnClear += queue_OnClear;

            worker.OnProgress += worker_OnProgress;
            worker.OnCompleted += worker_OnCompleted;

            updatedOp = Hashtable.Synchronized(new Hashtable());
        }

        private void frmHistory_Load(object sender, EventArgs e)
        {
            Bind();
            timer1.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            timer1.Stop();
            worker.queue.OnEnqueue -= queue_OnEnqueue;
            worker.queue.OnRemove -= queue_OnRemove;
            worker.queue.OnClear -= queue_OnClear;

            worker.OnProgress -= worker_OnProgress;
            worker.OnCompleted -= worker_OnCompleted;

            base.OnClosing(e);
        }

        private void queue_OnClear(object sender, EventArgs e)
        {
            if (lvItems.InvokeRequired)
                lvItems.Invoke(new AnonymousFunction(delegate() { Bind(); }));
            else
                Bind();
        }

        private void queue_OnRemove(object sender, DUQueueEventArgs e)
        {
            if (lvItems.InvokeRequired)
                lvItems.Invoke(new AnonymousFunction(delegate() { RemoveItem(e.op); }));
            else
                RemoveItem(e.op);
        }

        private void queue_OnEnqueue(object sender, DUQueueEventArgs e)
        {
            if (lvItems.InvokeRequired)
                lvItems.Invoke(new AnonymousFunction(delegate() { AddItem(e.op); }));
            else
                AddItem(e.op, 0);
        }

        private void worker_OnCompleted(object sender, DUWorkerEventArgs e)
        {
            ProgressListview.ProgressSubItem progress = e.op.Tag as ProgressListview.ProgressSubItem;
            if (progress != null && progress.Owner != null)
            {
                if (lvItems.InvokeRequired)
                {
                    lvItems.Invoke(new AnonymousFunction(delegate()
                    {
                        progress.ShowProgress = false;
                        progress.ForeColor = GetStatusColor(e.op);
                        progress.Text = GetStatusText(e.op);
                    }));
                }
                else
                {
                    progress.ShowProgress = false;
                    progress.ForeColor = GetStatusColor(e.op);
                    progress.Text = GetStatusText(e.op);
                }
            }
        }

        private void worker_OnProgress(object sender, DUWorkerEventArgs e)
        {
            lock (this)
            {
                if (!updatedOp.Contains(e.op))
                    updatedOp.Add(e.op, e.op);
            }
        }

        private void UpdateProgress(OperationInfo op)
        {
            ProgressListview.ProgressSubItem progress = op.Tag as ProgressListview.ProgressSubItem;
            if (progress != null)
            {
                progress.ProgressMaxValue = op.totalSize;
                progress.ProgressValue = op.doneSize;
                progress.ForeColor = GetStatusColor(op);
                progress.ShowProgress = true;

                double percent = 0;
                if (op.totalSize > 0)
                {
                    percent = (double)op.doneSize / (double)op.totalSize;
                    if (percent > 1.0f)
                        percent = 1.0f;
                }
                progress.Text = string.Format("{0}%  {1}/{2}", percent.ToString("F2"), Utils.HumanReadableSize(op.doneSize), Utils.HumanReadableSize(op.totalSize));
            }
        }

        private void Bind()
        {
            lvItems.Items.Clear();
            foreach (OperationInfo op in worker.queue.list())
            {
                AddItem(op);
            }
        }

        private void AddItem(OperationInfo op, int i = -1)
        {
            ListViewItem item = new ListViewItem(op.from);
            item.Tag = op;
            if (op.operation == Operation.Download)
                item.ImageIndex = 1;
            else
                item.ImageIndex = 0;
            ListViewItem.ListViewSubItem subitem = new ListViewItem.ListViewSubItem(item, op.to);
            item.SubItems.Add(subitem);
            ProgressListview.ProgressSubItem progress = new ProgressListview.ProgressSubItem(item, GetStatusText(op));
            progress.Owner = item;
            progress.ProgressMaxValue = op.totalSize;
            progress.ProgressValue = op.doneSize;
            progress.ShowProgress = false;
            progress.ForeColor = GetStatusColor(op);
            progress.Tag = op;
            op.Tag = progress;
            item.SubItems.Add(progress);
            if (i >= 0)
                lvItems.Items.Insert(i, item);
            else
                lvItems.Items.Add(item);
        }

        private void RemoveItem(OperationInfo op)
        {
            ProgressListview.ProgressSubItem progress = op.Tag as ProgressListview.ProgressSubItem;
            if (progress != null && progress.Owner != null)
            {
                lvItems.Items.Remove(progress.Owner);
            }
        }

        private string GetStatusText(OperationInfo op)
        {
            switch (op.status)
            {
                case OperationStatus.Pending:
                    return "Pending";
                case OperationStatus.Processing:
                    return "Processing";
                case OperationStatus.Cancel:
                    return "Cancel";
                case OperationStatus.Success:
                    return "Success";
                case OperationStatus.Fail:
                    return "Fail: " + op.errmsg;
                default:
                    return string.Empty;
            }
        }

        private Color GetStatusColor(OperationInfo op)
        {
            switch (op.status)
            {
                case OperationStatus.Pending:
                    return Color.Black;
                case OperationStatus.Processing:
                    return Color.Blue;
                case OperationStatus.Cancel:
                    return Color.Gray;
                case OperationStatus.Success:
                    return Color.Green;
                case OperationStatus.Fail:
                    return Color.Red;
                default:
                    return Color.Black;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (updatedOp.Count > 0)
                {
                    lock (this)
                    {
                        if (updatedOp.Count > 0)
                        {
                            //lvItems.BeginUpdate();

                            foreach (OperationInfo op in updatedOp.Keys)
                            {
                                if (this.InvokeRequired)
                                    this.Invoke(new AnonymousFunction(delegate() { UpdateProgress(op); }));
                                else
                                    UpdateProgress(op);
                            }

                            //lvItems.EndUpdate();

                            updatedOp.Clear();
                        }
                    }
                }
            }
            catch { }

        }

    }
}
