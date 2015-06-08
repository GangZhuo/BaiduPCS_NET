using System;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET.Native
{
    /* 定义了字符串列表。列表以链表形式存储。 */
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct NativePcsSList
    {
        public IntPtr str;
        public IntPtr next;
    }
}
