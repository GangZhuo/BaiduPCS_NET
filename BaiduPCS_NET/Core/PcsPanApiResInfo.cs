using System;
using System.Runtime.InteropServices;

using BaiduPCS_NET.Native;

namespace BaiduPCS_NET
{
    /*网盘API返回数据格式*/
    public struct PcsPanApiResInfo
    {
        public string path;
        public int error;

        public PcsPanApiResInfo(NativePcsPanApiResInfo fi)
        {
            this.path = NativeUtils.utf8_str(fi.path);
            this.error = fi.error;
        }
    }
}
