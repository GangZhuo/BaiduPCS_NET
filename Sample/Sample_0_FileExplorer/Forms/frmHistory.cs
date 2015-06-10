using System;
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
            worker.OnReset += worker_OnReset;
            worker.OnProgressChange += worker_OnProgressChange;
            worker.OnUploaded += worker_OnUploaded;
            worker.OnDownloaded += worker_OnDownloaded;
        }

        private void frmHistory_Load(object sender, EventArgs e)
        {
            Bind();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            worker.queue.OnEnqueue -= queue_OnEnqueue;
            worker.OnReset -= worker_OnReset;
            worker.OnProgressChange -= worker_OnProgressChange;
            worker.OnUploaded -= worker_OnUploaded;
            worker.OnDownloaded -= worker_OnDownloaded;

            base.OnClosing(e);
        }

        private void worker_OnReset(object sender)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new OnDUWorkerReset(worker_OnReset), new object[] { sender });
            }
            else
            {
                Bind();
            }
        }

        private void queue_OnEnqueue(object sender, OperationInfo op)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new OnDUQueueEnqueue(queue_OnEnqueue), new object[] { sender, op });
            }
            else
            {
                AddItem(op);
            }
        }

        private void worker_OnProgressChange(object sender, OperationInfo op)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new OnDUWorkerProgressChange(worker_OnProgressChange), new object[] { sender, op });
            }
            else
            {
                UpdateProgress(op);
            }
        }

        private void worker_OnDownloaded(object sender, OperationInfo op)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new OnDUWorkerDownloaded(worker_OnDownloaded), new object[] { sender, op });
            }
            else
            {
                if (op.status == OperationStatus.Success)
                {
                    MoveToCompletedDownload(op);
                }
                else
                {
                    ProgressListview.ProgressSubItem progress = op.Tag as ProgressListview.ProgressSubItem;
                    if (progress != null)
                    {
                        if (op.status == OperationStatus.Cancel)
                        {
                            progress.ShowProgress = false;
                            progress.Text = GetStatusText(op.status);
                        }
                        else
                        {
                            progress.ShowProgress = false;
                            progress.Text = GetStatusText(op.status) + ": " + op.errmsg;
                        }
                    }
                }
            }
        }

        private void worker_OnUploaded(object sender, OperationInfo op)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new OnDUWorkerUploaded(worker_OnUploaded), new object[] { sender, op });
            }
            else
            {
                if (op.status == OperationStatus.Success)
                {
                    MoveToCompletedUpload(op);
                }
                else
                {
                    ProgressListview.ProgressSubItem progress = op.Tag as ProgressListview.ProgressSubItem;
                    if (progress != null)
                    {
                        if (op.status == OperationStatus.Cancel)
                        {
                            progress.ShowProgress = false;
                            progress.Text = GetStatusText(op.status);
                        }
                        else
                        {
                            progress.ShowProgress = false;
                            progress.Text = GetStatusText(op.status) + ": " + op.errmsg;
                        }
                    }
                }
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
                progress.ProgressMaxValue = op.total;
                progress.ProgressValue = op.finished;

                double percent = 0;
                if (op.total > 0)
                {
                    percent = (double)op.finished / (double)op.total;
                    if (percent > 1.0f)
                        percent = 1.0f;
                }
                progress.Text = string.Format("{0}%  {1}/{2}", percent.ToString("F2"), Utils.HumanReadableSize(op.finished), Utils.HumanReadableSize(op.total));
            }
        }

        private void Bind()
        {
            lvDownloading.Items.Clear();
            lvUploading.Items.Clear();
            foreach (OperationInfo op in worker.queue)
            {
                AddItem(op);
            }

            lvCompletedDownload.Items.Clear();
            foreach (OperationInfo op in worker.completedDownload)
            {
                AddItem(op);
            }

            lvCompletedUpload.Items.Clear();
            foreach (OperationInfo op in worker.completedUpload)
            {
                AddItem(op);
            }
        }

        private void AddDownloadingItem(OperationInfo op)
        {
            ListViewItem item = new ListViewItem(op.from);
            item.Tag = op;
            ListViewItem.ListViewSubItem subitem = new ListViewItem.ListViewSubItem(item, op.to);
            item.SubItems.Add(subitem);
            ProgressListview.ProgressSubItem progress = new ProgressListview.ProgressSubItem(item, GetStatusText(op.status));
            progress.Owner = item;
            progress.ProgressMaxValue = op.total;
            progress.ProgressValue = op.finished;
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
            ProgressListview.ProgressSubItem progress = new ProgressListview.ProgressSubItem(item, GetStatusText(op.status));
            progress.Owner = item;
            progress.ProgressMaxValue = op.total;
            progress.ProgressValue = op.finished;
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
            subitem = new ListViewItem.ListViewSubItem(item, GetStatusText(op.status));
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
            subitem = new ListViewItem.ListViewSubItem(item, GetStatusText(op.status));
            subitem.Tag = op;
            item.SubItems.Add(subitem);
            lvCompletedUpload.Items.Add(item);
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

        private string GetStatusText(OperationStatus status)
        {
            switch(status)
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
                    return "Fail";
                default:
                    return string.Empty;
            }
        }

    }
}
