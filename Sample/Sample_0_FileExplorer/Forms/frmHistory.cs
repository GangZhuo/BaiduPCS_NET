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

        private object locker = new object();

        public frmHistory(DUWorker worker)
        {
            InitializeComponent();

            lvItems.DoubleClick += lvItems_DoubleClick;
            lvItems.KeyDown += lvItems_KeyDown;

            this.worker = worker;
            worker.queue.OnEnqueue += queue_OnEnqueue;
            worker.queue.OnRemove += queue_OnRemove;
            worker.queue.OnClear += queue_OnClear;
            worker.queue.OnClearSuccessItems += queue_OnClearSuccessItems;

            worker.OnProgress += worker_OnProgress;
            worker.OnCompleted += worker_OnCompleted;

            updatedOp = Hashtable.Synchronized(new Hashtable());
        }

        private void frmHistory_Load(object sender, EventArgs e)
        {
            Bind();
            RefreshControls();
            timer1.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            timer1.Stop();
            worker.queue.OnEnqueue -= queue_OnEnqueue;
            worker.queue.OnRemove -= queue_OnRemove;
            worker.queue.OnClear -= queue_OnClear;
            worker.queue.OnClearSuccessItems -= queue_OnClearSuccessItems;

            worker.OnProgress -= worker_OnProgress;
            worker.OnCompleted -= worker_OnCompleted;

            base.OnClosing(e);
        }

        private void lvItems_DoubleClick(object sender, EventArgs e)
        {
            if (lvItems.SelectedItems.Count == 0)
                return;
            OperationInfo op = lvItems.SelectedItems[0].Tag as OperationInfo;
            if (op != null)
            {
                if (op.operation == Operation.Download)
                {
                    try
                    {
                        if (System.IO.File.Exists(op.to))
                            System.Diagnostics.Process.Start("Explorer", "/select,\"" + op.to + "\"");
                        else
                            System.Diagnostics.Process.Start("Explorer", "\"" + System.IO.Path.GetDirectoryName(op.to) + "\"");
                    }
                    catch { }
                }
                else if (op.operation == Operation.Upload)
                {
                    try
                    {
                        if (System.IO.File.Exists(op.from))
                            System.Diagnostics.Process.Start("Explorer", "/select,\"" + op.from + "\"");
                        else
                            System.Diagnostics.Process.Start("Explorer", "\"" + System.IO.Path.GetDirectoryName(op.from) + "\"");
                    }
                    catch { }
                }
            }
        }

        private void lvItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in lvItems.Items)
                {
                    item.Selected = true;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                Delete();
            }
        }

        private void queue_OnClear(object sender, EventArgs e)
        {
            if (lvItems.InvokeRequired)
                lvItems.Invoke(new AnonymousFunction(delegate() { Bind(); RefreshControls(); }));
            else
            {
                Bind();
                RefreshControls();
            }
        }

        private void queue_OnClearSuccessItems(object sender, EventArgs e)
        {
            if (lvItems.InvokeRequired)
                lvItems.Invoke(new AnonymousFunction(delegate() { Bind(); RefreshControls(); }));
            else
            {
                Bind();
                RefreshControls();
            }
        }

        private void queue_OnRemove(object sender, DUQueueEventArgs e)
        {
            if (lvItems.InvokeRequired)
                lvItems.Invoke(new AnonymousFunction(delegate() {
                    lock (locker)
                    {
                        if (updatedOp.Contains(e.op))
                            updatedOp.Remove(e.op);
                    }
                    RemoveItem(e.op);
                    RefreshControls();
                }));
            else
            {
                lock (locker)
                {
                    if (updatedOp.Contains(e.op))
                        updatedOp.Remove(e.op);
                }
                RemoveItem(e.op);
                RefreshControls();
            }
        }

        private void queue_OnEnqueue(object sender, DUQueueEventArgs e)
        {
            if (lvItems.InvokeRequired)
                lvItems.Invoke(new AnonymousFunction(delegate() { AddItem(e.op); RefreshControls(); }));
            else
            {
                AddItem(e.op, 0);
                RefreshControls();
            }
        }

        private void worker_OnCompleted(object sender, DUWorkerEventArgs e)
        {
            if (e.op.status == OperationStatus.Fail)
            {
                if (AppSettings.RetryWhenDownloadFailed && e.op.operation == Operation.Download)
                {
                    if (lvItems.InvokeRequired)
                        lvItems.Invoke(new AnonymousFunction(delegate() { ChangeOpStatus(e.op, OperationStatus.Pending); }));
                    else
                        ChangeOpStatus(e.op, OperationStatus.Pending);
                    return;
                }
                else if (AppSettings.RetryWhenUploadFailed && e.op.operation == Operation.Upload)
                {
                    if (lvItems.InvokeRequired)
                        lvItems.Invoke(new AnonymousFunction(delegate() { ChangeOpStatus(e.op, OperationStatus.Pending); }));
                    else
                        ChangeOpStatus(e.op, OperationStatus.Pending);
                    return;
                }
            }
            ProgressListview.ProgressSubItem progress = e.op.Tag as ProgressListview.ProgressSubItem;
            if (progress != null && progress.Owner != null)
            {
                lock (locker)
                {
                    if (updatedOp.Contains(e.op))
                        updatedOp.Remove(e.op);
                }
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
            if(e.op.status == OperationStatus.Processing)
            {
                lock (locker)
                {
                    if (!updatedOp.Contains(e.op))
                        updatedOp.Add(e.op, e.op);
                }
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            worker.Resume();
            RefreshControls();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            worker.Pause();
            RefreshControls();
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure want to clear all successed items?", "Clean", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                worker.queue.ClearSuccessItems();
                RefreshControls();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            bool resume = false;
            bool pause = false;
            bool cancel = false;
            bool delete = false;
            foreach(ListViewItem item in lvItems.SelectedItems)
            {
                OperationInfo op = item.Tag as OperationInfo;
                if (op.status == OperationStatus.Cancel
                    || op.status == OperationStatus.Fail
                    || op.status == OperationStatus.Pause)
                    resume = true;
                if (op.status == OperationStatus.Pending
                    || op.status == OperationStatus.Processing)
                    pause = true;
                if (op.status == OperationStatus.Pending
                    || op.status == OperationStatus.Processing
                    || op.status == OperationStatus.Pause)
                    cancel = true;
                if (op.status != OperationStatus.Processing)
                    delete = true;
                if (resume && pause && cancel && delete)
                    break;
            }
            if(!resume && !pause && !cancel && !delete)
            {
                e.Cancel = true;
                return;
            }
            resumeToolStripMenuItem.Visible = resume;
            pauseToolStripMenuItem.Visible = pause;
            cancelToolStripMenuItem.Visible = cancel;
            deleteToolStripMenuItem.Visible = delete;
        }

        private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvItems.SelectedItems)
            {
                OperationInfo op = item.Tag as OperationInfo;
                if (op.status == OperationStatus.Cancel
                    || op.status == OperationStatus.Fail
                    || op.status == OperationStatus.Pause)
                {
                    ChangeOpStatus(op, OperationStatus.Pending);
                }
            }
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure want to pause the items?", "Pause", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (ListViewItem item in lvItems.SelectedItems)
                {
                    OperationInfo op = item.Tag as OperationInfo;
                    if (op.status == OperationStatus.Pending
                        || op.status == OperationStatus.Processing)
                    {
                        ChangeOpStatus(op, OperationStatus.Pause);
                    }
                }
            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure want to cancel the items?", "Cancel", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (ListViewItem item in lvItems.SelectedItems)
                {
                    OperationInfo op = item.Tag as OperationInfo;
                    if (op.status == OperationStatus.Pending
                        || op.status == OperationStatus.Processing
                        || op.status == OperationStatus.Pause)
                    {
                        ChangeOpStatus(op, OperationStatus.Cancel);
                    }
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                lblStatusST.Text = lvItems.Items.Count.ToString() + " items";
                if (updatedOp.Count > 0)
                {
                    lock (locker)
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

        private void ChangeOpStatus(OperationInfo op, OperationStatus status)
        {
            op.status = status;
            worker.queue.place(op);
            ProgressListview.ProgressSubItem progress = op.Tag as ProgressListview.ProgressSubItem;
            if (progress != null)
            {
                progress.ForeColor = GetStatusColor(op);
                progress.Text = GetStatusText(op);
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

        private void RefreshControls()
        {
            btnPlay.Enabled = worker.IsPause;
            btnPause.Enabled = !btnPlay.Enabled;
            btnClean.Enabled = lvItems.Items.Count > 0;
            lblStatus.Text = btnPause.Enabled ? "The download/upload worker running..."
                : "The download/upload worker stopped. Set auto start up on settings window.";
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
                case OperationStatus.Pause:
                    return "Pause";
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
                case OperationStatus.Pause:
                    return Color.Orange;
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

        private void Delete()
        {
            if (MessageBox.Show("Are you sure want to delete the items?", "Delete", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (ListViewItem item in lvItems.SelectedItems)
                {
                    OperationInfo op = item.Tag as OperationInfo;
                    if (op.status != OperationStatus.Processing)
                    {
                        worker.queue.Remove(op);
                    }
                }
            }
        }
    }
}
