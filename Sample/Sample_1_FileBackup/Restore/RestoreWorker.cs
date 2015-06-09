using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using BaiduPCS_NET;

namespace FileBackup
{
    public class RestoreWorker : Worker
    {
        /// <summary>
        /// 目前只有 pcs.getRawData() 函数使用，用于接收数据编码
        /// </summary>
        private string encode;

        /// <summary>
        /// 在控制台的 (X, Y) 处显示进度
        /// </summary>
        private int X = -1;

        /// <summary>
        /// 在控制台的 (X, Y) 处显示进度
        /// </summary>
        private int Y = -1;

        /// <summary>
        /// 上传单元
        /// </summary>
        public Downloader downloader { get; private set; }

        /// <summary>
        /// 存储网盘中各文件的元数据
        /// </summary>
        public IList<RemoteFileInfo> fileList { get; private set; }

        public override string WorkerName
        {
            get
            {
                return "Restore";
            }
        }

        public RestoreWorker(BaiduPCS pcs, BackupItem backupItem, string userDir)
            : base(pcs, backupItem, userDir)
        {

        }

        protected override void _PreRun()
        {
            base._PreRun();
            fileList = ReadRemoteFileListAsList();
            downloader = new SecureDownload(pcs, slice_dir, new MyBlockEncoder());
            downloader.ProgressChange += onProgressChange;
            downloader.DownloadSliceError += onDownloadSliceError;
            downloader.ProgressEnabled = true;
        }

        protected override void _Run()
        {
            WriteLogAndConsole("Restore " + backupItem.RemotePath + " => " + backupItem.LocalPath);
            WriteLogAndConsole("UID: " + pcs.getUID());

            Console.WriteLine();
            X = Console.CursorLeft;
            Y = Console.CursorTop;
            RestoreDirectory(backupItem.RemotePath, backupItem.LocalPath);

            string s = string.Format("Total: {0}, Skip: {1}, Fail: {2}, Succ: {3}, Rename Total: {4}, Rename Fail: {5}, Rename Succ: {6}\r\n\r\n",
                total, skip, fail, total - skip - fail,
                rename_total, rename_fail, rename_total - rename_fail);
            WriteLogAndConsole(s);
        }

        protected override void _RunCompleted()
        {


            Downloader downloader = new Downloader(pcs, slice_dir);
            downloader.DownloadSliceError += onDownloadSliceError;
            downloader.ProgressEnabled = false;

            string dir = Path.Combine(backupItem.LocalPath, ".meta");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string listfilename = pcs.cat(remote_ref_file);
            if (!string.IsNullOrEmpty(listfilename))
            {
                File.WriteAllText(Path.Combine(dir, "ref.txt"), listfilename);
                string listcontent = pcs.cat(listfilename);
                string filename = Path.GetFileName(listfilename);
                filename = Path.Combine(dir, "list", filename);
                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                File.WriteAllText(filename, listcontent);
            }

            base._RunCompleted();

        }

        /// <summary>
        /// 还原目录
        /// </summary>
        /// <param name="remotePath">网盘目录</param>
        /// <param name="localPath">本地目录</param>
        protected void RestoreDirectory(string remotePath, string localPath)
        {
            foreach (RemoteFileInfo f in fileList)
            {
                string filename = f.Path.Substring(remotePath.Length);
                if (filename.StartsWith("/"))
                    filename = filename.Substring(1);
                string localFilename = Path.Combine(localPath, filename);
                RestoreFile(f, localFilename);
            }
        }

        protected void RestoreFile(RemoteFileInfo remoteFileInfo, string localPath)
        {
            total++;

            FileInfo fi = new FileInfo(localPath);
            //本地文件修改时间
            long lastModifyTime = Utils.UnixTimeStamp(fi.LastWriteTimeUtc);

            //获取网盘中文件的最后修改时间
            long remoteLastModifyTime = remoteFileInfo.LastModifyTime;

            //假如本地修改时间 小于等于 网盘修改时间，则无需备份
            if (remoteLastModifyTime > 0 && lastModifyTime >= remoteLastModifyTime)
            {
                skip++;
                WriteLogAndConsole("Skip " + remoteFileInfo.Path.Substring(backupItem.RemotePath.Length), Y, X);
                return;
            }

            string dir = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(localPath))
                File.Delete(localPath);

            WriteConsole("Restore " + remoteFileInfo.Path.Substring(backupItem.RemotePath.Length) + "            ", Y, X);
            if (downloader.DownloadFile(remoteFileInfo.Path, localPath))
            {
                File.SetCreationTime(localPath, Utils.FromUnixTimeStamp(remoteLastModifyTime).ToLocalTime());
                File.SetLastWriteTime(localPath, Utils.FromUnixTimeStamp(remoteLastModifyTime).ToLocalTime());
                File.SetLastAccessTime(localPath, Utils.FromUnixTimeStamp(remoteLastModifyTime).ToLocalTime());
                WriteLogAndConsole("Restore " + remoteFileInfo.Path.Substring(backupItem.RemotePath.Length) + " Success" + "    ", Y, X);
            }
            else
            {
                fail++;
                WriteLogAndConsole("Failed to Restore " + remoteFileInfo.Path.Substring(backupItem.RemotePath.Length) + ": " + pcs.getError() + "    ", Y, X);
                WriteLogAndConsole(pcs.getRawData(out encode));
            }

        }

        /// <summary>
        /// 显示上传进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onProgressChange(object sender, ProgressChangeArgs e)
        {
            if (e.size <= 0)
                return;
            int percentage = (int)((e.finished * 10000) / e.size);
            string percentage_msg = Utils.HumanReadableSize(e.finished)
                + "/" + Utils.HumanReadableSize(e.size)
                + "  " + ((float)percentage / 100.0f).ToString("F2") + "%";
            WriteConsole(percentage_msg + "    ", Y + 1, X);
        }

        private void onDownloadSliceError(object sender, SliceErrorArgs e)
        {
            WriteLogAndConsole(e.errmsg + "\r\n" + e.raw + "\r\n" + (e.exception != null ? e.exception.Message + "\r\n" + e.exception.StackTrace : "") + "    ", Y + 1, X);
            e.cancelled = false; //重试上传分片
        }

        protected virtual void WriteLogAndConsole(string line, int y = -1, int x = -1)
        {
            WriteConsole(line, y, x);
            WriteLog(line);
        }
    }
}
