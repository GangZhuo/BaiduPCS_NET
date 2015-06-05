using System;

namespace FileBackup
{
    public static class Utils
    {

        public const int KB = 1024;
        public const int MB = 1024 * KB;
        public const int GB = 1024 * MB;

        /// <summary>
        /// Unix时间戳的开始时间
        /// </summary>
        public static DateTime UST = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        public static long UnixTimeStamp(DateTime dt)
        {
            TimeSpan ts = dt.ToUniversalTime() - UST;
            return (long)ts.TotalSeconds;
        }

        /// <summary>
        /// 格式化文件大小为人类可读的格式
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string HumanReadableSize(long size)
        {
            //B, KB, MB, or GB
            if (size >= GB)
            {
                double f = (double)size / (double)GB;
                string scalar = f.ToString("F2");
                if (scalar.EndsWith(".00"))
                    scalar = scalar.Substring(0, scalar.Length - 3);
                return scalar + "GB";
            }
            else if (size >= MB)
            {
                double f = (double)size / (double)MB;
                string scalar = f.ToString("F2");
                if (scalar.EndsWith(".00"))
                    scalar = scalar.Substring(0, scalar.Length - 3);
                return scalar + "MB";
            }
            else if (size >= KB)
            {
                double f = (double)size / (double)KB;
                string scalar = f.ToString("F2");
                if (scalar.EndsWith(".00"))
                    scalar = scalar.Substring(0, scalar.Length - 3);
                return scalar + "KB";
            }
            else
            {
                return size.ToString() + "B";
            }
        }

    }
}
