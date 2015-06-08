using System;
using System.IO;

using BaiduPCS_NET;
using BaiduPCS_NET.Native;

namespace FileBackup
{
    public class SecureUploader : BaiduPCS_NET.Uploader
    {
        /// <summary>
        /// 加密器
        /// </summary>
        public IBlockEncoder Encoder { get; set; }

        public SecureUploader()
            : base()
        { }

        public SecureUploader(BaiduPCS pcs, string slice_dir, IBlockEncoder encoder)
            : base(pcs, slice_dir)
        {
            Encoder = encoder;
        }


        /// <summary>
        /// 执行快速上传
        /// </summary>
        /// <param name="localPath">文件的本地绝对路径</param>
        /// <param name="remotePath">文件的网盘绝对路径</param>
        /// <param name="overwrite">如果网盘文件已经存在，是否覆盖原文件。true - 覆盖；false - 自动重命名</param>
        /// <param name="remoteFileInfo">上传成功后，此变量被设置为网盘中文件的元数据</param>
        /// <param name="filemd5">出入的文件的MD5值，如果没有传入，且内部需要，则会自动计算</param>
        /// <returns>返回是否上传成功</returns>
        protected override bool RapidUpload(string localPath, string remotePath, bool overwrite, out PcsFileInfo remoteFileInfo, out string fileMd5)
        {
            string slicemd5;
            byte[] slice = new byte[NativeConst.PCS_RAPIDUPLOAD_THRESHOLD];
            int len;
            fileMd5 = null;
            using (FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024))
            {
                len = fs.Read(slice, 0, slice.Length);
            }
            //文件读取错误
            if (len != NativeConst.PCS_RAPIDUPLOAD_THRESHOLD)
            {
                remoteFileInfo = new PcsFileInfo();
                return false;
            }
            slice = Encoder.Encode(slice, 0, slice.Length);
            slicemd5 = NativeUtils.pcs_md5(slice);

            // 计算 MD5 
            FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
            if (!pcs.md5(new OnReadBlockFunction(OnReadBlock), stream, out fileMd5))
            {
                stream.Close();
                remoteFileInfo = new PcsFileInfo();
                return false;
            }
            stream.Close();

            remoteFileInfo = pcs.rapid_upload(remotePath, localPath, ref fileMd5, ref slicemd5, overwrite); //快速上传
            if (!remoteFileInfo.IsEmpty) //上传成功，则直接返回
                return true;
            return false;
        }

        /// <summary>
        /// 直接上传
        /// </summary>
        /// <param name="localPath">文件的本地绝对路径</param>
        /// <param name="remotePath">文件的网盘绝对路径</param>
        /// <param name="overwrite">如果网盘文件已经存在，是否覆盖原文件。true - 覆盖；false - 自动重命名</param>
        /// <param name="remoteFileInfo">上传成功后，此变量被设置为网盘中文件的元数据</param>
        /// <param name="filesize">传入的文件大小，如果没有传入，且内部需要，则内部会自动读取</param>
        /// <param name="filemd5">出入的文件的MD5值，如果没有传入，且内部需要，则会自动计算</param>
        /// <returns>返回是否上传成功</returns>
        protected override bool Upload(string localPath, string remotePath, bool overwrite, out PcsFileInfo remoteFileInfo, long filesize = -1, string filemd5 = null)
        {
            if (filesize < 0)
                filesize = GetFileSize(localPath);
            FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
            remoteFileInfo = pcs.upload_s(remotePath, new OnReadBlockFunction(OnReadBlock), (uint)filesize, stream, overwrite);
            stream.Close();
            if (!remoteFileInfo.IsEmpty) //上传成功，则直接返回
                return true;
            return false;
        }

        /// <summary>
        /// 获取文件的大小
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>返回文件的大小</returns>
        protected override long GetFileSize(string filename)
        {
            long rawSize = base.GetFileSize(filename);
            return Encoder.CalculateSize(rawSize);
        }

        /// <summary>
        /// 当需要读取数据块来计算MD5时触发
        /// </summary>
        /// <param name="sender">触发此事件的对象</param>
        /// <param name="buf">读入成功后，此变量中存储读入的块数据</param>
        /// <param name="size">待读入的元素的字节大小</param>
        /// <param name="nmemb">一共读入多少个元素</param>
        /// <param name="userdata"></param>
        /// <returns>返回实际读入的数据的字节大小</returns>
        private int OnReadBlock(BaiduPCS sender, out byte[] buf, uint size, uint nmemb, object userdata)
        {
            buf = null;
            FileStream stream = (FileStream)userdata;
            buf = new byte[size * nmemb];
            int len = stream.Read(buf, 0, buf.Length);
            buf = Encoder.Encode(buf, 0, len);
            return len;
        }

        /// <summary>
        /// 当需要读取分片数据时触发
        /// </summary>
        /// <param name="sender">触发此事件的对象</param>
        /// <param name="buf">读入成功后，此变量中存储读入的块数据</param>
        /// <param name="size">待读入的元素的字节大小</param>
        /// <param name="nmemb">一共读入多少个元素</param>
        /// <param name="userdata"></param>
        /// <returns>返回实际读入的数据的字节大小</returns>
        protected override int OnReadSlice(BaiduPCS sender, out byte[] buf, uint size, uint nmemb, object userdata)
        {
            int len = base.OnReadSlice(sender, out buf, size, nmemb, userdata);
            if (len > 0 && len != NativeConst.CURL_READFUNC_ABORT && len != NativeConst.CURL_READFUNC_PAUSE)
                buf = Encoder.Encode(buf, 0, len);
            return len;
        }

    }
}
