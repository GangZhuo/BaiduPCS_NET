using System;

namespace FileBackup
{
    public class RemoteFileInfo
    {
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public long LastModifyTime { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; set; }
    }
}
