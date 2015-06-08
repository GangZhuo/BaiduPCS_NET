using System;

namespace BaiduPCS_NET
{
    public class SliceErrorArgs : EventArgs
    {
        /// <summary>
        /// 设置 true，将取消操作。
        /// </summary>
        public bool cancelled { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string errmsg { get; private set; }

        /// <summary>
        /// 原始数据
        /// </summary>
        public string raw { get; private set; }

        /// <summary>
        /// 出错的分片
        /// </summary>
        public Slice slice { get; private set; }

        /// <summary>
        /// 发生异常
        /// </summary>
        public Exception exception { get; private set; }

        public SliceErrorArgs(string errmsg, string raw, Slice slice, Exception ex)
        {
            this.errmsg = errmsg;
            this.raw = raw;
            this.slice = slice;
            this.cancelled = true;
            this.exception = ex;
        }
    }
}
