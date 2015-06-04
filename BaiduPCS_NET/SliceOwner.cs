using System;

namespace BaiduPCS_NET
{
    public class SliceOwner
    {

        /// <summary>
        /// 已经完成的长度
        /// </summary>
        public long finished { get; set; }

        /// <summary>
        /// 总长度
        /// </summary>
        public long size { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string filename { get; set; }

        /// <summary>
        /// 是否由用户取消
        /// </summary>
        public bool cancelled { get; set; }

    }
}
