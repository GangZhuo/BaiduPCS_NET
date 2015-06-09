using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using BaiduPCS_NET;

namespace FileExplorer
{
    public partial class frmMetaInformation : Form
    {
        public frmMetaInformation(PcsFileInfo fileInfo)
        {
            InitializeComponent();
            if (fileInfo.isdir)
            {
                txFileName.Text = fileInfo.server_filename;
                txFileType.Text = "Directory";
                txLocation.Text = fileInfo.path;
                txSize.Text = fileInfo.size.ToString() + "Bytes (" + Utils.HumanReadableSize(fileInfo.size) + ")";
                txCreateTime.Text = Utils.FromUnixTimeStamp(fileInfo.server_ctime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                txModifyTime.Text = Utils.FromUnixTimeStamp(fileInfo.server_mtime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                txMd5.Text = fileInfo.md5;
                
            }
            else
            {
                txFileName.Text = fileInfo.server_filename;
                txFileType.Text = Path.GetExtension(fileInfo.server_filename);
                txLocation.Text = fileInfo.path;
                txSize.Text = fileInfo.size.ToString() + "Bytes (" + Utils.HumanReadableSize(fileInfo.size) + ")";
                txCreateTime.Text = Utils.FromUnixTimeStamp(fileInfo.server_ctime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                txModifyTime.Text = Utils.FromUnixTimeStamp(fileInfo.server_mtime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                txMd5.Text = fileInfo.md5;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
