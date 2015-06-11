using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace FileExplorer
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

        public static DateTime FromUnixTimeStamp(long timestamp)
        {
            DateTime dt = UST.AddSeconds(timestamp);
            return dt;
        }

        public static string md5(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash. 
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes 
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data  
                // and format each one as a hexadecimal string. 
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string. 
                return sBuilder.ToString();
            }
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

        /// <summary>
        /// 获取 CPU 属性参数
        /// </summary>
        /// <returns>返回 CPU 属性参数</returns>
        public static string GetCPUProperties()
        {
            // Get the WMI class
            ManagementClass c = new ManagementClass(new ManagementPath("Win32_Processor"));
            // Get the properties in the class
            ManagementObjectCollection moc = c.GetInstances();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            // display the properties
            sb.AppendLine("Property Names: ");
            sb.AppendLine("=================");
            foreach (ManagementObject mo in moc)
            {
                PropertyDataCollection properties = mo.Properties;
                //获取内核数代码
                sb.AppendLine("物理内核数:" + properties["NumberOfCores"].Value);
                sb.AppendLine("逻辑内核数:" + properties["NumberOfLogicalProcessors"].Value);
                //其他属性获取代码
                foreach (PropertyData property in properties)
                {
                    sb.AppendLine(property.Name + ":" + property.Value);
                }
            }
            string s = sb.ToString();
            return s;
        }

        /// <summary>
        /// 获取 CPU 逻辑内核数
        /// </summary>
        /// <returns>返回 CPU 逻辑内核数</returns>
        public static int GetLogicProcessorCount()
        {
            int coreCount;
            return GetLogicProcessorCount(out coreCount);
        }

        /// <summary>
        /// 获取 CPU 逻辑内核数
        /// </summary>
        /// <param name="coreCount">函数执行成功后，此变量被设置为物理内核数</param>
        /// <returns>返回 CPU 逻辑内核数</returns>
        public static int GetLogicProcessorCount(out int coreCount)
        {
            int logicCount = 0;
            coreCount = 0;
            // Get the WMI class
            ManagementClass c = new ManagementClass(new ManagementPath("Win32_Processor"));
            // Get the properties in the class
            ManagementObjectCollection moc = c.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                PropertyDataCollection properties = mo.Properties;
                //获取内核数
                logicCount = Convert.ToInt32(properties["NumberOfLogicalProcessors"].Value);
                coreCount = Convert.ToInt32(properties["NumberOfCores"].Value);
                break;
            }
            return logicCount;
        }
    }
}
