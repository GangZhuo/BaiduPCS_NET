using System;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET.Native
{
    /*以链表形式存储的网盘文件元数据列表*/
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct NativePcsFileInfoList
    {
        public int count;
        public IntPtr link;
        public IntPtr link_tail;
    }
}
