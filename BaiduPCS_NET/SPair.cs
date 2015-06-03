using System;

namespace BaiduPCS_NET
{
    /// <summary>
    /// 字符串对
    /// </summary>
    public struct SPair
    {
        /// <summary>
        /// 旧
        /// </summary>
        public string str1;

        /// <summary>
        /// 新
        /// </summary>
        public string str2;

        public SPair(string str1, string str2)
        {
            this.str1 = str1;
            this.str2 = str2;
        }
    }
}
