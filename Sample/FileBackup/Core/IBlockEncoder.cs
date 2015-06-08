using System;

namespace FileBackup
{
    public interface IBlockEncoder
    {
        /// <summary>
        /// 计算编码后的文件大小
        /// </summary>
        /// <param name="rawSize">原文件大小</param>
        /// <returns>返回编码后的大小</returns>
        long CalculateSize(long rawSize);

        /// <summary>
        /// 编码数据块
        /// </summary>
        /// <param name="block">待编码的数据块</param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        byte[] Encode(byte[] block, int start, int length);
    }
}
