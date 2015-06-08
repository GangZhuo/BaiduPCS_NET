using System;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET.Native
{
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct NativePcsSList2
    {
        public IntPtr str1;
        public IntPtr str2;
        public IntPtr next;
    }
}
