using System;

namespace BaiduPCS_NET
{
    public enum SliceStatus
    {
        /// <summary>
        /// 等待处理
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 重试中
        /// </summary>
        Retrying,

        /// <summary>
        /// 失败
        /// </summary>
        Failed,

        /// <summary>
        /// 成功
        /// </summary>
        Successed,

        /// <summary>
        /// 取消
        /// </summary>
        Cancelled,
    }
}
