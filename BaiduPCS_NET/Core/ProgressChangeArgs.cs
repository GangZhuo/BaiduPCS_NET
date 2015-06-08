using System;

namespace BaiduPCS_NET
{
    public class ProgressChangeArgs : EventArgs
    {
        /// <summary>
        /// 已经完成大小
        /// </summary>
        public long finished { get; private set; }

        /// <summary>
        /// 总大小
        /// </summary>
        public long size { get; private set; }

        /// <summary>
        /// 设置 true，将取消操作。
        /// </summary>
        public bool cancelled { get; set; }

        public ProgressChangeArgs(long uploaded, long total)
        {
            finished = uploaded;
            size = total;
            cancelled = false;
        }
    }
}
