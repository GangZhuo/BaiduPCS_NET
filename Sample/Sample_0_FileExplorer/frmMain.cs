using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using BaiduPCS_NET;

namespace FileExplorer
{
    public partial class frmMain : Form
    {
        private BaiduPCS pcs = null;
        private Stack<string> history = null;
        private Stack<string> next = null;
        private string currentPath = string.Empty; //当前路径
        private ListViewItem contextItem = null;

        public frmMain()
        {
            InitializeComponent();
            history = new Stack<string>();
            next = new Stack<string>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lvFileList.DoubleClick += lvFileList_DoubleClick;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            cmbLocation.Width = this.Width - 32;
        }

        private void cmbLocation_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string newPath = cmbLocation.Text;
                Go(newPath);
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            string newPath = cmbLocation.Text;
            Go(newPath);
        }

        private void lvFileList_DoubleClick(object sender, EventArgs e)
        {
            if (lvFileList.SelectedItems.Count == 0)
                return;
            PcsFileInfo fileInfo = (PcsFileInfo)lvFileList.SelectedItems[0].Tag;
            if(fileInfo.isdir)
            {
                Go(fileInfo.path);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Back();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Next();
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            Up();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshFileList();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
        }

        private void btnNewFolder_Click(object sender, EventArgs e)
        {

        }

        private void btnUserInformation_Click(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {

        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Console.WriteLine("OnShown");
            if(pcs == null)
            {
                lblStatus.Text = "Logging in...";
                new Thread(new ThreadStart(delegate() {
                    if (!createBaiduPCS())
                    {
                        MessageBox.Show("Can't create BaiduPCS");
                        Application.Exit();
                        return;
                    }
                    this.Invoke(new AnonymousFunction(delegate()
                    {
                        Go("/");
                    }));
                })).Start();

            }
        }

        private bool createBaiduPCS()
        {
            string cookiefilename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".pcs", "cookie.txt");
            string dir = System.IO.Path.GetDirectoryName(cookiefilename);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            pcs = BaiduPCS.pcs_create(cookiefilename);
            if (pcs == null)
                return false;
            if (pcs.isLogin() != PcsRes.PCS_LOGIN)
            {
                System.Windows.Forms.DialogResult dr = System.Windows.Forms.DialogResult.None;
                this.Invoke(new AnonymousFunction(delegate()
                {
                    frmLogin frm = new frmLogin(pcs);
                    dr = frm.ShowDialog();
                }));
                if (dr != System.Windows.Forms.DialogResult.OK)
                    return false;
            }
            long quota, used;
            pcs.quota(out quota, out used);
            string title = pcs.getUID() + "'s Baidu Cloud Disk " + Utils.HumanReadableSize(used) + "/" + Utils.HumanReadableSize(quota);
            this.Invoke(new AnonymousFunction(delegate() {
                Text = title;
                lblStatus.Text = title;
            }));
            return true;
        }

        private void Go(string newPath)
        {
            if (newPath == "")
                return;
            if (!newPath.StartsWith("/"))
            {
                MessageBox.Show("Must be the full path. e.g. /movies");
                return;
            }
            if(ListFiles(newPath))
            {
                if (!string.IsNullOrEmpty(currentPath))
                    history.Push(currentPath);
                next.Clear();
                currentPath = newPath;
                RefreshControls();
            }
        }

        private void Back()
        {
            string path = history.Pop();
            if (!string.IsNullOrEmpty(path))
            {
                if (ListFiles(path))
                {
                    if (!string.IsNullOrEmpty(currentPath))
                        next.Push(currentPath);
                    currentPath = path;
                    RefreshControls();
                }
            }
        }

        private void Next()
        {
            string path = next.Pop();
            if (!string.IsNullOrEmpty(path))
            {
                if (ListFiles(path))
                {
                    if (!string.IsNullOrEmpty(currentPath))
                        history.Push(currentPath);
                    currentPath = path;
                    RefreshControls();
                }
            }
        }

        private void Up()
        {
            string path = Path.GetDirectoryName(currentPath).Replace("\\", "/");
            if (!string.IsNullOrEmpty(path) && path != currentPath)
            {
                Go(path);
            }
        }

        private void RefreshFileList()
        {
            ListFiles(currentPath);
        }

        private bool ListFiles(string path)
        {
            frmWaiting frm = new frmWaiting("Loading", "Loading files ...");
            PcsFileInfo[] list = null;
            string errmsg = null;
            new Thread(new ThreadStart(delegate()
            {
                try
                {
                    list = pcs.list(path, 1, int.MaxValue);
                    if (list == null)
                        errmsg = pcs.getError();
                }
                catch(Exception ex)
                {
                    errmsg = ex.Message;
                }
                finally
                {
                    this.Invoke(new AnonymousFunction(delegate()
                    {
                        frm.Close();
                    }));
                }
            })).Start();
            frm.ShowDialog();
            if (errmsg != null)
            {
                MessageBox.Show("Can't list the file for " + path + ": " + errmsg);
                return false;
            }
            cmbLocation.Text = path;
            lvFileList.Items.Clear();
            bigImageList.Images.Clear();
            smallImageList.Images.Clear();
            bigImageList.Images.Add(SystemIcon.GetFolderIcon(true));
            smallImageList.Images.Add(SystemIcon.GetFolderIcon(false));
            if (list == null)
            {
                lblStatus.Text = "0 items";//更新状态栏
                return true;
            }
            lblStatus.Text = list.Length.ToString() + " items"; //更新状态栏
            foreach (PcsFileInfo file in list)
            {
                ListViewItem item = lvFileList.Items.Add(file.server_filename);
                item.Tag = file;
                if(file.isdir)
                {
                    item.SubItems.Add("");
                    item.SubItems.Add("Directory");
                    item.SubItems.Add(Utils.FromUnixTimeStamp(file.server_mtime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                    item.ImageIndex = 0;
                }
                else
                {
                    item.SubItems.Add(Utils.HumanReadableSize(file.size));
                    item.SubItems.Add(Path.GetExtension(file.server_filename));
                    item.SubItems.Add(Utils.FromUnixTimeStamp(file.server_mtime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                    bigImageList.Images.Add(file.server_filename, SystemIcon.GetIcon(file, true));
                    smallImageList.Images.Add(file.server_filename, SystemIcon.GetIcon(file, false));
                    item.ImageKey = file.server_filename;
                }
            }
            return true;
        }

        private void RefreshControls()
        {
            btnBack.Enabled = history.Count > 0;
            btnNext.Enabled = next.Count > 0;
            string uppath = Path.GetDirectoryName(currentPath);
            btnUp.Enabled = !string.IsNullOrEmpty(uppath) && uppath != currentPath;

        }

        #region 上下文菜单代码

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            Point point = lvFileList.PointToClient(Cursor.Position);
            ListViewItem item = lvFileList.GetItemAt(point.X, point.Y);   //获得鼠标坐标处的ListViewItem
            contextItem = item;
            if (item == null)   //当前位置没有ListViewItem
            {
                openToolStripMenuItem1.Visible = false;
                uploadFileToolStripMenuItem.Visible = false;
                viewToolStripMenuItem1.Visible = true;
                refreshToolStripMenuItem1.Visible = true;
                cutToolStripMenuItem1.Visible = false;
                copyToolStripMenuItem1.Visible = false;
                deleteToolStripMenuItem1.Visible = false;
                parseToolStripMenuItem1.Visible = true;
                mkdirToolStripMenuItem1.Visible = true;
                attributesToolStripMenuItem1.Visible = true;
            }
            else    //有
            {
                PcsFileInfo fileinfo = (PcsFileInfo)item.Tag;
                if (fileinfo.isdir)
                {
                    openToolStripMenuItem1.Text = "Open Directory";
                    uploadFileToolStripMenuItem.Visible = true;
                }
                else
                {
                    openToolStripMenuItem1.Text = "Download file";
                    uploadFileToolStripMenuItem.Visible = false;
                }
                openToolStripMenuItem1.Visible = true;
                viewToolStripMenuItem1.Visible = false;
                refreshToolStripMenuItem1.Visible = false;
                cutToolStripMenuItem1.Visible = true;
                copyToolStripMenuItem1.Visible = true;
                deleteToolStripMenuItem1.Visible = true;
                parseToolStripMenuItem1.Visible = false;
                mkdirToolStripMenuItem1.Visible = false;
                attributesToolStripMenuItem1.Visible = true;
            }
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (contextItem == null)
                return;
            PcsFileInfo fileinfo = (PcsFileInfo)contextItem.Tag;
            if(fileinfo.isdir)
            {
                Go(fileinfo.path);
            }
            else
            {
                //Download
            }
        }

        private void uploadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void bigIconToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            lvFileList.View = View.LargeIcon;
            bigIconToolStripMenuItem1.Checked = true;
            detailToolStripMenuItem1.Checked = false;
        }

        private void detailToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            lvFileList.View = View.Details;
            bigIconToolStripMenuItem1.Checked = false;
            detailToolStripMenuItem1.Checked = true;
        }

        private void cutToolStripMenuItem2_Click(object sender, EventArgs e)
        {
        }

        private void copyToolStripMenuItem2_Click(object sender, EventArgs e)
        {
        }

        private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
        }

        private void parseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
        }

        private void mkdirToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void attributesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PcsFileInfo fileinfo = new PcsFileInfo();
            Form frm = new frmWaiting("Loading", "Loading meta information ...");
            string errmsg = null;
            if (contextItem == null)
            {
                new Thread(new ThreadStart(delegate()
                {
                    try
                    {
                        fileinfo = pcs.meta(currentPath);
                        if(fileinfo.IsEmpty)
                        {
                            errmsg = pcs.getError();
                        }
                    }
                    catch (Exception ex)
                    {
                        errmsg = ex.Message;
                    }
                    finally
                    {
                        this.Invoke(new AnonymousFunction(delegate()
                        {
                            frm.Close();
                        }));
                    }
                })).Start();
                frm.ShowDialog();
                if (errmsg != null)
                {
                    MessageBox.Show("Can't get meta information for " + currentPath + ": " + errmsg);
                    return;
                }
            }
            else
            {
                fileinfo = (PcsFileInfo)contextItem.Tag;
            }

            if (!fileinfo.IsEmpty)
            {
                frm = new frmMetaInformation(fileinfo);
                frm.ShowDialog();
            }
        }

        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            RefreshFileList();
        }

        #endregion


    }
}
