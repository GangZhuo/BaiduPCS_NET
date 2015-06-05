using System;

namespace FileBackup
{
    public class BackupItem
    {
        /// <summary>
        /// 本地目录。
        /// </summary>
        public string LocalPath { get; private set; }

        /// <summary>
        /// 网盘目录，不要以 '/' 结尾。
        /// </summary>
        public string RemotePath { get; private set; }

        public BackupItem(string localPath, string remotePath)
        {
            LocalPath = localPath;
            RemotePath = remotePath;
        }
    }
}
