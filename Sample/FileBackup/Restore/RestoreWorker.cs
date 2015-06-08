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
        public Uploader uploader { get; private set; }

        /// <summary>
        /// 存储网盘中各文件的元数据
        /// </summary>
        public IList<RemoteFileInfo> fileList { get; private set; }

        public RestoreWorker(BaiduPCS pcs, BackupItem backupItem, string userDir)
            : base(pcs, backupItem, userDir)
        {

        }

        protected override void _PreRun()
        {
            base._PreRun();
            fileList = ReadRemoteFileListAsList();
            uploader = new SecureUploader(pcs, slice_dir, new MyBlockEncoder());
            uploader.ProgressChange += uploader_ProgressChange;
            uploader.UploadSliceError += uploader_UploadSliceError;
            uploader.ProgressEnabled = true;
        }

        protected override void _Run()
        {
            WriteLogAndConsole("Restore " + backupItem.RemotePath + " => " + backupItem.LocalPath);

            RestoreDirectory(backupItem.RemotePath, backupItem.LocalPath);

            string s = string.Format("Total: {0}, Skip: {1}, Fail: {2}, Succ: {3}, Rename Total: {4}, Rename Fail: {5}, Rename Succ: {6}\r\n\r\n",
                total, skip, fail, total - skip - fail,
                rename_total, rename_fail, rename_total - rename_fail);
            WriteLogAndConsole(s);
        }

        protected override void _RunCompleted()
        {
            base._RunCompleted();

            Uploader uploader = new Uploader(pcs, slice_dir);
            uploader.UploadSliceError += uploader_UploadSliceError;
            uploader.ProgressEnabled = false;

            #region 把文件列表记录 上传到网盘

            uploader.ProgressEnabled = false;
            int x = Console.CursorLeft,
                y = Console.CursorTop;
            string filename = Path.GetFileName(list_file);
            string remote_list_file = backupItem.RemotePath + "/" + ".meta/list/" + filename;
            WriteConsole("Upload " + list_file);
            PcsFileInfo pfi = uploader.UploadFile(list_file, remote_list_file, true);
            if (!pfi.IsEmpty)
            {
                WriteLogAndConsole("Upload " + list_file + " Success", y, x);
            }
            else
            {
                WriteLogAndConsole("Failed to upload " + list_file + ": " + pcs.getError(), y, x);
                WriteLogAndConsole(pcs.getRawData(out encode));
            }

            // 设置上传的文件列表文件为最新的文件列表
            byte[] refBytes = Encoding.UTF8.GetBytes(remote_list_file);
            x = Console.CursorLeft;
            y = Console.CursorTop;
            WriteConsole("Set " + remote_ref_file);
            pfi = pcs.upload(remote_ref_file, refBytes, true);
            if (!pfi.IsEmpty)
            {
                WriteLogAndConsole("Set " + remote_ref_file + " Success", y, x);
            }
            else
            {
                WriteLogAndConsole("Failed to set " + remote_ref_file + ": " + pcs.getError(), y, x);
                WriteLogAndConsole(pcs.getRawData(out encode));
            }

            #endregion

            #region 上传日志

            filename = Path.GetFileName(log_file);
            string remote_log_file = backupItem.RemotePath + "/" + ".meta/log/" + filename;
            x = Console.CursorLeft;
            y = Console.CursorTop;
            WriteConsole("Upload log " + log_file);
            pfi = uploader.UploadFile(log_file, remote_log_file, true);
            if (!pfi.IsEmpty)
            {
                WriteLogAndConsole("Upload log " + log_file + " Success", y, x);
            }
            else
            {
                WriteLogAndConsole("Failed to upload log " + log_file + ": " + pcs.getError(), y, x);
                WriteLogAndConsole(pcs.getRawData(out encode));
            }

            #endregion

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
                string localFilename = Path.Combine(localPath, filename);
                RestoreFile(f.Path, localFilename);
            }
        }

        protected void RestoreFile(string remotePath, string localPath)
        {
            total++;

            string dir = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(localPath))
                File.Delete(localPath);

            PcsFileInfo pfi;
            FileInfo fi = new FileInfo(localPath);

            //本地文件修改时间
            long lastModifyTime = Utils.UnixTimeStamp(fi.LastWriteTimeUtc);

            //记录到列表中
            WriteList(lastModifyTime, remotePath);

            //获取网盘中文件的最后修改时间
            long remoteLastModifyTime = -1;
            if (fileList.ContainsKey(remotePath))
                remoteLastModifyTime = fileList[remotePath];

            //假如本地修改时间 小于等于 网盘修改时间，则无需备份
            if (remoteLastModifyTime > 0 && lastModifyTime <= remoteLastModifyTime)
            {
                skip++;
                WriteLogAndConsole("Skip " + localPath);
                return;
            }

            int x = Console.CursorLeft,
                y = Console.CursorTop;
            WriteConsole("Backup " + localPath);
            X = Console.CursorLeft;
            Y = Console.CursorTop;
            pfi = uploader.UploadFile(localPath, remotePath, true);
            pfi = Validate(pfi, remotePath);
            if (!pfi.IsEmpty)
            {
                WriteLogAndConsole("Backup " + localPath + " Success", y, x);
            }
            else
            {
                fail++;
                WriteLogAndConsole("Failed to backup " + localPath + ": " + pcs.getError(), y, x);
                WriteLogAndConsole(pcs.getRawData(out encode));
            }
        }

        /// <summary>
        /// BaiduPCS_NET.dll 使用的 API 在上传以点号开头的文件时（例如：'.gitignore'），
        /// 会自动去掉开始的点号（例如：'gitignore'）。
        /// 因此，我们使用重命名函数修正其文件名。
        /// </summary>
        /// <param name="fi">上传后获得的网盘文件元数据</param>
        /// <param name="remotePath">正确的网盘文件路径</param>
        /// <returns></returns>
        protected PcsFileInfo Validate(PcsFileInfo fi, string remotePath)
        {
            if (fi.IsEmpty)
                return fi;
            string filename1 = Path.GetFileName(remotePath);
            string filename2 = Path.GetFileName(fi.path);
            if (filename1 == filename2)
                return fi;
            rename_total++;
            PcsPanApiRes ppar = pcs.rename(new SPair[] {
                    new SPair(fi.path, filename1)
                });
            if (ppar.error == 0)
            {
                WriteLogAndConsole("Rename " + fi.path + " to " + filename1);
            }
            else
            {
                rename_fail++;
                WriteLogAndConsole("Failed to rename " + fi.path + " to " + filename1 + " : " + pcs.getError());
                WriteLogAndConsole(pcs.getRawData(out encode));
            }
            return fi;
        }

        /// <summary>
        /// 显示上传进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uploader_ProgressChange(object sender, ProgressChangeArgs e)
        {
            if (e.size <= 0)
                return;
            int percentage = (int)((e.finished * 10000) / e.size);
            string percentage_msg = Utils.HumanReadableSize(e.finished)
                + "/" + Utils.HumanReadableSize(e.size)
                + "  " + ((float)percentage / 100.0f).ToString("F2") + "%";
            WriteConsole(percentage_msg, Y, X);
        }

        private void uploader_UploadSliceError(object sender, SliceErrorArgs e)
        {
            WriteLogAndConsole(e.errmsg + "\r\n" + e.raw + "\r\n" + (e.exception != null ? e.exception.Message + "\r\n" + e.exception.StackTrace : ""));
            e.cancelled = false; //重试上传分片
        }

        protected virtual void WriteLogAndConsole(string line, int y = -1, int x = -1)
        {
            WriteConsole(line, y, x);
            WriteLog(line);
        }
    }
}
