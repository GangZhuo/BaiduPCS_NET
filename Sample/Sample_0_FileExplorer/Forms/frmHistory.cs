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

        public int SelectedTabIndex
        {
            get { return tabControl1.SelectedIndex; }
            set { tabControl1.SelectedIndex = value; }
        }

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
            if (this.InvokeRequired)
                this.Invoke(new AnonymousFunction(delegate() { Bind(); }));
            else
                Bind();
        }

        private void queue_OnRemove(object sender, DUQueueEventArgs e)
        {
            if (this.InvokeRequired)
                this.Invoke(new AnonymousFunction(delegate() { RemoveItem(e.op); }));
            else
                RemoveItem(e.op);
        }

        private void queue_OnEnqueue(object sender, DUQueueEventArgs e)
        {
            if (this.InvokeRequired)
                this.Invoke(new AnonymousFunction(delegate() { AddItem(e.op); }));
            else
                AddItem(e.op);
        }

        private void worker_OnCompleted(object sender, DUWorkerEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AnonymousFunction(delegate() { 
                    RemoveItem(e.op);
                    AddItem(e.op);
                }));
            }
            else
            {
                RemoveItem(e.op);
                AddItem(e.op);
            }
        }

        private void worker_OnProgress(object sender, DUWorkerEventArgs e)
        {
            lock(this)
            {
                if (!updatedOp.Contains(e.op))
                    updatedOp.Add(e.op, e.op);
            }
        }

        private void MoveToCompletedDownload(OperationInfo op)
        {
            ProgressListview.ProgressSubItem progress = op.Tag as ProgressListview.ProgressSubItem;
            if (progress != null && progress.Owner != null)
            {
                lvDownloading.Items.Remove(progress.Owner);
                AddCompletedDownloadItem(op);
            }
        }

        private void MoveToCompletedUpload(OperationInfo op)
        {
            ProgressListview.ProgressSubItem progress = op.Tag as ProgressListview.ProgressSubItem;
            if (progress != null && progress.Owner != null)
            {
                lvUploading.Items.Remove(progress.Owner);
                AddCompletedUploadItem(op);
            }
        }

        private void UpdateProgress(OperationInfo op)
        {
            ProgressListview.ProgressSubItem progress = op.Tag as ProgressListview.ProgressSubItem;
            if (progress != null)
            {
                progress.ProgressMaxValue = op.totalSize;
                progress.ProgressValue = op.doneSize;

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
            lvDownloading.Items.Clear();
            lvUploading.Items.Clear();
            lvCompletedDownload.Items.Clear();
            lvCompletedUpload.Items.Clear();
            foreach (OperationInfo op in worker.queue.list())
            {
                AddItem(op);
            }
        }

        private void AddItem(OperationInfo op)
        {
            if (op.status == OperationStatus.Success)
            {
                if (op.operation == Operation.Download)
                    AddCompletedDownloadItem(op);
                else if (op.operation == Operation.Upload)
                    AddCompletedUploadItem(op);
            }
            else
            {
                if (op.operation == Operation.Download)
                    AddDownloadingItem(op);
                else if (op.operation == Operation.Upload)
                    AddUploadingItem(op);
            }
        }

        private void RemoveItem(OperationInfo op)
        {
            ProgressListview.ProgressSubItem progress = op.Tag as ProgressListview.ProgressSubItem;
            if (progress != null && progress.Owner != null)
            {
                lvDownloading.Items.Remove(progress.Owner);
                lvUploading.Items.Remove(progress.Owner);
                lvCompletedDownload.Items.Remove(progress.Owner);
                lvCompletedUpload.Items.Remove(progress.Owner);
            }
        }

        private void AddDownloadingItem(OperationInfo op)
        {
            ListViewItem item = new ListViewItem(op.from);
            item.Tag = op;
            ListViewItem.ListViewSubItem subitem = new ListViewItem.ListViewSubItem(item, op.to);
            item.SubItems.Add(subitem);
            ProgressListview.ProgressSubItem progress = new ProgressListview.ProgressSubItem(item, GetStatusText(op));
            progress.Owner = item;
            progress.ProgressMaxValue = op.totalSize;
            progress.ProgressValue = op.doneSize;
            progress.Tag = op;
            op.Tag = progress;
            item.SubItems.Add(progress);
            lvDownloading.Items.Add(item);
        }

        private void AddUploadingItem(OperationInfo op)
        {
            ListViewItem item = new ListViewItem(op.from);
            item.Tag = op;
            ListViewItem.ListViewSubItem subitem = new ListViewItem.ListViewSubItem(item, op.to);
            item.SubItems.Add(subitem);
            ProgressListview.ProgressSubItem progress = new ProgressListview.ProgressSubItem(item, GetStatusText(op));
            progress.Owner = item;
            progress.ProgressMaxValue = op.totalSize;
            progress.ProgressValue = op.doneSize;
            progress.Tag = op;
            op.Tag = progress;
            item.SubItems.Add(progress);
            lvUploading.Items.Add(item);
        }

        private void AddCompletedDownloadItem(OperationInfo op)
        {
            ListViewItem item = new ListViewItem(op.from);
            item.Tag = op;
            ListViewItem.ListViewSubItem subitem = new ListViewItem.ListViewSubItem(item, op.to);
            item.SubItems.Add(subitem);
            subitem = new ListViewItem.ListViewSubItem(item, GetStatusText(op));
            subitem.Tag = op;
            item.SubItems.Add(subitem);
            lvCompletedDownload.Items.Add(item);
        }

        private void AddCompletedUploadItem(OperationInfo op)
        {
            ListViewItem item = new ListViewItem(op.from);
            item.Tag = op;
            ListViewItem.ListViewSubItem subitem = new ListViewItem.ListViewSubItem(item, op.to);
            item.SubItems.Add(subitem);
            subitem = new ListViewItem.ListViewSubItem(item, GetStatusText(op));
            subitem.Tag = op;
            item.SubItems.Add(subitem);
            lvCompletedUpload.Items.Add(item);
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                lock(this)
                {
                    lvDownloading.BeginUpdate();
                    lvUploading.BeginUpdate();

                    foreach(OperationInfo op in updatedOp.Keys)
                    {
                        if (this.InvokeRequired)
                            this.Invoke(new AnonymousFunction(delegate() { UpdateProgress(op); }));
                        else
                            UpdateProgress(op);
                    }

                    lvUploading.EndUpdate();
                    lvDownloading.EndUpdate();

                    updatedOp.Clear();
                }
            }
            catch { }

        }

    }
}
