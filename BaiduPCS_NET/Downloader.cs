using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.MemoryMappedFiles;

using BaiduPCS_NET.Native;

namespace BaiduPCS_NET
{
    public class Downloader
    {
        #region 常量

        /// <summary>
        /// 最小分片大小
        /// </summary>
        public const int MIN_SLICE_SIZE = (512 * 1024);

        /// <summary>
        /// 最大分片大小
        /// </summary>
        public const int MAX_SLICE_SIZE = (10 * 1024 * 1024);

        #endregion

        #region 属性和事件

        /// <summary>
        /// 获取或设置 Uploader 使用的 BaiduPCS 对象
        /// </summary>
        public BaiduPCS pcs { get; set; }

        public virtual SliceHelper sliceHelper { get; set; }

        /// <summary>
        /// 获取或设置分片数据文件的保存路径。
        /// 上传过程从中断恢复时，将从此目录回复上传状态。
        /// </summary>
        public string slice_dir { get; set; }

        /// <summary>
        /// 当上传进度发送改变时触发
        /// </summary>
        public event EventHandler<ProgressChangeArgs> ProgressChange;

        /// <summary>
        /// 上传分片出错时触发。
        /// </summary>
        public event EventHandler<SliceErrorArgs> DownloadSliceError;

        /// <summary>
        /// 是否允许分片上传。
        /// 分片上传支持断点续传。
        /// </summary>
        public bool SliceDownloadEnabled { get; set; }

        /// <summary>
        /// 是否启用进度条
        /// </summary>
        public bool ProgressEnabled { get; set; }

        #endregion

        #region 构造和析构函数

        public Downloader()
            : this(null, string.Empty)
        {
        }

        public Downloader(BaiduPCS pcs, string slice_dir)
        {
            this.pcs = pcs;
            this.slice_dir = slice_dir;

            SliceDownloadEnabled = true;
            ProgressEnabled = true;

            sliceHelper = new SliceHelper();

        }

        #endregion

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="remotePath">待下载的远端路径</param>
        /// <param name="localPath">保存到本地文件路径</param>
        /// <returns>返回是否下载成功</returns>
        public virtual bool DownloadFile(string remotePath, string localPath)
        {
            long filesize = pcs.filesize(remotePath);

            //允许分片下载，并且文件大小已经达到分片下载的要求
            if (SliceDownloadEnabled && filesize > MIN_SLICE_SIZE
                && SliceDownload(remotePath, localPath, filesize))
                return true;

            //直接下载
            if (DownloadDirect(remotePath, localPath))
                return true;
            return false;
        }

        protected virtual bool DownloadDirect(string remotePath, string localPath, long filesize = -1)
        {
            if (filesize == -1)
                filesize = pcs.filesize(remotePath);
            SliceOwner owner = new SliceOwner()
            {
                finished = 0,
                size = filesize,
                cancelled = false,
                local_filename = localPath,
                server_filename = remotePath
            };
            pcs.Write += onDirectWrite;
            pcs.WriteUserData = owner;
            PcsRes rc = pcs.download(remotePath, 0, 0);
            pcs.Write -= onDirectWrite;
            if (rc == PcsRes.PCS_OK)
                return true;
            return false;
        }

        protected virtual bool SliceDownload(string remotePath, string localPath, long filesize = -1)
        {
            PcsFileInfo remoteFileInfo = pcs.meta(remotePath);
            if (remoteFileInfo.IsEmpty)
                return false;

            if (filesize < 0)
                filesize = pcs.filesize(remotePath);

            #region 分片下载，可断点续传

            SliceOwner owner = new SliceOwner()
            {
                finished = 0,
                size = filesize,
                cancelled = false,
                local_filename = localPath,
                server_filename = remotePath
            };

            List<Slice> slicelist;

            //分片文件的存储路径
            string slice_filename = "download-" + NativeUtils.pcs_md5_string(localPath.ToLower()) + "-" + remoteFileInfo.md5 + ".slice";
            if (!string.IsNullOrEmpty(slice_dir))
                slice_filename = Path.Combine(slice_dir, slice_filename);

            if (File.Exists(slice_filename))
            {
                // 分片文件存在，则从该文件中还原分片信息
                slicelist = sliceHelper.RestoreSliceList(slice_filename, owner);
            }
            else
            {
                // 新建分片
                int maxSliceCount = (int)(filesize / MAX_SLICE_SIZE);
                if ((filesize % MAX_SLICE_SIZE) != 0)
                    maxSliceCount++;
                slicelist = sliceHelper.CreateSliceList(filesize, owner, MIN_SLICE_SIZE, maxSliceCount);
                //保存一次分片数据
                sliceHelper.SaveSliceList(slice_filename, slicelist);
            }
            if (DownloadSliceList(slicelist, slice_filename))
            {
                sliceHelper.DeleteSliceFile(slice_filename);
                return true;
            }
            #endregion

            return false;
        }

