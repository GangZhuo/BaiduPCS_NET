using System;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET
{
    /*网盘API返回数据格式*/
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct PcsPanApiResInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        string path;
        int error;
    }
}
