using System;

namespace BaiduPCS_NET
{
    public class Slice
    {
        /// <summary>
        /// 分片索引号
        /// </summary>
        public int index { get; set; }

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
        public long finished { get; set; }

        /// <summary>
        /// 上传成功后的分片的MD5值
        /// </summary>
        public string md5 { get; set; }

        /// <summary>
        /// 拥有该切片的角色
        /// </summary>
        public SliceOwner owner { get; set; }

        public Slice()
        {
            this.md5 = string.Empty;
        }
    }
}