        protected virtual bool DownloadSliceList(List<Slice> slicelist, string save_slice_to_filename)
        {
            SliceOwner owner = null;
            owner = slicelist[0].owner;

            //预先创建一个大文件
            using (FileStream fs = File.Create(owner.local_filename))
            {
                long offset = fs.Seek(owner.size - 1, SeekOrigin.Begin);
                fs.WriteByte((byte)0);
            }

            MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(owner.local_filename, FileMode.Open);

            owner.userdata = mmf;

            pcs.Write += onWrite;

            // 循环下载每一个分片
            for (int i = 0; i < slicelist.Count; i++)
            {
                Slice slice = slicelist[i];
                // 该分片已经上传成功，且得到了其 MD5 值
                if (slice.status == SliceStatus.Successed)
                    continue;
                else if (slice.status != SliceStatus.Pending) //上传失败，或正在上传，或正在重试，或用户取消上传
                    slice.status = SliceStatus.Retrying;
                //当前分片上传失败，且原因不是因为用户取消上传，则重试上传分片。
                while (!owner.cancelled && slice.status != SliceStatus.Cancelled && !DownloadSlice(slice))
                {
                    #region 触发错误处理事件，假如在错误处理事件中，用户设置 ErrorArgs.cancelled = false; 则重试。
                    if (DownloadSliceError != null)
                    {
                        SliceErrorArgs arg = new SliceErrorArgs(pcs.getError(), pcs.getRawData(), slice, null);
                        DownloadSliceError(this, arg);
                        if (arg.cancelled)
                        {
                            slice.owner.cancelled = true;
                            break;
                        }
                    }
                    #endregion
                }
                if (slice.status == SliceStatus.Cancelled)
                    owner.cancelled = true;
                if (owner.cancelled) //用户取消上传，则终止上传。
                    break;
                //下载成功，保存分片数据
                if (slice.status == SliceStatus.Successed)
                    sliceHelper.SaveSliceList(save_slice_to_filename, slicelist);
            }

            pcs.Write -= onWrite;
            mmf.Dispose();

            bool suc = !owner.cancelled;

            #region 检查是否所有分片都下载成功

            if (suc)
            {
                //检查是否所有分片都上传成功
                foreach (Slice slice in slicelist)
                {
                    if (slice.status != SliceStatus.Successed)
                    {
                        suc = false;
                        break;
                    }
                }
            }

            #endregion

            return suc;
        }

        protected virtual bool DownloadSlice(Slice slice)
        {
            SliceOwner owner = slice.owner;
            PcsRes rc;
            try
            {
                pcs.WriteUserData = slice;
                rc = pcs.download(slice.owner.server_filename, 0, slice.offset);
            }
            catch (Exception ex)
            {
                try { pcs.set_serrmsg("Error when download slice. " + ex.Message); }
                catch { }
                slice.status = SliceStatus.Failed;
                return false;
            }
            if (rc != PcsRes.PCS_OK)
            {
                //上传失败，重置其已经上传的数量和整个文件的已经上传数量
                if (slice.status != SliceStatus.Cancelled)
                    slice.status = SliceStatus.Failed;
                return false;
            }
            slice.status = SliceStatus.Successed;
            return true;
        }

        protected virtual uint onWrite(BaiduPCS sender, byte[] data, uint contentlength, object userdata)
        {
            Slice slice = (Slice)userdata;
            MemoryMappedFile mmf = (MemoryMappedFile)slice.owner.userdata;
            int len = data.Length;
            if (len > slice.size - slice.finished)
                len = (int)(slice.size - slice.finished);
            long offset = slice.offset + slice.finished;
            long size = len;
            using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(offset, size))
            {
                accessor.WriteArray<byte>(0, data, 0, len);
            }
            slice.finished += len;
            slice.owner.finished += len;
            if (ProgressEnabled && ProgressChange != null && slice.owner.size > 0)
            {
                ProgressChangeArgs args = new ProgressChangeArgs(slice.owner.finished, slice.owner.size);
                ProgressChange(this, args);

                if (args.cancelled)
                {
                    pcs.set_serrmsg("Cancelled when write slice block. ");
                    slice.status = SliceStatus.Cancelled;
                    slice.owner.cancelled = true;
                    len = 0;
                }
            }
            return (uint)len;
        }

        protected virtual uint onDirectWrite(BaiduPCS sender, byte[] data, uint contentlength, object userdata)
        {
            SliceOwner owner = (SliceOwner)userdata;
            using(FileStream fs = new FileStream(owner.local_filename, FileMode.Append))
            {
                fs.Write(data, 0, data.Length);
            }
            owner.finished += data.Length;
            if (ProgressEnabled && ProgressChange != null && owner.size > 0)
            {
                ProgressChangeArgs args = new ProgressChangeArgs(owner.finished, owner.size);
                ProgressChange(this, args);

                if (args.cancelled)
                {
                    pcs.set_serrmsg("Cancelled when write slice block. ");
                    return 0;
                }
            }
            return (uint)data.Length;
        }

    }
}
