using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileExplorer
{
    public class SliceHelper
    {
        /// <summary>
        /// 创建分片
        /// </summary>
        /// <param name="filesize">文件的大小</param>
        /// <param name="sliceSize">分片的字节大小</param>
        /// <returns>返回分片列表</returns>
        public static List<Slice> CreateSliceList(long filesize, int sliceSize)
        {
            List<Slice> slicelist = new List<Slice>();

            #region 开始分片

            long slice_count = 0; //分片数量

            // 先按照最小分片计算分片数量
            long slice_size = sliceSize;
            slice_count = (int)(filesize / slice_size);
            if ((filesize % slice_size) != 0)
                slice_count++;

            long offset = 0;
            for (int i = 0; i < slice_count; i++)
            {
                Slice ts = new Slice()
                {
                    index = i,
                    start = offset,
                    totalSize = slice_size
                };
                if (ts.start + ts.totalSize > filesize) ts.totalSize = filesize - ts.start;
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
        public static List<Slice> RestoreSliceList(string slice_filename)
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
        public static void SaveSliceList(string filename, List<Slice> list)
        {
            string dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
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
        public static void DeleteSliceFile(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename); // 删除分片文件
        }

        /// <summary>
        /// 读入一个分片
        /// </summary>
        /// <param name="br">用于读入数据的流</param>
        /// <returns></returns>
        public static Slice ReadSlice(BinaryReader br)
        {
            byte[] bs;
            Slice slice = new Slice();
            slice.index = br.ReadInt32();
            slice.start = br.ReadInt64();
            slice.totalSize = br.ReadInt64();
            slice.doneSize = br.ReadInt64();
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
        public static void WriteSlice(BinaryWriter br, Slice slice)
        {
            byte[] bs = new byte[32];
            br.Write(slice.index);
            br.Write(slice.start);
            br.Write(slice.totalSize);
            br.Write(slice.doneSize);
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
