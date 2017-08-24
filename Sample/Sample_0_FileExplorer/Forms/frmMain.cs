using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        private string currentPath = string.Empty;
        private string lastSearchPath = string.Empty;
        private List<PcsFileInfo> sources;
        private bool isMove = false;
        private frmHistory frmHistory = null;
        private DUWorker worker = null;
        private string tempFileName = null;

        public frmMain()
        {
            InitializeComponent();

            lvFileList.DoubleClick += lvFileList_DoubleClick;
            lvFileList.KeyDown += lvFileList_KeyDown;
            lvFileList.ColumnClick += LvFileList_ColumnClick;

            txSearchKeyword.GotFocus += txSearchKeyword_GotFocus;
            txSearchKeyword.LostFocus += txSearchKeyword_LostFocus;
            txSearchKeyword.KeyPress += txSearchKeyword_KeyPress;

            cmbLocation.KeyPress += cmbLocation_KeyPress;

            history = new Stack<string>();
            next = new Stack<string>();
            sources = new List<PcsFileInfo>();

            worker = new DUWorker();
            worker.workfolder = GetWorkFolder();
            worker.OnCompleted += worker_OnCompleted;

            ReadAppSettings();
            tempFileName = System.IO.Path.GetTempFileName();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (pcs == null)
            {
                bool succ = true;
                ExecTask("Login", "Logging in ...",
                    new ThreadStart(delegate()
                    {
                        try
                        {
                            if (!createBaiduPCS())
                            {
                                succ = false;
                                this.Invoke(new AnonymousFunction(delegate()
                                {
                                    Close();
                                    //MessageBox.Show("Can't create BaiduPCS");
                                    Application.Exit();
                                }));
                                return;
                            }
                        }
                        catch(Exception ex)
                        {
                            succ = false;
                            this.Invoke(new AnonymousFunction(delegate()
                            {
                                MessageBox.Show("Can't create BaiduPCS: " + ex.Message);
                                Close();
                                Application.Exit();
                            }));
                            return;
                        }
                    }),
                    new ThreadStart(delegate()
                    {
                        if (!succ)
                            return;
                        this.Invoke(new AnonymousFunction(delegate()
                        {
                            Go("/");
                            worker.pcs = pcs;
                            worker.Start();
                            if (AppSettings.ResumeDownloadAndUploadOnStartup)
                                worker.Resume();
                        }));
                    }));
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (frmHistory != null)
                frmHistory.Close();
            worker.Stop();
            if (File.Exists(tempFileName))
            {
                try { File.Delete(tempFileName); }
                catch { }
            }
            base.OnClosing(e);
        }

        #region 事件

        private void txSearchKeyword_LostFocus(object sender, EventArgs e)
        {
            string keyword = txSearchKeyword.Text.Trim();
            if (string.IsNullOrEmpty(keyword) || string.Equals(keyword, "Search by filename", StringComparison.InvariantCultureIgnoreCase))
            {
                txSearchKeyword.Text = "Search by filename";
                txSearchKeyword.ForeColor = Color.Gray;
            }
        }

        private void txSearchKeyword_GotFocus(object sender, EventArgs e)
        {
            string keyword = txSearchKeyword.Text.Trim();
            if (string.Equals(keyword, "Search by filename", StringComparison.InvariantCultureIgnoreCase))
            {
                txSearchKeyword.Text = "";
            }
            txSearchKeyword.ForeColor = Color.Black;
        }

        private void txSearchKeyword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnSearch_Click(sender, e);
            }
        }

        private void cmbLocation_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string newPath = cmbLocation.Text;
                Go(newPath);
            }
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
            else
            {
                string mime = MimeMapping.GetMimeMapping(fileInfo.server_filename);
                if (mime.StartsWith("text/"))
                {
                    if (fileInfo.size >= Utils.MB)
                    {
                        MessageBox.Show("File too big.");
                    }
                    else
                    {
                        string content = cat(fileInfo.path);
                        if (content != null)
                        {
                            try
                            {
                                File.WriteAllText(tempFileName, content);
                                System.Diagnostics.Process.Start("notepad.exe", "\"" + tempFileName + "\"");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Not support mime type.");
                }
            }
        }

        private void lvFileList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in lvFileList.Items)
                {
                    item.Selected = true;
                }
            }
            else if (e.KeyCode == Keys.C && e.Control)
            {
                Copy();
            }
            else if (e.KeyCode == Keys.X && e.Control)
            {
                Cut();
            }
            else if (e.KeyCode == Keys.V && e.Control)
            {
                if (sources.Count > 0)
                {
                    PcsFileInfo to;
                    if (currentPath.StartsWith("/"))
                    {
                        to = new PcsFileInfo();
                        to.path = currentPath;
                        to.isdir = true;
                        ParseTo(to);
                    }
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                Delete();
            }
        }

        private void LvFileList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (lvFileList.ListViewItemSorter != null &&
                ((ListViewItemComparer)lvFileList.ListViewItemSorter).Column == e.Column)
            {
                ((ListViewItemComparer)lvFileList.ListViewItemSorter).Descending = !((ListViewItemComparer)lvFileList.ListViewItemSorter).Descending;
            }
            else
            {
                ListViewItemComparer comparer = new ListViewItemComparer();
                comparer.Column = e.Column;
                switch (e.Column)
                {
                    case 0: comparer.By = ListViewItemComparer.ComparerBy.FileName; break;
                    case 1: comparer.By = ListViewItemComparer.ComparerBy.FileSize; break;
                    case 2: comparer.By = ListViewItemComparer.ComparerBy.FileType; break;
                    case 3: comparer.By = ListViewItemComparer.ComparerBy.FileModifyTime; break;
                    default: comparer.By = ListViewItemComparer.ComparerBy.None; break;
                }
                lvFileList.ListViewItemSorter = comparer;
            }
            lvFileList.Sort();
        }

        private void worker_OnCompleted(object sender, DUWorkerEventArgs e)
        {
            if(e.op.operation == Operation.Upload && e.op.status == OperationStatus.Success)
            {
                int i = e.op.to.LastIndexOf('/');
                string dir = e.op.to.Substring(0, i);
                if (string.Equals(dir, currentPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (lvFileList.InvokeRequired)
                        lvFileList.Invoke(new AnonymousFunction(delegate() { RefreshFileList(); }));
                    else
                        RefreshFileList();
                }
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            string newPath = cmbLocation.Text;
            Go(newPath);
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
            PcsFileInfo fileinfo = new PcsFileInfo();
            fileinfo.path = currentPath;
            fileinfo.isdir = true;
            if (UploadFile(fileinfo))
            {
                //if (string.Equals(fileinfo.path, currentPath, StringComparison.InvariantCultureIgnoreCase))
                //    RefreshFileList();
            }
        }

        private void btnNewFolder_Click(object sender, EventArgs e)
        {
            if (CreateNewFolder(currentPath))
            {
                RefreshFileList();
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            ShowSettingsWindow();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keywords = txSearchKeyword.Text.Trim();
            if (string.IsNullOrEmpty(keywords) || string.Equals(keywords, "Search by filename", StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show("Please input the filename.");
                txSearchKeyword.Focus();
                return;
            }
            if (keywords.Length <= 3)
            {
                if (MessageBox.Show("The keywords is too short, the result maybe very large, you maybe need to wait a long time, are you sure to continue?",
                    "Keywords too short", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                {
                    txSearchKeyword.Focus();
                    txSearchKeyword.SelectAll();
                    return;
                }
            }
            string path = currentPath;
            if (!path.StartsWith("/"))
                path = lastSearchPath;
            if (SearchFiles(path, keywords))
            {
                if (!string.IsNullOrEmpty(currentPath) && currentPath.StartsWith("/"))
                    history.Push(currentPath);
                next.Clear();
                lastSearchPath = currentPath;
                currentPath = cmbLocation.Text;
                RefreshControls();
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            ShowHistoryWindow();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure logout?", "Logout", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                worker.Stop();
                if(frmHistory != null)
                    frmHistory.Close();

                Logout();

                BindFileList(null);
                history.Clear();
                next.Clear();
                currentPath = string.Empty;
                sources.Clear();
                isMove = false;
                lastSearchPath = string.Empty;

                if (Login())
                {
                    Go("/");
                    worker.pcs = pcs;
                    worker.Start();
                    if (AppSettings.ResumeDownloadAndUploadOnStartup)
                        worker.Resume();
                }
                else
                {
                    Close();
                    Application.Exit();
                }
            }
        }

        private void btnGithub_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/GangZhuo/BaiduPCS_NET");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region 私有方法

        private void ReadAppSettings()
        {
            AppSettings.SettingsFileName = Path.Combine(GetWorkFolder(), "settings.xml");
            if (!AppSettings.Restore())
            {
                AppSettings.ResumeDownloadAndUploadOnStartup = false;

                AppSettings.AutomaticDownloadMaxThreadCount = true;
                AppSettings.DownloadMaxThreadCount = 1;
                AppSettings.RetryWhenDownloadFailed = false;
                AppSettings.MinDownloasSliceSize = MultiThreadDownloader.MIN_SLICE_SIZE / Utils.KB;

                AppSettings.AutomaticUploadMaxThreadCount = true;
                AppSettings.RetryWhenUploadFailed = false;
                AppSettings.UploadMaxThreadCount = 1;
                AppSettings.MaxCacheSize = 1024;
            }
            else
            {
                if (AppSettings.MaxCacheSize == 0)
                    AppSettings.MaxCacheSize = 1024;
            }
        }

        /// <summary>
        /// 获取工作目录
        /// </summary>
        /// <returns></returns>
        private string GetWorkFolder()
        {
            string dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BaiduCloudDisk");
            return dir;
        }

        private bool createBaiduPCS()
        {
            string cookiefilename = Path.Combine(GetWorkFolder(), "cookie.txt");
            string dir = System.IO.Path.GetDirectoryName(cookiefilename);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            pcs = BaiduPCS.pcs_create(cookiefilename);
            if (pcs == null)
                return false;
            if (pcs.isLogin() != PcsRes.PCS_LOGIN)
            {
                if (pcs.isLogin() != PcsRes.PCS_LOGIN)
                {
                    if (!Login())
                        return false;
                }
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

        private bool Login()
        {
            if(this.InvokeRequired)
            {
                System.Windows.Forms.DialogResult dr = System.Windows.Forms.DialogResult.None;
                this.Invoke(new AnonymousFunction(delegate()
                {
                    frmLogin frm = new frmLogin(pcs);
                    frm.TopMost = true;
                    dr = frm.ShowDialog();
                }));
                if (dr != System.Windows.Forms.DialogResult.OK)
                    return false;
                return true;
            }
            else
            {
                frmLogin frm = new frmLogin(pcs);
                frm.TopMost = true;
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    return true;
                return false;
            }
        }

        private void Logout()
        {
            string errmsg = null;
            ExecTask(new ThreadStart(delegate()
            {
                try
                {
                    PcsRes rc = pcs.logout();
                    if (rc != PcsRes.PCS_OK)
                    {
                        errmsg = pcs.getError();
                    }
                }
                catch (Exception ex)
                {
                    errmsg = ex.Message;
                }
            }));
            if (errmsg != null)
            {
                MessageBox.Show("Can't logout: " + errmsg);
            }
        }

        private void ExecTask(ThreadStart task, ThreadStart onTaskFinished = null)
        {
            ExecTask("Processing", "Processing...\r\nPlease wait...", task, onTaskFinished);
        }

        private void ExecTask(string title, string description, ThreadStart task, ThreadStart onTaskFinished = null)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new AnonymousFunction(delegate() {
                    frmWaiting frm = new frmWaiting(title, description);
                    frm.Exec(task, onTaskFinished);
                }));
            }
            else
            {
                frmWaiting frm = new frmWaiting(title, description);
                frm.Exec(task, onTaskFinished);
            }
        }

        private void Go(string newPath)
        {
            if (newPath == "")
                return;
            if (!newPath.StartsWith("/"))
            {
                MessageBox.Show("Must be the full path. e.g. \"/movies\"");
                return;
            }
            if(ListFiles(newPath))
            {
                if (!string.IsNullOrEmpty(currentPath) && currentPath.StartsWith("/"))
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
                    if (!string.IsNullOrEmpty(currentPath) && currentPath.StartsWith("/"))
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
                    if (!string.IsNullOrEmpty(currentPath) && currentPath.StartsWith("/"))
                        history.Push(currentPath);
                    currentPath = path;
                    RefreshControls();
                }
            }
        }

        private void Up()
        {
            if (!currentPath.StartsWith("/"))
                return;
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
            PcsFileInfo[] list = null;
            string errmsg = null;
            ExecTask("List", "List the directory...", new ThreadStart(delegate()
            {
                try
                {
                    list = pcs.list(path, 1, int.MaxValue);
                    if (list == null)
                        errmsg = pcs.getError();
                }
                catch (Exception ex)
                {
                    errmsg = ex.Message;
                }
            }));
            if (errmsg != null)
            {
                MessageBox.Show("Can't list the file for \"" + path + "\": " + errmsg);
                return false;
            }
            cmbLocation.Text = path;
            BindFileList(list);
            return true;
        }

        private bool SearchFiles(string path, string keyword)
        {
            PcsFileInfo[] list = null;
            string errmsg = null;
            ExecTask(new ThreadStart(delegate()
            {
                try
                {
                    list = pcs.search(path, keyword, true);
                    if (list == null)
                        errmsg = pcs.getError();
                    else
                    {
                        //排序
                        List<PcsFileInfo> l = new List<PcsFileInfo>(list);
                        l.Sort(new PcsFileInfoComparer());
                        list = l.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    errmsg = ex.Message;
                }
            }));
            if (errmsg != null)
            {
                MessageBox.Show("Can't search \"" + keyword + "\" in \"" + path + "\": " + errmsg);
                return false;
            }
            cmbLocation.Text = "Search \"" + keyword + "\" in \"" + path + "\"";
            BindFileList(list);
            return true;
        }

        private void BindFileList(PcsFileInfo[] list)
        {
            lvFileList.Items.Clear();
            bigImageList.Images.Clear();
            smallImageList.Images.Clear();
            Icon icon = SystemIcon.GetFolderIcon(true);
            if (icon != null)
                bigImageList.Images.Add(icon);
            else
                bigImageList.Images.Add(Properties.Resources.generic_folder);
            icon = SystemIcon.GetFolderIcon(false);
            if (icon != null)
                smallImageList.Images.Add(icon);
            else
                smallImageList.Images.Add(Properties.Resources.generic_folder);
            if (list == null)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (PcsFileInfo file in list)
            {
                ListViewItem item = new ListViewItem(file.server_filename);
                items.Add(item);
                item.ToolTipText = file.path;
                item.Tag = file;
                if (file.isdir)
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
                    icon = SystemIcon.GetIcon(file, true);
                    if (icon != null)
                        bigImageList.Images.Add(file.server_filename, icon);
                    else
                        bigImageList.Images.Add(file.server_filename, Properties.Resources.generic_file);
                    icon = SystemIcon.GetIcon(file, false);
                    if (icon != null)
                        smallImageList.Images.Add(file.server_filename, icon);
                    else
                        smallImageList.Images.Add(file.server_filename, Properties.Resources.generic_file);
                    item.ImageKey = file.server_filename;
                }
            }
            lvFileList.Items.AddRange(items.ToArray());
            lblStatus.Text = lvFileList.Items.Count.ToString() + " items"; //更新状态栏
        }

        private void RefreshControls()
        {
            btnBack.Enabled = history.Count > 0;
            btnNext.Enabled = next.Count > 0;
            if (currentPath.StartsWith("/"))
            {
                string uppath = Path.GetDirectoryName(currentPath);
                btnUp.Enabled = !string.IsNullOrEmpty(uppath) && uppath != currentPath;
            }
            else
                btnUp.Enabled = false;
        }

        private void ShowSettingsWindow()
        {
            frmSettings frm = new frmSettings();
            if(frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }
        
        private void ShowHistoryWindow()
        {
            if (frmHistory == null)
            {
                frmHistory = new frmHistory(worker);
                frmHistory.FormClosed += frmHistory_FormClosed;
                frmHistory.Show();
            }
            else
            {
                frmHistory.Activate();
            }
        }

        private void frmHistory_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmHistory = null;
        }

        private PcsFileInfo GetFileMetaInformation(string path)
        {
            PcsFileInfo fileinfo = new PcsFileInfo();
            string errmsg = null;
            ExecTask(new ThreadStart(delegate()
            {
                try
                {
                    fileinfo = pcs.meta(path);
                    if (fileinfo.IsEmpty)
                    {
                        errmsg = pcs.getError();
                    }
                }
                catch (Exception ex)
                {
                    errmsg = ex.Message;
                }
            }));
            if (errmsg != null)
            {
                MessageBox.Show("Can't get meta information for \"" + path + "\": " + errmsg);
            }
            return fileinfo;
        }

        private void ShowFileMetaInformation(PcsFileInfo fileinfo)
        {
            frmMetaInformation frm = new frmMetaInformation(fileinfo);
            frm.ShowDialog();
        }

        private bool CreateNewFolder(string parentPath)
        {
            frmFileName frmFN = new frmFileName(string.Empty);
            if(frmFN.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = frmFN.FileName;
                string errmsg = null;
                ExecTask(new ThreadStart(delegate()
                {
                    try
                    {
                        PcsRes rc = pcs.mkdir(BaiduPCS.build_path(parentPath, filename));
                        if (rc != PcsRes.PCS_OK)
                        {
                            errmsg = pcs.getError();
                        }
                    }
                    catch (Exception ex)
                    {
                        errmsg = ex.Message;
                    }
                }));
                if (errmsg != null)
                {
                    MessageBox.Show("Can't create the directory \"" + BaiduPCS.build_path(parentPath, filename) + "\": " + errmsg);
                    return false;
                }
                return true;
            }
            return false;
        }

        private string cat(string path)
        {
            string content = string.Empty;
            string errmsg = null;
            ExecTask("cat", "get file content...", new ThreadStart(delegate()
            {
                try
                {
                    content = pcs.cat(path);
                    if (content == null)
                    {
                        errmsg = "Can't get file content. " + pcs.getError();
                    }
                }
                catch (Exception ex)
                {
                    errmsg = ex.Message;
                }
            }));
            if (errmsg != null)
            {
                MessageBox.Show(errmsg.ToString());
                return content;
            }
            return content;
        }

        private void Copy()
        {
            if (lvFileList.SelectedItems.Count > 0)
            {
                sources.Clear();
                foreach (ListViewItem item in lvFileList.SelectedItems)
                {
                    sources.Add((PcsFileInfo)item.Tag);
                }
                isMove = false;
                lblStatus2.Text = "Copied";
            }
        }

        private void Cut()
        {
            if (lvFileList.SelectedItems.Count > 0)
            {
                sources.Clear();
                foreach (ListViewItem item in lvFileList.SelectedItems)
                {
                    sources.Add((PcsFileInfo)item.Tag);
                }
                isMove = true;
                lblStatus2.Text = "Cut";
            }
        }

        private void Delete()
        {
            if(MessageBox.Show("Are you sure want to delete selected items?", "Delete", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                if (lvFileList.SelectedItems.Count > 0)
                {
                    List<PcsFileInfo> sources = new List<PcsFileInfo>(lvFileList.SelectedItems.Count);
                    foreach (ListViewItem item in lvFileList.SelectedItems)
                    {
                        sources.Add((PcsFileInfo)item.Tag);
                    }
                    if (DeleteFile(sources))
                    {
                        RefreshFileList();
                    }
                }
            }
        }

        private void ParseTo(PcsFileInfo to)
        {
            bool succ;
            if (isMove)
                succ = MoveFile(sources, to);
            else
                succ = CopyFile(sources, to);
            if (succ)
            {
                if (string.Equals(to.path, currentPath, StringComparison.InvariantCultureIgnoreCase))
                    RefreshFileList();
                else if (isMove)
                {
                    bool refresh = false;
                    foreach (PcsFileInfo fi in sources)
                    {
                        if (fi.path.StartsWith(currentPath + "/", StringComparison.InvariantCultureIgnoreCase))
                        {
                            refresh = true;
                            break;
                        }
                    }
                    if (refresh)
                        RefreshFileList();
                }
                sources.Clear();
                lblStatus2.Text = string.Empty;
            }
        }

        private bool MoveFile(List<PcsFileInfo> sources, PcsFileInfo to)
        {
            PcsPanApiRes ar = new PcsPanApiRes();
            StringBuilder errmsg = new StringBuilder();
            ExecTask(new ThreadStart(delegate()
            {
                try
                {
                    List<SPair> spairList = new List<SPair>(sources.Count);
                    foreach (PcsFileInfo fi in sources)
                    {
                        spairList.Add(new SPair(
                        fi.path,
                        BaiduPCS.build_path(to.path, fi.server_filename)));
                    }
                    ar = pcs.move(spairList.ToArray());
                    if (ar.error != 0)
                    {
                        foreach (PcsPanApiResInfo ri in ar.info_list)
                        {
                            if (ri.error != 0)
                            {
                                errmsg.AppendLine("Can't move \"" + ri.path + "\": error=" + ri.error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errmsg.Append(ex.Message);
                }
            }));
            if (errmsg.Length > 0)
            {
                MessageBox.Show(errmsg.ToString());
                return false;
            }
            return true;
        }

        private bool CopyFile(List<PcsFileInfo> sources, PcsFileInfo to)
        {
            PcsPanApiRes ar = new PcsPanApiRes();
            StringBuilder errmsg = new StringBuilder();
            ExecTask(new ThreadStart(delegate()
            {
                try
                {
                    List<SPair> spairList = new List<SPair>(sources.Count);
                    foreach (PcsFileInfo fi in sources)
                    {
                        spairList.Add(new SPair(
                        fi.path,
                        BaiduPCS.build_path(to.path, fi.server_filename)));
                    }
                    ar = pcs.copy(spairList.ToArray());
                    if (ar.error != 0)
                    {
                        foreach (PcsPanApiResInfo ri in ar.info_list)
                        {
                            if (ri.error != 0)
                            {
                                errmsg.AppendLine("Can't copy \"" + ri.path + "\": error=" + ri.error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errmsg.Append(ex.Message);
                }
            }));
            if (errmsg.Length > 0)
            {
                MessageBox.Show(errmsg.ToString());
                return false;
            }
            return true;
        }

        private bool DeleteFile(List<PcsFileInfo> sources)
        {
            if (MessageBox.Show("Are you sure you want to delete selected directories or files?", "Delete", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder errmsg = new StringBuilder();
                ExecTask(new ThreadStart(delegate()
                {
                    try
                    {
                        List<string> pathList = new List<string>(sources.Count);
                        foreach (PcsFileInfo fi in sources)
                        {
                            pathList.Add(fi.path);
                        }
                        PcsPanApiRes rc = pcs.delete(pathList.ToArray());
                        if (rc.error != 0)
                        {
                            foreach (PcsPanApiResInfo ri in rc.info_list)
                            {
                                if (ri.error != 0)
                                {
                                    errmsg.AppendLine("Can't delete \"" + ri.path + "\": error=" + ri.error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errmsg.Append(ex.Message);
                    }
                }));
                if (errmsg.Length > 0)
                {
                    MessageBox.Show(errmsg.ToString());
                    return false;
                }
                return true;
            }
            return false;
        }

        private bool RenameFile(PcsFileInfo fileinfo)
        {
            frmFileName frmFN = new frmFileName(fileinfo.server_filename);
            if(frmFN.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = frmFN.FileName;
                SPair spair = new SPair(
                            fileinfo.path,
                            filename);
                PcsPanApiRes ar = new PcsPanApiRes();
                string errmsg = null;
                ExecTask(new ThreadStart(delegate()
                {
                    try
                    {
                        ar = pcs.rename(new SPair[] { spair });
                        if (ar.error != 0)
                        {
                            errmsg = pcs.getError();
                        }
                    }
                    catch (Exception ex)
                    {
                        errmsg = ex.Message;
                    }
                }));
                if (errmsg != null)
                {
                    MessageBox.Show("Can't rename (\"" + spair.str1 + "\" => \"" + spair.str2 + "\"): " + errmsg);
                    return false;
                }
                return true;
            }
            return false;
        }

        private bool DownloadFile(List<PcsFileInfo> sources)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int totalCount = 0;
                List<OperationInfo> list = new List<OperationInfo>();
                string uid = pcs.getUID();
                ShowHistoryWindow();
                ExecTask("Download", "Add files to download queue...", new ThreadStart(delegate()
                {
                    foreach (PcsFileInfo fi in sources)
                    {
                        AddFilesToDownloadQueue(fi, folderBrowserDialog1.SelectedPath, uid, list, OperationStatus.Pending, ref totalCount);
                    }
                }));
                if (list.Count > 0)
                    worker.queue.Enqueue(list.ToArray());
                if (totalCount > 0)
                    return true;
                string errmsg = string.Empty;
                errmsg = "Failed to add the files to the queue.";
                MessageBox.Show(errmsg, "Download");
                return false;
            }
            return false;
        }

        private void AddFilesToDownloadQueue(PcsFileInfo from, string toFolder, string uid,
            List<OperationInfo> list,
            OperationStatus status,
            ref int totalCount)
        {
            if (from.isdir)
            {
                int pageIndex = 1;
                int pageSize = 20;
                PcsFileInfo[] fis;
                do
                {
                    fis = pcs.list(from.path, pageIndex, pageSize);
                    if (fis != null)
                    {
                        foreach (PcsFileInfo fi in fis)
                        {
                            AddFilesToDownloadQueue(fi, Path.Combine(toFolder, from.server_filename), uid,
                                list, OperationStatus.Pending, ref totalCount);
                        }
                    }
                    pageIndex++;
                }
                while (fis != null && fis.Length >= pageSize);
            }
            else
            {
                OperationInfo op = new OperationInfo()
                {
                    uid = uid,
                    operation = Operation.Download,
                    from = from.path,
                    to = Path.Combine(toFolder, from.server_filename),
                    status = OperationStatus.Pending,
                    totalSize = from.size
                };
                if (worker.queue.Contains(op))
                    return;
                list.Add(op);
                totalCount++;
                if (list.Count >= 20)
                {
                    worker.queue.Enqueue(list.ToArray());
                    list.Clear();
                }
            }
        }

        private bool UploadFile(PcsFileInfo to)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string uid = pcs.getUID();
                int totalCount = 0;
                List<OperationInfo> list = new List<OperationInfo>();
                ShowHistoryWindow();
                string rootPath = to.path;
                if (rootPath.EndsWith("/"))
                    rootPath = rootPath.Substring(1);
                foreach (string filename in openFileDialog1.FileNames)
                {
                    FileInfo fi = new FileInfo(filename);
                    OperationInfo op = new OperationInfo()
                    {
                        uid = uid,
                        operation = Operation.Upload,
                        from = filename,
                        to = BaiduPCS.build_path(rootPath, Path.GetFileName(filename)),
                        status = OperationStatus.Pending,
                        totalSize = fi.Length
                    };
                    if (worker.queue.Contains(op))
                        continue;
                    list.Add(op);
                    totalCount++;
                    if(list.Count >= 100)
                    {
                        worker.queue.Enqueue(list.ToArray());
                        list.Clear();
                    }
                }
                string errmsg = string.Empty;
                if (list.Count > 0)
                    worker.queue.Enqueue(list.ToArray());
                if (totalCount > 0)
                {
                    if (totalCount != openFileDialog1.FileNames.Length)
                    {
                        errmsg = "Add " + totalCount + " items to the queue, duplicated " + (openFileDialog1.FileNames.Length - totalCount) + " items.";
                        MessageBox.Show(errmsg, "Upload");
                    }
                    return true;
                }
                else
                {
                    errmsg = "Failed to add the files to the queue.";
                    MessageBox.Show(errmsg, "Upload");
                    return false;
                }
            }
            return false;
        }

        private bool UploadDirectory(PcsFileInfo to)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int totalCount = 0;
                List<OperationInfo> list = new List<OperationInfo>();
                string uid = pcs.getUID();
                ShowHistoryWindow();
                ExecTask("Upload", "Add files to upload queue...", new ThreadStart(delegate()
                {
                    AddFilesToUploadQueue(folderBrowserDialog1.SelectedPath,
                        BaiduPCS.build_path(to.path, Path.GetFileName(folderBrowserDialog1.SelectedPath)),
                        uid, list, OperationStatus.Pending, ref totalCount);
                }));
                if (list.Count > 0)
                    worker.queue.Enqueue(list.ToArray());
                if (totalCount > 0)
                    return true;
                string errmsg = string.Empty;
                errmsg = "Failed to add the files to the queue.";
                MessageBox.Show(errmsg, "Upload");
                return false;
            }
            return false;
        }

        private void AddFilesToUploadQueue(string from, string toFolder, string uid,
            List<OperationInfo> list, OperationStatus status,
            ref int totalCount)
        {
            FileInfo fi = new FileInfo(from);

            if ((fi.Attributes & FileAttributes.Directory) != 0)
            {
                string[] files = Directory.GetFiles(from);
                foreach(string f in files)
                {
                    AddFilesToUploadQueue(f, BaiduPCS.build_path(toFolder, Path.GetFileName(f)), uid, list, status, ref totalCount);
                }
            }
            else
            {
                if (toFolder.EndsWith("/"))
                    toFolder = toFolder.Substring(1);
                OperationInfo op = new OperationInfo()
                {
                    uid = uid,
                    operation = Operation.Upload,
                    from = fi.FullName,
                    to = BaiduPCS.build_path(toFolder, Path.GetFileName(fi.FullName)),
                    status = OperationStatus.Pending,
                    totalSize = fi.Length
                };
                if (worker.queue.Contains(op))
                    return;
                list.Add(op);
                totalCount++;
                if (list.Count >= 100)
                {
                    worker.queue.Enqueue(list.ToArray());
                    list.Clear();
                }
            }
        }

        #endregion

        #region 上下文菜单代码

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (lvFileList.SelectedItems.Count == 0)   //当前位置没有ListViewItem
            {
                bool isdir = currentPath.StartsWith("/");

                openToolStripMenuItem1.Visible = false;
                downloadToolStripMenuItem.Visible = false;
                uploadFileToolStripMenuItem.Visible = isdir;
                uploadDirectoryToolStripMenuItem.Visible = isdir;
                viewToolStripMenuItem1.Visible = true;
                refreshToolStripMenuItem1.Visible = isdir;

                toolStripMenuItem3.Visible = isdir;

                renameToolStripMenuItem.Visible = false;
                cutToolStripMenuItem1.Visible = false;
                copyToolStripMenuItem1.Visible = false;
                deleteToolStripMenuItem1.Visible = false;
                parseToolStripMenuItem1.Visible = isdir && sources.Count > 0;
                mkdirToolStripMenuItem1.Visible = isdir;

                toolStripMenuItem4.Visible = isdir;

                attributesToolStripMenuItem1.Visible = isdir;

            }
            else    //有
            {
                bool isdir = false;
                bool isfile = false;
                if (lvFileList.SelectedItems.Count == 1)
                {
                    PcsFileInfo fileinfo = (PcsFileInfo)lvFileList.SelectedItems[0].Tag;
                    if (fileinfo.isdir)
                        isdir = true;
                    else
                        isfile = true;
                }
                openToolStripMenuItem1.Visible = isdir;
                downloadToolStripMenuItem.Visible = true;
                uploadFileToolStripMenuItem.Visible = isdir;
                uploadDirectoryToolStripMenuItem.Visible = isdir;
                viewToolStripMenuItem1.Visible = false;
                refreshToolStripMenuItem1.Visible = false;

                toolStripMenuItem3.Visible = true;

                renameToolStripMenuItem.Visible = isdir || isfile;
                cutToolStripMenuItem1.Visible = true;
                copyToolStripMenuItem1.Visible = true;
                deleteToolStripMenuItem1.Visible = true;
                parseToolStripMenuItem1.Visible = isdir && sources.Count > 0;
                mkdirToolStripMenuItem1.Visible = false;

                toolStripMenuItem4.Visible = isdir || isfile;

                attributesToolStripMenuItem1.Visible = isdir || isfile;

            }
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (lvFileList.SelectedItems.Count > 0)
            {
                PcsFileInfo fileinfo = (PcsFileInfo)lvFileList.SelectedItems[0].Tag;
                if (fileinfo.isdir)
                {
                    Go(fileinfo.path);
                }
            }
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFileList.SelectedItems.Count > 0)
            {
                List<PcsFileInfo> sources = new List<PcsFileInfo>(lvFileList.SelectedItems.Count);
                foreach (ListViewItem item in lvFileList.SelectedItems)
                {
                    sources.Add((PcsFileInfo)item.Tag);
                }
                DownloadFile(sources);
            }
        }

        private void uploadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFileList.SelectedItems.Count > 0)
            {
                PcsFileInfo fileinfo = (PcsFileInfo)lvFileList.SelectedItems[0].Tag;
                if (fileinfo.isdir)
                    UploadFile(fileinfo);
            }
            else if (currentPath.StartsWith("/"))
            {
                PcsFileInfo fileinfo = new PcsFileInfo();
                fileinfo.path = currentPath;
                fileinfo.isdir = true;
                UploadFile(fileinfo);
            }
        }

        private void uploadDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFileList.SelectedItems.Count > 0)
            {
                PcsFileInfo fileinfo = (PcsFileInfo)lvFileList.SelectedItems[0].Tag;
                if (fileinfo.isdir)
                    UploadDirectory(fileinfo);
            }
            else if (currentPath.StartsWith("/"))
            {
                PcsFileInfo fileinfo = new PcsFileInfo();
                fileinfo.path = currentPath;
                fileinfo.isdir = true;
                UploadDirectory(fileinfo);
            }
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
            Cut();
        }

        private void copyToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void parseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (sources.Count > 0)
            {
                PcsFileInfo to;
                if (lvFileList.SelectedItems.Count > 0)
                {
                    to = (PcsFileInfo)lvFileList.SelectedItems[0].Tag;
                }
                else if (currentPath.StartsWith("/"))
                {
                    to = new PcsFileInfo();
                    to.path = currentPath;
                    to.isdir = true;
                }
                else
                    return;
                if (to.isdir)
                {
                    ParseTo(to);
                }
            }
        }

        private void mkdirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFileList.SelectedItems.Count > 0 || !currentPath.StartsWith("/"))
                return;
            if (CreateNewFolder(currentPath))
            {
                RefreshFileList();
            }
        }

        private void metaInformationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PcsFileInfo fileinfo;
            if (lvFileList.SelectedItems.Count == 0 && currentPath.StartsWith("/"))
                fileinfo = GetFileMetaInformation(currentPath);
            else if (lvFileList.SelectedItems.Count == 1)
                fileinfo = (PcsFileInfo)lvFileList.SelectedItems[0].Tag;
            else
                return;
            ShowFileMetaInformation(fileinfo);
        }

        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (lvFileList.SelectedItems.Count == 0 && currentPath.StartsWith("/"))
                RefreshFileList();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFileList.SelectedItems.Count != 1)
                return;
            PcsFileInfo fileinfo = (PcsFileInfo)lvFileList.SelectedItems[0].Tag;
            if (RenameFile(fileinfo))
            {
                RefreshFileList();
            }
        }

        #endregion

        class PcsFileInfoComparer : IComparer<PcsFileInfo>
        {
            public int Compare(PcsFileInfo x, PcsFileInfo y)
            {
                int a = 10, b = 10;
                if(!x.IsEmpty)
                {
                    if (x.isdir)
                        a--;
                }
                if(!y.IsEmpty)
                {
                    if (y.isdir)
                        b--;
                }
                if (a != b)
                    return a - b;
                return string.Compare(x.server_filename, y.server_filename);
            }
        }
    }
}
