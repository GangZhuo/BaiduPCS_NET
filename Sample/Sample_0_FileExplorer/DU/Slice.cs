using System;

namespace FileExplorer
{
    public class Slice
    {
        /// <summary>
        /// 分片索引号
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 开始位置
        /// </summary>
        public long start { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public long totalSize { get; set; }

        /// <summary>
        /// 已经完成的长度
        /// </summary>
        public long doneSize { get; set; }

        /// <summary>
        /// 上传成功后的分片的MD5值
        /// </summary>
        public string md5 { get; set; }

        /// <summary>
        /// 分片的状态
        /// </summary>
        public SliceStatus status { get; set; }

        /// <summary>
        /// 用户数据
        /// </summary>
        public object userdata { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public long tid { get; set; }

        public Slice()
        {
            this.md5 = string.Empty;
        }
    }
}
