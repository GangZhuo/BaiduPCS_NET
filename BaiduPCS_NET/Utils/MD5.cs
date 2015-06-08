using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace BaiduPCS_NET
{
    internal enum MD5Length
    {
        Len32 = 0,
        Len16 = 1,
        Len8 = 2
    }

    /// <summary>
    /// 
    /// </summary>
    internal class MD5
    {
        /// <summary>
        /// MD5 encrypt
        /// </summary>
        /// <param name="bytes">original data</param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static String Encrypt(byte[] bytes, MD5Length len = MD5Length.Len32)
        {
            bytes = new MD5CryptoServiceProvider().ComputeHash(bytes);
            string ret = "";
            for (int i = 0; i < bytes.Length; i++)
                ret += bytes[i].ToString("x").PadLeft(2, '0');
            switch (len)
            {
                case MD5Length.Len32:
                    return ret;
                case MD5Length.Len16:
                    return ret.Substring(8, 8) + ret.Substring(16, 8);
                case MD5Length.Len8:
                    return ret.Substring(16, 8);
            }
            return ret;
        }

        /// <summary>
        /// MD5 encrypt
        /// </summary>
        /// <param name="str">original string</param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static String Encrypt(String str, MD5Length len = MD5Length.Len32)
        {
            byte[] b = Encoding.Default.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
                ret += b[i].ToString("x").PadLeft(2, '0');
            switch (len)
            {
                case MD5Length.Len32:
                    return ret;
                case MD5Length.Len16:
                    return ret.Substring(8, 8) + ret.Substring(16, 8);
                case MD5Length.Len8:
                    return ret.Substring(16, 8);
            }
            return ret;
        }

        /// <summary>
        /// SHA 256
        /// </summary>
        /// /// <param name="str">original string</param>
        /// <returns></returns>
        public static string SHA256(string str)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] Result = Sha256.ComputeHash(SHA256Data);
            return Convert.ToBase64String(Result);
        }
    }
}
