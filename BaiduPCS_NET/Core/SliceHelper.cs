using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BaiduPCS_NET
{
    public class SliceHelper
    {

        /// <summary>
        /// 创建分片
        /// </summary>
        /// <param name="owner">拥有这些分片的 SliceOwner 对象</param>
        /// <param name="filesize">待分片文件的大小</param>
        /// <param name="minSliceSize">允许的最小分片的字节大小</param>
        /// <param name="maxSliceCount">允许最多的分片数量</param>
        /// <returns>返回分片列表</returns>
        public virtual List<Slice> CreateSliceList(long filesize, SliceOwner owner = null, int minSliceSize = (512 * 1024), int maxSliceCount = -1)
        {
            List<Slice> slicelist = new List<Slice>();

            #region 开始分片

            long slice_count = 0; //分片数量

            // 先按照最小分片计算分片数量
            long slice_size = minSliceSize;
            slice_count = (int)(filesize / slice_size);
            if ((filesize % slice_size) != 0)
                slice_count++;

            //分片数量超过最大允许分片数量，因此使用允许的最大分片数量来重新计算每分片的大小
            if (maxSliceCount != -1 && slice_count > maxSliceCount)
            {
                slice_count = maxSliceCount;
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
                    status = SliceStatus.Pending,
                    owner = owner
                };
                if (ts.offset + ts.size > filesize) ts.size = filesize - ts.offset;
                offset += slice_size;
                slicelist.Add(ts);
            }

            #endregion

            return slicelist;
        }

        /// <summary>
        /// 从分片文件中还原分片信息
        /// </summary>
        /// <param name="slice_filename">上次上传时，存储的分片文件</param>
        /// <param name="owner">拥有这些分片的 SliceOwner 对象</param>
        /// <returns>返回分片列表</returns>
        public virtual List<Slice> RestoreSliceList(string slice_filename, SliceOwner owner)
        {
            List<Slice> list = new List<Slice>();
            Slice slice;
            using (FileStream fs = new FileStream(slice_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        slice = ReadSlice(br);
                        list.Add(slice);
                        slice.owner = owner;
                        if (slice.status == SliceStatus.Successed)
                            owner.finished += slice.finished;
                        else
                            slice.status = SliceStatus.Pending; // 重置状态为 Pending
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 保存分片数据到文件
        /// </summary>
        /// <param name="filename">保存分片数据到此文件中</param>
        /// <param name="list">待保存的分片列表</param>
        public virtual void SaveSliceList(string filename, List<Slice> list)
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

        /// <summary>
        /// 删除分片文件
        /// </summary>
        /// <param name="filename">待删除的分片文件</param>
        public virtual void DeleteSliceFile(string filename)
        {
            File.Delete(filename); // 删除分片文件
        }

        /// <summary>
        /// 读入一个分片
        /// </summary>
        /// <param name="br">用于读入数据的流</param>
        /// <returns></returns>
        public virtual Slice ReadSlice(BinaryReader br)
        {
            byte[] bs;
            Slice slice = new Slice();
            slice.index = br.ReadInt32();
            slice.offset = br.ReadInt64();
            slice.size = br.ReadInt64();
            slice.finished = br.ReadInt64();
            slice.status = (SliceStatus)br.ReadInt32();
            bs = br.ReadBytes(32);
            slice.md5 = Encoding.UTF8.GetString(bs).Trim('\0').Trim();
            return slice;
        }

        /// <summary>
        /// 写入一个分片
        /// </summary>
        /// <param name="br">用于写入数据的流</param>
        /// <param name="slice">待写入的分片</param>
        public virtual void WriteSlice(BinaryWriter br, Slice slice)
        {
            byte[] bs = new byte[32];
            br.Write(slice.index);
            br.Write(slice.offset);
            br.Write(slice.size);
            br.Write(slice.finished);
            br.Write((int)slice.status);
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

    }
}
