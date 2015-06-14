using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FileExplorer
{
    public partial class frmSettings : Form
    {
        public frmSettings()
        {
            InitializeComponent();
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            numMinDownloadSliceSize.Maximum = MultiThreadDownloader.MAX_SLICE_SIZE / Utils.KB;
            numMinDownloadSliceSize.Minimum = MultiThreadDownloader.MIN_SLICE_SIZE / Utils.KB;
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            ckResumeDownloadAndUploadOnStartup.Checked = AppSettings.ResumeDownloadAndUploadOnStartup;

            if (AppSettings.DownloadMaxThreadCount >= numDownloadMaxThreadCount.Minimum && AppSettings.DownloadMaxThreadCount <= numDownloadMaxThreadCount.Maximum)
                numDownloadMaxThreadCount.Value = AppSettings.DownloadMaxThreadCount;
            ckAutomaticDownloadMaxThreadCount.Checked = AppSettings.AutomaticDownloadMaxThreadCount;
            ckRetryWhenDownloadFail.Checked = AppSettings.RetryWhenDownloadFailed;
            if (AppSettings.MinDownloasSliceSize >= numMinDownloadSliceSize.Minimum && AppSettings.MinDownloasSliceSize <= numMinDownloadSliceSize.Maximum)
                numMinDownloadSliceSize.Value = AppSettings.MinDownloasSliceSize;

            if (AppSettings.UploadMaxThreadCount >= numUploadMaxThreadCount.Minimum && AppSettings.UploadMaxThreadCount <= numUploadMaxThreadCount.Maximum)
                numUploadMaxThreadCount.Value = AppSettings.UploadMaxThreadCount;
            ckAutomaticUploadMaxThreadCount.Checked = AppSettings.AutomaticUploadMaxThreadCount;
            ckRetryWhenUploadFail.Checked = AppSettings.RetryWhenUploadFailed;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            AppSettings.ResumeDownloadAndUploadOnStartup = ckResumeDownloadAndUploadOnStartup.Checked;

            AppSettings.DownloadMaxThreadCount = (int)numDownloadMaxThreadCount.Value;
            AppSettings.AutomaticDownloadMaxThreadCount = ckAutomaticDownloadMaxThreadCount.Checked;
            AppSettings.RetryWhenDownloadFailed = ckRetryWhenDownloadFail.Checked;
            AppSettings.MinDownloasSliceSize = (int)numMinDownloadSliceSize.Value;

            AppSettings.UploadMaxThreadCount = (int)numUploadMaxThreadCount.Value;
            AppSettings.AutomaticUploadMaxThreadCount = ckAutomaticUploadMaxThreadCount.Checked;
            AppSettings.RetryWhenUploadFailed = ckRetryWhenUploadFail.Checked;

            AppSettings.Save();

            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
