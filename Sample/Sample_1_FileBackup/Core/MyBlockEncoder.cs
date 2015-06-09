using System;

namespace FileBackup
{
    public class MyBlockEncoder: IBlockEncoder
    {
        public const int T = 0x55;

        /// <summary>
        /// 编码数据块
        /// </summary>
        /// <param name="block">待编码的数据块</param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] Encode(byte[] block, int start, int length)
        {
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                bytes[i] = (byte)(block[i + start] ^ T);
            }
            return bytes;
        }

        public long CalculateSize(long rawSize)
        {
            return rawSize;
        }
    }
}
