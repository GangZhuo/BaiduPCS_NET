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

        protected override uint onWrite(BaiduPCS sender, byte[] data, uint contentlength, object userdata)
        {
            data = Encoder.Encode(data, 0, data.Length);
            return base.onWrite(sender, data, contentlength, userdata);
        }

        protected override uint onDirectWrite(BaiduPCS sender, byte[] data, uint contentlength, object userdata)
        {
            data = Encoder.Encode(data, 0, data.Length);
            return base.onDirectWrite(sender, data, contentlength, userdata);
        }
    }
}
