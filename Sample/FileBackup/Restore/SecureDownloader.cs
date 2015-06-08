using System;
using System.IO;

using BaiduPCS_NET;

namespace FileBackup
{
    public class SecureDownload : BaiduPCS_NET.Downloader
    {
        /// <summary>
        /// 加密器
        /// </summary>
        public IBlockEncoder Encoder { get; set; }

        public SecureDownload()
            : base()
        { }

        public SecureDownload(BaiduPCS pcs, string slice_dir, IBlockEncoder encoder)
            : base(pcs, slice_dir)
        {
            Encoder = encoder;
        }

    }
}
