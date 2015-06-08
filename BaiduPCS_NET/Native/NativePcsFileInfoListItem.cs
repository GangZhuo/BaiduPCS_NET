using System;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET.Native
{
    /*网盘中文件元数据链表的单个节点*/
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct NativePcsFileInfoListItem
    {
        public IntPtr info;
        public IntPtr prev;
        public IntPtr next;
    }
}
