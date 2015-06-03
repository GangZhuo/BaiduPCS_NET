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

namespace Sample
{
    public partial class frmMain : Form
    {
        /// <summary>
        /// 最小分片大小
        /// </summary>
        public const int MIN_UPLOAD_SLICE_SIZE	=	(512 * 1024);
        /// <summary>
        /// 最大分片大小
        /// </summary>
        public const int MAX_UPLOAD_SLICE_SIZE	=	(10 * 1024 * 1024);
        /// <summary>
        /// 最大分片数量
        /// </summary>
        public const int MAX_UPLOAD_SLICE_COUNT = 1024;

        BaiduPCS pcs;

        TreeNode selectedNode;
        PcsFileInfo selected;

        frmProgress frmPrg = null;

        public frmMain()
        {
            InitializeComponent();
            treeView1.AfterExpand += treeView1_AfterExpand;
            treeView1.AfterSelect += treeView1_AfterSelect;
            selected = new PcsFileInfo();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            string cookiefilename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".pcs", "cookies.txt");
            string dir = System.IO.Path.GetDirectoryName(cookiefilename);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            pcs = BaiduPCS.pcs_create(cookiefilename);
            if (pcs == null)
            {
                MessageBox.Show("Can't create BaiduPCS");
                Application.Exit();
            }
            if (pcs.isLogin() != PcsRes.PCS_LOGIN)
            {
                frmLogin frm = new frmLogin(pcs);
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    long quota, used;
                    pcs.quota(out quota, out used);
                    Text = pcs.getUID() + "'s Disk " + HumanReadableSize(used) + "/" + HumanReadableSize(quota);
                    BindDirectoryTree();
                }
                else
                {
                    Application.Exit();
                }
            }
            else
            {
                long quota, used;
                pcs.quota(out quota, out used);
                Text = pcs.getUID() + "'s Disk " + HumanReadableSize(used) + "/" + HumanReadableSize(quota);
                BindDirectoryTree();
            }
            pcs.setOption(PcsOption.PCS_OPTION_DOWNLOAD_WRITE_FUNCTION, new OnHttpWriteFunction(onWrite));
            pcs.setOption(PcsOption.PCS_OPTION_PROGRESS_FUNCTION, new OnHttpProgressFunction(onUpload));
            pcs.setOption(PcsOption.PCS_OPTION_PROGRESS, false);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                pcs.Dispose();
                pcs = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.OnClosing(e);
        }

        private void BindDirectoryTree()
        {
            treeView1.Nodes.Clear();
            TreeNode root = new TreeNode(pcs.getUID());
            treeView1.Nodes.Add(root);
            BindDirectoryTree(root, "/", 2);
            root.Expand();
        }

        private void BindDirectoryTree(TreeNode root, string path, int max_level)
        {
            if(this.treeView1.InvokeRequired)
            {
                this.treeView1.Invoke(new VoidFun(delegate() {
                    BindDirectoryTree(root, path, max_level);
                }));
                return;
            }
            root.Nodes.Clear();
            int level = root.Level;
            PcsFileInfo[] list = pcs.list(path, 1, int.MaxValue);
            if (list != null)
            {
                TreeNode n;
                foreach (PcsFileInfo f in list)
                {
                    n = new TreeNode((f.isdir ? "[D] " : "[F] ") + f.server_filename);
                    n.Tag = f;
                    root.Nodes.Add(n);
                    if ((level + 1) < max_level)
                    {
                        BindDirectoryTree(n, f.path, max_level);
                    }
                }
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode n in e.Node.Nodes)
            {
                if (n.Nodes.Count > 0)
                    continue;
                PcsFileInfo f = (PcsFileInfo)n.Tag;
                BindDirectoryTree(n, f.path, n.Level + 1);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedNode = e.Node;
            if (e.Node.Tag == null)
            {
                selected = new PcsFileInfo();
                btnMeta.Enabled = false;
                btnUpload.Enabled = false;
                btnUploadSlice.Enabled = false;
                btnDownload.Enabled = false;
                btnResumeDownload.Enabled = false;
                btnCreateSubDir.Enabled = false;
                btnMove.Enabled = false;
                btnRename.Enabled = false;
                btnDelete.Enabled = false;
                return;
            }
            PcsFileInfo f = (PcsFileInfo)e.Node.Tag;
            selected = f;
            lblPath.Text = f.path;
            if(f.isdir)
            {
                btnMeta.Enabled = true;
                btnUpload.Enabled = true;
                btnUploadSlice.Enabled = true;
                btnDownload.Enabled = false;
                btnResumeDownload.Enabled = false;
                btnCreateSubDir.Enabled = true;
                btnMove.Enabled = true;
                btnRename.Enabled = true;
                btnDelete.Enabled = true;
            }
            else
            {
                btnMeta.Enabled = true;
                btnUpload.Enabled = false;
                btnUploadSlice.Enabled = false;
                btnDownload.Enabled = true;
                btnResumeDownload.Enabled = true;
                btnCreateSubDir.Enabled = false;
                btnMove.Enabled = true;
                btnRename.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void btnMeta_Click(object sender, EventArgs e)
        {
            if (selected.IsEmpty)
                return;
            PcsFileInfo rc = pcs.meta(selected.path);
            if (!rc.IsEmpty)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("fs_id={0}", rc.fs_id));
                sb.AppendLine(string.Format("path={0}", rc.path));
                sb.AppendLine(string.Format("server_filename={0}", rc.server_filename));
                sb.AppendLine(string.Format("server_ctime={0}", NativeUtils.time_str(rc.server_ctime)));
                sb.AppendLine(string.Format("server_mtime={0}", NativeUtils.time_str(rc.server_mtime)));
                sb.AppendLine(string.Format("local_ctime={0}", NativeUtils.time_str(rc.local_ctime)));
                sb.AppendLine(string.Format("local_mtime={0}", NativeUtils.time_str(rc.local_mtime)));
                sb.AppendLine(string.Format("size={0}", HumanReadableSize(rc.size)));
                sb.AppendLine(string.Format("category={0}", rc.category));
                sb.AppendLine(string.Format("isdir={0}", rc.isdir));
                sb.AppendLine(string.Format("dir_empty={0}", rc.dir_empty));
                sb.AppendLine(string.Format("empty={0}", rc.empty));
                sb.AppendLine(string.Format("md5={0}", rc.md5));
                sb.AppendLine(string.Format("dlink={0}", rc.dlink));
                sb.AppendLine(string.Format("block_list={0}", rc.block_list_str));
                sb.AppendLine(string.Format("ifhassubdir={0}", rc.ifhassubdir));
                sb.AppendLine(string.Format("user_flag={0}", rc.user_flag));
                MessageBox.Show(sb.ToString());
            }
            else
            {
                MessageBox.Show("Failed to get meta for " + selected.path + " : " + pcs.getError());
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (selected.IsEmpty || !selected.isdir)
                return;
            if (this.openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string localPath = this.openFileDialog1.FileName;
                string remotePath = System.IO.Path.Combine(selected.path, System.IO.Path.GetFileName(localPath)).Replace("\\", "/");
                string filemd5, slicemd5;
                frmPrg = new frmProgress();
                frmPrg.Label1 = "Upload " + localPath;
                frmPrg.Value = 0;
                frmPrg.Label2 = "0%";
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
                {
                    PcsFileInfo fi = pcs.rapid_upload(remotePath, localPath, out filemd5, out slicemd5, false);
                    if (fi.IsEmpty)
                    {
                        pcs.setOption(PcsOption.PCS_OPTION_PROGRESS, true);
                        fi = pcs.upload(remotePath, localPath, false);
                        pcs.setOption(PcsOption.PCS_OPTION_PROGRESS, false);
                        if (fi.IsEmpty)
                        {
                            MessageBox.Show("Failed to upload " + localPath + " : " + pcs.getError());
                        }
                        else
                        {
                            MessageBox.Show("Upload Success");
                            BindDirectoryTree(selectedNode, selected.path, selectedNode.Level + 1);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Rapid Upload Success");
                        BindDirectoryTree(selectedNode, selected.path, selectedNode.Level + 1);
                    }
                    frmPrg.CloseEx();
                    frmPrg = null;
                })).Start();
                frmPrg.ShowDialog();
            }
        }

        class UploadFileInfo
        {

            /// <summary>
            /// 整个文件已经上传的长度
            /// </summary>
            public long total_uploaded_size { get; set; }

            /// <summary>
            /// 文件的长度
            /// </summary>
            public long filesize { get; set; }

            /// <summary>
            /// 待上传的文件路径
            /// </summary>
            public string filename { get; set; }

            /// <summary>
            /// 是否由用户取消
            /// </summary>
            public bool cancelled_by_user { get; set; }

        }

        class Slice
        {
            /// <summary>
            /// 分片在文件中位置
            /// </summary>
            public long offset { get; set; }

            /// <summary>
            /// 分片长度
            /// </summary>
            public long size { get; set; }

            /// <summary>
            /// 已经上传的长度
            /// </summary>
            public long uploaded_size { get; set; }

            /// <summary>
            /// 上传成功后的文件
            /// </summary>
            public PcsFileInfo file { get; set; }

            /// <summary>
            /// 待上传的文件信息
            /// </summary>
            public UploadFileInfo local_file { get; set; }
        }

        private void btnUploadSlice_Click(object sender, EventArgs e)
        {
            if (selected.IsEmpty || !selected.isdir)
                return;
            if (this.openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string localPath = this.openFileDialog1.FileName;
                string remotePath = System.IO.Path.Combine(selected.path, System.IO.Path.GetFileName(localPath)).Replace("\\", "/");
                string filemd5, slicemd5;
                frmPrg = new frmProgress();
                frmPrg.Label1 = "Upload " + localPath;
                frmPrg.Value = 0;
                frmPrg.Label2 = "0%";
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
                {
                    PcsFileInfo fi = pcs.rapid_upload(remotePath, localPath, out filemd5, out slicemd5, false);
                    if (fi.IsEmpty)
                    {
                        long filesize = new FileInfo(localPath).Length;
                        UploadFileInfo ufi = new UploadFileInfo()
                        {
                            total_uploaded_size = 0,
                            filesize = filesize,
                            cancelled_by_user = false,
                            filename = localPath
                        };

                        if(filesize <= MIN_UPLOAD_SLICE_SIZE) 
                        {
                            MessageBox.Show("文件过小，不允许分片上传，大于" + HumanReadableSize((long)MIN_UPLOAD_SLICE_SIZE) + "的文件才需要分片上传");
                            return;
                        }

                        List<Slice> slicelist = new List<Slice>();

                        #region 开始分片

                        //假设启动的下载线程为 10 个，那么需要把文件分为 10 片。
                        long slice_count = 10;

                        //基于 “需要把文件分为10片” 来计算每个分片的大小
			            long slice_size = filesize / slice_count;
			            if ((filesize % slice_count) != 0)
				            slice_size++;

                        // 验证分片大小是否在允许的分片大小范围中，如果不在范围中，则重设分片大小。
			            if (slice_size <= MIN_UPLOAD_SLICE_SIZE)
				            slice_size = MIN_UPLOAD_SLICE_SIZE;
			            if (slice_size > MAX_UPLOAD_SLICE_SIZE)
				            slice_size = MAX_UPLOAD_SLICE_SIZE;

                        //基于新的分片大小计算分片数量
			            slice_count = (int)(filesize / slice_size);
			            if ((filesize % slice_size) != 0)
                            slice_count++;

                        //分片数量超过最大允许分片数量，因此使用允许的最大分片数量来重新计算每分片的大小
			            if (slice_count > MAX_UPLOAD_SLICE_COUNT) {
				            slice_count = MAX_UPLOAD_SLICE_COUNT;
				            slice_size = filesize / slice_count;
				            if ((filesize % slice_count) != 0)
					            slice_size++;
				            slice_count = (int)(filesize / slice_size);
				            if ((filesize % slice_size) != 0) slice_count++;
			            }
                        long offset = 0;
			            for (int i = 0; i < slice_count; i++) {
                            Slice ts = new Slice()
                            {
                                offset = offset,
                                size = slice_size,
                                uploaded_size = 0,
                                local_file = ufi
                            };
                            if (ts.offset + ts.size > filesize) ts.size = filesize - ts.offset;
                            offset += slice_size;
                            slicelist.Add(ts);
			            }

                        //TODO: 保存分片数据，因此上传中断后，可以还原分片记录，这样的话，已经上传好了的分片不需要再次上传。

                        #endregion

                        #region 循环下载每一个分片

                        foreach(Slice slice in slicelist)
                        {
                            while(true)
                            {
                                slice.file = pcs.upload_slicefile(new OnReadSliceFunction(OnReadSlice), slice, (uint)slice.size);

                                if (string.IsNullOrEmpty(slice.file.md5))
                                {
                                    if (ufi.cancelled_by_user) //因为用户取消上传导致的失败，则跳过。
                                        break;
                                    //上传失败，重新上传
                                    ufi.total_uploaded_size -= slice.uploaded_size;
                                    slice.uploaded_size = 0;
                                }
                                else
                                {
                                    //TODO: 保存分片数据，因此上传中断后，可以还原分片记录，这样的话，已经上传好了的分片不需要再次上传。
                                    break;
                                }
                            }
                            if (ufi.cancelled_by_user) //用户取消上传，则停止上传。
                                break;
                        }

                        #endregion

                        bool suc = !ufi.cancelled_by_user;
                        List<string> md5list = new List<string>();
                        if (suc)
                        {
                            //检查是否所有分片都上传成功
                            foreach (Slice slice in slicelist)
                            {
                                if (string.IsNullOrEmpty(slice.file.md5))
                                {
                                    suc = false;
                                    break;
                                }
                                md5list.Add(slice.file.md5);
                            }
                        }

                        if (suc)
                            fi = pcs.create_superfile(remotePath, md5list.ToArray(), false); //合并分片
                        else
                            fi = new PcsFileInfo();

                        if (fi.IsEmpty)
                        {
                            MessageBox.Show("Failed to upload " + localPath + " : " + pcs.getError());
                        }
                        else
                        {
                            MessageBox.Show("Upload Success");
                            BindDirectoryTree(selectedNode, selected.path, selectedNode.Level + 1);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Rapid Upload Success");
                        BindDirectoryTree(selectedNode, selected.path, selectedNode.Level + 1);
                    }
                    frmPrg.CloseEx();
                    frmPrg = null;
                })).Start();
                frmPrg.ShowDialog();
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (selected.IsEmpty || selected.isdir)
                return;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string localfile = saveFileDialog1.FileName;
                frmPrg = new frmProgress();
                frmPrg.Label1 = "Download " + selected.path;
                frmPrg.Value = 0;
                frmPrg.Label2 = "0%";
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
                {
                    FileStream fs = new FileStream(localfile, FileMode.Create);
                    pcs.setOption(PcsOption.PCS_OPTION_DOWNLOAD_WRITE_FUNCTION_DATA, fs);
                    PcsRes rc = pcs.download(selected.path, 0, 0);
                    fs.Close();
                    if (rc == PcsRes.PCS_OK)
                    {
                        MessageBox.Show("Download " + selected.path + " Success! Save to " + localfile);
                    }
                    else
                    {
                        MessageBox.Show("Failed to upload " + selected.path + " : " + pcs.getError());
                    }
                    frmPrg.CloseEx();
                    frmPrg = null;
                })).Start();
                frmPrg.ShowDialog();
            }
        }

        private void btnResumeDownload_Click(object sender, EventArgs e)
        {
            if (selected.IsEmpty || selected.isdir)
                return;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string localfile = saveFileDialog1.FileName;
                long filesize = pcs.filesize(selected.path);
                frmPrg = new frmProgress();
                frmPrg.Label1 = "Download " + selected.path;
                frmPrg.Value = 0;
                frmPrg.Label2 = "0%";
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
                {
                    FileStream fs = new FileStream(localfile, FileMode.Create);
                    pcs.setOption(PcsOption.PCS_OPTION_DOWNLOAD_WRITE_FUNCTION_DATA, fs);
                    PcsRes rc = pcs.download(selected.path, 0, filesize > 10 ? filesize - 10 : 0); // 仅下载最后10字节
                    fs.Close();
                    if (rc == PcsRes.PCS_OK)
                    {
                        MessageBox.Show("Download " + selected.path + " Success! Save to " + localfile);
                    }
                    else
                    {
                        MessageBox.Show("Failed to upload " + selected.path + " : " + pcs.getError());
                    }
                    frmPrg.CloseEx();
                    frmPrg = null;
                })).Start();
                frmPrg.ShowDialog();
            }
        }

        private void btnCreateSubDir_Click(object sender, EventArgs e)
        {
            if (selected.IsEmpty || !selected.isdir)
                return;
            string subdir = Microsoft.VisualBasic.Interaction.InputBox("Please input the directory path:", "mkdir", selected.path);
            if (!string.IsNullOrEmpty(subdir))
            {
                string path = "";
                if (subdir.StartsWith("/"))
                {
                    path = subdir;
                }
                else
                {
                    path = selected.path + "/" + subdir;
                }
                PcsRes rc = pcs.mkdir(path);
                if (rc == PcsRes.PCS_OK)
                {
                    MessageBox.Show("Create " + path + " Success! ");
                    BindDirectoryTree(selectedNode, selected.path, selectedNode.Level + 2);
                }
                else
                {
                    MessageBox.Show("Failed to create " + path + " : " + pcs.getError());
                }
            }
        }

        private void refreshParentNode(TreeNode n)
        {
            TreeNode p = n.Parent;
            if (p == null || p.Tag == null)
            {
                BindDirectoryTree();
            }
            else
            {
                PcsFileInfo f = (PcsFileInfo)p.Tag;
                BindDirectoryTree(p, f.path, p.Level + 1);
            }
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            if (selected.IsEmpty)
                return;
            string subdir = Microsoft.VisualBasic.Interaction.InputBox("Please input the target path:", "move", selected.path);
            if (!string.IsNullOrEmpty(subdir))
            {
                string path = "";
                if (subdir.StartsWith("/"))
                {
                    path = subdir;
                }
                else
                {
                    path = selected.path + "/" + subdir;
                }
                PcsPanApiRes rc = pcs.move(new SPair[] {
                    new SPair(selected.path, path)
                });
                if (rc.error == 0)
                {
                    MessageBox.Show("Move to " + path + " Success! ");
                    refreshParentNode(selectedNode);
                }
                else
                {
                    MessageBox.Show("Failed to move " + selected.path + " : " + pcs.getError());
                }
            }
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            if (selected.IsEmpty)
                return;
            string subdir = Microsoft.VisualBasic.Interaction.InputBox("Please input the new name:", "rename", selected.server_filename);
            if (!string.IsNullOrEmpty(subdir))
            {
                PcsPanApiRes rc = pcs.rename(new SPair[] {
                    new SPair(selected.path, subdir)
                });
                if (rc.error == 0)
                {
                    MessageBox.Show("Rename to " + subdir + " Success! ");
                    refreshParentNode(selectedNode);
                }
                else
                {
                    MessageBox.Show("Failed to rename " + selected.path + " : " + pcs.getError());
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selected.IsEmpty)
                return;
            if (MessageBox.Show("Are you sure delete " + selected.path + " ?", "delete", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                PcsPanApiRes rc = pcs.delete(new string[] { selected.path });
                if (rc.error == 0)
                {
                    MessageBox.Show("Delete " + selected.path + " Success! ");
                    refreshParentNode(selectedNode);
                }
                else
                {
                    MessageBox.Show("Failed to delete " + selected.path + " : " + pcs.getError());
                }
            }
        }

        int onUpload(BaiduPCS sender, double dltotal, double dlnow, double ultotal, double ulnow, object userdata)
        {
            try
            {
                if (frmPrg != null)
                {
                    if (frmPrg.Cancelled)
                        return -1;
                    if (ultotal < 1)
                        return 0;
                    int percentage = (int)((ulnow * 100) / ultotal);
                    frmPrg.Value = percentage;
                    frmPrg.Label2 = HumanReadableSize((long)ulnow) + "/" + HumanReadableSize((long)ultotal) + "  " + percentage + "%";
                }
                return 0;
            }
            catch { }
            return -1;
        }

        /// <summary>
        /// 当从网络获取到数据后触发该回调。
        /// </summary>
        /// <param name="data">从网络获取到的字节序</param>
        /// <param name="contentlength">HTTP头中的 Content-Length 的值</param>
        /// <param name="userdata"></param>
        /// <returns>返回写入的数据长度，字节为单位，必须和 data.Length 相同，否则将会中断下载。</returns>
        uint onWrite(BaiduPCS sender, byte[] data, uint contentlength, object userdata)
        {
            try
            {
                FileStream fs = (FileStream)userdata;
                fs.Write(data, 0, data.Length);
                if (frmPrg != null)
                {
                    if (frmPrg.Cancelled)
                        return 0;
                    if (contentlength > 0)
                    {
                        int percentage = (int)((fs.Position * 100.0f) / contentlength);
                        frmPrg.Value = percentage;
                        frmPrg.Label2 = HumanReadableSize(fs.Position) + "/" + HumanReadableSize((long)contentlength) + "  " + percentage + "%";
                    }
                }
                return (uint)data.Length;
            }
            catch { }
            return 0;
        }

        int OnReadSlice(BaiduPCS sender, out byte[] buf, uint size, uint nmemb, object userdata)
        {
            Slice slice = (Slice)userdata;
            try
            {
                FileStream fs = new FileStream(slice.local_file.filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
                int sz = (int)(size * nmemb);
                if (slice.uploaded_size + sz > slice.size)
                {
                    sz = (int)(slice.size - slice.uploaded_size);
                }
                buf = new byte[sz];
                //读取的位置为，本分片的开始位置 + 本分片已经上传的数量
                fs.Position = slice.offset + slice.uploaded_size;
                fs.Read(buf, 0, buf.Length);
                fs.Close();
                slice.uploaded_size += buf.Length;
                slice.local_file.total_uploaded_size += buf.Length;
                if (frmPrg != null)
                {
                    if (frmPrg.Cancelled)
                    {
                        slice.local_file.cancelled_by_user = true;
                        return NativeConst.CURL_READFUNC_ABORT;
                    }
                    if (slice.local_file.filesize > 0)
                    {
                        int percentage = (int)((slice.local_file.total_uploaded_size * 100.0f) / slice.local_file.filesize);
                        frmPrg.Value = percentage;
                        frmPrg.Label2 = HumanReadableSize((long)slice.local_file.total_uploaded_size) + "/" + HumanReadableSize((long)slice.local_file.filesize) + "  " + percentage + "%";
                    }
                }
                return buf.Length;
            }
            catch { }
            buf = null;
            slice.local_file.cancelled_by_user = true;
            return NativeConst.CURL_READFUNC_ABORT;
        }


        /// <summary>
        /// 格式化文件大小为人类可读的格式
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string HumanReadableSize(long size)
        {
            //B, KB, MB, or GB
            if (size >= GB)
            {
                double f = (double)size / (double)GB;
                string scalar = f.ToString("F2");
                if (scalar.EndsWith(".00"))
                    scalar = scalar.Substring(0, scalar.Length - 3);
                return scalar + "GB";
            }
            else if (size >= MB)
            {
                double f = (double)size / (double)MB;
                string scalar = f.ToString("F2");
                if (scalar.EndsWith(".00"))
                    scalar = scalar.Substring(0, scalar.Length - 3);
                return scalar + "MB";
            }
            else if (size >= KB)
            {
                double f = (double)size / (double)KB;
                string scalar = f.ToString("F2");
                if (scalar.EndsWith(".00"))
                    scalar = scalar.Substring(0, scalar.Length - 3);
                return scalar + "KB";
            }
            else
            {
                return size.ToString() + "B";
            }
        }

        public const int KB = 1024;
        public const int MB = 1024 * KB;
        public const int GB = 1024 * MB;

    }
}
