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
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            ckResumeDownloadAndUploadOnStartup.Checked = AppSettings.ResumeDownloadAndUploadOnStartup;

            numDownloadMaxThreadCount.Value = AppSettings.DownloadMaxThreadCount;
            ckAutomaticDownloadMaxThreadCount.Checked = AppSettings.AutomaticDownloadMaxThreadCount;
            ckRetryWhenDownloadFail.Checked = AppSettings.RetryWhenDownloadFailed;

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
