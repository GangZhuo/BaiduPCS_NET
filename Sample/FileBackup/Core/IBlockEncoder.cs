using System;

namespace FileBackup
{
    public interface IBlockEncoder
    {
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
