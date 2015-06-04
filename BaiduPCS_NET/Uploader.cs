using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BaiduPCS_NET
{
    /// <summary>
    /// 上传器
    /// </summary>
    public class Uploader
    {
        #region 常量

        /// <summary>
        /// 最小分片大小
        /// </summary>
        public const int MIN_UPLOAD_SLICE_SIZE = (512 * 1024);

        /// <summary>
        /// 最大分片大小
        /// </summary>
        public const int MAX_UPLOAD_SLICE_SIZE = (10 * 1024 * 1024);

        /// <summary>
        /// 最大分片数量
        /// </summary>
        public const int MAX_UPLOAD_SLICE_COUNT = 1024;

        #endregion

        #region 属性和事件

        /// <summary>
        /// 获取或设置 Uploader 使用的 BaiduPCS 对象
        /// </summary>
        public BaiduPCS pcs { get; set; }

        /// <summary>
        /// 获取或设置分片数据文件的保存路径
        /// </summary>
        public string slice_dir { get; set; }

        /// <summary>
        /// 当上传进度发送改变时触发
        /// </summary>
        public event EventHandler<ProgressChangeArgs> ProgressChange;

        /// <summary>
        /// 是否允许快速上传
        /// </summary>
        public bool RapidUploadEnabled { get; set; }

        /// <summary>
        /// 是否允许分片上传。
        /// 分片上传支持断点续传。
        /// </summary>
        public bool SliceUploadEnabled { get; set; }

        #endregion

        #region 构造和析构函数

        public Uploader()
            : this(null, string.Empty)
        {
        }

        public Uploader(BaiduPCS pcs, string slice_dir)
        {
            this.pcs = pcs;
            this.slice_dir = slice_dir;

            RapidUploadEnabled = true;
            SliceUploadEnabled = true;
        }

        #endregion

        /// <summary>
        /// 上传一个文件。
        /// 文件大小 小于等于 MIN_UPLOAD_SLICE_SIZE，则直接上传；
        /// 否则，执行快速上传，快速上传失败后执行分片上传。
        /// 分片上传支持断点续传。
        /// </summary>
        /// <param name="localPath">文件的本地绝对路径</param>
        /// <param name="remotePath">文件的网盘绝对路径</param>
        /// <param name="overwrite">如果网盘文件已经存在，是否覆盖原文件。true - 覆盖；false - 自动重命名</param>
        /// <returns>返回上传成功后的网盘中文件的元数据</returns>
        public PcsFileInfo UploadFile(string localPath, string remotePath, bool overwrite = false)
        {
            PcsFileInfo fi = new PcsFileInfo();
            long filesize = new FileInfo(localPath).Length;
            string filemd5 = string.Empty,
                slicemd5;

            //允许快速上传，并且文件大小已经达到快速上传的要求
            if (RapidUploadEnabled && filesize > MIN_UPLOAD_SLICE_SIZE)
            {
                fi = pcs.rapid_upload(remotePath, localPath, out filemd5, out slicemd5, overwrite); //快速上传
                if (!fi.IsEmpty) //上传成功，则直接返回
                    return fi;
            }

            //允许分片上传，并且文件大小已经达到分片上传的要求
            if (SliceUploadEnabled && filesize > MIN_UPLOAD_SLICE_SIZE)
            {
                #region 分片上传，可断点续传

                if (string.IsNullOrEmpty(filemd5)) // 计算文件的 MD5 值
                {
                    if (!pcs.md5_file(localPath, out filemd5))
                    {
                        //未能计算文件的 MD5 值，返回空对象
                        return new PcsFileInfo();
                    }
                }

                SliceOwner owner = new SliceOwner()
                {
                    finished = 0,
                    size = filesize,
                    cancelled = false,
                    filename = localPath
                };

                List<Slice> slicelist = new List<Slice>();

                string slice_filename = MD5.Encrypt(localPath.ToLower()) + "-" + filemd5 + ".slice";
                if (!string.IsNullOrEmpty(slice_dir))
                    slice_filename = Path.Combine(slice_dir, slice_filename);

                // 分片文件存在，则从该文件中还原分片信息
                if (File.Exists(slice_filename))
                {
                    #region 还原分片数据

                    slicelist = RestoreSliceList(slice_filename);

                    foreach (Slice slice in slicelist)
                    {
                        slice.owner = owner;
                        if (!string.IsNullOrEmpty(slice.md5))
                        {
                            owner.finished += slice.finished;
                        }
                    }

                    #endregion
                }
                else
                {
                    #region 开始分片

                    long slice_count = 0; //分片数量

                    // 先按照最小分片计算分片数量
                    long slice_size = MIN_UPLOAD_SLICE_SIZE;
                    slice_count = (int)(filesize / slice_size);
                    if ((filesize % slice_size) != 0)
                        slice_count++;

                    //分片数量超过最大允许分片数量，因此使用允许的最大分片数量来重新计算每分片的大小
                    if (slice_count > MAX_UPLOAD_SLICE_COUNT)
                    {
                        slice_count = MAX_UPLOAD_SLICE_COUNT;
                        slice_size = filesize / slice_count;
                        if ((filesize % slice_count) != 0)
                            slice_size++;
                        slice_count = (int)(filesize / slice_size);
                        if ((filesize % slice_size) != 0) slice_count++;
                    }

                    long offset = 0;
                    for (int i = 0; i < slice_count; i++)
                    {
                        Slice ts = new Slice()
                        {
                            index = i,
                            offset = offset,
                            size = slice_size,
                            finished = 0,
                            owner = owner
                        };
                        if (ts.offset + ts.size > filesize) ts.size = filesize - ts.offset;
                        offset += slice_size;
                        slicelist.Add(ts);
                    }

                    #endregion

                    SaveSliceList(slice_filename, slicelist);
                }

                #region 循环上传每一个分片

                foreach (Slice slice in slicelist)
                {
                    if (!string.IsNullOrEmpty(slice.md5)) // 跳过上传成功的分片
                        continue;
                    while (true)
                    {
                        fi = pcs.upload_slicefile(new OnReadSliceFunction(OnReadSlice), slice, (uint)slice.size);

                        if (string.IsNullOrEmpty(fi.md5))
                        {
                            if (owner.cancelled) //因为用户取消上传导致的失败，则跳过。
                                break;
                            //上传失败，重新上传
                            owner.finished -= slice.finished;
                            slice.finished = 0;
                        }
                        else
                        {
                            slice.md5 = fi.md5;
                            SaveSliceList(slice_filename, slicelist);
                            break;
                        }
                    }
                    if (owner.cancelled) //用户取消上传，则停止上传。
                        break;
                }

                #endregion

                bool suc = !owner.cancelled;
                List<string> md5list = new List<string>();

                #region 检查是否所有分片都上传成功，并创建合并分片的 md5 列表

                if (suc)
                {
                    //检查是否所有分片都上传成功
                    foreach (Slice slice in slicelist)
                    {
                        if (string.IsNullOrEmpty(slice.md5))
                        {
                            suc = false;
                            break;
                        }
                        md5list.Add(slice.md5);
                    }
                }

                #endregion

                if (suc)
                {
                    fi = pcs.create_superfile(remotePath, md5list.ToArray(), overwrite); //合并分片
                    File.Delete(slice_filename); // 删除分片文件
                }
                else
                    fi = new PcsFileInfo();

                return fi;

                #endregion
            }

            #region 直接上传

            pcs.setOption(PcsOption.PCS_OPTION_PROGRESS_FUNCTION, new OnHttpProgressFunction(onProgress));
            pcs.setOption(PcsOption.PCS_OPTION_PROGRESS, true);
            fi = pcs.upload(remotePath, localPath, overwrite);
            pcs.setOption(PcsOption.PCS_OPTION_PROGRESS, false);
            return fi;

            #endregion

        }

        protected List<Slice> RestoreSliceList(string filename)
        {
            List<Slice> list = new List<Slice>();
            Slice slice;
            byte[] bs = new byte[32];
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        slice = new Slice();
                        slice.index = br.ReadInt32();
                        slice.offset = br.ReadInt64();
                        slice.size = br.ReadInt64();
                        slice.finished = br.ReadInt64();
                        bs = br.ReadBytes(32);
                        slice.md5 = Encoding.ASCII.GetString(bs).Trim('\0').Trim();
                        list.Add(slice);
                    }
                }
            }
            return list;
        }

        protected void SaveSliceList(string filename, List<Slice> list)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter br = new BinaryWriter(fs))
                {
                    foreach (Slice slice in list)
                    {
                        WriteSlice(br, slice);
                    }
                }
            }
        }

        protected void WriteSlice(BinaryWriter br, Slice slice)
        {
            byte[] bs = new byte[32];
            br.Write(slice.index);
            br.Write(slice.offset);
            br.Write(slice.size);
            br.Write(slice.finished);
            if (!string.IsNullOrEmpty(slice.md5))
            {
                for (int i = 0; i < 32; i++)
                {
                    bs[i] = (byte)slice.md5[i];
                }
            }
            else
            {
                for (int i = 0; i < 32; i++)
                {
                    bs[i] = 0;
                }
            }
            br.Write(bs);
        }

        protected int OnReadSlice(BaiduPCS sender, out byte[] buf, uint size, uint nmemb, object userdata)
        {
            Slice slice = (Slice)userdata;
            try
            {
                FileStream fs = new FileStream(slice.owner.filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
                int sz = (int)(size * nmemb);
                if (slice.finished + sz > slice.size)
                {
                    sz = (int)(slice.size - slice.finished);
                }
                buf = new byte[sz];
                //读取的位置为，本分片的开始位置 + 本分片已经上传的数量
                fs.Position = slice.offset + slice.finished;
                fs.Read(buf, 0, buf.Length);
                fs.Close();
                slice.finished += buf.Length;
                slice.owner.finished += buf.Length;

                if (ProgressChange != null && slice.owner.size > 0)
                {
                    ProgressChangeArgs args = new ProgressChangeArgs(slice.owner.finished, slice.owner.size);
                    ProgressChange(this, args);

                    if(args.cancelled)
                    {
                        slice.owner.cancelled = true;
                        return NativeConst.CURL_READFUNC_ABORT;
                    }
                }
                return buf.Length;
            }
            catch (Exception ex)
            {

            }
            buf = null;
            slice.owner.cancelled = true;
            return NativeConst.CURL_READFUNC_ABORT;
        }

        protected int onProgress(BaiduPCS sender, double dltotal, double dlnow, double ultotal, double ulnow, object userdata)
        {
            if (ProgressChange != null && ultotal >= 1)
            {
                ProgressChangeArgs args = new ProgressChangeArgs((long)ulnow, (long)ultotal);
                ProgressChange(this, args);

                if (args.cancelled)
                    return -1;
            }
            return 0;
        }

    }
}
